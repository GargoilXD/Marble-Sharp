using System.Collections.Generic;
public class IFNode : Node {
    public Node Expression {private set; get;}
    public OperandInstructionNode Implication {private set; get;}
    public List<ElseIFNode> Else_IF_nodes {private set; get;}
    public OperandInstructionNode Inverse {private set; get;}
    public IFNode(Node expression, OperandInstructionNode implication, List<ElseIFNode> else_if_nodes, OperandInstructionNode inverse, TokenPosition position) : base(position){
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
        public OperandInstructionNode Implication {private set; get;}
        public ElseIFNode(Node expression, OperandInstructionNode implication, TokenPosition position) : base(position){
            Expression = expression;
            Implication = implication;
        }
        public override string ToString() {
            return $"(Else IF: {Expression}, {Implication})";
        }
    }
}
