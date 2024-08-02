public partial class KeywordVertex : Vertex {
    public KeywordToken.KEYWORD Keyword_type;
    public string Keyword;
    public KeywordVertex(KeywordToken.KEYWORD keyword_type, TokenPosition position, string keyword) : base(position) {
        Keyword_type = keyword_type;
        Keyword = keyword;
    }
    public static KeywordVertex FromToken(KeywordToken token) {
	    return new KeywordVertex(token.Keyword, token.Position, token.Token_value.ToString());
    }

    public override string ToString() {
        return $"({Keyword_type}, {Keyword})";
    }
}
