public class ForNode : Node {
    public DataNode Iterator {private set; get;}
    public Node Iteratable {private set; get;}
    public InstructionListNode Instructions {private set; get;}
    public ForNode(DataNode iterator, Node iteratable, InstructionListNode instructions, TokenPosition position) : base(position){
        Iterator = iterator;
        Iteratable = iteratable;
        Instructions = instructions;
    }
    public override string ToString() {
        return $"(For: {Iterator} in {Iteratable}, {Instructions})";
    }
}
