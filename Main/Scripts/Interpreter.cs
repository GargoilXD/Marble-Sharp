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
    private async Task<InterpreterOutput> RunFunction(StorageFunction function, List<Node> arguments, BinaryOperatorNode function_node, ContextualStorage storage, ContextualStorage child_storage) {
        if (arguments.Count != function.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function_node.Right.Position, "Different number of parameters");
        for (int x = 0; x < function.Arguments.Count; x++) {
            await AssignOperation(function.Arguments[x], (await InterpreteNode(arguments[x], storage)).ToMarbleData(arguments[x].Position), child_storage);
        }
        child_storage.CreateVariable("RETURN!", new StorageVariable(StorageEntity.ACCESS_MODE.PRIVATE, false, function.Datatype, false, null));
        FlowController output = await InterpreteInstructionListNode(function.Instructions, child_storage);
        switch (output.Type) {
            case FlowController.TYPE.RETURN:
                return output.Data;
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
            case StorageClass classs:
                switch (node.Right) {
                    case BinaryOperatorNode:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                    case DataNode identifier: {
                        if (!classs.Storage.GetEntity(identifier.Data as string, out StorageEntity storage_entity)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, identifier.Position);
                        if (storage_entity.AccessMode != StorageEntity.ACCESS_MODE.PUBLIC) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_VARIABLE, identifier.Position);
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
                        List<Node> arguments = (function.Right as DataNode).Data as List<Node>;
                        switch (data) {
                            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleDictionary:
                                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                            case MarbleList list: {
                                switch (identifier.Data as string) {
                                    case "append":
                                        if (arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        MarbleData new_element = (await InterpreteNode(arguments[0], storage)).ToMarbleData(arguments[0].Position);
                                        list.Elements.Add(new_element);
                                        break;
                                    case "has":
                                        if (arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        return list.contains((await InterpreteNode(arguments[0], storage)).ToMarbleData(arguments[0].Position), arguments[0].Position);
                                    case "clear":
                                        if (arguments.Count > 0) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, function.Position, "Too many parameters");
                                        list.Elements.Clear();
                                        return new FlowController(FlowController.TYPE.DONE);

                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
                                }
                                return null;
                            }
                            case MarbleObject marble_object: {
                                if (!marble_object.Class.Storage.HasFunction(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_FUNCTION, identifier.Position);
                                StorageFunction storage_function = marble_object.Class.Storage.GetFunction(identifier.Data as string);
                                if (storage_function.AccessMode != StorageEntity.ACCESS_MODE.PUBLIC) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_FUNCTION, function.Position);
                                return await RunFunction(storage_function, arguments, function, storage, marble_object.Class.Storage.CreateChild());
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
                                        return new MarbleInteger(marble_string.Value.Length);
                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, identifier.Position, "undefined");
                                }
                            case MarbleList list:
                                switch(identifier.Data) {
                                    case "size":
                                        return new MarbleInteger(list.Elements.Count);
                                    default:
                                        throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, identifier.Position, "undefined");
                                }
                            case MarbleObject marble_object: {
                                if (!marble_object.Class.Storage.GetEntity(identifier.Data as string, out StorageEntity storage_entity)) throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, identifier.Position);
                                if (storage_entity.AccessMode != StorageEntity.ACCESS_MODE.PUBLIC) throw new InterpreterError(InterpreterError.TYPE.ACCESSING_PRIVATE_VARIABLE, identifier.Position);
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
    private async Task<InterpreterOutput> CallerOperation(BinaryOperatorNode node, ContextualStorage storage) {
        List<Node> arguments = (node.Right as DataNode).Data as List<Node>;
        switch (node.Left) {
            case KeywordNode keyword_node: {
                switch (keyword_node.Keyword) {
                    case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.BOOLEAN: case KeywordToken.KEYWORD.INTEGER:
                    case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST: case KeywordToken.KEYWORD.DICTIONARY:
                        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
                    case KeywordToken.KEYWORD.ASSERT: {
                        if (arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                        MarbleData assertion = (await InterpreteNode(arguments[0], storage)).ToMarbleData(arguments[0].Position);
                        if (assertion is not MarbleBoolean) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Expected boolean");
                        if (!(assertion as MarbleBoolean).Value) throw new InterpreterError(InterpreterError.TYPE.ASSERTION_FAILED, node.Right.Position);
                        return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.PRINT: {
                        foreach (Node argument in arguments) Output += $"{(await InterpreteNode(argument, storage)).ToMarbleData(argument.Position)} ";
                        Output += '\n';
                        return new FlowController(FlowController.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.RANGE: {
                        if (arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        MarbleData data = (await InterpreteNode(arguments[0], storage)).ToMarbleData(arguments[0].Position);
                        if (data is not MarbleInteger)  throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Position);
                        List<MarbleData> elements = new List<MarbleData>();
                        for (int x = 0; x < (data as MarbleInteger).Value; x++) {
                            elements.Add(new MarbleInteger(x));
                        }
                        return new MarbleList(elements);
                    }
                    case KeywordToken.KEYWORD.RANDOM: {
                        if (arguments.Count > 2) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        MarbleData left = (await InterpreteNode(arguments[0], storage)).ToMarbleData(arguments[0].Position);
                        if (left is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, arguments[0].Position);
                        MarbleData right = (await InterpreteNode(arguments[1], storage)).ToMarbleData(arguments[1].Position);
                        if (right is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, arguments[1].Position);
                        int random_number = new Random().Next((int) left.get_data(), (int) right.get_data());
                        return new MarbleInteger(random_number);
                    }
                    case KeywordToken.KEYWORD.INPUT: {
                        if (arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
                        Input_dialog.DialogText = $"{(await InterpreteNode(arguments[0], storage)).ToMarbleData(arguments[0].Position)}";
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
                        if (storage.HasFunction(data_node.Data as string)) {
                            return await RunFunction(storage.GetFunction(data_node.Data as string), arguments, node, storage, storage.CreateChild());
                        } else if (storage.HasClass(data_node.Data as string)) {
                            StorageClass storage_class = storage.GetClass(data_node.Data as string);
                            ContextualStorage child_storage = storage_class.Storage.CreateChild();
                            StorageFunction function = storage_class.Storage.GetFunction("Constructor!");
                            if (function == null) {
                                return new MarbleObject(storage_class);
                            }
                            if (arguments.Count != function.Arguments.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Different number of parameters");
                            for (int x = 0; x < function.Arguments.Count; x++) {
                                await AssignOperation(function.Arguments[x], (await InterpreteNode(arguments[x], storage)).ToMarbleData(node.Right.Position), child_storage);
                            }
                            await InterpreteInstructionListNode(function.Instructions, child_storage);
                            return new MarbleObject(storage_class);
                        }
                        throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, data_node.Position);
                    default:
                        return null;
                }
            case BinaryOperatorNode:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
            default:
                return null;
        }
    }
    private async Task<InterpreterOutput> AccessorOperation(BinaryOperatorNode node, ContextualStorage storage) {
        MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);
        MarbleList Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position) as MarbleList;
        switch (Left) {
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleDictionary: case MarbleObject:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Left.Position);
            case MarbleList list:
                if (Right.Elements.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Too many parameters");
                MarbleData index = Right.Elements[0];
                if (index is not MarbleInteger) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Right.Position);
                int value = (int) index.get_data();
                if (value >= list.Elements.Count) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Right.Position, "Out of range");
                return list.Elements[value];
            default:
                return null;
        }

    }
    private async Task<StorageVariable> AssignOperation(Node left, MarbleData data, ContextualStorage storage) {
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
    private async Task<InterpreterOutput> InterpreteBinaryOperatorNode(BinaryOperatorNode node, ContextualStorage storage) {
        switch (node.Operator.Type) {
            case OperatorToken.OPERATOR.DOT:
                return await DotOperation(node, storage);
            case OperatorToken.OPERATOR.ACCESSOR:
                return await AccessorOperation(node, storage);
            case OperatorToken.OPERATOR.CALLER:
                return await CallerOperation(node, storage);
            case OperatorToken.OPERATOR.ADD: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.add(Right, node.Position);
            }
            case OperatorToken.OPERATOR.SUBTRACT: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.subtract(Right, node.Position);
            }
            case OperatorToken.OPERATOR.MULTIPLY: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.multiply(Right, node.Position);
            }
            case OperatorToken.OPERATOR.DIVIDE: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.divide(Right, node.Position);
            }
            case OperatorToken.OPERATOR.INTEGER_DIVIDE: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.integer_divide(Right, node.Position);
            }
            case OperatorToken.OPERATOR.EXPONENT: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.exponent(Right, node.Position);
            }
            case OperatorToken.OPERATOR.MODOLUS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.modolus(Right, node.Position);
            }
            case OperatorToken.OPERATOR.AND: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.and(Right, node.Position);
            }
            case OperatorToken.OPERATOR.BITWISE_AND: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.bitwise_and(Right, node.Position);
            }
            case OperatorToken.OPERATOR.OR: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.or(Right, node.Position);
            }
            case OperatorToken.OPERATOR.BITWISE_OR: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.bitwise_or(Right, node.Position);
            }
            case OperatorToken.OPERATOR.IN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.contains(Right, node.Position);
            }
            case OperatorToken.OPERATOR.IS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.is_is(Right);
            }
            case OperatorToken.OPERATOR.COLON: {
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "What?");
            }
            case OperatorToken.OPERATOR.EQUALS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.NOT_EQUALS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.not_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.GREATER_THAN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.greater_than(Right, node.Position);
            }
            case OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.greater_than_or_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.LESSER_THAN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.lesser_than(Right, node.Position);
            }
            case OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return Left.lesser_than_or_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.ASSIGN: {
                return await AssignOperation(node.Left, (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position), storage);
            }
            case OperatorToken.OPERATOR.ADD_AND_ASSIGN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return await AssignOperation(node.Left, Left.add(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return await AssignOperation(node.Left, Left.subtract(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return await AssignOperation(node.Left, Left.multiply(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return await AssignOperation(node.Left, Left.divide(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return await AssignOperation(node.Left, Left.exponent(Right, node.Position), storage);
            }
            case OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).ToMarbleData(node.Left.Position);;
                MarbleData Right = (await InterpreteNode(node.Right, storage)).ToMarbleData(node.Right.Position);
                return await AssignOperation(node.Left, Left.modolus(Right, node.Position), storage);
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
                        MarbleData data = (await InterpreteNode(node.Operand, storage)).ToMarbleData(node.Operand.Position);
                        return data.negate(node.Operand.Position);
                    }
                    case OperatorToken.OPERATOR.SUBTRACT: {
                        MarbleData data = (await InterpreteNode(node.Operand, storage)).ToMarbleData(node.Operand.Position);
                        if (data is not MarbleInteger && data is not MarbleFloat) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                        return data.negate(node.Operand.Position);
                    }
                }
                break;
            case KeywordToken keyword_token:
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
                        DataNode identifier = node.Operand as DataNode;
                        if (storage.HasVariable(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                        StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESS_MODE.NONE, false, (StorageEntity.DATATYPE) (keyword_token.Keyword - 4), false, null);
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
                                StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESS_MODE.NONE, false, datatype, (function.Right as DataNode).Data as List<Node>, definition.Right as InstructionListNode);
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
                            case KeywordToken.KEYWORD.CONSTRUCTOR: {
                                BinaryOperatorNode definition = node.Operand as BinaryOperatorNode;
                                StorageFunction storage_function = new StorageFunction(StorageEntity.ACCESS_MODE.NONE, false, StorageEntity.DATATYPE.USER_DEFINED, (definition.Left as DataNode).Data as List<Node>, definition.Right as InstructionListNode);
                                storage.CreateFunction("Constructor!", storage_function);
                                return storage_function;
                            }
                        }
                    break;
                }
                break;
            case DataToken: {
                DataNode identifier = node.Operand as DataNode;
                if (storage.HasVariable(identifier.Data as string)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                StorageVariable storage_variable = new StorageVariable(StorageEntity.ACCESS_MODE.NONE, false, StorageEntity.DATATYPE.USER_DEFINED, false, null);
                storage.CreateVariable(identifier.Data as string, storage_variable);
                return storage_variable;
            }
            default:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Weird keyword token");
    }
    private async Task<InterpreterOutput> InterpreteDataNode(DataNode node, ContextualStorage storage) {
        switch (node.Type) {
            case DataNode.TYPE.BOOLEAN: case DataNode.TYPE.INTEGER: case DataNode.TYPE.FLOAT: case DataNode.TYPE.STRING:
                return MarbleData.FromDataNode(node);
            case DataNode.TYPE.NULL:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case DataNode.TYPE.DICTIONARY: {
                Dictionary<object, MarbleData> dictionary = new Dictionary<object, MarbleData>();
                foreach (DataNode element in node.Data as List<Node>) {
                    Dictionary<string, object> key_value = element.Data as Dictionary<string, object>;
                    dictionary.Add((key_value["Token"] as DataToken).Data, (await InterpreteNode(key_value["Node"] as Node, storage)).ToMarbleData(element.Position));
                }
                return new MarbleDictionary(dictionary);
            }
            case DataNode.TYPE.LIST: {
                List<MarbleData> elements = new List<MarbleData>();
                foreach (Node element in node.Data as List<Node>) {
                    elements.Add((await InterpreteNode(element, storage)).ToMarbleData(element.Position));
                }
                return new MarbleList(elements);
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
                MarbleData data = (await InterpreteNode(node.Data, storage)).ToMarbleData(node.Data.Position);
                if (!storage.GetEntity("RETURN!", out StorageEntity storage_entity)) throw new InterpreterError(InterpreterError.TYPE.UNEXPECTED_TOKEN, node.Position);
                StorageVariable storage_variable = storage_entity as StorageVariable;
                switch (storage_variable.Datatype) {
                    case StorageEntity.DATATYPE.VOID:
                        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "This function does not return");
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
                        if (data is not MarbleObject) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Position);
                        storage_variable.Data = data.Duplicate();
                        break;
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
        while (MarbleBoolean.Convert((await InterpreteNode(node.Expression, storage)).ToMarbleData(node.Expression.Position), node.Expression.Position).Value) {
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
        StorageVariable Iterator = new StorageVariable(StorageEntity.ACCESS_MODE.NONE, false, StorageEntity.DATATYPE.VARIANT, false, null);
        child_storage.CreateVariable(node.Iterator.Data as string, Iterator);
        switch ((await InterpreteNode(node.Iteratable, child_storage)).ToMarbleData(node.Iteratable.Position)) {
            case MarbleBoolean: case MarbleFloat:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Iteratable.Position, "Can't loop with this");
            case MarbleInteger integer: {
                Iterator.Data = new MarbleInteger(0);
                for (int x = 0; x < integer.Value; x++) {
                    Iterator.Data.set_data(x);
                    FlowController breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == FlowController.TYPE.CONTINUE) continue;
                    if (breaker.Type == FlowController.TYPE.BREAK) break;
                    if (breaker.Type == FlowController.TYPE.RETURN) return breaker;
                }
                break;
            }
            case MarbleString data: {
                Iterator.Data = new MarbleString("");
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
                foreach (MarbleData element in list.Elements) {
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
    private async Task<FlowController> InterpreteIFNode(IFNode node, ContextualStorage storage) {
        if (MarbleBoolean.Convert((await InterpreteNode(node.Expression, storage)).ToMarbleData(node.Expression.Position), node.Expression.Position).Value) {
            return await InterpreteInstructionListNode(node.Implication, storage);
        } else {
            bool do_else = true;
            foreach (IFNode.ElseIFNode child in node.Else_IF_nodes) {
                if (MarbleBoolean.Convert((await InterpreteNode(child.Expression, storage)).ToMarbleData(child.Expression.Position), child.Expression.Position).Value) {
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
        if (MarbleBoolean.Convert((await InterpreteNode(node.Expression, storage)).ToMarbleData(node.Expression.Position), node.Expression.Position).Value) {
            return (await InterpreteNode(node.Implication, storage)).ToMarbleData(node.Implication.Position);
        } else {
            bool do_else = true;
            foreach (InlineIFNode.InlineElseIFNode child in node.Children) {
                if (MarbleBoolean.Convert((await InterpreteNode(child.Expression, storage)).ToMarbleData(child.Expression.Position), child.Expression.Position).Value) {
                    return (await InterpreteNode(child.Implication, storage)).ToMarbleData(child.Implication.Position);
                }
            }
            if (do_else && node.Inverse != null) {
                return (await InterpreteNode(node.Inverse, storage)).ToMarbleData(node.Inverse.Position);
            }
        }
        return null;
    }
}