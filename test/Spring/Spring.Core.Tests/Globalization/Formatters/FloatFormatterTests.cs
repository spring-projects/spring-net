#region License

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

#endregion

using System;

using NUnit.Framework;

namespace Spring.Globalization.Formatters
{
	/// <summary>
	/// Unit tests for FloatFormatter class.
	/// </summary>
    /// <author>Aleksandar Seovic</author>
    public class FloatFormatterTests
	{
        [Test]
        public void FormatNullValue()
        {
            FloatFormatter fmt = new FloatFormatter();
            Assert.Throws<ArgumentNullException>(() => fmt.Format(null));
        }

        [Test]
        public void ParseNullOrEmptyValue()
        {
            FloatFormatter fmt = new FloatFormatter();
            Assert.AreEqual(0, fmt.Parse(null));
            Assert.IsTrue(fmt.Parse("") is double);
        }

        [Test]
        public void FormatNonNumber()
        {
            FloatFormatter fmt = new FloatFormatter();
            Assert.Throws<ArgumentException>(() => fmt.Format("not a number"));
        }

        [Test]
        [Platform("Win")]
        public void FormatUsingDefaults()
        {
            FloatFormatter fmt = new FloatFormatter(FloatFormatter.DefaultFormat, "en-US");
            Assert.AreEqual("1234.00", fmt.Format(1234));
            Assert.AreEqual("-1234.00", fmt.Format(-1234));

            fmt = new FloatFormatter(FloatFormatter.DefaultFormat, "sr-SP-Latn");
#if NETFRAMEWORK 
            Assert.AreEqual("1234,00", fmt.Format(1234));
            Assert.AreEqual("-1234,00", fmt.Format(-1234));
#else
            Assert.AreEqual("1234,000", fmt.Format(1234));
            Assert.AreEqual("-1234,000", fmt.Format(-1234));
#endif
        }

        [Test]
        public void ParseUsingDefaults()
        {
            FloatFormatter fmt = new FloatFormatter(FloatFormatter.DefaultFormat, "en-US");
            Assert.AreEqual(1234.56, fmt.Parse("1234.56"));
            Assert.AreEqual(-1234, fmt.Parse("-1234"));
            Assert.AreEqual(1234.56, fmt.Parse("1.23456e+003"));
            Assert.AreEqual(-1234, fmt.Parse("-1.234e+003"));

            fmt = new FloatFormatter(FloatFormatter.DefaultFormat, "sr-SP-Cyrl");
            Assert.AreEqual(1234.56, fmt.Parse("1234,56"));
            Assert.AreEqual(-1234, fmt.Parse("-1234"));
            Assert.AreEqual(1234.56, fmt.Parse("1,23456e+003"));
            Assert.AreEqual(-1234, fmt.Parse("-1,234e+003"));
        }

        [Test]
        public void FormatUsingCustomSettings()
        {
            FloatFormatter fmt = new FloatFormatter("{0:e3}", "en-US");
            Assert.AreEqual("1.234e+003", fmt.Format(1234));
            Assert.AreEqual("-1.234e+003", fmt.Format(-1234));
            Assert.AreEqual("1.235e+003", fmt.Format(1234.56));
            Assert.AreEqual("-1.235e+003", fmt.Format(-1234.56));
        }
    }
}
