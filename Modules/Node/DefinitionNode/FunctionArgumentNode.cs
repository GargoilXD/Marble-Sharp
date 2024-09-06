public class FunctionArgumentNode : Node  {
    public bool IsClassified;
    public Node Datatype;
    public DataToken Identifier;
    public Node Value;
    public FunctionArgumentNode(bool is_classified, Node datatype, DataToken identifier, Node value, TokenPosition position) : base(position) {
        IsClassified = is_classified;
        Datatype = datatype;
        Identifier = identifier;
        Value = value;

    }
    public override string ToString() {
        return $"({(IsClassified? "Classified " : "")}Argument: {Datatype} {Identifier}{(Value == null? "" : " " + Value)})";
    }
}