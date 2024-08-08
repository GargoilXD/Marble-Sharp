public class InterpreterOutput {
    /*
    public ContextControl IsContextControl(TokenPosition position) {
        if (this is not ContextControl) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "You did the thing");
        return this as ContextControl;
    }
    */
    public MarbleData IsMarbleData(TokenPosition position) {
        if (this is not MarbleData) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "You did the thing");
        return this as MarbleData;
    }
}