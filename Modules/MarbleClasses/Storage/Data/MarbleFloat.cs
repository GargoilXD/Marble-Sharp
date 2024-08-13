using System;
using System.Collections.Generic;
public class MarbleFloat : MarbleData {
    public float Value;
    public MarbleFloat(float value) {
        Value = value;
    }
    public override MarbleFloat Duplicate() {
        return new MarbleFloat(Value);
    }
    public override string ToString() {
        return Value.ToString();
    }
    public override void set_data(object data) {
        Value = (float) data;
    }
    public override object get_data() {
        return Value;
    }
    public static MarbleFloat Convert(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return Convert(data.ToStatic(position), position);
            case MarbleBoolean data:
                return new MarbleFloat(data.Value? 1 : 0);
            case MarbleInteger data:
                return new MarbleFloat(data.Value);
            case MarbleFloat data:
                return new MarbleFloat(data.Value);
            case MarbleString data: {
                if (float.TryParse(data.Value, out float value)) return new MarbleFloat(value);
                break;
            }
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleFloat convert(MarbleData operand, TokenPosition position) {
        return Convert(operand, position);
    }
    public override MarbleData add(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return add(data.ToStatic(position), position);
            case MarbleInteger: case MarbleBoolean: case MarbleFloat: 
                return new MarbleFloat(Value + Convert(operand, position).Value);
            case MarbleString data:
                return new MarbleString(Value.ToString() + data.Value);
            case MarbleList data:
                MarbleList new_list = new MarbleList(new List<MarbleData>());
                new_list.Elements.Add(this.Duplicate());
                new_list.Elements.AddRange(data.Duplicate().Elements);
                return new_list;
            case MarbleDictionary: 
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData subtract(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return subtract(data.ToStatic(position), position);
            case MarbleInteger: case MarbleBoolean: case MarbleFloat: case MarbleString:
                return new MarbleFloat(Value - Convert(operand, position).Value);
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData multiply(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return multiply(data.ToStatic(position), position);
            case MarbleInteger: case MarbleBoolean: case MarbleFloat:
                return new MarbleFloat(Value * Convert(operand, position).Value);
            case MarbleString: case MarbleList:
                return operand.multiply(this, position);
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData divide(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return divide(data.ToStatic(position), position);
            case MarbleInteger: case MarbleBoolean: case MarbleFloat:
                float dividend = Convert(operand, position).Value;
                if (dividend == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                return new MarbleFloat(Value / dividend);
            case MarbleString: case MarbleList:
                return operand.divide(this, position);
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleInteger integer_divide(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return integer_divide(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                int dividend = MarbleInteger.Convert(operand, position).Value;
                if (dividend == 0) throw new InterpreterError(InterpreterError.TYPE.DIVISION_BY_ZERO, position);
                break;
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData exponent(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return exponent(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleFloat((float) Math.Pow(Value, Convert(operand, position).Value));
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData modolus(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return modolus(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleFloat(Value % Convert(operand, position).Value);
            case MarbleList: case MarbleDictionary:
                break;
        }
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
            case MarbleVariant data:
                return equals(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value == Convert(operand, position).Value);
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean not_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return not_equals(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value != Convert(operand, position).Value);
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean greater_than(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return greater_than(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value > Convert(operand, position).Value);
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return greater_than_or_equals(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value >= Convert(operand, position).Value);
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean lesser_than(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return lesser_than(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value < Convert(operand, position).Value);
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return lesser_than_or_equals(data.ToStatic(position), position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleString:
                return new MarbleBoolean(Value <= Convert(operand, position).Value);
            case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean contains(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return contains(data.ToStatic(position), position);
            case MarbleList: case MarbleDictionary: case MarbleString:
                return operand.contains(this, position);
            case MarbleBoolean: case MarbleInteger: case MarbleFloat:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData negate(TokenPosition position) {
        return new MarbleFloat(-Value);
    }
}
