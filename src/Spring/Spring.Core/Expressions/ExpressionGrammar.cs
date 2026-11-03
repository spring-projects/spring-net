using System.Text;
using Spring.Context.Support;
using static Spring.Expressions.Parser.ExpressionParserTokenTypes;

namespace Spring.Expressions;

/// <summary>
/// Parses the expression language into a tree of <see cref="SpringAST"/> nodes.
/// </summary>
/// <remarks>
/// The grammar is in <c>ExpressionGrammar.parlot.cs</c>. That file is only read at build time by the Parlot source
/// generator, which emits <see cref="TryParse"/> into this assembly, so that there is no runtime dependency on a parsing
/// library. This file has its tokens, the tree it builds and its syntax errors.
/// </remarks>
internal static partial class ExpressionGrammar
{
    internal enum Rule
    {
        Expression,
        Primary,
        Lambda
    }

    private const string IdStart = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
    private const string IdPart = IdStart + "0123456789";
    private const string NumberStart = "0123456789.";

    private const char EndOfText = '\uFFFF';

    // The parser fails at the limit on nesting, or when the stack of the thread ends. A small stack can end at this depth
    private const int SuspiciousDepth = 32;

    private static partial bool TryParse(string input, Rule rule, out BaseNode value, out int failure);

    /// <summary>Parses a whole expression.</summary>
    public static BaseNode ParseExpression(string input) => Parse(input, Rule.Expression);

    /// <summary>Parses the lambda expression at the start of the input, whatever follows it.</summary>
    public static LambdaExpressionNode ParseLambda(string input) => (LambdaExpressionNode) Parse(input, Rule.Lambda);

    /// <summary>
    /// Parses the primary expression at the start of the input and stops at the first token that can't continue it, which
    /// has to be a token that can follow a primary expression.
    /// </summary>
    public static BaseNode ParsePrimary(string input) => Parse(input, Rule.Primary);

    /// <summary>Parses the property name at the start of the input, whatever follows it.</summary>
    public static BaseNode ParseProperty(string input)
    {
        var start = SkipSpaces(input, 0);
        var length = ScanId(input.AsSpan(start));
        if (length == 0)
        {
            throw CreateSyntaxError(input, start, "ID", mayBeTooDeep: false);
        }

        return WithType(new PropertyOrFieldNode(), ID, input.Substring(start, length));
    }

    private static BaseNode Parse(string input, Rule rule)
    {
        if (TryParse(input, rule, out var node, out var failure))
        {
            return node;
        }

        var offset = SkipSpaces(input.AsSpan(), failure);
        ReadTokensBefore(input, offset, out var depth, out var afterOperand);

        // The parser doesn't return why it failed: at this depth it can be the limit on nesting
        var expected = rule == Rule.Lambda && offset == SkipSpaces(input.AsSpan(), 0) ? "LAMBDA"
            : rule == Rule.Expression && depth == 0 && afterOperand ? "EOF"
            : null;

        throw CreateSyntaxError(input, offset, expected, depth >= SuspiciousDepth);
    }

    // Tokens

