public class MarbleFloat : MarbleType {
    public static MarbleData Convert(MarbleData operand, TokenPosition position) {
        switch (operand.type) {
            case MarbleData.TYPE.BOOLEAN:
                return new MarbleData(MarbleData.TYPE.FLOAT, (bool) operand.value? 1 : 0);
            case MarbleData.TYPE.INTEGER:
                return new MarbleData(MarbleData.TYPE.FLOAT, (int) operand.value);
            case MarbleData.TYPE.FLOAT:
                return operand.duplicate();
            case MarbleData.TYPE.STRING: {
                if (float.TryParse((string) operand.value, out float value)) return new MarbleData(MarbleData.TYPE.FLOAT, value);
                break;
            }
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
}