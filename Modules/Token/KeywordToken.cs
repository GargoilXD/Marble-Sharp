public class KeywordToken : Token {
    public enum TYPE {
        MODIFIER,
        DATATYPE,
        FLOWCONTROL,
        DECISION,
        LOOP,
        DEFINITION,
        INBUILT_FUNCTION,
        EXCEPTION_HANDLING
    }
    public enum KEYWORD {
        CONSTANT,
        STATIC,
        PUBLIC,
        PRIVATE,
        CLASSIFIED,
        UNLIMITED,
        REFERENCE,
        VOID,
        VARIANT,
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        LIST,
        DICTIONARY,
        CALLABLE,
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
        FUNCTION,
        CLASS,
        STRUCTURE,
        VARIABLE,
        CONSTRUCTOR,
        ENUMERATION,
        ASSERT,
        PRINT,
        PRINTLINE,
        RANGE,
        RANDOM,
        INPUT,
        TRY,
        CATCH
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