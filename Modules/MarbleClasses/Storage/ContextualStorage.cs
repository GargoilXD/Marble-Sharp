using System.Collections.Generic;
public class ContextualStorage {
	public ContextualStorage Parent;
	private Dictionary<string, StorageVariable> Variables;
	private Dictionary<string, StorageFunction> Functions;
	private Dictionary<string, StorageFunction.Constructor> Constructors;
	private Dictionary<string, StorageClass> Classes;
	public ContextualStorage() {
        Variables = new Dictionary<string, StorageVariable>();
        Functions = new Dictionary<string, StorageFunction>();
        Constructors = new Dictionary<string, StorageFunction.Constructor>();
        Classes = new Dictionary<string, StorageClass>();
        Parent = null;
    }
	public ContextualStorage(ContextualStorage parent) {
        Variables = new Dictionary<string, StorageVariable>();
        Functions = new Dictionary<string, StorageFunction>();
        Constructors = new Dictionary<string, StorageFunction.Constructor>();
        Classes = new Dictionary<string, StorageClass>();
        Parent = parent;
    }
	public ContextualStorage(ContextualStorage parent, Dictionary<string, StorageVariable> variables, Dictionary<string, StorageFunction> functions, Dictionary<string, StorageFunction.Constructor> constructors, Dictionary<string, StorageClass> classes) {
        Parent = parent;
        Variables = variables;
        Functions = functions;
        Constructors = constructors;
        Classes = classes;
    }
	public ContextualStorage CreateChild() {
		return new ContextualStorage(this);
    }
    public ContextualStorage Duplicate() {
        Dictionary<string, StorageVariable> variables = new Dictionary<string, StorageVariable>();
        Dictionary<string, StorageFunction> functions = new Dictionary<string, StorageFunction>();
        Dictionary<string, StorageFunction.Constructor> constructors = new Dictionary<string, StorageFunction.Constructor>();
        Dictionary<string, StorageClass> classes = new Dictionary<string, StorageClass>();
        foreach (KeyValuePair<string, StorageVariable> KV in Variables) {
            variables.Add(KV.Key, KV.Value.Duplicate());
        }
        foreach (KeyValuePair<string, StorageFunction> KV in Functions) {
            functions.Add(KV.Key, KV.Value.Duplicate());
        }
        foreach (KeyValuePair<string, StorageFunction.Constructor> KV in Constructors) {
            constructors.Add(KV.Key, KV.Value.Duplicate());
        }
        foreach (KeyValuePair<string, StorageClass> KV in Classes) {
            if (KV.Value.Storage == this) classes.Add(KV.Key, KV.Value);
            else classes.Add(KV.Key, KV.Value.Duplicate());
        }
        ContextualStorage parent = null;
        if (Parent != null) parent = Parent.Duplicate();
        ContextualStorage storage = new ContextualStorage(parent, variables, functions, constructors, classes);
        return storage;
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
    public bool GetEntity(string name, out StorageEntity storage_entity) {
		if (Variables.ContainsKey(name)){
            storage_entity = Variables[name];
			return true;
        } else if (Functions.ContainsKey(name)) {
            storage_entity = Functions[name];
			return true;
        } else if (Classes.ContainsKey(name)) {
            storage_entity = Classes[name];
			return true;
        }
		else {
			if (Parent != null) return Parent.GetEntity(name, out storage_entity);
            storage_entity = null;
            return false;
        }
    }
	public StorageVariable GetVariable(string name) {
		if (Variables.ContainsKey(name)){
			return Variables[name];
        }
		else {
			if (Parent != null) return Parent.GetVariable(name);
            return null;
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
    public void CreateConstructor(string name, StorageFunction.Constructor definition) {
        Constructors[name] = definition;
    }
    public bool HasConstructor(string name) {
        bool has = Constructors.ContainsKey(name);
		return has;
    }
    public StorageFunction.Constructor GetConstructor(string name) {
        if (Constructors.ContainsKey(name)){
			return Constructors[name];
        }
		else {
            return null;
        }
    }
    public void CreateClass(string name, StorageClass definition) {
        Classes[name] = definition;
    }
    public bool HasClass(string name) {
        bool has = Classes.ContainsKey(name);
		if (!has) {
			if (Parent != null) {
				has = Parent.HasClass(name);
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