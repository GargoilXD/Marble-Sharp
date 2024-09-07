public class MarbleCallable : MarbleData {
    public StorageFunction value;
    public MarbleCallable(StorageFunction value) {
        this.value = value;
    }
    public override MarbleCallable duplicate() {
        return new MarbleCallable(value);
    }
    public override void set_value(object value) {
        if (value is StorageFunction) this.value = value as StorageFunction;
    }
    public override object get_value() {
        return value;
    }
}