using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Spring.Expressions.Parser.antlr.collections;

namespace Spring.Expressions;

/// <summary>
/// Compares the Parlot parser with the ANTLR one it replaces, on the strings of the test suite, on generated expressions,
/// and on mutations of both. PARSER_DIFFERENTIAL_ITERATIONS sets the number of mutations and generated expressions,
/// PARSER_DIFFERENTIAL_REPORT a file to write the differences to.
/// </summary>
[TestFixture]
public class ParserDifferentialTests
{
    private static readonly string[] Vocabulary =
    {
        " ", "  ", "\t", "\n", "\r\n", "a", "b", "foo", "Bar_1", "_x", "T", "T(", "(", ")", "[", "]", "{", "}", "{|", "|", ",", ";", ":", ".", "\\", "\\.", "`", "'", "''", "'s'",
        "=", "==", "!=", "!", "<", "<=", ">", ">=", "+", "-", "*", "/", "%", "^", "?", "??", "?{", "!{", "^{", "${", "$", "#", "#{", "@", "@(", "@[", "~", "\"", "&", "é", "\f", "\0",
        "and", "or", "xor", "in", "is", "between", "like", "matches", "null", "true", "false", "new", "new ", "And", "android", "input",
        "0", "1", "12", "1.5", ".5", "1.", "1e5", "1e", "1e+", "1E-5", "1f", "1d", "1m", "1L", "1u", "1UL", "1lU", "1Ul", "0x", "0x1F", "0x1Fl", "0X1", "0xg", "00",
    };

    private sealed class Rule
    {
        public string Name;
        public Func<string, object> Antlr;
        public Func<string, object> Parlot;
    }

    private static Func<string, object> Method(Type type, string name)
    {
        var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
        return input =>
        {
            try
            {
                return method.Invoke(null, new object[] { input });
            }
            catch (TargetInvocationException e)
            {
                return e.InnerException;
            }
        };
    }

    private static string Print(object result)
    {
        if (result is SyntaxErrorException syntaxError)
        {
            return "SyntaxError";
        }

        if (result is Exception e)
        {
            return "!" + e.GetType().Name;
        }

        var builder = new StringBuilder();
        Print((SpringAST) result, builder);
        return builder.ToString();
    }

    private static void Print(SpringAST node, StringBuilder builder)
    {
        builder.Append('(').Append(node.GetType().Name).Append('|').Append(node.Type).Append('|').Append(node.getText());

        // The tokens of a qualified name are only read through its text
        if (node is not QualifiedIdentifier)
        {
            for (var child = (SpringAST) node.getFirstChild(); child != null; child = (SpringAST) child.getNextSibling())
            {
                builder.Append(' ');
                Print(child, builder);
            }
        }

        builder.Append(')');
    }

