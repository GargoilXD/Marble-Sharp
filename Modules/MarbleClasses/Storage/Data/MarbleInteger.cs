public class MarbleInteger : MarbleData {
    public int value;
    public MarbleInteger(int value) {
        this.value = value;
    }
    public override MarbleInteger duplicate() {
        return new MarbleInteger(value);
    }
    public override void set_value(object value) {
        if (value is int) this.value = (int) value;
    }
    public override object get_value() {
        return value;
    }
}