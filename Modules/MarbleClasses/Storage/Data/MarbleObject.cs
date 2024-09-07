public class MarbleObject : MarbleData {
    public StorageClass value;
    public MarbleObject(StorageClass value) {
        this.value = value;
    }
    public override MarbleObject duplicate() {
        return new MarbleObject(value.Duplicate());
    }
    public override void set_value(object value) {
        if (value is StorageClass) this.value = value as StorageClass;
    }
    public override object get_value() {
        return value;
    }
}