public class MarbleObject : MarbleType {
    public static MarbleData Convert(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
}