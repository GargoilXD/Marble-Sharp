public class DataToken : Token {
    public enum TYPE {
        NONE,
        VARIANT,
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        LIST,
        DICTIONARY,
        ENUMERATION,
        OBJECT,
        IDENTIFIER,
        SELECTOR,
        FUNCTION,
        PARAMETER,
        INSTRUCTIONS
    }
    public TYPE Type;
    public object Data;
    public DataToken(TYPE type, object data, TokenPosition position) : base(TOKEN_TYPE.DATA, position) {
        Type = type;
        Data = data;
    }
    public static TYPE Convert(string dataType) {
        return dataType switch {
            "Variant" => TYPE.VARIANT,
            "Boolean" => TYPE.BOOLEAN,
            "Integer" => TYPE.INTEGER,
            "Float" => TYPE.FLOAT,
            "String" => TYPE.STRING,
            "List" => TYPE.LIST,
            "Dictionary" => TYPE.DICTIONARY,
            "Enumeration" => TYPE.ENUMERATION,
            "Object" => TYPE.OBJECT,
            _ => TYPE.NONE
        };
    }
    public override string ToString() {
        return $"({Type}, {Data})";
    }
}