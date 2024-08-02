public class KeywordToken : Token {
    public enum TYPE {
        MODIFIER,
        DATATYPE,
        FLOWCONTROL,
        DECISION,
        LOOP,
        INSTRUCTION_SET,
        FUNCTION
    }
    public TYPE Type;
    public string Keyword;
    public KeywordToken(TYPE type, string keyword, TokenPosition position) : base(TOKEN_TYPE.KEYWORD, position) {
        Type = type;
        Keyword = keyword;
    }
    public override string ToString() {
        return $"({Type}, {Keyword})";
    }
}