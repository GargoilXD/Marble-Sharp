using System;
using System.Collections.Generic;
using System.Linq;
public class Parser {
    private static readonly OperatorToken.OPERATOR[][] PRECEDENCE = new OperatorToken.OPERATOR[][] {
        new[] { OperatorToken.OPERATOR.ASSIGN, OperatorToken.OPERATOR.ADD_AND_ASSIGN, OperatorToken.OPERATOR.SUBTRACT_AND_ASSIGN, OperatorToken.OPERATOR.MULTIPLY_AND_ASSIGN, OperatorToken.OPERATOR.DIVIDE_AND_ASSIGN, OperatorToken.OPERATOR.EXPONENT_AND_ASSIGN, OperatorToken.OPERATOR.MODOLUS_AND_ASSIGN },
        new[] { OperatorToken.OPERATOR.AND, OperatorToken.OPERATOR.OR, OperatorToken.OPERATOR.BITWISE_AND, OperatorToken.OPERATOR.BITWISE_OR },
        new[] { OperatorToken.OPERATOR.EQUALS, OperatorToken.OPERATOR.NOT_EQUALS, OperatorToken.OPERATOR.LESSER_THAN, OperatorToken.OPERATOR.LESSER_THAN_OR_EQUALS, OperatorToken.OPERATOR.GREATER_THAN, OperatorToken.OPERATOR.GREATER_THAN_OR_EQUALS },
        new[] { OperatorToken.OPERATOR.IN },
        new[] { OperatorToken.OPERATOR.ADD, OperatorToken.OPERATOR.SUBTRACT },
        new[] { OperatorToken.OPERATOR.MULTIPLY, OperatorToken.OPERATOR.DIVIDE, OperatorToken.OPERATOR.INTEGER_DIVIDE },
        new[] { OperatorToken.OPERATOR.MODOLUS },
        new[] { OperatorToken.OPERATOR.EXPONENT },
        new[] { OperatorToken.OPERATOR.IS }
    };
    private struct STRUCTURES {
        public static byte VARIABLE = 1;
        public static byte FUNCTION = 2;
        public static byte CLASS = 4;
        public static byte CONSTRUCTOR = 8;
        public static byte STATEMENT = 16;
        public static byte FLOW_CONTROL = 32;
        public static byte MODIFIER = 64;
        public static byte RETURN_STATEMENT = 128;
        public static bool IN(int structure, int container) {
            return (structure & container) != 0;
        }
    }
    private List<string> NewDatatypes = new List<string>();
    public Action<string> OnNewDatatype;
    public Action<List<string>> ClearDatatypes;
    private Node get_expression(int depth = 0) {
        if (depth == PRECEDENCE.Length - 1) return get_binary_node(get_operand_node, PRECEDENCE[depth], get_operand_node);
        else return get_binary_node(() => get_expression(depth + 1), PRECEDENCE[depth], () => get_expression(depth + 1));
    }
    private int Index;
    private Token CurrentToken;
    private Token PreviousToken;
    private List<Token> Tokens;
    private void add_new_datatype(DataToken datatype) {
        string datatype_name = datatype.Data as string;
        if (NewDatatypes.Contains(datatype_name)) throw new ParserError(ParserError.TYPE.ALREADY_DEFINED_DATATYPE, datatype.Position);
        NewDatatypes.Add(datatype_name);
        OnNewDatatype(datatype_name);
    }
    private void next_token() {
        Index += 1;
        if (Index < Tokens.Count) {
            PreviousToken = CurrentToken;
            CurrentToken = Tokens[Index];
        }
    }
    private List<Node> get_bracket_node(SymbolToken.SYMBOL bracket_symbol, string start_error, string end_error, Func<Node> getter, out TokenPosition position) {
        if (!CurrentToken.is_symbol(bracket_symbol)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, $"Expected '{start_error}'");
        position = CurrentToken.Position;
        next_token();
        List<Node> elements = new List<Node>();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        while (!(CurrentToken.is_symbol(bracket_symbol + 1) || CurrentToken.is_symbol(SymbolToken.SYMBOL.END))) {
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            elements.Add(getter());
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COMMA)) break;
            next_token();
        }
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        if (!CurrentToken.is_symbol(bracket_symbol + 1)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, position + CurrentToken.Position, $"Expected '{end_error}'");
        position += CurrentToken.Position;
        next_token();
        return elements;
    }
    private Node get_dot_operation(Node node) {
        while (CurrentToken.is_operator(OperatorToken.OPERATOR.DOT, out OperatorToken operator_token)) {
            next_token();
            if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected identifier");
            node = new BinaryOperatorNode(node, operator_token, get_bracket_operation(get_operand_node()));
        }
        return node;
    }
    private Node get_dot_or_bracket(Node node) {
        while (true) {
            switch (CurrentToken) {
                case OperatorToken operator_token:
                    if (operator_token.Type != OperatorToken.OPERATOR.DOT) return node;
                        node = get_dot_operation(node);
                        break;
                case SymbolToken symbol_token:
                    switch (symbol_token.Symbol) {
                        case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET: case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET:
                            node = get_bracket_operation(node);
                            break;
                        default:
                            return node;
                    }
                    break;
                default:
                    return node;
            }
        }
    }
    private Node get_bracket_operation(Node node) { 
        while (true) {
            switch (CurrentToken) {
                case SymbolToken symbol_token:
                    switch (symbol_token.Symbol) {
                        case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET: {
                            node = new BinaryOperatorNode(node, new OperatorToken(OperatorToken.OPERATOR.CALLER, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", get_function_parameter, out TokenPosition position), position));
                            break;
                        }
                        case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET: {
                            node = new BinaryOperatorNode(node, new OperatorToken(OperatorToken.OPERATOR.ACCESSOR, CurrentToken.Position), new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET, "[", "]", () => get_expression(), out TokenPosition position), position));
                            break;
                        }
                        default:
                            return node;
                    }
                    break;
                default:
                    return node;
            }
        }
    }
    private Node get_binary_node(Func<Node> left, OperatorToken.OPERATOR[] precedence, Func<Node> right) {
        Node left_node = get_dot_or_bracket(left());
        while (CurrentToken is OperatorToken && precedence.Contains(((OperatorToken) CurrentToken).Type)) {
            OperatorToken operatorToken = CurrentToken as OperatorToken;
            next_token();
            left_node = new BinaryOperatorNode(left_node, operatorToken, get_dot_or_bracket(right()));
        }
        return left_node;
    }
    private Node get_operand_node() {
        switch (CurrentToken) {
            case KeywordToken keyword_token:
                switch (keyword_token.Type) {
                    case KeywordToken.TYPE.MODIFIER:
                        if (keyword_token.is_keyword(KeywordToken.KEYWORD.REFERENCE)) {
                            next_token();
                            return new UnaryOperatorNode(keyword_token, DataNode.FromToken(get_identifier_token()));
                        }
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position);
                    case KeywordToken.TYPE.FLOWCONTROL:
                    case KeywordToken.TYPE.LOOP:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position);
                    case KeywordToken.TYPE.DEFINITION:
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.FUNCTION:
                                next_token();
                                bool unlimited_arguments = false;
                                if (CurrentToken.is_keyword(KeywordToken.KEYWORD.UNLIMITED)) {
                                    unlimited_arguments = true;
                                    next_token();
                                }
                                return new InlineFunctionDefinitionNode(unlimited_arguments, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", get_function_argument, out _), get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.RETURN_STATEMENT + STRUCTURES.STATEMENT), keyword_token.Position + PreviousToken.Position);
                            default:
                                throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position);
                        }
                    case KeywordToken.TYPE.DECISION:
                        switch (keyword_token.Keyword) {
                            case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
                                throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, keyword_token.Position, "Expected if");
                            case KeywordToken.KEYWORD.IF:
                                return get_if_node(keyword_token);
                            default:
                                return null;
                        }
                    case KeywordToken.TYPE.DATATYPE:
                        next_token();
                        return new UnaryOperatorNode(keyword_token, get_operand_node());
                    default:
                        next_token();
                        return KeywordNode.FromToken(keyword_token);
                }
            case DataToken data_token:
                if (data_token.Type == DataToken.TYPE.WORD) {
                    if (NewDatatypes.Contains(data_token.Data as string) && (Index + 1 < Tokens.Count) && Tokens[Index + 1] is DataToken) {
                        next_token();
                        return new UnaryOperatorNode(data_token, get_operand_node());
                    }
                    if ((Index + 1 < Tokens.Count) && Tokens[Index + 1] is DataToken) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Not datatype");
                }
                next_token();
                return DataNode.FromToken(data_token);
            case OperatorToken operator_token:
                switch (operator_token.Type) {
                    case OperatorToken.OPERATOR.ADD: case OperatorToken.OPERATOR.SUBTRACT: case OperatorToken.OPERATOR.NOT: case OperatorToken.OPERATOR.FORMAT_STRING:
                        next_token();
                        return new UnaryOperatorNode(operator_token, get_operand_node());
                    default:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_OPERATOR, operator_token.Position, "Expected Unary Operator");
                }
            case SymbolToken symbol_token:
                switch (symbol_token.Symbol) {
                    case SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET:
                        next_token();
                        Node expression = get_expression();
                        if (CurrentToken is DataToken) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected operator");
                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, CurrentToken.Position, "Expected ')'");
                        next_token();
                        return expression;
                    case SymbolToken.SYMBOL.LEFT_CURLY_BRACKET: {
                        return new DataNode(DataNode.TYPE.DICTIONARY, get_bracket_node(symbol_token.Symbol, "{", "}", delegate {
                            if (CurrentToken is not DataToken) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected data key");
                            if (CurrentToken.is_data(DataToken.TYPE.NULL)) throw new Error(CurrentToken.Position, "Key can not be null");
                            Token key = CurrentToken;
                            next_token();
                            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.COLON)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected ':'");
                            next_token();
                            return new DataNode(DataNode.TYPE.DICTIONARY, new Dictionary<string, object>(){{"Token", key}, {"Node", get_operand_node()}}, key.Position + PreviousToken.Position);
                        }, out TokenPosition position), position);
                    }
                    case SymbolToken.SYMBOL.LEFT_SQUARE_BRACKET: {
                        return new DataNode(DataNode.TYPE.LIST, get_bracket_node(symbol_token.Symbol, "[", "]", () => get_expression(), out TokenPosition position), position);
                    }
                    default:
                        throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Operand");
                }
            default:
                throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Operand");
        }
    }
    private Node get_datatype_node() {
        Node datatype;
        if (CurrentToken.is_keyword_type(KeywordToken.TYPE.DATATYPE)) {
            datatype = KeywordNode.FromToken(CurrentToken as KeywordToken);
            next_token();
        } else if (CurrentToken.is_data(DataToken.TYPE.WORD) && NewDatatypes.Contains((CurrentToken as DataToken).Data as string)) {
            next_token();
            datatype = get_dot_operation(DataNode.FromToken(PreviousToken as DataToken));
        } else {
            throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected datatype");
        }
        return datatype;
    }
    private DataToken get_identifier_token() {
        if (!CurrentToken.is_data(DataToken.TYPE.WORD)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
        if (NewDatatypes.Contains((CurrentToken as DataToken).Data)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected Identifier");
        next_token();
        return PreviousToken as DataToken;
    }
    private FunctionParameterNode get_function_parameter() {
        Node possible_identifier = get_expression();
        if (possible_identifier is DataNode && (possible_identifier as DataNode).Type == DataNode.TYPE.IDENTIFIER) {
            if (CurrentToken.is_symbol(SymbolToken.SYMBOL.COLON)) {
                next_token();
                return new FunctionParameterNode.Classified(possible_identifier, get_expression(), possible_identifier.Position + PreviousToken.Position);
            }
        } else {
            if (CurrentToken.is_symbol(SymbolToken.SYMBOL.COLON)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
        }
        return new FunctionParameterNode(possible_identifier, possible_identifier.Position + PreviousToken.Position);
    }
    private FunctionArgumentNode get_function_argument() {
        bool is_classified = false;
        if (CurrentToken.is_keyword(KeywordToken.KEYWORD.CLASSIFIED)) {
            is_classified = true;
            next_token();
        }
        Node datatype = get_datatype_node();
        DataToken identifier = get_identifier_token();
        Node value = null;
        if (CurrentToken is OperatorToken) {
            if (!CurrentToken.is_operator(OperatorToken.OPERATOR.ASSIGN)) throw new ParserError(ParserError.TYPE.UNEXPECTED_OPERATOR, CurrentToken.Position, "Expected '='");
            next_token();
            value = get_expression();
        }
        return new FunctionArgumentNode(is_classified, datatype, identifier, value, datatype.Position + PreviousToken.Position);
    }
    private Node get_definition_node(int allowed_structures) {
        KeywordToken access_mode_modifier = null;
        KeywordToken static_modifier = null;
        KeywordToken constant_modifier = null;
        KeywordToken reference_modifier = null;
        while (CurrentToken.is_keyword_type(KeywordToken.TYPE.MODIFIER, out KeywordToken modifier)) {
            switch (modifier.Keyword) {
                case KeywordToken.KEYWORD.PRIVATE:
                    if (!STRUCTURES.IN(STRUCTURES.MODIFIER, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    if (access_mode_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    access_mode_modifier = modifier;
                    break;
                case KeywordToken.KEYWORD.PUBLIC:
                    if (!STRUCTURES.IN(STRUCTURES.MODIFIER, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    if (access_mode_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    access_mode_modifier = modifier;
                    break;
                case KeywordToken.KEYWORD.STATIC:
                    if (!STRUCTURES.IN(STRUCTURES.MODIFIER, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    if (static_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    static_modifier = modifier;
                    break;
                case KeywordToken.KEYWORD.CONSTANT:
                    if (!STRUCTURES.IN(STRUCTURES.MODIFIER, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    if (constant_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    constant_modifier = modifier;
                    break;
                case KeywordToken.KEYWORD.REFERENCE:
                    if (!STRUCTURES.IN(STRUCTURES.MODIFIER, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    if (reference_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
                    reference_modifier = modifier;
                    break;
                default:
                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, modifier.Position);
            }
            next_token();
        }
        if(!CurrentToken.is_keyword_type(KeywordToken.TYPE.DEFINITION, out KeywordToken definition)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "class, function or variable");
        next_token();
        if (definition.Keyword == KeywordToken.KEYWORD.CONSTRUCTOR) {
            if (!STRUCTURES.IN(STRUCTURES.CONSTRUCTOR, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
            if (constant_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, constant_modifier.Position);
            if (reference_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, reference_modifier.Position);
            bool unlimited_arguments = false;
            if (CurrentToken.is_keyword(KeywordToken.KEYWORD.UNLIMITED)) {
                unlimited_arguments = true;
                next_token();
            }
            List<Node> arguments = null;
            if (unlimited_arguments) {
                if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                next_token();
                arguments = new List<Node>() { get_function_argument() };
                if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                next_token();
            } else {
                arguments = get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", get_function_argument, out _);
            }
            List<Node> base_parameters = null;
            if (CurrentToken.is_symbol(SymbolToken.SYMBOL.COLON)) {
                next_token();
                base_parameters = get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_expression(), out _);
            }
            return new ConstructorNode(access_mode_modifier, static_modifier, unlimited_arguments, arguments, base_parameters, get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.STATEMENT), definition.Position + PreviousToken.Position);
        }
        switch (definition.Keyword) {
            case KeywordToken.KEYWORD.VARIABLE: {
                if (!STRUCTURES.IN(STRUCTURES.VARIABLE, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                Node datatype = get_datatype_node();
                if (datatype is KeywordNode && (datatype as KeywordNode).Keyword == KeywordToken.KEYWORD.VOID) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Unexpected datatype");
                DataToken identifier = get_identifier_token();
                Node value = null;
                if (CurrentToken is OperatorToken) {
                    if (!CurrentToken.is_operator(OperatorToken.OPERATOR.ASSIGN)) throw new ParserError(ParserError.TYPE.UNEXPECTED_OPERATOR, CurrentToken.Position, "Expected '='");
                    next_token();
                    value = get_expression();
                }
                return new VariableDefinitionNode(access_mode_modifier, static_modifier, constant_modifier, reference_modifier, datatype, identifier, value, definition.Position + PreviousToken.Position);
            }
            case KeywordToken.KEYWORD.FUNCTION: {
                if (!STRUCTURES.IN(STRUCTURES.FUNCTION, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                if (constant_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, constant_modifier.Position);
                if (reference_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, reference_modifier.Position);
                bool unlimited_arguments = false;
                if (CurrentToken.is_keyword(KeywordToken.KEYWORD.UNLIMITED)) {
                    unlimited_arguments = true;
                    next_token();
                }
                Node datatype = get_datatype_node();
                DataToken identifier = get_identifier_token();
                List<Node> arguments = null;
                if (unlimited_arguments) {
                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                    next_token();
                    arguments = new List<Node>() { get_function_argument() };
                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                    next_token();
                } else {
                    arguments = get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", get_function_argument, out _);
                }
                return new FunctionDefinitionNode(access_mode_modifier, static_modifier, unlimited_arguments, datatype, identifier, arguments, get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.RETURN_STATEMENT + STRUCTURES.STATEMENT), definition.Position + PreviousToken.Position);
            }
            case KeywordToken.KEYWORD.CLASS: {
                if (!STRUCTURES.IN(STRUCTURES.CLASS, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                if (constant_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, constant_modifier.Position);
                if (reference_modifier != null) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, reference_modifier.Position);
                Node datatype = null;
                if (CurrentToken.is_keyword_type(KeywordToken.TYPE.DATATYPE)) {
                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                } else if (CurrentToken.is_data(DataToken.TYPE.WORD) && NewDatatypes.Contains((CurrentToken as DataToken).Data as string)) {
                    next_token();
                    datatype = get_dot_operation(DataNode.FromToken(PreviousToken as DataToken));
                }
                DataToken identifier = get_identifier_token();
                add_new_datatype(identifier);
                return new ClassDefinitionNode(access_mode_modifier, static_modifier, datatype, identifier, get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.FUNCTION + STRUCTURES.CLASS + STRUCTURES.CONSTRUCTOR + STRUCTURES.MODIFIER), definition.Position + PreviousToken.Position);
            }
            default:
                throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
        }
    }
    private OperandInstructionNode get_operand_instruction_node(int allowed_structures) {
        if (CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET, out SymbolToken symbol_token)) {
            next_token();
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
            List<Node> instructions = get_nodes(allowed_structures, SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET);
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, symbol_token.Position + CurrentToken.Position, "Expected '}'");
            next_token();
            return new OperandInstructionNode(instructions, false, symbol_token.Position + PreviousToken.Position);
        } else if (CurrentToken.is_operator(OperatorToken.OPERATOR.EXECUTES, out OperatorToken operator_token)) {
            next_token();
            return new OperandInstructionNode(new List<Node>{ get_expression() }, true, operator_token.Position + PreviousToken.Position);
        } else {
            throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '{' or '=>'");
        }
    }
    private IFNode get_if_node(KeywordToken keyword_token) {
        next_token();
        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
        Node if_expression = get_operand_node();
        OperandInstructionNode implication = get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.FLOW_CONTROL + STRUCTURES.STATEMENT + STRUCTURES.RETURN_STATEMENT);
        List<IFNode.ElseIFNode> children = new List<IFNode.ElseIFNode>();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        while (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE_IF)) {
            KeywordToken else_if_token = CurrentToken as KeywordToken;
            next_token();
            if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
            children.Add(new IFNode.ElseIFNode(get_operand_node(), get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.FLOW_CONTROL + STRUCTURES.STATEMENT + STRUCTURES.RETURN_STATEMENT), else_if_token.Position + PreviousToken.Position));
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        }
        OperandInstructionNode inverse = null;
        if (CurrentToken.is_keyword(KeywordToken.KEYWORD.ELSE)) {
            next_token();
            inverse = get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.FLOW_CONTROL + STRUCTURES.STATEMENT + STRUCTURES.RETURN_STATEMENT);
        }
        return new IFNode(if_expression, implication, children, inverse, keyword_token.Position + PreviousToken.Position);
    }
    private List<Node> get_nodes(int allowed_structures, SymbolToken.SYMBOL breaker = SymbolToken.SYMBOL.END) {
        List<Node> nodes = new List<Node>();
        while (!(CurrentToken.is_symbol(SymbolToken.SYMBOL.END) || CurrentToken.is_symbol(breaker))) {
            switch (CurrentToken) {
                case KeywordToken keyword_token:
                    switch (keyword_token.Type) {
                        case KeywordToken.TYPE.DECISION: {
                            if (!STRUCTURES.IN(STRUCTURES.STATEMENT, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.ELSE_IF: case KeywordToken.KEYWORD.ELSE:
                                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected if");
                                case KeywordToken.KEYWORD.CASE: case KeywordToken.KEYWORD.DEFAULT:
                                    throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected match");
                                case KeywordToken.KEYWORD.MATCH: {
                                    throw new ParserError(ParserError.TYPE.UNIMPLEMENTED_TOKEN, CurrentToken.Position);
                                    /*
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                    Node expression = get_operand_node();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '{'");
                                    next_token();
                                    List<MatchNode.CaseNode> cases = new List<MatchNode.CaseNode>();
                                    while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                                    if (!CurrentToken.is_keyword(KeywordToken.KEYWORD.CASE)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected case");
                                    while (CurrentToken.is_keyword(KeywordToken.KEYWORD.CASE)) {
                                        TokenPosition case_token_position = CurrentToken.Position;
                                        next_token();
                                        if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                        DataNode case_expression = new DataNode(DataNode.TYPE.LIST, get_bracket_node(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET, "(", ")", () => get_expression(), out TokenPosition position), position);
                                        if ((case_expression.Data as List<Node>).Count == 0) throw new ParserError(ParserError.TYPE.EMPTHY_CASE, case_token_position);
                                        cases.Add(new MatchNode.CaseNode(case_expression, get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.FLOW_CONTROL + STRUCTURES.STATEMENT + STRUCTURES.RETURN_STATEMENT), case_token_position + PreviousToken.Position));
                                        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                                    }
                                    OperandInstructionNode default_node = null;
                                    if (CurrentToken.is_keyword(KeywordToken.KEYWORD.DEFAULT)) {
                                        next_token();
                                        default_node = get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.FLOW_CONTROL + STRUCTURES.STATEMENT + STRUCTURES.RETURN_STATEMENT);
                                        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
                                    }
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CURLY_BRACKET)) throw new ParserError(ParserError.TYPE.UNCLOSED_BRACKETS, keyword_token.Position + PreviousToken.Position, "Expected '{'");
                                    next_token();
                                    nodes.Add(new MatchNode(expression, cases, default_node, keyword_token.Position + PreviousToken.Position));
                                    break;*/
                                }
                                case KeywordToken.KEYWORD.IF: {
                                    nodes.Add(get_if_node(keyword_token));
                                    break;
                                }
                            }
                            break;
                        }
                        case KeywordToken.TYPE.FLOWCONTROL: {
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.RETURN:
                                    if (!STRUCTURES.IN(STRUCTURES.RETURN_STATEMENT, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                    next_token();
                                    nodes.Add(new FlowControlNode(get_expression(), keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.BREAK:
                                    if (!STRUCTURES.IN(STRUCTURES.FLOW_CONTROL, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.BREAK, keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.CONTINUE:
                                    if (!STRUCTURES.IN(STRUCTURES.FLOW_CONTROL, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.CONTINUE, keyword_token.Position + PreviousToken.Position));
                                    break;
                                case KeywordToken.KEYWORD.BREAKPOINT:
                                    if (!STRUCTURES.IN(STRUCTURES.FLOW_CONTROL, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                                    next_token();
                                    nodes.Add(new FlowControlNode(FlowController.TYPE.BREAKPOINT, keyword_token.Position + PreviousToken.Position));
                                    break;

                            }
                            break;
                        }
                        case KeywordToken.TYPE.LOOP: {
                            if (!STRUCTURES.IN(STRUCTURES.STATEMENT, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                            switch (keyword_token.Keyword) {
                                case KeywordToken.KEYWORD.FOR: {
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                    next_token();
                                    DataToken iterator1 = get_identifier_token();
                                    if (!CurrentToken.is_operator(OperatorToken.OPERATOR.IN))  throw new ParserError(ParserError.TYPE.EXPECTED_OPERATOR, CurrentToken.Position, "'in'");
                                    OperatorToken In = CurrentToken as OperatorToken;
                                    next_token();
                                    Node iteratable = get_expression();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.RIGHT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected ')'");
                                    next_token();
                                    OperandInstructionNode instructions = get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.FLOW_CONTROL + STRUCTURES.STATEMENT + STRUCTURES.RETURN_STATEMENT);
                                    nodes.Add(new ForNode(iterator1, iteratable, instructions, keyword_token.Position + PreviousToken.Position));
                                    break;
                                }
                                case KeywordToken.KEYWORD.WHILE: {
                                    next_token();
                                    if (!CurrentToken.is_symbol(SymbolToken.SYMBOL.LEFT_CIRCLE_BRACKET)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position, "Expected '('");
                                    Node expression = get_operand_node();
                                    OperandInstructionNode instructions = get_operand_instruction_node(STRUCTURES.VARIABLE + STRUCTURES.FLOW_CONTROL + STRUCTURES.STATEMENT + STRUCTURES.RETURN_STATEMENT);
                                    nodes.Add(new WhileNode(expression, instructions, keyword_token.Position + PreviousToken.Position));
                                    break;
                                }
                            }
                            break;
                        }
                        case KeywordToken.TYPE.MODIFIER: case KeywordToken.TYPE.DEFINITION:
                            nodes.Add(get_definition_node(allowed_structures));
                            break;
                        default:
                            if (!STRUCTURES.IN(STRUCTURES.STATEMENT, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                            nodes.Add(get_expression());
                            break;
                    }
                    break;
                default:
                    if (!STRUCTURES.IN(STRUCTURES.STATEMENT, allowed_structures)) throw new ParserError(ParserError.TYPE.UNEXPECTED_TOKEN, CurrentToken.Position);
                    nodes.Add(get_expression());
                    break;
            }
            while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        }
        return nodes;
    }
    public List<Node> Parse(List<Token> tokens) {
        Tokens = tokens;
        Index = 0;
        CurrentToken = tokens[0];
        ClearDatatypes(NewDatatypes);
        NewDatatypes.Clear();
        while (CurrentToken.is_symbol(SymbolToken.SYMBOL.END_OF_LINE)) next_token();
        return get_nodes(STRUCTURES.VARIABLE + STRUCTURES.FUNCTION + STRUCTURES.CLASS + STRUCTURES.STATEMENT);
    }
}