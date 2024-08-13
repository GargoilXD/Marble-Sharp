public class MarbleInstructionList {
    public InstructionListNode Instructions;
    public MarbleInstructionList(InstructionListNode instructions) {
        Instructions = instructions;
    }
    public override string ToString() {
        return Instructions.ToString();
    }
}