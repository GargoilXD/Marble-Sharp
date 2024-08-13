using System.Collections.Generic;
public class InterpreterStorage {
	private InterpreterStorage Parent;
	private Dictionary<string, StorageVariable> Variables;
	private Dictionary<string, FunctionDefinitionNode> Functions;
	public InterpreterStorage() {
        Variables = new Dictionary<string, StorageVariable>();
        Functions = new Dictionary<string, FunctionDefinitionNode>();
        Parent = null;
    }
	public InterpreterStorage(Dictionary<string, StorageVariable> variables, Dictionary<string, FunctionDefinitionNode> functions) {
        Variables = variables;
        Functions = functions;
        Parent = null;
    }
	public InterpreterStorage(InterpreterStorage parent) {
        Parent = parent;
        Variables = new Dictionary<string, StorageVariable>();
        Functions = new Dictionary<string, FunctionDefinitionNode>();
    }
	public InterpreterStorage CreateChild() {
		return new InterpreterStorage(this);
    }
	public void Reset() {
		Variables.Clear();
		Functions.Clear();
    }
	public void CreateVariable(string key, StorageVariable storage_variable) {
		Variables[key] = storage_variable;
    }
    public void DeleteVariable(string key) {
        Variables.Remove(key);
    }
	public bool HasVariable(string name) {
		bool has = Variables.ContainsKey(name);
		if (!has) {
			if (Parent != null) {
				has = Parent.HasVariable(name);
            }
        }
		return has;
    }
	public StorageVariable GetVariable(string name) {
		if (Variables.TryGetValue(name, out StorageVariable storage_variable)){
			return storage_variable;
        }
		else {
			if (Parent != null) {
				return Parent.GetVariable(name);
            }
        }
        return storage_variable;
    }
    public void CreateFunction(string name, FunctionDefinitionNode definition) {
        Functions[name] = definition;
    }
    public bool HasFunction(string name) {
        bool has = Functions.ContainsKey(name);
		if (!has) {
			if (Parent != null) {
				has = Parent.HasFunction(name);
            }
        }
		return has;
    }
    public FunctionDefinitionNode GetFunction(string name) {
        if (Functions.ContainsKey(name)){
			return Functions[name];
        }
		else {
			if (Parent != null) {
				return Parent.GetFunction(name);
            }
        }
        return null;
    }
    public override string ToString(){
        return $"Variables:\n{string.Join("\n", Variables)}";
    }
}