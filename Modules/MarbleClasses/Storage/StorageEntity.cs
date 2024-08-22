public abstract class StorageEntity : InterpreterOutput {
    public enum ACCESS_MODE {
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
        USER_DEFINED
    }
    public ACCESS_MODE AccessMode;
    public bool IsStatic;
    public DATATYPE Datatype;
    public StorageEntity(ACCESS_MODE access_mode, bool is_static, DATATYPE datatype) {
        IsStatic = is_static;
        AccessMode = access_mode;
        Datatype = datatype;
    }
}