public class BinaryOperatorVertex : OperatorVertex {
    public Vertex Left;
    public Vertex Right;
    public BinaryOperatorVertex(Vertex left, Token operator_token, Vertex right) : base(operator_token){
        Left = left;
        Right = right;
        Position = Left.Position + operator_token.Position + Right.Position;
    }

    public override string ToString() {
        return $"({Left}, {Operator}, {Right})";
    }
}
