using Godot;
public partial class EditorSave : Resource {
    [Export] public MarbleIDE.DISPLAY Stop_at;
    [Export(PropertyHint.MultilineText)] public string Code;
}
