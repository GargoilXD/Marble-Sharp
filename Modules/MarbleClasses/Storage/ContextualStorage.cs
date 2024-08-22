using System.Collections.Generic;
public class ContextualStorage {
	private ContextualStorage Parent;
	private Dictionary<string, StorageVariable> Variables;
	private Dictionary<string, StorageFunction> Functions;
	private Dictionary<string, StorageClass> Classes;
	public ContextualStorage() {
        Variables = new Dictionary<string, StorageVariable>();
        Functions = new Dictionary<string, StorageFunction>();
        Classes = new Dictionary<string, StorageClass>();
        Parent = null;
    }
	public ContextualStorage(ContextualStorage parent) {
        Parent = parent;
        Variables = new Dictionary<string, StorageVariable>();
        Functions = new Dictionary<string, StorageFunction>();
        Classes = new Dictionary<string, StorageClass>();
    }
	public ContextualStorage CreateChild() {
		return new ContextualStorage(this);
    }
    public ContextualStorage Duplicate() {
        Dictionary<string, StorageVariable> variables = new Dictionary<string, StorageVariable>();
        Dictionary<string, StorageFunction> functions = new Dictionary<string, StorageFunction>();
        Dictionary<string, StorageClass> classes = new Dictionary<string, StorageClass>();
        foreach (KeyValuePair<string, StorageVariable> KV in Variables) {
            variables.Add(KV.Key, KV.Value.Duplicate());
        }
        foreach (KeyValuePair<string, StorageFunction> KV in Functions) {
            functions.Add(KV.Key, KV.Value.Duplicate());
        }
        foreach (KeyValuePair<string, StorageClass> KV in Classes) {
            classes.Add(KV.Key, KV.Value.Duplicate());
        }
        return new ContextualStorage(this);
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
	public bool GetVariable(string name, out StorageVariable storage_variable) {
		if (Variables.ContainsKey(name)){
            storage_variable = Variables[name];
			return true;
        }
		else {
			if (Parent != null) return Parent.GetVariable(name, out storage_variable);
            storage_variable = null;
            return false;
        }
    }
    public void CreateFunction(string name, StorageFunction definition) {
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
    public StorageFunction GetFunction(string name) {
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
    public void CreateClass(string name, StorageClass definition) {
        Classes[name] = definition;
    }
    public bool HasClass(string name) {
        bool has = Classes.ContainsKey(name);
		if (!has) {
			if (Parent != null) {
				has = Parent.HasFunction(name);
            }
        }
		return has;
    }
    public StorageClass GetClass(string name) {
        if (Classes.ContainsKey(name)){
			return Classes[name].Duplicate();
        }
		else {
			if (Parent != null) {
				return Parent.GetClass(name);
            }
        }
        return null;
    }
    public override string ToString(){
        return $"Variables:\n{string.Join("\n", Variables)}";
    }
}