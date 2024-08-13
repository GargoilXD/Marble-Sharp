using System.Collections.Generic;
public class VariableDefinitionNode : DefinitionNode {
    public Token Datatype {private set; get;}
    public Node Value {private set; get;}
    public VariableDefinitionNode(List<KeywordToken> settings, DataToken identifier, Token datatype, Node value, TokenPosition position) : base(settings, identifier, position) {
        Datatype = datatype;
        Value = value;
    }
    public override string ToString() {
        return $"(Variable Definition: Settings: {string.Join(", ", Settings)}, Datatype:{Datatype}, Identifier: {Identifier}, Value: {Value})";
    }
}
