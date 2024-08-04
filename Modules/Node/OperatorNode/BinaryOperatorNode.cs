public class BinaryOperatorNode : OperatorNode {
    public Node Left {private set; get;}
    public Node Right {private set; get;}
    public BinaryOperatorNode(Node left, OperatorToken operator_token, Node right) : base(operator_token, left.Position + operator_token.Position + right.Position){
        Left = left;
        Right = right;
        //operator_token.Type == OperatorToken.OPERATOR.RUNS? left.Position + right.Position : left.Position + operator_token.Position + right.Position
    }
    public override string ToString() {
        return $"({Left}, {Operator}, {Right})";
    }
}
