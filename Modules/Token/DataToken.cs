public class DataToken : Token {
    public enum TYPE {
        NULL,
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        WORD
    }
    public TYPE Type {private set; get;}
    public object Data {private set; get;}
    public DataToken(TYPE type, object data, TokenPosition position) : base(position) {
        Type = type;
        Data = data;
    }
    public override string ToString() {
        if (Type == TYPE.STRING) return $"({Type}, '{Data}')";
        if (Type == TYPE.NULL) return $"({Type}, 'null')";
        return $"({Type}, {Data.ToString().ReplaceLineEndings("; ")})";
    }
}