public class TokenPosition {
    public int StartPoint { get; private set; }
    public int EndPoint { get; private set; }
    public int StartLine { get; private set; }
    public int EndLine { get; private set; }
    public TokenPosition(int start_point = 0, int end_point = 0, int start_line = 0, int end_line = 0) {
        StartPoint = start_point;
        EndPoint = end_point;
        StartLine = start_line;
        EndLine = end_line;
    }
    public TokenPosition(int index, int line) {
        StartPoint = index;
        EndPoint = index;
        StartLine = line;
        EndLine = line;
    }
    public TokenPosition Duplicate() {
        return new TokenPosition(StartPoint, EndPoint, StartLine, EndLine);
    }
    public override string ToString() {
        string point = (StartPoint == EndPoint) ? $"Point: ({StartPoint})" : $"Point: ({StartPoint} - {EndPoint})";
        string line = (StartLine == EndLine) ? $", Line: ({StartLine})" : $", Line: ({StartLine} - {EndLine})";
        return point + line;
    }
    public static TokenPosition operator +(TokenPosition left, TokenPosition right) {
        return new TokenPosition(left.StartPoint, right.EndPoint, left.StartLine, right.EndLine);
    }
}
