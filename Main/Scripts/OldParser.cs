/*
using System;
using System.Collections.Generic;
using System.Linq;
public class OldParser {
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
    private struct STRUCTURES {
        public static byte VARIABLE_DEFINITION = 1;
        public static byte FUNCTION_DEFINITION = 2;
        public static byte CLASS_DEFINITION = 4;
        public static byte STATEMENT = 8;
        public static byte FLOW_CONTROL = 16;
        public static int DEFINITION = VARIABLE_DEFINITION + FUNCTION_DEFINITION + CLASS_DEFINITION;
        public static int ALL = DEFINITION + STATEMENT + FLOW_CONTROL;
        public static bool IN(int structure, int container) {
            return (structure & container) != 0;
        }
    }
    private Node get_expression(int depth = 0) {
        if (depth == PRECEDENCE.Length - 1) return get_binary_node(get_operand_node, PRECEDENCE[depth], get_operand_node);
        else return get_binary_node(() => get_expression(depth + 1), PRECEDENCE[depth], () => get_expression(depth + 1));
    }
    private int Index;
    private Token CurrentToken;
    private Token PreviousToken;
    private List<Token> Tokens;
    private List<string> NewDatatypes = new List<string>(){};
    public Action<string> OnNewDatatype;
    public Action<List<string>> ClearDatatypes;
    public List<Node> Parse(List<Token> tokens) {
        Tokens = tokens;
        Index = 0;
        CurrentToken = tokens[0];
        ClearDatatypes(NewDatatypes);
        NewDatatypes.Clear();
        return get_nodes(SymbolToken.SYMBOL.END, STRUCTURES.ALL - STRUCTURES.FLOW_CONTROL);
    }
    private void next_token() {
        Index += 1;
        if (Index < Tokens.Count) {
            PreviousToken = CurrentToken;
            CurrentToken = Tokens[Index];
        }
    }
    private Token check_next_token() {
        if ((Index + 1) < Tokens.Count) {
            return Tokens[Index + 1];
        }
        return null;
    }
    private List<Node> get_bracket_node(SymbolToken.SYMBOL bracket_symbol, string start_error, string end_error, Func<Node> getter, out TokenPosition position) {
        if (!CurrentToken.is_symbol(bracket_symbol)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, $"Expected '{start_error}'");
        position = CurrentToken.Position;
        next_token();
        List<Node> elements = new List<Node>();
        while (!(CurrentToken.is_symbol(bracket_symbol + 1) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            elements.Add(getter());
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
            next_token();
        }
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        if (!CurrentToken.is_symbol(bracket_symbol + 1)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, position + CurrentToken.Position, $"Expected '{end_error}'");
        position += CurrentToken.Position;
        next_token();
        return elements;
    }
    private Node get_operand_node() {
        switch (CurrentToken) {
            case DataToken data_token:
                next_token();
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
                    case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET: {
                        return new DataNode(DataNode.TYPE.DICTIONARY, get_bracket_node(symbol_token.Symbol, "{", "}", delegate {
                            if (CurrentToken is not DataToken) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected data key");
                            if (CurrentToken.is_data(DataToken.TYPE.NULL)) throw new Error(CurrentToken.Position, "Key can not be null");
                            Token key = CurrentToken;
                            next_token();
                            if (!CurrentToken.is_operator(OperatorToken.OPERATOR.COLON)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected ':'");
                            next_token();
                            return new DataNode(DataNode.TYPE.DICTIONARY, new Dictionary<string, object>(){{"Token", key}, {"Node", get_operand_node()}}, null);
                        }, out TokenPosition position), position);
                    }
                    case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET: {
                        return new DataNode(DataNode.TYPE.LIST, get_bracket_node(symbol_token.Symbol, "[", "]", () => get_expression(), out TokenPosition position), position);
                    }
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
                    case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position, "Expected if");
                    case KeywordToken.KEYWORD.IF: {
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
                        return new InlineIFNode(if_expression, implication, children, inverse, keyword_token.Position + PreviousToken.Position);
                    }
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
    private Node get_bracket_operation(Node node) {
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET)) {
            if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) {
                node = new BinaryOperatorNode(node, new OperatorToken(OperatorToken.OPERATOR.CALLER, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_expression(), out TokenPosition position), position));
            }
            else if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET)) {
                node = new BinaryOperatorNode(node, new OperatorToken(OperatorToken.OPERATOR.ACCESSOR, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET, "[", "]", () => get_expression(), out TokenPosition position), position));
            }
        }
        //if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) {
            //throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
            //return new BinaryOperatorNode(left_node, new OperatorToken(OperatorToken.OPERATOR.DEFINES, CurrentToken.Position), get_instruction_list_node());
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
    private Node get_varaible_or_function_structure(Token datatype, int allowed_structures) {
        next_token();
        if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
        Node identifier = DataNode.FromToken(CurrentToken as DataToken);
        next_token();
        if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, out SymbolToken symbol_token)) {
            if (!STRUCTURES.IN(STRUCTURES.FUNCTION_DEFINITION, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
            identifier = new BinaryOperatorNode(identifier, new OperatorToken(OperatorToken.OPERATOR.CALLER, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_definition_structure(allowed_structures), out TokenPosition position), position));
            return new UnaryOperatorNode(datatype, new BinaryOperatorNode(identifier, new OperatorToken(OperatorToken.OPERATOR.DEFINE, CurrentToken.Position), get_instruction_list_node(STRUCTURES.VARIABLE_DEFINITION + STRUCTURES.STATEMENT + STRUCTURES.FLOW_CONTROL)));
        } else {
            if (!STRUCTURES.IN(STRUCTURES.VARIABLE_DEFINITION, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
            UnaryOperatorNode variable_definition = new UnaryOperatorNode(datatype, identifier);
            if (CurrentToken.is_operator(OperatorToken.OPERATOR.ASSIGN, out OperatorToken operator_token)) {
                if ( datatype.is_keyword(KeywordToken.KEYWORD.VOID)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, datatype.Position, "Unexpected datatype");
                next_token();
                return new BinaryOperatorNode(variable_definition, operator_token, get_expression());
            } else {
                return variable_definition;
            }
        }
    }
    private Node get_definition_structure(int allowed_structures) {
        switch (CurrentToken) {
            case KeywordToken keyword_token:
                switch (keyword_token.Type) {
                    case KeywordToken.TYPE.MODIFIER:
                        next_token();
                        return new UnaryOperatorNode(keyword_token, get_definition_structure(allowed_structures));
                    case KeywordToken.TYPE.DATATYPE:
                        return get_varaible_or_function_structure(keyword_token, allowed_structures);
                    case KeywordToken.TYPE.DEFINITION: {
                        next_token();
                        if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
                        string datatype_name = (CurrentToken as DataToken).Data as string;
                        if (NewDatatypes.Contains(datatype_name)) throw new ParserError(ParserError.TYPE.ALREADY_DEFINED_DATATYPE, CurrentToken.Position);
                        NewDatatypes.Add(datatype_name);
                        OnNewDatatype(datatype_name);
                        Node identifier = get_binary_node(get_operand_node, new [] {OperatorToken.OPERATOR.EXTENDS}, get_operand_node);
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.CLASS: case KeywordToken.KEYWORD.STRUCTURE:
                                if (!STRUCTURES.IN(STRUCTURES.CLASS_DEFINITION, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                return new UnaryOperatorNode(keyword_token, new BinaryOperatorNode(identifier, new OperatorToken(OperatorToken.OPERATOR.DEFINE, CurrentToken.Position), get_instruction_list_node(STRUCTURES.DEFINITION)));
                            case KeywordToken.KEYWORD.ENUMERATION:
                                return new UnaryOperatorNode(keyword_token, new BinaryOperatorNode(identifier, new OperatorToken(OperatorToken.OPERATOR.DEFINE, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, "{", "}", () => get_expression(), out TokenPosition position), position)));
                        }
                        break;
                    }
                }
                break;
            case DataToken data_token:
                switch (data_token.Type) {
                    case DataToken.TYPE.WORD:
                        if ((Index + 1) < Tokens.Count) {
                            switch (Tokens[Index + 1]) {
                                case DataToken:
                                    return get_varaible_or_function_structure(data_token, allowed_structures);
                            }
                        }
                        break;
                }
                if (!STRUCTURES.IN(STRUCTURES.STATEMENT, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                return get_expression();
        }
        return get_expression();
    }
    private OperandInstructionNode get_instruction_list_node(int allow_expression) {
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, out SymbolToken start)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        List<Node> instructions = get_nodes(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET, allow_expression);
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, start.Position + CurrentToken.Position, "Expected '}'");
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        return new OperandInstructionNode(instructions, start.Position + PreviousToken.Position);
    }
    private List<Node> get_nodes(SymbolToken.SYMBOL breaker, int allowed_structures) {
        List<Node> nodes = new List<Node>();
        while (!CurrentToken.is_symbol(breaker)) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            if (CurrentToken.is_symbol(breaker) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END)) break;
            switch (CurrentToken) {
                case KeywordToken keyword_token:
                    switch (keyword_token.Type) {
                        case KeywordToken.TYPE.DECISION:
                            if (!STRUCTURES.IN(STRUCTURES.STATEMENT, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
                                case KeywordToken.KEYWORD.CASE: case KeywordToken.KEYWORD.DEFAULT:
                                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected match");
                                case KeywordToken.KEYWORD.MATCH: {
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
                                        DataNode case_expression = new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_expression(), out TokenPosition position), position);
                                        if ((case_expression.Data as List<Node>).Count == 0) throw new ParserError(ParserError.TYPE.EMPTHY_CASE, case_token.Position);
                                        cases.Add(new MatchNode.CaseNode(case_expression, get_instruction_list_node(STRUCTURES.VARIABLE_DEFINITION + STRUCTURES.STATEMENT + STRUCTURES.FLOW_CONTROL), case_token.Position + PreviousToken.Position));
                                    }
                                    OperandInstructionNode default_node = null;
                                    if (CurrentToken.is_keyword(KeywordToken.KEYWORD.DEFAULT)) {
                                        next_token();
                                        default_node = get_instruction_list_node(STRUCTURES.VARIABLE_DEFINITION + STRUCTURES.STATEMENT +  + STRUCTURES.FLOW_CONTROL);
                                    }
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, keyword_token.Position + PreviousToken.Position, "Expected '{'");
                                    next_token();
                                    nodes.Add(new MatchNode(expression, cases, default_node, keyword_token.Position + PreviousToken.Position));
                                    break;
                                }
                                case KeywordToken.KEYWORD.IF: {
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                    Node if_expression = get_operand_node();
                                    OperandInstructionNode implication = get_instruction_list_node(STRUCTURES.VARIABLE_DEFINITION + STRUCTURES.STATEMENT + STRUCTURES.FLOW_CONTROL);
                                    List<IFNode.ElseIFNode> children = new List<IFNode.ElseIFNode>();
                                    while (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE_IF)) {
                                        KeywordToken else_if_token = CurrentToken as KeywordToken;
                                        next_token();
                                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                        children.Add(new IFNode.ElseIFNode(get_operand_node(), get_instruction_list_node(STRUCTURES.VARIABLE_DEFINITION + STRUCTURES.STATEMENT + STRUCTURES.FLOW_CONTROL), else_if_token.Position + PreviousToken.Position));
                                    }
                                    OperandInstructionNode inverse = null;
                                    if (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE)) {
                                        next_token();
                                        inverse = get_instruction_list_node(STRUCTURES.VARIABLE_DEFINITION + STRUCTURES.STATEMENT + STRUCTURES.FLOW_CONTROL);
                                    }
                                    nodes.Add(new IFNode(if_expression, implication, children, inverse, keyword_token.Position + PreviousToken.Position));
                                    break;
                                }
                            }
                            break;
                        case KeywordToken.TYPE.FLOWCONTROL:
                            if (!STRUCTURES.IN(STRUCTURES.FLOW_CONTROL, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.RETURN:
                                    next_token();
                                    nodes.Add(new FlowControlNode(get_expression(), keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.BREAK:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.BREAK, keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.CONTINUE:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.CONTINUE, keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.BREAKPOINT:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.BREAKPOINT, keyword_token.Position + PreviousToken.Position));
                                    break;
                            }
                            break;
                        case KeywordToken.TYPE.LOOP:
                            if (!STRUCTURES.IN(STRUCTURES.STATEMENT, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.FOR: {
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                    next_token();
                                    if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                    Node iterator = get_operand_node();
                                    if (!CurrentToken.is_operator(OperatorToken.OPERATOR.IN))  throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "'in'");
                                    OperatorToken In = CurrentToken as OperatorToken;
                                    next_token();
                                    Node iteratable = get_operand_node();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected ')'");
                                    next_token();
                                    OperandInstructionNode instructions = get_instruction_list_node(STRUCTURES.VARIABLE_DEFINITION + STRUCTURES.STATEMENT + STRUCTURES.FLOW_CONTROL);
                                    nodes.Add(new ForNode(iterator, iteratable, instructions, keyword_token.Position + PreviousToken.Position));
                                    break;
                                }
                                case KeywordToken.KEYWORD.WHILE: {
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                    Node expression = get_operand_node();
                                    OperandInstructionNode instructions = get_instruction_list_node(STRUCTURES.VARIABLE_DEFINITION + STRUCTURES.STATEMENT + STRUCTURES.FLOW_CONTROL);
                                    nodes.Add(new WhileNode(expression, instructions, keyword_token.Position + PreviousToken.Position));
                                    break;
                                }
                            }
                            break;
                        default:
                            nodes.Add(get_definition_structure(allowed_structures));
                            break;
                    }
                    break;
                default:
                    nodes.Add(get_definition_structure(allowed_structures));
                    break;
            }
        }
        return nodes;
    }
    
}
*/