using System;
using System.Collections.Generic;
using System.Linq;

public class MarbleList : MarbleData {
    public List<MarbleData> Elements;
    public MarbleList(List<MarbleData> elements, bool initialized = true) : base(initialized) {
        Elements = elements;
    }
    public override MarbleList Duplicate() {
        List<MarbleData> elements = new List<MarbleData>();
        foreach (MarbleData data in Elements) {
            elements.Add(data.Duplicate());
        }
        return new MarbleList(elements);
    }
    public override string ToString() {
        return string.Join(", ", Elements);
    }
    public override void set_data(object data) {
        Elements = (List<MarbleData>) data;
    }
    public override object get_data() {
        return Elements;
    }
    public static MarbleList Convert(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return Convert(data.ToStatic(position), position);
            case MarbleString data: {
                List<MarbleData> new_list = new List<MarbleData>();
                foreach (char item in data.Value) {
                    new_list.Add(new MarbleString(item + ""));
                }
                return new MarbleList(new_list);
            }
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleObject: case MarbleList: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleList convert(MarbleData operand, TokenPosition position) {
        return Convert(operand, position);
    }
    public override MarbleData add(MarbleData operand, TokenPosition position) {
        MarbleList new_list = new MarbleList(new List<MarbleData>());
        new_list.Elements.AddRange(Duplicate().Elements);
        new_list.Elements.Add(operand.Duplicate());
        return new_list;

    }
    public override MarbleData subtract(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData multiply(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return multiply(data.ToStatic(position), position);
            case MarbleInteger: case MarbleBoolean: case MarbleFloat:
                List<MarbleData> new_list = new List<MarbleData>();
                for (int x = 0; x < MarbleInteger.Convert(operand, position).Value; x++) new_list.AddRange(Duplicate().Elements);
                return new MarbleList(new_list);
            case MarbleString: case MarbleList: case MarbleObject: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData divide(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return divide(data.ToStatic(position), position);
            case MarbleInteger: case MarbleBoolean: case MarbleFloat: {
                List<MarbleData> sub_lists = new List<MarbleData>();
                MarbleList sub_list = new MarbleList(new List<MarbleData>());
                int sub_lenght = Elements.Count / Math.Abs(MarbleInteger.Convert(operand, position).Value);
                for (int x = 0; x < Elements.Count; x++) {
                    if (x % sub_lenght == 0) {
                        sub_lists.Add(sub_list);
                        sub_list = new MarbleList(new List<MarbleData>());
                    }
                    sub_list.Elements.Add(Elements[x]);
                }
                return new MarbleList(sub_lists);
            }
            case MarbleString: case MarbleList: case MarbleObject: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleInteger integer_divide(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData exponent(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData modolus(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    
    public override MarbleData bitwise_and(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData bitwise_or(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return equals(data.ToStatic(position), position);
            case MarbleList:
                break;
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleObject: case MarbleString: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean not_equals(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return not_equals(data.ToStatic(position), position);
            case MarbleList:
                break;
            case MarbleBoolean: case MarbleInteger: case MarbleFloat: case MarbleObject: case MarbleString: case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean greater_than(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean greater_than_or_equals(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean lesser_than(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean lesser_than_or_equals(MarbleData operand, TokenPosition position) {
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleBoolean contains(MarbleData operand, TokenPosition position) {
        switch (operand) {
            case MarbleVariant data:
                return contains(data.ToStatic(position), position);
            case MarbleString: case MarbleBoolean:case MarbleInteger: case MarbleFloat: case MarbleList: case MarbleDictionary: case MarbleObject:
                return new MarbleBoolean(Elements.Any(delegate (MarbleData element) {
                    return element.get_data() == operand.get_data();
                }));
        }
        throw new InterpreterError(InterpreterError.TYPE.INCOMPATIBLE_TYPES, position);
    }
    public override MarbleData negate(TokenPosition position) {
        MarbleList reverse = Duplicate();
        reverse.Elements.Reverse();
        return reverse;
    }
}