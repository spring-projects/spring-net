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
    /// The trees and syntax errors of the ANTLR 2 parser that this grammar replaced, when it was removed. A difference was
    /// accepted only in the position or text of an error, that the new parser reports at the token it could not read.
    /// </summary>
    [Test]
    public void SameTreesAsTheFormerParser()
    {
        var root = TestContext.CurrentContext.TestDirectory;
        while (!Directory.Exists(Path.Combine(root, "test")))
        {
            root = Path.GetDirectoryName(root);
        }

        var file = Path.Combine(root, "test", "Spring", "Spring.Core.Tests", "Expressions", "ExpressionGrammarGolden.json");
        var failures = new StringBuilder();
        foreach (var entry in JsonConvert.DeserializeObject<string[][]>(File.ReadAllText(file)))
        {
            var input = entry[0];
            var expression = Describe(() => Expression.Parse(input));
            if (expression != entry[1] && (!expression.StartsWith("ERROR") || !entry[1].StartsWith("ERROR")))
            {
                failures.AppendLine($"{input}{Environment.NewLine}  expected {entry[1]}{Environment.NewLine}  actual   {expression}");
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
        Assert.That(Expression.Parse(input).GetValue(), Is.EqualTo(1));
    }

    [TestCase("(", ")")]
    [TestCase("{", "}")]
    [TestCase("a[", "]")]
    [TestCase("f(", ")")]
    [TestCase("-(", ")")]
    public void DeepNestingIsASyntaxError(string open, string close)
    {
        var input = string.Concat(Enumerable.Repeat(open, 500_000)) + "1" + string.Concat(Enumerable.Repeat(close, 500_000));
        Assert.Throws<SyntaxErrorException>(() => Expression.Parse(input));
    }

    [Test]
    public void DeepNestingOfUnaryOperatorsIsASyntaxError()
    {
        Assert.Throws<SyntaxErrorException>(() => Expression.Parse(new string('-', 500_000) + "1"));
    }
}
