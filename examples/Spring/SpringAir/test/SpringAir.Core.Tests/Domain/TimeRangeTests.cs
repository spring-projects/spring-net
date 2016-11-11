#region Licence

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.ComponentModel;
using NUnit.Framework;

#endregion

namespace SpringAir.Domain
{
	/// <summary>
	/// Unit tests for the TimeRange class.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: TimeRangeTests.cs,v 1.1 2005/09/27 21:12:31 springboy Exp $</version>
	[TestFixture]
    public sealed class TimeRangeTests
    {
        [Test]
        public void DefaultTypeConverterIsSet()
        {
            TypeConverter vrtr = TypeDescriptor.GetConverter(typeof(TimeRange));
            Assert.IsNotNull(vrtr, "Must not be null, default converter must be returned.");
            Assert.AreEqual(typeof(TimeRange.TimeRangeTypeConverter), vrtr.GetType(), "Wrong type of default converter returned.");
        }
	}

    /// <summary>
    /// Unit tests for the TimeRange.TimeRangeTypeConverter class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class TimeRangeTimeRangeTypeConverterTests
    {
        [Test]
        public void ConvertFromSunnyDay()
        {
            TimeRange expectedRange = new TimeRange(12, 9);
            TimeRange.TimeRangeTypeConverter vrtr = new TimeRange.TimeRangeTypeConverter();
            TimeRange range = (TimeRange) vrtr.ConvertFrom("[12-9]");
            Assert.AreEqual(expectedRange, range);
        }

        [Test]
        public void ConvertFromSunnyDayWithLotsOfExtraneousWhitespace()
        {
            TimeRange expectedRange = new TimeRange(12, 9);
            TimeRange.TimeRangeTypeConverter vrtr = new TimeRange.TimeRangeTypeConverter();
            TimeRange range = (TimeRange) vrtr.ConvertFrom("[ 12 - 9 ]");
            Assert.AreEqual(expectedRange, range);
        }

        [Test]
        public void ConvertFromWithOutOfShortRangeException()
        {
            TimeRange.TimeRangeTypeConverter vrtr = new TimeRange.TimeRangeTypeConverter();
            Assert.Throws<FormatException>(() => vrtr.ConvertFrom("[ 1287876 - 9 ]"));
        }

        [Test]
        public void ConvertToSunnyDay()
        {
            string expectedRange = "[12 - 9]";
            TimeRange.TimeRangeTypeConverter vrtr = new TimeRange.TimeRangeTypeConverter();
            string range = (string) vrtr.ConvertTo(new TimeRange(12, 9), typeof(string));
            Assert.AreEqual(expectedRange, range);
        }
    }
}
