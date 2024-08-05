using System;

public class MarbleBoolean : MarbleData {
    public bool Value;
    public MarbleBoolean(bool value, bool initialized = true) : base(initialized) {
        Value = value;
    }
    public override MarbleBoolean Duplicate() {
        return new MarbleBoolean(Value);
    }
    public override string ToString() {
        return Value.ToString();
    }
    public override void set_data(object data) {
        Value = (bool) data;
    }
    public override object get_data() {
        return Value;
    }
    public static MarbleBoolean Convert(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return Convert(data.ToStatic());
            case MarbleBoolean data:
                return new MarbleBoolean(data.Value);
            case MarbleInteger data:
                return new MarbleBoolean(data.Value > 0);
            case MarbleFloat data:
                return new MarbleBoolean(data.Value > 0);
            case MarbleString data: {
                if (bool.TryParse(data.Value, out bool value)) return new MarbleBoolean(value);
                break;
            }
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean convert(MarbleData operand) {
        return Convert(operand);
    }
    public override MarbleData add(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData subtract(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData multiply(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData divide(MarbleData operand) {
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
        switch (operand) {
            case MarbleVariant data:
                return bitwise_and(data.ToStatic());
            case MarbleBoolean:
                return new MarbleBoolean(Value & Convert(operand).Value);
            case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData bitwise_or(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return bitwise_or(data.ToStatic());
            case MarbleBoolean:
                return new MarbleBoolean(Value | Convert(operand).Value);
            case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean equals(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return equals(data.ToStatic());
            case MarbleBoolean:
                return new MarbleBoolean(Value == Convert(operand).Value);
            case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean not_equals(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return not_equals(data.ToStatic());
            case MarbleBoolean:
                return new MarbleBoolean(Value != Convert(operand).Value);
            case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleObject: case MarbleList: case MarbleDictionary:
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
            case MarbleList: case MarbleDictionary: case MarbleString:
                return operand.contains(this);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleObject:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData negate() {
        return new MarbleBoolean(!Value);
    }
}
