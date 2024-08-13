using System.Collections.Generic;
public class InstructionListNode : Node {
    public List<Node> Instructions {private set; get;}
    public InstructionListNode(List<Node> instructions, TokenPosition position) : base(position){
        Instructions = instructions;
    }
    public override string ToString() {
        return $"({string.Join("\n", Instructions)})";
    }
}
