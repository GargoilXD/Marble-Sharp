using System;
using System.Collections.Generic;

public class Error : Exception {
    protected TokenPosition Position;
    public Error(TokenPosition position, string message = "") : base(string.IsNullOrEmpty(message)? "" : $" : {message}") {
        Position = position;
    }
    public string IndicateErrorLine() {
        if (Position == null) return "No position";
        string[] code = MarbleIDE.EditorCode.Split('\n');
        List<string> output = new List<string>();
        int line_start_index = 0;
        for (int line = 0; line != Position.StartLine; line ++) line_start_index += code[line].Length + 1;
        for (int line_number = Position.StartLine; line_number <= Position.EndLine; line_number ++){
            string line = code[line_number] + ' ';
            string indications = "";
            int start = Position.StartPoint - line_start_index;
            int end = Position.EndPoint - line_start_index;
            for (int x = 0; x < line.Length; x ++) {
                if (x < start) {
                    switch (line[x]) {
                        case '\t':
                            indications += "    ";
                            break;
                        default:
                            indications += ' ';
                            break;
                    }
                }
                else if (x >= start && x <= end) {
                    switch (line[x]) {
                        case '\t':
                            indications += "^^^^";
                            break;
                        default:
                            indications += '^';
                            break;
                    }
                }
                else {
                    break;
                }
            }
            line_start_index += line.Length;
            output.Add(line);
            output.Add(indications);
        }
       return '\n' + string.Join("\n", output);
    }
    public override string ToString() {
        return $"ERROR{Message}{IndicateErrorLine()}";
    }
}