/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using NUnit.Framework;

namespace Spring.Globalization.Formatters;

/// <summary>
/// Unit tests for NumberFormatter class.
/// </summary>
/// <author>Aleksandar Seovic</author>
public class NumberFormatterTests
{
    [Test]
    public void FormatNullValue()
    {
        NumberFormatter fmt = new NumberFormatter();
        Assert.Throws<ArgumentNullException>(() => fmt.Format(null));
    }

    [Test]
    public void ParseNullOrEmptyValue()
    {
        NumberFormatter fmt = new NumberFormatter();
        Assert.That(fmt.Parse(null), Is.EqualTo(0));
        Assert.That(fmt.Parse("") is double, Is.True);
    }

    [Test]
    public void FormatNonNumber()
    {
        NumberFormatter fmt = new NumberFormatter();
        Assert.Throws<ArgumentException>(() => fmt.Format("not a number"));
    }

    [Test]
    [Platform("Win")]
    public void FormatUsingDefaults()
    {
        NumberFormatter fmt = new NumberFormatter("en-US");
        Assert.That(fmt.Format(1234), Is.EqualTo("1,234.00"));
        Assert.That(fmt.Format(1234.56), Is.EqualTo("1,234.56"));
        Assert.That(fmt.Format(-1234), Is.EqualTo("-1,234.00"));
        Assert.That(fmt.Format(-1234.56), Is.EqualTo("-1,234.56"));

        fmt = new NumberFormatter("sr-SP-Latn");
#if NETFRAMEWORK
        Assert.That(fmt.Format(1234), Is.EqualTo("1.234,00"));
        Assert.That(fmt.Format(1234.56), Is.EqualTo("1.234,56"));
        Assert.That(fmt.Format(-1234), Is.EqualTo("-1.234,00"));
        Assert.That(fmt.Format(-1234.56), Is.EqualTo("-1.234,56"));
#else
        Assert.That(fmt.Format(1234), Is.EqualTo("1.234,000"));
        Assert.That(fmt.Format(1234.56), Is.EqualTo("1.234,560"));
        Assert.That(fmt.Format(-1234), Is.EqualTo("-1.234,000"));
        Assert.That(fmt.Format(-1234.56), Is.EqualTo("-1.234,560"));
#endif
    }

    [Test]
    [Platform("Win")]
    public void ParseUsingDefaults()
    {
        NumberFormatter fmt = new NumberFormatter("en-US");
        Assert.That(fmt.Parse("1,234.00"), Is.EqualTo(1234));
        Assert.That(fmt.Parse("1,234.56"), Is.EqualTo(1234.56));
        Assert.That(fmt.Parse("-1,234.00"), Is.EqualTo(-1234));
        Assert.That(fmt.Parse("-1,234.56"), Is.EqualTo(-1234.56));

        fmt = new NumberFormatter("sr-SP-Latn");
        Assert.That(fmt.Parse("1.234,00"), Is.EqualTo(1234));
        Assert.That(fmt.Parse("1.234,56"), Is.EqualTo(1234.56));
        Assert.That(fmt.Parse("-1.234,00"), Is.EqualTo(-1234));
        Assert.That(fmt.Parse("-1.234,56"), Is.EqualTo(-1234.56));
    }

    [Test]
    [Platform("Win")]
    public void FormatUsingCustomSettings()
    {
        NumberFormatter fmt = new NumberFormatter("en-US");
        fmt.DecimalDigits = 0;
        fmt.NegativePattern = 0;
        Assert.That(fmt.Format(1234), Is.EqualTo("1,234"));
        Assert.That(fmt.Format(1234.56), Is.EqualTo("1,235"));
        Assert.That(fmt.Format(-1234), Is.EqualTo("(1,234)"));
        Assert.That(fmt.Format(-1234.56), Is.EqualTo("(1,235)"));

        fmt = new NumberFormatter("sr-SP-Cyrl");
        fmt.GroupSizes = new int[] { 1, 2 };
        fmt.GroupSeparator = "'";

#if NETFRAMEWORK
        Assert.That(fmt.Format(1234), Is.EqualTo("1'23'4,00"));
        Assert.That(fmt.Format(1234.56), Is.EqualTo("1'23'4,56"));
        Assert.That(fmt.Format(-1234), Is.EqualTo("-1'23'4,00"));
        Assert.That(fmt.Format(-1234.56), Is.EqualTo("-1'23'4,56"));
#else
        Assert.That(fmt.Format(1234), Is.EqualTo("1'23'4,000"));
        Assert.That(fmt.Format(1234.56), Is.EqualTo("1'23'4,560"));
        Assert.That(fmt.Format(-1234), Is.EqualTo("-1'23'4,000"));
        Assert.That(fmt.Format(-1234.56), Is.EqualTo("-1'23'4,560"));
#endif
    }

    [Test]
    public void ParseUsingCustomSettings()
    {
        NumberFormatter fmt = new NumberFormatter("en-US");
        fmt.DecimalDigits = 0;
        fmt.NegativePattern = 0;
        Assert.That(fmt.Parse("1,234"), Is.EqualTo(1234));
        Assert.That(fmt.Parse("1,234.56"), Is.EqualTo(1234.56));
        Assert.That(fmt.Parse("(1,234)"), Is.EqualTo(-1234));
        Assert.That(fmt.Parse("(1,234.56)"), Is.EqualTo(-1234.56));

        fmt = new NumberFormatter("sr-SP-Cyrl");
        fmt.GroupSizes = new int[] { 1, 2 };
        fmt.GroupSeparator = "'";
        Assert.That(fmt.Parse("1'23'4,00"), Is.EqualTo(1234));
        Assert.That(fmt.Parse("1'23'4,56"), Is.EqualTo(1234.56));
        Assert.That(fmt.Parse("-1'23'4,00"), Is.EqualTo(-1234));
        Assert.That(fmt.Parse("-1'23'4,56"), Is.EqualTo(-1234.56));
    }
}
