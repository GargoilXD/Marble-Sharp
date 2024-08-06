public class ContextOutput : ContextControl {
    public InterpreterOutput Output;
    public ContextOutput(TYPE type, InterpreterOutput output) : base(type) {
        Output = output;
    }
    public override string ToString() {
        return $"{Type} {Output}";
    }
}