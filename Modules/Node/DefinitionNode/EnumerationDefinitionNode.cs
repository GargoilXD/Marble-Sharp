
using System.Collections.Generic;
public class EnumerationDefinitionNode : DefinitionNode {
    public List<Node> Enumerations {private set; get;}
    public EnumerationDefinitionNode(List<KeywordToken> settings, DataToken identifier, List<Node> enumerations, TokenPosition position) : base(settings, identifier, position) {
        Enumerations = enumerations;
    }
    public override string ToString() {
        return $"(Enumeration Definition: Settings: {string.Join(", ", Settings)}, Identifier: {Identifier}, Enumeration: {string.Join(", ", Enumerations)})";
    }
}
