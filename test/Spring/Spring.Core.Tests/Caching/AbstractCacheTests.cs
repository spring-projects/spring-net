#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;

#endregion

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
        MockRepository mocks;
        ExposingAbstractCache cache;
        string[] KEYS = new string[] { "keyA", "keyB" };

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            cache = (ExposingAbstractCache)mocks.PartialMock(typeof(ExposingAbstractCache));

            expectedPerItemTTL = new TimeSpan(0, 0, 10);
            expectedPerCacheTTL = new TimeSpan(0, 0, 20);

            cache.TimeToLive = expectedPerCacheTTL;
        }

        [Test]
        public void TestDefaults()
        {
            ExposingAbstractCache localCache = (ExposingAbstractCache)mocks.PartialMock(typeof(ExposingAbstractCache));
            Assert.AreEqual(TimeSpan.Zero, localCache.TimeToLive);
            Assert.AreEqual(false, localCache.EnforceTimeToLive);
        }

        [Test]
        public void AppliesPerCacheDefaultsIfNoPerItemValuesGiven()
        {
            // set expectations
            cache.DoInsertExposed("key", "value", expectedPerCacheTTL);
            mocks.ReplayAll();
            // verify
            cache.Insert("key", "value", expectedPerCacheTTL);
            mocks.VerifyAll();
        }

        [Test]
        public void AppliesPerCacheDefaultsIfTTLLessThanZero()
        {
            // set expectations
            cache.DoInsertExposed("key", "value", expectedPerCacheTTL);
            cache.DoInsertExposed("key", "value", expectedPerCacheTTL);
            cache.DoInsertExposed("key", "value", expectedPerCacheTTL);
            cache.DoInsertExposed("key", "value", expectedPerCacheTTL);
            mocks.ReplayAll();
            // verify
            cache.Insert("key", "value", new TimeSpan(Timeout.Infinite));
            cache.Insert("key", "value", new TimeSpan(-1));
            cache.Insert("key", "value", new TimeSpan(Int64.MinValue));
            cache.Insert("key", "value", TimeSpan.MinValue);
            mocks.VerifyAll();
        }

        [Test]
        public void AppliesPerCacheDefaultsIfEnfored()
        {
            // set expectations
            cache.DoInsertExposed("key", "value", expectedPerCacheTTL);
            mocks.ReplayAll();
            // verify
            cache.EnforceTimeToLive = true;
            cache.Insert("key", "value", expectedPerItemTTL);
            mocks.VerifyAll();
        }

        [Test]
        public void AppliesZeroTTLIfTTLIsZero()
        {
            // set expectations
            cache.DoInsertExposed("key", "value", TimeSpan.Zero);                        
            mocks.ReplayAll();
            // verify
            cache.Insert("key", "value", TimeSpan.Zero);
            mocks.VerifyAll();
        }

        [Test]
        public void AppliesPerItemTTLIfTTLGreaterZero()
        {
            // set expectations
            cache.DoInsertExposed("key", "value", expectedPerItemTTL);            
            mocks.ReplayAll();
            // verify
            cache.Insert("key", "value", expectedPerItemTTL);
            mocks.VerifyAll();
        }

        [Test]
        public void RemoveAllCausesCallsToRemove()
        {
            // set expectations
            cache.Remove(KEYS[0]);
            cache.Remove(KEYS[1]);
            mocks.ReplayAll();
            // verify
            cache.RemoveAll(KEYS);
            mocks.VerifyAll();
        }

        [Test]
        public void ClearCausesCallToRemoveAllUsingKeys()
        {
            // set expectations
            Expect.Call(cache.Keys).Return(KEYS);
            cache.RemoveAll(KEYS);
            mocks.ReplayAll();
            // verify
            cache.Clear();
            mocks.VerifyAll();
        }

        [Test]
        public void CountUsingKeys()
        {
            // set expectations
            Expect.Call(cache.Keys).Return(this.KEYS);
            mocks.ReplayAll();
            // verify
            Assert.AreEqual( this.KEYS.Length, cache.Count );
            mocks.VerifyAll();
        }
    }
}