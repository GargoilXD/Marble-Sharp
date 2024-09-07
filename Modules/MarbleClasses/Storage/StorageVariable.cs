public class StorageVariable : StorageEntity {
    public bool IsConstant;
    public MarbleData Data;
    public StorageVariable(ACCESSMODE access_mode, DATATYPE datatype, bool is_static, bool is_constant, MarbleData data) : base(access_mode, datatype, is_static) {
        IsConstant = is_constant;
        Data = data;
    }
    public override StorageVariable Duplicate() {
        return new StorageVariable(AccessMode, Datatype, IsStatic, IsConstant, Data == null? null : Data.duplicate()) { Class_name = Class_name };
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Constant: {IsConstant}, Static: {IsStatic}, Datatype: {Datatype}, Data: {Data}";
    }
}