using System;

public class UnaryOperatorVertex : OperatorVertex {
    public Vertex Operand;
    public UnaryOperatorVertex(OperatorToken operator_token, Vertex operand) : base(operator_token) {
        Operator = operator_token;
        Operand = operand;
    }

    public override string ToString() {
        string operatorValue;

        if (Operator.Type == Token.TYPE.OPERATOR) {
            operatorValue = Enum.GetName(typeof(OperatorToken.OPERATOR), Operator.Operator_type);
        }
        else if (Operator.Type == Token.TYPE.KEYWORD &&
                 Enum.IsDefined(typeof(DataToken.DATATYPE), Operator.Token_value)) {
            operatorValue = Enum.GetName(typeof(DataToken.DATATYPE), Operator.Token_value);
        }
        else {
            operatorValue = Operator.Token_value.ToString();
        }

        return $"({operatorValue}, {Operand})";
    }
}
