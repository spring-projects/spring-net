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

using System.Reflection;

using FakeItEasy;

using NUnit.Framework;

using Spring.Caching;
using Spring.Context;

namespace Spring.Aspects.Cache
{
    /// <summary>
    /// Unit tests for the InvalidateCacheAdvice class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class InvalidateCacheAdviceTests
    {
        private IApplicationContext mockContext;
        private InvalidateCacheAdvice advice;
        private ICache cache;

        [SetUp]
        public void SetUp()
        {
            mockContext = A.Fake<IApplicationContext>();

            advice = new InvalidateCacheAdvice();
            advice.ApplicationContext = mockContext;

            cache = new NonExpiringCache();
            cache.Insert(1, "one");
            cache.Insert(2, "two");
            cache.Insert(3, "three");
        }

        [Test]
        public void TestSingleKeyInvalidation()
        {
            MethodInfo method = typeof(InvalidateCacheTarget).GetMethod("InvalidateSingle");
            object[] args = new object[] { 2 };

            ExpectCacheInstanceRetrieval("cache", cache);

            Assert.AreEqual(3, cache.Count);

            // item "two" should be removed from cache
            advice.AfterReturning(null, method, args, null);
            Assert.AreEqual(2, cache.Count);
            Assert.IsNull(cache.Get(2));
        }

        [Test]
        public void TestMultiKeyInvalidation()
        {
            MethodInfo method = typeof(InvalidateCacheTarget).GetMethod("InvalidateMulti");
            object[] args = new object[] { 2 };

            ExpectCacheInstanceRetrieval("cache", cache);

            Assert.AreEqual(3, cache.Count);

            // all items except item "two" should be removed from cache
            advice.AfterReturning(null, method, args, null);
            Assert.AreEqual(1, cache.Count);
            Assert.AreEqual("two", cache.Get(2));
        }

        [Test]
        public void TestWholeCacheInvalidation()
        {
            MethodInfo method = typeof(InvalidateCacheTarget).GetMethod("InvalidateAll");

            ExpectCacheInstanceRetrieval("cache", cache);

            Assert.AreEqual(3, cache.Count);

            // all items should be removed from cache
            advice.AfterReturning(null, method, null, null);
            Assert.AreEqual(0, cache.Count);
        }

        [Test]
        public void TestMultipleCachesInvalidation()
        {
            MethodInfo method = typeof(InvalidateCacheTarget).GetMethod("InvalidateMultipleCaches");
            object[] args = new object[] { 2 };

            ExpectCacheInstanceRetrieval("cache", cache);
            ExpectCacheInstanceRetrieval("cache", cache);

            Assert.AreEqual(3, cache.Count);

            // item "two" should be removed from cache
            // all items except item "two" should be removed from cache
            advice.AfterReturning(null, method, args, null);
            Assert.AreEqual(0, cache.Count);
        }

        [Test]
        public void TestConditionInvalidation()
        {
            MethodInfo method = typeof(InvalidateCacheTarget).GetMethod("InvalidateWithCondition");
            object[] args = new object[] { 3 };

            Assert.AreEqual(3, cache.Count);

            // no items should be removed from cache
            advice.AfterReturning(null, method, args, null);
            Assert.AreEqual(3, cache.Count);
            Assert.AreEqual("three", cache.Get(3));
        }

        private void ExpectCacheInstanceRetrieval(string cacheName, ICache cacheToReturn)
        {
            A.CallTo(() => mockContext.GetObject(cacheName)).Returns(cacheToReturn).Once();
        }
    }

    #region Inner Class : InvalidateCacheTarget

    public sealed class InvalidateCacheTarget
    {
        [InvalidateCache("cache", Keys = "#key")]
        public void InvalidateSingle(int key)
        {
        }

        [InvalidateCache("cache", Keys = "{1, 2, 3} - { #key }")]
        public void InvalidateMulti(int key)
        {
        }

        [InvalidateCache("cache")]
        public void InvalidateAll()
        {
        }

        [InvalidateCache("cache", Keys = "#key")]
        [InvalidateCache("cache", Keys = "{1, 2, 3} - { #key }")]
        public void InvalidateMultipleCaches(int key)
        {
        }

        [InvalidateCache("cache", Keys = "{1, 2, 3} - { #key }", Condition="#key != 3")]
        public void InvalidateWithCondition(int key)
        {
        }
    }

    #endregion
}