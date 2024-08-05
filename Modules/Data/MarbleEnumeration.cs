public class MarbleEnumeration : MarbleData {
    public bool Value;
    public MarbleEnumeration(bool value, bool initialized = true) : base(initialized) {
        Value = value;
    }
    public override MarbleEnumeration Duplicate() {
        return new MarbleEnumeration(Value);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean and(MarbleData operand) {
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean or(MarbleData operand) {
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean equals(MarbleData operand) {
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean not_equals(MarbleData operand) {
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData negate() {
        Value = !Value;
        return this;
    }
}