    private static string Generate(Random random, int depth)
    {
        string E() => Generate(random, depth - 1);
        string Id() => new[] { "a", "b", "foo", "Name", "T", "x1", "_y", "input" }[random.Next(8)];
        string Sep(string separator, int min, Func<string> item)
        {
            var count = min + random.Next(3);
            return string.Join(separator, Enumerable.Range(0, count).Select(_ => item()));
        }

        string S() => random.Next(4) == 0 ? " " : "";

        if (depth <= 0)
        {
            return new[] { Id(), "1", "2.5", "'s'", "null", "true", "false", "0x1F", "#v", "$l", ".5", "1e5" }[random.Next(12)];
        }

        switch (random.Next(46))
        {
            case 0: return E() + S() + "=" + S() + E();
            case 1: return E() + S() + "??" + S() + E();
            case 2: return E() + S() + "?" + S() + E() + S() + ":" + S() + E();
            case 3: return E() + " or " + E();
            case 4: return E() + " xor " + E();
            case 5: return E() + " and " + E();
            case 6: return E() + " " + new[] { "==", "!=", "<", "<=", ">", ">=", "in", "is", "between", "like", "matches" }[random.Next(11)] + " " + E();
            case 7: return E() + S() + "+" + S() + E();
            case 8: return E() + S() + "-" + S() + E();
            case 9: return E() + S() + new[] { "*", "/", "%" }[random.Next(3)] + S() + E();
            case 10: return E() + S() + "^" + S() + E();
            case 11: return new[] { "+", "-", "!" }[random.Next(3)] + S() + E();
            case 12: return Id();
            case 13: return Id() + "(" + Sep(",", 0, E) + ")";
            case 14: return E() + "." + Id();
            case 15: return E() + S() + Id();
            case 16: return E() + "." + Id() + "(" + Sep(",", 0, E) + ")";
            case 17: return E() + "[" + Sep(",", 1, E) + "]";
            case 18: return "[" + Sep(",", 1, E) + "]";
            case 19: return E() + "!{" + E() + "}";
            case 20: return E() + "?{" + Sep(",", 1, E) + "}";
            case 21: return E() + "^{" + E() + "}";
            case 22: return E() + "${" + E() + "}";
            case 23: return "!{" + E() + "}";
            case 24: return "?{" + Sep(",", 1, E) + "}";
            case 25: return "(" + E() + ")";
            case 26: return "(" + E() + ";" + Sep(";", 1, E) + ")";
            case 27: return E() + "(" + E() + ";" + Sep(";", 1, E) + ")";
            case 28: return "#" + S() + Id();
            case 29: return "#" + S() + Id() + "(" + Sep(",", 0, E) + ")";
            case 30: return "$" + S() + Id();
            case 31: return "$" + S() + Id() + "(" + Sep(",", 0, E) + ")";
            case 32: return "#{" + Sep(",", 1, () => E() + ":" + E()) + "}";
            case 33: return "{" + Sep(",", 1, E) + "}";
            case 34: return "{|" + Sep(",", 0, Id) + "|" + E() + "}";
            case 35: return "T(" + Id() + Sep(S(), 0, () => new[] { ".", Id(), ",", "[]", "<", ">", "=", "1.5", "'q'", "`1" }[random.Next(10)]) + ")";
            case 36: return "@(" + Id() + ")";
            case 37: return "@(" + (random.Next(2) == 0 ? "'" + Id() + "'" : Id() + ".x") + ":" + (random.Next(2) == 0 ? "'" + Id() + " " + Id() + "'" : Id()) + ")";
            case 38: return "@[" + Id() + "." + Id() + "]";
            case 39: return "@[" + Id() + "(" + Sep(",", 0, () => random.Next(2) == 0 ? Id() + "=" + E() : E()) + ")]";
            case 40: return "new " + Id() + "." + Id() + "(" + Sep(",", 0, () => random.Next(2) == 0 ? Id() + "=" + E() : E()) + ")";
            case 41: return "new " + Id() + "[" + Sep(",", 0, E) + "]";
            case 42: return "new " + Id() + "[" + Sep(",", 0, E) + "]{" + Sep(",", 1, E) + "}";
            case 43: return "'" + Id() + "''" + Id() + "'";
            case 44: return E() + S() + ".";
            default: return E();
        }
    }

