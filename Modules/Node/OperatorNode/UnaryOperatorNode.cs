public class UnaryOperatorNode : OperatorNode {
    public Node Operand {private set; get;}
    public UnaryOperatorNode(Token operator_token, Node operand) : base(operator_token, operator_token.Position + operand.Position) {
        Operand = operand;
    }
    public override string ToString() {
        return $"({Operator}, {Operand})";
    }
}
