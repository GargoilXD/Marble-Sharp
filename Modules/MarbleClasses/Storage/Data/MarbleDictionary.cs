using System.Collections.Generic;

public class MarbleDictionary : MarbleData {
    public Dictionary<string, MarbleData> value;
    public MarbleDictionary(Dictionary<string, MarbleData> value) {
        this.value = value;
    }
    public override MarbleDictionary duplicate() {
        Dictionary<string, MarbleData> copy = new Dictionary<string, MarbleData>();
        foreach (KeyValuePair<string, MarbleData> element in value) copy.Add(element.Key, element.Value.duplicate());
        return new MarbleDictionary(value);
    }
    public override void set_value(object value) {
        if (value is Dictionary<string, MarbleData>) this.value = value as Dictionary<string, MarbleData>;
    }
    public override object get_value() {
        return value;
    }
}