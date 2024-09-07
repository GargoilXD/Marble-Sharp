using System.Reflection;

public abstract class MarbleData : InterpreterOutput {
    public enum TYPE {
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        LIST,
        DICTIONARY,
        CALLABLE,
        OBJECT
    }
    public virtual MarbleData duplicate() => throw new System.Exception();
    public abstract void set_value(object value);
    public abstract object get_value();
    public override string ToString() {
        return "[MarbleData]";
    }
}
