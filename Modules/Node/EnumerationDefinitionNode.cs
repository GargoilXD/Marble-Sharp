
using System.Collections.Generic;
public class EnumerationDefinitionNode : Node {
    public Node Identifier {private set; get;}
    public List<Node> Enumerations {private set; get;}
    public EnumerationDefinitionNode(Node identifier, List<Node> enumerations, TokenPosition position) : base(position){
        Identifier = identifier;
        Enumerations = enumerations;
    }
    public override string ToString() {
        return $"(Enumeration Definition: {Identifier}, ({string.Join(", ", Enumerations)}))";
    }
}
