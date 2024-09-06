public class MarbleBoolean : MarbleType {
    public static MarbleData Convert(MarbleData operand, TokenPosition position) {
        switch (operand.type) {
            case MarbleData.TYPE.BOOLEAN:
                return new MarbleData(MarbleData.TYPE.BOOLEAN, operand.value);
            case MarbleData.TYPE.INTEGER:
                return new MarbleData(MarbleData.TYPE.INTEGER, (int) operand.value > 0);
            case MarbleData.TYPE.FLOAT:
                return new MarbleData(MarbleData.TYPE.FLOAT, (float) operand.value > 0);
            case MarbleData.TYPE.STRING: {
                if (bool.TryParse((string) operand.value, out bool value)) return new MarbleData(MarbleData.TYPE.STRING, value);
                break;
            }
            case MarbleData.TYPE.LIST: case MarbleData.TYPE.DICTIONARY:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
}
