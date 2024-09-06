using System.Collections.Generic;
public class MarbleData : InterpreterOutput {
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
    public TYPE type;
    public object value;
    public MarbleData(TYPE type, object value) {
        this.type = type;
        this.value = value;
    }
    public MarbleData duplicate() {
        switch (value) {
            case List<MarbleData> list:
                List<MarbleData> list_copy = new List<MarbleData>();
                foreach (MarbleData element in list) list_copy.Add(element.duplicate());
                return new MarbleData(type, list_copy);
            case Dictionary<object, MarbleData> dictionary:
                Dictionary<object, MarbleData> dictionary_copy = new Dictionary<object, MarbleData>();
                foreach (KeyValuePair<object, MarbleData> element in dictionary) dictionary_copy.Add(element.Key, element.Value.duplicate());
                return new MarbleData(type, dictionary_copy);
            default:
                return new MarbleData(type, value);
        }
    }
    public MarbleData convert_to(TYPE type, TokenPosition position) {
        return type switch {
            TYPE.BOOLEAN => MarbleBoolean.Convert(this, position),
            TYPE.INTEGER => MarbleInteger.Convert(this, position),
            TYPE.FLOAT => MarbleFloat.Convert(this, position),
            TYPE.STRING => MarbleString.Convert(this),
            TYPE.LIST => MarbleList.Convert(this, position),
            TYPE.DICTIONARY => MarbleDictionary.Convert(this, position),
            TYPE.CALLABLE => MarbleCallable.Convert(this, position),
            TYPE.OBJECT => MarbleObject.Convert(this, position),
            _ => null
        };
    }
    public bool as_boolean(TokenPosition position = null) {
        if (type != TYPE.BOOLEAN) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
        return (bool) value;
    }
    public int as_integer(TokenPosition position = null) {
        if (type != TYPE.INTEGER) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
        return (int) value;
    }
    public float as_float(TokenPosition position = null) {
        if (type != TYPE.FLOAT) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
        return (float) value;
    }
    public string as_string(TokenPosition position = null) {
        if (type != TYPE.STRING) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
        return (string) value;
    }
    public List<MarbleData> as_list(TokenPosition position = null) {
        if (type != TYPE.LIST) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
        return value as List<MarbleData>;
    }
    public Dictionary<object, MarbleData> as_dictionary(TokenPosition position = null) {
        if (type != TYPE.DICTIONARY) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
        return value as Dictionary<object, MarbleData>;
    }
    public StorageFunction as_callable(TokenPosition position = null) {
        if (type != TYPE.CALLABLE) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
        return value as StorageFunction;
    }
    public StorageClass as_object(TokenPosition position = null) {
        if (type != TYPE.OBJECT) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
        return value as StorageClass;
    }
}