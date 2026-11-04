using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Spring.Expressions;

[TestFixture]
public class ExpressionGrammarTests
{
    private static string Print(object node)
    {
        var builder = new StringBuilder();
        Print((SpringAST) node, builder);
        return builder.ToString();
    }

    private static void Print(SpringAST node, StringBuilder builder)
    {
        builder.Append('(').Append(node.GetType().Name).Append('|').Append(node.Type).Append('|').Append(node.getText());
        for (var child = (SpringAST) node.getFirstChild(); child != null; child = (SpringAST) child.getNextSibling())
        {
            builder.Append(' ');
            Print(child, builder);
        }

        builder.Append(')');
    }

    private static T Internal<T>(string method, string input)
    {
        try
        {
            return (T) typeof(Expression)
                .GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(string) }, null)
                .Invoke(null, new object[] { input });
        }
        catch (TargetInvocationException e) when (e.InnerException is SyntaxErrorException)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    /// <summary>Fails instead of hanging when parsing takes much longer than it should.</summary>
    private static IExpression ParseWithinTimeLimit(string input)
    {
        var task = Task.Run(() => Expression.Parse(input));
        Assert.That(Task.WaitAny(new Task[] { task }, TimeSpan.FromSeconds(5)), Is.EqualTo(0), "Parsing took too long");
        return task.GetAwaiter().GetResult();
    }

    private static string Describe(Func<IExpression> parse)
    {
        try
        {
            return Print(parse());
        }
        catch (SyntaxErrorException e)
        {
            return "ERROR " + e.Column + " " + e.Message.Split(new[] { " in expression" }, StringSplitOptions.None)[0];
        }
    }

    /// <summary>
    /// The trees and syntax errors of the ANTLR 2 parser that this grammar replaced, written by that parser. The entries
    /// with five items are the errors with a deliberate difference: the position or text of this parser, at the token it
    /// could not read, then those of the former one, kept for reference.
    /// </summary>
    [Test]
    public void SameTreesAndErrorsAsTheFormerParser()
    {
        string[][] entries;
        using (var stream = typeof(ExpressionGrammarTests).Assembly.GetManifestResourceStream("Spring.Expressions.ExpressionGrammarGolden.json"))
        using (var reader = new StreamReader(stream))
        {
            entries = JsonConvert.DeserializeObject<string[][]>(reader.ReadToEnd());
        }

        var failures = new StringBuilder();
        foreach (var entry in entries)
        {
            var input = entry[0];
            var expression = Describe(() => Expression.Parse(input));
            var primary = Describe(() => Internal<IExpression>("ParsePrimary", input));
            if (expression != entry[1])
            {
                failures.AppendLine($"{input}{Environment.NewLine}  expected {entry[1]}{Environment.NewLine}  actual   {expression}");
            }

            if (primary != entry[2])
            {
                failures.AppendLine($"primary {input}{Environment.NewLine}  expected {entry[2]}{Environment.NewLine}  actual   {primary}");
            }
        }

        Assert.That(failures.ToString(), Is.Empty);
    }

    [TestCase("'date", 6, "expecting ''', found '<EOF>'")]
    [TestCase("a ~", 3, "unexpected char: '~'")]
    [TestCase("0xg", 3, "unexpected char: 'g'")]
    [TestCase("1.5e!", 5, "unexpected char: '!'")]
    [TestCase("a, b", 2, "expecting \"EOF\", found ','")]
    [TestCase("a\tb\t~", 17, "unexpected char: '~'")]
    public void SyntaxErrorsReportTheOffendingText(string input, int column, string message)
    {
        var e = Assert.Throws<SyntaxErrorException>(() => Expression.Parse(input));
        Assert.That(e.Line, Is.EqualTo(1));
        Assert.That(e.Column, Is.EqualTo(column));
        Assert.That(e.Message, Does.StartWith($"Syntax Error on line 1, column {column}: {message} in expression"));
        Assert.That(e.Expression, Is.EqualTo(input));
    }

    [TestCase("a?b", 4)]
    [TestCase("new a", 6)]
    [TestCase("new a.b", 8)]
    public void IncompleteExpressionsReportTheEndOfTheInput(string input, int column)
    {
        var e = Assert.Throws<SyntaxErrorException>(() => Expression.Parse(input));
        Assert.That(e.Message, Does.StartWith($"Syntax Error on line 1, column {column}: unexpected end of expression in expression"));
    }

    [Test]
    public void SyntaxErrorsKeepTheirStateWhenSerialized()
    {
        var original = Assert.Throws<SyntaxErrorException>(() => Expression.Parse("a ~"));

#pragma warning disable SYSLIB0050 // what is tested
        var info = new SerializationInfo(typeof(SyntaxErrorException), new FormatterConverter());
        ((ISerializable) original).GetObjectData(info, default);
#pragma warning restore SYSLIB0050
        var copy = (SyntaxErrorException) Activator.CreateInstance(
            typeof(SyntaxErrorException),
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new object[] { info, default(StreamingContext) },
            null);

        Assert.That(copy.Line, Is.EqualTo(original.Line));
        Assert.That(copy.Column, Is.EqualTo(original.Column));
        Assert.That(copy.Expression, Is.EqualTo(original.Expression));
        Assert.That(copy.Message, Is.EqualTo(original.Message));
        Assert.That(copy.HResult, Is.EqualTo(original.HResult));
        Assert.That(copy.StackTrace, Is.EqualTo(original.StackTrace));
    }

    [TestCase("foo", "(PropertyOrFieldNode|33|foo)")]
    [TestCase("  foo  bar", "(PropertyOrFieldNode|33|foo)")]
    [TestCase("foo-bar", "(PropertyOrFieldNode|33|foo)")]
    [TestCase("foo~", "(PropertyOrFieldNode|33|foo)")]
    [TestCase("in", "ERROR 1 Syntax Error on line 1, column 1: expecting \"ID\", found 'in'")]
    [TestCase("T(a", "ERROR 1 Syntax Error on line 1, column 1: expecting \"ID\", found 'T('")]
    [TestCase("1a", "ERROR 1 Syntax Error on line 1, column 1: expecting \"ID\", found '1'")]
    public void PropertyRuleReadsOneName(string input, string expected)
    {
        Assert.That(Describe(() => Internal<IExpression>("ParseProperty", input)), Is.EqualTo(expected));
    }

    [TestCase("{|a|$a}", "(LambdaExpressionNode|4|lambda (SpringAST|4|args (SpringAST|33|a)) (Expression|4|expression (LocalVariableNode|33|a)))")]
    [TestCase("{||1} ~", "(LambdaExpressionNode|4|lambda (Expression|4|expression (IntLiteralNode|51|1)))")]
    [TestCase("x", "ERROR 1 Syntax Error on line 1, column 1: expecting \"LAMBDA\", found 'x'")]
    [TestCase("{|a", "ERROR 4 Syntax Error on line 1, column 4: unexpected end of expression")]
    public void LambdaRuleStopsAtItsBrace(string input, string expected)
    {
        Assert.That(Describe(() => Internal<SpringAST>("ParseLambda", input) as IExpression), Is.EqualTo(expected));
    }

    [TestCase(" ")]
    [TestCase("")]
    [TestCase(null)]
    public void BlankTextIsAnEmptyExpression(string input)
    {
        Assert.That(Expression.Parse(input), Is.TypeOf<Expression>());
    }

    [Test]
    public void DepthOfParenthesesDoesNotCostExponentialTime()
    {
        // The former parser read each parenthesis twice: this one took 3 s at depth 18 and doubled with each level
        var input = new string('(', 50) + "1" + new string(')', 50);
        Assert.That(ParseWithinTimeLimit(input).GetValue(), Is.EqualTo(1));
    }

    [TestCase("(", ")")]
    [TestCase("{", "}")]
    [TestCase("a[", "]")]
    [TestCase("f(", ")")]
    [TestCase("-(", ")")]
    public void DeepNestingIsASyntaxError(string open, string close)
    {
        var input = string.Concat(Enumerable.Repeat(open, 500_000)) + "1" + string.Concat(Enumerable.Repeat(close, 500_000));
        var e = Assert.Throws<SyntaxErrorException>(() => Expression.Parse(input));
        Assert.That(e.Message, Does.Contain("nested too deeply"));
    }

    [Test]
    public void DeepNestingOfUnaryOperatorsIsASyntaxError()
    {
        var e = Assert.Throws<SyntaxErrorException>(() => Expression.Parse(new string('-', 500_000) + "1"));
        Assert.That(e.Message, Does.Contain("nested too deeply"));
    }

    [Test]
    public void FailedNamedArgumentsAreNotReadAgain()
    {
        // Reading each one again as an assignment took 1.6 s at depth 22, and multiplied by 1.5 with each level
        var input = string.Concat(Enumerable.Repeat("new a(b=", 60)) + "1";
        Assert.Throws<SyntaxErrorException>(() => ParseWithinTimeLimit(input));
    }

    [Test]
    public void LongPathsTakeLinearTime()
    {
        // Appending a node walked the others, which took 11 s for this path
        var input = "a" + string.Concat(Enumerable.Repeat(".a", 160_000));
        Assert.That(ParseWithinTimeLimit(input), Is.TypeOf<Expression>());
    }

    [TestCase(100_000, "1+")]
    public void EvaluatingTooDeepATreeThrows(int count, string repeated)
    {
        var input = string.Concat(Enumerable.Repeat(repeated, count)) + "1";
        Exception exception = null;

        // On a small stack, where the tree is too deep for the evaluation although the parser read it
        var thread = new Thread(() =>
        {
            try
            {
                Expression.Parse(input).GetValue();
            }
            catch (Exception e)
            {
                exception = e;
            }
        }, 256 * 1024);

        thread.Start();
        thread.Join();

        Assert.That(exception, Is.TypeOf<InsufficientExecutionStackException>());
    }
}
