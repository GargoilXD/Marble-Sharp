using System.Collections.Generic;
public abstract class DefinitionNode : Node {
    public List<KeywordToken> Settings {private set; get;}
    public DataToken Identifier {private set; get;}
    public DefinitionNode(List<KeywordToken> settings, DataToken identifier, TokenPosition position) : base(position){
        Settings = settings;
        Identifier = identifier;
    }
    public override string ToString() {
        return $"(Definition: Settings: {string.Join(", ", Settings)}, Identifier: {Identifier})";
    }
}
