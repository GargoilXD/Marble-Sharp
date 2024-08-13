public struct StorageVariable {
    public enum ACCESS_MODE {
        PRIVATE,
        PUBLIC
    }
    public enum DATATYPE {
        VARIANT,
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        LIST,
        DICTIONARY,
        USER_DEFINED
    }
    public ACCESS_MODE AccessMode;
    public bool IsConstant;
    public bool IsStatic;
    public DATATYPE Datatype;
    public MarbleData Data;
    public StorageVariable(ACCESS_MODE access_mode, bool is_constant, bool is_static, DATATYPE datatype, MarbleData data) {
        IsConstant = is_constant;
        IsStatic = is_static;
        AccessMode = access_mode;
        Datatype = datatype;
        Data = data;
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Constant: {IsConstant}, Static: {IsStatic}, Datatype: {Datatype}, Data: {Data}";
    }
}