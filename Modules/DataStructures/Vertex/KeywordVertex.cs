public partial class KeywordVertex : Vertex {
    public KeywordToken.TYPE Type;
    public KeywordToken.KEYWORD Keyword;
    public KeywordVertex(KeywordToken.TYPE keyword_type, KeywordToken.KEYWORD keyword, TokenPosition position) {
        Type = keyword_type;
        Keyword = keyword;
        Position = position;
    }
    public static KeywordVertex FromToken(KeywordToken token) {
	    return new KeywordVertex(token.Type, token.Keyword, token.Position);
    }

    public override string ToString() {
        return $"({Type}, {Keyword})";
    }
}
