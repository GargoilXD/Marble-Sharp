using System.Collections.Generic;
public class FunctionDefinitionNode : DefinitionNode {
    public bool UnlimitedArguments;
    public List<Node> Arguments;
    public OperandInstructionNode Instructions;
    public FunctionDefinitionNode(KeywordToken access_mode_modifier, KeywordToken static_modifier, bool unlimited_arguments, Node datatype, DataToken identifier, List<Node> arguments, OperandInstructionNode instructions, TokenPosition position) : base(access_mode_modifier, static_modifier, datatype, identifier, position) {
        UnlimitedArguments = unlimited_arguments;
        Arguments = arguments;
        Instructions = instructions;
    }
    public override string ToString() {
        string output = "Function: ";
        foreach (object element in new object[] {AccessModeModifier, StaticModifier, Datatype, Identifier}) {
            if (element != null) output += $"{element}, ";
        }
        return $"{output.Remove(output.Length - 2)}{(UnlimitedArguments? " Unlimited" : "")} Arguments: ({string.Join(", ", Arguments)})";
    }
}