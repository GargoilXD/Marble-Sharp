public class MarbleInteger : MarbleType {
    public static MarbleData Convert(MarbleData operand, TokenPosition position) {
        switch (operand.type) {
            case MarbleData.TYPE.BOOLEAN:
                return new MarbleData(MarbleData.TYPE.BOOLEAN, (bool) operand.value? 1 : 0);
            case MarbleData.TYPE.INTEGER:
                return new MarbleData(MarbleData.TYPE.INTEGER, (int) operand.value);
            case MarbleData.TYPE.FLOAT:
                return new MarbleData(MarbleData.TYPE.FLOAT, (int) operand.value);
            case MarbleData.TYPE.STRING: {
                if (int.TryParse((string) operand.value, out int value)) return new MarbleData(MarbleData.TYPE.STRING, value);
                break;
            }
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
}