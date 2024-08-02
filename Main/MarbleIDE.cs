using Godot;
using System.Collections.Generic;

public partial class MarbleIDE : Control {
    public enum DISPLAY {
        TOKENIZER,
        PARSER,
        INTERPRETER
    }
    public static string EditorCode = "";
    private DISPLAY Stop_at;
    private SaveData Save_data;
    private CodeEdit Editor;
    private TabContainer Stage_tabs;
    private OptionButton Stage_option_button;
    private RichTextLabel[] Displays;
    private AcceptDialog Input_Dialog;

    Tokenizer Tokenizer_object = new Tokenizer();
    Parser Parser_object =  new Parser();
    //var Interpreter_object:Interpreter = Interpreter.new ()

    public override void _Ready() {
        Save_data = (SaveData) ResourceLoader.Load("res://Configurations/Save_data.tres");
        Editor = (CodeEdit) GetNode("%Editor");
        Stage_tabs = (TabContainer) GetNode("%StageTabs");
        Stage_option_button = (OptionButton) GetNode("%StageOptionButton");
        Displays = new RichTextLabel[] {
            (RichTextLabel) GetNode("%TokenizerOutput"),
            (RichTextLabel) GetNode("%ParserOutput"),
            (RichTextLabel) GetNode("%InterpreterOutput")
        };
        Input_Dialog = (AcceptDialog) GetNode("%InputDialog");
        Editor.Text = Save_data.Code;
        Stop_at = Save_data.Stop_at;
        Stage_tabs.CurrentTab = (int) Stop_at;
        Stage_option_button.Select((int) Stop_at);
        Stage_option_button.ItemSelected += delegate (long index) {
            Stop_at = (DISPLAY) index;
            Stage_tabs.CurrentTab = (int) index;
        };
        GetNode<Button>("%Run").Pressed += delegate {
            if (Editor.Text != "") {
                EditorCode = Editor.Text;
                Run();
            }
        };
        GetNode<Button>("%Clear").Pressed += delegate {
            foreach (RichTextLabel display in Displays) {
                display.Clear();
            }
        };


        //Interpreter_object.PopUp_input = accept_dialog;
        CodeHighlighter Highlighter = new CodeHighlighter();
        Highlighter.NumberColor = Color.FromString("LIGHT_GREEN", Color.Color8(0,0,0));
        Highlighter.SymbolColor = Color.FromString("AQUA", Color.Color8(0, 0, 0));
        Highlighter.FunctionColor = Color.FromString("CORNFLOWER_BLUE", Color.Color8(0, 0, 0));
        Highlighter.MemberVariableColor = Color.FromString("LIGHT_BLUE", Color.Color8(0, 0, 0));

        foreach (string keyword in Tokenizer.MODIFIER_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("CRIMSON", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.DATA_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("INDIAN_RED", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.DATATYPE_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("FIREBRICK", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.OPERATOR_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("FIREBRICK", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.FLOWCONTROL_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("PURPLE", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.DECISION_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("YELLOW", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.LOOP_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("PURPLE", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.INSTRUCTION_SET_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("CADET_BLUE", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.FUNCTION_KEYWORDS) {
            Highlighter.KeywordColors[keyword] = Color.FromString("SEA_GREEN", Color.Color8(0, 0, 0));
        }
        Highlighter.AddColorRegion("\"", "\"", Color.FromString("GREEN_YELLOW", Color.Color8(0, 0, 0)));
        Highlighter.AddColorRegion("'", "'", Color.FromString("GREEN_YELLOW", Color.Color8(0, 0, 0)));
        Highlighter.AddColorRegion("#", "", Color.FromString("DIM_GRAY", Color.Color8(0, 0, 0)), true);
        Editor.SyntaxHighlighter = Highlighter;
    }
    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed("Save")) {
            Save_data.Code = Editor.Text;
            Save_data.Stop_at = Stop_at;
            Error State = ResourceSaver.Save(Save_data, "res://Configurations/Save_data.tres");
            if (State == Error.Ok) {
                Displays[(int) Stop_at].AppendText("Save Successful");
                Displays[(int) Stop_at].Newline();
            }
            else {
                Displays[(int) Stop_at].AppendText("Save Failed");
                Displays[(int) Stop_at].Newline();
            }
        }
    }

    public void Run() {
        List<Token> tokens;
        try {
            tokens = Tokenizer_object.Tokenize();
            string output = "";
            foreach (Token token in tokens) {
                output += token.ToString() + ", ";
                if (token.Type == Token.TYPE.END_OF_LINE) {
                    output += '\n';
                }
            }
            Display_data(DISPLAY.TOKENIZER, output);
            if (Stop_at == DISPLAY.TOKENIZER){
                Stage_tabs.CurrentTab = (int) DISPLAY.TOKENIZER;
                return;
            }
            
        } catch (MarbleError error) {
            Display_data(DISPLAY.TOKENIZER, error);
            Stage_tabs.CurrentTab = (int) DISPLAY.TOKENIZER;
            return;
        }
        try {
            List<Vertex> vertexes = Parser_object.Parse(tokens);
            string output = "";
            foreach (Vertex vertex in vertexes) {
                output += vertex.ToString() + '\n';
            }
            Display_data(DISPLAY.PARSER, output);
            if (Stop_at == DISPLAY.PARSER){
                Stage_tabs.CurrentTab = (int) DISPLAY.PARSER;
                return;
            }

        } catch (MarbleError error){
            Display_data(DISPLAY.PARSER, error);
            Stage_tabs.CurrentTab = (int) DISPLAY.PARSER;
            return;
        }
    }

    private void Display_data(DISPLAY display, object data) {
        Displays[(int) display].AppendText(data.ToString());
        Displays[(int) display].Newline();
    }
}
