using System;
using System.Collections.Generic;

public class MarbleInteger : MarbleData {
    public int Value;
    public MarbleInteger(int value, bool initialized = true) : base(initialized) {
        Value = value;
    }
    public override MarbleInteger Duplicate() {
        return new MarbleInteger(Value);
    }
    public override string ToString() {
        return Value.ToString();
    }
    public override void set_data(object data) {
        Value = (int) data;
    }
    public override object get_data() {
        return Value;
    }
    public static MarbleInteger Convert(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return Convert(data.ToStatic());
            case MarbleBoolean data:
                return new MarbleInteger(data.Value? 1 : 0);
            case MarbleInteger data:
                return new MarbleInteger(data.Value);
            case MarbleFloat data:
                return new MarbleInteger((int) data.Value);
            case MarbleString data: {
                if (int.TryParse(data.Value, out int value)) return new MarbleInteger(value);
                break;
            }
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleInteger convert(MarbleData operand) {
        return Convert(operand);
    }
    public override MarbleData add(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return add(data.ToStatic());
            case MarbleInteger: case MarbleBoolean: case MarbleFloat: 
                return new MarbleInteger(Value + Convert(operand).Value);
            case MarbleString data:
                return new MarbleString(Value.ToString() + data.Value);
            case MarbleList data:
                MarbleList new_list = new MarbleList(new List<MarbleData>());
                new_list.Elements.Add(this.Duplicate());
                new_list.Elements.AddRange(data.Duplicate().Elements);
                return new_list;
            case MarbleDictionary: case MarbleObject: 
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData subtract(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return subtract(data.ToStatic());
            case MarbleInteger: case MarbleBoolean: case MarbleFloat: case MarbleString:
                return new MarbleInteger(Value - Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData multiply(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return multiply(data.ToStatic());
            case MarbleInteger: case MarbleBoolean: case MarbleFloat:
                return new MarbleInteger(Value * Convert(operand).Value);
            case MarbleString: case MarbleList:
                return operand.multiply(this);
            case MarbleObject: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData divide(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return divide(data.ToStatic());
            case MarbleInteger: case MarbleBoolean: case MarbleFloat:
                int dividend = Convert(operand).Value;
                if (dividend == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO);
                return new MarbleInteger(Value / dividend);
            case MarbleString: case MarbleList:
                return operand.divide(this);
            case MarbleObject: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleInteger integer_divide(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return integer_divide(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                int dividend = Convert(operand).Value;
                if (dividend == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO);
                break;
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData exponent(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return exponent(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleInteger((int) Math.Pow(Value, Convert(operand).Value));
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData modolus(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return modolus(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleInteger(Value % Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData bitwise_and(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return bitwise_and(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleInteger(Value & Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleData bitwise_or(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return bitwise_or(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleInteger(Value | Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean equals(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return equals(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value == Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean not_equals(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return not_equals(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value != Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean greater_than(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return greater_than(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value > Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return greater_than_or_equals(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value >= Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean lesser_than(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return lesser_than(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value < Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand) {
        switch (operand) {
            case MarbleVariant data:
                return lesser_than_or_equals(data.ToStatic());
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value <= Convert(operand).Value);
            case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
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
        Value = -Value;
        return this;
    }
}
