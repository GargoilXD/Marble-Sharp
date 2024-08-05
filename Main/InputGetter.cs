using Godot;
public partial class InputGetter : AcceptDialog {
    public string Input = "";
    private LineEdit InputBox;
    public override void _Ready() {
        base._Ready();
        InputBox = GetNode<LineEdit>("%InputBox");
        RegisterTextEnter(InputBox);
        InputBox.TextChanged += delegate (string new_text) { Input = new_text; };
        VisibilityChanged += delegate {
            if (Visible) {
                InputBox.Text = "";
                Input = "";
                InputBox.GrabFocus();
            }
        };
    }
}
