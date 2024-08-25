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
        CONSTANT,
        STATIC,
        PUBLIC,
        PRIVATE,
        VOID,
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
        FUNCTION,
        CLASS,
        STRUCTURE,
        CONSTRUCTOR,
        ENUMERATION,
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