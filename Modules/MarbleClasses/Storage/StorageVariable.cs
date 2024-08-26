public class StorageVariable : StorageEntity {
    public bool IsConstant;
    public MarbleData Data;
    public StorageVariable(ACCESS_MODE access_mode, bool is_static, DATATYPE datatype, bool is_constant, MarbleData data) : base(access_mode, is_static, datatype) {
        IsConstant = is_constant;
        Data = data;
    }
    public override object GetData() {
        return Data;
    } 
    public override StorageVariable Duplicate() {
        return new StorageVariable(AccessMode, IsStatic, Datatype, IsConstant, Data == null? null : Data.Duplicate());
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Constant: {IsConstant}, Static: {IsStatic}, Datatype: {Datatype}, Data: {Data}";
    }
}