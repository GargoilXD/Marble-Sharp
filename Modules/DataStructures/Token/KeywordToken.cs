public class KeywordToken : Token {
    public enum KEYWORD {
        NONE,
        MODIFIER,
        DATATYPE,
        FLOWCONTROL,
        DECISION,
        LOOP,
        INSTRUCTION_SET,
        FUNCTION
    }

    public KEYWORD Keyword { get; private set; }

    public KeywordToken(KEYWORD keyword = KEYWORD.NONE, TokenPosition position = null, object tokenValue = null) {
        Keyword = keyword;
        Token_value = tokenValue;
        Position = position;
        Type = TYPE.KEYWORD;
    }

    public override string ToString() {
        string keywordStr = Keyword.ToString();
        if (Keyword == KEYWORD.DATATYPE && Token_value is DataToken.DATATYPE) {
            return $"({keywordStr}, {Token_value})";
        }
        return $"({keywordStr}, {Token_value})";
    }
}