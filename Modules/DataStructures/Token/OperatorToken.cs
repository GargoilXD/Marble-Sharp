public class OperatorToken : Token {
    public enum TYPE {
        DOT,
        NOT,
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        INTEGER_DIVIDE,
        EXPONENT,
        MODOLUS,
        AND,
        BITWISE_AND,
        OR,
        BITWISE_OR,
        IN,
        IS,
        COLON,
        EQUALS,
        NOT_EQUALS,
        GREATER_THAN,
        GREATER_THAN_OR_EQUALS,
        LESSER_THAN,
        LESSER_THAN_OR_EQUALS,
        ASSIGN,
        ADD_AND_ASSIGN,
        SUBTRACT_AND_ASSIGN,
        MULTIPLY_AND_ASSIGN,
        DIVIDE_AND_ASSIGN,
        EXPONENT_AND_ASSIGN,
        MODOLUS_AND_ASSIGN,
        RUNS,
        EXTENDS
    }
    public TYPE Type;
    public OperatorToken(TYPE type, TokenPosition position) : base(TOKEN_TYPE.OPERATOR, position) {
        Type = type;
    }
    public override string ToString() {
        return $"({Type})";
    }
}