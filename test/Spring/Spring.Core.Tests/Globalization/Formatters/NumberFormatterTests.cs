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
            Assert.AreEqual(0, fmt.Parse(null));
            Assert.IsTrue(fmt.Parse("") is double);
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
            Assert.AreEqual("1,234.00", fmt.Format(1234));
            Assert.AreEqual("1,234.56", fmt.Format(1234.56));
            Assert.AreEqual("-1,234.00", fmt.Format(-1234));
            Assert.AreEqual("-1,234.56", fmt.Format(-1234.56));

            fmt = new NumberFormatter("sr-SP-Latn");
#if NETFRAMEWORK
            Assert.AreEqual("1.234,00", fmt.Format(1234));
            Assert.AreEqual("1.234,56", fmt.Format(1234.56));
            Assert.AreEqual("-1.234,00", fmt.Format(-1234));
            Assert.AreEqual("-1.234,56", fmt.Format(-1234.56));
#else
            Assert.AreEqual("1.234,000", fmt.Format(1234));
            Assert.AreEqual("1.234,560", fmt.Format(1234.56));
            Assert.AreEqual("-1.234,000", fmt.Format(-1234));
            Assert.AreEqual("-1.234,560", fmt.Format(-1234.56));
#endif
        }

        [Test]
        [Platform("Win")]
        public void ParseUsingDefaults()
        {
            NumberFormatter fmt = new NumberFormatter("en-US");
            Assert.AreEqual(1234, fmt.Parse("1,234.00"));
            Assert.AreEqual(1234.56, fmt.Parse("1,234.56"));
            Assert.AreEqual(-1234, fmt.Parse("-1,234.00"));
            Assert.AreEqual(-1234.56, fmt.Parse("-1,234.56"));

            fmt = new NumberFormatter("sr-SP-Latn");
            Assert.AreEqual(1234, fmt.Parse("1.234,00"));
            Assert.AreEqual(1234.56, fmt.Parse("1.234,56"));
            Assert.AreEqual(-1234, fmt.Parse("-1.234,00"));
            Assert.AreEqual(-1234.56, fmt.Parse("-1.234,56"));
        }

        [Test]
        [Platform("Win")]
        public void FormatUsingCustomSettings()
        {
            NumberFormatter fmt = new NumberFormatter("en-US");
            fmt.DecimalDigits = 0;
            fmt.NegativePattern = 0;
            Assert.AreEqual("1,234", fmt.Format(1234));
            Assert.AreEqual("1,235", fmt.Format(1234.56));
            Assert.AreEqual("(1,234)", fmt.Format(-1234));
            Assert.AreEqual("(1,235)", fmt.Format(-1234.56));

            fmt = new NumberFormatter("sr-SP-Cyrl");
            fmt.GroupSizes = new int[] {1, 2};
            fmt.GroupSeparator = "'";
            
#if NETFRAMEWORK
            Assert.AreEqual("1'23'4,00", fmt.Format(1234));
            Assert.AreEqual("1'23'4,56", fmt.Format(1234.56));
            Assert.AreEqual("-1'23'4,00", fmt.Format(-1234));
            Assert.AreEqual("-1'23'4,56", fmt.Format(-1234.56));
#else
            Assert.AreEqual("1'23'4,000", fmt.Format(1234));
            Assert.AreEqual("1'23'4,560", fmt.Format(1234.56));
            Assert.AreEqual("-1'23'4,000", fmt.Format(-1234));
            Assert.AreEqual("-1'23'4,560", fmt.Format(-1234.56));
#endif
        }

        [Test]
        public void ParseUsingCustomSettings()
        {
            NumberFormatter fmt = new NumberFormatter("en-US");
            fmt.DecimalDigits = 0;
            fmt.NegativePattern = 0;
            Assert.AreEqual(1234, fmt.Parse("1,234"));
            Assert.AreEqual(1234.56, fmt.Parse("1,234.56"));
            Assert.AreEqual(-1234, fmt.Parse("(1,234)"));
            Assert.AreEqual(-1234.56, fmt.Parse("(1,234.56)"));

            fmt = new NumberFormatter("sr-SP-Cyrl");
            fmt.GroupSizes = new int[] {1, 2};
            fmt.GroupSeparator = "'";
            Assert.AreEqual(1234, fmt.Parse("1'23'4,00"));
            Assert.AreEqual(1234.56, fmt.Parse("1'23'4,56"));
            Assert.AreEqual(-1234, fmt.Parse("-1'23'4,00"));
            Assert.AreEqual(-1234.56, fmt.Parse("-1'23'4,56"));
        }

    }
}
