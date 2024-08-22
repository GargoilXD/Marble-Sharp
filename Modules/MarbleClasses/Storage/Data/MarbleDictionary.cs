using System.Collections.Generic;
public class MarbleDictionary : MarbleData {
    public Dictionary<object, MarbleData> dictionary;
    public MarbleDictionary(Dictionary<object, MarbleData> dictionary) {
        this.dictionary = dictionary;
    }
    public override MarbleDictionary Duplicate() {
        return new MarbleDictionary(dictionary);
    }
    public override string ToString() {
        return string.Join(", ", dictionary);
    }
    public override void set_data(object data) {
        dictionary = (Dictionary<object, MarbleData>) data;
    }
    public override object get_data() {
        return dictionary;
    }
    public static MarbleList Convert(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleList convert(MarbleData operand, TokenPosition position) {
        return Convert(operand, position);
    }    public override MarbleData add(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData subtract(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData multiply(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData divide(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleInteger integer_divide(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData exponent(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData modolus(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData bitwise_and(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData bitwise_or(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean not_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean greater_than(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean lesser_than(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean contains(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData negate(TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, position);
    }
}
