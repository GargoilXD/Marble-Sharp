public abstract class MarbleData : InterpreterOutput {
    public abstract MarbleData Duplicate();
    public static MarbleData FromDataNode(DataNode data_node) {
        switch (data_node.Type) {
            case DataNode.TYPE.BOOLEAN:
                return new MarbleBoolean((bool) data_node.Data);
            case DataNode.TYPE.INTEGER:
                return new MarbleInteger((int) data_node.Data);
            case DataNode.TYPE.FLOAT:
                return new MarbleFloat((float) data_node.Data);
            case DataNode.TYPE.STRING:
                return new MarbleString((string) data_node.Data);
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, data_node.Position);
    }
    public abstract void set_data(object data);
    public abstract object get_data();
    public abstract MarbleData convert(MarbleData operand, TokenPosition position);
    public abstract MarbleData add(MarbleData operand, TokenPosition position);
    public abstract MarbleData subtract(MarbleData operand, TokenPosition position);
    public abstract MarbleData multiply(MarbleData operand, TokenPosition position);
    public abstract MarbleData divide(MarbleData operand, TokenPosition position);
    public abstract MarbleInteger integer_divide(MarbleData operand, TokenPosition position);
    public abstract MarbleData exponent(MarbleData operand, TokenPosition position);
    public abstract MarbleData modolus(MarbleData operand, TokenPosition position);
    public abstract MarbleData bitwise_and(MarbleData operand, TokenPosition position);
    public abstract MarbleData bitwise_or(MarbleData operand, TokenPosition position);
    public abstract MarbleBoolean equals(MarbleData operand, TokenPosition position);
    public abstract MarbleBoolean not_equals(MarbleData operand, TokenPosition position);
    public abstract MarbleBoolean greater_than(MarbleData operand, TokenPosition position);
    public abstract MarbleBoolean greater_than_or_equals(MarbleData operand, TokenPosition position);
    public abstract MarbleBoolean lesser_than(MarbleData operand, TokenPosition position);
    public abstract MarbleBoolean lesser_than_or_equals(MarbleData operand, TokenPosition position);
    public abstract MarbleBoolean contains(MarbleData operand, TokenPosition position);
    public abstract MarbleData negate(TokenPosition position);
    public MarbleBoolean is_is(MarbleData operand) {
        return new MarbleBoolean(GetType() == operand.GetType());
    }
    public MarbleBoolean and(MarbleData operand, TokenPosition position) {
        return new MarbleBoolean(MarbleBoolean.Convert(this, position).Value && MarbleBoolean.Convert(operand, position).Value);
    }
    public MarbleBoolean or(MarbleData operand, TokenPosition position) {
        return new MarbleBoolean(MarbleBoolean.Convert(this, position).Value || MarbleBoolean.Convert(operand, position).Value);
    }
}