    private static bool IsIdStart(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

    private static bool IsDigit(char c) => c >= '0' && c <= '9';

    private static bool IsIdPart(char c) => IsIdStart(c) || IsDigit(c);

    private static bool IsHexDigit(char c) => IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

    private static bool IsWhiteSpace(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';

    private static char CharAt(ReadOnlySpan<char> span, int index) => index < span.Length ? span[index] : EndOfText;

    private static int SkipSpaces(ReadOnlySpan<char> span, int offset)
    {
        while (offset < span.Length && IsWhiteSpace(span[offset]))
        {
            offset++;
        }

        return offset;
    }

    /// <summary>The token of this type at the start of the text, which isn't the start of a longer one.</summary>
    private static int ScanToken(ReadOnlySpan<char> span, int expected)
    {
        if (span.IsEmpty)
        {
            return 0;
        }

        var length = NextToken(span, out var type);
        return type == expected ? length : 0;
    }

    private static int ScanLeftCurly(ReadOnlySpan<char> span) => ScanToken(span, LCURLY);

    private static int ScanAssign(ReadOnlySpan<char> span) => ScanToken(span, ASSIGN);

    private static int ScanDollar(ReadOnlySpan<char> span) => ScanToken(span, DOLLAR);

    private static int ScanPower(ReadOnlySpan<char> span) => ScanToken(span, POWER);

    private static int ScanDot(ReadOnlySpan<char> span) => CharAt(span, 0) == '.' && !IsDigit(CharAt(span, 1)) ? 1 : 0;

    private static int ScanBang(ReadOnlySpan<char> span) => ScanToken(span, BANG);

    private static int ScanQuestion(ReadOnlySpan<char> span) => ScanToken(span, QMARK);

    private static int ScanWord(ReadOnlySpan<char> span)
    {
        if (!IsIdStart(CharAt(span, 0)))
        {
            return 0;
        }

        var size = 1;
        while (size < span.Length && IsIdPart(span[size]))
        {
            size++;
        }

        return size;
    }

    /// <summary>Returns the type of the keyword <paramref name="word"/> is, or <c>ID</c>.</summary>
    private static int WordType(ReadOnlySpan<char> word)
    {
        static bool Is(ReadOnlySpan<char> word, string keyword) => word.SequenceEqual(keyword.AsSpan());

        switch (word.Length)
        {
            case 2:
                return Is(word, "or") ? OR : Is(word, "in") ? IN : Is(word, "is") ? IS : ID;
            case 3:
                return Is(word, "and") ? AND : Is(word, "xor") ? XOR : Is(word, "new") ? LITERAL_new : ID;
            case 4:
                return Is(word, "true") ? TRUE : Is(word, "like") ? LIKE : Is(word, "null") ? NULL_LITERAL : ID;
            case 5:
                return Is(word, "false") ? FALSE : ID;
            case 7:
                return Is(word, "between") ? BETWEEN : Is(word, "matches") ? MATCHES : ID;
            default:
                return ID;
        }
    }

    /// <summary>An identifier: neither a keyword nor the <c>T(</c> of a type.</summary>
    private static int ScanId(ReadOnlySpan<char> span)
    {
        var size = ScanWord(span);
        if (size == 0 || WordType(span.Slice(0, size)) != ID || (size == 1 && CharAt(span, 0) == 'T' && CharAt(span, 1) == '('))
        {
            return 0;
        }

        return size;
    }

    /// <summary><c>ID (DOT ID)*</c>, with any white space between the tokens.</summary>
    private static int ScanQualifiedId(ReadOnlySpan<char> span)
    {
        var size = ScanId(span);
        while (size > 0)
        {
            var dot = SkipSpaces(span, size);
            if (CharAt(span, dot) != '.' || IsDigit(CharAt(span, dot + 1)))
            {
                break;
            }

            var next = SkipSpaces(span, dot + 1);
            var length = next < span.Length ? ScanId(span.Slice(next)) : 0;
            if (length == 0)
            {
                break;
            }

            size = next + length;
        }

        return size;
    }

    /// <summary>
    /// A type or object name: an identifier, then any tokens but ')', ':' and a lone quote. It stops before anything that
    /// isn't a token, which then fails in the parser that follows.
    /// </summary>
    private static int ScanName(ReadOnlySpan<char> span)
    {
        var size = ScanId(span);
        while (size > 0)
        {
            var next = SkipSpaces(span, size);
            if (next == span.Length)
            {
                break;
            }

            var length = NextToken(span.Slice(next), out var type);
            if (length == 0 || type == RPAREN || type == COLON || type == QUOTE)
            {
                break;
            }

            size = next + length;
        }

        return size;
    }

    private static int ScanString(ReadOnlySpan<char> span)
    {
        if (CharAt(span, 0) != '\'')
        {
            return 0;
        }

        for (var i = 1; i < span.Length; i++)
        {
            if (span[i] == '\'')
            {
                if (CharAt(span, i + 1) != '\'')
                {
                    return i + 1;
                }

                i++;
            }
        }

        return 0;
    }

    private static int SkipDigits(ReadOnlySpan<char> span, int offset)
    {
        while (offset < span.Length && IsDigit(span[offset]))
        {
            offset++;
        }

        return offset;
    }

    /// <summary>
    /// The exponent and suffix of a real number. After a fraction 'e' has to start an exponent: there is no token then.
    /// </summary>
    private static int SkipRealEnd(ReadOnlySpan<char> span, int offset, bool isFraction)
    {
        var c = CharAt(span, offset);
        if (c == 'e' || c == 'E')
        {
            var digits = offset + 1;
            while (CharAt(span, digits) == '+' || CharAt(span, digits) == '-')
            {
                digits++;
            }

            if (IsDigit(CharAt(span, digits)))
            {
                offset = SkipDigits(span, digits);
                c = CharAt(span, offset);
            }
            else if (isFraction)
            {
                return 0;
            }
        }

        return IsRealSuffix(c) ? offset + 1 : offset;
    }

    private static bool IsRealSuffix(char c) => c == 'F' || c == 'f' || c == 'D' || c == 'd' || c == 'M' || c == 'm';

    private static int SkipIntegerSuffix(ReadOnlySpan<char> span, int offset)
    {
        var first = CharAt(span, offset);
        var second = CharAt(span, offset + 1);
        switch (first)
        {
            case 'U':
                return second == 'L' ? offset + 2 : offset + 1;
            case 'L':
                return second == 'U' ? offset + 2 : offset + 1;
            case 'u':
                return second == 'l' || second == 'L' ? offset + 2 : offset + 1;
            case 'l':
                return second == 'u' || second == 'U' ? offset + 2 : offset + 1;
            default:
                return offset;
        }
    }

    /// <summary>A decimal integer, a hexadecimal integer or a real number.</summary>
    private static int ScanNumber(ReadOnlySpan<char> span)
    {
        if (CharAt(span, 0) == '.')
        {
            return IsDigit(CharAt(span, 1)) ? SkipRealEnd(span, SkipDigits(span, 1), isFraction: true) : 0;
        }

        if (!IsDigit(CharAt(span, 0)))
        {
            return 0;
        }

        if (CharAt(span, 0) == '0' && CharAt(span, 1) == 'x')
        {
            var hex = 2;
            while (IsHexDigit(CharAt(span, hex)))
            {
                hex++;
            }

            return hex == 2 ? 0 : SkipIntegerSuffix(span, hex);
        }

        var size = SkipDigits(span, 1);
        if (CharAt(span, size) == '.' && IsDigit(CharAt(span, size + 1)))
        {
            return SkipRealEnd(span, SkipDigits(span, size + 1), isFraction: true);
        }

        var end = SkipRealEnd(span, size, isFraction: false);
        return end != size ? end : SkipIntegerSuffix(span, size);
    }

    private static int NumberType(ReadOnlySpan<char> number)
    {
        if (number.Length > 1 && number[0] == '0' && number[1] == 'x')
        {
            return HEXADECIMAL_INTEGER_LITERAL;
        }

        foreach (var c in number)
        {
            if (c == '.' || c == 'e' || c == 'E' || IsRealSuffix(c))
            {
                return REAL_LITERAL;
            }
        }

        return INTEGER_LITERAL;
    }

    /// <summary>
    /// Reads the token at the start of <paramref name="span"/>, which isn't empty and doesn't start with white space, and
    /// returns its length, or 0 when the text is not a token.
    /// </summary>
    private static int NextToken(ReadOnlySpan<char> span, out int type)
    {
        var next = CharAt(span, 1);
        switch (span[0])
        {
            case '@': type = AT; return 1;
            case '`': type = BACKTICK; return 1;
            case '|': type = PIPE; return 1;
            case '#': type = POUND; return 1;
            case '(': type = LPAREN; return 1;
            case ')': type = RPAREN; return 1;
            case '[': type = LBRACKET; return 1;
            case ']': type = RBRACKET; return 1;
            case '}': type = RCURLY; return 1;
            case ',': type = COMMA; return 1;
            case ';': type = SEMI; return 1;
            case ':': type = COLON; return 1;
            case '+': type = PLUS; return 1;
            case '-': type = MINUS; return 1;
            case '/': type = DIV; return 1;
            case '*': type = STAR; return 1;
            case '%': type = MOD; return 1;
            case '?':
                type = next == '?' ? DEFAULT : next == '{' ? SELECT : QMARK;
                return type == QMARK ? 1 : 2;
            case '=':
                type = next == '=' ? EQUAL : ASSIGN;
                return type == ASSIGN ? 1 : 2;
            case '!':
                type = next == '=' ? NOT_EQUAL : next == '{' ? PROJECT : BANG;
                return type == BANG ? 1 : 2;
            case '<':
                type = next == '=' ? LESS_THAN_OR_EQUAL : LESS_THAN;
                return type == LESS_THAN ? 1 : 2;
            case '>':
                type = next == '=' ? GREATER_THAN_OR_EQUAL : GREATER_THAN;
                return type == GREATER_THAN ? 1 : 2;
            case '^':
                type = next == '{' ? SELECT_FIRST : POWER;
                return type == POWER ? 1 : 2;
            case '$':
                type = next == '{' ? SELECT_LAST : DOLLAR;
                return type == DOLLAR ? 1 : 2;
            case '{':
                type = next == '|' ? LAMBDA : LCURLY;
                return type == LCURLY ? 1 : 2;
            case '\\':
                type = next == '.' ? DOT_ESCAPED : BACKSLASH;
                return type == BACKSLASH ? 1 : 2;
            case '\'':
                type = span.Length == 1 ? QUOTE : STRING_LITERAL;
                return span.Length == 1 ? 1 : ScanString(span);
            case 'T' when next == '(':
                type = TYPE;
                return 2;
        }

        var size = ScanWord(span);
        if (size > 0)
        {
            type = WordType(span.Slice(0, size));
            return size;
        }

        size = ScanNumber(span);
        if (size > 0)
        {
            type = NumberType(span.Slice(0, size));
            return size;
        }

        // A dot followed by a digit starts a number, even if it is not a valid one
        var isDot = span[0] == '.' && !IsDigit(next);
        type = isDot ? DOT : 0;
        return isDot ? 1 : 0;
    }

    /// <summary>The text of the tokens of a name, without the white space between them and the quotes of its strings.</summary>
    private static string TokensText(string text)
    {
        if (text.IndexOfAny(TokensTextChars) < 0)
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        var offset = 0;
        while ((offset = SkipSpaces(text.AsSpan(), offset)) < text.Length)
        {
            var length = NextToken(text.AsSpan(offset), out var type);
            var token = text.Substring(offset, length);
            builder.Append(type == STRING_LITERAL ? StringText(token) : token);
            offset += length;
        }

        return builder.ToString();
    }

    private static readonly char[] TokensTextChars = { ' ', '\t', '\n', '\r', '\'' };

    private static string StringText(string token)
    {
        var text = token.Substring(1, token.Length - 2);
        return text.IndexOf('\'') < 0 ? text : text.Replace("''", "'");
    }

    /// <summary>
    /// Whether what follows a primary expression is acceptable: the rule that parses it alone doesn't read until the end
    /// of the input, but it does look at the next token to choose between its alternatives.
    /// </summary>
    private static bool CanFollowPrimary(BaseNode node, string input, int offset)
    {
        var next = SkipSpaces(input.AsSpan(), offset);
        if (next == input.Length)
        {
            return true;
        }

        if (NextToken(input.AsSpan(next), out var type) == 0 || Contains(NodeStarts, type))
        {
            // Not a token, or a node that failed: the primary expression stopped before it
            return false;
        }

        if (input[offset - 1] == '.')
        {
            return true;
        }

        var last = node.getFirstChild();
        if (last.getNextSibling() == null)
        {
            return Contains(TokensAfterStartNode, type);
        }

        while (last.getNextSibling() != null)
        {
            last = last.getNextSibling();
        }

        return !(last is PropertyOrFieldNode) || Contains(TokensAfterName, type);
    }

    private static bool Contains(long tokens, int type) => type < 64 && (tokens & (1L << type)) != 0;

    // The tokens a node of a primary expression starts with
    private const long NodeStarts =
        (1L << LPAREN) | (1L << DOT) | (1L << ID) | (1L << LBRACKET) | (1L << PROJECT) | (1L << SELECT) | (1L << SELECT_FIRST) | (1L << SELECT_LAST);

    // The tokens that can follow a primary expression without any further node, in the grammar
    private const long TokensAfterStartNode =
        (1L << EOF) | (1L << AND) | (1L << OR) | (1L << XOR) | (1L << IN) | (1L << IS) | (1L << BETWEEN) | (1L << LIKE) | (1L << MATCHES)
        | (1L << SEMI) | (1L << RPAREN) | (1L << ASSIGN) | (1L << DEFAULT) | (1L << QMARK) | (1L << COLON) | (1L << PLUS) | (1L << MINUS)
        | (1L << STAR) | (1L << DIV) | (1L << MOD) | (1L << POWER) | (1L << COMMA) | (1L << RBRACKET) | (1L << RCURLY) | (1L << EQUAL)
        | (1L << NOT_EQUAL) | (1L << LESS_THAN) | (1L << LESS_THAN_OR_EQUAL) | (1L << GREATER_THAN) | (1L << GREATER_THAN_OR_EQUAL);

    // The tokens that can follow a name, in the grammar
    private const long TokensAfterName = TokensAfterStartNode | NodeStarts;

    // The tree

    private static BaseNode WithType(BaseNode node, int type, string text)
    {
        node.Type = type;
        node.setText(text);
        return node;
    }

    private static SpringAST TokenNode(int type, string text)
    {
        var node = new SpringAST();
        node.Type = type;
        node.setText(text);
        return node;
    }

    private static BaseNode WithText(BaseNode node, string text)
    {
        node.setText(text);
        return node;
    }

    private static void SetChildren(SpringAST node, IReadOnlyList<BaseNode> children)
    {
        if (children == null || children.Count == 0)
        {
            return;
        }

        SpringAST previous = children[0];
        node.setFirstChild(previous);
        for (var i = 1; i < children.Count; i++)
        {
            SpringAST child = children[i];
            previous.setNextSibling(child);
            previous = child;
        }
    }

    private static BaseNode Node(BaseNode node, int type, string text, IReadOnlyList<BaseNode> children)
    {
        SetChildren(node, children);
        return WithType(node, type, text);
    }

    private static BaseNode Operator(BaseNode node, int type, string text, BaseNode child)
    {
        node.setFirstChild(child);
        return WithType(node, type, text);
    }

    private static BaseNode Operator(BaseNode node, int type, string text, BaseNode first, BaseNode second) =>
        WithChildren(WithType(node, type, text), first, second);

    private static BaseNode WithChildren(BaseNode node, BaseNode first, BaseNode second)
    {
        first.setNextSibling(second);
        node.setFirstChild(first);
        return node;
    }

    private static BaseNode NumberLiteral(string text)
    {
        var type = NumberType(text.AsSpan());
        BaseNode node = type == HEXADECIMAL_INTEGER_LITERAL ? new HexLiteralNode() : type == REAL_LITERAL ? new RealLiteralNode() : new IntLiteralNode();
        return WithType(node, type, text);
    }

    /// <summary>Makes <paramref name="child"/> the first child of <paramref name="node"/>, before those it has.</summary>
    private static BaseNode PrependChild(BaseNode node, BaseNode child)
    {
        child.setNextSibling(node.getFirstChild());
        node.setFirstChild(child);
        return node;
    }

    // While a primary expression is read its first child is the last one, whose next sibling is the first one, so that a
    // long path doesn't walk its nodes for each one
    private static BaseNode Primary(BaseNode startNode)
    {
        startNode.setNextSibling(startNode);
        return Operator(new Expression(), EXPR, "expression", startNode);
    }

    private static BaseNode AppendToPrimary(BaseNode primary, BaseNode child)
    {
        var last = primary.getFirstChild();
        child.setNextSibling(last.getNextSibling());
        last.setNextSibling(child);
        primary.setFirstChild(child);
        return primary;
    }

    private static BaseNode FinishPrimary(BaseNode primary)
    {
        var last = primary.getFirstChild();
        primary.setFirstChild(last.getNextSibling());
        last.setNextSibling(null);
        return primary;
    }

    private static BaseNode MethodOrProperty(string name, IReadOnlyList<BaseNode> arguments) =>
        arguments == null ? WithType(new PropertyOrFieldNode(), ID, name) : Node(new MethodNode(), ID, name, arguments);

    private static BaseNode FunctionOrVariable(string name, IReadOnlyList<BaseNode> arguments) =>
        arguments == null ? WithType(new VariableNode(), ID, name) : Node(new FunctionNode(), ID, name, arguments);

    private static BaseNode LocalFunctionOrVariable(string name, IReadOnlyList<BaseNode> arguments) =>
        arguments == null ? WithType(new LocalVariableNode(), ID, name) : Node(new LocalFunctionNode(), ID, name, arguments);

    private static BaseNode MapEntry(BaseNode key, BaseNode value) => Operator(new MapEntryNode(), EXPR, "entry", key, value);

    private static BaseNode QuotableName(string name, int type) => WithType(new QualifiedIdentifier(), type, name);

    private static BaseNode Reference(BaseNode first, BaseNode second) =>
        second == null ? Operator(new ReferenceNode(), EXPR, "ref", first) : Operator(new ReferenceNode(), EXPR, "ref", first, second);

    private static BaseNode NamedArgument(string name, BaseNode value) => Operator(new NamedArgumentNode(), ID, name, value);

    private static BaseNode ExpressionList(BaseNode first, IReadOnlyList<BaseNode> others)
    {
        var previous = first;
        foreach (var other in others)
        {
            previous.setNextSibling(other);
            previous = other;
        }

        return Operator(new ExpressionListNode(), EXPR, "expressionList", first);
    }

    private static BaseNode ArrayConstructor(IReadOnlyList<BaseNode> rank, BaseNode initializer)
    {
        var rankNode = TokenNode(LBRACKET, "[");
        SetChildren(rankNode, rank);

        var node = WithType(new ArrayConstructorNode(), EXPR, null);
        node.setFirstChild(rankNode);
        rankNode.setNextSibling(initializer);
        return node;
    }

    private static BaseNode Lambda(IReadOnlyList<string> arguments, BaseNode body)
    {
        var node = WithType(new LambdaExpressionNode(), EXPR, "lambda");

        if (arguments.Count == 0)
        {
            node.setFirstChild(body);
            return node;
        }

        var args = TokenNode(EXPR, "args");

        SpringAST previous = null;
        foreach (var name in arguments)
        {
            var argument = TokenNode(ID, name);
            if (previous == null)
            {
                args.setFirstChild(argument);
            }
            else
            {
                previous.setNextSibling(argument);
            }

            previous = argument;
        }

        node.setFirstChild(args);
        args.setNextSibling(body);
        return node;
    }

    // Syntax errors

    /// <summary>
    /// The line is always 1 and a tab moves to the next multiple of 8, as in the parser this one replaced, which counted
    /// the columns of the whole expression and never the lines.
    /// </summary>
    private static int ColumnOf(string input, int offset)
    {
        var column = 1;
        for (var i = 0; i < offset; i++)
        {
            column = input[i] == '\t' ? ((column - 1) / 8 + 1) * 8 + 1 : column + 1;
        }

        return column;
    }

    /// <summary>
    /// Reads the tokens before <paramref name="offset"/> to tell how many brackets are open and prefix operators are
    /// pending there, and whether the last one ends an operand.
    /// </summary>
    private static void ReadTokensBefore(string input, int offset, out int depth, out bool afterOperand)
    {
        var open = 0;
        var prefixes = 0;
        afterOperand = false;

        for (var position = SkipSpaces(input.AsSpan(), 0); position < offset; position = SkipSpaces(input.AsSpan(), position))
        {
            var length = NextToken(input.AsSpan(position), out var type);
            if (length == 0)
            {
                break;
            }

            position += length;
            switch (type)
            {
                case LPAREN or LBRACKET or LCURLY or LAMBDA or PROJECT or SELECT or SELECT_FIRST or SELECT_LAST or TYPE:
                    open++;
                    afterOperand = false;
                    break;
                case RPAREN or RBRACKET or RCURLY:
                    open--;
                    afterOperand = true;
                    break;
                case ID or STRING_LITERAL or INTEGER_LITERAL or HEXADECIMAL_INTEGER_LITERAL or REAL_LITERAL or TRUE or FALSE or NULL_LITERAL or DOT:
                    afterOperand = true;
                    break;
                default:
                    afterOperand = false;
                    break;
            }

            prefixes = !afterOperand && type is PLUS or MINUS or BANG ? prefixes + 1 : 0;
        }

        depth = open + prefixes;
    }

    private static SyntaxErrorException CreateSyntaxError(string input, int offset, string expected, bool mayBeTooDeep)
    {
        if (offset >= input.Length)
        {
            return SyntaxError(input, input.Length, expected != null ? $"expecting \"{expected}\", found '<EOF>'" : "unexpected end of expression");
        }

        var span = input.AsSpan(offset);
        var length = NextToken(span, out var type);
        if (length == 0)
        {
            return CreateLexicalError(input, offset);
        }

        var text = type == STRING_LITERAL ? StringText(input.Substring(offset, length)) : input.Substring(offset, length);
        if (mayBeTooDeep)
        {
            return SyntaxError(input, offset, $"unexpected token or expression nested too deeply: \"{text}\"");
        }

        return SyntaxError(input, offset, expected != null
            ? $"expecting \"{expected}\", found '{text}'"
            : $"unexpected token: [\"{text}\",<{type}>,line=1,col={ColumnOf(input, offset)}]");
    }

    /// <summary>The error for the text at <paramref name="offset"/>, which is not a token.</summary>
    private static SyntaxErrorException CreateLexicalError(string input, int offset)
    {
        if (input[offset] == '\'')
        {
            return SyntaxError(input, input.Length, "expecting ''', found '<EOF>'");
        }

        // A number is only invalid at the char that follows its "0x", or the 'e' of the exponent of its fraction
        var end = offset;
        if (input[end] == '0' && end + 1 < input.Length && input[end + 1] == 'x')
        {
            end += 2;
        }
        else if (IsDigit(input[end]) || input[end] == '.')
        {
            while (end < input.Length && (IsDigit(input[end]) || input[end] == '.'))
            {
                end++;
            }

            end++;
            while (end < input.Length && (input[end] == '+' || input[end] == '-'))
            {
                end++;
            }
        }

        return SyntaxError(input, end, end < input.Length ? "unexpected char: '" + input[end] + "'" : "unexpected end of expression");
    }

    private static SyntaxErrorException SyntaxError(string input, int offset, string message) =>
        new SyntaxErrorException(message, 1, ColumnOf(input, offset), input);
}
