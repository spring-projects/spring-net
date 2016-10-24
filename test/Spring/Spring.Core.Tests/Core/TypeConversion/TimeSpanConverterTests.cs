#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region Imports

using System;

using NUnit.Framework;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Unit tests for the TimeSpanConverter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public sealed class TimeSpanConverterTests
    {
        [Test]
        public void ConvertFromNullReference()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            Assert.Throws<NotSupportedException>(() => tsc.ConvertFrom(null));
        }

        [Test]
        public void ConvertFromNonSupportedOptionBails()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            Assert.Throws<NotSupportedException>(() => tsc.ConvertFrom(12));
        }

        [Test]
        public void ConvertFromStringMalformed()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            Assert.Throws<FormatException>(() => tsc.ConvertFrom("15a"));
        }

        [Test]
        public void BaseConvertFrom()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            object timeSpan = tsc.ConvertFrom("00:00:10");
            Assert.IsNotNull(timeSpan);
            Assert.IsTrue(timeSpan is TimeSpan);
            Assert.AreEqual(TimeSpan.FromSeconds(10), (TimeSpan)timeSpan);
        }

        [Test]
        public void ConvertFrom()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            object timeSpan = tsc.ConvertFrom("1000");
            Assert.IsNotNull(timeSpan);
            Assert.IsTrue(timeSpan is TimeSpan);
            Assert.AreEqual(TimeSpan.Parse("1000"), (TimeSpan)timeSpan);
        }

        [Test]
        public void ConvertFromStringWithMilliSecondSpecifier()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            object timeSpan = tsc.ConvertFrom("100ms");
            Assert.IsNotNull(timeSpan);
            Assert.IsTrue(timeSpan is TimeSpan);
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), (TimeSpan)timeSpan);
        }

        [Test]
        public void ConvertFromStringWithSecondSpecifier()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            object timeSpan = tsc.ConvertFrom("10s");
            Assert.IsNotNull(timeSpan);
            Assert.IsTrue(timeSpan is TimeSpan);
            Assert.AreEqual(TimeSpan.FromSeconds(10), (TimeSpan)timeSpan);
        }

        [Test]
        public void ConvertFromStringWithMinuteSpecifier()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            object timeSpan = tsc.ConvertFrom("2m");
            Assert.IsNotNull(timeSpan);
            Assert.IsTrue(timeSpan is TimeSpan);
            Assert.AreEqual(TimeSpan.FromMinutes(2), (TimeSpan)timeSpan);
        }

        [Test]
        public void ConvertFromStringWithHourSpecifier()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            
            object timeSpan = tsc.ConvertFrom("1H");
            Assert.IsNotNull(timeSpan);
            Assert.IsTrue(timeSpan is TimeSpan);
            Assert.AreEqual(TimeSpan.FromHours(1), (TimeSpan)timeSpan);

            tsc.ConvertFrom("1h");
            Assert.IsNotNull(timeSpan);
            Assert.IsTrue(timeSpan is TimeSpan);
            Assert.AreEqual(TimeSpan.FromHours(1), (TimeSpan)timeSpan);
        }

        [Test]
        public void ConvertFromStringWithDaySpecifier()
        {
            TimeSpanConverter tsc = new TimeSpanConverter();
            object timeSpan = tsc.ConvertFrom("1d");
            Assert.IsNotNull(timeSpan);
            Assert.IsTrue(timeSpan is TimeSpan);
            Assert.AreEqual(TimeSpan.FromDays(1), (TimeSpan)timeSpan);
        }
    }
}