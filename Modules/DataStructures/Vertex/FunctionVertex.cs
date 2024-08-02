using System.Collections.Generic;

public class FunctionVertex : Vertex {
    public Vertex Identifier;
    public List<Vertex> Arguments;
    public FunctionVertex(DataVertex identifier, List<Vertex> arguments, TokenPosition position) : base(position) {
        Identifier = identifier;
        Arguments = arguments;
    }
    /*
    public override string ToString() {
        return $"({Data})";
    }
    */
}
