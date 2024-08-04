public abstract class Node {
    public TokenPosition Position {private set; get;}
    public Node(TokenPosition position) {
        Position = position;
    }
}
