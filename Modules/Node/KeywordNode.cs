public partial class KeywordNode : Node {
    public KeywordToken.TYPE Type {private set; get;}
    public KeywordToken.KEYWORD Keyword {private set; get;}
    public KeywordNode(KeywordToken.TYPE keyword_type, KeywordToken.KEYWORD keyword, TokenPosition position) : base(position) {
        Type = keyword_type;
        Keyword = keyword;
    }
    public static KeywordNode FromToken(KeywordToken token) {
	    return new KeywordNode(token.Type, token.Keyword, token.Position);
    }
    public override string ToString() {
        return $"({Type}, {Keyword})";
    }
}
