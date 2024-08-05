public class InterpreterError : Error {
    public enum TYPE {
        INCOMPATIBLE_TYPES,
        INVALID_OPERATION,
        UNDEFINED_IDENTIFIER,
        DATATYPE_MISMATCH,
        ALREADY_DEFINED_IDENTIFIER,
        DIVISION_BY_ZERO,
        UNEXPECTED_TOKEN,
        EXPECTED_IDENTIFIER,
        UNINITIALIZED_IDENTIFIER,
        ASSERTION_FAILED,
        MESSAGE,
    }
    public TYPE Type {private set; get; }
    public InterpreterError(TYPE type, TokenPosition position = null, string message = "") : base(position, message) {
        Type = type;
    }
    public void ResetPosition(TokenPosition position) {
        Position = position;
    }
    public override string ToString() {
        return Type + Message + IndicateErrorLine();
    }
}