public abstract class OperatorNode : Node{
    public Token Operator {private set; get;}
    public OperatorNode(Token operator_token, TokenPosition position) : base(position) {
        Operator = operator_token;
    }
}
