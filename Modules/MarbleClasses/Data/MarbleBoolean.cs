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
    public static MarbleBoolean Convert(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return Convert(data.ToStatic(position), position);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean convert(MarbleData operand, TokenPosition position) {
        return Convert(operand, position);
    }
    public override MarbleData add(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData subtract(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData multiply(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData divide(MarbleData operand, TokenPosition position) {
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
        switch (operand) {
            case MarbleVariant data:
                return bitwise_and(data.ToStatic(position), position);
            case MarbleBoolean:
                return new MarbleBoolean(Value & Convert(operand, position).Value);
            case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData bitwise_or(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return bitwise_or(data.ToStatic(position), position);
            case MarbleBoolean:
                return new MarbleBoolean(Value | Convert(operand, position).Value);
            case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return equals(data.ToStatic(position), position);
            case MarbleBoolean:
                return new MarbleBoolean(Value == Convert(operand, position).Value);
            case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean not_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return not_equals(data.ToStatic(position), position);
            case MarbleBoolean:
                return new MarbleBoolean(Value != Convert(operand, position).Value);
            case MarbleInteger: case MarbleFloat: case MarbleString: case MarbleObject: case MarbleList: case MarbleDictionary:
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
            case MarbleVariant data:
                return contains(data.ToStatic(position), position);
            case MarbleList: case MarbleDictionary: case MarbleString:
                return operand.contains(this, position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleObject:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData negate(TokenPosition position) {
        return new MarbleBoolean(!Value);
    }
}
