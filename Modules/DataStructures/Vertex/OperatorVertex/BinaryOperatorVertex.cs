public class BinaryOperatorVertex : OperatorVertex {
    public Vertex Left;
    public Vertex Right;
    public BinaryOperatorVertex(Vertex left, OperatorToken operator_token, Vertex right) : base(operator_token) {
        Left = left;
        Operator = operator_token;
        Right = right;
    }

    public override string ToString() {
        return $"({Left}, {Operator.Operator_type}, {Right})";
    }
}
