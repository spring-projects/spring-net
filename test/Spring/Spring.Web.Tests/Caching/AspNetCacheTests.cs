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

using System;
using System.Collections;
using System.Web;
using System.Web.Caching;

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Caching
{
    /// <summary>
    /// Test AspNetCache behaviour.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AspNetCacheTests
    {
        private AspNetCache thisCache;
        private AspNetCache otherCache;
        private readonly Cache aspCache = HttpRuntime.Cache;
        private readonly TimeSpan ttl10Seconds = new TimeSpan(0, 0, 10);

        AspNetCache.IRuntimeCache mockedRuntimeCache;
        AspNetCache mockedCache;

        [SetUp]
        public void SetUp()
        {
            // cleanup underlying static asp.net cache
            foreach (DictionaryEntry entry in HttpRuntime.Cache)
            {
                aspCache.Remove((string)entry.Key);
            }

            thisCache = new AspNetCache();
            otherCache = new AspNetCache();

            mockedRuntimeCache = A.Fake<AspNetCache.IRuntimeCache>();
            mockedCache = new AspNetCache(mockedRuntimeCache);
        }

        [Test]
        public void Get()
        {
            mockedCache.Get("key");

            A.CallTo(() => mockedRuntimeCache.Get(mockedCache.GenerateKey("key"))).MustHaveHappened();
        }

        [Test]
        public void Remove()
        {
            mockedCache.Remove("key");

            A.CallTo(() => mockedRuntimeCache.Remove(mockedCache.GenerateKey("key"))).MustHaveHappened();
        }

        [Test]
        public void DoesNotAcceptNullKeysOnInsert()
        {
            Assert.Throws<ArgumentNullException>(() => thisCache.Insert(null, "value", TimeSpan.Zero, true));
        }

        [Test]
        public void GetNullKeysReturnsNull()
        {
            Assert.IsNull(thisCache.Get(null));
        }

        [Test]
        public void IgnoreNullKeysOnRemove()
        {
            thisCache.Remove(null);
        }

        [Test]
        public void ReturnsOnlyKeysOwnedByCache()
        {
            DictionaryEntry[] mockedRuntimeCacheEntries =
                {
                      new DictionaryEntry(mockedCache.GenerateKey("keyA"), null)
                    , new DictionaryEntry(mockedCache.GenerateKey("keyB"), null)
                    , new DictionaryEntry(thisCache.GenerateKey("keyC"), null)
                    , new DictionaryEntry(otherCache.GenerateKey("keyD"), null)
                };

            A.CallTo(() => mockedRuntimeCache.GetEnumerator()).Returns(mockedRuntimeCacheEntries.GetEnumerator());

            ICollection keys = mockedCache.Keys;
            CollectionAssert.AreEqual(new string[] { "keyA", "keyB" }, keys);
        }

        [Test]
        public void AddItemToCacheNoExpiration()
        {
            thisCache.Insert( "key", "thisValue", TimeSpan.Zero, false );
            Assert.AreEqual(1, thisCache.Count);
            Assert.AreEqual("thisValue", aspCache.Get(thisCache.GenerateKey("key")));

            otherCache.Insert("key", "otherValue", TimeSpan.Zero, false);
            Assert.AreEqual(1, otherCache.Count);
            Assert.AreEqual("otherValue", aspCache.Get(otherCache.GenerateKey("key")));
        }

        [Test]
        public void AddItemToCacheAbsoluteExpiration()
        {
            thisCache.Insert("key", "thisValue", ttl10Seconds, false);
            Assert.AreEqual(1, thisCache.Count);
            Assert.AreEqual("thisValue", aspCache.Get(thisCache.GenerateKey("key")));

            otherCache.Insert("key", "otherValue", ttl10Seconds, false);
            Assert.AreEqual(1, otherCache.Count);
            Assert.AreEqual("otherValue", aspCache.Get(otherCache.GenerateKey("key")));
        }

        [Test]
        public void AddItemToCacheSlidingExpiration()
        {
            thisCache.Insert("key", "thisValue", ttl10Seconds, true);
            Assert.AreEqual(1, thisCache.Count);
            Assert.AreEqual("thisValue", aspCache.Get(thisCache.GenerateKey("key")));

            otherCache.Insert("key", "otherValue", ttl10Seconds, true);
            Assert.AreEqual(1, otherCache.Count);
            Assert.AreEqual("otherValue", aspCache.Get(otherCache.GenerateKey("key")));
        }

        [Test]
        public void DifferentCacheNamesCauseSameKeyToBeDifferent()
        {
            Assert.AreNotEqual("key", thisCache.GenerateKey("key"));
            Assert.AreNotEqual(thisCache.GenerateKey("key"), otherCache.GenerateKey("key"));
        }

        [Test]
        public void PassesParametersToRuntimeCache()
        {
            AspNetCache.IRuntimeCache runtimeCache = A.Fake<AspNetCache.IRuntimeCache>();
            AspNetCache cache = new AspNetCache(runtimeCache);

            DateTime expectedAbsoluteExpiration = DateTime.Now;
            TimeSpan expectedSlidingExpiration = ttl10Seconds;
            CacheItemPriority expectedPriority = CacheItemPriority.Low;

//          TODO: find way to test non-sliding expiration case
//            runtimeCache.Insert(cache.GenerateKey("key"), "value", null, DateTime.Now.Add(ttl10Seconds), Cache.NoSlidingExpiration, expectedPriority, null);

//            cache.Insert( "key", "value", ttl10Seconds, false, expectedPriority );
            cache.Insert("key", "value", ttl10Seconds, true, expectedPriority);

            A.CallTo(() => runtimeCache.Insert(cache.GenerateKey("key"), "value", null, Cache.NoAbsoluteExpiration, ttl10Seconds, expectedPriority, null)).MustHaveHappened();
        }

        [Test]
        public void ZeroTTLCausesNoExpiration()
        {
            AspNetCache.IRuntimeCache runtimeCache = A.Fake<AspNetCache.IRuntimeCache>();
            AspNetCache cache = new AspNetCache(runtimeCache);

            CacheItemPriority expectedPriority = CacheItemPriority.Low;

            cache.Insert("key", "value", TimeSpan.Zero, true, expectedPriority);
            A.CallTo(() => runtimeCache.Insert(cache.GenerateKey("key"), "value", null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, expectedPriority, null)).MustHaveHappened();

            cache.Insert("key", "value", TimeSpan.Zero, false, expectedPriority);
            A.CallTo(() => runtimeCache.Insert(cache.GenerateKey("key"), "value", null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, expectedPriority, null)).MustHaveHappened();
        }
    }
}