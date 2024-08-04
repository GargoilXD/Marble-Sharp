using System.Collections.Generic;
public class IFNode : Node {
    public Node Expression {private set; get;}
    public InstructionListNode Implication {private set; get;}
    public List<IFNode> Children {private set; get;}
    public InstructionListNode Inverse {private set; get;}

    public IFNode(Node expression, InstructionListNode implication, List<IFNode> children, InstructionListNode inverse, TokenPosition position) : base(position){
        Expression = expression;
        Implication = implication;
        Children = children;
        Inverse = inverse;
    }
    public override string ToString() {
        return $"(IF: {Expression}, {Implication}, {(Children == null? "()" : string.Join(", ", Children))}, {(Inverse == null? "()" : Inverse)})";
    }
}
