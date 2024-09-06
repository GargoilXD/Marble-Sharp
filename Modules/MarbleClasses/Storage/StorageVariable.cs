public class StorageVariable : StorageEntity {
    public bool IsConstant;
    public MarbleData Data;
    public StorageVariable(ACCESSMODE access_mode, DATATYPE datatype, bool is_static, bool is_constant, MarbleData data) : base(access_mode, datatype, is_static) {
        IsConstant = is_constant;
        Data = data;
    }
    public void assign(MarbleData data, TokenPosition position) {
        switch (Datatype) {
            case DATATYPE.VARIANT:
                Data = data.duplicate();
                break;
            case DATATYPE.BOOLEAN:
                Data = MarbleBoolean.Convert(data, position);
                break;
            case DATATYPE.INTEGER:
                Data = MarbleInteger.Convert(data, position);
                break;
            case DATATYPE.FLOAT:
                Data = MarbleFloat.Convert(data, position);
                break;
            case DATATYPE.STRING:
                Data = MarbleString.Convert(data);
                break;
            case DATATYPE.LIST:
                Data = MarbleList.Convert(data, position);
                break;
            case DATATYPE.DICTIONARY:
                Data = MarbleDictionary.Convert(data, position);
                break;
            case DATATYPE.CALLABLE:
                if (data.type != MarbleData.TYPE.CALLABLE) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
                Data = data.duplicate();
                break;
            case DATATYPE.USER_DEFINED:
                if (data.type != MarbleData.TYPE.OBJECT) throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
                if (data.as_object().Class_name != Class_name)  throw new InterpreterError(InterpreterError.TYPE.DATATYPE_MISMATCH, position);
                Data = data.duplicate();
                break;
        }
    }
    public override StorageVariable Duplicate() {
        return new StorageVariable(AccessMode, Datatype, IsStatic, IsConstant, Data == null? null : Data.duplicate());
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Constant: {IsConstant}, Static: {IsStatic}, Datatype: {Datatype}, Data: {Data}";
    }
}