    private static string Mutate(Random random, string input)
    {
        var builder = new StringBuilder(input);
        var count = 1 + random.Next(3);
        for (var i = 0; i < count; i++)
        {
            var position = random.Next(builder.Length + 1);
            switch (random.Next(5))
            {
                case 0:
                    builder.Insert(position, Vocabulary[random.Next(Vocabulary.Length)]);
                    break;
                case 1:
                    if (position < builder.Length)
                    {
                        builder.Remove(position, Math.Min(1 + random.Next(3), builder.Length - position));
                    }

                    break;
                case 2:
                    if (position < builder.Length)
                    {
                        builder.Remove(position, 1).Insert(position, Vocabulary[random.Next(Vocabulary.Length)]);
                    }

                    break;
                case 3:
                    builder.Length = position;
                    break;
                default:
                    if (position + 1 < builder.Length)
                    {
                        (builder[position], builder[position + 1]) = (builder[position + 1], builder[position]);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    private static void Report(StringBuilder report, Dictionary<string, int> counts, string kind, string rule, string input, string antlr, string parlot, int samples = 40)
    {
        counts.TryGetValue(kind, out var count);
        counts[kind] = count + 1;
        if (count < samples)
        {
            report.Append(kind).Append(" [").Append(rule).Append("] ").AppendLine(JsonConvert.ToString(input));
            report.Append("  antlr:  ").AppendLine(antlr);
            report.Append("  parlot: ").AppendLine(parlot);
        }
    }

    [Test]
    public void BothParsersAgree()
    {
        var assembly = typeof(Expression).Assembly;
        var grammar = assembly.GetType("Spring.Expressions.ExpressionGrammar");
        var rules = new[]
        {
            new Rule { Name = "expression", Antlr = Method(typeof(Expression), "Parse"), Parlot = Method(grammar, "ParseExpression") },
            new Rule { Name = "primary", Antlr = Method(typeof(Expression), "ParsePrimary"), Parlot = Method(grammar, "ParsePrimary") },
            new Rule { Name = "property", Antlr = Method(typeof(Expression), "ParseProperty"), Parlot = Method(grammar, "ParseProperty") },
            new Rule { Name = "lambda", Antlr = Method(typeof(Expression), "ParseLambda"), Parlot = Method(grammar, "ParseLambda") },
        };

        var root = TestContext.CurrentContext.TestDirectory;
        while (!Directory.Exists(Path.Combine(root, "test")))
        {
            root = Path.GetDirectoryName(root);
        }

        var corpusFile = Path.Combine(root, "test", "Spring", "Spring.Core.Tests", "Expressions", "DifferentialCorpus.json");
        var corpus = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(corpusFile));

        var iterations = int.TryParse(Environment.GetEnvironmentVariable("PARSER_DIFFERENTIAL_ITERATIONS"), out var value) ? value : 100_000;
        var random = new Random(42);
        var report = new StringBuilder();
        var counts = new Dictionary<string, int>();
        var accepted = 0L;
        var rejected = 0L;

        void Compare(string input)
        {
            // ANTLR takes exponential time in the depth of nested parentheses, and doesn't read blank text with these rules
            if (string.IsNullOrWhiteSpace(input) || input.Count(c => c == '(') > 12 || input.Contains('\uFFFF'))
            {
                return;
            }

            foreach (var rule in rules)
            {
                var antlr = rule.Antlr(input);
                var parlot = rule.Parlot(input);
                var antlrText = Print(antlr);
                var parlotText = Print(parlot);
                if (antlrText != parlotText)
                {
                    Report(report, counts, antlrText == "SyntaxError" || parlotText == "SyntaxError" ? "ACCEPTANCE" : "TREE", rule.Name, input, antlr is Exception a ? antlrText + " " + a.Message : antlrText, parlot is Exception p ? parlotText + " " + p.Message : parlotText);
                    continue;
                }

                if (antlr is SyntaxErrorException antlrError && parlot is SyntaxErrorException parlotError)
                {
                    rejected++;
                    var shape = System.Text.RegularExpressions.Regex.Replace(antlrError.Message.Split(new[] { " in expression" }, StringSplitOptions.None)[0], "^Syntax Error on line .*?: ", "");
                    shape = System.Text.RegularExpressions.Regex.Replace(shape, "(found|char:|token:).*$", "$1");
                    var match = (antlrError.Column == parlotError.Column ? "samecol " : "diffcol ") + (antlrError.Message == parlotError.Message ? "samemsg " : "diffmsg ");
                    counts.TryGetValue(match + shape, out var c0);
                    counts[match + shape] = c0 + 1;
                    if (antlrError.Column != parlotError.Column || antlrError.Line != parlotError.Line)
                    {
                        Report(report, counts, "POSITION", rule.Name, input, antlrError.Message, parlotError.Message, 60);
                    }
                    else if (antlrError.Message != parlotError.Message)
                    {
                        Report(report, counts, "MESSAGE", rule.Name, input, antlrError.Message, parlotError.Message, 60);
                    }
                }
                else
                {
                    accepted++;
                }
            }
        }

        foreach (var input in corpus)
        {
            Compare(input);
        }

        var seeds = corpus.Where(x => x.Length <= 200).ToArray();
        for (var i = 0; i < iterations; i++)
        {
            switch (i % 3)
            {
                case 0:
                    Compare(Mutate(random, seeds[random.Next(seeds.Length)]));
                    break;
                case 1:
                    Compare(Generate(random, 1 + random.Next(4)));
                    break;
                default:
                    Compare(Mutate(random, Generate(random, 1 + random.Next(4))));
                    break;
            }
        }

        var summary = $"accepted {accepted}, rejected {rejected}, corpus {corpus.Length}: " + string.Join(", ", counts.OrderByDescending(x => x.Value).Select(x => x.Key + "=" + x.Value + Environment.NewLine));
        TestContext.Out.WriteLine(summary);
        var reportFile = Environment.GetEnvironmentVariable("PARSER_DIFFERENTIAL_REPORT");
        if (reportFile != null)
        {
            File.WriteAllText(reportFile, summary + Environment.NewLine + report);
        }

        counts.TryGetValue("ACCEPTANCE", out var acceptance);
        counts.TryGetValue("TREE", out var tree);
        Assert.AreEqual(0, acceptance + tree, summary + Environment.NewLine + report);
    }
}
