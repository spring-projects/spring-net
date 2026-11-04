using NUnit.Framework;

namespace Spring.Expressions;

/// <summary>
/// Evaluates integer literals, with and without the <c>U</c>, <c>L</c> and <c>UL</c> suffixes.
/// </summary>
[TestFixture]
public class IntegerLiteralTests
{
    [TestCase("5U", 5u)]
    [TestCase("5u", 5u)]
    [TestCase("4294967295U", uint.MaxValue)]
    [TestCase("0xFFFFFFFFU", uint.MaxValue)]
    [TestCase("0x1fu", 31u)]
    public void SuffixedUIntegerIsUInt(string expression, uint expected)
    {
        object value = ExpressionEvaluator.GetValue(null, expression);

        Assert.That(value, Is.TypeOf<uint>());
        Assert.That(value, Is.EqualTo(expected));
    }

    [TestCase("5L", 5L)]
    [TestCase("5l", 5L)]
    [TestCase("9223372036854775807L", long.MaxValue)]
    [TestCase("0x1FL", 31L)]
    [TestCase("0x1Fl", 31L)]
    [TestCase("0x7FFFFFFFFFFFFFFFL", long.MaxValue)]
    public void SuffixedLIntegerIsLong(string expression, long expected)
    {
        object value = ExpressionEvaluator.GetValue(null, expression);

        Assert.That(value, Is.TypeOf<long>());
        Assert.That(value, Is.EqualTo(expected));
    }

    [TestCase("5UL", 5UL)]
    [TestCase("5LU", 5UL)]
    [TestCase("5ul", 5UL)]
    [TestCase("5uL", 5UL)]
    [TestCase("5lu", 5UL)]
    [TestCase("5lU", 5UL)]
    [TestCase("18446744073709551615UL", ulong.MaxValue)]
    [TestCase("0x1FUL", 31UL)]
    [TestCase("0xFFFFFFFFFFFFFFFFUL", ulong.MaxValue)]
    public void SuffixedULIntegerIsULong(string expression, ulong expected)
    {
        object value = ExpressionEvaluator.GetValue(null, expression);

        Assert.That(value, Is.TypeOf<ulong>());
        Assert.That(value, Is.EqualTo(expected));
    }

    [TestCase("4294967296U", 4294967296UL)]
    [TestCase("9223372036854775808L", 9223372036854775808UL)]
    [TestCase("18446744073709551615L", ulong.MaxValue)]
    [TestCase("0x100000000U", 4294967296UL)]
    [TestCase("0xFFFFFFFFFFFFFFFFL", ulong.MaxValue)]
    public void SuffixedValueTooLargeForItsTypeIsULong(string expression, ulong expected)
    {
        object value = ExpressionEvaluator.GetValue(null, expression);

        Assert.That(value, Is.TypeOf<ulong>());
        Assert.That(value, Is.EqualTo(expected));
    }

    [TestCase("18446744073709551616UL")]
    [TestCase("18446744073709551616L")]
    [TestCase("0x10000000000000000UL")]
    public void SuffixedValueAboveULongMaxOverflows(string expression)
    {
        Assert.Throws<OverflowException>(() => ExpressionEvaluator.GetValue(null, expression));
    }

    [Test]
    public void UnsuffixedIntegerIsInt()
    {
        object value = ExpressionEvaluator.GetValue(null, "5");

        Assert.That(value, Is.TypeOf<int>());
        Assert.That(value, Is.EqualTo(5));
    }

    [Test]
    public void UnsuffixedIntegerAboveIntMaxIsLong()
    {
        object value = ExpressionEvaluator.GetValue(null, "2147483648");

        Assert.That(value, Is.TypeOf<long>());
        Assert.That(value, Is.EqualTo(2147483648L));
    }

    [Test]
    public void UnsuffixedHexWithEightDigitsWrapsToNegativeInt()
    {
        // kept on purpose: Int32.Parse with NumberStyles.HexNumber reads 0xFFFFFFFF as two's complement
        object value = ExpressionEvaluator.GetValue(null, "0xFFFFFFFF");

        Assert.That(value, Is.TypeOf<int>());
        Assert.That(value, Is.EqualTo(-1));
    }

    [Test]
    public void UnsuffixedHexAboveEightDigitsIsLong()
    {
        object value = ExpressionEvaluator.GetValue(null, "0x100000000");

        Assert.That(value, Is.TypeOf<long>());
        Assert.That(value, Is.EqualTo(4294967296L));
    }

    [Test]
    public void SuffixedLiteralTakesPartInArithmetic()
    {
        object value = ExpressionEvaluator.GetValue(null, "5L + 1");

        Assert.That(value, Is.TypeOf<long>());
        Assert.That(value, Is.EqualTo(6L));
    }
}
