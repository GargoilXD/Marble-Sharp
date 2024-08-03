public class KeywordToken : Token {
    public enum TYPE {
        MODIFIER,
        DATATYPE,
        FLOWCONTROL,
        DECISION,
        LOOP,
        DEFINITION,
        INBUILT_FUNCTION
    }
    public enum KEYWORD {
        CONST,
        STATIC,
        PUBLIC,
        PRIVATE,
        SELF,
        VARIANT,
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        LIST,
        DICTIONARY,
        ENUMERATION,
        OBJECT,
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
    public TYPE Type;
    public KEYWORD Keyword;
    public KeywordToken(TYPE type, KEYWORD keyword, TokenPosition position) : base(position) {
        Type = type;
        Keyword = keyword;
    }
    public override string ToString() {
        return $"({Type}, {Keyword})";
    }
}