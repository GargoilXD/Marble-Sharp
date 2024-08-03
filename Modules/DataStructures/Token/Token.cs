public class Token {
    public TokenPosition Position;
    public Token(TokenPosition position) {
        Position = position;
    }
    public bool is_data(DataToken.TYPE type) => (this is DataToken) && (this as DataToken).Type == type;
    public bool is_keyword(KeywordToken.KEYWORD keyword) => (this is KeywordToken) && (this as KeywordToken).Keyword == keyword;
    public bool is_operator(OperatorToken.OPERATOR type) => (this is OperatorToken) && (this as OperatorToken).Type == type;
    public bool is_symbol(SymbolToken.SYMBOL symbol) => (this is SymbolToken) && (this as SymbolToken).Symbol == symbol;
}
