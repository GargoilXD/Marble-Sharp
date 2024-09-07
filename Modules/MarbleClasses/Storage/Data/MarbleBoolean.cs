public class MarbleBoolean : MarbleData {
    public bool value;
    public MarbleBoolean(bool value) {
        this.value = value;
    }
    public override MarbleBoolean duplicate() {
        return new MarbleBoolean(value);
    }
    public override void set_value(object value) {
        if (value is bool) this.value = (bool) value;
    }
    public override object get_value() {
        return value;
    }
}
