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
        return null;
    }
    public abstract void set_data(object data);
    public abstract object get_data();
    public abstract MarbleData Convert(MarbleData operand);
    public abstract MarbleData add(MarbleData operand);
    public abstract MarbleData subtract(MarbleData operand);
    public abstract MarbleData multiply(MarbleData operand);
    public abstract MarbleData divide(MarbleData operand);
    public abstract MarbleData integer_divide(MarbleData operand);
    public abstract MarbleData exponent(MarbleData operand);
    public abstract MarbleData modolus(MarbleData operand);
    public abstract MarbleBoolean and(MarbleData operand);
    public abstract MarbleInteger bitwise_and(MarbleData operand);
    public abstract MarbleBoolean or(MarbleData operand);
    public abstract MarbleInteger bitwise_or(MarbleData operand);
    public abstract MarbleBoolean equals(MarbleData operand);
    public abstract MarbleBoolean not_equals(MarbleData operand);
    public abstract MarbleBoolean greater_than(MarbleData operand);
    public abstract MarbleBoolean greater_than_or_equals(MarbleData operand);
    public abstract MarbleBoolean lesser_than(MarbleData operand);
    public abstract MarbleBoolean lesser_than_or_equals(MarbleData operand);
    public abstract MarbleBoolean contains(MarbleData operand);
    public abstract MarbleBoolean is_is(MarbleData operand);
    public abstract MarbleData negate();
}
