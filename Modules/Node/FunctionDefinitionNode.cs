using System.Collections.Generic;
public class FunctionDefinitionNode : Node {
    public Token Identifier {private set; get;}
    public List<Node> Arguments {private set; get;}
    public InstructionListNode Instructions {private set; get;}
    public FunctionDefinitionNode(Token identifier, List<Node> arguments, InstructionListNode instructions, TokenPosition position) : base(position){
        Identifier = identifier;
        Arguments = arguments;
        Instructions = instructions;
    }
    public override string ToString() {
        return $"(Function Definition: {Identifier}, ({string.Join(", ", Arguments)}), ({Instructions}))";
    }
}
