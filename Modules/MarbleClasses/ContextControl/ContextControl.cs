public class ContextControl : InterpreterOutput {
    public enum TYPE {
        DONE,
        RETURN,
        BREAK,
        CONTINUE
    }
    public TYPE Type;
    public ContextControl(TYPE type) {
        Type = type;
    }
    public override string ToString() {
        return Type.ToString();
    }
}