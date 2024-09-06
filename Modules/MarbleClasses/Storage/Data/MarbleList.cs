using System.Collections.Generic;
public class MarbleList : MarbleType {
    public static MarbleData Convert(MarbleData operand, TokenPosition position) {
        switch (operand.type) {
            case MarbleData.TYPE.STRING: {
                List<MarbleData> new_list = new List<MarbleData>();
                foreach (char item in operand.value as string) {
                    new_list.Add(new MarbleData(MarbleData.TYPE.STRING, item + ""));
                }
                return new MarbleData(MarbleData.TYPE.LIST, new_list);
            }
            case MarbleData.TYPE.LIST:
                return operand.duplicate();
            case MarbleData.TYPE.BOOLEAN: case MarbleData.TYPE.INTEGER: case MarbleData.TYPE.FLOAT:
                return new MarbleData(MarbleData.TYPE.LIST, new List<MarbleData>(){ operand.duplicate() });
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
}