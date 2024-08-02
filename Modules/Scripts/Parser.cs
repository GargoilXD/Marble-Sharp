using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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
    private Token CurrentToken;
    private List<Token> Tokens;
    private List<Vertex> Parsed;

    private Func<Vertex> get_statement => () => get_binary_vertex(
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
                PRECEDENCE[10]))
    );
    
    private void next_token() {
        Index += 1;
        if (Index < Tokens.Count) {
            CurrentToken = Tokens[Index];
        }
        else {
            CurrentToken = null;
        }
    }



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
        throw new MarbleError(MarbleError.TYPE.UNEXPECTED_OPERAND, CurrentToken.Position);
    }
    private Vertex get_operand_vertex(Func<Vertex> function = null) {
        if (CurrentToken is DataToken){
            DataVertex operand = DataVertex.FromToken(CurrentToken as DataToken);
            next_token();
            if (CurrentToken.Type == Token.TYPE.LEFT_CIRCLE_BRACKET){
                //return get_function_vertex(operand);
            }
            return operand;
        }
        else {
            switch (CurrentToken.Type) {
                case Token.TYPE.LEFT_CIRCLE_BRACKET:
                    next_token();
                    var expression = get_statement();
                    if (CurrentToken.Type != Token.TYPE.RIGHT_CIRCLE_BRACKET) {
                        throw new MarbleError(MarbleError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                    }
                    next_token();
                    return expression;
                case Token.TYPE.LEFT_CURLY_BRACKET:
                case Token.TYPE.LEFT_SQUARE_BRACKET:
                    break;
                    //return get_bracket_data_vertex();
            }
        }
        throw new MarbleError(MarbleError.TYPE.EXPECTED_OPERAND, CurrentToken.Position, "Expected Operand");
    }
    private Vertex get_unary_vertex() {
        switch (CurrentToken) {
            case KeywordToken keywordToken:
                switch (keywordToken.Token_value) {
                    case "Const":
                    case "Static":
                    case "Public":
                    case "Private":
                    case "Void":
                    case "Match":
                    case "Case":
                    case "Default":
                    case "Class":
                        throw new MarbleError(MarbleError.TYPE.MESSAGE, CurrentToken.Position, "UnImplemented Keyword");
                    case DataToken.DATATYPE.VARIANT:
                    case DataToken.DATATYPE.BOOLEAN:
                    case DataToken.DATATYPE.INTEGER:
                    case DataToken.DATATYPE.FLOAT:
                    case DataToken.DATATYPE.STRING:
                    case DataToken.DATATYPE.LIST:
                    case DataToken.DATATYPE.DICTIONARY:
                        next_token();
                        switch (CurrentToken) {
                            case DataToken data_token:
                                Vertex operand = get_operand_vertex(get_unary_vertex);
                                //return new UnaryOperatorVertex(keywordToken, operand);
                                break;
                            case Token token:
                                switch (token.Type) {
                                    case Token.TYPE.LEFT_CIRCLE_BRACKET:
                                        //return get_function_vertex(keyword);
                                        break;
                                    case Token.TYPE.LEFT_CURLY_BRACKET:
                                    case Token.TYPE.LEFT_SQUARE_BRACKET:
                                        throw new MarbleError(MarbleError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);

                                }
                                break;
                        }
                        return KeywordVertex.FromToken(keywordToken);
                    case DataToken.DATATYPE.ENUMERATION:
                        next_token();
                        if (CurrentToken is DataToken) {
                            Vertex operand = get_operand_vertex(get_unary_vertex);
                            if (CurrentToken.Type != Token.TYPE.LEFT_CURLY_BRACKET){
                                throw new MarbleError(MarbleError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                            }
                            //return new BinaryOperatorVertex(new UnaryOperatorVertex(keywordToken, operand), new OperatorToken(OperatorToken.OPERATOR.ASSIGN), get_bracket_data_vertex());
                            break;
                        }
                        else if (CurrentToken.Type == Token.TYPE.LEFT_CIRCLE_BRACKET) {
                            //return get_function_vertex(keywordToken);
                        }
                        return KeywordVertex.FromToken(keywordToken);
                    case "Return":
                    case "Break":
                    case "Continue":
                    case "Breakpoint":
                        next_token();
                        switch (CurrentToken.Type) {
                            case Token.TYPE.LEFT_CIRCLE_BRACKET:
                            case Token.TYPE.LEFT_CURLY_BRACKET:
                            case Token.TYPE.LEFT_SQUARE_BRACKET:
                            case Token.TYPE.DATA:
                                if ((string) keywordToken.Token_value != "Return") {
                                    throw new MarbleError(MarbleError.TYPE.UNEXPECTED_OPERAND, CurrentToken.Position);
                                }
                                //Vertex expression = get_statement(get_operand_vertex(get_operand_vertex));
                                //return new UnaryOperatorVertex(keywordToken, expression);
                                break;
                            default:
                                if ((string) keywordToken.Token_value == "Return") {
                                    throw new MarbleError(MarbleError.TYPE.EXPECTED_OPERAND, keywordToken.Position);
                                }
                                break;
                        }
                        return KeywordVertex.FromToken(keywordToken);
                    case "if":
                        break;
                    case "For":
                    case "While":
                        /*
                        next_token();
                        Vertex expression = ((string) keywordToken.Token_value == "For")? get_binary_vertex(get_unary_vertex, PRECEDENCE[6]) : get_statement(get_operand_vertex(get_operand_vertex));
                        if (CurrentToken.Type != Token.TYPE.LEFT_CURLY_BRACKET){
                            throw new MarbleError(MarbleError.TYPE.EXPECTED_TOKEN, CurrentToken.Position, "'{'");
                        }
                        return new BinaryOperatorVertex(new UnaryOperatorVertex(keywordToken, expression), new OperatorToken(OperatorToken.OPERATOR.RUNS), get_instructions_vertex());
                        */
                        break;
                    case "Function":
                        break;
                    case "Print":
                    case "Range":
                    case "Assert":
                    case "Random":
                    case "Input":
                        next_token();
                        if (CurrentToken.Type == Token.TYPE.LEFT_CIRCLE_BRACKET) {
                            //return get_function_vertex(keyword)
                        }
                        else {
                            throw new MarbleError(MarbleError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                        }
                        break;
                    default:
                        throw new MarbleError(MarbleError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                }
                break;
            case OperatorToken operatorToken:
                switch (operatorToken.Operator_type) {
                    case OperatorToken.OPERATOR.NOT:
                    case OperatorToken.OPERATOR.ADD:
                    case OperatorToken.OPERATOR.SUBTRACT:
                        next_token();
                        return new UnaryOperatorVertex(operatorToken, get_operand_vertex(get_unary_vertex));
                    default:
                        throw new MarbleError(MarbleError.TYPE.UNEXPECTED_OPERAND, CurrentToken.Position, "Unexpected Unary Operand");
                }
            case Token token:
                switch (token.Type) {
                    case Token.TYPE.DATA:
                    case Token.TYPE.LEFT_CURLY_BRACKET:
                    case Token.TYPE.LEFT_SQUARE_BRACKET:
                    case Token.TYPE.LEFT_CIRCLE_BRACKET:
                        return get_operand_vertex();
                }
                break;
            default:
                throw new MarbleError(MarbleError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
        }
        return null;
    }
    private Vertex get_binary_vertex(Func<Vertex> function = null, OperatorToken.OPERATOR[] precedence = null) {
        Vertex left = function();
        while (CurrentToken is OperatorToken && precedence.Contains(((OperatorToken) CurrentToken).Operator_type)) {
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
            switch (CurrentToken.Type) {
                case Token.TYPE.END_OF_LINE:
                case Token.TYPE.END_OF_FILE:
                    next_token();
                    break;
                default:
                    Parsed.Add(get_statement());
                    break;
            }
        }
        return Parsed;
    }
}
