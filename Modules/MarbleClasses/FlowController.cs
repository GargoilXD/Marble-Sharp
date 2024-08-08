public class FlowController : InterpreterOutput {
    public enum TYPE {
        DONE,
        RETURN,
        BREAK,
        CONTINUE,
        BREAKPOINT
    }
    public TYPE Type;
    public MarbleData Data;
    public FlowController(TYPE type, MarbleData data = null) {
        Type = type;
        Data = data;
    }
    public override string ToString() {
        return Type.ToString();
    }
}