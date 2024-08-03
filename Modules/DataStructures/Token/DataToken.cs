public class DataToken : Token {
    public enum TYPE {
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        OBJECT,
        VARIANT,
        IDENTIFIER
    }
    public TYPE Type;
    public object Data;
    public DataToken(TYPE type, object data, TokenPosition position) : base(position) {
        Type = type;
        Data = data;
    }
    public override string ToString() {
        return $"({Type}, {Data})";
    }
}