using System.Collections.Generic;
public class StructureDefinitionNode : DefinitionNode {
    public List<VariableDefinitionNode> VariableDefinitionNodes {private set; get;}
    public StructureDefinitionNode(List<KeywordToken> settings, DataToken identifier, List<VariableDefinitionNode> variable_definition_nodes, TokenPosition position) : base(settings, identifier, position) {
        VariableDefinitionNodes = variable_definition_nodes;
    }
    public override string ToString() {
        return $"(Structure Definition: Settings: {string.Join(", ", Settings)}, Identifier: {Identifier}\nVariables: {string.Join(", ", VariableDefinitionNodes)})";
    }
}
