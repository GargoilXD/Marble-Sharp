public class ParserError : Error {
    public enum TYPE {
        UNIMPLEMENTED_TOKEN,
        EXPECTED_OPERAND,
        UNEXPECTED_OPERAND,
        EXPECTED_OPERATOR,
        UNEXPECTED_OPERATOR,
        EXPECTED_TOKENs,
        UNEXPECTED_TOKEN,
        UNCLOSED_BRACKETS,
        EMPTHY_ENUMERATION,
        EMPTHY_CASE,
        ALREADY_DEFINED_DATATYPE

    }
    public TYPE Type {private set; get; }
    public ParserError(TYPE type, TokenPosition position, string message = "") : base(position, message) {
        Type = type;
    }
    public override string ToString() {
        return Type + Message + IndicateErrorLine();
    }
}