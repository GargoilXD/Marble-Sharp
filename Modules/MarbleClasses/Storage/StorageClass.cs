using System.Collections.Generic;
public class StorageClass : StorageEntity {
    public ContextualStorage Storage {private set; get;}
    public StorageClass(ACCESS_MODE access_mode, bool is_static, ContextualStorage storage) : base(access_mode, is_static, DATATYPE.USER_DEFINED) {
        Storage = storage;
    }
    public StorageClass Duplicate() {
        return new StorageClass(AccessMode, IsStatic, Storage.Duplicate());
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Static: {IsStatic}, Return type: {Datatype}";
    }
}