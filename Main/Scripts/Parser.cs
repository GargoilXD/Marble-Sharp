using System;
using System.Collections.Generic;
using System.Linq;
public class Parser {
    private static readonly OperatorToken.OPERATOR[][] PRECEDENCE = new OperatorToken.OPERATOR[][] {
        new[] { OperatorToken.OPERATOR.ASSIGN, OperatorToken.OPERATOR.ADD_AND_ASSIGN, OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN, OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN, OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN, OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN, OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN },
        new[] { OperatorToken.OPERATOR.AND, OperatorToken.OPERATOR.OR, OperatorToken.OPERATOR.BITWISE_AND, OperatorToken.OPERATOR.BITWISE_OR },
        new[] { OperatorToken.OPERATOR.EQUALS, OperatorToken.OPERATOR.NOT_EQUALS, OperatorToken.OPERATOR.LESSER_THAN, OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS, OperatorToken.OPERATOR.GREATER_THAN, OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS },
        new[] { OperatorToken.OPERATOR.IN },
        new[] { OperatorToken.OPERATOR.ADD, OperatorToken.OPERATOR.SUBTRACT },
        new[] { OperatorToken.OPERATOR.MULTIPLY, OperatorToken.OPERATOR.DIVIDE, OperatorToken.OPERATOR.INTEGER_DIVIDE },
        new[] { OperatorToken.OPERATOR.MODOLUS },
        new[] { OperatorToken.OPERATOR.EXPONENT },
        new[] { OperatorToken.OPERATOR.IS },
        new[] { OperatorToken.OPERATOR.DOT }
    };
    private Node get_expression(int depth = 0) {
        if (depth == PRECEDENCE.Length - 1) return get_binary_node(get_operand_node, PRECEDENCE[depth], get_operand_node);
        else return get_binary_node(() => get_expression(depth + 1), PRECEDENCE[depth], () => get_expression(depth + 1));
    }
    private Node get_statement(bool get_expression_on_fail = true) {
        return get_binary_node(() => get_unary_node(get_expression_on_fail), PRECEDENCE[0], () => get_expression());
    }
    private int Index;
    private Token CurrentToken;
    private Token PreviousToken;
    private List<Token> Tokens;
    private List<string> NewDatatypes = new List<string>(){};
    public Action<string> OnNewDatatype;
    public Action<List<string>> ClearDatatypes;
    private void next_token() {
        Index += 1;
        if (Index < Tokens.Count) {
            PreviousToken = CurrentToken;
            CurrentToken = Tokens[Index];
        }
    }
    private void AddNewDataType(DataToken datatype) {
        string datatype_name = (string) datatype.Data;
        if (NewDatatypes.Contains(datatype_name)) throw new ParserError(ParserError.TYPE.ALREADY_DEFINED_DATATYPE, datatype.Position);
        NewDatatypes.Add(datatype_name);
        OnNewDatatype(datatype_name);
    }
    private List<Node> get_bracket_node(SymbolToken.SYMBOL bracket_symbol, string start_error, string end_error, Func<Node> getter) {
        if (!CurrentToken.is_symbol(bracket_symbol, out SymbolToken start)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, $"Expected '{start_error}'");
        next_token();
        List<Node> elements = new List<Node>();
        while (!(CurrentToken.is_symbol(bracket_symbol + 1) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            elements.Add(getter());
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
            next_token();
        }
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        if (!CurrentToken.is_symbol(bracket_symbol + 1)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, start.Position + CurrentToken.Position, $"Expected '{end_error}'");
        next_token();
        return elements;
    }
    private Node get_operand_node() {
        switch (CurrentToken) {
            case DataToken data_token:
                next_token();
                //if (data_token.is_data(DataToken.TYPE.WORD) && NewDatatypes.Contains(data_token.Data)) return new DataNode(DataNode.TYPE.DATATYPE, data_token.Data, data_token.Position);
                //else
                return DataNode.FromToken(data_token);
            case SymbolToken symbol_token:
                switch (symbol_token.Symbol) {
                    case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET:
                        next_token();
                        Node expression = get_expression();
                        if (CurrentToken is DataToken) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected operator");
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position, "Expected ')'");
                        next_token();
                        return expression;
                    case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET:
                        return new DataNode(DataNode.TYPE.DICTIONARY, get_bracket_node(symbol_token.Symbol, "{", "}", delegate {
                            if (CurrentToken is not DataToken) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected data key");
                            if (CurrentToken.is_data(DataToken.TYPE.NULL)) throw new Error(CurrentToken.Position, "Key can not be null");
                            Token key = CurrentToken;
                            next_token();
                            if (!CurrentToken.is_operator(OperatorToken.OPERATOR.COLON)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected ':'");
                            next_token();
                            return new DataNode(DataNode.TYPE.DICTIONARY, new Dictionary<string, object>(){{"Token", key}, {"Node", get_operand_node()}}, null);
                        }), symbol_token.Position + PreviousToken.Position);
                    case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET:
                        return new DataNode(DataNode.TYPE.LIST, get_bracket_node(symbol_token.Symbol, "[", "]", () => get_expression()), symbol_token.Position + PreviousToken.Position);
                }
                break;
            case KeywordToken keyword_token:
                switch (keyword_token.Type) {
                    case KeywordToken.TYPE.DATATYPE:
                        next_token();
                        return new DataNode(DataNode.TYPE.DATATYPE, keyword_token.Keyword, keyword_token.Position);
                    case KeywordToken.TYPE.INBUILT_FUNCTION:
                        next_token();
                        return new DataNode(DataNode.TYPE.INBUILT_FUNCTION, keyword_token.Keyword, keyword_token.Position);
                }
                switch (keyword_token.Keyword) {
                    case KeywordToken.KEYWORD.IF:
                        return get_inline_if_node(keyword_token.Position);
                    
                    case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position, "Expected if");
                }
                break;
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.NOT: case OperatorToken.OPERATOR.ADD: case OperatorToken.OPERATOR.SUBTRACT:
                        next_token();
                        return new UnaryOperatorNode(operator_token, get_operand_node());
                    default:
                        throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "Expected unary operator");
                }
        }
        throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
    }
    private Node get_unary_node(bool get_expression_on_fail = true) {
        switch (CurrentToken) {
            case KeywordToken keyword_token:
                switch (keyword_token.Type) {
                    case KeywordToken.TYPE.MODIFIER:
                        next_token();
                        return new UnaryOperatorNode(keyword_token, get_unary_node());
                    case KeywordToken.TYPE.DATATYPE: {
                        next_token();
                        if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
                        //if (NewDatatypes.Contains((CurrentToken as DataToken).Data)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
                        Node identifier = get_operand_node();
                        if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, out SymbolToken symbol_token)) {
                            identifier = new BinaryOperatorNode(identifier, new OperatorToken(OperatorToken.OPERATOR.CALLER, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_statement(false)), symbol_token.Position + PreviousToken.Position));
                            return new UnaryOperatorNode(keyword_token, new BinaryOperatorNode(identifier, new OperatorToken(OperatorToken.OPERATOR.DEFINE, CurrentToken.Position), get_instruction_list_node()));
                        } else {
                            if (keyword_token.Keyword == KeywordToken.KEYWORD.VOID) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position, "Unexpected datatype");
                            return new UnaryOperatorNode(keyword_token, identifier);
                        }
                    }
                    case KeywordToken.TYPE.DEFINITION: {
                        next_token();
                        if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
                        //if (NewDatatypes.Contains((CurrentToken as DataToken).Data)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
                        AddNewDataType(CurrentToken as DataToken);
                        Node identifier = get_binary_node(get_operand_node, new [] {OperatorToken.OPERATOR.EXTENDS}, get_operand_node);
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.CLASS: case KeywordToken.KEYWORD.STRUCTURE:
                                return new UnaryOperatorNode(keyword_token, new BinaryOperatorNode(identifier, new OperatorToken(OperatorToken.OPERATOR.DEFINE, CurrentToken.Position), get_instruction_list_node(true)));
                            case KeywordToken.KEYWORD.ENUMERATION:
                                Token start = CurrentToken;
                                return new UnaryOperatorNode(keyword_token, new BinaryOperatorNode(identifier, new OperatorToken(OperatorToken.OPERATOR.DEFINE, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, "{", "}", () => get_expression()), start.Position + PreviousToken.Position)));
                        }
                        break;
                    }
                }
                break;
            case DataToken data_token:
                switch (data_token.Type) {
                    case DataToken.TYPE.WORD:
                        //if (data_token.is_data(DataToken.TYPE.WORD) && NewDatatypes.Contains(data_token.Data)) {
                            next_token();
                            return new UnaryOperatorNode(data_token, get_operand_node());
                        //}
                        //break;
                }
                break;
        }
        if (get_expression_on_fail) return get_expression();
        else throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "Expected unary operator");
    }
    private InstructionListNode get_instruction_list_node(bool definition = false) {
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, out SymbolToken start)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        List<Node> instructions = get_nodes(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET, definition);
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, start.Position + CurrentToken.Position, "Expected '}'");
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        return new InstructionListNode(instructions, start.Position + PreviousToken.Position);
    }
    private IFNode get_if_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node if_expression = get_operand_node();
        InstructionListNode implication = get_instruction_list_node();
        List<IFNode.ElseIFNode> children = new List<IFNode.ElseIFNode>();
        while (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE_IF)) {
            KeywordToken else_if_token = CurrentToken as KeywordToken;
            next_token();
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
            children.Add(new IFNode.ElseIFNode(get_operand_node(), get_instruction_list_node(), else_if_token.Position + PreviousToken.Position));
        }
        InstructionListNode inverse = null;
        if (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE)) {
            next_token();
            inverse = get_instruction_list_node();
        }
        return new IFNode(if_expression, implication, children, inverse, start_position + PreviousToken.Position);
    }
    private MatchNode get_match_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node expression = get_operand_node();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
        next_token();
        List<MatchNode.CaseNode> cases = new List<MatchNode.CaseNode>();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        if (!CurrentToken.is_keyword(KeywordToken.KEYWORD.CASE)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected case");
        while (CurrentToken.is_keyword(KeywordToken.KEYWORD.CASE)) {
            KeywordToken case_token = CurrentToken as KeywordToken;
            next_token();
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
            DataNode case_expression = new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_expression()), PreviousToken.Position);
            if ((case_expression.Data as List<Node>).Count == 0) throw new ParserError(ParserError.TYPE.EMPTHY_CASE, case_token.Position);
            cases.Add(new MatchNode.CaseNode(case_expression, get_instruction_list_node(), case_token.Position + PreviousToken.Position));
        }
        InstructionListNode default_node = null;
        if (CurrentToken.is_keyword(KeywordToken.KEYWORD.DEFAULT)) {
            next_token();
            default_node = get_instruction_list_node();
        }
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, start_position + PreviousToken.Position, "Expected '{'");
        next_token();
        return new MatchNode(expression, cases, default_node, start_position + PreviousToken.Position);
    }
    private InlineIFNode get_inline_if_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node if_expression = get_operand_node();
        Node implication = get_expression();
        List<InlineIFNode.InlineElseIFNode> children = new List<InlineIFNode.InlineElseIFNode>();
        while (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE_IF)) {
            KeywordToken else_if_token = CurrentToken as KeywordToken;
            next_token();
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
            children.Add(new InlineIFNode.InlineElseIFNode(get_operand_node(), get_expression(), else_if_token.Position + PreviousToken.Position));
        }
        Node inverse = null;
        if (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE)) {
            next_token();
            inverse = get_operand_node();
        }
        return new InlineIFNode(if_expression, implication, children, inverse, start_position + PreviousToken.Position);
    }
    private ForNode get_for_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        next_token();
        Node iterator = get_binary_node(() => get_unary_node(), new [] {OperatorToken.OPERATOR.ASSIGN}, get_operand_node);
        if (!CurrentToken.is_operator(OperatorToken.OPERATOR.IN))  throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "'in'");
        OperatorToken In = CurrentToken as OperatorToken;
        next_token();
        Node iteratable = get_operand_node();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected ')'");
        next_token();
        InstructionListNode instructions = get_instruction_list_node();
        return new ForNode(iterator, iteratable, instructions, start_position + PreviousToken.Position);

    }
    private WhileNode get_while_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node expression = get_operand_node();
        InstructionListNode instructions = get_instruction_list_node();
        return new WhileNode(expression, instructions, start_position + PreviousToken.Position);
    }
    private Node get_bracket_operation(Node node) {
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET)) {
            SymbolToken symbol_token = CurrentToken as SymbolToken;
            if (symbol_token.Symbol == SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET) {
                node = new BinaryOperatorNode(node, new OperatorToken(OperatorToken.OPERATOR.CALLER, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_expression()), symbol_token.Position + PreviousToken.Position));
            }
            else if (symbol_token.Symbol == SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET) {
                node = new BinaryOperatorNode(node, new OperatorToken(OperatorToken.OPERATOR.ACCESSOR, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET, "[", "]", () => get_expression()), symbol_token.Position + PreviousToken.Position));
            }
        }
        //if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) {
            //throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
            //return new BinaryOperatorNode(left_node, new OperatorToken(OperatorToken.OPERATOR.EXECUTE, CurrentToken.Position), get_instruction_list_node());
        //}
        return node;
    }
    private Node get_binary_node(Func<Node> left, OperatorToken.OPERATOR[] precedence, Func<Node> right) {
        Node left_node = get_bracket_operation(left());
        while (CurrentToken is OperatorToken && precedence.Contains(((OperatorToken) CurrentToken).Type)) {
            OperatorToken operatorToken = CurrentToken as OperatorToken;
            next_token();
            left_node = new BinaryOperatorNode(left_node, operatorToken, get_bracket_operation(right()));
        }
        return left_node;
    }
    private List<Node> get_nodes(SymbolToken.SYMBOL breaker, bool definition = false) {
        List<Node> nodes = new List<Node>();
        while (!CurrentToken.is_symbol(breaker)) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            if (CurrentToken.is_symbol(breaker) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END)) break;
            switch (CurrentToken) {
                case KeywordToken keyword_token:
                    switch (keyword_token.Type) {
                        case KeywordToken.TYPE.DECISION:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
                                case KeywordToken.KEYWORD.CASE: case KeywordToken.KEYWORD.DEFAULT:
                                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected match");
                                case KeywordToken.KEYWORD.MATCH: 
                                    nodes.Add(get_match_node(keyword_token.Position));
                                    break;
                                case KeywordToken.KEYWORD.IF:
                                    nodes.Add(get_if_node(keyword_token.Position));
                                    break;
                            }
                            break;
                        case KeywordToken.TYPE.FLOWCONTROL:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.RETURN:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.RETURN, get_expression(), keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.BREAK:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.BREAK, null, keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.CONTINUE:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.CONTINUE, null, keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.BREAKPOINT:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.BREAKPOINT, null, keyword_token.Position + PreviousToken.Position));
                                    break;
                            }
                            break;
                        case KeywordToken.TYPE.LOOP:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.FOR: {
                                    nodes.Add(get_for_node(keyword_token.Position));
                                    break;
                                }
                                case KeywordToken.KEYWORD.WHILE: {
                                    nodes.Add(get_while_node(keyword_token.Position));
                                    break;
                                }
                            }
                            break;
                        case KeywordToken.TYPE.DEFINITION:

                        default:
                            nodes.Add(get_statement(!definition));
                            break;
                    }
                    break;
                default:
                    nodes.Add(get_statement(!definition));
                    break;
            }
        }
        return nodes;
    }
    public List<Node> Parse(List<Token> tokens) {
        Tokens = tokens;
        Index = 0;
        CurrentToken = tokens[0];
        ClearDatatypes(NewDatatypes);
        NewDatatypes.Clear();
        return get_nodes(SymbolToken.SYMBOL.END);
    }
}
