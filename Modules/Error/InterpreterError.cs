public class InterpreterError : Error {
    public enum TYPE {
        UNIMPLEMENTED_FEATURE,
        INCOMPATIBLE_TYPES,
        INVALID_OPERATION,
        UNDEFINED_IDENTIFIER,
        UNDEFINED_FUNCTION,
        DATATYPE_MISMATCH,
        ALREADY_DEFINED_IDENTIFIER,
        ALREADY_DEFINED_FUNCTION,
        DIVISION_BY_ZERO,
        UNEXPECTED_TOKEN,
        EXPECTED_IDENTIFIER,
        UNINITIALIZED_IDENTIFIER,
        ASSERTION_FAILED,
        MESSAGE,
    }
    public TYPE Type {private set; get; }
    public InterpreterError(TYPE type, TokenPosition position, string message = "") : base(position, message) {
        Type = type;
    }
    public override string ToString() {
        return Type + Message + IndicateErrorLine();
    }
}