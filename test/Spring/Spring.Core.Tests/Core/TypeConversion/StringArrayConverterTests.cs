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
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Spring.Util;

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Unit tests for the StringArrayConverter class.
	/// </summary>
	/// <author>Rick Evans</author>
    [TestFixture]
    public sealed class StringArrayConverterTests
    {
        [Test]
        public void CanConvertFrom()
        {
            StringArrayConverter vrt = new StringArrayConverter();
            Assert.IsTrue(vrt.CanConvertFrom(typeof (string)), "Conversion from a string instance must be supported.");
            Assert.IsFalse(vrt.CanConvertFrom(null));
        }

        [Test]
        public void ConvertFrom()
        {
            object[] expected = new object[] {"1", "Foo", "3"};
            StringArrayConverter vrt = new StringArrayConverter();
            object actual = vrt.ConvertFrom("1,Foo,3");
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof (string[]), actual.GetType());
            Assert.AreEqual(3, ((string[]) actual).Length, "Wrong number of elements in the resulting array.");
            Assert.IsTrue(ArrayUtils.AreEqual(expected, (string[]) actual),
                "Individual array elements not correctly converted.");
        }

        [Test]
        public void ConvertFromPreservesExtraneousWhitespace()
        {
            object[] expected = new object[] {"1 ", " Foo ", " 3"};
            StringArrayConverter vrt = new StringArrayConverter();
            object actual = vrt.ConvertFrom("1 , Foo , 3");
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof (string[]), actual.GetType());
            Assert.IsTrue(ArrayUtils.AreEqual(expected, (string[]) actual),
                "Individual array elements not correctly converted (check the whitespace?).");
        }

        [Test]
        public void ConvertFromNullReference()
        {
            StringArrayConverter vrt = new StringArrayConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(null));
        }

        [Test]
        public void ConvertFromNonSupportedOptionBails()
        {
            StringArrayConverter vrt = new StringArrayConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(12));
        }

        [Test]
        public void EnsureCultureListSeparatorIsIgnored()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                CultureInfo frenchCulture = new CultureInfo("fr-FR");
                Thread.CurrentThread.CurrentCulture = frenchCulture;
                object[] expected = new object[] {"1", "Foo", "3"};
                StringArrayConverter vrt = new StringArrayConverter();
                // France uses the ';' (semi-colon) to separate list items...
                object actual = vrt.ConvertFrom("1,Foo,3");
                Assert.IsNotNull(actual);
                Assert.AreEqual(typeof (string[]), actual.GetType());
                Assert.IsTrue(ArrayUtils.AreEqual(expected, (string[]) actual),
                              "Individual array elements not correctly converted.");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

	    [Test]
        public void EmptyListSeparator()
	    {
	        StringArrayConverter vrt = new StringArrayConverter();
            Assert.Throws<ArgumentException>(() => vrt.ListSeparator = string.Empty);
        }

        [Test]
        public void TooLongListSeparator()
        {
            StringArrayConverter vrt = new StringArrayConverter();
            Assert.Throws<ArgumentException>(() => vrt.ListSeparator = "  ");
        }

        [Test]
        public void CustomListSeparator()
        {
            object[] expected = new object[] {"1", "Foo", "3"};
            StringArrayConverter vrt = new StringArrayConverter();
            const string customSeparator = "#";
            vrt.ListSeparator = customSeparator;
            object actual = vrt.ConvertFrom(string.Format("1{0}Foo{0}3", customSeparator));
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof (string[]), actual.GetType());
            Assert.IsTrue(ArrayUtils.AreEqual(expected, (string[]) actual),
                "Individual array elements not correctly converted.");
        }

	    [Test]
	    public void NullingTheListSeparatorMakesItRevertToTheDefault()
	    {
            object[] expected = new object[] {"1", "Foo", "3"};
	        StringArrayConverter vrt = new StringArrayConverter();
            vrt.ListSeparator = null;
            object actual = vrt.ConvertFrom("1,Foo,3");
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof (string[]), actual.GetType());
            Assert.IsTrue(ArrayUtils.AreEqual(expected, (string[]) actual),
                "Individual array elements not correctly converted.");
	    }
    }
}