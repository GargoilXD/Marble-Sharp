using System;
using System.Collections.Generic;
using System.Linq;
public class Parser {
    private static readonly OperatorToken.OPERATOR[][] PRECEDENCE = new OperatorToken.OPERATOR[][] {
        new[] { OperatorToken.OPERATOR.DOT },
        new[] { OperatorToken.OPERATOR.IS },
        new[] { OperatorToken.OPERATOR.EXPONENT },
        new[] { OperatorToken.OPERATOR.MODOLUS },
        new[] { OperatorToken.OPERATOR.MULTIPLY, OperatorToken.OPERATOR.DIVIDE, OperatorToken.OPERATOR.INTEGER_DIVIDE },
        new[] { OperatorToken.OPERATOR.ADD, OperatorToken.OPERATOR.SUBTRACT },
        new[] { OperatorToken.OPERATOR.IN },
        new[] { OperatorToken.OPERATOR.EQUALS, OperatorToken.OPERATOR.NOT_EQUALS, OperatorToken.OPERATOR.LESSER_THAN, OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS, OperatorToken.OPERATOR.GREATER_THAN, OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS },
        new[] { OperatorToken.OPERATOR.AND, OperatorToken.OPERATOR.OR, OperatorToken.OPERATOR.BITWISE_AND, OperatorToken.OPERATOR.BITWISE_OR },
        new[] { OperatorToken.OPERATOR.COLON },
        new[] { OperatorToken.OPERATOR.ASSIGN, OperatorToken.OPERATOR.ADD_AND_ASSIGN, OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN, OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN, OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN, OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN, OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN },
    };
    private Func<Node> get_expression => (
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_binary_node(
        () => get_operand_node(),
        PRECEDENCE[0], get_operand_node),
        PRECEDENCE[1], get_operand_node),
        PRECEDENCE[2], get_operand_node),
        PRECEDENCE[3], get_operand_node),
        PRECEDENCE[4], get_operand_node),
        PRECEDENCE[5], get_operand_node),
        PRECEDENCE[6], get_operand_node),
        PRECEDENCE[7], get_operand_node),
        PRECEDENCE[8], get_operand_node),
        PRECEDENCE[9], get_operand_node),
        PRECEDENCE[10], get_operand_node)
    );
    private int Index;
    private Token CurrentToken;
    private Token PreviousToken;
    private List<Token> Tokens;
    private List<string> NewDatatypes = new List<string>(){};
    public Action<string> OnNewDatatype;
    public Action<List<string>> ClearDatatypes;
    private void next_token() {
        Index += 1;
        if (Index < Tokens.Count) {
            PreviousToken = CurrentToken;
            CurrentToken = Tokens[Index];
        }
    }
    private void AddNewDataType(DataToken datatype) {
        string datatype_name = (string) datatype.Data;
        if (NewDatatypes.Contains(datatype_name)) throw new ParserError(ParserError.TYPE.ALREADY_DEFINED_DATATYPE, datatype.Position);
        NewDatatypes.Add(datatype_name);
        OnNewDatatype(datatype_name);
    }
    private List<Node> get_bracket_node(SymbolToken.SYMBOL bracket_symbol, string start_error, string end_error, Func<Node> getter) {
        if (!CurrentToken.is_symbol(bracket_symbol, out SymbolToken start)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, $"Expected '{start_error}'");
        next_token();
        List<Node> elements = new List<Node>();
        while (!(CurrentToken.is_symbol(bracket_symbol + 1) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            elements.Add(getter());
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
            next_token();
        }
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        if (!CurrentToken.is_symbol(bracket_symbol + 1)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, start.Position + CurrentToken.Position, $"Expected '{end_error}'");
        next_token();
        return elements;
    }
    private Node get_operand_node() {
        switch (CurrentToken) {
            case DataToken data_token:
                next_token();
                if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)){
                    if (!data_token.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, data_token.Position, "Expected Indetifier");
                    return new FunctionNode(data_token, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", get_expression), data_token.Position + PreviousToken.Position);
                }
                return DataNode.FromToken(data_token);
            case SymbolToken symbol_token:
                switch (symbol_token.Symbol) {
                    case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET:
                        next_token();
                        Node expression = get_expression();
                        if (CurrentToken is DataToken) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected operator");
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position, "Expected ')'");
                        next_token();
                        return expression;
                    case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET:
                        return new DataNode(DataNode.TYPE.DICTIONARY, get_bracket_node(symbol_token.Symbol, "{", "}", delegate {
                            if (CurrentToken is not DataToken) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected data key");
                            if (CurrentToken.is_data(DataToken.TYPE.NULL)) throw new Error(CurrentToken.Position, "Key can not be null");
                            Token key = CurrentToken;
                            next_token();
                            if (!CurrentToken.is_operator(OperatorToken.OPERATOR.COLON)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected ':'");
                            next_token();
                            return new DataNode(DataNode.TYPE.DICTIONARY, new Dictionary<string, object>(){{"Token", key}, {"Node", get_operand_node()}}, null);
                        }), symbol_token.Position + PreviousToken.Position);
                    case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET:
                        return new DataNode(DataNode.TYPE.LIST, get_bracket_node(symbol_token.Symbol, "[", "]", get_expression), symbol_token.Position + PreviousToken.Position);
                }
                break;
            case KeywordToken keyword_token:
                switch (keyword_token.Keyword) {
                    case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.INTEGER: case KeywordToken.KEYWORD.BOOLEAN: case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST: case KeywordToken.KEYWORD.DICTIONARY:
                        next_token();
                        if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) return new FunctionNode(keyword_token, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", get_expression), keyword_token.Position + PreviousToken.Position);
                        return new DataNode(DataNode.TYPE.DATATYPE, keyword_token.Keyword, keyword_token.Position);
                    case KeywordToken.KEYWORD.PRINT: case KeywordToken.KEYWORD.RANGE: case KeywordToken.KEYWORD.ASSERT: case KeywordToken.KEYWORD.RANDOM: case KeywordToken.KEYWORD.INPUT:
                        next_token();
                        return new FunctionNode(keyword_token, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", get_expression), keyword_token.Position + PreviousToken.Position);
                    case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position, "Exppected if");
                    case KeywordToken.KEYWORD.IF:
                        return get_inline_if_node(keyword_token.Position);
                }
                break;
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.NOT: case OperatorToken.OPERATOR.ADD: case OperatorToken.OPERATOR.SUBTRACT:
                        next_token();
                        return new UnaryOperatorNode(operator_token, get_operand_node());
                    default:
                        throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "Expected unary operator");
                }
        }
        throw new ParserError(ParserError.TYPE.EXPECTED_OPERAND, CurrentToken.Position);
    }
    private UnaryOperatorNode get_unary_nodee() {
        switch (CurrentToken) {
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.NOT: case OperatorToken.OPERATOR.ADD: case OperatorToken.OPERATOR.SUBTRACT:
                        next_token();
                        return new UnaryOperatorNode(operator_token, get_operand_node());
                }
                break;
        }
        throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "Expected unary operator");
    }
    private InstructionListNode get_instruction_list_node() {
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, out SymbolToken start)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        List<Node> instructions = get_nodes(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET);
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, start.Position + CurrentToken.Position, "Expected '}'");
        next_token();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        return new InstructionListNode(instructions, start.Position + PreviousToken.Position);
    }
    private IFNode get_if_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node if_expression = get_operand_node();
        InstructionListNode implication = get_instruction_list_node();
        List<IFNode.ElseIFNode> children = new List<IFNode.ElseIFNode>();
        while (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE_IF)) {
            KeywordToken else_if_token = CurrentToken as KeywordToken;
            next_token();
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
            children.Add(new IFNode.ElseIFNode(get_operand_node(), get_instruction_list_node(), else_if_token.Position + PreviousToken.Position));
        }
        InstructionListNode inverse = null;
        if (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE)) {
            next_token();
            inverse = get_instruction_list_node();
        }
        return new IFNode(if_expression, implication, children, inverse, start_position + PreviousToken.Position);
    }
    private MatchNode get_match_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node expression = get_operand_node();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
        next_token();
        List<MatchNode.CaseNode> cases = new List<MatchNode.CaseNode>();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        if (!CurrentToken.is_keyword(KeywordToken.KEYWORD.CASE)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected case");
        while (CurrentToken.is_keyword(KeywordToken.KEYWORD.CASE)) {
            KeywordToken case_token = CurrentToken as KeywordToken;
            next_token();
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
            DataNode case_expression = new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", get_expression), PreviousToken.Position);
            if ((case_expression.Data as List<Node>).Count == 0) throw new ParserError(ParserError.TYPE.EMPTHY_CASE, case_token.Position);
            cases.Add(new MatchNode.CaseNode(case_expression, get_instruction_list_node(), case_token.Position + PreviousToken.Position));
        }
        InstructionListNode default_node = null;
        if (CurrentToken.is_keyword(KeywordToken.KEYWORD.DEFAULT)) {
            next_token();
            default_node = get_instruction_list_node();
        }
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, start_position + PreviousToken.Position, "Expected '{'");
        next_token();
        return new MatchNode(expression, cases, default_node, start_position + PreviousToken.Position);
    }
    private InlineIFNode get_inline_if_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node if_expression = get_operand_node();
        Node implication = get_operand_node();
        List<InlineIFNode.InlineElseIFNode> children = new List<InlineIFNode.InlineElseIFNode>();
        while (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE_IF)) {
            KeywordToken else_if_token = CurrentToken as KeywordToken;
            next_token();
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
            children.Add(new InlineIFNode.InlineElseIFNode(get_operand_node(), get_operand_node(), else_if_token.Position + PreviousToken.Position));
        }
        Node inverse = null;
        if (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE)) {
            next_token();
            inverse = get_operand_node();
        }
        return new InlineIFNode(if_expression, implication, children, inverse, start_position + PreviousToken.Position);
    }
    private ForNode get_for_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        next_token();
        Node iterator = get_definition_node(get_variable_definition_node);
        if (!CurrentToken.is_operator(OperatorToken.OPERATOR.IN))  throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "'in'");
        OperatorToken In = CurrentToken as OperatorToken;
        next_token();
        Node iteratable = get_operand_node();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected ')'");
        next_token();
        InstructionListNode instructions = get_instruction_list_node();
        return new ForNode(iterator, iteratable, instructions, start_position + PreviousToken.Position);

    }
    private WhileNode get_while_node(TokenPosition start_position) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node expression = get_operand_node();
        InstructionListNode instructions = get_instruction_list_node();
        return new WhileNode(expression, instructions, start_position + PreviousToken.Position);
    }
    private bool word_is_datatype(Token token) {
        return token.is_data(DataToken.TYPE.WORD, out DataToken data_token) && NewDatatypes.Contains(data_token.Data);
    }
    private DefinitionNode get_definition_node(Func<TokenPosition, List<KeywordToken>, Token, DataToken, DefinitionNode> getter) {
        TokenPosition start_position = CurrentToken.Position;
        List<KeywordToken> settings = new List<KeywordToken>();
        bool access = false;
        while (CurrentToken.is_keyword_type(KeywordToken.TYPE.DEFINITION_SETTING, out KeywordToken keyword_token)) {
            if (settings.Any(token => token.Keyword == keyword_token.Keyword)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position, "Duplicate");
            if (keyword_token.Keyword == KeywordToken.KEYWORD.PUBLIC || keyword_token.Keyword == KeywordToken.KEYWORD.PRIVATE) {
                if (access) {
                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position, "Duplicate");
                } else {
                    access = true;
                }
            }
            settings.Add(keyword_token);
            next_token();
        }
        if (!word_is_datatype(CurrentToken) && !CurrentToken.is_keyword_type(KeywordToken.TYPE.DATATYPE)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Datatype");
        Token datatype_token = CurrentToken;
        next_token();
        if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
        if (word_is_datatype(CurrentToken)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
        DataToken identifier = CurrentToken as DataToken;
        next_token();
        return getter(start_position, settings, datatype_token, identifier);
    }
    private VariableDefinitionNode get_variable_definition_node(TokenPosition start_position, List<KeywordToken> settings, Token datatype_token, DataToken identifier) {
        Node value = null;
        if (CurrentToken.is_operator(OperatorToken.OPERATOR.ASSIGN)) {
            next_token();
            value = get_expression();
        }
        if (CurrentToken is DataToken) throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "Expected '='");
        return new VariableDefinitionNode(settings, identifier, datatype_token, value, start_position + PreviousToken.Position);
    }
    private EnumerationDefinitionNode get_enumeration_definition_node(TokenPosition start_position, List<KeywordToken> settings, Token datatype_token, DataToken identifier) {
        TokenPosition start = CurrentToken.Position;
        if (!word_is_datatype(identifier)) AddNewDataType(identifier);
        List<Node> enumerations = get_bracket_node(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, "{", "}", delegate {
            if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected identifier");
            if (word_is_datatype(CurrentToken)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected identifier");
            next_token();
            return DataNode.FromToken(PreviousToken as DataToken);
        });
        if (enumerations.Count == 0) throw new Error(start + PreviousToken.Position, "Empty enumeration");
        return new EnumerationDefinitionNode(settings, identifier, enumerations, start_position + PreviousToken.Position);
    }
    private FunctionDefinitionNode get_function_definition_node(TokenPosition start_position, List<KeywordToken> settings, Token datatype_token, DataToken identifier) {
        return new FunctionDefinitionNode(settings, identifier, datatype_token, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_definition_node(get_variable_definition_node)), get_instruction_list_node(), identifier.Position + PreviousToken.Position);
    }
    private Node get_binary_node(Func<Node> left, OperatorToken.OPERATOR[] precedence, Func<Node> right) {
        Node left_node = left();
        if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET) || CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected [");
        if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET)){
            SymbolToken start = CurrentToken as SymbolToken;
            left_node = new BinaryOperatorNode(left_node, new OperatorToken(OperatorToken.OPERATOR.ACCESSOR, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET, "[", "]", get_expression), start.Position + PreviousToken.Position));
        }
        while (CurrentToken is OperatorToken && precedence.Contains(((OperatorToken) CurrentToken).Type)) {
            OperatorToken operatorToken = CurrentToken as OperatorToken;
            next_token();
            left_node = new BinaryOperatorNode(left_node, operatorToken, right());
        }
        return left_node;
    }
    private List<Node> get_nodes(SymbolToken.SYMBOL breaker) {
        List<Node> nodes = new List<Node>();
        while (!CurrentToken.is_symbol(breaker)) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            if (CurrentToken.is_symbol(breaker) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END)) break;
            switch (CurrentToken) {
                case KeywordToken keyword_token:
                    switch (keyword_token.Type) {
                        case KeywordToken.TYPE.DATATYPE:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.VARIANT: case KeywordToken.KEYWORD.INTEGER: case KeywordToken.KEYWORD.BOOLEAN:
                                case KeywordToken.KEYWORD.FLOAT: case KeywordToken.KEYWORD.STRING: case KeywordToken.KEYWORD.LIST:
                                case KeywordToken.KEYWORD.DICTIONARY:
                                    nodes.Add(get_definition_node(get_variable_definition_node));
                                    break;
                            }
                            break;
                        case KeywordToken.TYPE.DECISION:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
                                case KeywordToken.KEYWORD.CASE: case KeywordToken.KEYWORD.DEFAULT:
                                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected match");
                                case KeywordToken.KEYWORD.MATCH: 
                                    nodes.Add(get_match_node(keyword_token.Position));
                                    break;
                                case KeywordToken.KEYWORD.IF:
                                    nodes.Add(get_if_node(keyword_token.Position));
                                    break;
                            }
                            break;
                        case KeywordToken.TYPE.FLOWCONTROL:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.RETURN:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.RETURN, get_expression(), keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.BREAK:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.BREAK, null, keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.CONTINUE:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.CONTINUE, null, keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.BREAKPOINT:
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.BREAKPOINT, null, keyword_token.Position + PreviousToken.Position));
                                    break;
                            }
                            break;
                        case KeywordToken.TYPE.LOOP:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.FOR: {
                                    nodes.Add(get_for_node(keyword_token.Position));
                                    break;
                                }
                                case KeywordToken.KEYWORD.WHILE: {
                                    nodes.Add(get_while_node(keyword_token.Position));
                                    break;
                                }
                            }
                            break;
                        case KeywordToken.TYPE.DEFINITION_SETTING:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.CONSTANT: case KeywordToken.KEYWORD.STATIC: case KeywordToken.KEYWORD.PUBLIC: case KeywordToken.KEYWORD.PRIVATE: {
                                    /*nodes.Add(get_definition_node(delegate (TokenPosition start_position, List<KeywordToken> settings, Token datatype_token, DataToken identifier) {
                                        if (datatype_token.is_keyword(KeywordToken.KEYWORD.ENUMERATION)) {
                                            return get_enumeration_definition_node(start_position, settings, datatype_token, identifier);
                                        } else {
                                            return get_variable_definition_node(start_position, settings, datatype_token, identifier);
                                        }
                                    }));*/
                                    nodes.Add(get_definition_node(get_variable_definition_node));
                                    break;
                                }
                            }
                            break;
                        case KeywordToken.TYPE.DEFINITION:
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.FUNCTION:
                                    next_token();
                                    nodes.Add(get_definition_node(get_function_definition_node));
                                    break;
                                case KeywordToken.KEYWORD.CLASS: case KeywordToken.KEYWORD.STRUCTURE:
                                    throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                            }
                            break;
                        case KeywordToken.TYPE.INBUILT_FUNCTION:
                            nodes.Add(get_operand_node());
                            break;
                    }
                break;
                case DataToken data_token:
                    if (word_is_datatype(data_token)) {
                        nodes.Add(get_definition_node(get_variable_definition_node));
                        break;
                    }
                    nodes.Add(get_expression());
                    if (CurrentToken is DataToken) throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position);
                    break;
                case OperatorToken:
                    nodes.Add(get_operand_node());
                    break;
                case SymbolToken:
                    nodes.Add(get_operand_node());
                    break;
                default:
                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
            }
        }
        return nodes;
    }
    public List<Node> Parse(List<Token> tokens) {
        Tokens = tokens;
        Index = 0;
        CurrentToken = tokens[0];
        ClearDatatypes(NewDatatypes);
        NewDatatypes.Clear();
        return get_nodes(SymbolToken.SYMBOL.END);
    }
}
