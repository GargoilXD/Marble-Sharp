public class FunctionParameterNode : Node  {
    public Node Parameter;
    public FunctionParameterNode(Node parameter, TokenPosition position) : base(position) {
        Parameter = parameter;
    }
    public override string ToString() {
        return $"{Parameter}";
    }
    public class Classified : FunctionParameterNode {
        public Node Identifier;
        public Classified(Node identifier, Node parameter, TokenPosition position) : base(parameter, position) {
            Identifier = identifier;
        }
        public override string ToString() {
            return $"{Identifier}: {Parameter}";
        }
    }
}