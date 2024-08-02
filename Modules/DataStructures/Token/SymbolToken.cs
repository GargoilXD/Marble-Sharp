public class SymbolToken : Token {
    enum SYMBOL {
        LEFT_CURLY_BRACKET,
        RIGHT_CURLY_BRACKET,
        LEFT_SQUARE_BRACKET,
        RIGHT_SQUARE_BRACKET,
        LEFT_CIRCLE_BRACKET,
        RIGHT_CIRCLE_BRACKET,
        END_OF_LINE,
        END
    }
    public TYPE Symbol;
    public SymbolToken(SYMBOL symbol, TokenPosition position) : base(TOKEN_TYPE.SYMBOL, position) {
        Symbol = symbol;
    }
    public override string ToString() {
        return $"({Symbol})";
    }
}
