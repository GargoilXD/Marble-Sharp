using System.Collections.Generic;
public class StorageFunction : StorageEntity {
    public bool UnlimitedArguments;
    public List<Node> Arguments {private set; get;}
    public OperandInstructionNode Instructions {private set; get;}
    public StorageFunction(ACCESSMODE access_mode, DATATYPE datatype, bool is_static, bool unlimited_arguments, List<Node> arguments, OperandInstructionNode instructions) : base(access_mode, datatype, is_static) {
        UnlimitedArguments = unlimited_arguments;
        Arguments = arguments;
        Instructions = instructions;
    }
    public override StorageFunction Duplicate() {
        return new StorageFunction(AccessMode, Datatype, IsStatic, UnlimitedArguments, Arguments, Instructions);
    }
    public override string ToString() {
        return $"Access mode: {AccessMode}, Static: {IsStatic}, Return type: {Datatype}, Arguments: {Arguments}";
    }
    public class Constructor : StorageFunction {
        public List<Node> BaseParameters;
        public Constructor(ACCESSMODE access_mode, bool unlimited_arguments, List<Node> arguments, List<Node> base_parameters, OperandInstructionNode instructions) : base(access_mode, DATATYPE.USER_DEFINED, false, unlimited_arguments, arguments, instructions) {
            BaseParameters = base_parameters;
        }
        public override Constructor Duplicate() {
            return new Constructor(AccessMode, UnlimitedArguments, Arguments, BaseParameters, Instructions);
        }
        public override string ToString() {
            return $"Access mode: {AccessMode}, Static: {IsStatic}, Return type: {Datatype}, Arguments: {Arguments}, BaseArguments: {BaseParameters}";
        }
    }
}