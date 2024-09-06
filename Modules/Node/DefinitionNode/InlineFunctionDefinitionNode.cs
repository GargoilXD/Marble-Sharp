using System.Collections.Generic;
public class InlineFunctionDefinitionNode : Node {
    public bool UnlimitedArguments;
    public List<Node> Arguments;
    public OperandInstructionNode Instructions;
    public InlineFunctionDefinitionNode(bool unlimited_arguments, List<Node> arguments, OperandInstructionNode instructions, TokenPosition position) : base(position) {
        UnlimitedArguments = unlimited_arguments;
        Arguments = arguments;
        Instructions = instructions;
    }
    public override string ToString() {
        return $"(InlineFunction: Arguments: ({string.Join(", ", Arguments)}))";
    }
}