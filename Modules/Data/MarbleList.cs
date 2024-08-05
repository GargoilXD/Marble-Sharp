using System.Collections.Generic;

public class MarbleList : MarbleData {
    public List<MarbleData> Elements;
    public MarbleList(List<MarbleData> elements, bool initialized = true) : base(initialized) {
        Elements = elements;
    }
    public override MarbleList Duplicate() {
        return new MarbleList(Elements);
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
    public override MarbleData add(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData subtract(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData multiply(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData divide(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData exponent(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData modolus(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData and(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData or(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData equals(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData not_equals(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData greater_than(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData greater_than_or_equals(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData lesser_than(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData lesser_than_or_equals(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData contains(MarbleData operand){
        switch (operand) {
            case MarbleBoolean:
                break;
            case MarbleInteger:
                break;
            case MarbleFloat:
                break;
            case MarbleString:
                break;
            case MarbleObject:
                break;
            case MarbleVariant:
                break;
            case MarbleList:
                break;
            case MarbleDictionary:
                break;
        }
        throw new InterpreterError(InterpreterError.TYPE.INVALID_OPERATION, null);
    }
    public override MarbleData negate(){
        Elements.Reverse();
        return this;
    }
}
