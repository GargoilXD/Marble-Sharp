public class UnaryOperatorVertex : OperatorVertex {
    public Vertex Operand;
    public UnaryOperatorVertex(Token operator_token, Vertex operand) : base(operator_token) {
        Operand = operand;
        Position = operator_token.Position + Operand.Position;
    }
    public override string ToString() {
        return $"({Operator}, {Operand})";
    }
}
