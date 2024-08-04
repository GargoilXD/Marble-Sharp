using System.Collections.Generic;
public class Tokenizer {
    public static readonly Dictionary<string, List<string>> KEYWORD_CLASSIFICATIONS = new Dictionary<string, List<string>> {
        {"MODIFIER", new List<string> { "const", "static", "public", "private"}},
        {"DATA", new List<string> { "true", "false", "null", "self" }},
        {"DATATYPE", new List<string> { "variant", "boolean", "integer", "float", "string", "list", "dictionary", "enumeration", "object" }},
        {"OPERATOR", new List<string> { "not", "and", "or", "in", "is", "extends" }},
        {"FLOW_CONTROL", new List<string> { "break", "continue", "return", "breakpoint" }},
        {"DECISION", new List<string> { "if", "else", "elseif", "match", "case", "default" }},
        {"LOOP", new List<string> { "for", "while" }},
        {"DEFINITION", new List<string> { "class", "function", "structure" }},
        {"INBUILT_FUNCTION", new List<string> { "Assert", "Print", "Range", "Random", "Input" }},
    };
    public static readonly List<char> LETTERS = new List<char> { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm', '_' };
    public static readonly List<char> NUMBERS = new List<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
    public static readonly List<char> OPERATOR_CHARACTERS = new List<char> { '!', '+', '-', '*', '/', '^', '%', '=', '<', '>', ':', '.', '&', '|' };
    private int Index;
    private int Line;
    private char Character;
    private List<Token> Tokens = new List<Token>();
    private TokenPositioner Positioner = new TokenPositioner();
    private void NextCharacter() {
        Index += 1;
        Character = Index < MarbleIDE.EditorCode.Length ? MarbleIDE.EditorCode[Index] : '\0';
        if (Character == '\n') Line += 1;
    }
    public List<Token> Tokenize() {
        Index = -1;
        Line = 0;
        Character = '\0';
        Tokens.Clear();
        Positioner.Reset();
        NextCharacter();
        while (Character != '\0') {
            if (Character == ' ' || Character == '\t') {
                NextCharacter();
            }
            else if (LETTERS.Contains(char.ToLower(Character))) {
                Tokens.Add(MakeLetterToken());
            }
            else if (Character == '"' || Character == '\'') {
                Tokens.Add(MakeStringToken());
            }
            else if (NUMBERS.Contains(Character)) {
                Tokens.Add(MakeNumberToken());
            }
            else if (OPERATOR_CHARACTERS.Contains(Character)) {
                Tokens.Add(MakeSymbolToken());
            }
            else {
                switch (Character) {
                    case '\n':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.END_OF_LINE, new TokenPosition(Index, Line - 1)));
                        NextCharacter();
                        break;
                    case '#':
                        IgnoreComment();
                        break;
                    case ',':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.COMMA, new TokenPosition(Index, Line)));
                        NextCharacter();
                        break;
                    case '{':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, new TokenPosition(Index, Line)));
                        NextCharacter();
                        break;
                    case '}':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET, new TokenPosition(Index, Line)));
                        NextCharacter();
                        break;
                    case '(':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, new TokenPosition(Index, Line)));
                        NextCharacter();
                        break;
                    case ')':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET, new TokenPosition(Index, Line)));
                        NextCharacter();
                        break;
                    case '[':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET, new TokenPosition(Index, Line)));
                        NextCharacter();
                        break;
                    case ']':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET, new TokenPosition(Index, Line)));
                        NextCharacter();
                        break;
                    default:
                        throw new TokenizerError(TokenizerError.TYPE.INVALID_CHARACTER, new TokenPosition(Index, Line), $"'{Character}'");
                }
            }
        }
        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.END, new TokenPosition(Index, Line)));
        return Tokens;
    }
    private DataToken MakeNumberToken() {
        string data = "";
        int dot = 0;
        Positioner.Start(Index, Line);
        while (NUMBERS.Contains(Character) || Character == '.') {
            if (Character == '.') dot += 1;
            if (dot == 2) break;
            data += Character;
            NextCharacter();
        }
        if (dot == 1) {
            return new DataToken(DataToken.TYPE.FLOAT, float.Parse(data), Positioner.End(Index, Line));
        }
        else {
            return new DataToken(DataToken.TYPE.INTEGER, int.Parse(data), Positioner.End(Index, Line));
        }
    }
    private Token MakeLetterToken() {
        string data = "";
        Positioner.Start(Index, Line);
        while (LETTERS.Contains(char.ToLower(Character)) || NUMBERS.Contains(Character)) {
            data += Character;
            NextCharacter();
        }
        return data switch {
            "const" => new KeywordToken(KeywordToken.TYPE.MODIFIER, KeywordToken.KEYWORD.CONST, Positioner.End(Index, Line)),
            "static" => new KeywordToken(KeywordToken.TYPE.MODIFIER, KeywordToken.KEYWORD.STATIC, Positioner.End(Index, Line)),
            "public" => new KeywordToken(KeywordToken.TYPE.MODIFIER, KeywordToken.KEYWORD.PUBLIC, Positioner.End(Index, Line)),
            "private" => new KeywordToken(KeywordToken.TYPE.MODIFIER, KeywordToken.KEYWORD.PRIVATE, Positioner.End(Index, Line)),
            
            "true" => new DataToken(DataToken.TYPE.BOOLEAN, true, Positioner.End(Index, Line)),
            "false" => new DataToken(DataToken.TYPE.BOOLEAN, false, Positioner.End(Index, Line)),
            "null" => new DataToken(DataToken.TYPE.VARIANT, null, Positioner.End(Index, Line)),
            "self" => new DataToken(DataToken.TYPE.OBJECT, KeywordToken.KEYWORD.SELF, Positioner.End(Index, Line)),
            
            "variant" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.VARIANT, Positioner.End(Index, Line)),
            "boolean" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.BOOLEAN, Positioner.End(Index, Line)),
            "integer" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.INTEGER, Positioner.End(Index, Line)),
            "float" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.FLOAT, Positioner.End(Index, Line)),
            "string" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.STRING, Positioner.End(Index, Line)),
            "list" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.LIST, Positioner.End(Index, Line)),
            "dictionary" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.DICTIONARY, Positioner.End(Index, Line)),
            "enumeration" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.ENUMERATION, Positioner.End(Index, Line)),
            "object" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.OBJECT, Positioner.End(Index, Line)),
            
            "not" => new OperatorToken(OperatorToken.OPERATOR.NOT, Positioner.End(Index, Line)),
            "and" => new OperatorToken(OperatorToken.OPERATOR.ADD, Positioner.End(Index, Line)),
            "or" => new OperatorToken(OperatorToken.OPERATOR.OR, Positioner.End(Index, Line)),
            "in" => new OperatorToken(OperatorToken.OPERATOR.IN, Positioner.End(Index, Line)),
            "is" => new OperatorToken(OperatorToken.OPERATOR.IS, Positioner.End(Index, Line)),
            "extends" => new OperatorToken(OperatorToken.OPERATOR.EXTENDS, Positioner.End(Index, Line)),
            
            "break" => new KeywordToken(KeywordToken.TYPE.FLOWCONTROL, KeywordToken.KEYWORD.BREAK, Positioner.End(Index, Line)),
            "continue" => new KeywordToken(KeywordToken.TYPE.FLOWCONTROL, KeywordToken.KEYWORD.CONTINUE, Positioner.End(Index, Line)),
            "return" => new KeywordToken(KeywordToken.TYPE.FLOWCONTROL, KeywordToken.KEYWORD.RETURN, Positioner.End(Index, Line)),
            "breakpoint" => new KeywordToken(KeywordToken.TYPE.FLOWCONTROL, KeywordToken.KEYWORD.BREAKPOINT, Positioner.End(Index, Line)),
            
            "if" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.IF, Positioner.End(Index, Line)),
            "else" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.ELSE, Positioner.End(Index, Line)),
            "elseif" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.ELSE_IF, Positioner.End(Index, Line)),
            "match" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.MATCH, Positioner.End(Index, Line)),
            "case" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.CASE, Positioner.End(Index, Line)),
            "default" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.DEFAULT, Positioner.End(Index, Line)),
            
            "for" => new KeywordToken(KeywordToken.TYPE.LOOP, KeywordToken.KEYWORD.FOR, Positioner.End(Index, Line)),
            "while" => new KeywordToken(KeywordToken.TYPE.LOOP, KeywordToken.KEYWORD.WHILE, Positioner.End(Index, Line)),
            
            "class" => new KeywordToken(KeywordToken.TYPE.DEFINITION, KeywordToken.KEYWORD.CLASS, Positioner.End(Index, Line)),
            "function" => new KeywordToken(KeywordToken.TYPE.DEFINITION, KeywordToken.KEYWORD.FUNCTION, Positioner.End(Index, Line)),
            "structure" => new KeywordToken(KeywordToken.TYPE.DEFINITION, KeywordToken.KEYWORD.STRUCTURE, Positioner.End(Index, Line)),
            
            "Assert" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.ASSERT, Positioner.End(Index, Line)),
            "Print" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.PRINT, Positioner.End(Index, Line)),
            "Range" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.RANGE, Positioner.End(Index, Line)),
            "Random" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.RANDOM, Positioner.End(Index, Line)),
            "Input" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.INPUT, Positioner.End(Index, Line)),
            
            _ => new DataToken(DataToken.TYPE.IDENTIFIER, data, Positioner.End(Index, Line))
        };
    }
    private DataToken MakeStringToken() {
        char start = Character;
        string data = "";
        Positioner.Start(Index, Line);
        NextCharacter();
        while (Character != start) {
            if (Character == '\0') throw new TokenizerError(TokenizerError.TYPE.INCOMPLETE_STRING, Positioner.End(Index, Line));
            data += Character;
            NextCharacter();
        }
        NextCharacter();
        return new DataToken(DataToken.TYPE.STRING, data, Positioner.End(Index, Line));
    }
    private Token MakeSymbolToken() {
        string data = "";
        Positioner.Start(Index, Line);
        while (OPERATOR_CHARACTERS.Contains(Character)) {
            data += Character;
            NextCharacter();
        }
        return data switch {
            "." => new OperatorToken(OperatorToken.OPERATOR.DOT, Positioner.End(Index, Line)),
            "+" => new OperatorToken(OperatorToken.OPERATOR.ADD, Positioner.End(Index, Line)),
            "-" => new OperatorToken(OperatorToken.OPERATOR.SUBTRACT, Positioner.End(Index, Line)),
            "*" => new OperatorToken(OperatorToken.OPERATOR.MULTIPLY, Positioner.End(Index, Line)),
            "/" => new OperatorToken(OperatorToken.OPERATOR.DIVIDE, Positioner.End(Index, Line)),
            "^" => new OperatorToken(OperatorToken.OPERATOR.EXPONENT, Positioner.End(Index, Line)),
            "%" => new OperatorToken(OperatorToken.OPERATOR.MODOLUS, Positioner.End(Index, Line)),
            
            "=" => new OperatorToken(OperatorToken.OPERATOR.ASSIGN, Positioner.End(Index, Line)),
            ":" => new OperatorToken(OperatorToken.OPERATOR.COLON, Positioner.End(Index, Line)),

            "!" => new OperatorToken(OperatorToken.OPERATOR.NOT, Positioner.End(Index, Line)),
            "|" => new OperatorToken(OperatorToken.OPERATOR.BITWISE_OR, Positioner.End(Index, Line)),
            "&" => new OperatorToken(OperatorToken.OPERATOR.BITWISE_AND, Positioner.End(Index, Line)),
            "<" => new OperatorToken(OperatorToken.OPERATOR.LESSER_THAN, Positioner.End(Index, Line)),
            ">" => new OperatorToken(OperatorToken.OPERATOR.GREATER_THAN, Positioner.End(Index, Line)),

            "//" => new OperatorToken(OperatorToken.OPERATOR.INTEGER_DIVIDE, Positioner.End(Index, Line)),

            "||" => new OperatorToken(OperatorToken.OPERATOR.OR, Positioner.End(Index, Line)),
            "&&" => new OperatorToken(OperatorToken.OPERATOR.AND, Positioner.End(Index, Line)),
            
            "==" => new OperatorToken(OperatorToken.OPERATOR.EQUALS, Positioner.End(Index, Line)),
            "!=" => new OperatorToken(OperatorToken.OPERATOR.NOT_EQUALS, Positioner.End(Index, Line)),
            "<=" => new OperatorToken(OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS, Positioner.End(Index, Line)),
            ">=" => new OperatorToken(OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS, Positioner.End(Index, Line)),

            "+=" => new OperatorToken(OperatorToken.OPERATOR.ADD_AND_ASSIGN, Positioner.End(Index, Line)),
            "-=" => new OperatorToken(OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN, Positioner.End(Index, Line)),
            "*=" => new OperatorToken(OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN, Positioner.End(Index, Line)),
            "/=" => new OperatorToken(OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN, Positioner.End(Index, Line)),
            "^=" => new OperatorToken(OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN, Positioner.End(Index, Line)),
            "%=" => new OperatorToken(OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN, Positioner.End(Index, Line)),
            
            "<<" => new SymbolToken(SymbolToken.SYMBOL.LEFT_TUPLE_BRACKET, Positioner.End(Index, Line)),
            ">>" => new SymbolToken(SymbolToken.SYMBOL.RIGHT_TUPLE_BRACKET, Positioner.End(Index, Line)),
            _ => throw new TokenizerError(TokenizerError.TYPE.UNIDENTIFIED_OPERATOR, Positioner.End(Index, Line), $"'{data}'")
        };
    }
    private void IgnoreComment() {
        NextCharacter();
        while (Character != '\n' && Character != '\0') NextCharacter();
    }
}
