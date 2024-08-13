using System.Collections.Generic;
public class IFNode : Node {
    public Node Expression {private set; get;}
    public InstructionListNode Implication {private set; get;}
    public List<ElseIFNode> Else_IF_nodes {private set; get;}
    public InstructionListNode Inverse {private set; get;}
    public IFNode(Node expression, InstructionListNode implication, List<ElseIFNode> else_if_nodes, InstructionListNode inverse, TokenPosition position) : base(position){
        Expression = expression;
        Implication = implication;
        Else_IF_nodes = else_if_nodes;
        Inverse = inverse;
    }
    public override string ToString() {
        return $"(IF: {Expression}, {Implication}, {(Else_IF_nodes == null? "()" : string.Join(", ", Else_IF_nodes))}, {(Inverse == null? "()" : Inverse)})";
    }
    public class ElseIFNode : Node {
        public Node Expression {private set; get;}
        public InstructionListNode Implication {private set; get;}
        public ElseIFNode(Node expression, InstructionListNode implication, TokenPosition position) : base(position){
            Expression = expression;
            Implication = implication;
        }
        public override string ToString() {
            return $"(Else IF: {Expression}, {Implication})";
        }
    }
}
