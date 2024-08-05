
using System.Collections.Generic;

public class MarbleVariant : MarbleData {
    public object Value;
    public MarbleVariant(object value, bool initialized = true) : base(initialized) {
        Value = value;
    }
    public override MarbleVariant Duplicate() {
        return new MarbleVariant(Value);
    }
    public override string ToString() {
        return Value.ToString();
    }
    public override void set_data(object data) {
        Value = data;
    }
    public override object get_data() {
        return Value;
    }
    public MarbleData ToStatic() {
        switch (Value) {
            case bool data:
                return new MarbleBoolean(data);
            case int data:
                return new MarbleInteger(data);
            case float data:
                return new MarbleFloat(data);
            case string data:
                return new MarbleString(data);
            case List<MarbleData> data:
                return new MarbleList(data);
            case Dictionary<object, MarbleData> data:
                return new MarbleDictionary(data);
        }
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, null, "Variant covertion failed");
    }
    public MarbleVariant Convert(MarbleData operand) {
        return new MarbleVariant(operand.get_data());
    }
    public override MarbleVariant convert(MarbleData operand) {
        return Convert(operand);
    }
    public override MarbleData add(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData subtract(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData multiply(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData divide(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleInteger integer_divide(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData exponent(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData modolus(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData bitwise_and(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData bitwise_or(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean equals(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean not_equals(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean greater_than(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean lesser_than(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleBoolean contains(MarbleData operand) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, null);
    }
    public override MarbleData negate() {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
}
