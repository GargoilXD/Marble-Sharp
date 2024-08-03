public class TokenizerError : Error {
    public enum TYPE {
        INVALID_CHARACTER,
        UNIDENTIFIED_OPERATOR,
        INCOMPLETE_STRING
    }
    private TYPE Type;
    public TokenizerError(TYPE type, TokenPosition position, string message = "") : base(position, message) {
        Type = type;
    }
    public override string ToString() {
        return Type + Message + "\n" + DrawPosition();
    }
}