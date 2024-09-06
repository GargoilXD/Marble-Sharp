using System.Collections.Generic;
public class ConstructorNode : DefinitionNode {
    public bool UnlimitedArguments;
    public List<Node> Arguments;
    public List<Node> BaseParameters;
    public OperandInstructionNode Instructions;
    public ConstructorNode(KeywordToken access_mode_modifier, KeywordToken static_modifier, bool unlimited_arguments, List<Node> arguments, List<Node> base_parameters, OperandInstructionNode instructions, TokenPosition position) : base(access_mode_modifier, static_modifier, null, null, position) {
        UnlimitedArguments = unlimited_arguments;
        Arguments = arguments;
        BaseParameters = base_parameters;
        Instructions = instructions;
    }
    public override string ToString() {
        string output = "Constructor: ";
        foreach (object element in new object[] {AccessModeModifier, StaticModifier, Datatype, Identifier}) {
            if (element != null) output += $"{element}, ";
        }
        return $"{output.Remove(output.Length - 2)}{(UnlimitedArguments? " Unlimited" : "")} Arguments: ({string.Join(", ", Arguments)}) BaseParameters: ({string.Join(", ", BaseParameters)})";
    }
}