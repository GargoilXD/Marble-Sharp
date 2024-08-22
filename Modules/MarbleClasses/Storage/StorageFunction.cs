using System.Collections.Generic;
public class StorageFunction : StorageEntity {
    public List<Node> Arguments {private set; get;}
    public InstructionListNode Instructions {private set; get;}
    public StorageFunction(ACCESS_MODE access_mode, bool is_static, DATATYPE datatype, List<Node> arguments, InstructionListNode instructions) : base(access_mode, is_static, datatype) {
        Arguments = arguments;
        Instructions = instructions;
    }
    public StorageFunction Duplicate() {
        return new StorageFunction(AccessMode, IsStatic, Datatype, Arguments, Instructions);
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Static: {IsStatic}, Return type: {Datatype}, Arguments: {Arguments}";
    }
}