public class TokenPositioner {
    private int Point;
    private int Line;
    public TokenPositioner() {
        Reset();
    }
    public void Reset() {
        Point = 0;
        Line = 0;
    }
    public void Start(int index, int line) {
        Point = index;
        Line = line;
    }
    public TokenPosition End(int index, int line) {
        return new TokenPosition(Point, index - 1, Line, line);
    }
}