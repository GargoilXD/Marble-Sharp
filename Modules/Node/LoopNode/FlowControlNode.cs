public class FlowControlNode : Node {
    public FlowController.TYPE Type;
    public Node Data;
    public FlowControlNode(FlowController.TYPE type, Node data, TokenPosition position) : base(position){
        Type = type;
        Data = data;
    }
    public override string ToString() {
        return $"({Type} {Data})";
    }
}
