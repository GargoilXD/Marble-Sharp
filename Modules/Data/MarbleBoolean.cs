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
    public override MarbleData Convert(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                return operand;
            case MarbleInteger data:
                return new MarbleBoolean(data.Value > 0);
            case MarbleFloat data:
                return new MarbleBoolean(data.Value > 0);
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData add(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData subtract(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData multiply(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData divide(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData integer_divide(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData exponent(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData modolus(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean and(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean data:
                return new MarbleBoolean(Value && data.Value);
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleInteger bitwise_and(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean or(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean data:
                return new MarbleBoolean(Value || data.Value);
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleInteger bitwise_or(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean equals(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean data:
                return new MarbleBoolean(Value == data.Value);
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean not_equals(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean data:
                return new MarbleBoolean(Value != data.Value);
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean greater_than(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean lesser_than(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean contains(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean is_is(MarbleData operand) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData negate() {
        return new MarbleBoolean(!Value);
    }
}
