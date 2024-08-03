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

    public class TokenPositioner {
        public int StartPoint;
        public int EndPoint;
        public int StartLine;
        public int EndLine;
        public TokenPositioner() {
            Reset();
        }
        public void Reset() {
            StartPoint = 0;
            EndPoint = 0;
            StartLine = 0;
            EndLine = 0;
        }
        public static TokenPosition DropPoint(int index, int line) {
            return new TokenPosition(index, index, line, line);
        }
        public void SetStart(int index, int line) {
            StartPoint = index;
            StartLine = line;
        }
        public TokenPosition SetEnd(int index, int line, char character) {
            EndPoint = index - 1;
            EndLine = character == '\n' ? line - 1 : line;
            return new TokenPosition(StartPoint, EndPoint, StartLine, EndLine);
        }
    }
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
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.END_OF_LINE, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '#':
                        IgnoreComment();
                        break;
                    case ',':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.COMMA, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '{':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '}':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '(':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case ')':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '[':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case ']':
                        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.RIGHT_SQUARE_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    default:
                        throw new TokenizerError(TokenizerError.TYPE.INVALID_CHARACTER, TokenPositioner.DropPoint(Index, Line), $"'{Character}'");
                }
            }
        }
        Tokens.Add(new SymbolToken(SymbolToken.SYMBOL.END, TokenPositioner.DropPoint(Index, Line)));
        return Tokens;
    }
    private DataToken MakeNumberToken() {
        string data = "";
        int dot = 0;
        Positioner.SetStart(Index, Line);
        while (NUMBERS.Contains(Character) || Character == '.') {
            if (Character == '.') {
                dot += 1;
            }
            if (dot == 2) {
                break;
            }
            data += Character;
            NextCharacter();
        }
        if (dot == 1) {
            return new DataToken(DataToken.TYPE.FLOAT, float.Parse(data), Positioner.SetEnd(Index, Line, Character));
        }
        else {
            return new DataToken(DataToken.TYPE.INTEGER, int.Parse(data), Positioner.SetEnd(Index, Line, Character));
        }
    }
    private Token MakeLetterToken() {
        string data = "";
        Positioner.SetStart(Index, Line);
        while (LETTERS.Contains(char.ToLower(Character)) || NUMBERS.Contains(Character)) {
            data += Character;
            NextCharacter();
        }
        return data switch {
            "const" => new KeywordToken(KeywordToken.TYPE.MODIFIER, KeywordToken.KEYWORD.CONST, Positioner.SetEnd(Index, Line, Character)),
            "static" => new KeywordToken(KeywordToken.TYPE.MODIFIER, KeywordToken.KEYWORD.STATIC, Positioner.SetEnd(Index, Line, Character)),
            "public" => new KeywordToken(KeywordToken.TYPE.MODIFIER, KeywordToken.KEYWORD.PUBLIC, Positioner.SetEnd(Index, Line, Character)),
            "private" => new KeywordToken(KeywordToken.TYPE.MODIFIER, KeywordToken.KEYWORD.PRIVATE, Positioner.SetEnd(Index, Line, Character)),
            
            "true" => new DataToken(DataToken.TYPE.BOOLEAN, true, Positioner.SetEnd(Index, Line, Character)),
            "false" => new DataToken(DataToken.TYPE.BOOLEAN, false, Positioner.SetEnd(Index, Line, Character)),
            "null" => new DataToken(DataToken.TYPE.VARIANT, null, Positioner.SetEnd(Index, Line, Character)),
            "self" => new DataToken(DataToken.TYPE.OBJECT, KeywordToken.KEYWORD.SELF, Positioner.SetEnd(Index, Line, Character)),
            
            "variant" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.VARIANT, Positioner.SetEnd(Index, Line, Character)),
            "boolean" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.BOOLEAN, Positioner.SetEnd(Index, Line, Character)),
            "integer" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.INTEGER, Positioner.SetEnd(Index, Line, Character)),
            "float" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.FLOAT, Positioner.SetEnd(Index, Line, Character)),
            "string" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.STRING, Positioner.SetEnd(Index, Line, Character)),
            "list" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.LIST, Positioner.SetEnd(Index, Line, Character)),
            "dictionary" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.DICTIONARY, Positioner.SetEnd(Index, Line, Character)),
            "enumeration" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.ENUMERATION, Positioner.SetEnd(Index, Line, Character)),
            "object" => new KeywordToken(KeywordToken.TYPE.DATATYPE, KeywordToken.KEYWORD.OBJECT, Positioner.SetEnd(Index, Line, Character)),
            
            "not" => new OperatorToken(OperatorToken.OPERATOR.NOT, Positioner.SetEnd(Index, Line, Character)),
            "and" => new OperatorToken(OperatorToken.OPERATOR.ADD, Positioner.SetEnd(Index, Line, Character)),
            "or" => new OperatorToken(OperatorToken.OPERATOR.OR, Positioner.SetEnd(Index, Line, Character)),
            "in" => new OperatorToken(OperatorToken.OPERATOR.IN, Positioner.SetEnd(Index, Line, Character)),
            "is" => new OperatorToken(OperatorToken.OPERATOR.IS, Positioner.SetEnd(Index, Line, Character)),
            "extends" => new OperatorToken(OperatorToken.OPERATOR.EXTENDS, Positioner.SetEnd(Index, Line, Character)),
            
            "break" => new KeywordToken(KeywordToken.TYPE.FLOWCONTROL, KeywordToken.KEYWORD.BREAK, Positioner.SetEnd(Index, Line, Character)),
            "continue" => new KeywordToken(KeywordToken.TYPE.FLOWCONTROL, KeywordToken.KEYWORD.CONTINUE, Positioner.SetEnd(Index, Line, Character)),
            "return" => new KeywordToken(KeywordToken.TYPE.FLOWCONTROL, KeywordToken.KEYWORD.RETURN, Positioner.SetEnd(Index, Line, Character)),
            "breakpoint" => new KeywordToken(KeywordToken.TYPE.FLOWCONTROL, KeywordToken.KEYWORD.BREAKPOINT, Positioner.SetEnd(Index, Line, Character)),
            
            "if" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.IF, Positioner.SetEnd(Index, Line, Character)),
            "else" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.ELSE, Positioner.SetEnd(Index, Line, Character)),
            "elseif" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.ELSE_IF, Positioner.SetEnd(Index, Line, Character)),
            "match" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.MATCH, Positioner.SetEnd(Index, Line, Character)),
            "case" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.CASE, Positioner.SetEnd(Index, Line, Character)),
            "default" => new KeywordToken(KeywordToken.TYPE.DECISION, KeywordToken.KEYWORD.DEFAULT, Positioner.SetEnd(Index, Line, Character)),
            
            "for" => new KeywordToken(KeywordToken.TYPE.LOOP, KeywordToken.KEYWORD.FOR, Positioner.SetEnd(Index, Line, Character)),
            "while" => new KeywordToken(KeywordToken.TYPE.LOOP, KeywordToken.KEYWORD.WHILE, Positioner.SetEnd(Index, Line, Character)),
            
            "class" => new KeywordToken(KeywordToken.TYPE.DEFINITION, KeywordToken.KEYWORD.CLASS, Positioner.SetEnd(Index, Line, Character)),
            "function" => new KeywordToken(KeywordToken.TYPE.DEFINITION, KeywordToken.KEYWORD.FUNCTION, Positioner.SetEnd(Index, Line, Character)),
            "structure" => new KeywordToken(KeywordToken.TYPE.DEFINITION, KeywordToken.KEYWORD.STRUCTURE, Positioner.SetEnd(Index, Line, Character)),
            
            "Assert" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.ASSERT, Positioner.SetEnd(Index, Line, Character)),
            "Print" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.PRINT, Positioner.SetEnd(Index, Line, Character)),
            "Range" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.RANGE, Positioner.SetEnd(Index, Line, Character)),
            "Random" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.RANDOM, Positioner.SetEnd(Index, Line, Character)),
            "Input" => new KeywordToken(KeywordToken.TYPE.INBUILT_FUNCTION, KeywordToken.KEYWORD.INPUT, Positioner.SetEnd(Index, Line, Character)),
            
            _ => new DataToken(DataToken.TYPE.IDENTIFIER, data, Positioner.SetEnd(Index, Line, Character))
        };
    }
    private DataToken MakeStringToken() {
        char start = Character;
        string data = "";
        Positioner.SetStart(Index, Line);
        NextCharacter();
        while (Character != start) {
            if (Character == '\n' || Character == '\0') {
                throw new TokenizerError(TokenizerError.TYPE.INCOMPLETE_STRING, Positioner.SetEnd(Index, Line, Character));
            }
            data += Character;
            NextCharacter();
        }
        NextCharacter();
        return new DataToken(DataToken.TYPE.STRING, data, Positioner.SetEnd(Index, Line, Character));
    }
    private Token MakeSymbolToken() {
        string data = "";
        Positioner.SetStart(Index, Line);
        while (OPERATOR_CHARACTERS.Contains(Character)) {
            data += Character;
            NextCharacter();
        }
        return data switch {
            "." => new OperatorToken(OperatorToken.OPERATOR.DOT, Positioner.SetEnd(Index, Line, Character)),
            "+" => new OperatorToken(OperatorToken.OPERATOR.ADD, Positioner.SetEnd(Index, Line, Character)),
            "-" => new OperatorToken(OperatorToken.OPERATOR.SUBTRACT, Positioner.SetEnd(Index, Line, Character)),
            "*" => new OperatorToken(OperatorToken.OPERATOR.MULTIPLY, Positioner.SetEnd(Index, Line, Character)),
            "/" => new OperatorToken(OperatorToken.OPERATOR.DIVIDE, Positioner.SetEnd(Index, Line, Character)),
            "^" => new OperatorToken(OperatorToken.OPERATOR.EXPONENT, Positioner.SetEnd(Index, Line, Character)),
            "%" => new OperatorToken(OperatorToken.OPERATOR.MODOLUS, Positioner.SetEnd(Index, Line, Character)),
            
            "=" => new OperatorToken(OperatorToken.OPERATOR.ASSIGN, Positioner.SetEnd(Index, Line, Character)),
            ":" => new OperatorToken(OperatorToken.OPERATOR.COLON, Positioner.SetEnd(Index, Line, Character)),

            "!" => new OperatorToken(OperatorToken.OPERATOR.NOT, Positioner.SetEnd(Index, Line, Character)),
            "|" => new OperatorToken(OperatorToken.OPERATOR.BITWISE_OR, Positioner.SetEnd(Index, Line, Character)),
            "&" => new OperatorToken(OperatorToken.OPERATOR.BITWISE_AND, Positioner.SetEnd(Index, Line, Character)),
            "<" => new OperatorToken(OperatorToken.OPERATOR.LESSER_THAN, Positioner.SetEnd(Index, Line, Character)),
            ">" => new OperatorToken(OperatorToken.OPERATOR.GREATER_THAN, Positioner.SetEnd(Index, Line, Character)),

            "//" => new OperatorToken(OperatorToken.OPERATOR.INTEGER_DIVIDE, Positioner.SetEnd(Index, Line, Character)),

            "||" => new OperatorToken(OperatorToken.OPERATOR.OR, Positioner.SetEnd(Index, Line, Character)),
            "&&" => new OperatorToken(OperatorToken.OPERATOR.AND, Positioner.SetEnd(Index, Line, Character)),
            
            "==" => new OperatorToken(OperatorToken.OPERATOR.EQUALS, Positioner.SetEnd(Index, Line, Character)),
            "!=" => new OperatorToken(OperatorToken.OPERATOR.NOT_EQUALS, Positioner.SetEnd(Index, Line, Character)),
            "<=" => new OperatorToken(OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS, Positioner.SetEnd(Index, Line, Character)),
            ">=" => new OperatorToken(OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS, Positioner.SetEnd(Index, Line, Character)),

            "+=" => new OperatorToken(OperatorToken.OPERATOR.ADD_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character)),
            "-=" => new OperatorToken(OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character)),
            "*=" => new OperatorToken(OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character)),
            "/=" => new OperatorToken(OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character)),
            "^=" => new OperatorToken(OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character)),
            "%=" => new OperatorToken(OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character)),
            
            "<<" => new SymbolToken(SymbolToken.SYMBOL.LEFT_TUPLE_BRACKET, Positioner.SetEnd(Index, Line, Character)),
            ">>" => new SymbolToken(SymbolToken.SYMBOL.RIGHT_TUPLE_BRACKET, Positioner.SetEnd(Index, Line, Character)),
            _ => throw new TokenizerError(TokenizerError.TYPE.UNIDENTIFIED_OPERATOR, Positioner.SetEnd(Index, Line, Character), $"'{data}'")
        };
    }
    private void IgnoreComment() {
        NextCharacter();
        while (Character != '\n' && Character != '\0') {
            NextCharacter();
        }
    }
}
