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
    /// Unit tests for CurrencyFormatter class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class CurrencyFormatterTests
    {
        [Test]
        public void FormatNullValue()
        {
            CurrencyFormatter fmt = new CurrencyFormatter();
            Assert.Throws<ArgumentNullException>(() => fmt.Format(null));
        }

        [Test]
        public void ParseNullOrEmptyValue()
        {
            CurrencyFormatter fmt = new CurrencyFormatter();
            Assert.AreEqual(0, fmt.Parse(null));
            Assert.IsTrue(fmt.Parse("") is double);
        }

        [Test]
        public void FormatNonNumber()
        {
            CurrencyFormatter fmt = new CurrencyFormatter();
            Assert.Throws<ArgumentException>(() => fmt.Format("not a number"));
        }

        [Test]
        [Platform("Win")]
        public void FormatUsingDefaults()
        {
            CurrencyFormatter fmt = new CurrencyFormatter("en-US");
            Assert.AreEqual("$1,234.00", fmt.Format(1234));
            Assert.AreEqual("$1,234.56", fmt.Format(1234.56));
            Assert.AreEqual("($1,234.00)", fmt.Format(-1234));
            Assert.AreEqual("($1,234.56)", fmt.Format(-1234.56));

            fmt = new CurrencyFormatter(CultureInfoUtils.SerbianLatinCultureName);

            Assert.AreEqual("1.234 RSD", fmt.Format(1234));
            Assert.AreEqual("1.235 RSD", fmt.Format(1234.56));
            Assert.AreEqual("-1.234 RSD", fmt.Format(-1234));
            Assert.AreEqual("-1.235 RSD", fmt.Format(-1234.56));

            fmt = new CurrencyFormatter(CultureInfoUtils.SerbianCyrillicCultureName);

#if NETFRAMEWORK
            Assert.AreEqual("1.234,00 дин.", fmt.Format(1234));
            Assert.AreEqual("1.234,56 дин.", fmt.Format(1234.56));
            Assert.AreEqual("-1.234,00 дин.", fmt.Format(-1234));
            Assert.AreEqual("-1.234,56 дин.", fmt.Format(-1234.56));
#else
            Assert.AreEqual("1.234 RSD", fmt.Format(1234));
            Assert.AreEqual("1.235 RSD", fmt.Format(1234.56));
            Assert.AreEqual("-1.234 RSD", fmt.Format(-1234));
            Assert.AreEqual("-1.235 RSD", fmt.Format(-1234.56));
#endif
        }

        [Test]
        [Platform("Win")]
        public void ParseUsingDefaults()
        {
            CurrencyFormatter fmt = new CurrencyFormatter("en-US");
            Assert.AreEqual(1234, fmt.Parse("$1,234.00"));
            Assert.AreEqual(1234.56, fmt.Parse("$1,234.56"));
            Assert.AreEqual(-1234, fmt.Parse("($1,234.00)"));
            Assert.AreEqual(-1234.56, fmt.Parse("($1,234.56)"));

            fmt = new CurrencyFormatter(CultureInfoUtils.SerbianLatinCultureName);

            Assert.AreEqual(1234, fmt.Parse("1.234 RSD"));
            Assert.AreEqual(-1234, fmt.Parse("-1.234 RSD"));

            fmt = new CurrencyFormatter(CultureInfoUtils.SerbianCyrillicCultureName);

#if NETFRAMEWORK
            Assert.AreEqual(1234, fmt.Parse("1.234,00 дин."));
            Assert.AreEqual(1234.56, fmt.Parse("1.234,56 дин."));
            Assert.AreEqual(-1234, fmt.Parse("-1.234,00 дин."));
            Assert.AreEqual(-1234.56, fmt.Parse("-1.234,56 дин."));
#endif
        }

        [Test]
        [Platform("Win")]
        public void FormatUsingCustomSettings()
        {
            CurrencyFormatter fmt = new CurrencyFormatter("en-US");
            fmt.DecimalDigits = 0;
            fmt.NegativePattern = 1;
            Assert.AreEqual("$1,234", fmt.Format(1234));
            Assert.AreEqual("$1,235", fmt.Format(1234.56));
            Assert.AreEqual("-$1,234", fmt.Format(-1234));
            Assert.AreEqual("-$1,235", fmt.Format(-1234.56));

            fmt = new CurrencyFormatter(CultureInfoUtils.SerbianLatinCultureName);
            fmt.PositivePattern = 1;
            fmt.CurrencySymbol = "din";

            Assert.AreEqual("1.234din", fmt.Format(1234));
            Assert.AreEqual("1.235din", fmt.Format(1234.56));
            Assert.AreEqual("-1.234 din", fmt.Format(-1234));
            Assert.AreEqual("-1.235 din", fmt.Format(-1234.56));

            fmt = new CurrencyFormatter(CultureInfoUtils.SerbianCyrillicCultureName);
            fmt.GroupSizes = new int[] { 1, 2 };
            fmt.GroupSeparator = "'";

#if NETFRAMEWORK
            Assert.AreEqual("1'23'4,00 дин.", fmt.Format(1234));
            Assert.AreEqual("1'23'4,56 дин.", fmt.Format(1234.56));
            Assert.AreEqual("-1'23'4,00 дин.", fmt.Format(-1234));
            Assert.AreEqual("-1'23'4,56 дин.", fmt.Format(-1234.56));
#else
            Assert.AreEqual("1'23'4 RSD", fmt.Format(1234));
            Assert.AreEqual("1'23'5 RSD", fmt.Format(1234.56));
            Assert.AreEqual("-1'23'4 RSD", fmt.Format(-1234));
            Assert.AreEqual("-1'23'5 RSD", fmt.Format(-1234.56));
#endif
        }

        [Test]
        [Platform("Win")]
        public void ParseUsingCustomSettings()
        {
            CurrencyFormatter fmt = new CurrencyFormatter("en-US");
            fmt.DecimalDigits = 0;
            fmt.NegativePattern = 1;
            Assert.AreEqual(1234, fmt.Parse("$1,234"));
            Assert.AreEqual(1234.56, fmt.Parse("$1,234.56"));
            Assert.AreEqual(-1234, fmt.Parse("-$1,234"));
            Assert.AreEqual(-1234.56, fmt.Parse("-$1,234.56"));

            fmt = new CurrencyFormatter("sr-SP-Latn");
            fmt.PositivePattern = 1;
            fmt.CurrencySymbol = "din";
            Assert.AreEqual(1234, fmt.Parse("1.234,00din"));
            Assert.AreEqual(1234.56, fmt.Parse("1.234,56din"));
            Assert.AreEqual(-1234, fmt.Parse("-1.234,00 din"));
            Assert.AreEqual(-1234.56, fmt.Parse("-1.234,56 din"));

            fmt = new CurrencyFormatter(CultureInfoUtils.SerbianCyrillicCultureName);
            fmt.GroupSizes = new int[] { 1, 2 };
            fmt.GroupSeparator = "'";

#if NETFRAMEWORK
            Assert.AreEqual(1234, fmt.Parse("1'23'4,00 дин."));
            Assert.AreEqual(1234.56, fmt.Parse("1'23'4,56 дин."));
            Assert.AreEqual(-1234, fmt.Parse("-1'23'4,00 дин."));
            Assert.AreEqual(-1234.56, fmt.Parse("-1'23'4,56 дин."));
#endif
        }
    }
}
