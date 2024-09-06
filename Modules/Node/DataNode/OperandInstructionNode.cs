using System.Collections.Generic;
public class OperandInstructionNode : Node {
    public List<Node> Instructions {private set; get;}
    public bool Oneline;
    public OperandInstructionNode(List<Node> instructions, bool oneline, TokenPosition position) : base(position){
        Instructions = instructions;
        Oneline = oneline;
    }
    public override string ToString() {
        return $"({string.Join("\n", Instructions)})";
    }
}
