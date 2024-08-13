public abstract class Token {
    public TokenPosition Position {private set; get;}
    public Token(TokenPosition position) {
        Position = position;
    }
    public bool is_data(DataToken.TYPE type) => (this is DataToken) && (this as DataToken).Type == type;
    public bool is_keyword(KeywordToken.KEYWORD keyword) => (this is KeywordToken) && (this as KeywordToken).Keyword == keyword;
    public bool is_keyword_type(KeywordToken.TYPE type) => (this is KeywordToken) && (this as KeywordToken).Type == type;
    public bool is_operator(OperatorToken.OPERATOR type) => (this is OperatorToken) && (this as OperatorToken).Type == type;
    public bool is_symbol(SymbolToken.SYMBOL symbol) => (this is SymbolToken) && (this as SymbolToken).Symbol == symbol;
    public bool is_data(DataToken.TYPE type, out DataToken data_token) {
        if (this is DataToken) {
            data_token = this as DataToken;
            if (data_token.Type == type) {
                return true;
            }
        }
        data_token = null;
        return false;
    }
    public bool is_keyword(KeywordToken.KEYWORD keyword, out KeywordToken keyword_token) {
        if (this is KeywordToken) {
            keyword_token = this as KeywordToken;
            if (keyword_token.Keyword == keyword) {
                return true;
            }
        }
        keyword_token = null;
        return false;
    }
    public bool is_keyword_type(KeywordToken.TYPE type, out KeywordToken keyword_token) {
        if (this is KeywordToken) {
            keyword_token = this as KeywordToken;
            if (keyword_token.Type == type) {
                return true;
            }
        }
        keyword_token = null;
        return false;
    }
    public bool is_operator(OperatorToken.OPERATOR type, out OperatorToken operator_token) {
        if (this is OperatorToken) {
            operator_token = this as OperatorToken;
            if (operator_token.Type == type) {
                return true;
            }
        }
        operator_token = null;
        return false;
    }
    public bool is_symbol(SymbolToken.SYMBOL symbol, out SymbolToken symbol_token) {
        if (this is SymbolToken) {
            symbol_token = this as SymbolToken;
            if (symbol_token.Symbol == symbol) {
                return true;
            }
        }
        symbol_token = null;
        return false;
    }
}
