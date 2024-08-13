using System.Collections.Generic;
public class FunctionNode : Node {
    public Token Identifier {private set; get;}
    public List<Node> Arguments {private set; get;}
    public FunctionNode(Token identifier, List<Node> arguments, TokenPosition position) : base(position){
        Identifier = identifier;
        Arguments = arguments;
    }
    public override string ToString() {
        return $"(Function: {Identifier}, ({string.Join(", ", Arguments)}))";
    }
}
