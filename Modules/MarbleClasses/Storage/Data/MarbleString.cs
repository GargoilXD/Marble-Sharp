public class MarbleString : MarbleType {
    public static MarbleData Convert(MarbleData operand) {
        return new MarbleData(MarbleData.TYPE.STRING, operand.value.ToString());
    }
}