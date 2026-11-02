using Parlot;
using Parlot.Fluent;
using Parlot.SourceGenerator;
using static Parlot.Fluent.Parsers;
using static Spring.Expressions.Parser.ExpressionParserTokenTypes;

namespace Spring.Expressions;

/// <summary>
/// The grammar of the expression language. This file is build-only: the Parlot source generator runs it at compile time
/// and emits <see cref="TryParse"/> and everything it needs into this assembly.
/// </summary>
/// <remarks>
/// Tokens that are the start of a longer one, such as the '^' of "^{", are scanned so as not to match it, like a lexer
/// would. Without that an alternative that fails could be read again another way, which an LL(k) grammar never does.
/// </remarks>
internal static partial class ExpressionGrammar
{
    [GenerateParser(nameof(TryParse))]
    private static Parser<BaseNode> Build(Rule rule)
    {
        var expression = Deferred<BaseNode>();

        var lparen = Terms.Char('(');
        var rparen = Terms.Char(')');
        var lbracket = Terms.Char('[');
        var rbracket = Terms.Char(']');
        var lcurly = Terms.Scan(ScanLeftCurly, "{");
        var rcurly = Terms.Char('}');
        var comma = Terms.Char(',');
        var colon = Terms.Char(':');
        var semi = Terms.Char(';');
        var assign = Terms.Scan(ScanAssign, "=");

        var id = Terms.Scan(ScanId, IdStart);
        var qualifiedId = Terms.Scan(ScanQualifiedId, IdStart).Then(static x => TokensText(x.ToString()));
        var name = Terms.Scan(ScanName, IdStart).Then(static x => TokensText(x.ToString()));
        var stringLiteral = Terms.Scan(ScanString, "'").Then(static x => StringText(x.ToString()));

        // A name followed by '(' is a call, it can't be read again as a name followed by an expression list
        var arguments = lparen.SkipAnd(Separated(comma, expression, min: 0)).AndSkip(rparen);
        var call = arguments.Or(Not(lparen).Then(static _ => (IReadOnlyList<BaseNode>) null));

        var expressions = Separated(comma, expression, min: 1);

        var methodOrProperty = id.And(call).Then(static x => MethodOrProperty(x.Item1.ToString(), x.Item2));
        var indexer = lbracket.SkipAnd(expressions).AndSkip(rbracket).Then(static x => Node(new IndexerNode(), LBRACKET, "[", x));
        var projection = Terms.Text("!{").SkipAnd(expression).AndSkip(rcurly).Then(static x => Operator(new ProjectionNode(), PROJECT, "!{", x));
        var selection = Terms.Text("?{").SkipAnd(expressions).AndSkip(rcurly).Then(static x => Node(new SelectionNode(), SELECT, "?{", x));
        var firstSelection = Terms.Text("^{").SkipAnd(expression).AndSkip(rcurly).Then(static x => Operator(new SelectionFirstNode(), SELECT_FIRST, "^{", x));
        var lastSelection = Terms.Text("${").SkipAnd(expression).AndSkip(rcurly).Then(static x => Operator(new SelectionLastNode(), SELECT_LAST, "${", x));

        var functionOrVariable = id.And(call).Then(static x => FunctionOrVariable(x.Item1.ToString(), x.Item2));
        var mapEntry = expression.AndSkip(colon).And(expression).Then(static x => MapEntry(x.Item1, x.Item2));
        var mapInitializer = lcurly.SkipAnd(Separated(comma, mapEntry, min: 1)).AndSkip(rcurly).Then(static x => Node(new MapInitializerNode(), LCURLY, "{", x));
        var pound = Terms.Char('#').SkipAnd(functionOrVariable.Or(mapInitializer));

        var dollar = Terms.Scan(ScanDollar, "$").SkipAnd(id).And(call).Then(static x => LocalFunctionOrVariable(x.Item1.ToString(), x.Item2));

        var quotableName = stringLiteral.Then(static x => QuotableName(x, STRING_LITERAL)).Or(name.Then(static x => QuotableName(x, ID)));
        var reference = lparen.SkipAnd(quotableName).And(colon.SkipAnd(quotableName).Optional()).AndSkip(rparen)
            .Then(static x => Reference(x.Item1, x.Item2.TryGetValue(out var second) ? second : null));

        // A named argument and the assignment that starts the same way can't both parse: its right side is an expression
        var namedArgument = id.AndSkip(assign).And(expression).Then(static x => NamedArgument(x.Item1.ToString(), x.Item2));
        var constructorArguments = lparen.SkipAnd(Separated(comma, namedArgument.Or(expression), min: 0)).AndSkip(rparen);
        var attribute = lbracket.SkipAnd(qualifiedId).And(constructorArguments.Optional()).AndSkip(rbracket)
            .Then(static x => Node(new AttributeNode(), EXPR, x.Item1, x.Item2.TryGetValue(out var args) ? args : null));
        var at = Terms.Char('@').SkipAnd(reference.Or(attribute));

        var listInitializer = lcurly.SkipAnd(expressions).AndSkip(rcurly).Then(static x => Node(new ListInitializerNode(), LCURLY, "{", x));
        var arrayRank = lbracket.SkipAnd(Separated(comma, expression, min: 0)).AndSkip(rbracket);
        var constructor = Terms.Keyword("new", IdPart).SkipAnd(qualifiedId).And(
            constructorArguments.Then(static x => Node(new ConstructorNode(), EXPR, null, x))
                .Or(arrayRank.And(listInitializer.Optional()).Then(static x => ArrayConstructor(x.Item1, x.Item2.TryGetValue(out var list) ? list : null))))
            .Then(static x => WithText(x.Item2, x.Item1));

        var type = Terms.Text("T(").SkipAnd(name).AndSkip(rparen).Then(static x => Leaf(new TypeNode(), EXPR, x));

        var lambda = Terms.Text("{|").SkipAnd(Separated(comma, id.Then(static x => x.ToString()), min: 0)).AndSkip(Terms.Char('|'))
            .And(expression).AndSkip(rcurly).Then(static x => Lambda(x.Item1, x.Item2));

        // '(' expression ')' is the expression, with the type of node it has
        var parenthesized = lparen.SkipAnd(expression).And(ZeroOrMany(semi.SkipAnd(expression))).AndSkip(rparen)
            .Then(static x => x.Item2.Count == 0 ? x.Item1 : ExpressionList(x.Item1, x.Item2));
        var expressionList = lparen.SkipAnd(expression).And(OneOrMany(semi.SkipAnd(expression))).AndSkip(rparen)
            .Then(static x => ExpressionList(x.Item1, x.Item2));

        var literal = OneOf(
            Terms.Scan(ScanNumber, NumberStart).Then(static x => NumberLiteral(x.ToString())),
            stringLiteral.Then(static x => Leaf(new StringLiteralNode(), STRING_LITERAL, x)),
            Terms.Keyword("null", IdPart).Then(static _ => Leaf(new NullLiteralNode(), NULL_LITERAL, "null")),
            Terms.Keyword("true", IdPart).Then(static _ => Leaf(new BooleanLiteralNode(), TRUE, "true")),
            Terms.Keyword("false", IdPart).Then(static _ => Leaf(new BooleanLiteralNode(), FALSE, "false")));

        var startNode = OneOf(
            methodOrProperty,
            parenthesized,
            pound,
            dollar,
            at,
            indexer,
            literal,
            type,
            constructor,
            projection,
            selection,
            firstSelection,
            lastSelection,
            lambda,
            listInitializer);

        var node = OneOf(
            methodOrProperty,
            indexer,
            projection,
            selection,
            firstSelection,
            lastSelection,
            expressionList);

        // The nodes of a primary expression are its children, the dots between them being optional
        var primary = startNode.Then(static x => Primary(x)).Operators()
            .Postfix(
                (node, static (x, child) => AppendChild(x, child)),
                (Terms.Scan(ScanDot, "."), static (x, _) => x));

        var unary = primary
            .Unary(
                (Terms.Char('+'), static x => Operator(new OpUnaryPlus(), PLUS, "+", x)),
                (Terms.Char('-'), static x => Operator(new OpUnaryMinus(), MINUS, "-", x)),
                (Terms.Scan(ScanBang, "!").Then(static _ => '!'), static x => Operator(new OpNOT(), BANG, "!", x)));

        // Neither the power nor the relational operators chain, which an operator table can't express
        var power = unary.And(Terms.Scan(ScanPower, "^").SkipAnd(unary).Optional())
            .Then(static x => x.Item2.TryGetValue(out var right) ? Operator(new OpPOWER(), POWER, "^", x.Item1, right) : x.Item1);

        var sum = power.Operators()
            .LeftAssociative(
                (Terms.Char('*'), static (a, b) => Operator(new OpMULTIPLY(), STAR, "*", a, b)),
                (Terms.Char('/'), static (a, b) => Operator(new OpDIVIDE(), DIV, "/", a, b)),
                (Terms.Char('%'), static (a, b) => Operator(new OpMODULUS(), MOD, "%", a, b)))
            .LeftAssociative(
                (Terms.Char('+'), static (a, b) => Operator(new OpADD(), PLUS, "+", a, b)),
                (Terms.Char('-'), static (a, b) => Operator(new OpSUBTRACT(), MINUS, "-", a, b)));

        var relationalOperator = OneOf(
            Terms.Text("==").Then(static _ => Relational(new OpEqual(), "==")),
            Terms.Text("!=").Then(static _ => Relational(new OpNotEqual(), "!=")),
            Terms.Text("<=").Then(static _ => Relational(new OpLessOrEqual(), "<=")),
            Terms.Text("<").Then(static _ => Relational(new OpLess(), "<")),
            Terms.Text(">=").Then(static _ => Relational(new OpGreaterOrEqual(), ">=")),
            Terms.Text(">").Then(static _ => Relational(new OpGreater(), ">")),
            Terms.Keyword("in", IdPart).Then(static _ => Relational(new OpIn(), "in")),
            Terms.Keyword("is", IdPart).Then(static _ => Relational(new OpIs(), "is")),
            Terms.Keyword("between", IdPart).Then(static _ => Relational(new OpBetween(), "between")),
            Terms.Keyword("like", IdPart).Then(static _ => Relational(new OpLike(), "like")),
            Terms.Keyword("matches", IdPart).Then(static _ => Relational(new OpMatches(), "matches")));

        var relational = sum.And(relationalOperator.And(sum).Optional())
            .Then(static x => x.Item2.TryGetValue(out var rest) ? PrependChild(AppendChild(rest.Item1, rest.Item2), x.Item1) : x.Item1);

        var logical = relational.Operators()
            .LeftAssociative((Terms.Keyword("and", IdPart), static (a, b) => Operator(new OpAND(), AND, "and", a, b)))
            .LeftAssociative((Terms.Keyword("xor", IdPart), static (a, b) => Operator(new OpXOR(), XOR, "xor", a, b)))
            .LeftAssociative((Terms.Keyword("or", IdPart), static (a, b) => Operator(new OpOR(), OR, "or", a, b)));

        // Neither do these, and the last operand of a conditional is any expression: not levels of the table either
        var tail = OneOf(
            assign.SkipAnd(logical).Then(static x => Operator(new AssignNode(), ASSIGN, "=", x)),
            Terms.Text("??").SkipAnd(logical).Then(static x => Operator(new DefaultNode(), DEFAULT, "??", x)),
            Terms.Scan(ScanQuestion, "?").SkipAnd(expression).AndSkip(colon).And(expression)
                .Then(static x => Operator(new TernaryNode(), QMARK, "?", x.Item1, x.Item2)));

        expression.Parser = logical.And(tail.Optional())
            .Then(static x => x.Item2.TryGetValue(out var parent) ? PrependChild(parent, x.Item1) : x.Item1);

        Parser<BaseNode> wholeExpression = expression.AndSkip(SkipWhiteSpace(Eof()));
        Parser<BaseNode> primaryAlone = primary.When(static (context, x) => CanFollowPrimary(x, context.Scanner.Cursor.Buffer, context.Scanner.Cursor.Offset));

        return If<BaseNode>(() => rule == Rule.Expression, wholeExpression, If<BaseNode>(() => rule == Rule.Primary, primaryAlone, lambda))
            .WithWhiteSpaceSet(WhiteSpaceSet.SpaceTabCrLf);
    }
}
