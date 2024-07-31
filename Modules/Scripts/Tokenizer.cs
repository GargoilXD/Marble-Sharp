using System.Collections.Generic;

public class Tokenizer {
    public static string Code = "";
    public static readonly List<string> MODIFIER_KEYWORDS = new List<string> { "Const", "Static", "Public", "Private", "Void" };
    public static readonly List<string> DATA_KEYWORDS = new List<string> { "true", "false", "null", "self" };
    public static readonly List<string> DATATYPE_KEYWORDS = new List<string> { "Variant", "Boolean", "Integer", "Float", "String", "List", "Dictionary", "Enumeration", "Object" };
    public static readonly List<string> OPERATOR_KEYWORDS = new List<string> { "not", "and", "or", "in", "is", "extends" };
    public static readonly List<string> FLOWCONTROL_KEYWORDS = new List<string> { "Break", "Continue", "Return", "Breakpoint" };
    public static readonly List<string> DECISION_KEYWORDS = new List<string> { "if", "else", "elseif", "Match", "Case", "Default" };
    public static readonly List<string> LOOP_KEYWORDS = new List<string> { "For", "While" };
    public static readonly List<string> INSTRUCTION_SET_KEYWORDS = new List<string> { "Class", "Function" };
    public static readonly List<string> FUNCTION_KEYWORDS = new List<string> { "Assert", "Print", "Range", "Random", "Input" };
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
        Character = Index < Code.Length ? Code[Index] : '\0';
        if (Character == '\n') Line += 1;
    }

    public List<Token> Tokenize(string code) {
        Code = code;
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
                Tokens.Add(MakeOperatorToken());
            }
            else {
                switch (Character) {
                    case '$':
                        if (Tokens.Count == 0) {
                            NextCharacter();
                        }
                        else {
                            var errorToken = Tokens[^1];
                            Tokens.RemoveAt(Tokens.Count - 1);
                            throw new MarbleError(MarbleError.TYPE.MESSAGE, errorToken.Position, errorToken.ToString());
                        }
                        break;
                    case '\n':
                        Tokens.Add(new Token(Token.TYPE.END_OF_LINE, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '#':
                        IgnoreComment();
                        break;
                    case ',':
                        Tokens.Add(new Token(Token.TYPE.COMMA, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '{':
                        Tokens.Add(new Token(Token.TYPE.LEFT_CURLY_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '}':
                        Tokens.Add(new Token(Token.TYPE.RIGHT_CURLY_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '(':
                        Tokens.Add(new Token(Token.TYPE.LEFT_CIRCLE_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case ')':
                        Tokens.Add(new Token(Token.TYPE.RIGHT_CIRCLE_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case '[':
                        Tokens.Add(new Token(Token.TYPE.LEFT_SQUARE_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    case ']':
                        Tokens.Add(new Token(Token.TYPE.RIGHT_SQUARE_BRACKET, TokenPositioner.DropPoint(Index, Line)));
                        NextCharacter();
                        break;
                    default:
                        throw new MarbleError(MarbleError.TYPE.INVALID_CHARACTER, TokenPositioner.DropPoint(Index, Line), $"'{Character.ToString()}'");
                }
            }
        }
        Tokens.Add(new Token(Token.TYPE.END_OF_FILE, TokenPositioner.DropPoint(Index, Line)));
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
            return new DataToken(DataToken.DATATYPE.FLOAT, Positioner.SetEnd(Index, Line, Character), float.Parse(data));
        }
        else {
            return new DataToken(DataToken.DATATYPE.INTEGER, Positioner.SetEnd(Index, Line, Character), int.Parse(data));
        }
    }

    private Token MakeLetterToken() {
        string data = "";
        Positioner.SetStart(Index, Line);
        while (LETTERS.Contains(char.ToLower(Character)) || NUMBERS.Contains(Character)) {
            data += Character;
            NextCharacter();
        }
        if (MODIFIER_KEYWORDS.Contains(data)) {
            return data switch {
                "Const" => new KeywordToken(KeywordToken.KEYWORD.MODIFIER, Positioner.SetEnd(Index, Line, Character), data),
                "Static" => new KeywordToken(KeywordToken.KEYWORD.MODIFIER, Positioner.SetEnd(Index, Line, Character), data),
                "Public" => new KeywordToken(KeywordToken.KEYWORD.MODIFIER, Positioner.SetEnd(Index, Line, Character), data),
                "Private" => new KeywordToken(KeywordToken.KEYWORD.MODIFIER, Positioner.SetEnd(Index, Line, Character), data),
                "Void" => new KeywordToken(KeywordToken.KEYWORD.MODIFIER, Positioner.SetEnd(Index, Line, Character), data),
                _ => null
            };
        }
        else if (DATA_KEYWORDS.Contains(data)) {
            return data switch {
                "true" => new DataToken(DataToken.DATATYPE.BOOLEAN, Positioner.SetEnd(Index, Line, Character), true),
                "false" => new DataToken(DataToken.DATATYPE.BOOLEAN, Positioner.SetEnd(Index, Line, Character), false),
                "null" => new DataToken(DataToken.DATATYPE.VARIANT, Positioner.SetEnd(Index, Line, Character), null),
                "self" => new DataToken(DataToken.DATATYPE.OBJECT, Positioner.SetEnd(Index, Line, Character), data),
                _ => null
            };
        }
        else if (DATATYPE_KEYWORDS.Contains(data)) {
            return data switch {
                "Variant" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                "Boolean" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                "Integer" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                "Float" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                "String" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                "List" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                "Dictionary" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                "Enumeration" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                "Object" => new KeywordToken(KeywordToken.KEYWORD.DATATYPE, Positioner.SetEnd(Index, Line, Character), data),
                _ => null
            };
        }
        else if (OPERATOR_KEYWORDS.Contains(data)) {
            return data switch {
                "not" => new OperatorToken(OperatorToken.OPERATOR.NOT, Positioner.SetEnd(Index, Line, Character)),
                "and" => new OperatorToken(OperatorToken.OPERATOR.ADD, Positioner.SetEnd(Index, Line, Character)),
                "or" => new OperatorToken(OperatorToken.OPERATOR.OR, Positioner.SetEnd(Index, Line, Character)),
                "in" => new OperatorToken(OperatorToken.OPERATOR.IN, Positioner.SetEnd(Index, Line, Character)),
                "is" => new OperatorToken(OperatorToken.OPERATOR.IS, Positioner.SetEnd(Index, Line, Character)),
                "extends" => new OperatorToken(OperatorToken.OPERATOR.EXTENDS, Positioner.SetEnd(Index, Line, Character)),
                _ => null
            };
        }
        else if (FLOWCONTROL_KEYWORDS.Contains(data)) {
            return data switch {
                "Break" => new KeywordToken(KeywordToken.KEYWORD.FLOWCONTROL, Positioner.SetEnd(Index, Line, Character), data),
                "Continue" => new KeywordToken(KeywordToken.KEYWORD.FLOWCONTROL, Positioner.SetEnd(Index, Line, Character), data),
                "Return" => new KeywordToken(KeywordToken.KEYWORD.FLOWCONTROL, Positioner.SetEnd(Index, Line, Character), data),
                "Breakpoint" => new KeywordToken(KeywordToken.KEYWORD.FLOWCONTROL, Positioner.SetEnd(Index, Line, Character), data),
                _ => null
            };
        }
        else if (DECISION_KEYWORDS.Contains(data)) {
            return data switch {
                "if" => new KeywordToken(KeywordToken.KEYWORD.DECISION, Positioner.SetEnd(Index, Line, Character), data),
                "else" => new KeywordToken(KeywordToken.KEYWORD.DECISION, Positioner.SetEnd(Index, Line, Character), data),
                "elseif" => new KeywordToken(KeywordToken.KEYWORD.DECISION, Positioner.SetEnd(Index, Line, Character), data),
                "Match" => new KeywordToken(KeywordToken.KEYWORD.DECISION, Positioner.SetEnd(Index, Line, Character), data),
                "Case" => new KeywordToken(KeywordToken.KEYWORD.DECISION, Positioner.SetEnd(Index, Line, Character), data),
                "Default" => new KeywordToken(KeywordToken.KEYWORD.DECISION, Positioner.SetEnd(Index, Line, Character), data),
                _ => null
            };
        }
        else if (LOOP_KEYWORDS.Contains(data)) {
            return data switch {
                "For" => new KeywordToken(KeywordToken.KEYWORD.LOOP, Positioner.SetEnd(Index, Line, Character), data),
                "While" => new KeywordToken(KeywordToken.KEYWORD.LOOP, Positioner.SetEnd(Index, Line, Character), data),
                _ => null
            };
        }
        else if (INSTRUCTION_SET_KEYWORDS.Contains(data)) {
            return data switch {
                "Class" => new KeywordToken(KeywordToken.KEYWORD.INSTRUCTION_SET, Positioner.SetEnd(Index, Line, Character), data),
                "Function" => new KeywordToken(KeywordToken.KEYWORD.INSTRUCTION_SET, Positioner.SetEnd(Index, Line, Character), data),
                _ => null
            };
        }
        else if (FUNCTION_KEYWORDS.Contains(data)) {
            return data switch {
                "Assert" => new KeywordToken(KeywordToken.KEYWORD.FUNCTION, Positioner.SetEnd(Index, Line, Character), data),
                "Print" => new KeywordToken(KeywordToken.KEYWORD.FUNCTION, Positioner.SetEnd(Index, Line, Character), data),
                "Range" => new KeywordToken(KeywordToken.KEYWORD.FUNCTION, Positioner.SetEnd(Index, Line, Character), data),
                "Random" => new KeywordToken(KeywordToken.KEYWORD.FUNCTION, Positioner.SetEnd(Index, Line, Character), data),
                "Input" => new KeywordToken(KeywordToken.KEYWORD.FUNCTION, Positioner.SetEnd(Index, Line, Character), data),
                _ => null
            };
        }
        else {
            return new DataToken(DataToken.DATATYPE.IDENTIFIER, Positioner.SetEnd(Index, Line, Character), data);
        }
    }

    private DataToken MakeStringToken() {
        char start = Character;
        string data = "";
        Positioner.SetStart(Index, Line);
        NextCharacter();
        while (Character != start) {
            if (Character == '\n' || Character == '\0') {
                throw new MarbleError(MarbleError.TYPE.INCOMPLETE_STRING, Positioner.SetEnd(Index, Line, Character));
            }
            data += Character;
            NextCharacter();
        }
        NextCharacter();
        return new DataToken(DataToken.DATATYPE.STRING, Positioner.SetEnd(Index, Line, Character), data);
    }

    private Token MakeOperatorToken() {
        string data = "";
        Positioner.SetStart(Index, Line);
        while (OPERATOR_CHARACTERS.Contains(Character)) {
            data += Character;
            NextCharacter();
        }
        switch (data) {
            case ".": return new OperatorToken(OperatorToken.OPERATOR.DOT, Positioner.SetEnd(Index, Line, Character));
            case "!": return new OperatorToken(OperatorToken.OPERATOR.NOT, Positioner.SetEnd(Index, Line, Character));
            case "+": return new OperatorToken(OperatorToken.OPERATOR.ADD, Positioner.SetEnd(Index, Line, Character));
            case "-": return new OperatorToken(OperatorToken.OPERATOR.SUBTRACT, Positioner.SetEnd(Index, Line, Character));
            case "*": return new OperatorToken(OperatorToken.OPERATOR.MULTIPLY, Positioner.SetEnd(Index, Line, Character));
            case "/": return new OperatorToken(OperatorToken.OPERATOR.DIVIDE, Positioner.SetEnd(Index, Line, Character));
            case "^": return new OperatorToken(OperatorToken.OPERATOR.EXPONENT, Positioner.SetEnd(Index, Line, Character));
            case "%": return new OperatorToken(OperatorToken.OPERATOR.MODOLUS, Positioner.SetEnd(Index, Line, Character));
            case "=": return new OperatorToken(OperatorToken.OPERATOR.ASSIGN, Positioner.SetEnd(Index, Line, Character));
            case "<": return new OperatorToken(OperatorToken.OPERATOR.LESSER_THAN, Positioner.SetEnd(Index, Line, Character));
            case ">": return new OperatorToken(OperatorToken.OPERATOR.GREATER_THAN, Positioner.SetEnd(Index, Line, Character));
            case ":": return new OperatorToken(OperatorToken.OPERATOR.COLON, Positioner.SetEnd(Index, Line, Character));
            case "&": return new OperatorToken(OperatorToken.OPERATOR.BITWISE_AND, Positioner.SetEnd(Index, Line, Character));
            case "|": return new OperatorToken(OperatorToken.OPERATOR.BITWISE_OR, Positioner.SetEnd(Index, Line, Character));
            case "!=": return new OperatorToken(OperatorToken.OPERATOR.NOT_EQUALS, Positioner.SetEnd(Index, Line, Character));
            case "+=": return new OperatorToken(OperatorToken.OPERATOR.ADD_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character));
            case "-=": return new OperatorToken(OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character));
            case "*=": return new OperatorToken(OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character));
            case "/=": return new OperatorToken(OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character));
            case "^=": return new OperatorToken(OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character));
            case "%=": return new OperatorToken(OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN, Positioner.SetEnd(Index, Line, Character));
            case "==": return new OperatorToken(OperatorToken.OPERATOR.EQUALS, Positioner.SetEnd(Index, Line, Character));
            case "<=": return new OperatorToken(OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS, Positioner.SetEnd(Index, Line, Character));
            case ">=": return new OperatorToken(OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS, Positioner.SetEnd(Index, Line, Character));
            default: throw new MarbleError(MarbleError.TYPE.UNIDENTIFIED_OPERATOR, Positioner.SetEnd(Index, Line, Character), data);
            }
    }

    private void IgnoreComment() {
        NextCharacter();
        while (Character != '\n' && Character != '\0') {
            NextCharacter();
        }
    }
}


