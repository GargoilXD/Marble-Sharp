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
    private List<Node> Parsed;
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
        else {
            CurrentToken = null;
        }
    }
    private Node get_bracket_node(SymbolToken symbol_token) {
        switch (symbol_token.Symbol) {
            case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET:
                next_token();
                Node expression = get_expression();
                if (!(CurrentToken is SymbolToken && (CurrentToken as SymbolToken).Symbol == SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position, "Expected ')'");
                next_token();
                return expression;
            case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET:
                next_token();
                Dictionary<Node, Node> mapping = new Dictionary<Node, Node>();
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
                    mapping.Add(key, get_operand_node());
                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
                    next_token();
                }
                while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position);
                next_token();
                return new DataNode(DataNode.TYPE.DICTIONARY, mapping, symbol_token.Position + PreviousToken.Position);
            case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET:
                next_token();
                List<Node> elements = new List<Node>();
                while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
                    while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                    elements.Add(get_expression());
                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
                    next_token();
                }
                while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, symbol_token.Position + PreviousToken.Position, "Expected ']'");
                next_token();
                return new DataNode(DataNode.TYPE.LIST, elements, symbol_token.Position + PreviousToken.Position);
            case SymbolToken.SYMBOL.END: case SymbolToken.SYMBOL.END_OF_LINE:
                throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, PreviousToken.Position);
            default:
                throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
            }
    }
    private Node get_operand_node() {
        switch (CurrentToken) {
            case DataToken data_token:
                next_token();
                if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) return get_function_node(data_token);
                return DataNode.FromToken(data_token);
            case SymbolToken symbol_token:
                switch (symbol_token.Symbol) {
                    case SymbolToken.SYMBOL.END: case SymbolToken.SYMBOL.END_OF_LINE:
                        throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
                    case SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET: case SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET: case SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET:
                        throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
                default:
                    return get_bracket_node(symbol_token);
                }
            default:
                throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
        }
    }
    private Node get_unary_node() {
        switch (CurrentToken) {
            case KeywordToken keyword_token:
                switch (keyword_token.Keyword) {
                    case KeywordToken.KEYWORD.CONST:
                    case KeywordToken.KEYWORD.STATIC:
                    case KeywordToken.KEYWORD.PUBLIC:
                    case KeywordToken.KEYWORD.PRIVATE:
                    case KeywordToken.KEYWORD.MATCH:
                    case KeywordToken.KEYWORD.CASE:
                    case KeywordToken.KEYWORD.DEFAULT:
                    case KeywordToken.KEYWORD.CLASS:
                    case KeywordToken.KEYWORD.OBJECT:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);

                    case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.INTEGER: case KeywordToken.KEYWORD.BOOLEAN: case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST: case KeywordToken.KEYWORD.DICTIONARY: case KeywordToken.KEYWORD.ENUMERATION:
                        next_token();
                        switch (CurrentToken) {
                            case DataToken: {
                                UnaryOperatorNode unary = new UnaryOperatorNode(keyword_token, get_operand_node());
                                if (keyword_token.Keyword == KeywordToken.KEYWORD.ENUMERATION) {
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
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
                                    if (enumerations.Count == 0) throw new ParserError(ParserError.TYPE.EMPTHY_ENUMERATION, unary.Position + PreviousToken.Position);
                                    return new EnumerationDefinitionNode(unary, enumerations, unary.Position + PreviousToken.Position);
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

                    case KeywordToken.KEYWORD.RETURN:
                        next_token();
                        return new UnaryOperatorNode(keyword_token, get_expression());

                    case KeywordToken.KEYWORD.BREAK: case KeywordToken.KEYWORD.CONTINUE: case KeywordToken.KEYWORD.BREAKPOINT:
                        next_token();
                        return KeywordNode.FromToken(keyword_token);

                    case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
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
                        return new IFNode(if_expression, implication, children, inverse, keyword_token.Position + PreviousToken.Position);
                    }
                    case KeywordToken.KEYWORD.FOR: {
                        next_token();
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'('");
                        next_token();
                        Node iterator_statement = get_statement();
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "')'");
                        next_token();
                        UnaryOperatorNode unary = new UnaryOperatorNode(keyword_token, iterator_statement);
                        return new BinaryOperatorNode(unary, new OperatorToken(OperatorToken.OPERATOR.RUNS, unary.Position),  get_instruction_list_node());
                    }
                    case KeywordToken.KEYWORD.WHILE: {
                        next_token();
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'('");
                        Node expression = get_expression();
                        UnaryOperatorNode unary = new UnaryOperatorNode(keyword_token, expression);
                        return new BinaryOperatorNode(unary, new OperatorToken(OperatorToken.OPERATOR.RUNS, unary.Position),  get_instruction_list_node());
                    }
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
                        return new FunctionDefinitionNode(function_identifier, arguments, get_instruction_list_node(), function_identifier.Position + PreviousToken.Position);
                    }
                    case KeywordToken.KEYWORD.PRINT: case KeywordToken.KEYWORD.RANGE: case KeywordToken.KEYWORD.ASSERT: case KeywordToken.KEYWORD.RANDOM: case KeywordToken.KEYWORD.INPUT:
                        next_token();
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'('");
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
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'{'");
        SymbolToken symbol_token = CurrentToken as SymbolToken;
        List<Node> instructions = new List<Node>();
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            instructions.Add(get_statement());
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        }
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, symbol_token.Position + PreviousToken.Position);
        next_token();
        return new InstructionListNode(instructions, symbol_token.Position + PreviousToken.Position);
    }
    private FunctionNode get_function_node(Token identifier) {
        next_token();
        List<Node> arguments = new List<Node>();
        while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            arguments.Add(get_expression());
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
            next_token();
        }
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, identifier.Position + PreviousToken.Position);
        next_token();
        return new FunctionNode(identifier, arguments, identifier.Position + PreviousToken.Position);
    }
    private Node get_binary_node(Func<Node> function = null, OperatorToken.OPERATOR[] precedence = null) {
        Node left = function();
        while (CurrentToken is OperatorToken && precedence.Contains(((OperatorToken) CurrentToken).Type)) {
            OperatorToken operatorToken = (OperatorToken) CurrentToken;
            next_token();
            Node right = function();
            left = new BinaryOperatorNode(left, operatorToken, right);
        }
        return left;
    }
    public List<Node> Parse(List<Token> tokens) {
        Tokens = tokens;
        Index = 0;
        CurrentToken = tokens[0];
        Parsed = new List<Node>();
        while (CurrentToken != null) {
            switch (CurrentToken) {
                case SymbolToken symbol_token:
                    switch (symbol_token.Symbol) {
                        case SymbolToken.SYMBOL.END_OF_LINE: case SymbolToken.SYMBOL.END:
                            next_token();
                            break;
                        default:
                            throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                    }
                    break;
                
                default:
                    Parsed.Add(get_statement());
                    break;
            }
        }
        return Parsed;
    }
}
