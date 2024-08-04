using System.Collections.Generic;

public class DataNode : Node {
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
    public TYPE Type {private set; get;}
    public object Data {private set; get;}
    public DataNode(TYPE type, object data, TokenPosition position) : base(position){
        Type = type;
        Data = data;
    }
    public static DataNode FromToken(DataToken token) {
        return new DataNode((TYPE) token.Type, token.Data, token.Position);
    }
    public override string ToString() {
        switch (Type) {
            case TYPE.LIST:
                return $"({Type}, {string.Join(", ", Data as List<Node>)})";
            case TYPE.DICTIONARY:
                return $"({Type}, {string.Join(", ", Data as Dictionary<Node, Node>)})";
            case TYPE.STRING:
            return $"({Type}, {Data.ToString().ReplaceLineEndings("; ")})";
            default:
                return $"({Type}, {Data})";
        }
    }
}
