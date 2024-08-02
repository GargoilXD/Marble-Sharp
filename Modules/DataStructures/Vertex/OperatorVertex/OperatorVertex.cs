public class OperatorVertex : Vertex{
    public OperatorToken Operator;
    public OperatorVertex(OperatorToken _operator, TokenPosition position = null) : base(position) {
        Operator = _operator;
    }

}
