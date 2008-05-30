#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System.Collections;
using System.Reflection;
using AopAlliance.Intercept;
using DotNetMock.Dynamic;
using NUnit.Framework;
using Spring.Caching;
using Spring.Context;

#endregion

namespace Spring.Aspects.Cache
{
    /// <summary>
    /// Unit tests for the CacheResultAdvice class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: CacheResultAdviceTests.cs,v 1.4 2007/08/22 20:16:51 oakinger Exp $</version>
    [TestFixture]
    public sealed class CacheResultAdviceTests
    {
        private IDynamicMock mockInvocation;
        private IDynamicMock mockContext;
        private CacheResultAdvice advice;
        private ICache resultCache;
        private ICache itemCache;

        [SetUp]
        public void SetUp()
        {
            mockInvocation = new DynamicMock(typeof(IMethodInvocation));
            mockContext = new DynamicMock(typeof(IApplicationContext));

            advice = new CacheResultAdvice();
            advice.ApplicationContext = (IApplicationContext) mockContext.Object;

            resultCache = new NonExpiringCache();
            itemCache = new NonExpiringCache();
        }

        /// <summary>
        /// Change History:
        /// 2007-08-22 (oakinger): changed behaviour to cache null values as well
        /// </summary>
        [Test]
        public void CacheResultOfMethodThatReturnsNull()
        {
            MethodInfo method = typeof(CacheResultTarget).GetMethod("ReturnsNothing");
            object expectedReturnValue = null;

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, null);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(expectedReturnValue);

            // check that the null retVal is cached as well - it might be 
            // the result of an expensive webservice/database call etc.
            object returnValue = advice.Invoke((IMethodInvocation) mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);

            mockInvocation.Verify();
            mockContext.Verify();
        }

        [Test]
        public void CacheResultOfMethodThatReturnsObject()
        {
            MethodInfo method = typeof(CacheResultTarget).GetMethod("ReturnsScalar");
            object expectedReturnValue = CacheResultTarget.Scalar;

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, null);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(expectedReturnValue);

            // return value should be added to cache
            object returnValue = advice.Invoke((IMethodInvocation) mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);

