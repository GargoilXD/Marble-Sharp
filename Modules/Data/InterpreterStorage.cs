using System.Collections.Generic;
public class InterpreterStorage {
	private InterpreterStorage Parent;
	private Dictionary<string, MarbleData> Variables;
	private Dictionary<string, object> Functions;
	public InterpreterStorage() {
        Variables = new Dictionary<string, MarbleData>();
        Functions = new Dictionary<string, object>();
        Parent = null;
    }
	public InterpreterStorage(Dictionary<string, MarbleData> variables, Dictionary<string, object> functions) {
        Variables = variables;
        Functions = functions;
        Parent = null;
    }
	public InterpreterStorage(InterpreterStorage parent) {
        Parent = parent;
        Variables = new Dictionary<string, MarbleData>();
        Functions = new Dictionary<string, object>();
    }
	public InterpreterStorage CreateChild() {
		return new InterpreterStorage(this);
    }
	public void Reset() {
		Variables.Clear();
		Functions.Clear();
    }
	public MarbleData CreateVariable(string key, MarbleData value = null) {
		Variables[key] = value;
		return value;
    }
    public void DeleteVariable(string key) {
        Variables.Remove(key);
    }
	public bool HasVariable(string name){
		bool has = Variables.ContainsKey(name);
		if (!has) {
			if (Parent != null) {
				has = Parent.HasVariable(name);
            }
        }
		return has;
    }
	public void SetVariable(string name, MarbleData value) {
		if (Variables.ContainsKey(name)){
			Variables[name] = value;
        }
		else {
			if (Parent != null) {
				Parent.SetVariable(name, value);
            }
        }
    }
	public MarbleData GetVariable(string name) {
		if (Variables.ContainsKey(name)){
			return Variables[name];
        }
		else {
			if (Parent != null) {
				return Parent.GetVariable(name);
            }
        }
        throw new InterpreterError(InterpreterError.TYPE.UNDEFINED_IDENTIFIER, null);
    }
    public override string ToString(){
        return $"Variables:\n{string.Join("\n", Variables)}";
    }
}