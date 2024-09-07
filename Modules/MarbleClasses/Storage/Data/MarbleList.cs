using System.Collections.Generic;
public class MarbleList : MarbleData {
    public List<MarbleData> value;
    public MarbleList(List<MarbleData> value) {
        this.value = value;
    }
    public override MarbleList duplicate() {
        List<MarbleData> copy = new List<MarbleData>();
        foreach (MarbleData element in value) copy.Add(element.duplicate());
        return new MarbleList(value);
    }
    public override void set_value(object value) {
        if (value is List<MarbleData>) this.value = value as List<MarbleData>;
    }
    public override object get_value() {
        return value;
    }
}