public class MarbleFloat : MarbleData {
    public float value;
    public MarbleFloat(float value) {
        this.value = value;
    }
    public override MarbleFloat duplicate() {
        return new MarbleFloat(value);
    }
    public override void set_value(object value) {
        if (value is float) this.value = (float) value;
    }
    public override object get_value() {
        return value;
    }
}