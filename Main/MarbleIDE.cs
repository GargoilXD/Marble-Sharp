using Godot;
using System;
using System.Collections.Generic;
public partial class MarbleIDE : Control {
    public enum DISPLAY {
        TOKENIZER,
        PARSER,
        INTERPRETER
    }
    public static string EditorCode {private set; get;} = "";
    private DISPLAY Stop_at;
    private EditorSave Save_data;
    private CodeEdit Editor;
    private TabContainer Stage_tabs;
    private OptionButton Stage_option_button;
    private RichTextLabel[] Displays;
    private InputGetter Input_dialog;
    Tokenizer Tokenizer_object = new Tokenizer();
    Parser Parser_object = new Parser();
    Interpreter Interpreter_object = new Interpreter();
    public override void _Ready() {
        base._Ready();
        Save_data = (EditorSave) ResourceLoader.Load("res://Main/Save/save.tres");
        Editor = (CodeEdit) GetNode("%Editor");
        Stage_tabs = (TabContainer) GetNode("%StageTabs");
        Stage_option_button = (OptionButton) GetNode("%StageOptionButton");
        Displays = new RichTextLabel[] {
            (RichTextLabel) GetNode("%TokenizerOutput"),
            (RichTextLabel) GetNode("%ParserOutput"),
            (RichTextLabel) GetNode("%InterpreterOutput")
        };
        Parser_object.OnNewDatatype = delegate (string new_datatype) {
            (Editor.SyntaxHighlighter as CodeHighlighter).AddKeywordColor(new_datatype, Color.FromString("BROWN", Color.Color8(0,0,0)));
        };
        Parser_object.ClearDatatypes = delegate (List<string> datatypes) {
            foreach (string datatype in datatypes) {
                (Editor.SyntaxHighlighter as CodeHighlighter).RemoveKeywordColor(datatype);
            }
        };
        Input_dialog = (InputGetter) GetNode("%InputDialog");
        Interpreter.Input_dialog = Input_dialog;
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

        CodeHighlighter Highlighter = new CodeHighlighter{
            NumberColor = Color.FromString("LIGHT_GREEN", Color.Color8(0, 0, 0)),
            SymbolColor = Color.FromString("AQUA", Color.Color8(0, 0, 0)),
            FunctionColor = Color.FromString("CORNFLOWER_BLUE", Color.Color8(0, 0, 0)),
            MemberVariableColor = Color.FromString("LIGHT_BLUE", Color.Color8(0, 0, 0))
        };
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["MODIFIER"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("CRIMSON", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["DATA"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("INDIAN_RED", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["DATATYPE"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("FIREBRICK", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["OPERATOR"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("PURPLE", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["FLOW_CONTROL"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("YELLOW", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["DECISION"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("PURPLE", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["LOOP"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("PURPLE", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["DEFINITION"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("CADET_BLUE", Color.Color8(0, 0, 0));
        }
        foreach (string keyword in Tokenizer.KEYWORD_CLASSIFICATIONS["INBUILT_FUNCTION"]) {
            Highlighter.KeywordColors[keyword] = Color.FromString("SEA_GREEN", Color.Color8(0, 0, 0));
        }
        Highlighter.AddColorRegion("\"", "\"", Color.FromString("GREEN_YELLOW", Color.Color8(0, 0, 0)));
        Highlighter.AddColorRegion("'", "'", Color.FromString("GREEN_YELLOW", Color.Color8(0, 0, 0)));
        Highlighter.AddColorRegion("#", "#", Color.FromString("DIM_GRAY", Color.Color8(0, 0, 0)));
        Editor.SyntaxHighlighter = Highlighter;
    }
    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed("Save")) {
            Save_data.Code = Editor.Text;
            Save_data.Stop_at = Stop_at;
            Godot.Error State = ResourceSaver.Save(Save_data, "res://Main//Save/save.tres");
            if (State == Godot.Error.Ok) {
                Displays[(int) Stop_at].AppendText("Save Successful");
                Displays[(int) Stop_at].Newline();
            }
            else {
                Displays[(int) Stop_at].AppendText("Save Failed");
                Displays[(int) Stop_at].Newline();
            }
        }
        if (@event.IsActionPressed("ShowTabs")) {
                Stage_tabs.TabsVisible = !Stage_tabs.TabsVisible;
        }
    }
    public async void Run() {
        Displays[0].Clear();
        Displays[1].Clear();
        try {
            List<Token> tokens = Tokenizer_object.Tokenize();
            Display_data(DISPLAY.TOKENIZER, string.Join(", ", tokens).Replace("(END_OF_LINE), ", "\n"));
            if (Stop_at == DISPLAY.TOKENIZER){
                Stage_tabs.CurrentTab = (int) DISPLAY.TOKENIZER;
                return;
            }
            List<Node> nodes = Parser_object.Parse(tokens);
            Display_data(DISPLAY.PARSER, string.Join("\n", nodes));
            if (Stop_at == DISPLAY.PARSER){
                Stage_tabs.CurrentTab = (int) DISPLAY.PARSER;
                return;
            }
            ContextualStorage storage = new ContextualStorage();
            await Interpreter_object.Interprete(nodes, storage);
            Console.WriteLine(storage);
            Display_data(DISPLAY.INTERPRETER, Interpreter_object.Output);
            Interpreter_object.Output = "";
            if (Stop_at == DISPLAY.INTERPRETER){
                Stage_tabs.CurrentTab = (int) DISPLAY.INTERPRETER;
                return;
            }
        } catch (Error error) {
            switch (error) {
                case TokenizerError:
                    Display_data(DISPLAY.TOKENIZER, error);
                    Stage_tabs.CurrentTab = (int) DISPLAY.TOKENIZER;
                    return;
                case ParserError:
                    Display_data(DISPLAY.PARSER, error);
                    Stage_tabs.CurrentTab = (int) DISPLAY.PARSER;
                    return;
                case InterpreterError:
                    Display_data(DISPLAY.INTERPRETER, error);
                    Stage_tabs.CurrentTab = (int) DISPLAY.INTERPRETER;
                    return;
                default:
                    Display_data((DISPLAY) Stage_tabs.CurrentTab, error);
                    break;
            }
        }
    }
    private void Display_data(DISPLAY display, object data) {
        Displays[(int) display].AppendText(data.ToString());
        Displays[(int) display].Newline();
    }
}
