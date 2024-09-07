using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public class Interpreter {
    public static InputGetter Input_dialog;
    public string Output = "";
    public async Task<InterpreterOutput> Interprete(List<Node> nodes, ContextualStorage storage) {
        foreach (Node node in nodes) await InterpreteNode(node, storage);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<InterpreterOutput> InterpreteNode(Node node, ContextualStorage storage) => node switch {
        InlineFunctionDefinitionNode switch_node => await InterpreteInlineFunctionDefinitionNode(switch_node),
        VariableDefinitionNode switch_node => await InterpreteVariableDefinitionNode(switch_node, storage),
        ClassDefinitionNode switch_node => await InterpreteClassDefinitionNode(switch_node, storage),
        FunctionDefinitionNode switch_node => await InterpreteFunctionDefinitionNode(switch_node, storage),
        FunctionArgumentNode switch_node => await InterpreteFunctionArgumentNode(switch_node, storage),
        ConstructorNode switch_node => await InterpreteConstructorNode(switch_node, storage),
        BinaryOperatorNode switch_node => await InterpreteBinaryOperatorNode(switch_node, storage),
        UnaryOperatorNode switch_node => await InterpreteUnaryOperatorNode(switch_node, storage),
        
        DataNode switch_node => await InterpreteDataNode(switch_node, storage),

        FlowControlNode switch_node => await InterpreteFlowControlNode(switch_node, storage),
        WhileNode switch_node => await InterpreteWhileNode(switch_node, storage),
        ForNode switch_node => await InterpreteForNode(switch_node, storage),
        IFNode switch_node => await InterpreteIFNode(switch_node, storage),
        FunctionParameterNode switch_node => await InterpreteFunctionParameterNode(switch_node, storage),
        _ => throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position, "new node?")
    };
    
    private async Task<StorageVariable> InterpreteFunctionArgumentNode(FunctionArgumentNode node, ContextualStorage storage) {
        if (storage.HasVariable(node.Identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Identifier.Position);
        StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.VOID, false, false, null);
        if (node.Datatype is KeywordNode) {
            switch ((node.Datatype as KeywordNode).Keyword) {
                case KeywordToken.KEYWORD.VARIANT:
                    storage_variable.Datatype = StorageEntity.DATATYPE.VARIANT;
                    break;
                case KeywordToken.KEYWORD.BOOLEAN:
                    storage_variable.Datatype = StorageEntity.DATATYPE.BOOLEAN;
                    break;
                case KeywordToken.KEYWORD.INTEGER: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.INTEGER;
                    break;
                case KeywordToken.KEYWORD.FLOAT: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.FLOAT;
                    break;
                case KeywordToken.KEYWORD.STRING: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.STRING;
                    break;
                case KeywordToken.KEYWORD.LIST: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.LIST;
                    break;
                case KeywordToken.KEYWORD.OBJECT:
                    storage_variable.Datatype = StorageEntity.DATATYPE.USER_DEFINED;
                    break;
                case KeywordToken.KEYWORD.DICTIONARY: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.DICTIONARY;
                    break;
                case KeywordToken.KEYWORD.CALLABLE: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.CALLABLE;
                    break;
            }
        } else {
            storage_variable.Datatype = StorageEntity.DATATYPE.USER_DEFINED;
            StorageClass storage_class = (await InterpreteNode(node.Datatype, storage)).IsStorageClass(node.Datatype.Position);
            storage_variable.Class_name = storage_class.Class_name;
        }
        storage.CreateVariable(node.Identifier.Data as string, storage_variable);
        return storage_variable;
    }
    private async Task<MarbleData> InterpreteInlineFunctionDefinitionNode(InlineFunctionDefinitionNode node) {
        StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.VARIANT, false, node.UnlimitedArguments, node.Arguments, node.Instructions);
        return await Task.Run(() => new MarbleData(MarbleData.TYPE.CALLABLE, storage_function));
    }
    private async Task<InterpreterOutput> InterpreteFunctionParameterNode(FunctionParameterNode node, ContextualStorage storage) {
        if (node is FunctionParameterNode.Classified) throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
        return await InterpreteNode(node.Parameter, storage);
    }
    private async Task<FlowController> InterpreteInstructionListNode(OperandInstructionNode node, ContextualStorage storage) {
        if (node.Oneline) {
            return new FlowController.Return((await InterpreteNode(node.Instructions[0], storage)).ToMarbleData(node.Instructions[0].Position));
        } else {
            foreach (Node statement in node.Instructions) {
                InterpreterOutput result = await InterpreteNode(statement, storage);
                if (result is FlowController && (result as FlowController).Type != FlowController.TYPE.DONE) return result as FlowController;
            }
            return new FlowController(FlowController.TYPE.DONE);
        }
    }
    private async Task<InterpreterOutput> RunFunction(StorageFunction function, List<Node> parameters, BinaryOperatorNode function_node, ContextualStorage storage, ContextualStorage child_storage) {
        if (function.UnlimitedArguments) {
            if (storage.HasVariable((function.Arguments[0] as DataNode).Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, function.Arguments[0].Position);
            List<MarbleData> unlimited = new List<MarbleData>();
            for (int x = 0; x < parameters.Count; x++) {
                unlimited.Add((await InterpreteNode(parameters[x], storage)).ToMarbleData(parameters[x].Position));
            }
            child_storage.CreateVariable((function.Arguments[0] as DataNode).Data as string, new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.LIST, false, false, new MarbleData(MarbleData.TYPE.LIST, unlimited)));
        } else {
            if (parameters.Count != function.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function_node.Right.Position, "Different number of parameters");
            for (int x = 0; x < function.Arguments.Count; x++) {
                await AssignOperation(function.Arguments[x], (await InterpreteNode(parameters[x], storage)).ToMarbleData(parameters[x].Position), child_storage);
            }
        }
        child_storage.CreateVariable("RETURN!", new StorageVariable(StorageEntity.ACCESSMODE.PRIVATE, function.Datatype, false,  false, null));
        FlowController output = await InterpreteInstructionListNode(function.Instructions, child_storage);
        switch (output.Type) {
            case FlowController.TYPE.RETURN:
                return (output as FlowController.Return).Data;
            case FlowController.TYPE.DONE:
                if (function.Datatype != StorageEntity.DATATYPE.VOID) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function_node.Left.Position, "No return statement");
                return output;
            default:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function_node.Left.Position, "Check function output");
        }
    }
    private async Task<InterpreterOutput> DotOperation(BinaryOperatorNode node, ContextualStorage storage) {
        InterpreterOutput Left = await InterpreteNode(node.Left, storage);
        switch (Left) {
            case StorageFunction function:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
            case StorageClass storage_class:
                switch (node.Right) {
                    case BinaryOperatorNode function: {
                        DataNode identifier = function.Left as DataNode;
                        List<Node> parameters = (function.Right as DataNode).Data as List<Node>;
                        if (storage_class.Storage.HasFunction(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_FUNCTION, identifier.Position);
                        StorageFunction storage_function = storage_class.Storage.GetFunction(identifier.Data as string);
                        if (!storage_function.IsStatic) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, identifier.Position, "non static");
                        if (storage_function.AccessMode != StorageEntity.ACCESSMODE.PUBLIC) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_FUNCTION, function.Position);
                        return await RunFunction(storage_function, parameters, function, storage, storage_class.Storage.CreateChild());
                    }
                    case DataNode identifier: {
                        if (!storage_class.Storage.GetEntity(identifier.Data as string, out StorageEntity storage_entity)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, identifier.Position);
                        if (storage_entity.AccessMode != StorageEntity.ACCESSMODE.PUBLIC) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_VARIABLE, identifier.Position);
                        if (!storage_entity.IsStatic) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, identifier.Position, "non static");
                        return storage_entity;
                    }
                    default:
                        return null;
                }
                
            case StorageVariable: case MarbleData:
                MarbleData data = Left.ToMarbleData(node.Position);
                switch (node.Right) {
                    case BinaryOperatorNode function: {
                        DataNode identifier = function.Left as DataNode;
                        List<Node> parameters = (function.Right as DataNode).Data as List<Node>;
                        switch (data.type) {
                            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: case MarbleData.TYPE.DICTIONARY:
                                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                            case MarbleData.TYPE.STRING:
                                switch (identifier.Data as string) {
                                    case "has":
                                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        return InOperation((await InterpreteNode(parameters[0], storage)).ToMarbleData(parameters[0].Position), data, parameters[0].Position);
                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
                                }
                            case MarbleData.TYPE.LIST: {
                                switch (identifier.Data as string) {
                                    case "append":
                                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        MarbleData new_element = (await InterpreteNode(parameters[0], storage)).ToMarbleData(parameters[0].Position);
                                        (data.value as List<MarbleData>).Add(new_element);
                                        break;
                                    case "has":
                                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        return InOperation((await InterpreteNode(parameters[0], storage)).ToMarbleData(parameters[0].Position), data, parameters[0].Position);
                                    case "clear":
                                        if (parameters.Count > 0) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        (data.value as List<MarbleData>).Clear();
                                        return new FlowController(FlowController.TYPE.DONE);

                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
                                }
                                return null;
                            }
                            case MarbleData.TYPE.OBJECT: {
                                if (!data.as_object().Storage.HasFunction(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_FUNCTION, identifier.Position);
                                StorageFunction storage_function = data.as_object().Storage.GetFunction(identifier.Data as string);
                                if (storage_function.AccessMode != StorageEntity.ACCESSMODE.PUBLIC) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_FUNCTION, function.Position);
                                return await RunFunction(storage_function, parameters, function, storage, data.as_object().Storage.CreateChild());
                            }
                            default:
                                return null;
                        }
                    }
                    case DataNode identifier: {
                        switch (data.type) {
                            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: case MarbleData.TYPE.DICTIONARY:
                                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                            case MarbleData.TYPE.STRING:
                                switch(identifier.Data) {
                                    case "size":
                                        return new MarbleData(MarbleData.TYPE.INTEGER, data.as_string().Length);
                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, identifier.Position, "undefined");
                                }
                            case MarbleData.TYPE.LIST:
                                switch(identifier.Data) {
                                    case "size":
                                        return new MarbleData(MarbleData.TYPE.INTEGER, data.as_list().Count);
                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, identifier.Position, "undefined");
                                }
                            case MarbleData.TYPE.OBJECT: {
                                if (!data.as_object().Storage.GetEntity(identifier.Data as string, out StorageEntity storage_entity)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, identifier.Position);
                                if (storage_entity.AccessMode != StorageEntity.ACCESSMODE.PUBLIC) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_VARIABLE, identifier.Position);
                                return storage_entity;
                            }
                            default:
                                return null;
                        }
                    }
                    default:
                        return null;
                }
            default:
                return null;
        }
    }
    private async Task<MarbleData> MarbleDataToString(MarbleData data, ContextualStorage storage, TokenPosition position) {
        switch (data.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                return new MarbleData(MarbleData.TYPE.STRING, $"{data.value}");
            case MarbleData.TYPE.STRING:
                return data;
            case MarbleData.TYPE.LIST:
                string output = "[";
                foreach (MarbleData element in data.as_list()) {
                    output += (await MarbleDataToString(element, storage, position)).value + ", ";
                }
                output = output.Remove(output.Length - 2);
                output += "]";
                return new MarbleData(MarbleData.TYPE.STRING, output);
            case MarbleData.TYPE.DICTIONARY:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, position);
            case MarbleData.TYPE.CALLABLE:
                return new MarbleData(MarbleData.TYPE.STRING, "<Callable>");
            case MarbleData.TYPE.OBJECT:
                if (data.as_object().Storage.HasFunction("to_string")) {
                    StorageFunction function = data.as_object().Storage.GetFunction("to_string");
                    return (await RunFunction(function, function.Arguments, null, storage, data.as_object().Storage.CreateChild())).ToMarbleData(position);
                } else {
                    return new MarbleData(MarbleData.TYPE.STRING, "<Object>");
                }
            default:
                return null;
        }
    }
    private async Task<InterpreterOutput> CallerOperation(BinaryOperatorNode node, ContextualStorage storage) {
        List<Node> parameters = (node.Right as DataNode).Data as List<Node>;
        switch (node.Left) {
            case KeywordNode keyword_node: {
                switch (keyword_node.Keyword) {
                    case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.BOOLEAN: case KeywordToken.KEYWORD.INTEGER:
                    case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST: case KeywordToken.KEYWORD.DICTIONARY:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                    case KeywordToken.KEYWORD.ASSERT: {
                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                        MarbleData assertion = (await InterpreteNode(parameters[0], storage)).ToMarbleData(parameters[0].Position);
                        if (assertion.type != MarbleData.TYPE.BOOLEAN) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Expected boolean");
                        if (!assertion.as_boolean()) throw new InterpreterError(InterpreterError.TYPE.ASSERTION_FAILED, node.Right.Position);
                        return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.PRINT: {
                        foreach (Node parameter in parameters) {
                            Output += $"{(await MarbleDataToString((await InterpreteNode(parameter, storage)).ToMarbleData(parameter.Position), storage, parameter.Position)).value} ";
                        }
                        return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.PRINTLINE: {
                        foreach (Node parameter in parameters) {
                            Output += $"{(await MarbleDataToString((await InterpreteNode(parameter, storage)).ToMarbleData(parameter.Position), storage, parameter.Position)).value} ";
                        }
                        Output += '\n';
                        return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.RANGE: {
                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        MarbleData data = (await InterpreteNode(parameters[0], storage)).ToMarbleData(parameters[0].Position);
                        if (data.type != MarbleData.TYPE.INTEGER)  throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Position);
                        List<MarbleData> elements = new List<MarbleData>();
                        for (int x = 0; x < (int) data.value; x++) {
                            elements.Add(new MarbleData(MarbleData.TYPE.INTEGER, x));
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, elements);
                    }
                    case KeywordToken.KEYWORD.RANDOM: {
                        if (parameters.Count > 2) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        MarbleData left = (await InterpreteNode(parameters[0], storage)).ToMarbleData(parameters[0].Position);
                        if (left.type != MarbleData.TYPE.INTEGER) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, parameters[0].Position);
                        MarbleData right = (await InterpreteNode(parameters[1], storage)).ToMarbleData(parameters[1].Position);
                        if (right.type != MarbleData.TYPE.INTEGER) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, parameters[1].Position);
                        int random_number = new Random().Next(left.as_integer(), right.as_integer());
                        return new  MarbleData(MarbleData.TYPE.INTEGER, random_number);
                    }
                    case KeywordToken.KEYWORD.INPUT: {
                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        Input_dialog.DialogText = $"{(await InterpreteNode(parameters[0], storage)).ToMarbleData(parameters[0].Position)}";
                        Input_dialog.Show();
                        await Input_dialog.ToSignal(Input_dialog, "confirmed");
                        return new MarbleData(MarbleData.TYPE.STRING, Input_dialog.Input);
                    }
                    default:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                }
            }
            case DataNode data_node:
                switch (data_node.Type) {
                    case DataNode.TYPE.IDENTIFIER:
                        if (storage.HasFunction(data_node.Data as string)) {
                            return await RunFunction(storage.GetFunction(data_node.Data as string), parameters, node, storage, storage.CreateChild());
                        } else if (storage.HasVariable(data_node.Data as string)) {
                            StorageVariable variable = storage.GetVariable(data_node.Data as string);
                            if (variable.Datatype != StorageEntity.DATATYPE.CALLABLE) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_FUNCTION, data_node.Position);
                            return await RunFunction(variable.Data.as_callable(), parameters, null, storage, storage.CreateChild());
                        } else if (storage.HasClass(data_node.Data as string)) {
                            StorageClass storage_class = storage.GetClass(data_node.Data as string);
                            ContextualStorage child_storage = storage_class.Storage.CreateChild();
                            StorageFunction.Constructor constructor = storage_class.Storage.GetConstructor("1");
                            if (constructor == null) {
                                return new MarbleData(MarbleData.TYPE.OBJECT, storage_class);
                            }
                            ContextualStorage parameter_child_storage = child_storage.CreateChild();
                            if (constructor.UnlimitedArguments) {
                                if (storage.HasVariable((constructor.Arguments[0] as DataNode).Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, constructor.Arguments[0].Position);
                                List<MarbleData> unlimited = new List<MarbleData>();
                                for (int x = 0; x < parameters.Count; x++) {
                                    unlimited.Add((await InterpreteNode(parameters[x], storage)).ToMarbleData(parameters[x].Position));
                                }
                                parameter_child_storage.CreateVariable((constructor.Arguments[0] as DataNode).Data as string, new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.LIST, false, false, new MarbleData(MarbleData.TYPE.LIST, unlimited)));
                            } else {
                                if (parameters.Count != constructor.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Different number of parameters");
                                for (int x = 0; x < constructor.Arguments.Count; x++) {
                                    await AssignOperation(constructor.Arguments[x], (await InterpreteNode(parameters[x], storage)).ToMarbleData(node.Right.Position), parameter_child_storage);
                                }
                            }
                            if (constructor.BaseParameters != null && constructor.BaseParameters.Count != 0) {
                                if (storage_class.Storage.Parent == null) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, constructor.BaseParameters[0].Position, "No parent");
                                if (!storage_class.Storage.Parent.HasConstructor("1")) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, constructor.BaseParameters[0].Position, "parent has no constructor");
                                StorageFunction.Constructor base_constructor = storage_class.Storage.Parent.GetConstructor("1");
                                if (base_constructor.UnlimitedArguments) {
                                    if (storage.HasVariable((base_constructor.Arguments[0] as DataNode).Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, base_constructor.Arguments[0].Position);
                                    List<MarbleData> unlimited = new List<MarbleData>();
                                    for (int x = 0; x < parameters.Count; x++) {
                                        unlimited.Add((await InterpreteNode(parameters[x], storage)).ToMarbleData(parameters[x].Position));
                                    }
                                    child_storage.CreateVariable((base_constructor.Arguments[0] as DataNode).Data as string, new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.LIST, false, false, new MarbleData(MarbleData.TYPE.LIST, unlimited)));
                                } else {
                                    if (constructor.BaseParameters.Count != base_constructor.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Different number of parameters");
                                    for (int x = 0; x < base_constructor.Arguments.Count; x++) {
                                        await AssignOperation(base_constructor.Arguments[x], (await InterpreteNode(constructor.BaseParameters[x], parameter_child_storage)).ToMarbleData(node.Right.Position), child_storage);
                                    }
                                }
                                await InterpreteInstructionListNode(base_constructor.Instructions, child_storage);
                            }
                            await InterpreteInstructionListNode(constructor.Instructions, parameter_child_storage);
                            return new MarbleData(MarbleData.TYPE.OBJECT, storage_class);
                        }
                        throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, data_node.Position);
                    default:
                        return null;
                }
            case BinaryOperatorNode binary: {
                StorageFunction function = (await InterpreteBinaryOperatorNode(binary, storage)).IsStorageFunction(binary.Position);
                return await RunFunction(function, parameters, binary, storage, storage.CreateChild());
            }
            default:
                return null;
        }
    }
    private async Task<InterpreterOutput> AccessorOperation(BinaryOperatorNode node, ContextualStorage storage) {
        MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);
        List<MarbleData> Elements = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position).as_list();
        switch (Left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: case MarbleData.TYPE.DICTIONARY: case MarbleData.TYPE.OBJECT:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
            case MarbleData.TYPE.STRING: {
                if (Elements.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                MarbleData index = Elements[0];
                if (index.type != MarbleData.TYPE.INTEGER) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Right.Position);
                int value = index.as_integer(node.Right.Position);
                if (value >= Left.as_string().Length) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Out of range");
                return new MarbleData(MarbleData.TYPE.INTEGER, Left.as_string()[value] + "");
            }
            case MarbleData.TYPE.LIST: {
                if (Elements.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                MarbleData index = Elements[0];
                if (index.type != MarbleData.TYPE.INTEGER) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Right.Position);
                if (index.as_integer() >= Left.as_list().Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Out of range");
                return Left.as_list()[index.as_integer()];
            }
            default:
                return null;
        }

    }
    private async Task<StorageVariable> AssignOperation(Node left, MarbleData data, ContextualStorage storage) {
        InterpreterOutput output = await InterpreteNode(left, storage);
        if (output is not StorageVariable) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, left.Position);
        StorageVariable storage_variable = output as StorageVariable;
        if (storage_variable.IsConstant && storage_variable.Data != null) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, left.Position, "This variable is constant");
        storage_variable.assign(data, left.Position);
        return storage_variable;
    }
    private async Task<MarbleData> AddOperation(MarbleData left, MarbleData right, ContextualStorage storage, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value + (int) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                    case MarbleData.TYPE.STRING:
                        return new MarbleData(MarbleData.TYPE.STRING, $"{(await MarbleDataToString(left, storage, position)).value}{(await MarbleDataToString(right, storage, position)).value}");
                    case MarbleData.TYPE.LIST: {
                        MarbleData copy = right.duplicate();
                        (copy.value as List<MarbleData>).Insert(0, left.duplicate());
                        return new MarbleData(MarbleData.TYPE.LIST, copy);
                    }
                }
                break;
            case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value + (int) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                    case MarbleData.TYPE.STRING:
                        return new MarbleData(MarbleData.TYPE.STRING, $"{(await MarbleDataToString(left, storage, position)).value}{(await MarbleDataToString(right, storage, position)).value}");
                    case MarbleData.TYPE.LIST: {
                        MarbleData copy = right.duplicate();
                        (copy.value as List<MarbleData>).Insert(0, left.duplicate());
                        return new MarbleData(MarbleData.TYPE.LIST, copy);
                    }
                };
                break;
            case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (int) right.value);
                    case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.FLOAT, left.as_float() + right.as_integer());
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                    case MarbleData.TYPE.STRING:
                        return new MarbleData(MarbleData.TYPE.STRING, $"{(await MarbleDataToString(left, storage, position)).value}{(await MarbleDataToString(right, storage, position)).value}");
                    case MarbleData.TYPE.LIST: {
                        MarbleData copy = right.duplicate();
                        (copy.value as List<MarbleData>).Insert(0, left.duplicate());
                        return new MarbleData(MarbleData.TYPE.LIST, copy);
                    }
                };
                break;
            case MarbleData.TYPE.STRING:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.STRING:
                        return new MarbleData(MarbleData.TYPE.STRING, $"{(await MarbleDataToString(left, storage, position)).value}{(await MarbleDataToString(right, storage, position)).value}");
                    case MarbleData.TYPE.LIST: {
                        MarbleData copy = right.duplicate();
                        (copy.value as List<MarbleData>).Insert(0, left.duplicate());
                        return new MarbleData(MarbleData.TYPE.LIST, copy);
                    }
                };
                break;
            case MarbleData.TYPE.LIST:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: case MarbleData.TYPE.STRING: {
                        MarbleData copy = right.duplicate();
                        (copy.value as List<MarbleData>).Insert(0, left.duplicate());
                        return new MarbleData(MarbleData.TYPE.LIST, copy);
                    }
                    case MarbleData.TYPE.LIST: {
                        MarbleData copy = right.duplicate();
                        foreach (MarbleData element in left.value as List<MarbleData>) (copy.value as List<MarbleData>).Add(element.duplicate());
                        return new MarbleData(MarbleData.TYPE.LIST, copy);
                    }
                };
                break;
            case MarbleData.TYPE.DICTIONARY:
                if (right.type == MarbleData.TYPE.STRING) return new MarbleData(MarbleData.TYPE.STRING, $"{(await MarbleDataToString(left, storage, position)).value}{(await MarbleDataToString(right, storage, position)).value}");
                break;
            case MarbleData.TYPE.CALLABLE:
                if (right.type == MarbleData.TYPE.STRING) return new MarbleData(MarbleData.TYPE.STRING, $"{(await MarbleDataToString(left, storage, position)).value}{(await MarbleDataToString(right, storage, position)).value}");
                break;
            case MarbleData.TYPE.OBJECT:
                if (right.type == MarbleData.TYPE.STRING) return new MarbleData(MarbleData.TYPE.STRING, $"{(await MarbleDataToString(left, storage, position)).value}{(await MarbleDataToString(right, storage, position)).value}");
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData SubtractOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value - (int) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value - (float) right.value);
                }
                break;
            case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value - (int) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value - (float) right.value);
                };
                break;
            case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value - (float) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value - (float) right.value);
                };
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData MultiplyOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value * (int) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value * (float) right.value);
                    case MarbleData.TYPE.STRING:
                        string new_string = "";
                        for (int x = 0; x < (int) right.value; x++) new_string += left.value;
                        return new MarbleData(MarbleData.TYPE.STRING, new_string);
                    case MarbleData.TYPE.LIST: {
                        List<MarbleData> new_list = new List<MarbleData>();
                        for (int x = 0; x < (int) right.value; x++) new_list.AddRange(left.duplicate().value as List<MarbleData>);
                        return new MarbleData(MarbleData.TYPE.LIST, new_list);
                    }
                }
                break;
            case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value + (int) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                    case MarbleData.TYPE.STRING:
                        string new_string = "";
                        for (int x = 0; x < (int) right.value; x++) new_string += left.value;
                        return new MarbleData(MarbleData.TYPE.STRING, new_string);
                    case MarbleData.TYPE.LIST: {
                        List<MarbleData> new_list = new List<MarbleData>();
                        for (int x = 0; x < (int) right.value; x++) new_list.AddRange(left.duplicate().value as List<MarbleData>);
                        return new MarbleData(MarbleData.TYPE.LIST, new_list);
                    }
                };
                break;
            case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                    case MarbleData.TYPE.STRING:
                        string new_string = "";
                        for (int x = 0; x < (int) right.value; x++) new_string += left.value;
                        return new MarbleData(MarbleData.TYPE.STRING, new_string);
                    case MarbleData.TYPE.LIST: {
                        List<MarbleData> new_list = new List<MarbleData>();
                        for (int x = 0; x < (int) right.value; x++) new_list.AddRange(left.duplicate().value as List<MarbleData>);
                        return new MarbleData(MarbleData.TYPE.LIST, new_list);
                    }
                };
                break;
            case MarbleData.TYPE.STRING:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        string new_string = "";
                        for (int x = 0; x < (int) right.convert_to(MarbleData.TYPE.INTEGER, position).value; x++) new_string += left.value;
                        return new MarbleData(MarbleData.TYPE.STRING, new_string);
                };
                break;
            case MarbleData.TYPE.LIST:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        List<MarbleData> new_list = new List<MarbleData>();
                        for (int x = 0; x < (int) right.convert_to(MarbleData.TYPE.INTEGER, position).value; x++) new_list.AddRange(left.duplicate().value as List<MarbleData>);
                        return new MarbleData(MarbleData.TYPE.LIST, new_list);
                };
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData DivideOperation(MarbleData left, MarbleData right, TokenPosition position) {
        float dividend = (float) right.convert_to(MarbleData.TYPE.FLOAT, position).value;
        if (dividend == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value / (int) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value / (float) right.value);
                }
                break;
            case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value + (int) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                };
                break;
            case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, (float) left.value + (float) right.value);
                };
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData IntegerDivideOperation(MarbleData left, MarbleData right, TokenPosition position) {
        int dividend = (int) right.convert_to(MarbleData.TYPE.INTEGER, position).value;
        if (dividend == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value / (int) right.value);
                    case MarbleData.TYPE.STRING: {
                        string right_string = right.value as string;
                        List<MarbleData> sub_strings = new List<MarbleData>();
                        string sub_string = "";
                        int sub_lenght = right_string.Length / (int) left.value;
                        for (int x = 0; x < right_string.Length; x++) {
                            if (x % sub_lenght == 0) {
                                sub_strings.Add(new MarbleData(MarbleData.TYPE.STRING, sub_string));
                                sub_string = "";
                            }
                            sub_string += right_string[x];
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, sub_strings);
                    }
                    case MarbleData.TYPE.LIST: {
                        List<MarbleData> right_list = right.value as List<MarbleData>;
                        List<MarbleData> sub_lists = new List<MarbleData>();
                        List<MarbleData> sub_list = new List<MarbleData>();
                        int sub_lenght = right_list.Count / (int) left.value;
                        for (int x = 0; x < right_list.Count; x++) {
                            if (x % sub_lenght == 0) {
                                sub_lists.Add(new MarbleData(MarbleData.TYPE.LIST, sub_list));
                                sub_list = new List<MarbleData>();
                            }
                            sub_list.Add(right_list[x]);
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, sub_lists);
                    }
                }
                break;
            case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value + (int) right.value);
                    case MarbleData.TYPE.STRING: {
                        string right_string = right.value as string;
                        List<MarbleData> sub_strings = new List<MarbleData>();
                        string sub_string = "";
                        int sub_lenght = right_string.Length / (int) left.value;
                        for (int x = 0; x < right_string.Length; x++) {
                            if (x % sub_lenght == 0) {
                                sub_strings.Add(new MarbleData(MarbleData.TYPE.STRING, sub_string));
                                sub_string = "";
                            }
                            sub_string += right_string[x];
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, sub_strings);
                    }
                    case MarbleData.TYPE.LIST: {
                        List<MarbleData> right_list = right.value as List<MarbleData>;
                        List<MarbleData> sub_lists = new List<MarbleData>();
                        List<MarbleData> sub_list = new List<MarbleData>();
                        int sub_lenght = right_list.Count / (int) left.value;
                        for (int x = 0; x < right_list.Count; x++) {
                            if (x % sub_lenght == 0) {
                                sub_lists.Add(new MarbleData(MarbleData.TYPE.LIST, sub_list));
                                sub_list = new List<MarbleData>();
                            }
                            sub_list.Add(right_list[x]);
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, sub_lists);
                    }
                };
                break;
            case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value + (int) right.value);
                    case MarbleData.TYPE.STRING: {
                        string right_string = right.value as string;
                        List<MarbleData> sub_strings = new List<MarbleData>();
                        string sub_string = "";
                        int sub_lenght = right_string.Length / (int) left.value;
                        for (int x = 0; x < right_string.Length; x++) {
                            if (x % sub_lenght == 0) {
                                sub_strings.Add(new MarbleData(MarbleData.TYPE.STRING, sub_string));
                                sub_string = "";
                            }
                            sub_string += right_string[x];
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, sub_strings);
                    }
                    case MarbleData.TYPE.LIST: {
                        List<MarbleData> right_list = right.value as List<MarbleData>;
                        List<MarbleData> sub_lists = new List<MarbleData>();
                        List<MarbleData> sub_list = new List<MarbleData>();
                        int sub_lenght = right_list.Count / (int) left.value;
                        for (int x = 0; x < right_list.Count; x++) {
                            if (x % sub_lenght == 0) {
                                sub_lists.Add(new MarbleData(MarbleData.TYPE.LIST, sub_list));
                                sub_list = new List<MarbleData>();
                            }
                            sub_list.Add(right_list[x]);
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, sub_lists);
                    }
                };
                break;
            case MarbleData.TYPE.STRING:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: {
                        string left_string = left.value as string;
                        List<MarbleData> sub_strings = new List<MarbleData>();
                        string sub_string = "";
                        int sub_lenght = left_string.Length / (int) right.value;
                        for (int x = 0; x < left_string.Length; x++) {
                            if (x % sub_lenght == 0) {
                                sub_strings.Add(new MarbleData(MarbleData.TYPE.STRING, sub_string));
                                sub_string = "";
                            }
                            sub_string += left_string[x];
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, sub_strings);
                    }
                };
                break;
            case MarbleData.TYPE.LIST:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: {
                        List<MarbleData> left_list = left.value as List<MarbleData>;
                        List<MarbleData> sub_lists = new List<MarbleData>();
                        List<MarbleData> sub_list = new List<MarbleData>();
                        int sub_lenght = left_list.Count / (int) right.value;
                        for (int x = 0; x < left_list.Count; x++) {
                            if (x % sub_lenght == 0) {
                                sub_lists.Add(new MarbleData(MarbleData.TYPE.LIST, sub_list));
                                sub_list = new List<MarbleData>();
                            }
                            sub_list.Add(left_list[x]);
                        }
                        return new MarbleData(MarbleData.TYPE.LIST, sub_lists);
                    }
                };
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData ExponentOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT,  Math.Pow((float) left.value, (float) right.value));
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData ModolusOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.FLOAT, left.as_integer() % right.as_integer());
                }
                break;
            case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.FLOAT, left.as_float() % right.as_float());
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData AndOperation(MarbleData left, MarbleData right, TokenPosition position) {
        return new MarbleData(MarbleData.TYPE.BOOLEAN, (bool) left.convert_to(MarbleData.TYPE.BOOLEAN, position).value && (bool) right.convert_to(MarbleData.TYPE.BOOLEAN, position).value);
    }
    private MarbleData BitwiseAndOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value & (int) right.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData OrOperation(MarbleData left, MarbleData right, TokenPosition position) {
        return new MarbleData(MarbleData.TYPE.BOOLEAN, (bool) left.convert_to(MarbleData.TYPE.BOOLEAN, position).value || (bool) right.convert_to(MarbleData.TYPE.BOOLEAN, position).value);
    }
    private MarbleData BitwiseOrOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.INTEGER, (int) left.value | (int) right.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData EqualsOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, left.value == right.value);
                }
                break;
            case MarbleData.TYPE.INTEGER:
                switch (right.type) {
                    case MarbleData.TYPE.INTEGER:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, left.as_integer() == right.as_integer());
                }
                break;
            case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, left.as_float() == right.as_float());
                }
                break;
            case MarbleData.TYPE.STRING:
                switch (right.type) {
                    case MarbleData.TYPE.STRING:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, left.as_string() == right.as_string());
                }
                break;
        }
        return new MarbleData(MarbleData.TYPE.BOOLEAN, false);
    }
    private MarbleData NotEqualsOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: case MarbleData.TYPE.STRING:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: case MarbleData.TYPE.STRING:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, left.value != right.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData LesserThanOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, (int) left.value < (int) right.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData LesserThanOrEqualsOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, (int) left.value <= (int) right.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData GreaterThanOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, (int) left.value > (int) right.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData GreaterThanOrEqualsOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                switch (right.type) {
                    case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, (int) left.value >= (int) right.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData InOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT: case MarbleData.TYPE.STRING:
                switch (right.type) {
                    case MarbleData.TYPE.LIST:
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, (right.value as List<MarbleData>).Any((MarbleData element) => element.value == left.value));}
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData IsOperation(MarbleData left, MarbleData right) {
        return new MarbleData(MarbleData.TYPE.BOOLEAN,left.type == right.type);
    }
    private async Task<InterpreterOutput> InterpreteBinaryOperatorNode(BinaryOperatorNode node, ContextualStorage storage) {
        switch (node.Operator.Type) {
            case OperatorToken.OPERATOR.DOT: return await DotOperation(node, storage);
            case OperatorToken.OPERATOR.ACCESSOR: return await AccessorOperation(node, storage);
            case OperatorToken.OPERATOR.CALLER: return await CallerOperation(node, storage);
            case OperatorToken.OPERATOR.ADD: return await AddOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), storage, node.Position);
            case OperatorToken.OPERATOR.SUBTRACT: return SubtractOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.MULTIPLY: return MultiplyOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.DIVIDE: return DivideOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.INTEGER_DIVIDE: return IntegerDivideOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.EXPONENT: return ExponentOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.MODOLUS: return ModolusOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.AND: return AndOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.BITWISE_AND: return BitwiseAndOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.OR: return OrOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.BITWISE_OR: return BitwiseOrOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.IN: return InOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.IS: return IsOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position));
            case OperatorToken.OPERATOR.EQUALS: return EqualsOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.NOT_EQUALS: return NotEqualsOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.GREATER_THAN: return GreaterThanOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS: return GreaterThanOrEqualsOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.LESSER_THAN: return LesserThanOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS: return LesserThanOrEqualsOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.ASSIGN: return await AssignOperation(node.Left, (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), storage); 
            case OperatorToken.OPERATOR.ADD_AND_ASSIGN: return await AssignOperation(node.Left, await AddOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), storage, node.Position), storage); 
            case OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN: return await AssignOperation(node.Left, SubtractOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position), storage); 
            case OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN: return await AssignOperation(node.Left, MultiplyOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position), storage); 
            case OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN: return await AssignOperation(node.Left, DivideOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position), storage); 
            case OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN: return await AssignOperation(node.Left, ExponentOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position), storage); 
            case OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN: return await AssignOperation(node.Left, ModolusOperation((await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), node.Position), storage);
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<InterpreterOutput> InterpreteUnaryOperatorNode(UnaryOperatorNode node, ContextualStorage storage) {
        switch (node.Operator) {
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.ADD:
                        return await InterpreteNode(node.Operand, storage);
                    case OperatorToken.OPERATOR.NOT: {
                        MarbleData data = (await InterpreteNode(node.Operand, storage)).ToMarbleData(node.Operand.Position);
                        if (data.type != MarbleData.TYPE.BOOLEAN) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                        return new MarbleData(MarbleData.TYPE.BOOLEAN, !data.as_boolean());
                    }
                    case OperatorToken.OPERATOR.SUBTRACT: {
                        MarbleData data = (await InterpreteNode(node.Operand, storage)).ToMarbleData(node.Operand.Position);
                        if (data.type != MarbleData.TYPE.INTEGER && data.type != MarbleData.TYPE.FLOAT) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                        return new MarbleData(data.type, -data.as_float());
                    }
                }
                break;
            /*case KeywordToken keyword_token:
                throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                switch (keyword_token.Type) {
                    case KeywordToken.TYPE.MODIFIER: {
                        StorageEntity storage_data = (await InterpreteNode(node.Operand, storage)).IsStorageEntity(node.Operand.Position);
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.CONSTANT:
                                if (storage_data is not StorageVariable) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Only for variables");
                                StorageVariable storage_variable = storage_data as StorageVariable;
                                if (storage_variable.IsConstant) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Repeat");
                                storage_variable.IsConstant = true;
                                break;
                            case KeywordToken.KEYWORD.PUBLIC:
                                if (storage_data.AccessMode != StorageEntity.ACCESSMODE.NONE) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Repeat");
                                storage_data.AccessMode = StorageEntity.ACCESSMODE.PUBLIC;
                                break;
                            case KeywordToken.KEYWORD.PRIVATE:
                                if (storage_data.AccessMode != StorageEntity.ACCESSMODE.NONE) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Repeat");
                                storage_data.AccessMode = StorageEntity.ACCESSMODE.PRIVATE;
                                break;
                            case KeywordToken.KEYWORD.STATIC:
                                if (storage_data.IsStatic) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Repeat");
                                storage_data.IsStatic = true;
                                break;
                        }
                        return storage_data;
                    }
                    case KeywordToken.TYPE.DATATYPE: {
                        DataNode identifier = node.Operand as DataNode;
                        if (storage.HasVariable(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                        StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESSMODE.NONE, false, (StorageEntity.DATATYPE) (keyword_token.Keyword - 4), false, null);
                        storage.CreateVariable(identifier.Data as string, storage_variable);
                        return storage_variable;
                    }
                    case KeywordToken.TYPE.DEFINITION:
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.FUNCTION: {
                                BinaryOperatorNode definition = node.Operand as BinaryOperatorNode;
                                BinaryOperatorNode function = definition.Left as BinaryOperatorNode;
                                UnaryOperatorNode identifier = function.Left as UnaryOperatorNode;
                                string name = (identifier.Operand as DataNode).Data as string;
                                if (storage.HasFunction(name)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                                StorageEntity.DATATYPE datatype = identifier.Operator.is_keyword_type(KeywordToken.TYPE.DATATYPE, out KeywordToken type)? (StorageEntity.DATATYPE) (type.Keyword - 4) : StorageEntity.DATATYPE.USER_DEFINED;
                                StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESSMODE.NONE, false, datatype, (function.Right as DataNode).Data as List<Node>, definition.Right as OperandInstructionNode);
                                storage.CreateFunction(name, storage_function);
                                return storage_function;
                            }
                            case KeywordToken.KEYWORD.CLASS: {
                                DataNode identifier = null;
                                ContextualStorage Class = null;
                                switch ((node.Operand as BinaryOperatorNode).Left) {
                                    case DataNode data: {
                                        identifier = data;
                                        Class = new ContextualStorage();
                                        break;
                                    }
                                    case BinaryOperatorNode bin: {
                                        identifier = bin.Left as DataNode;
                                        StorageEntity output = (await InterpreteNode(bin.Right, storage)).IsStorageEntity(bin.Right.Position);
                                        if (output is not StorageClass) new InterpreterError(InterpreterError.TYPE.MESSAGE, bin.Right.Position);
                                        Class = (output as StorageClass).Storage.CreateChild();
                                        break;
                                    }
                                }
                                switch (keyword_token.Keyword) {
                                    case KeywordToken.KEYWORD.CLASS:
                                        string name = identifier.Data as string;
                                        if (storage.HasClass(name)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                                        await InterpreteInstructionListNode((node.Operand as BinaryOperatorNode).Right as OperandInstructionNode, Class);
                                        StorageClass storage_class = new StorageClass(StorageEntity.ACCESSMODE.NONE, false, Class);
                                        storage.CreateClass(name, storage_class);
                                        return storage_class;
                                    case KeywordToken.KEYWORD.STRUCTURE:
                                    case KeywordToken.KEYWORD.ENUMERATION:
                                        break;
                                }
                                break;
                            }
                            case KeywordToken.KEYWORD.CONSTRUCTOR: {
                                BinaryOperatorNode definition = node.Operand as BinaryOperatorNode;
                                StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESSMODE.NONE, false, StorageEntity.DATATYPE.USER_DEFINED, (definition.Left as DataNode).Data as List<Node>, definition.Right as OperandInstructionNode);
                                storage.CreateFunction("Constructor!", storage_function);
                                return storage_function;
                            }
                        }
                    break;
                }
                break;
            case DataToken: {
                throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                DataNode identifier = node.Operand as DataNode;
                if (storage.HasVariable(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESSMODE.NONE, false, StorageEntity.DATATYPE.USER_DEFINED, false, null);
                storage.CreateVariable(identifier.Data as string, storage_variable);
                return storage_variable;
            }
            default:
                break;*/
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Weird keyword token");
    }
    private async Task<InterpreterOutput> InterpreteDataNode(DataNode node, ContextualStorage storage) {
        switch (node.Type) {
            case DataNode.TYPE.BOOLEAN:
                return new MarbleData(MarbleData.TYPE.BOOLEAN, node.Data);
            case DataNode.TYPE.INTEGER:
                return new MarbleData(MarbleData.TYPE.INTEGER, node.Data);
            case DataNode.TYPE.FLOAT:
                return new MarbleData(MarbleData.TYPE.FLOAT, node.Data);
            case DataNode.TYPE.STRING:
                return new MarbleData(MarbleData.TYPE.STRING, node.Data);
            case DataNode.TYPE.NULL:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case DataNode.TYPE.DICTIONARY: {
                Dictionary<object, MarbleData> dictionary = new Dictionary<object, MarbleData>();
                foreach (DataNode element in node.Data as List<Node>) {
                    Dictionary<string, object> key_value = element.Data as Dictionary<string, object>;
                    dictionary.Add((key_value["Token"] as DataToken).Data, (await InterpreteNode(key_value["Node"] as Node, storage)).ToMarbleData(element.Position));
                }
                return new MarbleData(MarbleData.TYPE.DICTIONARY, dictionary);
            }
            case DataNode.TYPE.LIST: {
                List<MarbleData> elements = new List<MarbleData>();
                foreach (Node element in node.Data as List<Node>) {
                    elements.Add((await InterpreteNode(element, storage)).ToMarbleData(element.Position));
                }
                return new  MarbleData(MarbleData.TYPE.LIST, elements);
            }
            case DataNode.TYPE.IDENTIFIER:
                if (!storage.GetEntity(node.Data as string, out StorageEntity storage_entity)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, node.Position);
                return storage_entity;
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<FlowController> InterpreteFlowControlNode(FlowControlNode node, ContextualStorage storage) {
        switch (node.Type) {
            case FlowController.TYPE.BREAKPOINT:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case FlowController.TYPE.RETURN:
                if (!storage.HasVariable("RETURN!")) throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, node.Position);
                StorageVariable storage_variable = storage.GetVariable("RETURN!");
                if (storage_variable.Datatype == StorageEntity.DATATYPE.VOID) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "This function does not return");
                storage_variable.assign((await InterpreteNode(node.Data, storage)).ToMarbleData(node.Data.Position), node.Position);
                return new FlowController.Return(storage_variable.Data);
            case FlowController.TYPE.BREAK:
                return new FlowController(FlowController.TYPE.BREAK);
            case FlowController.TYPE.CONTINUE:
                return new FlowController(FlowController.TYPE.CONTINUE);
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<FlowController> InterpreteWhileNode(WhileNode node, ContextualStorage storage) {
        while ((await InterpreteNode(node.Expression, storage)).ToMarbleData(node.Expression.Position).convert_to(MarbleData.TYPE.BOOLEAN, node.Expression.Position).as_boolean()) {
            FlowController breaker = await InterpreteInstructionListNode(node.Instructions, storage);
            if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
            if (breaker.Type == FlowController.TYPE.BREAK) break;
            if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteForNode(ForNode node, ContextualStorage storage) {
        ContextualStorage child_storage = storage.CreateChild();
        if (child_storage.HasVariable(node.Iterator.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Iterator.Position);
        StorageVariable Iterator = new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.VARIANT, false, false, null);
        child_storage.CreateVariable(node.Iterator.Data as string, Iterator);
        MarbleData Iteratable = (await InterpreteNode(node.Iteratable, child_storage)).ToMarbleData(node.Iteratable.Position);
        switch (Iteratable.type) {
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.FLOAT:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Iteratable.Position, "Can't loop with this");
            case MarbleData.TYPE.INTEGER: {
                Iterator.Data = new MarbleData(MarbleData.TYPE.INTEGER, 0);
                for (int x = 0; x < Iteratable.as_integer(); x++) {
                    Iterator.Data.value = x;
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleData.TYPE.STRING: {
                Iterator.Data = new MarbleData(MarbleData.TYPE.STRING, "");
                foreach (char item in Iteratable.as_string()) {
                    Iterator.Data.value = item + "";
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleData.TYPE.LIST: {
                Iterator.Data = new MarbleData(MarbleData.TYPE.INTEGER, 0);
                foreach (MarbleData element in Iteratable.as_list()) {
                    Iterator.Data = element;
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleData.TYPE.DICTIONARY:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteIFNode(IFNode node, ContextualStorage storage) {
        if ((await InterpreteNode(node.Expression, storage)).ToMarbleData(node.Expression.Position).convert_to(MarbleData.TYPE.BOOLEAN, node.Expression.Position).as_boolean()) {
            return await InterpreteInstructionListNode(node.Implication, storage);
        } else {
            bool do_else = true;
            foreach (IFNode.ElseIFNode child in node.Else_IF_nodes) {
                if ((await InterpreteNode(node.Expression, storage)).ToMarbleData(node.Expression.Position).convert_to(MarbleData.TYPE.BOOLEAN, node.Expression.Position).as_boolean()) {
                    return await InterpreteInstructionListNode(child.Implication, storage);
                }
            }
            if (do_else && node.Inverse != null) {
                return await InterpreteInstructionListNode(node.Inverse, storage);
            }
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteVariableDefinitionNode(VariableDefinitionNode node, ContextualStorage storage) {
        if (storage.HasVariable(node.Identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Identifier.Position);
        StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.VOID, node.StaticModifier != null, node.ConstantModifier != null, null);
        if (node.AccessModeModifier != null) {
            storage_variable.AccessMode = node.AccessModeModifier.Keyword switch {
                KeywordToken.KEYWORD.PRIVATE => StorageEntity.ACCESSMODE.PRIVATE,
                KeywordToken.KEYWORD.PUBLIC => StorageEntity.ACCESSMODE.PUBLIC,
                _ => throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.AccessModeModifier.Position)
            };
        }
        if (node.ReferenceModifier != null) {
            throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.ReferenceModifier.Position);
        }
        MarbleData data = null;
        if (node.Value != null) {
            data = (await InterpreteNode(node.Value, storage)).ToMarbleData(node.Value.Position);
        }
        if (node.Datatype is KeywordNode) {
            switch ((node.Datatype as KeywordNode).Keyword) {
                case KeywordToken.KEYWORD.VARIANT:
                    storage_variable.Datatype = StorageEntity.DATATYPE.VARIANT;
                    break;
                case KeywordToken.KEYWORD.BOOLEAN:
                    storage_variable.Datatype = StorageEntity.DATATYPE.BOOLEAN;
                    break;
                case KeywordToken.KEYWORD.INTEGER: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.INTEGER;
                    break;
                case KeywordToken.KEYWORD.FLOAT: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.FLOAT;
                    break;
                case KeywordToken.KEYWORD.STRING: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.STRING;
                    break;
                case KeywordToken.KEYWORD.LIST: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.LIST;
                    break;
                case KeywordToken.KEYWORD.OBJECT:
                    storage_variable.Datatype = StorageEntity.DATATYPE.USER_DEFINED;
                    break;
                case KeywordToken.KEYWORD.DICTIONARY: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.DICTIONARY;
                    break;
                case KeywordToken.KEYWORD.CALLABLE: 
                    storage_variable.Datatype = StorageEntity.DATATYPE.CALLABLE;
                    break;
            }
            if (data != null) storage_variable.assign(data, node.Value.Position);
        } else {
            storage_variable.Datatype = StorageEntity.DATATYPE.USER_DEFINED;
            if (data.type != MarbleData.TYPE.OBJECT) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
            StorageClass storage_class = (await InterpreteNode(node.Datatype, storage)).IsStorageClass(node.Datatype.Position);
            storage_variable.Class_name = storage_class.Class_name;
            if (data.as_object().Class_name != storage_class.Class_name) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
            storage_variable.Data = data;
        }
        storage.CreateVariable(node.Identifier.Data as string, storage_variable);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteClassDefinitionNode(ClassDefinitionNode node, ContextualStorage storage) {
        if (storage.HasClass(node.Identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Identifier.Position);
        ContextualStorage new_class;
        if (node.Datatype == null) {
            new_class = new ContextualStorage();
        } else {
            if (node.Datatype is KeywordNode) throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Identifier.Position);
            StorageClass parent_class = (await InterpreteNode(node.Datatype, storage)).IsStorageClass(node.Datatype.Position);
            new_class = parent_class.Storage.CreateChild();
        }
        StorageClass storage_class = new StorageClass(StorageEntity.ACCESSMODE.NONE, node.StaticModifier != null, node.Identifier.Data as string, new_class);
        if (node.AccessModeModifier != null) {
            switch (node.AccessModeModifier.Keyword) {
                case KeywordToken.KEYWORD.PRIVATE:
                    storage_class.AccessMode = StorageEntity.ACCESSMODE.PRIVATE;
                    break;
                case KeywordToken.KEYWORD.PUBLIC:
                    storage_class.AccessMode = StorageEntity.ACCESSMODE.PUBLIC;
                    break;
            }
        }
        await InterpreteInstructionListNode(node.Definition, storage_class.Storage);
        storage.CreateClass(node.Identifier.Data as string, storage_class);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteFunctionDefinitionNode(FunctionDefinitionNode node, ContextualStorage storage) {
        if (storage.HasFunction(node.Identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Identifier.Position);
        StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.VOID, node.StaticModifier != null, node.UnlimitedArguments, node.Arguments, node.Instructions);
        if (node.AccessModeModifier != null) {
            switch (node.AccessModeModifier.Keyword) {
                case KeywordToken.KEYWORD.PRIVATE:
                    storage_function.AccessMode = StorageEntity.ACCESSMODE.PRIVATE;
                    break;
                case KeywordToken.KEYWORD.PUBLIC:
                    storage_function.AccessMode = StorageEntity.ACCESSMODE.PUBLIC;
                    break;
            }
        }
        if (node.Datatype is KeywordNode) {
            switch ((node.Datatype as KeywordNode).Keyword) {
                case KeywordToken.KEYWORD.VARIANT:
                    storage_function.Datatype = StorageEntity.DATATYPE.VARIANT;
                    break;
                case KeywordToken.KEYWORD.BOOLEAN:
                    storage_function.Datatype = StorageEntity.DATATYPE.BOOLEAN;
                    break;
                case KeywordToken.KEYWORD.INTEGER:
                    storage_function.Datatype = StorageEntity.DATATYPE.INTEGER;
                    break;
                case KeywordToken.KEYWORD.FLOAT:
                    storage_function.Datatype = StorageEntity.DATATYPE.FLOAT;
                    break;
                case KeywordToken.KEYWORD.STRING:
                    storage_function.Datatype = StorageEntity.DATATYPE.STRING;
                    break;
                case KeywordToken.KEYWORD.LIST:
                    storage_function.Datatype = StorageEntity.DATATYPE.LIST;
                    break;
                case KeywordToken.KEYWORD.DICTIONARY:
                    storage_function.Datatype = StorageEntity.DATATYPE.DICTIONARY;
                    break;
            }
        } else {
            storage_function.Datatype = StorageEntity.DATATYPE.USER_DEFINED;
            StorageClass storage_class = (await InterpreteNode(node.Datatype, storage)).IsStorageClass(node.Datatype.Position);
            storage_function.Class_name = storage_class.Class_name;
        }
        storage.CreateFunction(node.Identifier.Data as string, storage_function);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteConstructorNode(ConstructorNode node, ContextualStorage storage) {
        StorageFunction.Constructor storage_constructor = new StorageFunction.Constructor(StorageEntity.ACCESSMODE.NONE, node.UnlimitedArguments, node.Arguments, node.BaseParameters, node.Instructions);
        if (node.AccessModeModifier != null) {
            switch (node.AccessModeModifier.Keyword) {
                case KeywordToken.KEYWORD.PRIVATE:
                    storage_constructor.AccessMode = StorageEntity.ACCESSMODE.PRIVATE;
                    break;
                case KeywordToken.KEYWORD.PUBLIC:
                    storage_constructor.AccessMode = StorageEntity.ACCESSMODE.PUBLIC;
                    break;
            }
        }
        storage.CreateConstructor("1", storage_constructor);
        return await Task.Run(() => new FlowController(FlowController.TYPE.DONE));
    }
}