public abstract class StorageEntity : InterpreterOutput {
    public enum ACCESSMODE {
        NONE,
        PRIVATE,
        PUBLIC
    }
    public enum DATATYPE {
        VOID,
        VARIANT,
        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        LIST,
        DICTIONARY,
        CALLABLE,
        USER_DEFINED
    }
    public ACCESSMODE AccessMode;
    public DATATYPE Datatype;
    public bool IsStatic;
    public string Class_name;
    public StorageEntity(ACCESSMODE access_mode, DATATYPE datatype, bool is_static) {
        IsStatic = is_static;
        AccessMode = access_mode;
        Datatype = datatype;
    }
    public abstract StorageEntity Duplicate();
    public override string ToString() {
        return $"Access mode: {AccessMode}, Static: {IsStatic}, Datatype: {Datatype}";
    }
}