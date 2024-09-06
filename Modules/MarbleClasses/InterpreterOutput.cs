public abstract class InterpreterOutput {
    public FlowController IsContextControl(TokenPosition position) {
        if (this is not FlowController) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Expexted break");
        return this as FlowController;
    }
    public MarbleData ToMarbleData(TokenPosition position) {
        if (this is MarbleData) {
            return this as MarbleData;
        } else if (this is StorageVariable) {
            if ((this as StorageVariable).Data == null) throw new InterpreterError(InterpreterError.TYPE.UNINITIALIZED_IDENTIFIER, position);
            return (this as StorageVariable).Data;
        }
        else throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Expected data");
    }
    public StorageEntity IsStorageEntity(TokenPosition position) {
        if (this is not StorageEntity) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Expected data");
        return this as StorageEntity;
    }
    public StorageVariable IsStorageVariable(TokenPosition position) {
        if (this is not StorageVariable) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Expected data");
        return this as StorageVariable;
    }
    public StorageFunction IsStorageFunction(TokenPosition position) {
        if (this is not StorageFunction) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Expected function");
        return this as StorageFunction;
    }
    public StorageClass IsStorageClass(TokenPosition position) {
        if (this is not StorageClass) throw new InterpreterError(InterpreterError.TYPE.MESSAGE, position, "Expected class");
        return this as StorageClass;
    }
}