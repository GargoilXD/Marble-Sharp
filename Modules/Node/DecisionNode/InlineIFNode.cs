using System.Collections.Generic;
public class InlineIFNode : Node {
    public Node Expression {private set; get;}
    public Node Implication {private set; get;}
    public List<InlineElseIFNode> Children {private set; get;}
    public Node Inverse {private set; get;}
    public InlineIFNode(Node expression, Node implication, List<InlineElseIFNode> children, Node inverse, TokenPosition position) : base(position){
        Expression = expression;
        Implication = implication;
        Children = children;
        Inverse = inverse;
    }
    public override string ToString() {
        return $"(InlineIF: {Expression}, {Implication}, {(Children == null? "()" : string.Join(", ", Children))}, {(Inverse == null? "()" : Inverse)})";
    }
    public class InlineElseIFNode : Node {
        public Node Expression {private set; get;}
        public Node Implication {private set; get;}
        public InlineElseIFNode(Node expression, Node implication, TokenPosition position) : base(position){
            Expression = expression;
            Implication = implication;
        }
        public override string ToString() {
            return $"(Else IF: {Expression}, {Implication})";
        }
    }
}
