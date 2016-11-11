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
	/// Unit tests for IntegerFormatter class.
	/// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public class IntegerFormatterTests
	{
        [Test]
        public void FormatNullValue()
        {
            IntegerFormatter fmt = new IntegerFormatter();
            Assert.Throws<ArgumentNullException>(() => fmt.Format(null));
        }

        [Test]
        public void ParseNullOrEmptyValue()
        {
            IntegerFormatter fmt = new IntegerFormatter();
            Assert.AreEqual( 0, fmt.Parse(null));
            Assert.AreEqual( 0, fmt.Parse(string.Empty) );
        }

        [Test]
        public void FormatNonNumber()
        {
            IntegerFormatter fmt = new IntegerFormatter();
            Assert.Throws<ArgumentException>(() => fmt.Format("not a number"));
        }

        [Test]
        public void FormatUsingDefaults()
        {
            IntegerFormatter fmt = new IntegerFormatter();
            Assert.AreEqual("1234", fmt.Format(1234));
            Assert.AreEqual("-1234", fmt.Format(-1234));
        }

        [Test]
        public void ParseUsingDefaults()
        {
            IntegerFormatter fmt = new IntegerFormatter();
            Assert.AreEqual(1234, fmt.Parse("1234"));
            Assert.AreEqual(-1234, fmt.Parse("-1234"));
        }

        [Test]
        public void FormatUsingCustomSettings()
        {
            IntegerFormatter fmt = new IntegerFormatter("{0:00000}");
            Assert.AreEqual("01234", fmt.Format(1234));
            Assert.AreEqual("-01234", fmt.Format(-1234));

            fmt = new IntegerFormatter("{0,10}");
            Assert.AreEqual("      1234", fmt.Format(1234));

            fmt = new IntegerFormatter("{0,-10}");
            Assert.AreEqual("1234      ", fmt.Format(1234));

            fmt = new IntegerFormatter("{0:(###) ###-####}");
            Assert.AreEqual("(813) 555-4034", fmt.Format(8135554034));
        }
    }
}
