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
    public TYPE Type {private set; get;}
    public object Data {private set; get;}
    public DataToken(TYPE type, object data, TokenPosition position) : base(position) {
        Type = type;
        Data = data;
    }
    public override string ToString() {
        return $"({Type}, {Data.ToString().ReplaceLineEndings("; ")})";
    }
}