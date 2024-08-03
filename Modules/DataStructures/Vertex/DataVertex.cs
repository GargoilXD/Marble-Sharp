using System.Collections.Generic;

public class DataVertex : Vertex {
    public enum TYPE {
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        OBJECT,
        VARIANT,
        IDENTIFIER,
        LIST,
        TUPLE,
        DICTIONARY
    }
    public TYPE Type;
    public object Data;
    public DataVertex(TYPE type, object data, TokenPosition position) {
        Type = type;
        Data = data;
        Position = position;
    }
    public static DataVertex FromToken(DataToken token) {
        return new DataVertex((TYPE) token.Type, token.Data, token.Position);
    }
    public override string ToString() {
        switch (Type) {
            case TYPE.LIST:
                return $"({Type}, {string.Join(", ", Data as List<Vertex>)})";
            case TYPE.DICTIONARY:
                return $"({Type}, {string.Join(", ", Data as Dictionary<Vertex, Vertex>)})";
            default:
                return $"({Type}, {Data})";
        }
    }
}
