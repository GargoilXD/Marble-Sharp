public class TokenPosition {
    public int StartPoint { get; private set; }
    public int EndPoint { get; private set; }
    public int StartLine { get; private set; }
    public int EndLine { get; private set; }

    public TokenPosition(int startPoint = 0, int endPoint = 0, int startLine = 0, int endLine = 0) {
        StartPoint = startPoint;
        EndPoint = endPoint;
        StartLine = startLine;
        EndLine = endLine;
    }

    public TokenPosition Extend(TokenPosition position) {
        EndPoint = position.EndPoint;
        EndLine = position.EndLine;
        return this;
    }

    public TokenPosition Duplicate() {
        return new TokenPosition(StartPoint, EndPoint, StartLine, EndLine);
    }

    public override string ToString() {
        string point = (StartPoint == EndPoint) ? $"Point: ({StartPoint})" : $"Point: ({StartPoint} - {EndPoint})";
        string line = (StartLine == EndLine) ? $", Line: ({StartLine})" : $", Line: ({StartLine} - {EndLine})";
        return point + line;
    }
}
