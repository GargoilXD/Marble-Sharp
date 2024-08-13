public class KeywordToken : Token {
    public enum TYPE {
        DEFINITION_SETTING,
        DATATYPE,
        FLOWCONTROL,
        DECISION,
        LOOP,
        DEFINITION,
        INBUILT_FUNCTION
    }
    public enum KEYWORD {
        CONSTANT,
        STATIC,
        PUBLIC,
        PRIVATE,
        VARIANT,
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        LIST,
        DICTIONARY,
        BREAK,
        CONTINUE,
        RETURN,
        BREAKPOINT,
        IF,
        ELSE,
        ELSE_IF,
        MATCH,
        CASE,
        DEFAULT,
        FOR,
        WHILE,
        CLASS,
        FUNCTION,
        STRUCTURE,
        ASSERT,
        PRINT,
        RANGE,
        RANDOM,
        INPUT
    };
    public TYPE Type {private set; get;}
    public KEYWORD Keyword {private set; get;}
    public KeywordToken(TYPE type, KEYWORD keyword, TokenPosition position) : base(position) {
        Type = type;
        Keyword = keyword;
    }
    public override string ToString() {
        return $"({Type}, {Keyword})";
    }
}