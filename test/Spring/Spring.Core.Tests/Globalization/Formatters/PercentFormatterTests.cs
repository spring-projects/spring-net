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
/// Unit tests for PercentFormatter class.
/// </summary>
/// <author>Aleksandar Seovic</author>
public class PercentFormatterTests
{
    [Test]
    public void FormatNullValue()
    {
        PercentFormatter fmt = new PercentFormatter();
        Assert.Throws<ArgumentNullException>(() => fmt.Format(null));
    }

    [Test]
    public void ParseNullOrEmptyValue()
    {
        PercentFormatter fmt = new PercentFormatter();
        Assert.That(fmt.Parse(null), Is.EqualTo(0));
        Assert.That(fmt.Parse("") is double, Is.True);
    }

    [Test]
    public void FormatNonNumber()
    {
        PercentFormatter fmt = new PercentFormatter();
        Assert.Throws<ArgumentException>(() => fmt.Format("not a number"));
    }

    [Test]
    [Platform("Win")]
    public void FormatUsingDefaults()
    {
        PercentFormatter fmt = new PercentFormatter("en-US");
        Assert.That(fmt.Format(0.25).Replace(" ", ""), Is.EqualTo("25.00%"));
        Assert.That(fmt.Format(0.2534).Replace(" ", ""), Is.EqualTo("25.34%"));

        fmt = new PercentFormatter("sr-SP-Latn");
#if NETFRAMEWORK
        Assert.That(fmt.Format(0.25), Is.EqualTo("25,00%"));
        Assert.That(fmt.Format(0.2534), Is.EqualTo("25,34%"));
#else
        Assert.That(fmt.Format(0.25), Is.EqualTo("25,000%"));
        Assert.That(fmt.Format(0.2534), Is.EqualTo("25,340%"));
#endif
    }

    [Test]
    public void ParseUsingDefaults()
    {
        PercentFormatter fmt = new PercentFormatter("en-US");
        Assert.That(fmt.Parse("25.00 %"), Is.EqualTo(0.25));
        Assert.That(fmt.Parse("25.34 %"), Is.EqualTo(0.2534));

        fmt = new PercentFormatter("sr-SP-Latn");
        Assert.That(fmt.Parse("25,00%"), Is.EqualTo(0.25));
        Assert.That(fmt.Parse("25,34%"), Is.EqualTo(0.2534));
    }

    [Test]
    public void FormatUsingCustomSettings()
    {
        PercentFormatter fmt = new PercentFormatter("en-US");
        fmt.DecimalDigits = 0;
        fmt.PositivePattern = 1;
        Assert.That(fmt.Format(0.25), Is.EqualTo("25%"));
        Assert.That(fmt.Format(0.2534), Is.EqualTo("25%"));

        fmt = new PercentFormatter("sr-SP-Latn");
        fmt.DecimalDigits = 1;
        Assert.That(fmt.Format(0.25), Is.EqualTo("25,0%"));
        Assert.That(fmt.Format(0.2534), Is.EqualTo("25,3%"));
    }

    [Test]
    public void ParseUsingCustomSettings()
    {
        PercentFormatter fmt = new PercentFormatter("en-US");
        fmt.DecimalDigits = 0;
        fmt.PositivePattern = 1;
        Assert.That(fmt.Parse("25%"), Is.EqualTo(0.25));
        Assert.That(fmt.Parse("25.34%"), Is.EqualTo(0.2534));

        fmt = new PercentFormatter("sr-SP-Latn");
        fmt.DecimalDigits = 1;
        Assert.That(fmt.Parse("25,0%"), Is.EqualTo(0.25));
        Assert.That(fmt.Parse("25,3%"), Is.EqualTo(0.253));
    }
}
