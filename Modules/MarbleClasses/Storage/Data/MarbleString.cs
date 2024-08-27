using System;
using System.Collections.Generic;

public class MarbleString : MarbleData {
    public string Value;
    public MarbleString(string value) {
        Value = value;
    }
    public override MarbleString Duplicate() {
        return new MarbleString(Value);
    }
    public override string ToString() {
        return Value;
    }
    public override void set_data(object data) {
        Value = (string) data;
    }
    public override object get_data() {
        return Value;
    }
    public static MarbleString Convert(MarbleData operand) {
        return new MarbleString(operand.get_data() as string);
    }
     public override MarbleString convert(MarbleData operand, TokenPosition position) {
        return Convert(operand);
    }
    public override MarbleData add(MarbleData operand, TokenPosition position) {
        return new MarbleString(Value + operand.get_data());
    }
    public override MarbleData subtract(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData multiply(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleInteger: case MarbleBoolean: case MarbleFloat:
                string new_string = "";
                for (int x = 0; x < MarbleInteger.Convert(operand, position).Value; x++) new_string += Value;
                return new MarbleString(new_string);
            case MarbleString: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData divide(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleInteger: case MarbleBoolean: case MarbleFloat: {
                List<MarbleData> sub_strings = new List<MarbleData>();
                string sub_string = "";
                int sub_lenght = Value.Length / Math.Abs(MarbleInteger.Convert(operand, position).Value);
                for (int x = 0; x < Value.Length; x++) {
                    if (x % sub_lenght == 0) {
                        sub_strings.Add(new MarbleString(sub_string));
                        sub_string = "";
                    }
                    sub_string += Value[x];
                }
                return new MarbleList(sub_strings);
            }
            case MarbleString: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleInteger integer_divide(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData exponent(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData modolus(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    
    public override MarbleData bitwise_and(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData bitwise_or(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleString:
                return new MarbleBoolean(Value == Convert(operand).Value);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleList: case MarbleDictionary:
                break;
        }
        return new MarbleBoolean(false);
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean not_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleString:
                return new MarbleBoolean(Value != Convert(operand).Value);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean greater_than(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean lesser_than(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean contains(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleString: case MarbleBoolean: case MarbleInteger: case MarbleFloat:
                return new MarbleBoolean(Value.Contains(Convert(operand).Value));
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData negate(TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
}