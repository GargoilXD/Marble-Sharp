using System;
using System.Collections.Generic;

public class MarbleError : Exception {
    public enum TYPE {
        NONE,
        INVALID_CHARACTER,
        UNIDENTIFIED_OPERATOR,
        INCOMPLETE_STRING,
        EXPECTED_OPERAND,
        UNEXPECTED_OPERAND,
        EXPECTED_TOKEN,
        UNDEFINED_IDENTIFIER,
        DATATYPE_MISMATCH,
        ALREADY_DEFINED_IDENTIFIER,
        DIVISION_BY_ZERO,
        UNEXPECTED_TOKEN,
        EXPECTED_IDENTIFIER,
        UNINITIALIZED_IDENTIFIER,
        BREAK,
        CONTINUE,
        RETURN,
        INCOMPATIBLE_TYPES,
        ASSERTION_FAILED,
        MESSAGE
    }
    public TYPE Type { get; private set; }
    public string Details { get; private set; }
    public TokenPosition Position { get; private set; }

    public MarbleError(TYPE type = TYPE.NONE, TokenPosition position = null, string message = "") : base(message) {
        Type = type;
        Position = position;
        Details = message;
    }

    public string DrawPosition() {
        if (Position == null)
            return "";

        int start = Position.StartPoint;
        int end = Position.EndPoint - Position.StartPoint;
        int lineNumber = 0;
        var lines = new List<string>();
        string line = "";
        string fullLength = "";

        foreach (char character in MarbleIDE.EditorCode + '\n') {
            if (lineNumber < Position.StartLine)
                start -= 1;

            if (lineNumber >= Position.StartLine && lineNumber <= Position.EndLine) {
                fullLength += character;
                line += character;
                if (character == '\n') {
                    lines.Add(line);
                    line = "";
                }
            }
            else if (lineNumber > Position.EndLine) {
                break;
            }

            if (character == '\n')
                lineNumber += 1;
        }

        end += start;
        int index = 0;
        int lineIndex = 0;
        int lineSize = lines[lineIndex].Length;

        while (index <= Position.EndPoint && lineIndex < lines.Count) {
            if (index > end)
                break;

            if (index == lineSize) {
                lineIndex += 1;
                lineSize += lines[lineIndex].Length;
            }

            if (index < start) {
                switch (fullLength[index]) {
                    case '\t':
                        lines[lineIndex] += "    ";
                        break;
                    case '\n':
                        lines[lineIndex] += "\n";
                        break;
                    default:
                        lines[lineIndex] += " ";
                        break;
                }
            }
            else if (index >= start && index <= end) {
                switch (fullLength[index]) {
                    case '\n':
                        lines[lineIndex] += "^\n";
                        break;
                    case '\t':
                        lines[lineIndex] += "^^^^";
                        break;
                    default:
                        lines[lineIndex] += "^";
                        break;
                }
            }

            index += 1;
        }

        string output = string.Join("", lines);
        return output;
    }

    public override string ToString() {
        return $"{Type}" + (string.IsNullOrEmpty(Details) ? "" : $" : {Details}") + "\n" + DrawPosition();
    }
}