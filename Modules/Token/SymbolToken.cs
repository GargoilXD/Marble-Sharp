public class SymbolToken : Token {
    public enum SYMBOL {
        COMMA,
        LEFT_CURLY_BRACKET,
        RIGHT_CURLY_BRACKET,
        LEFT_SQUARE_BRACKET,
        RIGHT_SQUARE_BRACKET,
        LEFT_CIRCLE_BRACKET,
        RIGHT_CIRCLE_BRACKET,
        END_OF_LINE,
        END
    }
    public SYMBOL Symbol {private set; get;}
    public SymbolToken(SYMBOL symbol, TokenPosition position) : base(position) {
        Symbol = symbol;
    }
    public override string ToString() {
        return $"({Symbol})";
    }
}
