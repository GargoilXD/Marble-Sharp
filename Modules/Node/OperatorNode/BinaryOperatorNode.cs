public class BinaryOperatorNode : OperatorNode {
    public Node Left {private set; get;}
    public OperatorToken Operator {private set; get;}
    public Node Right {private set; get;}
    public BinaryOperatorNode(Node left, OperatorToken operator_token, Node right) : base(left.Position + operator_token.Position + right.Position){
        Left = left;
        Operator = operator_token;
        Right = right;
    }
    public override string ToString() {
        return $"({Left}, {Operator}, {Right})";
    }
}
