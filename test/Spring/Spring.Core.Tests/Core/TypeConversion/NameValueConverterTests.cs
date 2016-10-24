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
using System.Collections.Specialized;

using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Unit tests for the NameValueConverter class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class NameValueConverterTests
    {
        [Test]
        public void CanConvertFromString () 
        {
            NameValueConverter vrt = new NameValueConverter ();
            Assert.IsTrue (vrt.CanConvertFrom (typeof (string)), "Conversion from a string instance must be supported.");
        }

		[Test]
		public void CanConvertOnlyFromString()
		{
			NameValueConverter vrt = new NameValueConverter ();
			Assert.IsFalse(vrt.CanConvertFrom(typeof(TestObject)),
				"Seem to be able to convert from non-supported Type.");
		}

		[Test]
        public void ConvertFrom () 
        {
            string xml =
                "<foo>" + 
                "	<add key=\"one\" value=\"1\"/>" +
                "	<add key=\"two\" value=\"2\"/>" +
                "</foo>"; 
            NameValueConverter vrt = new NameValueConverter ();
            NameValueCollection actual = vrt.ConvertFrom (xml) as NameValueCollection;
            Assert.IsNotNull (actual);
            Assert.AreEqual (2, actual.Count);
            Assert.AreEqual ("one", actual.GetKey (0));
            Assert.AreEqual ("two", actual.GetKey (1));
            Assert.AreEqual ("1", actual ["one"]);
            Assert.AreEqual ("2", actual ["two"]);
        }

        [Test]
        public void ConvertFromNullReference()
        {
            NameValueConverter vrt = new NameValueConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(null));
        }

        [Test]
        public void ConvertFromNonSupportedOptionBails()
        {
            NameValueConverter vrt = new NameValueConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(true));
        } 
	}
}
