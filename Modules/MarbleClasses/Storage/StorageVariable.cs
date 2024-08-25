using System.Collections.Generic;
public class StorageVariable : StorageEntity {
    public bool IsConstant;
    public MarbleData Data;
    public StorageVariable(ACCESS_MODE access_mode, bool is_static, DATATYPE datatype, bool is_constant, MarbleData data) : base(access_mode, is_static, datatype) {
        IsConstant = is_constant;
        Data = data;
    }
    public StorageVariable Duplicate() {
        return new StorageVariable(AccessMode, IsStatic, Datatype, IsConstant, Data == null? null : Data.Duplicate());
    }
    public void Initialize() {
        switch (Datatype) {
            case DATATYPE.VARIANT:
                Data = new MarbleBoolean(false);
                break;
            case DATATYPE.BOOLEAN:
                Data = new MarbleBoolean(false);
                break;
            case DATATYPE.INTEGER:
                Data = new MarbleInteger(0);
                break;
            case DATATYPE.FLOAT:
                Data = new MarbleFloat(0);
                break;
            case DATATYPE.STRING:
                Data = new MarbleString("");
                break;
            case DATATYPE.LIST:
                Data = new MarbleList(new List<MarbleData>());
                break;
            case DATATYPE.DICTIONARY:
                Data = new MarbleDictionary(new Dictionary<object, MarbleData>());
                break;
            default:
                break;
        }
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Constant: {IsConstant}, Static: {IsStatic}, Datatype: {Datatype}, Data: {Data}";
    }
}