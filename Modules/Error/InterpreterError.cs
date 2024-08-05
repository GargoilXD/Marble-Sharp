public class InterpreterError : Error {
    public enum TYPE {
        UNCOMPATIBLE_TYPES,
        INVALID_OPERATION,
        UNDEFINED_IDENTIFIER,
        DATATYPE_MISMATCH,
        ALREADY_DEFINED_IDENTIFIER,
        DIVISION_BY_ZERO,
        UNEXPECTED_TOKEN,
        EXPECTED_IDENTIFIER,
        UNINITIALIZED_IDENTIFIER,
        INCOMPATIBLE_TYPES,
        ASSERTION_FAILED,
        MESSAGE,
    }
    public TYPE Type {private set; get; }
    public InterpreterError(TYPE type, TokenPosition position, string message = "") : base(position, message) {
        Type = type;
    }
    public InterpreterError ResetPosition(TokenPosition position) {
        Position = position;
        return this;
    }
    public override string ToString() {
        return Type + Message + IndicateErrorLine();
    }
}