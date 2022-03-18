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
            Assert.AreEqual(0, fmt.Parse(null));
            Assert.IsTrue(fmt.Parse("") is double);
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
            Assert.AreEqual("25.00%", fmt.Format(0.25).Replace(" ", ""));
            Assert.AreEqual("25.34%", fmt.Format(0.2534).Replace(" ", ""));

            fmt = new PercentFormatter("sr-SP-Latn");
#if NETFRAMEWORK
            Assert.AreEqual("25,00%", fmt.Format(0.25));
            Assert.AreEqual("25,34%", fmt.Format(0.2534));
#else
            Assert.AreEqual("25,000%", fmt.Format(0.25));
            Assert.AreEqual("25,340%", fmt.Format(0.2534));
#endif
        }

        [Test]
        public void ParseUsingDefaults()
        {
            PercentFormatter fmt = new PercentFormatter("en-US");
            Assert.AreEqual(0.25, fmt.Parse("25.00 %"));
            Assert.AreEqual(0.2534, fmt.Parse("25.34 %"));

            fmt = new PercentFormatter("sr-SP-Latn");
            Assert.AreEqual(0.25, fmt.Parse("25,00%"));
            Assert.AreEqual(0.2534, fmt.Parse("25,34%"));
        }

        [Test]
        public void FormatUsingCustomSettings()
        {
            PercentFormatter fmt = new PercentFormatter("en-US");
            fmt.DecimalDigits = 0;
            fmt.PositivePattern = 1;
            Assert.AreEqual("25%", fmt.Format(0.25));
            Assert.AreEqual("25%", fmt.Format(0.2534));

            fmt = new PercentFormatter("sr-SP-Latn");
            fmt.DecimalDigits = 1;
            Assert.AreEqual("25,0%", fmt.Format(0.25));
            Assert.AreEqual("25,3%", fmt.Format(0.2534));
        }

        [Test]
        public void ParseUsingCustomSettings()
        {
            PercentFormatter fmt = new PercentFormatter("en-US");
            fmt.DecimalDigits = 0;
            fmt.PositivePattern = 1;
            Assert.AreEqual(0.25, fmt.Parse("25%"));
            Assert.AreEqual(0.2534, fmt.Parse("25.34%"));

            fmt = new PercentFormatter("sr-SP-Latn");
            fmt.DecimalDigits = 1;
            Assert.AreEqual(0.25, fmt.Parse("25,0%"));
            Assert.AreEqual(0.253, fmt.Parse("25,3%"));
        }
    }
}
