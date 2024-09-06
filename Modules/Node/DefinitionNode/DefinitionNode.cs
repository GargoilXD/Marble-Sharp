public abstract class DefinitionNode : Node {
    public KeywordToken AccessModeModifier;
    public KeywordToken StaticModifier;
    public Node Datatype;
    public DataToken Identifier;
    public DefinitionNode(KeywordToken access_mode_modifier, KeywordToken static_modifier, Node datatype, DataToken identifier, TokenPosition position) : base(position) {
        AccessModeModifier = access_mode_modifier;
        StaticModifier = static_modifier;
        Datatype = datatype;
        Identifier = identifier;
    }
}