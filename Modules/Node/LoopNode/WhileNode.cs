public class WhileNode : Node {
    public Node Expression {private set; get;}
    public OperandInstructionNode Instructions {private set; get;}
    public WhileNode(Node expression, OperandInstructionNode instructions, TokenPosition position) : base(position){
        Expression = expression;
        Instructions = instructions;
    }
    public override string ToString() {
        return $"(While: {Expression}, {Instructions})";
    }
}
