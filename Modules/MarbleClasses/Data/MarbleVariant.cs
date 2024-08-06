
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
    public MarbleData ToStatic(TokenPosition position) {
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
        throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Variant covertion failed");
    }
    public MarbleVariant Convert(MarbleData operand) {
        return new MarbleVariant(operand.get_data());
    }
    public override MarbleVariant convert(MarbleData operand, TokenPosition position) {
        return Convert(operand);
    }
    public override MarbleData add(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleData subtract(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleData multiply(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleData divide(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleInteger integer_divide(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleData exponent(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleData modolus(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleData bitwise_and(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleData bitwise_or(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleBoolean equals(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleBoolean not_equals(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleBoolean greater_than(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleBoolean lesser_than(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleBoolean contains(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
    public override MarbleData negate(TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
}
