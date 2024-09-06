public class VariableDefinitionNode : DefinitionNode {
    public KeywordToken ConstantModifier;
    public KeywordToken ReferenceModifier;
    public Node Value;
    public VariableDefinitionNode(KeywordToken access_mode_modifier, KeywordToken static_modifier, KeywordToken constant_modifier, KeywordToken reference_modifier, Node datatype, DataToken identifier, Node value, TokenPosition position) : base(access_mode_modifier, static_modifier, datatype, identifier, position) {
        ConstantModifier = constant_modifier;
        ReferenceModifier = reference_modifier;
        Value = value;
    }
    public override string ToString() {
        string output = "Variable: ";
        foreach (object element in new object[] {AccessModeModifier, StaticModifier, ConstantModifier, Datatype, Identifier, Value}) {
            if (element != null) output += $"{element}, ";
        }
        return output.Remove(output.Length - 2);
    }
}