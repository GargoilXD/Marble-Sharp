public class ForNode : Node {
    public DataToken Iterator {private set; get;}
    public Node Iteratable {private set; get;}
    public OperandInstructionNode Instructions {private set; get;}
    public ForNode(DataToken iterator, Node iteratable, OperandInstructionNode instructions, TokenPosition position) : base(position){
        Iterator = iterator;
        Iteratable = iteratable;
        Instructions = instructions;
    }
    public override string ToString() {
        return $"(For: {Iterator} in {Iteratable}, {Instructions})";
    }
}
