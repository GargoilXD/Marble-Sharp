public abstract class OperatorVertex : Vertex{
    public Token Operator;
    public OperatorVertex(Token operator_token) {
        Operator = operator_token;
    }
}
