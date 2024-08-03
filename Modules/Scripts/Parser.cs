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
    private List<Vertex> Parsed;
    private Func<Vertex> get_statement => (
        () => get_binary_vertex(
            () => get_binary_vertex(
                () => get_binary_vertex(
                    () => get_binary_vertex(
                        () => get_binary_vertex(
                            () => get_binary_vertex(
                                () => get_binary_vertex(
                                    () => get_binary_vertex(
                                        () => get_binary_vertex(
                                            () => get_binary_vertex(
                                                () => get_binary_vertex(
                                                    () => get_unary_vertex(),
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
    private Func<Vertex> get_expression => (
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_binary_vertex(
        () => get_operand_vertex(),
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
/*
    private Vertex get_instructions_vertex(){
        Func<bool> helper = () =>{
            while (CurrentToken.Type == Token.TYPE.END_OF_LINE) next_token();
            return CurrentToken.Type == Token.TYPE.RIGHT_CURLY_BRACKET;
        };

        TokenPosition position = CurrentToken.Position;
        List<Vertex> instructions = new List<Vertex>();
        next_token();
        while (CurrentToken.Type != Token.TYPE.RIGHT_CURLY_BRACKET){
            if (helper()){
                break;
            }
            instructions.Add(get_statement());
            if (helper()){
                break;
            }
        }
        if (CurrentToken.Type == Token.TYPE.RIGHT_CURLY_BRACKET){
            position.Extend(CurrentToken.Position);
            next_token();
            return new DataVertex(DataToken.DATATYPE.INSTRUCTIONS, position, new Dictionary<string, object> { { "instructions", instructions } });
        }
        throw new ParserError(ParserError.TYPE.UNEXPECTED_OPERAND, CurrentToken.Position);
    }
*/
    private Vertex get_bracket_vertex(SymbolToken symbol_token) {
        switch (symbol_token.Symbol) {
            case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET:
                next_token();
                Vertex expression = get_expression();
                brackets_closed(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET);
                next_token();
                return expression;
            case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET:
                next_token();
                Dictionary<Vertex, Vertex> mapping = new Dictionary<Vertex, Vertex>();
                while (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) {
                    while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                    if (CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) break;
                    Vertex key = get_operand_vertex();
                    switch (key) {
                        case DataVertex data_vertex:
                            switch (data_vertex.Type) {
                                case DataVertex.TYPE.OBJECT: case DataVertex.TYPE.VARIANT: case DataVertex.TYPE.IDENTIFIER: case DataVertex.TYPE.LIST: case DataVertex.TYPE.TUPLE: case DataVertex.TYPE.DICTIONARY:
                                    throw new Error(key.Position, "Key should not be dynamic");
                            }
                            break;
                        default:
                            throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, key.Position, "Expected Data");
                    }                    
                    if (!CurrentToken.is_operator(OperatorToken.OPERATOR.COLON)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "':'");
                    next_token();
                    mapping.Add(key, get_operand_vertex());
                    while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                    if (CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) break;
                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, PreviousToken.Position, "','");
                    next_token();
                }
                brackets_closed(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET);
                next_token();
                return new DataVertex(DataVertex.TYPE.DICTIONARY, mapping, symbol_token.Position + PreviousToken.Position);
            case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET:
                next_token();
                List<Vertex> elements = new List<Vertex>();
                while (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) {
                    while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                    if (CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET)) break;
                    elements.Add(get_expression());
                    while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                    if (CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET)) break;
                    if (CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) {
                        next_token();
                    } else {
                        throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, PreviousToken.Position, "','");
                    }
                }
                brackets_closed(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET);
                next_token();
                return new DataVertex(DataVertex.TYPE.LIST, elements, symbol_token.Position + PreviousToken.Position);
            case SymbolToken.SYMBOL.END: case SymbolToken.SYMBOL.END_OF_LINE:
                throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, PreviousToken.Position);
            default:
                throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
            }
    }
    private void brackets_closed(SymbolToken.SYMBOL bracket) {
        if (CurrentToken is not SymbolToken) {
            throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position);
        }
        if ((CurrentToken as SymbolToken).Symbol != bracket) {
            throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position);
        }
    }
    private Vertex get_operand_vertex() {
        switch (CurrentToken) {
            case DataToken data_token:
                next_token();
                return DataVertex.FromToken(data_token);
            case SymbolToken symbol_token:
                return get_bracket_vertex(symbol_token);
            default:
                throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position, "Expected Operand");
        }
    }
    private Vertex get_unary_vertex() {
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
                    case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.INTEGER: case KeywordToken.KEYWORD.BOOLEAN: case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST: case KeywordToken.KEYWORD.DICTIONARY:
                        next_token();
                        switch (CurrentToken) {
                            case DataToken:
                                return new UnaryOperatorVertex(keyword_token, get_operand_vertex());
                            case SymbolToken symbol_token:
                                switch (symbol_token.Symbol) {
                                    case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET:
                                        //return get_function_vertex(keyword);
                                        break;
                                    case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET:
                                    case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET:
                                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                }
                                break;
                        }
                        return KeywordVertex.FromToken(keyword_token);
                    case KeywordToken.KEYWORD.ENUMERATION:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                        /*
                        next_token();
                        if (CurrentToken is DataToken) {
                            Vertex operand = get_operand_vertex();
                            if ((CurrentToken as SymbolToken).Symbol != SymbolToken.SYMBOL.LEFT_CURLY_BRACKET){
                                throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                            }
                            //return new BinaryOperatorVertex(new UnaryOperatorVertex(keywordToken, operand), new OperatorToken(OperatorToken.OPERATOR.ASSIGN), get_bracket_data_vertex());
                            break;
                        }
                        else if ((CurrentToken as SymbolToken).Symbol == SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET) {
                            //return get_function_vertex(keywordToken);
                        }
                        return KeywordVertex.FromToken(keyword_token);*/
                    case KeywordToken.KEYWORD.RETURN: case KeywordToken.KEYWORD.BREAK: case KeywordToken.KEYWORD.CONTINUE: case KeywordToken.KEYWORD.BREAKPOINT:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                        /*
                        next_token();
                        switch (CurrentToken) {
                            case SymbolToken symbol_token:
                                switch (symbol_token.Symbol) {
                                    case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET: case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET: case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET:
                                        goto Yes;
                                }
                                break;
                            case DataToken data_token:
                                goto Yes;
                            default:
                                if (keyword_token.Keyword == KeywordToken.KEYWORD.RETURN) {
                                    throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
                                }
                                break;
                        }
                        Yes:
                            if (keyword_token.Keyword != KeywordToken.KEYWORD.RETURN) {
                                throw new ParserError(ParserError.TYPE.UNEXPECTED_OPERAND, CurrentToken.Position);
                            }
                            //Vertex expression = get_statement(get_operand_vertex(get_operand_vertex));
                            //return new UnaryOperatorVertex(keywordToken, expression);
                        return KeywordVertex.FromToken(keyword_token);*/
                    case KeywordToken.KEYWORD.IF:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                    case KeywordToken.KEYWORD.FOR:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                    case KeywordToken.KEYWORD.WHILE:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                        /*
                        next_token();
                        Vertex expression = ((string) keywordToken.Token_value == "For")? get_binary_vertex(get_unary_vertex, PRECEDENCE[6]) : get_statement(get_operand_vertex(get_operand_vertex));
                        if (CurrentToken.Type != Token.TYPE.LEFT_CURLY_BRACKET){
                            throw new ParserError(ParserError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'{'");
                        }
                        return new BinaryOperatorVertex(new UnaryOperatorVertex(keywordToken, expression), new OperatorToken(OperatorToken.OPERATOR.RUNS), get_instructions_vertex());
                        */
                    case KeywordToken.KEYWORD.FUNCTION:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                    case KeywordToken.KEYWORD.PRINT: case KeywordToken.KEYWORD.RANGE: case KeywordToken.KEYWORD.ASSERT: case KeywordToken.KEYWORD.RANDOM: case KeywordToken.KEYWORD.INPUT:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                        /*
                        next_token();
                        if ((CurrentToken as SymbolToken).Symbol == SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET) {
                            //return get_function_vertex(keyword)
                        }
                        else {
                            throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                        }
                        break;
                    default:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);*/
                    default:
                        throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, keyword_token.Position);
                }
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.NOT:
                    case OperatorToken.OPERATOR.ADD:
                    case OperatorToken.OPERATOR.SUBTRACT:
                        next_token();
                        return new UnaryOperatorVertex(operator_token, get_operand_vertex());
                    default:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_OPERAND, CurrentToken.Position, "Unexpected Unary Operand");
                }
            default:
                return get_operand_vertex();
        }
    }
    private Vertex get_binary_vertex(Func<Vertex> function = null, OperatorToken.OPERATOR[] precedence = null) {
        Vertex left = function();
        while (CurrentToken is OperatorToken && precedence.Contains(((OperatorToken) CurrentToken).Type)) {
            OperatorToken operatorToken = (OperatorToken) CurrentToken;
            next_token();
            Vertex right = function();
            left = new BinaryOperatorVertex(left, operatorToken, right);
        }
        return left;
    }
    public List<Vertex> Parse(List<Token> tokens) {
        Tokens = tokens;
        Index = 0;
        CurrentToken = tokens[0];
        Parsed = new List<Vertex>();
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
