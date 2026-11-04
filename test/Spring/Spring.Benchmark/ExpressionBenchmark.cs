using System.Collections;
using BenchmarkDotNet.Attributes;
using Spring.Expressions;
using Spring.Objects;

namespace Spring.Benchmark;

/// <summary>
/// The expression language, from parsing alone to the layers above it. Only uses the public API, so that the same file
/// builds against any version of Spring.Core, to compare two of them.
/// </summary>
[MemoryDiagnoser]
public class ExpressionBenchmark
{
    private const string Small = "Name.Length + 1";

    private const string Medium =
        "(Age >= 18 and Name like 'A*') ? Address.City.ToUpper() + ', ' + Address.Zip : 'minor'";

    private const string Large =
        "Friends.?{Age > 20 and Address.City != 'Paris' or Name matches '^B.*$'}"
        + ".!{Name + ' (' + Age + ')'}.Count"
        + " + {1, 2, 3, 0x10, 5}.!{#this * 2 % 7}.Count"
        + " + #{'a' : 1, 'b' : 2.5e0}.Count"
        + " + (Friends.^{Age >= 30}.Name.Length ^ 2) - -Friends.${Age < 100}.Age"
        + " + (T(System.Math).Max(Age, 10) / 2 between {1, 100} ? new System.Text.StringBuilder('abc').Length : 0)"
        + " + (Address.Zip in {'75001', '75002'} and !(Name == null) xor Friends[0].Name is T(string) ? 1 : 2)"
        + " + (#sum = {|a, b| $a + $b}; #sum(1, 2))";

    private readonly Person root = new()
    {
        Name = "Alice",
        Age = 34,
        Address = new Address { City = "Lyon", Zip = "69001" },
        Friends = new ArrayList
        {
            new Person { Name = "Bob", Age = 41, Address = new Address { City = "Paris", Zip = "75001" } },
            new Person { Name = "Carol", Age = 19, Address = new Address { City = "Nice", Zip = "06000" } },
            new Person { Name = "Dan", Age = 28, Address = new Address { City = "Lille", Zip = "59000" } },
        },
    };

    private readonly Dictionary<string, object> variables = new();
    private string expression;
    private IExpression prepared;

    [Params("Small", "Medium", "Large")]
    public string Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        expression = Size switch
        {
            "Small" => Small,
            "Medium" => Medium,
            _ => Large,
        };

        prepared = Expression.Parse(expression);
    }

    /// <summary>Parsing alone.</summary>
    [Benchmark]
    public object Parse() => Expression.Parse(expression);

    /// <summary><see cref="ExpressionEvaluator"/> parses its expression on each call, without a cache.</summary>
    [Benchmark]
    public object GetValueUncached() => ExpressionEvaluator.GetValue(root, expression, variables);

    /// <summary>What the documentation recommends: parse once, evaluate many times.</summary>
    [Benchmark]
    public object ParseThenGetValue100()
    {
        var parsed = Expression.Parse(expression);
        object value = null;
        for (var i = 0; i < 100; i++)
        {
            value = parsed.GetValue(root, variables);
        }

        return value;
    }

    /// <summary>The evaluation alone, which doesn't depend on the parser.</summary>
    [Benchmark]
    public object GetValuePrepared() => prepared.GetValue(root, variables);
}

/// <summary>
/// <see cref="ObjectWrapper"/> parses the name of a property each time it is asked for it.
/// </summary>
[MemoryDiagnoser]
public class ObjectWrapperBenchmark
{
    private readonly ObjectWrapper wrapper = new(new Person
    {
        Name = "Alice",
        Friends = new ArrayList { new Person(), new Person { Address = new Address { City = "Nice" } } },
    });

    /// <summary>Only reads a name, without the parser.</summary>
    [Benchmark]
    public object SimpleProperty() => wrapper.GetPropertyValue("Name");

    /// <summary>Takes the primary expression parsing path.</summary>
    [Benchmark]
    public object PropertyPath() => wrapper.GetPropertyValue("Friends[1].Address.City");
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Address Address { get; set; }
    public IList Friends { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Zip { get; set; }
}
