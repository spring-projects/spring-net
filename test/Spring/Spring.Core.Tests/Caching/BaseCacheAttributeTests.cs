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

namespace Spring.Caching
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class BaseCacheAttributeTests
    {
        private BaseCacheAttribute att;

        private class DerivedCacheAttribute : BaseCacheAttribute
        {
            public DerivedCacheAttribute()
            {
                
            }

            public DerivedCacheAttribute(string cacheName, string key)
                : base(cacheName, key)
            {
                
            }
        }

        public string TestProperty
        {
            get { return "OK"; }
        }

        [SetUp]
        public void SetUp()
        {
            att = new DerivedCacheAttribute();
        }

        [Test]
        public void CacheNameIsSet()
        {
            att.CacheName = "MyCache";
            Assert.AreEqual("MyCache", att.CacheName);
        }

        [Test]
        public void KeyIsParsed()
        {
            att.Key = "TestProperty";
            string result = att.KeyExpression.GetValue(this) as string;
            Assert.AreEqual("OK", result);
        }

        [Test]
        public void ConditionIsParsed()
        {
            att.Condition = "TestProperty";
            string result = att.ConditionExpression.GetValue(this) as string;
            Assert.AreEqual("OK", result);
        }

        [Test]
        public void AllowsForExtendedTimeSpanConverterSyntax()
        {
            att.TimeToLive = "5ms";
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 5), att.TimeToLiveTimeSpan);
        }

        [Test]
        public void KeyCannotBeEmptyString()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => att = new DerivedCacheAttribute("someName", string.Empty));
        }

        [Test]
        public void KeyCannotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => att = new DerivedCacheAttribute("someName", null));
        }
    }
}