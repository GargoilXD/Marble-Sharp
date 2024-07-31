using Godot;
using System;

public partial class SaveData : Resource {
    [Export] public MarbleIDE.DISPLAY Stop_at;
    [Export(PropertyHint.MultilineText)] public string Code;
}
