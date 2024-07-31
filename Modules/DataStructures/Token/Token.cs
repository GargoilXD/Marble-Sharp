public class Token {
    public enum TYPE {
        NONE,
        DATA,
        KEYWORD,
        OPERATOR,
        COMMA,
        LEFT_CURLY_BRACKET,
        RIGHT_CURLY_BRACKET,
        LEFT_SQUARE_BRACKET,
        RIGHT_SQUARE_BRACKET,
        LEFT_CIRCLE_BRACKET,
        RIGHT_CIRCLE_BRACKET,
        END_OF_LINE,
        END_OF_FILE
    }

    public TYPE Type;
    public TokenPosition Position;
    public object Token_value;

    public Token(TYPE type = TYPE.NONE, TokenPosition position = null, object value = null) {
        Type = type;
        Token_value = value;
        Position = position;
    }

    public override string ToString() {
        return $"({Type})";
    }
}
