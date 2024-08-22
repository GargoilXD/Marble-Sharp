using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Interpreter {
    public static InputGetter Input_dialog;
    public string Output = "";
    public async Task<InterpreterOutput> Interprete(List<Node> nodes, ContextualStorage storage) {
        foreach (Node node in nodes) await InterpreteNode(node, storage);
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<InterpreterOutput> InterpreteNode(Node node, ContextualStorage storage) => node switch {
        BinaryOperatorNode switch_node => await InterpreteBinaryOperatorNode(switch_node, storage),
        UnaryOperatorNode switch_node => await InterpreteUnaryOperatorNode(switch_node, storage),
        InlineIFNode switch_node => await InterpreteInlineIFNode(switch_node, storage),
        DataNode switch_node => await InterpreteDataNode(switch_node, storage),

        FlowControlNode switch_node => await InterpreteFlowControlNode(switch_node, storage),
        WhileNode switch_node => await InterpreteWhileNode(switch_node, storage),
        ForNode switch_node => await InterpreteForNode(switch_node, storage),
        IFNode switch_node => await InterpreteIFNode(switch_node, storage),
        _ => throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position, "new node?")
    };
    
    private async Task<FlowController> InterpreteInstructionListNode(InstructionListNode node, ContextualStorage storage) {
        foreach (Node statement in node.Instructions) {
            InterpreterOutput result = await InterpreteNode(statement, storage);
            if (result is FlowController && (result as FlowController).Type != FlowController.TYPE.DONE) return result as FlowController;
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<InterpreterOutput> InterpreteBinaryOperatorNode(BinaryOperatorNode node, ContextualStorage storage) {
        switch ((node.Operator as OperatorToken).Type) {
            case OperatorToken.OPERATOR.DOT: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                switch (Left) {
                    case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleDictionary:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                    case MarbleObject mobject: {
                        switch (node.Right) {
                            case BinaryOperatorNode sfunction: {
                                List<Node> Arguments = (sfunction.Right as DataNode).Data as List<Node>;
                                if (!mobject.Class.Storage.HasFunction((sfunction.Left as DataNode).Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_FUNCTION, sfunction.Left.Position);
                                StorageFunction function = mobject.Class.Storage.GetFunction((sfunction.Left as DataNode).Data as string);
                                if (function.AccessMode == StorageEntity.ACCESS_MODE.PRIVATE) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_FUNCTION, sfunction.Position);
                                ContextualStorage child_storage = mobject.Class.Storage.CreateChild();
                                if (Arguments.Count != function.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Different number of parameters");
                                for (int x = 0; x < function.Arguments.Count; x++) {
                                    await assign_operation(function.Arguments[x], GetStorageVariable(await InterpreteNode(Arguments[x], storage), node.Right.Position), child_storage);
                                }
                                child_storage.CreateVariable("RETURN!", new StorageVariable(StorageEntity.ACCESS_MODE.PRIVATE, false, function.Datatype, false, null));
                                FlowController output = await InterpreteInstructionListNode(function.Instructions, child_storage);
                                switch (output.Type) {
                                    case FlowController.TYPE.CONTINUE: case FlowController.TYPE.BREAK:
                                        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Unexpected Keyword");
                                    case FlowController.TYPE.RETURN:
                                        return output.Data;
                                    case FlowController.TYPE.DONE:
                                        if (function.Datatype != StorageEntity.DATATYPE.VOID) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "No return statement");
                                        return new FlowController(FlowController.TYPE.DONE);
                                }
                                
                                break;
                            }
                            case DataNode data: {
                                if (!mobject.Class.Storage.GetVariable(data.Data as string, out StorageVariable variable)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, data.Position);
                                if (variable.AccessMode == StorageEntity.ACCESS_MODE.PRIVATE) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_VARIABLE, data.Position);
                                return variable;
                            }

                        }
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
                    }
                    case MarbleList data: {
                        if (node.Right is not BinaryOperatorNode) throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Right.Position);
                        BinaryOperatorNode function = node.Right as BinaryOperatorNode;
                        List<Node> Arguments = (function.Right as DataNode).Data as List<Node>;
                        switch ((function.Left as DataNode).Data as string) {
                            case "append":
                                if (Arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                MarbleData new_element = GetStorageVariable(await InterpreteNode(Arguments[0], storage), Arguments[0].Position);
                                data.Elements.Add(new_element);
                                break;
                            default:
                                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
                        }
                        break;
                    }
                }
                return Left;
            }
            case OperatorToken.OPERATOR.ACCESSOR: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleList Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position) as MarbleList;
                switch (Left) {
                    case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleDictionary:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                    case MarbleList data:
                        if (Right.Elements.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                        MarbleData index = Right.Elements[0];
                        if (index is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Right.Position);
                        int value = (int) index.get_data();
                        if (value >= data.Elements.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Out of range");
                        return data.Elements[value];
                }
                return Left;
            }
            case OperatorToken.OPERATOR.CALLER:
                switch (node.Left) {
                    case DataNode data_node:
                        List<Node> Arguments = (node.Right as DataNode).Data as List<Node>;
                        switch (data_node.Type) {
                            case DataNode.TYPE.DATATYPE:
                                switch(data_node.Data) {
                                    case KeywordToken.KEYWORD:
                                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                                    default:
                                        if (!storage.HasClass(data_node.Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, data_node.Position);
                                        return new MarbleObject(storage.GetClass(data_node.Data as string));

                                }
                            case DataNode.TYPE.IDENTIFIER:
                                if (!storage.HasFunction(data_node.Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, data_node.Position);
                                ContextualStorage child_storage = storage.CreateChild();
                                StorageFunction function = storage.GetFunction(data_node.Data as string);
                                if (Arguments.Count != function.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Different number of parameters");
                                for (int x = 0; x < function.Arguments.Count; x++) {
                                    await assign_operation(function.Arguments[x], GetStorageVariable(await InterpreteNode(Arguments[x], storage), node.Right.Position), child_storage);
                                }
                                child_storage.CreateVariable("RETURN!", new StorageVariable(StorageEntity.ACCESS_MODE.PRIVATE, false, function.Datatype, false, null));
                                FlowController output = await InterpreteInstructionListNode(function.Instructions, child_storage);
                                switch (output.Type) {
                                    case FlowController.TYPE.CONTINUE: case FlowController.TYPE.BREAK:
                                        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Unexpected Keyword");
                                    case FlowController.TYPE.RETURN:
                                        return output.Data;
                                    case FlowController.TYPE.DONE:
                                        if (function.Datatype != StorageEntity.DATATYPE.VARIANT) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, data_node.Position, "No return statement");
                                        return null;
                                }
                                break;
                            case DataNode.TYPE.INBUILT_FUNCTION:
                                switch (data_node.Data) {
                                    case KeywordToken.KEYWORD.ASSERT:
                                        if (Arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                                        MarbleData assertion = GetStorageVariable(await InterpreteNode(Arguments[0], storage), Arguments[0].Position);
                                        if (assertion is not MarbleBoolean) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Expected boolean");
                                        if (!(assertion as MarbleBoolean).Value) throw new InterpreterError(InterpreterError.TYPE.ASSERTION_FAILED, node.Right.Position);
                                        return new FlowController(FlowController.TYPE.DONE);
                                    case KeywordToken.KEYWORD.PRINT: {
                                        foreach (Node argument in Arguments) Output += $"{GetStorageVariable(await InterpreteNode(argument, storage), argument.Position)} ";
                                        Output += '\n';
                                        return new FlowController(FlowController.TYPE.DONE);
                                    }
                                    case KeywordToken.KEYWORD.RANGE: {
                                        if (Arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                                        MarbleData data = GetStorageVariable(await InterpreteNode(Arguments[0], storage), Arguments[0].Position);
                                        if (data is not MarbleInteger)  throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Position);
                                        List<MarbleData> elements = new List<MarbleData>();
                                        for (int x = 0; x < (data as MarbleInteger).Value; x++) {
                                            elements.Add(new MarbleInteger(x));
                                        }
                                        return new MarbleList(elements);
                                    }
                                    case KeywordToken.KEYWORD.RANDOM: {
                                        if (Arguments.Count > 2) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                                        MarbleData left = GetStorageVariable(await InterpreteNode(Arguments[0], storage), Arguments[0].Position);
                                        if (left is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, Arguments[0].Position);
                                        MarbleData right = GetStorageVariable(await InterpreteNode(Arguments[1], storage), Arguments[1].Position);
                                        if (right is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, Arguments[1].Position);
                                        int random_number = new Random().Next((int) left.get_data(), (int) right.get_data());
                                        return new MarbleInteger(random_number);
                                    }
                                    case KeywordToken.KEYWORD.INPUT: {
                                        if (Arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                                        Input_dialog.DialogText = $"{GetStorageVariable(await InterpreteNode(Arguments[0], storage), Arguments[0].Position)}";
                                        Input_dialog.Show();
                                        await Input_dialog.ToSignal(Input_dialog, "confirmed");
                                        return new MarbleString(Input_dialog.Input);
                                    }
                                }
                                break;
                        }
                        break;
                    case BinaryOperatorNode:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
            }
            return null;
            case OperatorToken.OPERATOR.DEFINE:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Left.Position, "def");
            case OperatorToken.OPERATOR.ADD: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.add(Right, node.Position);
            }
            case OperatorToken.OPERATOR.SUBTRACT: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.subtract(Right, node.Position);
            }
            case OperatorToken.OPERATOR.MULTIPLY: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.multiply(Right, node.Position);
            }
            case OperatorToken.OPERATOR.DIVIDE: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.divide(Right, node.Position);
            }
            case OperatorToken.OPERATOR.INTEGER_DIVIDE: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.integer_divide(Right, node.Position);
            }
            case OperatorToken.OPERATOR.EXPONENT: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.exponent(Right, node.Position);
            }
            case OperatorToken.OPERATOR.MODOLUS: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.modolus(Right, node.Position);
            }
            case OperatorToken.OPERATOR.AND: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.and(Right, node.Position);
            }
            case OperatorToken.OPERATOR.BITWISE_AND: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.bitwise_and(Right, node.Position);
            }
            case OperatorToken.OPERATOR.OR: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.or(Right, node.Position);
            }
            case OperatorToken.OPERATOR.BITWISE_OR: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.bitwise_or(Right, node.Position);
            }
            case OperatorToken.OPERATOR.IN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.contains(Right, node.Position);
            }
            case OperatorToken.OPERATOR.IS: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.is_is(Right);
            }
            case OperatorToken.OPERATOR.COLON: {
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "What?");
            }
            case OperatorToken.OPERATOR.EQUALS: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.NOT_EQUALS: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.not_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.GREATER_THAN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.greater_than(Right, node.Position);
            }
            case OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.greater_than_or_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.LESSER_THAN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.lesser_than(Right, node.Position);
            }
            case OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return Left.lesser_than_or_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.ASSIGN: {
                return await assign_operation(node.Left, GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position), storage);
            }
            case OperatorToken.OPERATOR.ADD_AND_ASSIGN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return await assign_operation(node.Left, Left.add(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return await assign_operation(node.Left, Left.subtract(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return await assign_operation(node.Left, Left.multiply(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return await assign_operation(node.Left, Left.divide(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return await assign_operation(node.Left, Left.exponent(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN: {
                MarbleData Left = GetStorageVariable(await InterpreteNode(node.Left, storage), node.Left.Position);
                MarbleData Right = GetStorageVariable(await InterpreteNode(node.Right, storage), node.Right.Position);
                return await assign_operation(node.Left, Left.modolus(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.EXTENDS: {
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            }
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
                        MarbleData data = (await InterpreteNode(node.Operand, storage)).IsMarbleData(node.Operand.Position);
                        return data.negate(node.Operand.Position);
                    }
                    case OperatorToken.OPERATOR.SUBTRACT: {
                        MarbleData data = (await InterpreteNode(node.Operand, storage)).IsMarbleData(node.Operand.Position);
                        if (data is not MarbleInteger && data is not MarbleFloat) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                        return data.negate(node.Operand.Position);
                    }
                }
                break;
            case KeywordToken keyword_token:
                switch (keyword_token.Type) {
                    case KeywordToken.TYPE.MODIFIER: {
                        StorageEntity storage_data = await InterpreteNode(node.Operand, storage) as StorageEntity;
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.CONSTANT:
                                if (storage_data is not StorageVariable) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Only for variables");
                                StorageVariable storage_variable = storage_data as StorageVariable;
                                if (storage_variable.IsConstant) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Repeat");
                                storage_variable.IsConstant = true;
                                break;
                            case KeywordToken.KEYWORD.PUBLIC:
                                if (storage_data.AccessMode != StorageEntity.ACCESS_MODE.NONE) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Repeat");
                                storage_data.AccessMode = StorageEntity.ACCESS_MODE.PUBLIC;
                                break;
                            case KeywordToken.KEYWORD.PRIVATE:
                                if (storage_data.AccessMode != StorageEntity.ACCESS_MODE.NONE) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Repeat");
                                storage_data.AccessMode = StorageEntity.ACCESS_MODE.PRIVATE;
                                break;
                            case KeywordToken.KEYWORD.STATIC:
                                if (storage_data.IsStatic) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Operator.Position, "Repeat");
                                storage_data.IsStatic = true;
                                break;
                        }
                        return storage_data;
                    }
                    case KeywordToken.TYPE.DATATYPE: {
                        switch (node.Operand) {
                            case DataNode name: {
                                if (storage.HasVariable(name.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                                StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESS_MODE.NONE, false, (StorageEntity.DATATYPE) (keyword_token.Keyword - 4), false, null);
                                storage.CreateVariable(name.Data as string, storage_variable);
                                return storage_variable;
                            }
                            case BinaryOperatorNode function: {
                                BinaryOperatorNode identifier = function.Left as BinaryOperatorNode;
                                string name = (identifier.Left as DataNode).Data as string;
                                if (storage.HasFunction(name)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                                StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESS_MODE.NONE, false, (StorageEntity.DATATYPE) (keyword_token.Keyword - 4), (identifier.Right as DataNode).Data as List<Node>, function.Right as InstructionListNode);
                                storage.CreateFunction(name, storage_function);
                                return storage_function;
                            }
                        }
                        break;
                    }
                    case KeywordToken.TYPE.DEFINITION: {
                        DataNode parent = null;
                        DataNode identifier = null;
                        switch ((node.Operand as BinaryOperatorNode).Left) {
                            case DataNode data: {
                                identifier = data;
                                break;
                            }
                            case BinaryOperatorNode bin: {
                                identifier = bin.Left as DataNode;
                                parent = bin.Right as DataNode;
                                if (!storage.HasClass(parent.Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, parent.Position);
                                break;
                            }
                        }
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.CLASS:
                                string name = identifier.Data as string;
                                if (storage.HasClass(name)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                                ContextualStorage Class = new ContextualStorage();
                                await InterpreteInstructionListNode((node.Operand as BinaryOperatorNode).Right as InstructionListNode, Class);
                                StorageClass storage_class = new StorageClass(StorageEntity.ACCESS_MODE.NONE, false, Class);
                                storage.CreateClass(name, storage_class);
                                return storage_class;
                            case KeywordToken.KEYWORD.STRUCTURE:
                            case KeywordToken.KEYWORD.ENUMERATION:
                                break;
                        }
                        break;
                    }
                }
                break;
            case DataToken:
                switch (node.Operand) {
                    case DataNode name: {
                        if (storage.HasVariable(name.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                        StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESS_MODE.NONE, false, StorageEntity.DATATYPE.USER_DEFINED, false, null);
                        storage.CreateVariable(name.Data as string, storage_variable);
                        return storage_variable;
                    }
                    case BinaryOperatorNode function: {
                        BinaryOperatorNode identifier = function.Left as BinaryOperatorNode;
                        string name = (identifier.Left as DataNode).Data as string;
                        if (storage.HasFunction(name)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                        StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESS_MODE.NONE, false, StorageEntity.DATATYPE.USER_DEFINED, (identifier.Right as DataNode).Data as List<Node>, function.Right as InstructionListNode);
                        storage.CreateFunction(name, storage_function);
                        return storage_function;
                    }
                }
                break;
            default:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Weird keyword token");
    }
    /*
    private async Task<InterpreterOutput> InterpreteFunctionNode(FunctionNode node, ContextualStorage storage) {
        switch (node.Identifier) {
            case KeywordToken keyword_token:
                switch (keyword_token.Keyword) {
                    case KeywordToken.KEYWORD.INPUT: {
                        if (node.Arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
						Input_dialog.DialogText = $"{(await InterpreteNode(node.Arguments[0], storage)).IsMarbleData(node.Arguments[0].Position)}";
						Input_dialog.Show();
                        await Input_dialog.ToSignal(Input_dialog, "confirmed");
						return new MarbleString(Input_dialog.Input);
                    }
                    case KeywordToken.KEYWORD.PRINT: {
                        foreach (Node argument in node.Arguments){
							Output += $"{(await InterpreteNode(argument, storage)).IsMarbleData(argument.Position)} ";
                        }
						Output += '\n';
						return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.RANGE: {
                        if (node.Arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
						MarbleData data = (await InterpreteNode(node.Arguments[0], storage)).IsMarbleData(node.Arguments[0].Position);
                        if (data is not MarbleInteger)  throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Position);
						List<MarbleData> elements = new List<MarbleData>();
                        for (int x = 0; x < (data as MarbleInteger).Value; x++) {
                            elements.Add(new MarbleInteger(x));
                        }
						return new MarbleList(elements);
                    }
                    case KeywordToken.KEYWORD.RANDOM: {
                        if (node.Arguments.Count > 2) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        MarbleData left = (await InterpreteNode(node.Arguments[0], storage)).IsMarbleData(node.Arguments[0].Position);
                        if (left is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Arguments[0].Position);
                        MarbleData right = (await InterpreteNode(node.Arguments[1], storage)).IsMarbleData(node.Arguments[1].Position);
                        if (right is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Arguments[1].Position);
                        int random_number = new Random().Next((int) left.get_data(), (int) right.get_data());
                        return new MarbleInteger(random_number);
                    }
                    case KeywordToken.KEYWORD.ASSERT:
                        break;
                }
                break;
            case DataToken identifier:
                if (!storage.HasFunction(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, identifier.Position);
                ContextualStorage child_storage = storage.CreateChild();
                StorageFunction function = storage.GetFunction(identifier.Data as string);
                if (node.Arguments.Count != function.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Different number of parameters");
                for (int x = 0; x < function.Arguments.Count; x++) {
                    VariableDefinitionNode variable_definition_node = function.Arguments[x] as VariableDefinitionNode;
                    variable_definition_node.Value = node.Arguments[x];
                    await InterpreteVariableDefinitionNode(variable_definition_node, child_storage);
                }
                child_storage.CreateVariable("RETURN!", new StorageVariable(StorageEntity.ACCESS_MODE.PRIVATE, false, function.Datatype, false, null));
                FlowController output = await InterpreteInstructionListNode(function.Instructions, child_storage);
                switch (output.Type) {
                    case FlowController.TYPE.CONTINUE: case FlowController.TYPE.BREAK:
                        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Unexpected Keyword");
                    case FlowController.TYPE.RETURN:
                        return output.Data;
                    case FlowController.TYPE.DONE:
                        if (function.Datatype != StorageEntity.DATATYPE.VARIANT) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, identifier.Position, "No return statement");
                        return null;
                }
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    */
    private async Task<InterpreterOutput> InterpreteDataNode(DataNode node, ContextualStorage storage) {
        switch (node.Type) {
            case DataNode.TYPE.BOOLEAN: case DataNode.TYPE.INTEGER: case DataNode.TYPE.FLOAT: case DataNode.TYPE.STRING:
                return MarbleData.FromDataNode(node);
            case DataNode.TYPE.NULL: case DataNode.TYPE.DATATYPE:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case DataNode.TYPE.DICTIONARY: {
                Dictionary<object, MarbleData> dictionary = new Dictionary<object, MarbleData>();
                foreach (DataNode element in node.Data as List<Node>) {
                    Dictionary<string, object> key_value = element.Data as Dictionary<string, object>;
                    dictionary.Add((key_value["Token"] as DataToken).Data, GetStorageVariable(await InterpreteNode(key_value["Node"] as Node, storage), element.Position));
                }
                return new MarbleDictionary(dictionary);
            }
            case DataNode.TYPE.LIST: {
                List<MarbleData> elements = new List<MarbleData>();
                foreach (Node element in node.Data as List<Node>) {
                    elements.Add(GetStorageVariable(await InterpreteNode(element, storage), element.Position));
                }
                return new MarbleList(elements);
            }
            case DataNode.TYPE.IDENTIFIER:
                if (!storage.GetVariable(node.Data as string, out StorageVariable storage_variable)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, node.Position);
                return storage_variable;
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<FlowController> InterpreteFlowControlNode(FlowControlNode node, ContextualStorage storage) {
        switch (node.Type) {
            case FlowController.TYPE.BREAKPOINT:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case FlowController.TYPE.RETURN:
                MarbleData data = GetStorageVariable(await InterpreteNode(node.Data, storage), node.Data.Position);
                if (!storage.GetVariable("RETURN!", out StorageVariable storage_variable)) throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, node.Position);
                switch (storage_variable.Datatype) {
                    case StorageEntity.DATATYPE.VARIANT:
                        storage_variable.Data = data;
                        break;
                    case StorageEntity.DATATYPE.BOOLEAN:
                        storage_variable.Data = MarbleBoolean.Convert(data, node.Position);
                        break;
                    case StorageEntity.DATATYPE.INTEGER:
                        storage_variable.Data = MarbleInteger.Convert(data, node.Position);
                        break;
                    case StorageEntity.DATATYPE.FLOAT:
                        storage_variable.Data = MarbleFloat.Convert(data, node.Position);
                        break;
                    case StorageEntity.DATATYPE.STRING:
                        storage_variable.Data = MarbleString.Convert(data);
                        break;
                    case StorageEntity.DATATYPE.LIST:
                        storage_variable.Data = MarbleList.Convert(data, node.Position);
                        break;
                    case StorageEntity.DATATYPE.DICTIONARY:
                        storage_variable.Data = MarbleDictionary.Convert(data, node.Position);
                        break;
                    case StorageEntity.DATATYPE.USER_DEFINED:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
                }
                return new FlowController(FlowController.TYPE.RETURN, storage_variable.Data);
            case FlowController.TYPE.BREAK:
                return new FlowController(FlowController.TYPE.BREAK);
            case FlowController.TYPE.CONTINUE:
                return new FlowController(FlowController.TYPE.CONTINUE);
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<FlowController> InterpreteWhileNode(WhileNode node, ContextualStorage storage) {
        while (MarbleBoolean.Convert((await InterpreteNode(node.Expression, storage)).IsMarbleData(node.Expression.Position), node.Expression.Position).Value) {
            FlowController breaker = await InterpreteInstructionListNode(node.Instructions, storage);
            if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
            if (breaker.Type == FlowController.TYPE.BREAK) break;
            if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<FlowController> InterpreteForNode(ForNode node, ContextualStorage storage) {
        ContextualStorage child_storage = storage.CreateChild();
        InterpreterOutput output = await InterpreteNode(node.Iterator, child_storage);
        if (output is not StorageVariable) throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Iterator.Position);
        StorageVariable Iterator = output as StorageVariable;
        if (Iterator.Data == null) Iterator.Initialize();
        switch (GetStorageVariable(await InterpreteNode(node.Iteratable, child_storage), node.Iteratable.Position)) {
            case MarbleBoolean: case MarbleFloat:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Iteratable.Position, "Can't loop with this");
            case MarbleInteger integer: {
                if (Iterator.Data is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Iteratable.Position);
                int start = (int) Iterator.Data.get_data();
                for (int x = start; x < integer.Value; x++) {
                    Iterator.Data.set_data(x);
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleString data: {
                //MarbleData Iterator = assign_operation(DataNode.FromToken((node.Iterator as VariableDefinitionNode).Identifier), new MarbleString(""), child_storage);
                if (Iterator.Data is not MarbleString) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Iteratable.Position);
                foreach (char item in data.Value) {
                    Iterator.Data.set_data(item + "");
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleList list: {
                //MarbleData Iterator = assign_operation(DataNode.FromToken((node.Iterator as VariableDefinitionNode).Identifier), new MarbleList(new List<MarbleData>()), child_storage);
                foreach (MarbleData element in list.Elements) {
                    Iterator.Data.set_data(Iterator.Data.convert(element, node.Iterator.Position).get_data());
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
    private async Task<FlowController> InterpreteIFNode(IFNode node, ContextualStorage storage) {
        if (MarbleBoolean.Convert((await InterpreteNode(node.Expression, storage)).IsMarbleData(node.Expression.Position), node.Expression.Position).Value) {
            return await InterpreteInstructionListNode(node.Implication, storage);
        } else {
            bool do_else = true;
            foreach (IFNode.ElseIFNode child in node.Else_IF_nodes) {
                if (MarbleBoolean.Convert((await InterpreteNode(child.Expression, storage)).IsMarbleData(child.Expression.Position), child.Expression.Position).Value) {
                    return await InterpreteInstructionListNode(child.Implication, storage);
                }
            }
            if (do_else && node.Inverse != null) {
                return await InterpreteInstructionListNode(node.Inverse, storage);
            }
        }
        return new FlowController(FlowController.TYPE.DONE);
    }
    private async Task<MarbleData> InterpreteInlineIFNode(InlineIFNode node, ContextualStorage storage) {
        if (MarbleBoolean.Convert((await InterpreteNode(node.Expression, storage)).IsMarbleData(node.Expression.Position), node.Expression.Position).Value) {
            return (await InterpreteNode(node.Implication, storage)).IsMarbleData(node.Implication.Position);
        } else {
            bool do_else = true;
            foreach (InlineIFNode.InlineElseIFNode child in node.Children) {
                if (MarbleBoolean.Convert((await InterpreteNode(child.Expression, storage)).IsMarbleData(child.Expression.Position), child.Expression.Position).Value) {
                    return (await InterpreteNode(child.Implication, storage)).IsMarbleData(child.Implication.Position);
                }
            }
            if (do_else && node.Inverse != null) {
                return (await InterpreteNode(node.Inverse, storage)).IsMarbleData(node.Inverse.Position);
            }
        }
        return null;
    }
    private MarbleData GetStorageVariable(InterpreterOutput output, TokenPosition position) {
        if (output is StorageVariable) {
            if ((output as StorageVariable).Data == null) throw new InterpreterError(InterpreterError.TYPE.UNINITIALIZED_IDENTIFIER, position);
            return (output as StorageVariable).Data;
        } else if (output is MarbleData) {
            return output as MarbleData;
        } else {
            throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "else");
        }
    }
    private async Task<StorageVariable> assign_operation(Node left, MarbleData data, ContextualStorage storage) {
        InterpreterOutput output = await InterpreteNode(left, storage);
        if (output is not StorageVariable) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, left.Position);
        StorageVariable storage_variable = output as StorageVariable;
        if (storage_variable.IsConstant && storage_variable.Data != null) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, left.Position, "This variable is constant");
        switch (storage_variable.Datatype) {
            case StorageEntity.DATATYPE.VARIANT:
                storage_variable.Data = data;
                break;
            case StorageEntity.DATATYPE.BOOLEAN:
                storage_variable.Data = MarbleBoolean.Convert(data, left.Position);
                break;
            case StorageEntity.DATATYPE.INTEGER:
                storage_variable.Data = MarbleInteger.Convert(data, left.Position);
                break;
            case StorageEntity.DATATYPE.FLOAT:
                storage_variable.Data = MarbleFloat.Convert(data, left.Position);
                break;
            case StorageEntity.DATATYPE.STRING:
                storage_variable.Data = MarbleString.Convert(data);
                break;
            case StorageEntity.DATATYPE.LIST:
                storage_variable.Data = MarbleList.Convert(data, left.Position);
                break;
            case StorageEntity.DATATYPE.DICTIONARY:
                storage_variable.Data = MarbleDictionary.Convert(data, left.Position);
                break;
            case StorageEntity.DATATYPE.USER_DEFINED:
                if (data is not MarbleObject) throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, left.Position);
                storage_variable.Data = data;
                break;
        }
        return storage_variable;
    }
}