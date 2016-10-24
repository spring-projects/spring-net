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

using NUnit.Framework;

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Unit tests for the RuntimeTypeConverter class.
	/// </summary>
	[TestFixture]
    public class RuntimeTypeConverterTests
    {
        [Test]
        public void CanConvertFrom ()
        {
            RuntimeTypeConverter cnv = new RuntimeTypeConverter ();
            Assert.IsTrue (cnv.CanConvertFrom (typeof (string)), "Mmm... I can't convert from a string to a Type.");
            Assert.IsFalse (cnv.CanConvertFrom (GetType ()), "Mmm... managed to convert to a Type from a Type of this test. Boing!");
        }

        [Test]
        public void CanConvertTo ()
        {
            RuntimeTypeConverter cnv = new RuntimeTypeConverter ();
            Assert.IsTrue (cnv.CanConvertTo (typeof (Type)), "Mmm... I can't convert to a Type at all.");
            Assert.IsFalse (cnv.CanConvertTo (typeof (void)), "Mmm... managed to convert to a Type from a bad type. Boing!");
        }

        [Test]
        public void ConvertFromNonSupportedType ()
        {
            RuntimeTypeConverter cnv = new RuntimeTypeConverter ();
            Assert.Throws<NotSupportedException>(() => cnv.ConvertFrom (12));
        }

        [Test]
        public void ConvertFromString () 
        {
            RuntimeTypeConverter cnv = new RuntimeTypeConverter ();
            object foo = cnv.ConvertFrom ("System.String");
            Assert.IsNotNull (foo);
            Assert.AreEqual (GetType().GetType().FullName, foo.GetType ().FullName);
        }

        [Test]
        public void ConvertToString () 
        {
            RuntimeTypeConverter cnv = new RuntimeTypeConverter ();
            object foo = cnv.ConvertTo (typeof (string), typeof (string));
            Assert.IsNotNull (foo);
            Assert.AreEqual (typeof (string).AssemblyQualifiedName, foo);
        }

        [Test]
        public void ConvertToStringFromNonSupportedType ()
        {
            RuntimeTypeConverter cnv = new RuntimeTypeConverter ();
            Assert.Throws<NotSupportedException>(() => cnv.ConvertTo (typeof (string), GetType ()));
        }
	}
}
