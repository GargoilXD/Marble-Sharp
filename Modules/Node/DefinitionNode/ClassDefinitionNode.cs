using System.Collections.Generic;
public class ClassDefinitionNode : DefinitionNode {
    public List<VariableDefinitionNode> VariableDefinitionNodes {private set; get;}
    public List<FunctionDefinitionNode> FunctionDefinitionNodes {private set; get;}
    public ClassDefinitionNode(List<KeywordToken> settings, DataToken identifier, List<VariableDefinitionNode> variable_definition_nodes, List<FunctionDefinitionNode> function_definition_nodes, TokenPosition position) : base(settings, identifier, position) {
        VariableDefinitionNodes = variable_definition_nodes;
        FunctionDefinitionNodes = function_definition_nodes;
    }
    public override string ToString() {
        return $"(Class Definition: Settings: {string.Join(", ", Settings)}, Identifier: {Identifier}\nVariables: {string.Join(", ", VariableDefinitionNodes)}\nFunctions: {string.Join(", ", FunctionDefinitionNodes)})";
    }
}
