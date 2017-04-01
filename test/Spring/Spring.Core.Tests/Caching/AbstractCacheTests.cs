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

using System;
using System.Threading;

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Caching
{
    /// <summary>
    /// Tests <see cref="AbstractCache"/> behaviour to ensure,
    /// that derived classes maybe rely on this default behaviour
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AbstractCacheTests
    {
        #region ExposingAbstractCache utility class

        /// <summary>
        /// Exposes DoInsert() method for testing
        /// </summary>
        public abstract class ExposingAbstractCache : AbstractCache
        {
            protected override void DoInsert(object key, object value, TimeSpan timeToLive)
            {
                DoInsertExposed(key, value, timeToLive);
            }

            public abstract void DoInsertExposed(object key, object value, TimeSpan timeToLive);
        }

        #endregion

        TimeSpan expectedPerItemTTL;
        TimeSpan expectedPerCacheTTL;
        ExposingAbstractCache cache;
        string[] KEYS = new string[] {"keyA", "keyB"};

        [SetUp]
        public void SetUp()
        {
            cache = A.Fake<ExposingAbstractCache>(options => options.CallsBaseMethods());

            expectedPerItemTTL = new TimeSpan(0, 0, 10);
            expectedPerCacheTTL = new TimeSpan(0, 0, 20);

            cache.TimeToLive = expectedPerCacheTTL;
        }

        [Test]
        public void TestDefaults()
        {
            ExposingAbstractCache localCache = A.Fake<ExposingAbstractCache>();
            Assert.AreEqual(TimeSpan.Zero, localCache.TimeToLive);
            Assert.AreEqual(false, localCache.EnforceTimeToLive);
        }

        [Test]
        public void AppliesPerCacheDefaultsIfNoPerItemValuesGiven()
        {
            // set expectations
            cache.DoInsertExposed("key", "value", expectedPerCacheTTL);
            cache.Insert("key", "value", expectedPerCacheTTL);
            A.CallTo(() => cache.DoInsertExposed("key", "value", expectedPerCacheTTL)).MustHaveHappened();
        }

        [Test]
        public void AppliesPerCacheDefaultsIfTTLLessThanZero()
        {
            cache.Insert("key", "value", new TimeSpan(Timeout.Infinite));
            A.CallTo(() => cache.DoInsertExposed("key", "value", expectedPerCacheTTL)).MustHaveHappened();

            cache.Insert("key", "value", new TimeSpan(-1));
            A.CallTo(() => cache.DoInsertExposed("key", "value", expectedPerCacheTTL)).MustHaveHappened();

            cache.Insert("key", "value", new TimeSpan(long.MinValue));
            A.CallTo(() => cache.DoInsertExposed("key", "value", expectedPerCacheTTL)).MustHaveHappened();

            cache.Insert("key", "value", TimeSpan.MinValue);
            A.CallTo(() => cache.DoInsertExposed("key", "value", expectedPerCacheTTL)).MustHaveHappened();
        }

        [Test]
        public void AppliesPerCacheDefaultsIfEnfored()
        {
            cache.EnforceTimeToLive = true;
            cache.Insert("key", "value", expectedPerItemTTL);

            A.CallTo(() => cache.DoInsertExposed("key", "value", expectedPerCacheTTL)).MustHaveHappened();
        }

        [Test]
        public void AppliesZeroTTLIfTTLIsZero()
        {
            cache.Insert("key", "value", TimeSpan.Zero);

            A.CallTo(() => cache.DoInsertExposed("key", "value", TimeSpan.Zero)).MustHaveHappened();
        }

        [Test]
        public void AppliesPerItemTTLIfTTLGreaterZero()
        {
            cache.Insert("key", "value", expectedPerItemTTL);

            A.CallTo(() => cache.DoInsertExposed("key", "value", expectedPerItemTTL)).MustHaveHappened();
        }

        [Test]
        public void RemoveAllCausesCallsToRemove()
        {
            cache.RemoveAll(KEYS);

            A.CallTo(() => cache.Remove(KEYS[0])).MustHaveHappened();
            A.CallTo(() => cache.Remove(KEYS[1])).MustHaveHappened();
        }

        [Test]
        public void ClearCausesCallToRemoveAllUsingKeys()
        {
            A.CallTo(() => cache.Keys).Returns(this.KEYS);

            cache.Clear();

            A.CallTo(() => cache.Keys).MustHaveHappened();
            A.CallTo(() => cache.RemoveAll(KEYS)).MustHaveHappened();
        }

        [Test]
        public void CountUsingKeys()
        {
            // set expectations
            A.CallTo(() => cache.Keys).Returns(this.KEYS);

            Assert.AreEqual(this.KEYS.Length, cache.Count);
        }
    }
}