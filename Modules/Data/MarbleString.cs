using System;
using System.Collections.Generic;

public class MarbleString : MarbleData {
    public string Value;
    public MarbleString(string value, bool initialized = true) : base(initialized) {
        Value = value;
    }
    public override MarbleString Duplicate() {
        return new MarbleString(Value);
    }
    public override string ToString() {
        return Value.ToString();
    }
    public override void set_data(object data) {
        Value = (string) data;
    }
    public override object get_data() {
        return Value;
    }
    public MarbleString Convert(MarbleData operand) {
        return new MarbleString(operand.get_data() as string);
    }
     public override MarbleString convert(MarbleData operand) {
        return Convert(operand);
    }
    public override MarbleData add(MarbleData operand) {
        return new MarbleString(Value + operand.get_data());
    }
    public override MarbleData subtract(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData multiply(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return multiply(data.ToStatic());
            case MarbleInteger: case MarbleBoolean: case MarbleFloat:
                string new_string = "";
                for (int x = 0; x < MarbleInteger.Convert(operand).Value; x++) new_string += Value;
                return new MarbleString(new_string);
            case MarbleString: case MarbleList: case MarbleObject: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData divide(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return divide(data.ToStatic());
            case MarbleInteger: case MarbleBoolean: case MarbleFloat: {
                List<MarbleData> sub_strings = new List<MarbleData>();
                string sub_string = "";
                int sub_lenght = Value.Length / Math.Abs(MarbleInteger.Convert(operand).Value);
                for (int x = 0; x < Value.Length; x++) {
                    if (x % sub_lenght == 0) {
                        sub_strings.Add(new MarbleString(sub_string));
                        sub_string = "";
                    }
                    sub_string += Value[x];
                }
                return new MarbleList(sub_strings);
            }
            case MarbleString: case MarbleList: case MarbleObject: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleInteger integer_divide(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData exponent(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData modolus(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    
    public override MarbleData bitwise_and(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData bitwise_or(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean equals(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return equals(data.ToStatic());
            case MarbleString:
                return new MarbleBoolean(Value == Convert(operand).Value);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean not_equals(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return not_equals(data.ToStatic());
            case MarbleString:
                return new MarbleBoolean(Value != Convert(operand).Value);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean greater_than(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean lesser_than(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean contains(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return contains(data.ToStatic());
            case MarbleString: case MarbleBoolean: case MarbleInteger: case MarbleFloat:
                return new MarbleBoolean(Value.Contains(Convert(operand).Value));
            case MarbleList: case MarbleDictionary: case MarbleObject:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData negate() {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
}