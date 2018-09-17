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
using System.Drawing;
using NUnit.Framework;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Unit tests for the RGBColorConverter class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class RGBColorConverterTests
    {
        [Test]
        public void ConvertFromRGB()
        {
            Color expected = Color.BlanchedAlmond;
            RGBColorConverter converter = new RGBColorConverter();
            Color actual =
                (Color) converter.ConvertFrom(String.Format("{0}, {1}, {2}", expected.R, expected.G, expected.B));
            Assert.AreEqual(expected.A, actual.A);
            Assert.AreEqual(expected.R, actual.R);
            Assert.AreEqual(expected.G, actual.G);
            Assert.AreEqual(expected.B, actual.B);
        }

        [Test]
        public void ConvertFromCommaSeparatedListWithNotEnoughValues()
        {
            RGBColorConverter converter = new RGBColorConverter();
            Assert.Throws<FormatException>(() => converter.ConvertFrom("255, 235"));
        }

        [Test]
        public void ConvertFromCommaSeparatedListWithOutOfRangeValue()
        {
            RGBColorConverter converter = new RGBColorConverter();
            Assert.Throws<FormatException>(() => converter.ConvertFrom("255, 235, 4567"));
        }

        [Test]
        public void ConvertFromNullReference()
        {
            RGBColorConverter vrt = new RGBColorConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(null));
        }

        [Test]
        public void ConvertFromEmptyString()
        {
            RGBColorConverter vrt = new RGBColorConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(string.Empty));
        }

#if NETFRAMEWORK
        [Test]
        public void ConvertFromGarbageBails()
        {
            RGBColorConverter vrt = new RGBColorConverter();
            Assert.Throws<FormatException>(() => vrt.ConvertFrom("*&&%%^£"));
        }

        [Test]
        public void ConvertFromName()
        {
            Color expected = Color.BlanchedAlmond;
            RGBColorConverter vrt = new RGBColorConverter();
            Color actual = (Color) vrt.ConvertFrom(expected.Name);
            Assert.AreEqual(expected, actual);
        }
#endif

        [Test]
        public void ConvertFromNonSupportedOptionBails()
        {
            RGBColorConverter vrt = new RGBColorConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(12));
        }

        [Test]
        public void ConvertFromARGB()
        {
            Color expected = Color.BlanchedAlmond;
            RGBColorConverter converter = new RGBColorConverter();
            Color actual =
                (Color) converter.ConvertFrom(String.Format("{0}, {1}, {2}, {3}", expected.A, expected.R, expected.G, expected.B));
            Assert.AreEqual(expected.A, actual.A);
            Assert.AreEqual(expected.R, actual.R);
            Assert.AreEqual(expected.G, actual.G);
            Assert.AreEqual(expected.B, actual.B);
        }

        [Test]
        public void CanConvertFrom()
        {
            RGBColorConverter vrt = new RGBColorConverter();
            Assert.IsTrue(vrt.CanConvertFrom(typeof (string)), "Conversion from a string instance must be supported.");
            Assert.IsFalse(vrt.CanConvertFrom(typeof (int)));
        }
    }
}