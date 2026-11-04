using System.Globalization;

namespace Spring.Expressions;

/// <summary>
/// Evaluates the text of integer literal tokens, including the C# style <c>U</c>, <c>L</c> and <c>UL</c> suffixes.
/// </summary>
internal static class IntegerLiteral
{
    /// <summary>
    /// Evaluates a decimal literal such as <c>5</c>, <c>5L</c> or <c>5UL</c>.
    /// </summary>
    internal static object ParseDecimal(string text)
    {
        var digits = SplitSuffix(text, out var unsigned, out var @long);
        if (!unsigned && !@long)
        {
            try
            {
                return Int32.Parse(digits);
            }
            catch (OverflowException)
            {
                return Int64.Parse(digits);
            }
        }

        return Suffixed(UInt64.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture), unsigned, @long);
    }

    /// <summary>
    /// Evaluates a hexadecimal literal such as <c>0x1F</c>, <c>0x1FL</c> or <c>0x1FUL</c>.
    /// </summary>
    internal static object ParseHex(string text)
    {
        var digits = SplitSuffix(text, out var unsigned, out var @long).Substring(2);
        if (!unsigned && !@long)
        {
            try
            {
                return Int32.Parse(digits, NumberStyles.HexNumber);
            }
            catch (OverflowException)
            {
                return Int64.Parse(digits, NumberStyles.HexNumber);
            }
        }

        return Suffixed(UInt64.Parse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture), unsigned, @long);
    }

    /// <summary>
    /// Types a suffixed value by the C# rules: <c>U</c> gives <c>uint</c> or <c>ulong</c>, <c>L</c> gives <c>long</c> or
    /// <c>ulong</c>, and <c>UL</c> gives <c>ulong</c>.
    /// </summary>
    private static object Suffixed(ulong value, bool unsigned, bool @long)
    {
        if (unsigned && @long)
        {
            return value;
        }

        // each branch boxes its own type: a conditional expression over uint and ulong would convert to ulong
        if (unsigned)
        {
            if (value <= UInt32.MaxValue)
            {
                return (uint) value;
            }

            return value;
        }

        if (value <= Int64.MaxValue)
        {
            return (long) value;
        }

        return value;
    }

    private static string SplitSuffix(string text, out bool unsigned, out bool @long)
    {
        unsigned = false;
        @long = false;
        var end = text.Length;
        while (end > 0)
        {
            var c = text[end - 1];
            if (c == 'U' || c == 'u')
            {
                unsigned = true;
            }
            else if (c == 'L' || c == 'l')
            {
                @long = true;
            }
            else
            {
                break;
            }

            end--;
        }

        return end == text.Length ? text : text.Substring(0, end);
    }
}
