using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Interpreter {
    public static Godot.AcceptDialog InputGetter;
    public string Output;
    public async void Interprete(List<Node> nodes, InterpreterStorage storage) {
        foreach (Node node in nodes) {
            await InterpreteNode(node, storage);
        }
    }
    private async Task<MarbleData> InterpreteNode(Node node, InterpreterStorage storage) => node switch {
        EnumerationDefinitionNode swith_node => await InterpreteEnumerationDefinitionNode(swith_node, storage),
        FunctionDefinitionNode swith_node => await InterpreteFunctionDefinitionNode(swith_node, storage),
        BinaryOperatorNode swith_node => await InterpreteBinaryOperatorNode(swith_node, storage),
        UnaryOperatorNode swith_node => await InterpreteUnaryOperatorNode(swith_node, storage),
        FunctionNode swith_node => await InterpreteFunctionNode(swith_node, storage),
        KeywordNode swith_node => await InterpreteKeywordNode(swith_node, storage),
        DataNode swith_node => await InterpreteDataNode(swith_node, storage),
        IFNode swith_node => await InterpreteIFNode(swith_node, storage),
        _ => null
    };
    private Task<MarbleData> InterpreteEnumerationDefinitionNode(EnumerationDefinitionNode node, InterpreterStorage storage) {
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Undone");
    }
    private Task<MarbleData> InterpreteFunctionDefinitionNode(FunctionDefinitionNode node, InterpreterStorage storage) {
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Undone");
    }
    private async Task<MarbleData> InterpreteBinaryOperatorNode(BinaryOperatorNode node, InterpreterStorage storage) {
        switch ((node.Operator as OperatorToken).Type) {
            case OperatorToken.OPERATOR.DOT: {
                break;
            }
            case OperatorToken.OPERATOR.ACCESSOR: {
                break;
            }
            case OperatorToken.OPERATOR.ADD: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.SUBTRACT: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.MULTIPLY: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.DIVIDE: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.INTEGER_DIVIDE: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.EXPONENT: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.MODOLUS: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.AND: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.BITWISE_AND: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.OR: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.BITWISE_OR: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.IN: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.IS: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.COLON: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.EQUALS: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.NOT_EQUALS: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.GREATER_THAN: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.LESSER_THAN: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.ASSIGN: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                return Left.add(Right);
            }
            case OperatorToken.OPERATOR.ADD_AND_ASSIGN: {
                return await assign_operation(node, storage, delegate (MarbleData left, MarbleData right) { left.set_data(left.add(right)); });
            }
            case OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN: {
                MarbleData Left = await InterpreteNode(node.Left, storage);
                MarbleData Right = await InterpreteNode(node.Right, storage);
                if (Left is MarbleVariant) {
                    Left.set_data(Right.get_data());
                }
                else{
                    if (Left.GetType() == Right.GetType()) {
                        Left.set_data(Left.subtract(Right).get_data());
                    }
                    else {
                        MarbleData converted = Left.Convert(Right);
                        Left.set_data(Left.subtract(converted).get_data());
                    }
                }
                return Left;
            }
            case OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN: {}
            case OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN: {}
            case OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN: {}
            case OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN: {}
            case OperatorToken.OPERATOR.EXTENDS: {}
            case OperatorToken.OPERATOR.RUNS: {}
        }
    }
    private async Task<MarbleData> InterpreteUnaryOperatorNode(UnaryOperatorNode node, InterpreterStorage storage) {
        switch (node.Operator) {
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.ADD:
                        return await InterpreteNode(node.Operand, storage);
                    case OperatorToken.OPERATOR.NOT: {
                        MarbleData data = await InterpreteNode(node.Operand, storage);
                        try { return data.negate(); } catch (InterpreterError error) { throw error.ResetPosition(node.Position); }
                    }
                    case OperatorToken.OPERATOR.SUBTRACT: {
                       MarbleData data = await InterpreteNode(node.Operand, storage);
                       if (data is not MarbleInteger && data is not MarbleFloat) throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, node.Position);
                        try { return data.negate(); } catch (InterpreterError error) { throw error.ResetPosition(node.Position); }
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
                    case KeywordToken.KEYWORD.FOR:
                        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Undone");
                    case KeywordToken.KEYWORD.WHILE:
                        MarbleData logicked = await InterpreteNode(node.Operand, storage);
                        if (logicked is not MarbleBoolean) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, node.Operand.Position);
                        return logicked;
                }
                break;
            default:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Weird keyword token");
    }
    private Task<MarbleData> InterpreteFunctionNode(FunctionNode node, InterpreterStorage storage) {
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Undone");
    }
    private Task<MarbleData> InterpreteKeywordNode(KeywordNode node, InterpreterStorage storage) {
        switch (node.Keyword) {
            case KeywordToken.KEYWORD.BREAK: case KeywordToken.KEYWORD.CONTINUE: case KeywordToken.KEYWORD.BREAKPOINT:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Weird keyword token");
    }
    private async Task<MarbleData> InterpreteDataNode(DataNode node, InterpreterStorage storage) {
        switch (node.Type) {
            case DataNode.TYPE.BOOLEAN: case DataNode.TYPE.INTEGER: case DataNode.TYPE.FLOAT: case DataNode.TYPE.STRING:
                return MarbleData.FromDataNode(node);
            case DataNode.TYPE.VARIANT: case DataNode.TYPE.OBJECT:
                throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Hmmm");
            case DataNode.TYPE.TUPLE:
            case DataNode.TYPE.DICTIONARY: {
                Dictionary<object, MarbleData> dictionary = new Dictionary<object, MarbleData>();
                foreach (DataNode element in node.Data as List<Node>) {
                    Dictionary<string, object> key_value = element.Data as Dictionary<string, object>;
                    dictionary.Add((key_value["Token"] as DataToken).Data, await InterpreteNode(key_value["Node"] as Node, storage));
                }
                return new MarbleDictionary(dictionary);
            }
            case DataNode.TYPE.LIST: {
                List<MarbleData> elements = new List<MarbleData>();
                foreach (Node element in node.Data as List<Node>) {
                    elements.Add(await InterpreteNode(element, storage));
                }
                return new MarbleList(elements);
            }
            case DataNode.TYPE.IDENTIFIER:
                try { return storage.GetVariable(node.Data as string); } catch (InterpreterError error) { throw error.ResetPosition(node.Position); }
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Weird data token");
    }
    private Task<MarbleData> InterpreteIFNode(IFNode node, InterpreterStorage storage) {
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, node.Position, "Undone");
    }
    private async Task<MarbleData> assign_operation(BinaryOperatorNode node, InterpreterStorage storage, Action<MarbleData, MarbleData> operation){
        MarbleData Left = await InterpreteNode(node.Left, storage);
        MarbleData Right = await InterpreteNode(node.Right, storage);
        if (Left is MarbleVariant) {
            Left.set_data(Right.get_data());
        }
        else {
            if (Left.GetType() == Right.GetType()) {
                operation(Left, Right);
            }
            else {
                operation(Left, Left.Convert(Right));
            }
        }
        return Left;
    }
    private async Task<MarbleData> Binary_operation(BinaryOperatorNode node, InterpreterStorage storage, Func<MarbleData> operation){
        MarbleData Left = await InterpreteNode(node.Left, storage);
        MarbleData Right = await InterpreteNode(node.Right, storage);
        return operation();
    }
}