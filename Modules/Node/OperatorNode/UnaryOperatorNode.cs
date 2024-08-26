public class UnaryOperatorNode : OperatorNode {
    public Token Operator {private set; get;}
    public Node Operand {private set; get;}
    public UnaryOperatorNode(Token operator_token, Node operand) : base(operator_token.Position + operand.Position) {
        Operator = operator_token;
        Operand = operand;
    }
    public override string ToString() {
        return $"({Operator}, {Operand})";
    }
}
