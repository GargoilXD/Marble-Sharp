using System.Collections.Generic;
public class MatchNode : Node {
    public Node Expression {private set; get;}
    public List<CaseNode> Cases {private set; get;}
    public InstructionListNode Default {private set; get;}
    public MatchNode(Node expression, List<CaseNode> cases, InstructionListNode default_node, TokenPosition position) : base(position){
        Expression = expression;
        Cases = cases;
        Default = default_node;
    }
    public override string ToString() {
        return $"(Match: {Expression}, {(Cases == null? "()" : string.Join(", ", Cases))}, {(Default == null? "()" : Default)})";
    }
    public class CaseNode : Node {
        public Node Expression {private set; get;}
        public InstructionListNode Instructions {private set; get;}
        public CaseNode(Node expression, InstructionListNode instructions, TokenPosition position) : base(position){
            Expression = expression;
            Instructions = instructions;
        }
        public override string ToString() {
            return $"(Case: {Expression}, {Instructions})";
        }
    }
}
