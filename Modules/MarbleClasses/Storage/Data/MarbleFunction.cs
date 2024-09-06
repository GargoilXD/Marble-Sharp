public class MarbleCallable : MarbleType {
public static MarbleData Convert(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
}