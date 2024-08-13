using System.Collections.Generic;
public class FunctionDefinitionNode : DefinitionNode {
    public Token ReturnType {private set; get;}
    public List<Node> Arguments {private set; get;}
    public InstructionListNode Instructions {private set; get;}
    public FunctionDefinitionNode(List<KeywordToken> settings, DataToken identifier, Token return_type, List<Node> arguments, InstructionListNode instructions, TokenPosition position) : base(settings, identifier, position) {
        ReturnType = return_type;
        Arguments = arguments;
        Instructions = instructions;
    }
    public override string ToString() {
        return $"(Function Definition: Settings: {string.Join(", ", Settings)}, Return: {ReturnType}, Identifier: {Identifier}, Arguments: {string.Join(", ", Arguments)}, Instructions: {Instructions})";
    }
}
