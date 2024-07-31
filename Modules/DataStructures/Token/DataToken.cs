public class DataToken : Token {
    public enum DATATYPE {
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

    public DATATYPE Data_type { get; private set; }

    public DataToken(DATATYPE data_type = DATATYPE.NONE, TokenPosition position = null, object tokenValue = null) {
        Data_type = data_type;
        Token_value = tokenValue;
        Position = position;
        Type = TYPE.DATA;
    }

    public static DATATYPE Convert(string dataType) {
        return dataType switch {
            "Variant" => DATATYPE.VARIANT,
            "Boolean" => DATATYPE.BOOLEAN,
            "Integer" => DATATYPE.INTEGER,
            "Float" => DATATYPE.FLOAT,
            "String" => DATATYPE.STRING,
            "List" => DATATYPE.LIST,
            "Dictionary" => DATATYPE.DICTIONARY,
            "Enumeration" => DATATYPE.ENUMERATION,
            "Object" => DATATYPE.OBJECT,
            _ => DATATYPE.NONE
        };
    }

    public override string ToString() {
        return $"({Data_type}, {Token_value})";
    }
}