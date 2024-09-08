using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public class Interpreter {
    public static InputGetter Input_dialog;
    public string Output = "";
    public async Task<InterpreterOutput> Interprete(List<Node> nodes, ContextualStorage main_context) {
        foreach (Node node in nodes) await InterpreteNode(node, main_context);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<InterpreterOutput> InterpreteNode(Node node, ContextualStorage main_context) => node switch {
        InlineFunctionDefinitionNode switch_node => await InterpreteInlineFunctionDefinitionNode(switch_node),
        VariableDefinitionNode switch_node => await InterpreteVariableDefinitionNode(switch_node, main_context),
        ClassDefinitionNode switch_node => await InterpreteClassDefinitionNode(switch_node, main_context),
        FunctionDefinitionNode switch_node => await InterpreteFunctionDefinitionNode(switch_node, main_context),
        FunctionArgumentNode switch_node => await InterpreteFunctionArgumentNode(switch_node, main_context),
        ConstructorNode switch_node => await InterpreteConstructorNode(switch_node, main_context),
        BinaryOperatorNode switch_node => await InterpreteBinaryOperatorNode(switch_node, main_context),
        UnaryOperatorNode switch_node => await InterpreteUnaryOperatorNode(switch_node, main_context),
        
        DataNode switch_node => await InterpreteDataNode(switch_node, main_context),

        FlowControlNode switch_node => await InterpreteFlowControlNode(switch_node, main_context),
        WhileNode switch_node => await InterpreteWhileNode(switch_node, main_context),
        ForNode switch_node => await InterpreteForNode(switch_node, main_context),
        IFNode switch_node => await InterpreteIFNode(switch_node, main_context),
        FunctionParameterNode switch_node => await InterpreteFunctionParameterNode(switch_node, main_context),
        _ => throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position, "new node?")
    };
    
    private async Task<StorageVariable> InterpreteFunctionArgumentNode(FunctionArgumentNode node, ContextualStorage main_context) {
        if (main_context.HasVariable(node.Identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Identifier.Position);
        StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.VOID, false, false, null);
        MarbleData data = null;
        if (node.Value != null) {
            data = (await InterpreteNode(node.Value, main_context)).ToMarbleData(node.Value.Position);
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
            if (data != null) {
                switch (storage_variable.Datatype) {
                    case StorageEntity.DATATYPE.VARIANT:
                        storage_variable.Data = data.duplicate();
                        break;
                    case StorageEntity.DATATYPE.BOOLEAN:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.BOOLEAN, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.INTEGER:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.INTEGER, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.FLOAT:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.FLOAT, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.STRING:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.STRING, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.LIST:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.LIST, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.DICTIONARY:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.DICTIONARY, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.CALLABLE:
                        if (data is MarbleCallable) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
                        storage_variable.Data = data.duplicate();
                        break;
                }
            }
        } else {
            storage_variable.Datatype = StorageEntity.DATATYPE.USER_DEFINED;
            StorageClass storage_class = (await InterpreteNode(node.Datatype, main_context)).IsStorageClass(node.Datatype.Position);
            storage_variable.Class_name = storage_class.Class_name;
            if (data != null) {
                if (data is not MarbleObject) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
                if ((data.get_value() as StorageClass).Class_name != storage_class.Class_name) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
                storage_variable.Data = data;
            }
        }
        main_context.CreateVariable(node.Identifier.Data as string, storage_variable);
        return storage_variable;
    }
    private async Task<MarbleData> InterpreteInlineFunctionDefinitionNode(InlineFunctionDefinitionNode node) {
        StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.VARIANT, false, node.UnlimitedArguments, node.Arguments, node.Instructions);
        return await Task.Run(() => new MarbleCallable(storage_function));
    }
    private async Task<InterpreterOutput> InterpreteFunctionParameterNode(FunctionParameterNode node, ContextualStorage main_context) {
        if (node is FunctionParameterNode.Classified) throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
        return await InterpreteNode(node.Parameter, main_context);
    }
    private async Task<FlowController> InterpreteInstructionListNode(OperandInstructionNode node, ContextualStorage main_context) {
        if (node.Oneline) {
            return new FlowController.Return((await InterpreteNode(node.Instructions[0], main_context)).ToMarbleData(node.Instructions[0].Position));
        } else {
            foreach (Node statement in node.Instructions) {
                InterpreterOutput result = await InterpreteNode(statement, main_context);
                if (result is FlowController && (result as FlowController).Type != FlowController.TYPE.DONE) return result as FlowController;
            }
            return new FlowController(FlowController.TYPE.DONE);
        }
    }
    private async Task<InterpreterOutput> RunFunction(StorageFunction function, List<Node> parameters, ContextualStorage main_context, ContextualStorage function_context, TokenPosition position) {
        if (function.UnlimitedArguments) {
            if (main_context.HasVariable((function.Arguments[0] as DataNode).Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, function.Arguments[0].Position);
            List<MarbleData> unlimited = new List<MarbleData>();
            for (int x = 0; x < parameters.Count; x++) {
                unlimited.Add((await InterpreteNode(parameters[x], main_context)).ToMarbleData(parameters[x].Position));
            }
            function_context.CreateVariable((function.Arguments[0] as DataNode).Data as string, new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.LIST, false, false, new MarbleList(unlimited)));
        } else {
            if (parameters.Count != function.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Different number of parameters");
            for (int x = 0; x < function.Arguments.Count; x++) {
                await AssignOperation(function.Arguments[x], (await InterpreteNode(parameters[x], main_context)).ToMarbleData(parameters[x].Position), function_context);
            }
        }
        StorageVariable come_back = new StorageVariable(StorageEntity.ACCESSMODE.PRIVATE, function.Datatype, false,  false, null);
        if (function.Datatype == StorageEntity.DATATYPE.USER_DEFINED) come_back.Class_name = function.Class_name;
        function_context.CreateVariable("RETURN!", come_back);
        FlowController output = await InterpreteInstructionListNode(function.Instructions, function_context);
        switch (output.Type) {
            case FlowController.TYPE.RETURN:
                return (output as FlowController.Return).Data;
            case FlowController.TYPE.DONE:
                if (function.Datatype != StorageEntity.DATATYPE.VOID) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "No return statement");
                return output;
            default:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Check function output");
        }
    }
    private async Task<InterpreterOutput> DotOperation(BinaryOperatorNode node, ContextualStorage main_context) {
        InterpreterOutput Left = await InterpreteNode(node.Left, main_context);
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
                        return await RunFunction(storage_function, parameters, main_context, storage_class.Storage.CreateChild(), function.Position);
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
                        /*ACCESSORS?*/
                        DataNode identifier = function.Left as DataNode;
                        List<Node> parameters = (function.Right as DataNode).Data as List<Node>;
                        switch (data) {
                            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleDictionary:
                                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                            case MarbleString:
                                switch (identifier.Data as string) {
                                    case "has":
                                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        return InOperation((await InterpreteNode(parameters[0], main_context)).ToMarbleData(parameters[0].Position), data, parameters[0].Position);
                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
                                }
                            case MarbleList list: {
                                switch (identifier.Data as string) {
                                    case "append":
                                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        MarbleData new_element = (await InterpreteNode(parameters[0], main_context)).ToMarbleData(parameters[0].Position);
                                        list.value.Add(new_element);
                                        break;
                                    case "remove":
                                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        MarbleData point = (await InterpreteNode(parameters[0], main_context)).ToMarbleData(parameters[0].Position);
                                        if (point is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, parameters[0].Position);
                                        list.value.RemoveAt((int) point.get_value());
                                        break;
                                    case "has":
                                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        return InOperation((await InterpreteNode(parameters[0], main_context)).ToMarbleData(parameters[0].Position), data, parameters[0].Position);
                                    case "clear":
                                        if (parameters.Count > 0) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        list.value.Clear();
                                        return new FlowController(FlowController.TYPE.DONE);

                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
                                }
                                return null;
                            }
                            case MarbleObject marble_object: {
                                //if (function.Operator.Type == OperatorToken.OPERATOR.EXPONENT) {
                                if(!marble_object.value.Storage.HasFunction(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_FUNCTION, identifier.Position);
                                StorageFunction storage_function = marble_object.value.Storage.GetFunction(identifier.Data as string);
                                return await RunFunction(storage_function, parameters, main_context, marble_object.value.Storage.CreateChild(), function.Position);
                                //} else {
                                //    return await InterpreteNode(node.Right, main_context.CreateLoveChild(marble_object.value.Storage));
                                //}
                            }
                            default:
                                return null;
                        }
                    }
                    case DataNode identifier: {
                        switch (data) {
                            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleDictionary:
                                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                            case MarbleString marble_string:
                                switch(identifier.Data) {
                                    case "size":
                                        return new MarbleInteger(marble_string.value.Length);
                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, identifier.Position, "undefined");
                                }
                            case MarbleList list:
                                switch(identifier.Data) {
                                    case "size":
                                        return new MarbleInteger(list.value.Count);
                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, identifier.Position, "undefined");
                                }
                            case MarbleObject marble_object: {
                                if (!marble_object.value.Storage.GetEntity(identifier.Data as string, out StorageEntity storage_entity)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, identifier.Position);
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
    private async Task<InterpreterOutput> CallerOperation(BinaryOperatorNode node, ContextualStorage main_context) {
        List<Node> parameters = (node.Right as DataNode).Data as List<Node>;
        switch (node.Left) {
            case KeywordNode keyword_node: {
                switch (keyword_node.Keyword) {
                    case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.BOOLEAN: case KeywordToken.KEYWORD.INTEGER:
                    case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST: case KeywordToken.KEYWORD.DICTIONARY:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                    case KeywordToken.KEYWORD.ASSERT: {
                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                        MarbleBoolean assertion = (await ConvertOperation(MarbleData.TYPE.BOOLEAN, (await InterpreteNode(parameters[0], main_context)).ToMarbleData(parameters[0].Position), parameters[0].Position)) as MarbleBoolean;
                        if (!assertion.value) throw new InterpreterError(InterpreterError.TYPE.ASSERTION_FAILED, node.Right.Position);
                        return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.PRINT: {
                        foreach (Node parameter in parameters) {
                            Output += $"{(await ConvertOperation(MarbleData.TYPE.STRING, (await InterpreteNode(parameter, main_context)).ToMarbleData(parameter.Position), parameter.Position)).get_value()} ";
                        }
                        return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.PRINTLINE: {
                        foreach (Node parameter in parameters) {
                            Output += $"{(await ConvertOperation(MarbleData.TYPE.STRING, (await InterpreteNode(parameter, main_context)).ToMarbleData(parameter.Position), parameter.Position)).get_value()} ";
                        }
                        Output += '\n';
                        return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.RANGE: {
                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        MarbleInteger integer = (await ConvertOperation(MarbleData.TYPE.INTEGER, (await InterpreteNode(parameters[0], main_context)).ToMarbleData(parameters[0].Position), parameters[0].Position)) as MarbleInteger;
                        List<MarbleData> elements = new List<MarbleData>();
                        for (int x = 0; x < integer.value; x++) {
                            elements.Add(new MarbleInteger(x));
                        }
                        return new MarbleList(elements);
                    }
                    case KeywordToken.KEYWORD.RANDOM: {
                        if (parameters.Count > 2) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        MarbleInteger left = (await ConvertOperation(MarbleData.TYPE.INTEGER, (await InterpreteNode(parameters[0], main_context)).ToMarbleData(parameters[0].Position), parameters[0].Position)) as MarbleInteger;
                        MarbleInteger right = (await ConvertOperation(MarbleData.TYPE.INTEGER, (await InterpreteNode(parameters[1], main_context)).ToMarbleData(parameters[1].Position), parameters[1].Position)) as MarbleInteger;
                        return new MarbleInteger(new Random().Next(left.value, right.value));
                    }
                    case KeywordToken.KEYWORD.INPUT: {
                        if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        Input_dialog.DialogText = $"{(await InterpreteNode(parameters[0], main_context)).ToMarbleData(parameters[0].Position)}";
                        Input_dialog.Show();
                        await Input_dialog.ToSignal(Input_dialog, "confirmed");
                        return new MarbleString(Input_dialog.Input);
                    }
                    default:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                }
            }
            case DataNode data_node:
                switch (data_node.Type) {
                    case DataNode.TYPE.IDENTIFIER:
                        if (main_context.HasFunction(data_node.Data as string)) {
                            return await RunFunction(main_context.GetFunction(data_node.Data as string), parameters, main_context, main_context.CreateChild(), node.Position);
                        } else if (main_context.HasVariable(data_node.Data as string)) {
                            StorageVariable variable = main_context.GetVariable(data_node.Data as string);
                            if (variable.Datatype != StorageEntity.DATATYPE.CALLABLE) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_FUNCTION, data_node.Position);
                            return await RunFunction(variable.Data.get_value() as StorageFunction, parameters, main_context, main_context.CreateChild(), null);
                        } else if (main_context.HasClass(data_node.Data as string)) {
                            StorageClass storage_class = main_context.GetClass(data_node.Data as string);
                            ContextualStorage child_storage = storage_class.Storage.CreateChild();
                            StorageFunction.Constructor constructor = storage_class.Storage.GetConstructor("1");
                            if (constructor == null) {
                                return new MarbleObject(storage_class);
                            }
                            ContextualStorage parameter_child_storage = child_storage.CreateChild();
                            if (constructor.UnlimitedArguments) {
                                if (main_context.HasVariable((constructor.Arguments[0] as DataNode).Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, constructor.Arguments[0].Position);
                                List<MarbleData> unlimited = new List<MarbleData>();
                                for (int x = 0; x < parameters.Count; x++) {
                                    unlimited.Add((await InterpreteNode(parameters[x], main_context)).ToMarbleData(parameters[x].Position));
                                }
                                parameter_child_storage.CreateVariable((constructor.Arguments[0] as DataNode).Data as string, new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.LIST, false, false, new MarbleList(unlimited)));
                            } else {
                                if (parameters.Count != constructor.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Different number of parameters");
                                for (int x = 0; x < constructor.Arguments.Count; x++) {
                                    await AssignOperation(constructor.Arguments[x], (await InterpreteNode(parameters[x], main_context)).ToMarbleData(node.Right.Position), parameter_child_storage);
                                }
                            }
                            if (constructor.BaseParameters != null && constructor.BaseParameters.Count != 0) {
                                if (storage_class.Storage.Parent == null) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, constructor.BaseParameters[0].Position, "No parent");
                                if (!storage_class.Storage.Parent.HasConstructor("1")) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, constructor.BaseParameters[0].Position, "parent has no constructor");
                                StorageFunction.Constructor base_constructor = storage_class.Storage.Parent.GetConstructor("1");
                                if (base_constructor.UnlimitedArguments) {
                                    if (main_context.HasVariable((base_constructor.Arguments[0] as DataNode).Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, base_constructor.Arguments[0].Position);
                                    List<MarbleData> unlimited = new List<MarbleData>();
                                    for (int x = 0; x < parameters.Count; x++) {
                                        unlimited.Add((await InterpreteNode(parameters[x], main_context)).ToMarbleData(parameters[x].Position));
                                    }
                                    child_storage.CreateVariable((base_constructor.Arguments[0] as DataNode).Data as string, new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.LIST, false, false, new MarbleList(unlimited)));
                                } else {
                                    if (constructor.BaseParameters.Count != base_constructor.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Different number of parameters");
                                    for (int x = 0; x < base_constructor.Arguments.Count; x++) {
                                        await AssignOperation(base_constructor.Arguments[x], (await InterpreteNode(constructor.BaseParameters[x], parameter_child_storage)).ToMarbleData(node.Right.Position), child_storage);
                                    }
                                }
                                await InterpreteInstructionListNode(base_constructor.Instructions, child_storage);
                            }
                            await InterpreteInstructionListNode(constructor.Instructions, parameter_child_storage);
                            return new MarbleObject(storage_class);
                        }
                        throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, data_node.Position);
                    default:
                        return null;
                }
            case BinaryOperatorNode binary: {
                StorageFunction function = (await InterpreteBinaryOperatorNode(binary, main_context)).IsStorageFunction(binary.Position);
                return await RunFunction(function, parameters, main_context, main_context.CreateChild(), binary.Position);
            }
            default:
                return null;
        }
    }
    private async Task<InterpreterOutput> AccessorOperation(BinaryOperatorNode node, ContextualStorage main_context) {
        MarbleData Left = (await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position);
        List<MarbleData> parameters = ((await ConvertOperation(MarbleData.TYPE.LIST, (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Right.Position)) as MarbleList).value;
        switch (Left) {
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleDictionary: case MarbleObject:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
            case MarbleString marble_string: {
                if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                MarbleData index = parameters[0];
                if (index is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Right.Position);
                int value = (int) index.get_value();
                if (value >= marble_string.value.Length) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Out of range");
                return new MarbleString(marble_string.value[value] + "");
            }
            case MarbleList list: {
                if (parameters.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                MarbleData index = parameters[0];
                if (index is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Right.Position);
                int value = (int) index.get_value();
                if (value >= list.value.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Out of range");
                return list.value[value];
            }
            default:
                return null;
        }
    }
    private async Task<StorageVariable> AssignOperation(Node left, MarbleData data, ContextualStorage main_context) {
        InterpreterOutput output = await InterpreteNode(left, main_context);
        if (output is not StorageVariable) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, left.Position);
        StorageVariable storage_variable = output as StorageVariable;
        if (storage_variable.IsConstant && storage_variable.Data != null) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, left.Position, "This variable is constant");
        switch (storage_variable.Datatype) {
            case StorageEntity.DATATYPE.VARIANT:
                storage_variable.Data = data.duplicate();
                break;
            case StorageEntity.DATATYPE.BOOLEAN:
                storage_variable.Data = await ConvertOperation(MarbleData.TYPE.BOOLEAN, data, left.Position);
                break;
            case StorageEntity.DATATYPE.INTEGER:
                storage_variable.Data = await ConvertOperation(MarbleData.TYPE.INTEGER, data, left.Position);
                break;
            case StorageEntity.DATATYPE.FLOAT:
                storage_variable.Data = await ConvertOperation(MarbleData.TYPE.FLOAT, data, left.Position);
                break;
            case StorageEntity.DATATYPE.STRING:
                storage_variable.Data = await ConvertOperation(MarbleData.TYPE.STRING, data, left.Position);
                break;
            case StorageEntity.DATATYPE.LIST:
                storage_variable.Data = await ConvertOperation(MarbleData.TYPE.LIST, data, left.Position);
                break;
            case StorageEntity.DATATYPE.DICTIONARY:
                storage_variable.Data = await ConvertOperation(MarbleData.TYPE.DICTIONARY, data, left.Position);
                break;
            case StorageEntity.DATATYPE.CALLABLE:
                if (data is MarbleCallable) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, left.Position);
                storage_variable.Data = data.duplicate();
                break;
            case StorageEntity.DATATYPE.USER_DEFINED:
                if (data is not MarbleObject) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, left.Position);
                if ((data as MarbleObject).value.Class_name != storage_variable.Class_name)  throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, left.Position);
                storage_variable.Data = data.duplicate();
                break;
        }
        return storage_variable;
    }
    private async Task<MarbleData> ConvertOperation(MarbleData.TYPE type, MarbleData operand, TokenPosition position) {
        switch (type) {
            case MarbleData.TYPE.BOOLEAN:
                switch (operand) {
                    case MarbleBoolean:
                        return operand.duplicate();
                    case MarbleInteger data:
                        return new MarbleBoolean(data.value > 0);
                    case MarbleFloat data:
                        return new MarbleBoolean(data.value > 0);
                    case MarbleString data:
                        return new MarbleBoolean(bool.Parse(data.value));
                    case MarbleList data:
                        return new MarbleBoolean(data != null);
                    case MarbleDictionary data:
                        return new MarbleBoolean(data != null);
                    case MarbleCallable data:
                        return new MarbleBoolean(data != null);
                    case MarbleObject data:
                        return new MarbleBoolean(data != null);
                }
                break;
            case MarbleData.TYPE.INTEGER:
                switch (operand) {
                    case MarbleBoolean data:
                        return new MarbleInteger(data.value? 1 : 0);
                    case MarbleInteger data:
                        return data.duplicate();
                    case MarbleFloat data:
                        return new MarbleInteger((int) data.value);
                    case MarbleString data:
                        return new MarbleInteger(int.Parse(data.value));
                }
                break;
            case MarbleData.TYPE.FLOAT:
                switch (operand) {
                    case MarbleBoolean data:
                        return new MarbleFloat(data.value? 1 : 0);
                    case MarbleInteger data:
                        return new MarbleFloat(data.value);
                    case MarbleFloat data:
                        return data.duplicate();
                    case MarbleString data:
                        return new MarbleFloat(float.Parse(data.value));
                }
                break;
            case MarbleData.TYPE.STRING:
                switch (operand) {
                    case MarbleBoolean: case MarbleInteger: case MarbleFloat:
                        return new MarbleString($"{operand.get_value()}");
                    case MarbleString:
                        return operand.duplicate();
                    case MarbleList data:
                        string output = "[";
                        foreach (MarbleData element in data.value) {
                            output += (await ConvertOperation(MarbleData.TYPE.STRING, element, position)).get_value() + ", ";
                        }
                        output = output.Remove(output.Length - 2);
                        output += "]";
                        return new MarbleString(output);
                    case MarbleDictionary:
                        return new MarbleString("<Dictionary>");
                    case MarbleCallable:
                        return new MarbleString("<Callable>");
                    case MarbleObject data:
                        if (data.value.Storage.HasFunction("to_string")) {
                            StorageFunction function = data.value.Storage.GetFunction("to_string");
                            ContextualStorage main_context = data.value.Storage.CreateChild();
                            return (await RunFunction(function, new List<Node>(), main_context, main_context, function.Position)).ToMarbleData(position);
                        } else {
                            return new MarbleString("<Object>");
                        }
                }
                break;
            case MarbleData.TYPE.LIST:
                switch (operand) {
                    case MarbleBoolean: case MarbleInteger: case MarbleFloat:
                        return new MarbleList(new List<MarbleData>(){ operand.duplicate() });
                    case MarbleString right_string:
                        MarbleList list = new MarbleList(new List<MarbleData>());
                        foreach (char element in right_string.value) {
                            list.value.Add(new MarbleString(element + ""));
                        }
                        return list;
                    case MarbleList:
                        return operand.duplicate();
                }
                break;
            case MarbleData.TYPE.DICTIONARY:
            case MarbleData.TYPE.CALLABLE:
            case MarbleData.TYPE.OBJECT:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private async Task<MarbleData> AddOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean left_boolean:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleInteger((left_boolean.value? 1 : 0) + right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat((left_boolean.value? 1 : 0) + right_float.value);
                    case MarbleString right_string:
                        return new MarbleString($"{left_boolean.value}{right_string}");
                    case MarbleList right_list: {
                        MarbleList copy = right_list.duplicate();
                        copy.value.Insert(0, left_boolean.duplicate());
                        return copy;
                    }
                }
                break;
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleInteger(left_integer.value + (right_boolean.value? 1 : 0));
                    case MarbleInteger right_integer:
                        return new MarbleInteger(left_integer.value + right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat(left_integer.value + right_float.value);
                    case MarbleString right_string:
                        return new MarbleString($"{left_integer.value}{right_string}");
                    case MarbleList right_list: {
                        MarbleList copy = right_list.duplicate();
                        copy.value.Insert(0, left_integer.duplicate());
                        return copy;
                    }
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleFloat(left_float.value + (right_boolean.value? 1 : 0));
                    case MarbleInteger right_integer:
                        return new MarbleFloat(left_float.value + right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat(left_float.value + right_float.value);
                    case MarbleString right_string:
                        return new MarbleString($"{left_float.value}{right_string}");
                    case MarbleList right_list: {
                        MarbleList copy = right_list.duplicate();
                        copy.value.Insert(0, left.duplicate());
                        return copy;
                    }
                };
                break;
            case MarbleString left_string:
                switch (right) {
                    case MarbleBoolean: case MarbleInteger: case MarbleString:
                        return new MarbleString($"{left_string.value}{right.get_value()}");
                    case MarbleList right_list: {
                        MarbleList copy = right_list.duplicate();
                        copy.value.Insert(0, left_string.duplicate());
                        return copy;
                    }
                    case MarbleCallable: case MarbleObject:
                        return new MarbleString($"{left_string.value}{await ConvertOperation(MarbleData.TYPE.STRING, right, position)}");
                };
                break;
            case MarbleList left_list:
                switch (right) {
                    case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString: {
                        MarbleList copy = left_list.duplicate();
                        copy.value.Add(right.duplicate());
                        return copy;
                    }
                    case MarbleList right_list: {
                        MarbleList copy = left_list.duplicate();
                        foreach (MarbleData element in right_list.value) copy.value.Add(element.duplicate());
                        return copy;
                    }
                };
                break;
            case MarbleDictionary left_dictionary:
                switch (right) {
                    case MarbleString right_string:
                        return new MarbleString($"{await ConvertOperation(MarbleData.TYPE.STRING, left_dictionary, position)}{right_string.value}");
                    case MarbleList right_list: {
                        MarbleList copy = right_list.duplicate();
                        copy.value.Insert(0, left_dictionary.duplicate());
                        return copy;
                    }
                }
                break;
            case MarbleCallable left_callable:
                switch (right) {
                    case MarbleString right_string:
                        return new MarbleString($"{await ConvertOperation(MarbleData.TYPE.STRING, left_callable, position)}{right_string.value}");
                    case MarbleList right_list: {
                        MarbleList copy = right_list.duplicate();
                        copy.value.Insert(0, left_callable.duplicate());
                        return copy;
                    }
                }
                break;
            case MarbleObject left_object:
                switch (right) {
                    case MarbleString right_string:
                        return new MarbleString($"{await ConvertOperation(MarbleData.TYPE.STRING, left_object, position)}{right_string}");
                    case MarbleList right_list: {
                        MarbleList copy = right_list.duplicate();
                        copy.value.Insert(0, left_object.duplicate());
                        return copy;
                    }
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData SubtractOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean left_boolean:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleInteger((left_boolean.value? 1 : 0) - right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat((left_boolean.value? 1 : 0) - right_float.value);
                }
                break;
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleInteger(left_integer.value - (right_boolean.value? 1 : 0));
                    case MarbleInteger right_integer:
                        return new MarbleInteger(left_integer.value - right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat(left_integer.value - right_float.value);
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleFloat(left_float.value - (right_boolean.value? 1 : 0));
                    case MarbleInteger right_integer:
                        return new MarbleFloat(left_float.value - right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat(left_float.value - right_float.value);
                }
                break;
            };
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData MultiplyOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean left_boolean:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleInteger((left_boolean.value? 1 : 0) * right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat((left_boolean.value? 1 : 0) * right_float.value);
                }
                break;
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleInteger(left_integer.value * (right_boolean.value? 1 : 0));
                    case MarbleInteger right_integer:
                        return new MarbleInteger(left_integer.value * right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat(left_integer.value * right_float.value);
                    case MarbleString right_string:
                        string new_string = "";
                        for (int x = 0; x < left_integer.value; x++) new_string += right_string.value;
                        return new MarbleString(new_string);
                    case MarbleList right_list: {
                        List<MarbleData> new_list = new List<MarbleData>();
                        for (int x = 0; x < left_integer.value; x++) new_list.Add(right_list.duplicate());
                        return new MarbleList(new_list);
                    }
                };
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleFloat(left_float.value * (right_boolean.value? 1 : 0));
                    case MarbleInteger right_integer:
                        return new MarbleFloat(left_float.value * right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleFloat(left_float.value * right_float.value);
                    case MarbleString right_string:
                        string new_string = "";
                        for (int x = 0; x < (int) left_float.value; x++) new_string += right_string.value;
                        return new MarbleString(new_string);
                    case MarbleList right_list: {
                        List<MarbleData> new_list = new List<MarbleData>();
                        for (int x = 0; x < (int) left_float.value; x++) new_list.Add(right_list.duplicate());
                        return new MarbleList(new_list);
                    }
                };
                break;
            case MarbleString left_string:
                switch (right) {
                    case MarbleInteger: case MarbleFloat:
                        string new_string = "";
                        for (int x = 0; x < (int) right.get_value(); x++) new_string += left_string.value;
                        return new MarbleString(new_string);
                };
                break;
            case MarbleList left_list:
                switch (right) {
                    case MarbleInteger: case MarbleFloat:
                        List<MarbleData> new_list = new List<MarbleData>();
                        for (int x = 0; x < (int) right.get_value(); x++) new_list.Add(left_list.duplicate());
                        return new MarbleList(new_list);
                };
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData DivideOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        if (right_integer.value == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                        return new MarbleInteger(left_integer.value / right_integer.value);
                    case MarbleFloat right_float:
                        if (right_float.value == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                        return new MarbleFloat(left_integer.value / right_float.value);
                };
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleInteger right_integer:
                        if (right_integer.value == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                        return new MarbleFloat(left_float.value / right_integer.value);
                    case MarbleFloat right_float:
                        if (right_float.value == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                        return new MarbleFloat(left_float.value / right_float.value);
                };
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData IntegerDivideOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        if (right_integer.value == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                        return new MarbleInteger(left_integer.value / right_integer.value);
                    case MarbleFloat right_float:
                        if (right_float.value == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                        return new MarbleInteger(left_integer.value / (int) right_float.value);
                    case MarbleString right_string: {
                        List<MarbleData> sub_strings = new List<MarbleData>();
                        string sub_string = "";
                        int sub_lenght = right_string.value.Length / left_integer.value;
                        for (int x = 0; x < right_string.value.Length; x++) {
                            if (x % sub_lenght == 0) {
                                sub_strings.Add(new MarbleString(sub_string));
                                sub_string = "";
                            }
                            sub_string += right_string.value[x];
                        }
                        return new MarbleList(sub_strings);
                    }
                    case MarbleList right_list: {
                        List<MarbleData> sub_lists = new List<MarbleData>();
                        List<MarbleData> sub_list = new List<MarbleData>();
                        int sub_lenght = right_list.value.Count / left_integer.value;
                        for (int x = 0; x < right_list.value.Count; x++) {
                            if (x % sub_lenght == 0) {
                                sub_lists.Add(new MarbleList(sub_list));
                                sub_list = new List<MarbleData>();
                            }
                            sub_list.Add(right_list.value[x]);
                        }
                        return new MarbleList(sub_lists);
                    }
                };
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleInteger right_integer:
                        if (right_integer.value == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                        return new MarbleInteger((int) left_float.value / right_integer.value);
                    case MarbleFloat right_float:
                        if (right_float.value == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                        return new MarbleInteger( (int) left_float.value / (int) right_float.value);
                    case MarbleString right_string: {
                        List<MarbleData> sub_strings = new List<MarbleData>();
                        string sub_string = "";
                        int sub_lenght = right_string.value.Length / (int) left_float.value;
                        for (int x = 0; x < right_string.value.Length; x++) {
                            if (x % sub_lenght == 0) {
                                sub_strings.Add(new MarbleString(sub_string));
                                sub_string = "";
                            }
                            sub_string += right_string.value[x];
                        }
                        return new MarbleList(sub_strings);
                    }
                    case MarbleList right_list: {
                        List<MarbleData> sub_lists = new List<MarbleData>();
                        List<MarbleData> sub_list = new List<MarbleData>();
                        int sub_lenght = right_list.value.Count / (int) left_float.value;
                        for (int x = 0; x < right_list.value.Count; x++) {
                            if (x % sub_lenght == 0) {
                                sub_lists.Add(new MarbleList(sub_list));
                                sub_list = new List<MarbleData>();
                            }
                            sub_list.Add(right_list.value[x]);
                        }
                        return new MarbleList(sub_lists);
                    }
                };
                break;
            case MarbleString left_string:
                switch (right) {
                    case MarbleInteger: case MarbleFloat: {
                        List<MarbleData> sub_strings = new List<MarbleData>();
                        string sub_string = "";
                        int sub_lenght = left_string.value.Length / (int) right.get_value();
                        for (int x = 0; x < left_string.value.Length; x++) {
                            if (x % sub_lenght == 0) {
                                sub_strings.Add(new MarbleString(sub_string));
                                sub_string = "";
                            }
                            sub_string += left_string.value[x];
                        }
                        return new MarbleList(sub_strings);
                    }
                };
                break;
            case MarbleList left_list:
                switch (right) {
                    case MarbleInteger: case MarbleFloat: {
                        List<MarbleData> sub_lists = new List<MarbleData>();
                        List<MarbleData> sub_list = new List<MarbleData>();
                        int sub_lenght = left_list.value.Count / (int) right.get_value();
                        for (int x = 0; x < left_list.value.Count; x++) {
                            if (x % sub_lenght == 0) {
                                sub_lists.Add(new MarbleList(sub_list));
                                sub_list = new List<MarbleData>();
                            }
                            sub_list.Add(left_list.value[x]);
                        }
                        return new MarbleList(sub_lists);
                    }
                };
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleFloat ExponentOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean left_boolean:
                switch (right) {
                    case MarbleBoolean right_boolean: 
                        return new MarbleFloat((float) Math.Pow(left_boolean.value? 1 : 0, right_boolean.value? 1 : 0));
                    case MarbleInteger: case MarbleFloat:
                        return new MarbleFloat((float) Math.Pow(left_boolean.value? 1 : 0, (float) right.get_value()));
                }
                break;
            case MarbleInteger: case MarbleFloat:
                switch (right) {
                    case MarbleBoolean right_boolean: 
                        return new MarbleFloat((float) Math.Pow((float) left.get_value(), right_boolean.value? 1 : 0));
                    case MarbleInteger: case MarbleFloat:
                        return new MarbleFloat((float) Math.Pow((float) left.get_value(), (float) right.get_value()));
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData ModolusOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleInteger(left_integer.value % right_integer.value);
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleFloat right_float:
                        return new MarbleFloat(left_float.value % right_float.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private async Task<MarbleBoolean> AndOperation(MarbleData left, MarbleData right, TokenPosition position) {
        return new MarbleBoolean((bool) (await ConvertOperation(MarbleData.TYPE.BOOLEAN, left, position)).get_value() && (bool) (await ConvertOperation(MarbleData.TYPE.BOOLEAN, right, position)).get_value());
    }
    private MarbleData BitwiseAndOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean left_boolean: 
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleBoolean(left_boolean.value & right_boolean.value);
                    case MarbleInteger right_integer:
                        return new MarbleInteger((left_boolean.value? 1 : 0) & right_integer.value);
                }
                break;
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleInteger(left_integer.value & (right_boolean.value? 1 : 0));
                    case MarbleInteger right_integer:
                        return new MarbleInteger(left_integer.value & right_integer.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private async Task<MarbleBoolean> OrOperation(MarbleData left, MarbleData right, TokenPosition position) {
        return new MarbleBoolean((bool) (await ConvertOperation(MarbleData.TYPE.BOOLEAN, left, position)).get_value() || (bool) (await ConvertOperation(MarbleData.TYPE.BOOLEAN, right, position)).get_value());
    }
    private MarbleData BitwiseOrOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean left_boolean: 
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleBoolean(left_boolean.value | right_boolean.value);
                    case MarbleInteger right_integer:
                        return new MarbleInteger((left_boolean.value? 1 : 0) | right_integer.value);
                }
                break;
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleInteger(left_integer.value | (right_boolean.value? 1 : 0));
                    case MarbleInteger right_integer:
                        return new MarbleInteger(left_integer.value | right_integer.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleBoolean EqualsOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean left_boolean:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleBoolean(left_boolean.value == right_boolean.value);
                }
                break;
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_integer.value == right_integer.value);
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_float.value == right_float.value);
                }
                break;
            case MarbleString left_string:
                switch (right) {
                    case MarbleString right_string:
                        return new MarbleBoolean(left_string.value == right_string.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleBoolean NotEqualsOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean left_boolean:
                switch (right) {
                    case MarbleBoolean right_boolean:
                        return new MarbleBoolean(left_boolean.value != right_boolean.value);
                }
                break;
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_integer.value != right_integer.value);
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_float.value != right_float.value);
                }
                break;
            case MarbleString left_string:
                switch (right) {
                    case MarbleString right_string:
                        return new MarbleBoolean(left_string.value != right_string.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleBoolean LesserThanOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_integer.value < right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_integer.value < right_float.value);
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_float.value < right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_float.value < right_float.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData LesserThanOrEqualsOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_integer.value <= right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_integer.value <= right_float.value);
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_float.value <= right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_float.value <= right_float.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData GreaterThanOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_integer.value > right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_integer.value > right_float.value);
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_float.value > right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_float.value > right_float.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleData GreaterThanOrEqualsOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleInteger left_integer:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_integer.value >= right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_integer.value >= right_float.value);
                }
                break;
            case MarbleFloat left_float:
                switch (right) {
                    case MarbleInteger right_integer:
                        return new MarbleBoolean(left_float.value >= right_integer.value);
                    case MarbleFloat right_float:
                        return new MarbleBoolean(left_float.value >= right_float.value);
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleBoolean InOperation(MarbleData left, MarbleData right, TokenPosition position) {
        switch (left) {
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                switch (right) {
                    case MarbleList list:
                        return new MarbleBoolean(list.value.Any((MarbleData element) => element.get_value() == left.get_value()));}
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    private MarbleBoolean IsOperation(MarbleData left, MarbleData right) {
        return new MarbleBoolean(left.GetType() == right.GetType());
    }
    private async Task<InterpreterOutput> InterpreteBinaryOperatorNode(BinaryOperatorNode node, ContextualStorage main_context) {
        switch (node.Operator.Type) {
            case OperatorToken.OPERATOR.DOT: return await DotOperation(node, main_context);
            case OperatorToken.OPERATOR.ACCESSOR: return await AccessorOperation(node, main_context);
            case OperatorToken.OPERATOR.CALLER: return await CallerOperation(node, main_context);
            case OperatorToken.OPERATOR.ADD: return await AddOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.SUBTRACT: return SubtractOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.MULTIPLY: return MultiplyOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.DIVIDE: return DivideOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.INTEGER_DIVIDE: return IntegerDivideOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.EXPONENT: return ExponentOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.MODOLUS: return ModolusOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.AND: return await AndOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.BITWISE_AND: return BitwiseAndOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.OR: return await OrOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.BITWISE_OR: return BitwiseOrOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.IN: return InOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.IS: return IsOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position));
            case OperatorToken.OPERATOR.EQUALS: return EqualsOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.NOT_EQUALS: return NotEqualsOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.GREATER_THAN: return GreaterThanOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS: return GreaterThanOrEqualsOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.LESSER_THAN: return LesserThanOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS: return LesserThanOrEqualsOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position);
            case OperatorToken.OPERATOR.ASSIGN: return await AssignOperation(node.Left, (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), main_context); 
            case OperatorToken.OPERATOR.ADD_AND_ASSIGN: return await AssignOperation(node.Left, await AddOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position), main_context); 
            case OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN: return await AssignOperation(node.Left, SubtractOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position), main_context); 
            case OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN: return await AssignOperation(node.Left, MultiplyOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position), main_context); 
            case OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN: return await AssignOperation(node.Left, DivideOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position), main_context); 
            case OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN: return await AssignOperation(node.Left, ExponentOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position), main_context); 
            case OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN: return await AssignOperation(node.Left, ModolusOperation((await InterpreteNode(node.Left, main_context)).ToMarbleData(node.Left.Position), (await InterpreteNode(node.Right, main_context)).ToMarbleData(node.Right.Position), node.Position), main_context);
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<InterpreterOutput> InterpreteUnaryOperatorNode(UnaryOperatorNode node, ContextualStorage main_context) {
        switch (node.Operator) {
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.ADD:
                        return await InterpreteNode(node.Operand, main_context);
                    case OperatorToken.OPERATOR.NOT: {
                        MarbleData data = (await InterpreteNode(node.Operand, main_context)).ToMarbleData(node.Operand.Position);
                        if (data is not MarbleBoolean) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                        return new MarbleBoolean(!(bool) data.get_value());
                    }
                    case OperatorToken.OPERATOR.SUBTRACT: {
                        MarbleData data = (await InterpreteNode(node.Operand, main_context)).ToMarbleData(node.Operand.Position);
                        if (data is MarbleInteger) {
                            return new MarbleInteger(-(int) data.get_value());
                        } else if (data is MarbleFloat) {
                            return new MarbleFloat(-(float) data.get_value());
                        } else {
                            throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                        }
                    }
                }
                break;
            /*case KeywordToken keyword_token:
                throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                switch (keyword_token.Type) {
                    case KeywordToken.TYPE.MODIFIER: {
                        StorageEntity storage_data = (await InterpreteNode(node.Operand, main_context)).IsStorageEntity(node.Operand.Position);
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
                        if (main_context.HasVariable(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                        StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESSMODE.NONE, false, (StorageEntity.DATATYPE) (keyword_token.Keyword - 4), false, null);
                        main_context.CreateVariable(identifier.Data as string, storage_variable);
                        return storage_variable;
                    }
                    case KeywordToken.TYPE.DEFINITION:
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.FUNCTION: {
                                BinaryOperatorNode definition = node.Operand as BinaryOperatorNode;
                                BinaryOperatorNode function = definition.Left as BinaryOperatorNode;
                                UnaryOperatorNode identifier = function.Left as UnaryOperatorNode;
                                string name = (identifier.Operand as DataNode).Data as string;
                                if (main_context.HasFunction(name)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                                StorageEntity.DATATYPE datatype = identifier.Operator.is_keyword_type(KeywordToken.TYPE.DATATYPE, out KeywordToken type)? (StorageEntity.DATATYPE) (type.Keyword - 4) : StorageEntity.DATATYPE.USER_DEFINED;
                                StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESSMODE.NONE, false, datatype, (function.Right as DataNode).Data as List<Node>, definition.Right as OperandInstructionNode);
                                main_context.CreateFunction(name, storage_function);
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
                                        StorageEntity output = (await InterpreteNode(bin.Right, main_context)).IsStorageEntity(bin.Right.Position);
                                        if (output is not StorageClass) new InterpreterError(InterpreterError.TYPE.MESSAGE, bin.Right.Position);
                                        Class = (output as StorageClass).Storage.CreateChild();
                                        break;
                                    }
                                }
                                switch (keyword_token.Keyword) {
                                    case KeywordToken.KEYWORD.CLASS:
                                        string name = identifier.Data as string;
                                        if (main_context.HasClass(name)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                                        await InterpreteInstructionListNode((node.Operand as BinaryOperatorNode).Right as OperandInstructionNode, Class);
                                        StorageClass storage_class = new StorageClass(StorageEntity.ACCESSMODE.NONE, false, Class);
                                        main_context.CreateClass(name, storage_class);
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
                                main_context.CreateFunction("Constructor!", storage_function);
                                return storage_function;
                            }
                        }
                    break;
                }
                break;
            case DataToken: {
                throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                DataNode identifier = node.Operand as DataNode;
                if (main_context.HasVariable(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESSMODE.NONE, false, StorageEntity.DATATYPE.USER_DEFINED, false, null);
                main_context.CreateVariable(identifier.Data as string, storage_variable);
                return storage_variable;
            }
            default:
                break;*/
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Weird keyword token");
    }
    private async Task<InterpreterOutput> InterpreteDataNode(DataNode node, ContextualStorage main_context) {
        switch (node.Type) {
            case DataNode.TYPE.BOOLEAN:
                return new MarbleBoolean((bool) node.Data);
            case DataNode.TYPE.INTEGER:
                return new MarbleInteger((int) node.Data);
            case DataNode.TYPE.FLOAT:
                return new MarbleFloat((float) node.Data);
            case DataNode.TYPE.STRING:
                return new MarbleString((string) node.Data);
            case DataNode.TYPE.NULL:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case DataNode.TYPE.DICTIONARY: {
                Dictionary<string, MarbleData> dictionary = new Dictionary<string, MarbleData>();
                foreach (DataNode element in node.Data as List<Node>) {
                    Dictionary<string, object> key_value = element.Data as Dictionary<string, object>;
                    dictionary.Add((key_value["Token"] as DataToken).Data.ToString(), (await InterpreteNode(key_value["Node"] as Node, main_context)).ToMarbleData(element.Position));
                }
                return new MarbleDictionary(dictionary);
            }
            case DataNode.TYPE.LIST: {
                List<MarbleData> elements = new List<MarbleData>();
                foreach (Node element in node.Data as List<Node>) {
                    elements.Add((await InterpreteNode(element, main_context)).ToMarbleData(element.Position));
                }
                return new  MarbleList(elements);
            }
            case DataNode.TYPE.IDENTIFIER:
                if (!main_context.GetEntity(node.Data as string, out StorageEntity storage_entity)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, node.Position);
                return storage_entity;
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<FlowController> InterpreteFlowControlNode(FlowControlNode node, ContextualStorage main_context) {
        switch (node.Type) {
            case FlowController.TYPE.BREAKPOINT:
                return new FlowController(FlowController.TYPE.DONE);
                //throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case FlowController.TYPE.RETURN:
                if (!main_context.HasVariable("RETURN!")) throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, node.Position);
                StorageVariable storage_variable = main_context.GetVariable("RETURN!");
                if (storage_variable.Datatype == StorageEntity.DATATYPE.VOID) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "This function does not return");
                MarbleData data = (await InterpreteNode(node.Data, main_context)).ToMarbleData(node.Data.Position);
                switch (storage_variable.Datatype) {
                    case StorageEntity.DATATYPE.VARIANT:
                        storage_variable.Data = data.duplicate();
                        break;
                    case StorageEntity.DATATYPE.BOOLEAN:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.BOOLEAN, data, node.Data.Position);
                        break;
                    case StorageEntity.DATATYPE.INTEGER:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.INTEGER, data, node.Data.Position);
                        break;
                    case StorageEntity.DATATYPE.FLOAT:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.FLOAT, data, node.Data.Position);
                        break;
                    case StorageEntity.DATATYPE.STRING:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.STRING, data, node.Data.Position);
                        break;
                    case StorageEntity.DATATYPE.LIST:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.LIST, data, node.Data.Position);
                        break;
                    case StorageEntity.DATATYPE.DICTIONARY:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.DICTIONARY, data, node.Data.Position);
                        break;
                    case StorageEntity.DATATYPE.CALLABLE:
                        if (data is MarbleCallable) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Data.Position);
                        storage_variable.Data = data.duplicate();
                        break;
                    case StorageEntity.DATATYPE.USER_DEFINED:
                        if (data is not MarbleObject) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Data.Position);
                        if ((data as MarbleObject).value.Class_name != storage_variable.Class_name)  throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Data.Position);
                        storage_variable.Data = data.duplicate();
                        break;
                }
                return new FlowController.Return(storage_variable.Data);
            case FlowController.TYPE.BREAK:
                return new FlowController(FlowController.TYPE.BREAK);
            case FlowController.TYPE.CONTINUE:
                return new FlowController(FlowController.TYPE.CONTINUE);
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<FlowController> InterpreteWhileNode(WhileNode node, ContextualStorage main_context) {
        while ((await ConvertOperation(MarbleData.TYPE.BOOLEAN, (await InterpreteNode(node.Expression, main_context)).ToMarbleData(node.Expression.Position), node.Expression.Position) as MarbleBoolean).value) {
            FlowController breaker = await InterpreteInstructionListNode(node.Instructions, main_context);
            if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
            if (breaker.Type == FlowController.TYPE.BREAK) break;
            if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteForNode(ForNode node, ContextualStorage main_context) {
        ContextualStorage child_storage = main_context.CreateChild();
        if (child_storage.HasVariable(node.Iterator.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Iterator.Position);
        StorageVariable Iterator = new StorageVariable(StorageEntity.ACCESSMODE.NONE, StorageEntity.DATATYPE.VARIANT, false, false, null);
        child_storage.CreateVariable(node.Iterator.Data as string, Iterator);
        MarbleData Iteratable = (await InterpreteNode(node.Iteratable, child_storage)).ToMarbleData(node.Iteratable.Position);
        switch (Iteratable) {
            case MarbleBoolean: case MarbleFloat:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Iteratable.Position, "Can't loop with this");
            case MarbleInteger integer: {
                Iterator.Data = new MarbleInteger(0);
                for (int x = 0; x < integer.value; x++) {
                    Iterator.Data.set_value(x);
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleString marble_string: {
                Iterator.Data = new MarbleString("");
                foreach (char item in marble_string.value) {
                    Iterator.Data.set_value(item + "");
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleList list: {
                foreach (MarbleData element in list.value) {
                    Iterator.Data = element;
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleDictionary:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteIFNode(IFNode node, ContextualStorage main_context) {
        if ((await ConvertOperation(MarbleData.TYPE.BOOLEAN, (await InterpreteNode(node.Expression, main_context)).ToMarbleData(node.Expression.Position), node.Expression.Position) as MarbleBoolean).value) {
            return await InterpreteInstructionListNode(node.Implication, main_context);
        } else {
            bool do_else = true;
            foreach (IFNode.ElseIFNode child in node.Else_IF_nodes) {
                if ((await ConvertOperation(MarbleData.TYPE.BOOLEAN, (await InterpreteNode(child.Expression, main_context)).ToMarbleData(child.Expression.Position), child.Expression.Position) as MarbleBoolean).value) {
                    return await InterpreteInstructionListNode(child.Implication, main_context);
                }
            }
            if (do_else && node.Inverse != null) {
                return await InterpreteInstructionListNode(node.Inverse, main_context);
            }
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteVariableDefinitionNode(VariableDefinitionNode node, ContextualStorage main_context) {
        if (main_context.HasVariable(node.Identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Identifier.Position);
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
            data = (await InterpreteNode(node.Value, main_context)).ToMarbleData(node.Value.Position);
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
            if (data != null) {
                switch (storage_variable.Datatype) {
                    case StorageEntity.DATATYPE.VARIANT:
                        storage_variable.Data = data.duplicate();
                        break;
                    case StorageEntity.DATATYPE.BOOLEAN:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.BOOLEAN, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.INTEGER:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.INTEGER, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.FLOAT:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.FLOAT, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.STRING:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.STRING, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.LIST:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.LIST, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.DICTIONARY:
                        storage_variable.Data = await ConvertOperation(MarbleData.TYPE.DICTIONARY, data, node.Value.Position);
                        break;
                    case StorageEntity.DATATYPE.CALLABLE:
                        if (data is MarbleCallable) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
                        storage_variable.Data = data.duplicate();
                        break;
                    case StorageEntity.DATATYPE.USER_DEFINED:
                        if (data is not MarbleObject) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
                        if ((data as MarbleObject).value.Class_name != storage_variable.Class_name)  throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
                        storage_variable.Data = data.duplicate();
                        break;
                }
            }
        } else {
            storage_variable.Datatype = StorageEntity.DATATYPE.USER_DEFINED;
            StorageClass storage_class = (await InterpreteNode(node.Datatype, main_context)).IsStorageClass(node.Datatype.Position);
            storage_variable.Class_name = storage_class.Class_name;
            if (data != null) {
                if (data is not MarbleObject) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
                if ((data.get_value() as StorageClass).Class_name != storage_class.Class_name) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Value.Position);
                storage_variable.Data = data;
            }
        }
        main_context.CreateVariable(node.Identifier.Data as string, storage_variable);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteClassDefinitionNode(ClassDefinitionNode node, ContextualStorage main_context) {
        if (main_context.HasClass(node.Identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Identifier.Position);
        ContextualStorage new_class;
        if (node.Datatype == null) {
            new_class = new ContextualStorage();
        } else {
            if (node.Datatype is KeywordNode) throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Identifier.Position);
            StorageClass parent_class = (await InterpreteNode(node.Datatype, main_context)).IsStorageClass(node.Datatype.Position);
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
        new_class.CreateClass(node.Identifier.Data as string, storage_class);
        await InterpreteInstructionListNode(node.Definition, storage_class.Storage);
        main_context.CreateClass(node.Identifier.Data as string, storage_class);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteFunctionDefinitionNode(FunctionDefinitionNode node, ContextualStorage main_context) {
        if (main_context.HasFunction(node.Identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Identifier.Position);
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
                case KeywordToken.KEYWORD.CALLABLE:
                    storage_function.Datatype = StorageEntity.DATATYPE.CALLABLE;
                    break;
            }
        } else {
            storage_function.Datatype = StorageEntity.DATATYPE.USER_DEFINED;
            StorageClass storage_class = (await InterpreteNode(node.Datatype, main_context)).IsStorageClass(node.Datatype.Position);
            storage_function.Class_name = storage_class.Class_name;
        }
        main_context.CreateFunction(node.Identifier.Data as string, storage_function);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteConstructorNode(ConstructorNode node, ContextualStorage main_context) {
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
        main_context.CreateConstructor("1", storage_constructor);
        return await Task.Run(() => new FlowController(FlowController.TYPE.DONE));
    }
}