public class Token {
    public enum TOKEN_TYPE {
        NONE,
        DATA,
        KEYWORD,
        OPERATOR,
        COMMA,
        SYMBOL
    }
    public TOKEN_TYPE Token_type;
    public TokenPosition Position;
    public Token(TOKEN_TYPE token_type, TokenPosition position) {
        Token_type = token_type;
        Position = position;
    }
    public override string ToString() {
        return $"({Token_type})";
    }
}
