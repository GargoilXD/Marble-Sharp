public class WhileNode : Node {
    public Node Expression {private set; get;}
    public InstructionListNode Instructions {private set; get;}
    public WhileNode(Node expression, InstructionListNode instructions, TokenPosition position) : base(position){
        Expression = expression;
        Instructions = instructions;
    }
    public override string ToString() {
        return $"(While: {Expression}, {Instructions})";
    }
}
