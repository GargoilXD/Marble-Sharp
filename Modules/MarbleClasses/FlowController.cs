public class FlowController : InterpreterOutput {
    public enum TYPE {
        DONE,
        RETURN,
        BREAK,
        CONTINUE,
        BREAKPOINT
    }
    public TYPE Type;
    public FlowController(TYPE type) {
        Type = type;
    }
    public override string ToString() {
        return Type.ToString();
    }
    public class Return : FlowController {
        public MarbleData Data;
        public Return(MarbleData data = null) : base(TYPE.RETURN) {
            Data = data;
        }

    }
}