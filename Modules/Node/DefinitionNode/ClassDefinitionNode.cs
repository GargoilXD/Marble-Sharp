public class ClassDefinitionNode : DefinitionNode {
    public OperandInstructionNode Definition;
    public ClassDefinitionNode(KeywordToken access_mode_modifier, KeywordToken static_modifier, Node datatype, DataToken identifier, OperandInstructionNode definition, TokenPosition position) : base(access_mode_modifier, static_modifier, datatype, identifier, position) {
        Definition = definition;
    }
    public override string ToString() {
        string output = "Class: ";
        foreach (object element in new object[] {AccessModeModifier, StaticModifier, Datatype, Identifier}) {
            if (element != null) output += $"{element}, ";
        }
        return output.Remove(output.Length - 2);
    }
}