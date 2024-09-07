public class MarbleString : MarbleData {
    public string value;
    public MarbleString(string value) {
        this.value = value;
    }
    public override MarbleString duplicate() {
        return new MarbleString(value);
    }
    public override void set_value(object value) {
        this.value = (string) value;
    }
    public override object get_value() {
        return value;
    }
    public override string ToString() {
        return value;
    }
}