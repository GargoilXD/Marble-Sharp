public class FlowControlNode : Node {
    public FlowController.TYPE Type;
    public Node Data;
    public FlowControlNode(Node data, TokenPosition position) : base(position){
        Type = FlowController.TYPE.RETURN;
        Data = data;
    }
    public FlowControlNode(FlowController.TYPE type, TokenPosition position) : base(position){
        Type = type;
        Data = null;
    }
    public override string ToString() {
        return $"({Type} {Data})";
    }
}
