#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.ComponentModel;
using NUnit.Framework;
using Spring.Util;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Tests <see cref="UniqueKeyConverter"/> functionality.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class UniqueKeyConverterTests
    {
        [Test]
        public void CanConvertFromStringOnly()
        {
            TypeConverter c = new UniqueKeyConverter();
            Assert.IsTrue(c.CanConvertFrom(typeof(string)));
            Assert.IsFalse(c.CanConvertFrom(typeof(object)));
        }

        [Test]
        public void CanConvertToStringOnly()
        {
            TypeConverter c = new UniqueKeyConverter();
            Assert.IsTrue(c.CanConvertTo(typeof(string)));
            Assert.IsFalse(c.CanConvertTo(typeof(object)));
        }

        [Test]
        public void ConvertToStringOrUniqueKeyOnly()
        {
            TypeConverter c = new UniqueKeyConverter();
            UniqueKey key = UniqueKey.GetInstanceScoped(new object(), "PartialKey");

            c.ConvertTo(key, typeof(UniqueKey));
            c.ConvertTo(key, typeof(string));
            try
            {
                c.ConvertTo(key, typeof(object));
                Assert.Fail();
            }
            catch(NotSupportedException) {}
        }

        [Test]
        public void ConvertFromStringOrUniqueKeyOnly()
        {
            TypeConverter c = new UniqueKeyConverter();
            UniqueKey key = UniqueKey.GetInstanceScoped(new object(), "PartialKey");
            c.ConvertFrom(key);
            c.ConvertFrom(key.ToString());
            try
            {
                c.ConvertFrom(new object());
                Assert.Fail();
            }
            catch(NotSupportedException) {}
        }

        [Test]
        public void ConvertFromReturnsNullIfInputNull()
        {
            TypeConverter c = new UniqueKeyConverter();
            Assert.IsNull(c.ConvertFrom(null)); 
        }

        [Test]
        public void ConvertToReturnsNullIfInputNull()
        {
            TypeConverter c = new UniqueKeyConverter();
            Assert.IsNull(c.ConvertTo(null, typeof(string)));
        }

        [Test]
        public void ConvertToEqualsToString()
        {
            object testObject = new object();
            UniqueKey key = UniqueKey.GetInstanceScoped(testObject, "PartialKey");

            TypeConverter c = new UniqueKeyConverter();
            string stringKey = (string) c.ConvertTo(key, typeof(string));
            Assert.AreEqual(key.ToString(), stringKey);
        }

        [Test]
        public void ConvertToAndFromAreInSync()
        {
            object testObject = new object();
            UniqueKey expectedKey = UniqueKey.GetInstanceScoped(testObject, "PartialKey");

            TypeConverter c = new UniqueKeyConverter();
            string stringKey = (string)c.ConvertTo(expectedKey, typeof(string));
            UniqueKey key2 = (UniqueKey) c.ConvertFrom(stringKey);
            Assert.AreEqual( expectedKey, key2 );
        }

    }
}