            // and again, but without Proceed()...
            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, null);
            ExpectCacheInstanceRetrieval("results", resultCache);

            // cached value should be returned
            object cachedValue = advice.Invoke((IMethodInvocation) mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, cachedValue);
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreSame(returnValue, cachedValue);

            mockInvocation.Verify();
            mockContext.Verify();
        }

        [Test]
        public void CacheResultOfMethodThatReturnsCollection()
        {
            MethodInfo method = typeof(CacheResultTarget).GetMethod("ReturnsCollection");
            object expectedReturnValue = new object[] {"one", "two", "three"};

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(new object[] { "one", "two", "three" });

            // return value should be added to cache
            object returnValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);

            // and again, but without Proceed()...
            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);

            // cached value should be returned
            object cachedValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, cachedValue);
            Assert.AreNotSame(expectedReturnValue, cachedValue);
            Assert.AreEqual(expectedReturnValue, resultCache.Get(5));
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreSame(returnValue, cachedValue);
            Assert.AreSame(cachedValue, resultCache.Get(5));

            mockInvocation.Verify();
            mockContext.Verify();
        }

        [Test]
        public void CacheResultAndItemsOfMethodThatReturnsCollection()
        {
            MethodInfo method = typeof(CacheResultTarget).GetMethod("ReturnsCollectionAndItems");
            object expectedReturnValue = new object[] { "one", "two", "three" };

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);

            // and again, but without Proceed() and item cache access...
            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);

            // cached value should be returned
            object cachedValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, cachedValue);
            Assert.AreNotSame(expectedReturnValue, cachedValue);
            Assert.AreEqual(expectedReturnValue, resultCache.Get(5));
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);
            Assert.AreSame(returnValue, cachedValue);
            Assert.AreSame(cachedValue, resultCache.Get(5));

            mockInvocation.Verify();
            mockContext.Verify();
        }

        [Test]
        public void CacheOnlyItemsOfMethodThatReturnsCollection()
        {
            MethodInfo method = typeof(CacheResultTarget).GetMethod("ReturnsItems");
            object expectedReturnValue = new object[] { "one", "two", "three" };

            ExpectAttributeRetrieval(method);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);

            // and again, but without Proceed() and item cache access...
            ExpectAttributeRetrieval(method);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheInstanceRetrieval("items", itemCache);

            // new return value should be returned
            object newReturnValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, newReturnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);
            Assert.AreEqual("two", itemCache.Get("two"));
            Assert.AreNotSame(returnValue, newReturnValue);

            mockInvocation.Verify();
            mockContext.Verify();
        }

        [Test]
        public void CacheOnlyItemsOfMethodThatReturnsCollectionWithinTwoDifferentCaches()
        {
            MethodInfo method = typeof(CacheResultTarget).GetMethod("MultipleCacheResultItems");
            object expectedReturnValue = new object[] { "one", "two", "three" };

            ExpectAttributeRetrieval(method);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheInstanceRetrieval("items", itemCache);
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(6, itemCache.Count);

            // and again, but without Proceed() and item cache access...
            ExpectAttributeRetrieval(method);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheInstanceRetrieval("items", itemCache);
            ExpectCacheInstanceRetrieval("items", itemCache);

            // new return value should be returned
            object newReturnValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, newReturnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(6, itemCache.Count);
            Assert.AreEqual("two", itemCache.Get("two"));
            Assert.AreEqual("two", itemCache.Get("TWO"));
            Assert.AreNotSame(returnValue, newReturnValue);

            mockInvocation.Verify();
            mockContext.Verify();
        }

        [Test]
        public void CacheOnlyItemsOfMethodThatReturnsCollectionOnCondition()
        {
            MethodInfo method = typeof(CacheResultTarget).GetMethod("CacheResultItemsWithCondition");
            object expectedReturnValue = new object[] { "one", "two", "three" };

            ExpectAttributeRetrieval(method);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(2, itemCache.Count);
            Assert.AreEqual("two", itemCache.Get("two"));
            Assert.AreEqual("three", itemCache.Get("three"));

            mockInvocation.Verify();
            mockContext.Verify();
        }

        [Test]
        public void CacheResultOfMethodThatReturnsCollectionOnCondition()
        {
            MethodInfo method = typeof(CacheResultTarget).GetMethod("CacheResultWithCondition");
            object expectedReturnValue = new object[] { };

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(new object[] { });

            // return value should not be added to cache
            object returnValue = advice.Invoke((IMethodInvocation)mockInvocation.Object);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(0, resultCache.Count);

            mockInvocation.Verify();
            mockContext.Verify();
        }


        #region Helper methods

        private void ExpectAttributeRetrieval(MethodInfo method)
        {
            mockInvocation.ExpectAndReturn("Method", method);
            mockInvocation.ExpectAndReturn("Method", method);
        }

        private void ExpectCacheKeyGeneration(MethodInfo method, params object[] arguments)
        {
            mockInvocation.ExpectAndReturn("Method", method);
            mockInvocation.ExpectAndReturn("Arguments", arguments);
        }

        private void ExpectCacheInstanceRetrieval(string cacheName, ICache cache)
        {
            mockContext.ExpectAndReturn("GetObject", cache, cacheName);
        }

        private void ExpectCallToProceed(object expectedReturnValue)
        {
            mockInvocation.ExpectAndReturn("Proceed", expectedReturnValue);
        }

        #endregion

    }

    #region Inner Class : CacheResultTarget

    public interface ICacheResultTarget
    {
        void ReturnsNothing();
        int ReturnsScalar();
        ICollection ReturnsCollection(int key, params object[] elements);
        ICollection ReturnsCollectionAndItems(int key, params object[] elements);
        ICollection ReturnsItems(int key, params object[] elements);
    }

    public sealed class CacheResultTarget : ICacheResultTarget
    {
        public const int Scalar = int.MaxValue;

        [CacheResult("results", "'key'")]
        public void ReturnsNothing()
        {
        }

        [CacheResult("results", "'key'")]
        public int ReturnsScalar()
        {
            return Scalar;
        }

        [CacheResult("results", "#key")]
        public ICollection ReturnsCollection(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResult("results", "#key")]
        [CacheResultItems("items", "#this")]
        public ICollection ReturnsCollectionAndItems(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResultItems("items", "#this")]
        public ICollection ReturnsItems(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResultItems("items", "#this")]
        [CacheResultItems("items", "#this.ToUpper()")]
        public ICollection MultipleCacheResultItems(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResultItems("items", "#this", Condition="#this.StartsWith('t')")]
        public ICollection CacheResultItemsWithCondition(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResult("results", "#key", Condition="#this.Length > 0")]
        public ICollection CacheResultWithCondition(int key, params object[] elements)
        {
            return elements;
        }
    }

    #endregion
}