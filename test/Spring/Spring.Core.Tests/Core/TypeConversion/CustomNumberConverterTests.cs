#region License

/*
 * Copyright 2004 the original author or authors.
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
using System.Globalization;

using NUnit.Framework;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Unit tests for the CustomNumberConverter class.
    /// </summary>
    [TestFixture]
    public class CustomNumberConverterTests
    {
        [Test]
        public void Instantiation()
        {
            CustomNumberConverter verter
                = new CustomNumberConverter(typeof(int), null, true);
            // mmm, this should still pass... it aint a number though
            verter
                = new CustomNumberConverter(typeof(bool), null, true);
        }

        [Test]
        public void InstantiationWithNonPrimitiveType()
        {
            Assert.Throws<ArgumentException>(() => new CustomNumberConverter(typeof(CustomNumberConverterTests), null, true));
        }

        [Test]
        public void CanConvertFromString()
        {
            CustomNumberConverter verter = new CustomNumberConverter(typeof(int), null, true);
            Assert.IsTrue(verter.CanConvertFrom(typeof(string)));
            Assert.IsFalse(verter.CanConvertFrom(null));
        }

        [Test]
        public void ConvertsEmptyStringToZeroWhenAllowed()
        {
            CustomNumberConverter verter = new CustomNumberConverter(typeof(int), null, true);
            int actual = (int) verter.ConvertFrom(null, CultureInfo.CurrentUICulture, string.Empty);
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void ConvertFromSupportedNumericType()
        {
            Type[] numTypes = new Type[]
            {
                typeof(int),
                typeof(uint),
                typeof(short),
                typeof(ushort),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(double),
            };
            int expected = 12;
            foreach (Type numType in numTypes)
            {
                try
                {
                    CustomNumberConverter verter = new CustomNumberConverter(numType, null, true);
                    object actual = verter.ConvertFrom(null, CultureInfo.CurrentUICulture, expected.ToString());
                    Assert.AreEqual(expected, actual);
                }
                catch (Exception ex)
                {
                    Assert.Fail("Bailed when converting type '" + numType + "' : " + ex);
                }
            }
        }

        [Test]
        public void BailsOnEmptyStringWhenNotAllowed()
        {
            CustomNumberConverter verter
                = new CustomNumberConverter(typeof(int), null, false);
            Assert.Throws<FormatException>(() => verter.ConvertFrom(null, CultureInfo.CurrentUICulture, string.Empty));
        }

        [Test]
        public void ConvertFromNonSupportedNumericTypeOptionBails()
        {
            CustomNumberConverter verter
                = new CustomNumberConverter(typeof(char), null, false);
            Assert.Throws<NotSupportedException>(() => verter.ConvertFrom("12"));
        }

        [Test]
        public void ConvertFromNonSupportedOptionBails()
        {
            CustomNumberConverter verter = new CustomNumberConverter(typeof(int), null, false);
            Assert.Throws<NotSupportedException>(() => verter.ConvertFrom(12));
        }
    }
}