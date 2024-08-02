public class DataVertex : Vertex {
    public DataToken.DATATYPE Data_type;
    public object Data;
    public DataVertex(DataToken.DATATYPE type, TokenPosition position, object data) : base(position) {
        Data_type = type;
        Data = data;
    }
    public static DataVertex FromToken(DataToken token) {
        return new DataVertex(token.Data_type, token.Position, token.Token_value);
    }

    public override string ToString() {/*
        if (Data_type == DataToken.DATATYPE.FUNCTION) {
            return "{" + $"Subroutine: {Data.Identifier}<{Data.Parameters}>" + "}";
        }
        if (Data_type == DataToken.DATATYPE.SELECTOR) {
		    return "{" + $"Selector: {Data.Identifier}<{Data.Parameters}>" + "}";
        }*/
        return $"({Data})";
    }
}
