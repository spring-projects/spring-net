/*
 * Copyright 2002-2026 the original author or authors.
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

using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Spring.Core.TypeConversion;

/// <summary>
/// Unit tests for the LogLevelConverter class.
/// </summary>
[TestFixture]
public sealed class LogLevelConverterTests
{
    private readonly LogLevelConverter converter = new LogLevelConverter();

    [TestCase("All", LogLevel.Trace)]
    [TestCase("Info", LogLevel.Information)]
    [TestCase("info", LogLevel.Information)]
    [TestCase(" Info ", LogLevel.Information)]
    [TestCase("Warn", LogLevel.Warning)]
    [TestCase("WARN", LogLevel.Warning)]
    [TestCase("Fatal", LogLevel.Critical)]
    [TestCase("Off", LogLevel.None)]
    public void ConvertsLegacyCommonLoggingLevelNames(string text, LogLevel expected)
    {
        Assert.That(converter.ConvertFrom(text), Is.EqualTo(expected));
    }

    [TestCase("Trace", LogLevel.Trace)]
    [TestCase("Debug", LogLevel.Debug)]
    [TestCase("Information", LogLevel.Information)]
    [TestCase("information", LogLevel.Information)]
    [TestCase("Warning", LogLevel.Warning)]
    [TestCase("Error", LogLevel.Error)]
    [TestCase("Critical", LogLevel.Critical)]
    [TestCase("None", LogLevel.None)]
    public void ConvertsCanonicalLevelNames(string text, LogLevel expected)
    {
        Assert.That(converter.ConvertFrom(text), Is.EqualTo(expected));
    }

    [Test]
    public void ConvertsNumericStrings()
    {
        Assert.That(converter.ConvertFrom("2"), Is.EqualTo(LogLevel.Information));
    }

    [Test]
    public void ThrowsFormatExceptionForUnknownLevelName()
    {
        Assert.Throws<FormatException>(() => converter.ConvertFrom("Verbose"));
    }

    [Test]
    public void CanConvertFromString()
    {
        Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
    }
}
