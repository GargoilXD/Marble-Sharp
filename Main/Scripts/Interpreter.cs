using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Interpreter {
    public static InputGetter Input_dialog;
    public string Output = "";
    public async Task<InterpreterOutput> Interprete(List<Node> nodes, InterpreterStorage storage) {
        foreach (Node node in nodes) {
            InterpreterOutput output = await InterpreteNode(node, storage) ;
        }
        return new ContextControl(ContextControl.TYPE.DONE);
    }
    private async Task<InterpreterOutput> InterpreteNode(Node node, InterpreterStorage storage) => node switch {
        EnumerationDefinitionNode switch_node => await InterpreteEnumerationDefinitionNode(switch_node, storage),
        FunctionDefinitionNode switch_node => await InterpreteFunctionDefinitionNode(switch_node, storage),

        BinaryOperatorNode switch_node => await InterpreteBinaryOperatorNode(switch_node, storage),
        UnaryOperatorNode switch_node => await InterpreteUnaryOperatorNode(switch_node, storage),
        KeywordNode switch_node => InterpreteKeywordNode(switch_node, storage),
        DataNode switch_node => await InterpreteDataNode(switch_node, storage),

        FunctionNode switch_node => await InterpreteFunctionNode(switch_node, storage),

        WhileNode switch_node => await InterpreteWhileNode(switch_node, storage),
        ForNode switch_node => await InterpreteForNode(switch_node, storage),
        IFNode switch_node => await InterpreteIFNode(switch_node, storage),
        _ => null
    };
    
    private async Task<ContextControl> InterpreteInstructionListNode(InstructionListNode node, InterpreterStorage storage) {
        foreach (Node statement in node.Instructions) {
            InterpreterOutput result = await InterpreteNode(statement, storage);
            if (result is ContextControl && (result as ContextControl).Type != ContextControl.TYPE.DONE) return result as ContextControl;
        }
        return new ContextControl(ContextControl.TYPE.DONE);
    }
    private Task<InterpreterOutput> InterpreteEnumerationDefinitionNode(EnumerationDefinitionNode node, InterpreterStorage storage) {
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private Task<InterpreterOutput> InterpreteFunctionDefinitionNode(FunctionDefinitionNode node, InterpreterStorage storage) {
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<MarbleData> InterpreteBinaryOperatorNode(BinaryOperatorNode node, InterpreterStorage storage) {
        switch ((node.Operator as OperatorToken).Type) {
            case OperatorToken.OPERATOR.DOT: {
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            }
            case OperatorToken.OPERATOR.ACCESSOR: {
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            }
            case OperatorToken.OPERATOR.ADD: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.add(Right, node.Position);
            }
            case OperatorToken.OPERATOR.SUBTRACT: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.subtract(Right, node.Position);
            }
            case OperatorToken.OPERATOR.MULTIPLY: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.multiply(Right, node.Position);
            }
            case OperatorToken.OPERATOR.DIVIDE: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.divide(Right, node.Position);
            }
            case OperatorToken.OPERATOR.INTEGER_DIVIDE: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.integer_divide(Right, node.Position);
            }
            case OperatorToken.OPERATOR.EXPONENT: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.exponent(Right, node.Position);
            }
            case OperatorToken.OPERATOR.MODOLUS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.modolus(Right, node.Position);
            }
            case OperatorToken.OPERATOR.AND: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.and(Right, node.Position);
            }
            case OperatorToken.OPERATOR.BITWISE_AND: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.bitwise_and(Right, node.Position);
            }
            case OperatorToken.OPERATOR.OR: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.or(Right, node.Position);
            }
            case OperatorToken.OPERATOR.BITWISE_OR: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.bitwise_or(Right, node.Position);
            }
            case OperatorToken.OPERATOR.IN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.contains(Right, node.Position);
            }
            case OperatorToken.OPERATOR.IS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.is_is(Right);
            }
            case OperatorToken.OPERATOR.COLON: {
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "What?");
            }
            case OperatorToken.OPERATOR.EQUALS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.NOT_EQUALS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.not_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.GREATER_THAN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.greater_than(Right, node.Position);
            }
            case OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.greater_than_or_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.LESSER_THAN: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.lesser_than(Right, node.Position);
            }
            case OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS: {
                MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
                MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
                return Left.lesser_than_or_equals(Right, node.Position);
            }
            case OperatorToken.OPERATOR.ASSIGN: {
                return await assign_operation(node, storage, delegate (MarbleData left, MarbleData right) { left.set_data(right.get_data()); });
            }
            case OperatorToken.OPERATOR.ADD_AND_ASSIGN: {
                return await assign_operation(node, storage, delegate (MarbleData left, MarbleData right) { left.set_data(left.add(right, node.Position).get_data()); });
            }
            case OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN: {
                return await assign_operation(node, storage, delegate (MarbleData left, MarbleData right) { left.set_data(left.subtract(right, node.Position).get_data()); });
            }
            case OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN: {
                return await assign_operation(node, storage, delegate (MarbleData left, MarbleData right) { left.set_data(left.multiply(right, node.Position).get_data()); });
            }
            case OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN: {
                return await assign_operation(node, storage, delegate (MarbleData left, MarbleData right) { left.set_data(left.divide(right, node.Position).get_data()); });
            }
            case OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN: {
                return await assign_operation(node, storage, delegate (MarbleData left, MarbleData right) { left.set_data(left.exponent(right, node.Position).get_data()); });
            }
            case OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN: {
                return await assign_operation(node, storage, delegate (MarbleData left, MarbleData right) { left.set_data(left.modolus(right, node.Position).get_data()); });
            }
            case OperatorToken.OPERATOR.EXTENDS: {
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            }
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<InterpreterOutput> InterpreteUnaryOperatorNode(UnaryOperatorNode node, InterpreterStorage storage) {
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
                switch (keyword_token.Keyword) {
                    case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.INTEGER: case KeywordToken.KEYWORD.BOOLEAN: case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST: case KeywordToken.KEYWORD.DICTIONARY: case KeywordToken.KEYWORD.ENUMERATION: {
                        string name = (node.Operand as DataNode).Data as string;
                        if (storage.HasVariable(name)) throw new InterpreterError(InterpreterError.TYPE.ALREADY_DEFINED_IDENTIFIER, node.Operand.Position);
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.VARIANT:
                                return storage.CreateVariable(name, new MarbleVariant(null, false));
                            case KeywordToken.KEYWORD.INTEGER:
                                return storage.CreateVariable(name, new MarbleInteger(0, false));
                            case KeywordToken.KEYWORD.BOOLEAN:
                                return storage.CreateVariable(name, new MarbleBoolean(false, false));
                            case KeywordToken.KEYWORD.FLOAT:
                                return storage.CreateVariable(name, new MarbleFloat(0, false));
                            case KeywordToken.KEYWORD.STRING:
                                return storage.CreateVariable(name, new MarbleString("", false));
                            case KeywordToken.KEYWORD.LIST:
                                return storage.CreateVariable(name, new MarbleList(null, false));
                            case KeywordToken.KEYWORD.DICTIONARY:
                                return storage.CreateVariable(name, new MarbleDictionary(null, false));
                            case KeywordToken.KEYWORD.ENUMERATION:
                                break;
                                //return storage.CreateVariable(name, new MarbleEnumeration("", false));
                        }
                        break;
                    }
                    case KeywordToken.KEYWORD.RETURN:
                        break;
                }
                break;
            default:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Weird keyword token");
    }
    private async Task<InterpreterOutput> InterpreteFunctionNode(FunctionNode node, InterpreterStorage storage) {
        switch (node.Identifier) {
            case KeywordToken keyword_token:
                switch (keyword_token.Keyword) {
                    case KeywordToken.KEYWORD.INPUT: {
                        if (node.Arguments.Count > 1) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Too many parameters");
						Input_dialog.DialogText = $"{(await InterpreteNode(node.Arguments[0], storage)).IsMarbleData(node.Arguments[0].Position)}";
						Input_dialog.Show();
                        await Input_dialog.ToSignal(Input_dialog, "confirmed");
                        //return new ContextOutput(ContextControl.TYPE.RETURN, new MarbleString(Input_dialog.Input));
						return new MarbleString(Input_dialog.Input);
                    }
                    case KeywordToken.KEYWORD.PRINT: {
                        foreach (Node argument in node.Arguments){
							Output += $"{(await InterpreteNode(argument, storage)).IsMarbleData(argument.Position)} ";
                        }
						Output += '\n';
						return new ContextControl(ContextControl.TYPE.DONE);
                    }
                    case KeywordToken.KEYWORD.RANGE:
                    case KeywordToken.KEYWORD.ASSERT:
                    case KeywordToken.KEYWORD.RANDOM:
                        break;
                }
                break;
            case DataToken:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private InterpreterOutput InterpreteKeywordNode(KeywordNode node, InterpreterStorage storage) {
        switch (node.Keyword) {
            case KeywordToken.KEYWORD.BREAKPOINT:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case KeywordToken.KEYWORD.BREAK:
                return new ContextControl(ContextControl.TYPE.BREAK);
            case KeywordToken.KEYWORD.CONTINUE: 
                return new ContextControl(ContextControl.TYPE.CONTINUE);
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<MarbleData> InterpreteDataNode(DataNode node, InterpreterStorage storage) {
        switch (node.Type) {
            case DataNode.TYPE.BOOLEAN: case DataNode.TYPE.INTEGER: case DataNode.TYPE.FLOAT: case DataNode.TYPE.STRING:
                return MarbleData.FromDataNode(node);
            case DataNode.TYPE.TUPLE: case DataNode.TYPE.VARIANT: case DataNode.TYPE.OBJECT:
                throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
            case DataNode.TYPE.DICTIONARY: {
                Dictionary<object, MarbleData> dictionary = new Dictionary<object, MarbleData>();
                foreach (DataNode element in node.Data as List<Node>) {
                    Dictionary<string, object> key_value = element.Data as Dictionary<string, object>;
                    dictionary.Add((key_value["Token"] as DataToken).Data, (await InterpreteNode(key_value["Node"] as Node, storage)).IsMarbleData(element.Position));
                }
                return new MarbleDictionary(dictionary);
            }
            case DataNode.TYPE.LIST: {
                List<MarbleData> elements = new List<MarbleData>();
                foreach (Node element in node.Data as List<Node>) {
                    elements.Add((await InterpreteNode(element, storage)).IsMarbleData(element.Position));
                }
                return new MarbleList(elements);
            }
            case DataNode.TYPE.IDENTIFIER:
                MarbleData variable;
                variable = storage.GetVariable(node.Data as string, node.Position);
                if (!variable.Initialized && !storage.CanGetUninitializedVariable) throw new InterpreterError(InterpreterError.TYPE.UNINITIALIZED_IDENTIFIER, node.Position);
                if (variable is MarbleVariant) return (variable as MarbleVariant).ToStatic(node.Position);
                return variable;
        }
        throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
    }
    private async Task<ContextControl> InterpreteWhileNode(WhileNode node, InterpreterStorage storage) {
        while (MarbleBoolean.Convert((await InterpreteNode(node.Expression, storage)).IsMarbleData(node.Expression.Position), node.Expression.Position).Value) {
            ContextControl breaker = await InterpreteInstructionListNode(node.Instructions, storage);
            if (breaker.Type == ContextControl.TYPE.CONTINUE) continue;
            if (breaker.Type == ContextControl.TYPE.BREAK) break;
            if (breaker is ContextOutput) return breaker;
        }
        return new ContextControl(ContextControl.TYPE.DONE);
    }
    private async Task<ContextControl> InterpreteForNode(ForNode node, InterpreterStorage storage) {
        InterpreterStorage child_storage = storage.CreateChild();
        MarbleData Interator = (await InterpreteNode(node.Iterator, child_storage)).IsMarbleData(node.Iterator.Position);
        MarbleData Interatable = (await InterpreteNode(node.Iteratable, child_storage)).IsMarbleData(node.Iteratable.Position);
        switch (Interatable) {
            case MarbleBoolean: case MarbleFloat:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Iteratable.Position, "Can't loop with this");
            case MarbleInteger integer:
                if (Interator is not MarbleInteger && Interator is not MarbleVariant) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Iteratable.Position);
                int start = (int) Interator.get_data();
                for (int x = start; x < integer.Value; x++) {
                    Interator.set_data(x);
                    ContextControl breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == ContextControl.TYPE.CONTINUE) continue;
                    if (breaker.Type == ContextControl.TYPE.BREAK) break;
                    if (breaker is ContextOutput) return breaker;
                }
                break;
            case MarbleString data:
                if (Interator is not MarbleString || Interator is not MarbleVariant) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Iteratable.Position);
                foreach (char item in data.Value) {
                    Interator.set_data(item + "");
                    ContextControl breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == ContextControl.TYPE.CONTINUE) continue;
                    if (breaker.Type == ContextControl.TYPE.BREAK) break;
                    if (breaker is ContextOutput) return breaker;
                }
                break;
                case MarbleList list:
                foreach (MarbleData element in list.Elements) {
                    Interator.set_data(Interator.convert(element, node.Iterator.Position).get_data());
                    ContextControl breaker = await InterpreteInstructionListNode(node.Instructions, child_storage.CreateChild());
                    if (breaker.Type == ContextControl.TYPE.CONTINUE) continue;
                    if (breaker.Type == ContextControl.TYPE.BREAK) break;
                    if (breaker is ContextOutput) return breaker;
                }
                break;
                case MarbleDictionary:
                    throw new InterpreterError(InterpreterError.TYPE.UNIMPLEMENTED_FEATURE, node.Position);
        }
        return new ContextControl(ContextControl.TYPE.DONE);
    }
    private async Task<ContextControl> InterpreteIFNode(IFNode node, InterpreterStorage storage) {
        if (MarbleBoolean.Convert((await InterpreteNode(node.Expression, storage)).IsMarbleData(node.Expression.Position), node.Expression.Position).Value) {
            return await InterpreteInstructionListNode(node.Implication, storage);
        } else {
            bool do_else = true;
            foreach (IFNode child in node.Children) {
                if (MarbleBoolean.Convert((await InterpreteNode(child.Expression, storage)).IsMarbleData(child.Expression.Position), child.Expression.Position).Value) {
                    return await InterpreteInstructionListNode(child.Implication, storage);
                }
            }
            if (do_else && node.Inverse != null) {
                return await InterpreteInstructionListNode(node.Inverse, storage);
            }
        }
        return new ContextControl(ContextControl.TYPE.DONE);
    }
    private async Task<MarbleData> assign_operation(BinaryOperatorNode node, InterpreterStorage storage, Action<MarbleData, MarbleData> operation) {
        storage.CanGetUninitializedVariable = true;
        MarbleData Left = (await InterpreteNode(node.Left, storage)).IsMarbleData(node.Left.Position);
        storage.CanGetUninitializedVariable = false;
        MarbleData Right = (await InterpreteNode(node.Right, storage)).IsMarbleData(node.Right.Position);
        if (Left is MarbleVariant) {
            Left.set_data(Right.get_data());
            Left.Initialized = true;
        }
        else {
            if (Left.GetType() == Right.GetType()) {
                operation(Left, Right);
                Left.Initialized = true;
            }
            else {
                operation(Left, Left.convert(Right, node.Right.Position));
                Left.Initialized = true;
            }
        }
        return Left;
    }
}