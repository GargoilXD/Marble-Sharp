using System;
using System.Collections.Generic;
using System.Linq;
public class Parser {
    private static readonly OperatorToken.OPERATOR[][] PRECEDENCE = new OperatorToken.OPERATOR[][]{
        new[] { OperatorToken.OPERATOR.DOT },
        new[] { OperatorToken.OPERATOR.IS, OperatorToken.OPERATOR.EXTENDS },
        new[] { OperatorToken.OPERATOR.EXPONENT },
        new[] { OperatorToken.OPERATOR.MODOLUS },
        new[] { OperatorToken.OPERATOR.MULTIPLY, OperatorToken.OPERATOR.DIVIDE },
        new[] { OperatorToken.OPERATOR.ADD, OperatorToken.OPERATOR.SUBTRACT },
        new[] { OperatorToken.OPERATOR.IN },
        new[] { OperatorToken.OPERATOR.EQUALS, OperatorToken.OPERATOR.NOT_EQUALS, OperatorToken.OPERATOR.LESSER_THAN, OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS, OperatorToken.OPERATOR.GREATER_THAN, OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS },
        new[] { OperatorToken.OPERATOR.AND, OperatorToken.OPERATOR.OR },
        new[] { OperatorToken.OPERATOR.COLON },
        new[] { OperatorToken.OPERATOR.ASSIGN, OperatorToken.OPERATOR.ADD_AND_ASSIGN, OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN, OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN, OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN, OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN, OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN },
    };
    private int Index;
    private Token PreviousToken;
    private Token CurrentToken;
    private List<Token> Tokens;
    private Func<Node> get_statement => (
        () => get_binary_node(
            () => get_binary_node(
                () => get_binary_node(
                    () => get_binary_node(
                        () => get_binary_node(
                            () => get_binary_node(
                                () => get_binary_node(
                                    () => get_binary_node(
                                        () => get_binary_node(
                                            () => get_binary_node(
                                                () => get_binary_node(
                                                    () => get_unary_node(),
                                                PRECEDENCE[0]),
                                            PRECEDENCE[1]),
                                        PRECEDENCE[2]),
                                    PRECEDENCE[3]),
                                PRECEDENCE[4]),
                            PRECEDENCE[5]),
                        PRECEDENCE[6]),
                    PRECEDENCE[7]),
                PRECEDENCE[8]),
            PRECEDENCE[9]),
        PRECEDENCE[10])
    );
    private Func<Node> get_expression => (
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_operand_node(),
        PRECEDENCE[0]),
        PRECEDENCE[1]),
        PRECEDENCE[2]),
        PRECEDENCE[3]),
        PRECEDENCE[4]),
        PRECEDENCE[5]),
        PRECEDENCE[6]),
        PRECEDENCE[7]),
        PRECEDENCE[8]),
        PRECEDENCE[9])
    );
    private void next_token() {
        PreviousToken = CurrentToken;
        Index += 1;
        if (Index < Tokens.Count) {
            CurrentToken = Tokens[Index];
        }
    }
    private List<Node> get_bracket_nodes(SymbolToken.SYMBOL bracket_symbol, string start_error, string end_error, Func<Node> getter) {
        if (!CurrentToken.is_symbol(bracket_symbol)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, $"'{start_error}'");
        Token start = CurrentToken;
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
                if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) return get_function_node(data_token);
                return DataNode.FromToken(data_token);
            case SymbolToken symbol_token:
                switch (symbol_token.Symbol) {
                    case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET:
                        next_token();
                        Node expression = get_expression();
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position, "Expected ')'");
                        next_token();
                        return expression;
                    case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET:
                        next_token();
                        Dictionary<Node, Node> dictionary = new Dictionary<Node, Node>();
                        while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
                            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                            Node key = get_operand_node();
                            switch (key) {
                                case DataNode data_node:
                                    switch (data_node.Type) {
                                        case DataNode.TYPE.OBJECT: case DataNode.TYPE.VARIANT: case DataNode.TYPE.IDENTIFIER: case DataNode.TYPE.LIST: case DataNode.TYPE.TUPLE: case DataNode.TYPE.DICTIONARY:
                                            throw new Error(key.Position, "Key should not be dynamic");
                                    }
                                    break;
                                default:
                                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, key.Position, "Expected data key");
                            }                    
                            if (!CurrentToken.is_operator(OperatorToken.OPERATOR.COLON)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "':'");
                            next_token();
                            dictionary.Add(key, get_operand_node());
                            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
                            next_token();
                        }
                        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position);
                        next_token();
                        return new DataNode(DataNode.TYPE.DICTIONARY, dictionary, symbol_token.Position + PreviousToken.Position);
                    case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET:
                        return new DataNode(DataNode.TYPE.LIST, get_bracket_nodes(symbol_token.Symbol, "[", "]", get_expression), symbol_token.Position + PreviousToken.Position);
                    case SymbolToken.SYMBOL.END: case SymbolToken.SYMBOL.END_OF_LINE:
                        throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
                    case SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET: case SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET: case SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET:
                        throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
                    default:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                }
            default:
                throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
        }
    }
    private Node get_unary_node() {
        switch (CurrentToken) {
            case KeywordToken keyword_token:
                switch (keyword_token.Keyword) {
                    case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.INTEGER: case KeywordToken.KEYWORD.BOOLEAN: case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST: case KeywordToken.KEYWORD.DICTIONARY: case KeywordToken.KEYWORD.ENUMERATION:
                        next_token();
                        switch (CurrentToken) {
                            case DataToken: {
                                UnaryOperatorNode unary = new UnaryOperatorNode(keyword_token, get_operand_node());
                                if (keyword_token.Keyword == KeywordToken.KEYWORD.ENUMERATION) {
                                    
                                    /*if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
                                    next_token();
                                    List<Node> enumerations = new List<Node>();
                                    while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
                                        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                                        Node operand = get_operand_node();
                                        if (!(operand is DataNode && (operand as DataNode).Type == DataNode.TYPE.IDENTIFIER)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, operand.Position);
                                        enumerations.Add(operand);
                                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
                                        next_token();
                                    }
                                    while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, unary.Position + PreviousToken.Position, "Expected '}'");
                                    next_token();
                                    if (enumerations.Count == 0) throw new ParserError(ParserError.TYPE.EMPTHY_ENUMERATION, unary.Position + PreviousToken.Position);*/
                                    return new EnumerationDefinitionNode(unary, get_bracket_nodes(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, "{", "}", delegate {
                                        if (CurrentToken.is_data(DataToken.TYPE.IDENTIFIER)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                        return DataNode.FromToken(CurrentToken as DataToken);
                                    }), unary.Position + PreviousToken.Position);
                                }
                                return unary;
                            }
                            case SymbolToken symbol_token:
                                switch (symbol_token.Symbol) {
                                    case SymbolToken.SYMBOL.END: case SymbolToken.SYMBOL.END_OF_LINE:
                                        break;
                                    case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET:
                                        return get_function_node(keyword_token);
                                    default:
                                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                }
                                break;
                        }
                        return KeywordNode.FromToken(keyword_token);

                    case KeywordToken.KEYWORD.PRINT: case KeywordToken.KEYWORD.RANGE: case KeywordToken.KEYWORD.ASSERT: case KeywordToken.KEYWORD.RANDOM: case KeywordToken.KEYWORD.INPUT:
                        next_token();
                        return get_function_node(keyword_token);

                    default:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                }
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.NOT: case OperatorToken.OPERATOR.ADD: case OperatorToken.OPERATOR.SUBTRACT:
                        next_token();
                        return new UnaryOperatorNode(operator_token, get_operand_node());
                    default:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_OPERATOR, CurrentToken.Position, "Unexpected unary operator");
                }
            default:
                return get_operand_node();
        }
    }
    private InstructionListNode get_instruction_list_node() {
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
        Token start = CurrentToken;
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        List<Node> instructions = get_nodes(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET);
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position, "Expected '}'");
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        return new InstructionListNode(instructions, start.Position + PreviousToken.Position);
    }
    private FunctionNode get_function_node(Token identifier) {
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'('");
        next_token();
        List<Node> arguments = new List<Node>();
        while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            arguments.Add(get_expression());
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
            next_token();
        }
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position);
        next_token();
        return new FunctionNode(identifier, arguments, identifier.Position + PreviousToken.Position);
    }
    private Node get_binary_node(Func<Node> function = null, OperatorToken.OPERATOR[] precedence = null) {
        Node left = function();
        if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET)){
            Token start = CurrentToken;
            next_token();
            List<Node> elements = new List<Node>();
            while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
                while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                elements.Add(get_expression());
                if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
                next_token();
            }
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, start.Position + PreviousToken.Position, "Expected ']'");
            next_token();
            left = new BinaryOperatorNode(left, new OperatorToken(OperatorToken.OPERATOR.ACCESSOR, start.Position + PreviousToken.Position), new DataNode(DataNode.TYPE.LIST, elements, start.Position + PreviousToken.Position));
        }
        while (CurrentToken is OperatorToken && precedence.Contains(((OperatorToken) CurrentToken).Type)) {
            OperatorToken operatorToken = (OperatorToken) CurrentToken;
            next_token();
            Node right = function();
            left = new BinaryOperatorNode(left, operatorToken, right);
        }
        return left;
    }
    private List<Node> get_nodes(SymbolToken.SYMBOL breaker) {
        List<Node> nodes = new List<Node>();
        while (!CurrentToken.is_symbol(breaker)) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            if (CurrentToken.is_symbol(breaker)) break;
            switch (CurrentToken) {
                case KeywordToken keyword_token:
                    switch (keyword_token.Type) {
                        case KeywordToken.TYPE.DATATYPE:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.OBJECT:
                                    throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                                default:
                                    nodes.Add(get_statement());
                                    break;
                            }
                            break;
                        case KeywordToken.TYPE.DECISION:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE: case KeywordToken.KEYWORD.CASE: case KeywordToken.KEYWORD.DEFAULT:
                                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                case KeywordToken.KEYWORD.IF: {
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'('");
                                    Node if_expression = get_expression();

                                    InstructionListNode implication = get_instruction_list_node();
                                    List<IFNode> children = new List<IFNode>();
                                    InstructionListNode inverse = null;
                                    while (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE_IF)) {
                                        KeywordToken else_if_token = CurrentToken as KeywordToken;
                                        next_token();
                                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'('");
                                        children.Add(new IFNode(get_expression(), get_instruction_list_node(), null, null, else_if_token.Position + PreviousToken.Position));
                                    }
                                    if (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE)) {
                                        next_token();
                                        inverse = get_instruction_list_node();
                                    }
                                    nodes.Add(new IFNode(if_expression, implication, children, inverse, keyword_token.Position + PreviousToken.Position));
                                    break;
                                }
                                case KeywordToken.KEYWORD.MATCH:
                                    throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                            }
                            break;
                        case KeywordToken.TYPE.FLOWCONTROL:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.RETURN:
                                    next_token();
                                    nodes.Add(new UnaryOperatorNode(keyword_token, get_expression()));
                                    break;
                                case KeywordToken.KEYWORD.BREAK: case KeywordToken.KEYWORD.CONTINUE: case KeywordToken.KEYWORD.BREAKPOINT:
                                    next_token();
                                    nodes.Add(KeywordNode.FromToken(keyword_token));
                                    break;
                            }
                            break;
                        case KeywordToken.TYPE.LOOP:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.FOR: {
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'('");
                                    next_token();
                                    Node iterator_statement = get_statement();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "')'");
                                    next_token();
                                    UnaryOperatorNode unary = new UnaryOperatorNode(keyword_token, iterator_statement);
                                    nodes.Add(new BinaryOperatorNode(unary, new OperatorToken(OperatorToken.OPERATOR.RUNS, unary.Position),  get_instruction_list_node()));
                                    break;
                                }
                                case KeywordToken.KEYWORD.WHILE: {
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'('");
                                    Node expression = get_expression();
                                    UnaryOperatorNode unary = new UnaryOperatorNode(keyword_token, expression);
                                    nodes.Add(new BinaryOperatorNode(unary, new OperatorToken(OperatorToken.OPERATOR.RUNS, unary.Position),  get_instruction_list_node()));
                                    break;
                                }
                            }
                            break;
                        case KeywordToken.TYPE.MODIFIER:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.CONST: case KeywordToken.KEYWORD.STATIC: case KeywordToken.KEYWORD.PUBLIC: case KeywordToken.KEYWORD.PRIVATE:
                                    throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                            }
                            break;
                        case KeywordToken.TYPE.DEFINITION:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.FUNCTION: {
                                    next_token();
                                    if (!CurrentToken.is_data(DataToken.TYPE.IDENTIFIER)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                    DataToken function_identifier = CurrentToken as DataToken;
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                    next_token();
                                    List<Node> arguments = new List<Node>();
                                    while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
                                        if (!((CurrentToken is KeywordToken) && (CurrentToken as KeywordToken).Type == KeywordToken.TYPE.DATATYPE)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Datatype");
                                        KeywordToken datatype = CurrentToken as KeywordToken;
                                        next_token();
                                        if (!CurrentToken.is_data(DataToken.TYPE.IDENTIFIER)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
                                        DataNode identifier = DataNode.FromToken(CurrentToken as DataToken);
                                        next_token();
                                        arguments.Add(new UnaryOperatorNode(datatype, identifier));
                                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
                                        next_token();
                                    }
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, function_identifier.Position + PreviousToken.Position, "Expected ')'");
                                    next_token();
                                    nodes.Add(new FunctionDefinitionNode(function_identifier, arguments, get_instruction_list_node(), function_identifier.Position + PreviousToken.Position));
                                    break;
                                }
                                case KeywordToken.KEYWORD.CLASS:
                                    throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                            }
                            break;
                        case KeywordToken.TYPE.INBUILT_FUNCTION:
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.PRINT: case KeywordToken.KEYWORD.RANGE: case KeywordToken.KEYWORD.ASSERT: case KeywordToken.KEYWORD.RANDOM: case KeywordToken.KEYWORD.INPUT:
                                nodes.Add(get_statement());
                                break;
                        }
                        break;
                    }
                break;
                case DataToken data_token:
                    switch (data_token.Type) {
                        case DataToken.TYPE.OBJECT:
                            throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                        case DataToken.TYPE.IDENTIFIER:
                            nodes.Add(get_statement());
                            break;
                        default:
                            throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                    }
                    break;
                default:
                    nodes.Add(get_statement());
                    break;
            }
        }
        return nodes;
    }
    public List<Node> Parse(List<Token> tokens) {
        Tokens = tokens;
        Index = 0;
        CurrentToken = tokens[0];
        return get_nodes(SymbolToken.SYMBOL.END);
    }
}
