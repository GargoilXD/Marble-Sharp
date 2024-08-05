public abstract class MarbleData {
    public bool Initialized = false;
    public MarbleData(bool initialized = true) {
        Initialized = initialized;
    }
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
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES);
    }
    public abstract void set_data(object data);
    public abstract object get_data();
    public abstract MarbleData convert(MarbleData operand);
    public abstract MarbleData add(MarbleData operand);
    public abstract MarbleData subtract(MarbleData operand);
    public abstract MarbleData multiply(MarbleData operand);
    public abstract MarbleData divide(MarbleData operand);
    public abstract MarbleInteger integer_divide(MarbleData operand);
    public abstract MarbleData exponent(MarbleData operand);
    public abstract MarbleData modolus(MarbleData operand);
    public MarbleBoolean and(MarbleData operand) {
        return new MarbleBoolean(MarbleBoolean.Convert(this).Value && MarbleBoolean.Convert(operand).Value);
    }
    public MarbleBoolean or(MarbleData operand) {
        return new MarbleBoolean(MarbleBoolean.Convert(this).Value || MarbleBoolean.Convert(operand).Value);
    }
    public abstract MarbleData bitwise_and(MarbleData operand);
    public abstract MarbleData bitwise_or(MarbleData operand);
    public abstract MarbleBoolean equals(MarbleData operand);
    public abstract MarbleBoolean not_equals(MarbleData operand);
    public abstract MarbleBoolean greater_than(MarbleData operand);
    public abstract MarbleBoolean greater_than_or_equals(MarbleData operand);
    public abstract MarbleBoolean lesser_than(MarbleData operand);
    public abstract MarbleBoolean lesser_than_or_equals(MarbleData operand);
    public abstract MarbleBoolean contains(MarbleData operand);
    public MarbleBoolean is_is(MarbleData operand) {
        return new MarbleBoolean(this.GetType() == operand.GetType());
    }
    public abstract MarbleData negate();
}
