using System;
using System.Collections.Generic;
public class TokenSequencer {
    public static SequenceDefinitionSection[] function_sequence_definition = new SequenceDefinitionSection[] {
        new SequenceDefinitionSection(new SequenceDefinitionItem[][] {
            new SequenceDefinitionItem[] {
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.KEYWORD_TYPE, KeywordToken.TYPE.MODIFIER, true),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.KEYWORD_TYPE, KeywordToken.TYPE.DATATYPE, "Expected Datatype"),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.CUSTOM, null, "Expected identifier")
            },
            new SequenceDefinitionItem[] {
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.KEYWORD_TYPE, KeywordToken.TYPE.DATATYPE, "Expected Datatype"),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.CUSTOM, null, "Expected identifier")
            }
        }),
        new SequenceDefinitionSection(new SequenceDefinitionItem[][] {
            new SequenceDefinitionItem[] {
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.SYMBOL, SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.CUSTOM, delegate () {}, true),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.SYMBOL, SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET),

                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.SYMBOL, SymbolToken.SYMBOL.LEFT_CURLY_BRACKET),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.CUSTOM, delegate () {}, true),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.SYMBOL, SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)
            }
        })
    };
    public static SequenceDefinitionSection[] variable_sequence_definition = new SequenceDefinitionSection[] {
        new SequenceDefinitionSection(new SequenceDefinitionItem[][] {
            new SequenceDefinitionItem[] {
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.KEYWORD_TYPE, KeywordToken.TYPE.MODIFIER, true),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.KEYWORD_TYPE, KeywordToken.TYPE.DATATYPE, "Expected Datatype"),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.IDENTIFIER, null, "Expected identifier")
            },
            new SequenceDefinitionItem[] {
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.KEYWORD_TYPE, KeywordToken.TYPE.DATATYPE, "Expected Datatype"),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.IDENTIFIER, null, "Expected identifier")
            }
        }),
        new SequenceDefinitionSection(false, new SequenceDefinitionItem[][] {
            new SequenceDefinitionItem[] {
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.OPERATOR, OperatorToken.OPERATOR.ASSIGN),
                new SequenceDefinitionItem(SequenceDefinitionItem.TYPE.DATA, DataToken.TYPE.INTEGER, "Expected operand")
            }
        })
    };
    private SequenceDefinitionItem[][][] SequenceDefinition;
    public Action NextToken;
    public Token Current_token = null;
    public TokenSequencer(SequenceDefinitionItem[][][] sequence_definition) {
        SequenceDefinition = sequence_definition;
    }
    public List<Token> GetSequence() {
        List<Token> sequence = new List<Token>();
        foreach (SequenceDefinitionItem[][] section in SequenceDefinition) {
            bool pass = true;
            foreach (SequenceDefinitionItem[] series in section) {
                pass = true;
                bool started = false;
                bool next = false;
                foreach (SequenceDefinitionItem item in series) {
                    Action<Func<bool>> thing = delegate (Func<bool> logic) {
                        if (item.Recursive) {
                            List<Token> sub_sequence = new List<Token>();
                            while (logic()) {
                                sub_sequence.Add(Current_token);
                                NextToken();
                            }
                            if (sub_sequence.Count == 0) {
                                if (started) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, Current_token.Position, item.ErrorMessage);
                                else next = true;
                            } else sequence.AddRange(sub_sequence);
                        } else {
                            if (logic()) sequence.Add(Current_token);
                            else if (started) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, Current_token.Position, item.ErrorMessage);
                            else next = true;
                        }
                    };
                    switch (item.Type) {
                        case SequenceDefinitionItem.TYPE.NONE:
                            throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, Current_token.Position);
                        case SequenceDefinitionItem.TYPE.IDENTIFIER:
                            thing(() => Current_token.is_data(DataToken.TYPE.WORD));
                            break;
                        case SequenceDefinitionItem.TYPE.OPERATOR:
                            thing(() => Current_token.is_operator((OperatorToken.OPERATOR) item.Data));
                            break;
                        case SequenceDefinitionItem.TYPE.KEYWORD:
                            thing(() => Current_token.is_keyword((KeywordToken.KEYWORD) item.Data));
                            break;
                        case SequenceDefinitionItem.TYPE.KEYWORD_TYPE:
                            thing(() => Current_token.is_keyword_type((KeywordToken.TYPE) item.Data));
                            break;
                        case SequenceDefinitionItem.TYPE.DATA:
                            thing(() => Current_token.is_data((DataToken.TYPE) item.Data));
                            break;
                        case SequenceDefinitionItem.TYPE.SYMBOL:
                            thing(() => Current_token.is_symbol((SymbolToken.SYMBOL) item.Data));
                            break;
                        case SequenceDefinitionItem.TYPE.CUSTOM:
                            break;
                    }
                    if (next) {
                        pass = false;
                        break;
                    }
                    started = true;
                    if (!item.Recursive) NextToken();
                }
                if (!next) break;
            }
            if (!pass) return sequence;
        }
        return sequence;
    }
}

public struct SequenceDefinitionSection {
    public bool Required;
    public SequenceDefinitionItem[][] Section;
    public SequenceDefinitionSection(bool required, SequenceDefinitionItem[][] section) {
        Required = required;
        Section = section;
    }
    public SequenceDefinitionSection(SequenceDefinitionItem[][] section) {
        Required = true;
        Section = section;
    }
}

public struct SequenceDefinitionItem {
    public enum TYPE {
        NONE,
        OPERATOR,
        IDENTIFIER,
        KEYWORD,
        KEYWORD_TYPE,
        DATA,
        SYMBOL,
        CUSTOM

    }
    public TYPE Type;
    public object Data;
    public string ErrorMessage;
    public bool Recursive;
    public SequenceDefinitionItem(TYPE type, object data) {
        Type = type;
        Data = data;
        ErrorMessage = "";
        Recursive = false;
    }
    public SequenceDefinitionItem(TYPE type, object data, string error_message) {
        Type = type;
        Data = data;
        ErrorMessage = error_message;
        Recursive = false;
    }
    public SequenceDefinitionItem(TYPE type, object data, bool recursive, string error_message = "") {
        Type = type;
        Data = data;
        ErrorMessage = error_message;
        Recursive = recursive;
    }
    public SequenceDefinitionItem(TYPE type, Func<List<Token>> data, bool recursive, string error_message = "") {
        Type = type;
        Data = data;
        ErrorMessage = error_message;
        Recursive = recursive;
    }
    
}