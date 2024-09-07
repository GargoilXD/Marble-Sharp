public class StorageClass : StorageEntity {
    public ContextualStorage Storage {private set; get;}
    public StorageFunction.Constructor constructor;
    public StorageClass(ACCESSMODE access_mode, bool is_static, string class_name, ContextualStorage storage) : base(access_mode, DATATYPE.USER_DEFINED, is_static) {
        Class_name = class_name;
        Storage = storage;
    }
    public override StorageClass Duplicate() {
        return new StorageClass(AccessMode, IsStatic, Class_name, Storage.Duplicate()) { Class_name = Class_name };
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Static: {IsStatic}, Datatype: {Datatype}";
    }
}