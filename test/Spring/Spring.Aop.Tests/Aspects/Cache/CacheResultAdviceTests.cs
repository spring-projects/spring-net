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

using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using AopAlliance.Intercept;

using FakeItEasy;

using NUnit.Framework;
using Spring.Caching;
using Spring.Context;

namespace Spring.Aspects.Cache
{
    /// <summary>
    /// Unit tests for the CacheResultAdvice class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class CacheResultAdviceTests
    {
        object[] IGNORED_ARGS = null;

        private IMethodInvocation mockInvocation;
        private IApplicationContext mockContext;
        private CacheResultAdvice advice;
        private ICache resultCache;
        private ICache itemCache;
        private ICache binaryFormatterCache;
        private CacheResultTarget cacheResultTarget = new CacheResultTarget();

        [SetUp]
        public void SetUp()
        {
            mockInvocation = A.Fake<IMethodInvocation>();
            mockContext = A.Fake<IApplicationContext>();

            advice = new CacheResultAdvice();
            advice.ApplicationContext = mockContext;

            resultCache = new NonExpiringCache();
            itemCache = new NonExpiringCache();
            binaryFormatterCache = new BinaryFormatterCache();
        }

        /// <summary>
        /// Change History:
        /// 2007-08-22 (oakinger): changed behaviour to cache null values as well
        /// </summary>
        [Test]
        public void CacheResultOfMethodThatReturnsNull()
        {
            MethodInfo method = new VoidMethod(cacheResultTarget.ReturnsNothing).Method;
            object expectedReturnValue = null;

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, null);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(expectedReturnValue);

            // check that the null retVal is cached as well - it might be
            // the result of an expensive webservice/database call etc.
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);
        }

        [Test]
        public void CacheResultOfMethodThatReturnsNullWithSerializingCache()
        {
            MethodInfo method = new VoidMethod(cacheResultTarget.ReturnsNothing).Method;
            object expectedReturnValue = null;

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, null);
            ExpectCacheInstanceRetrieval("results", binaryFormatterCache);
            ExpectCallToProceed(expectedReturnValue);

            // check that the null retVal is cached as well - it might be
            // the result of an expensive webservice/database call etc.
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, binaryFormatterCache.Count);

            // cached value should be returned
            object cachedValue = advice.Invoke(mockInvocation);
            Assert.IsNull(cachedValue, "Should recognize cached value as null-value marker.");
        }

        [Test]
        public void CacheResultOfMethodThatReturnsObject()
        {
            MethodInfo method = new IntMethod(cacheResultTarget.ReturnsScalar).Method;
            object expectedReturnValue = CacheResultTarget.Scalar;

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, null);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(expectedReturnValue);

            // return value should be added to cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);

            // cached value should be returned
            object cachedValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, cachedValue);
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreSame(returnValue, cachedValue);
        }

        [Test]
        public void CacheResultOfMethodThatReturnsCollection()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.ReturnsCollection).Method;
            object expectedReturnValue = new object[] {"one", "two", "three"};

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(new object[] {"one", "two", "three"});

            // return value should be added to cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);

            // cached value should be returned
            object cachedValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, cachedValue);
            Assert.AreNotSame(expectedReturnValue, cachedValue);
            Assert.AreEqual(expectedReturnValue, resultCache.Get(5));
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreSame(returnValue, cachedValue);
            Assert.AreSame(cachedValue, resultCache.Get(5));
        }

        [Test]
        public void CacheResultAndItemsOfMethodThatReturnsCollection()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.ReturnsCollectionAndItems).Method;
            object expectedReturnValue = new object[] {"one", "two", "three"};

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(new object[] {"one", "two", "three"});
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);

            // cached value should be returned
            object cachedValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, cachedValue);
            Assert.AreNotSame(expectedReturnValue, cachedValue);
            Assert.AreEqual(expectedReturnValue, resultCache.Get(5));
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);
            Assert.AreSame(returnValue, cachedValue);
            Assert.AreSame(cachedValue, resultCache.Get(5));
        }

        [Test]
        public void CacheOnlyItemsOfMethodThatReturnsCollection()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.ReturnsItems).Method;
            object expectedReturnValue = new object[] { "one", "two", "three" };

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);

            ExpectAttributeRetrieval(method);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);

            object newReturnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, newReturnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);
            Assert.AreEqual("two", itemCache.Get("two"));
            Assert.AreNotSame(returnValue, newReturnValue);
        }

        [Test]
        public void CacheOnlyItemsOfMethodThatReturnsCollectionWithinTwoDifferentCaches()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.MultipleCacheResultItems).Method;
            object expectedReturnValue = new object[] { "one", "two", "three" };

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCallToProceed(new object[] { "one", "two", "three" });
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(6, itemCache.Count);

            // and again, but without Proceed() and item cache access...
            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCallToProceed(new object[] { "one", "two", "three" });

            // new return value should be returned
            object newReturnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, newReturnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(6, itemCache.Count);
            Assert.AreEqual("two", itemCache.Get("two"));
            Assert.AreEqual("two", itemCache.Get("TWO"));
            Assert.AreNotSame(returnValue, newReturnValue);
        }

        [Test]
        public void CacheOnlyItemsOfMethodThatReturnsCollectionOnCondition()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.CacheResultItemsWithCondition).Method;
            object expectedReturnValue = new object[] {"one", "two", "three"};

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCallToProceed(new object[] {"one", "two", "three"});
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(2, itemCache.Count);
            Assert.AreEqual("two", itemCache.Get("two"));
            Assert.AreEqual("three", itemCache.Get("three"));
        }

        [Test]
        public void CacheResultOfMethodThatReturnsCollectionOnCondition()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.CacheResultWithCondition).Method;
            object expectedReturnValue = new object[] {};

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(new object[] {});

            // return value should not be added to cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(0, resultCache.Count);
        }

        [Test]
        public void AcceptsEnumerableOnlyReturn()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.ReturnsEnumerableOnlyAndItems).Method;
            object[] args = new object[] { "one", "two", "three" };
            EnumerableOnlyResult expectedReturnValue = new EnumerableOnlyResult(args);

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue.InnerArray);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(expectedReturnValue);
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);
            Assert.AreSame(expectedReturnValue, returnValue);
            Assert.AreSame(expectedReturnValue, resultCache.Get(5));

            // and again, but without Proceed() and item cache access...
            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, IGNORED_ARGS);

            // cached value should be returned, cache remains unchanged
            object cachedValue = advice.Invoke(mockInvocation);
            Assert.AreSame(expectedReturnValue, cachedValue);
            Assert.AreSame(returnValue, cachedValue);
            Assert.AreSame(expectedReturnValue, resultCache.Get(5));
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);
        }

        [Test]
        public void CacheResultOfMethodThatReturnsCollectionContainingNullItems()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.ReturnsEnumerableOnlyAndItems).Method;
            object expectedReturnValue = new object[] { null, "two", null };

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(expectedReturnValue);
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreSame(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(2, itemCache.Count); // 2 null items result into 1 cached item

            // and again, but without Proceed() and item cache access...
            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, IGNORED_ARGS);

            // cached value should be returned
            object cachedValue = advice.Invoke(mockInvocation);
            Assert.AreSame(expectedReturnValue, cachedValue);
            Assert.AreSame(returnValue, cachedValue);
            Assert.AreSame(expectedReturnValue, resultCache.Get(5));
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(2, itemCache.Count);
        }

        [Test]
        public void CacheResultWithMethodInfo()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.CacheResultWithMethodInfo).Method;
            object expectedReturnValue = new object[] {"one", "two", "three"};

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCacheInstanceRetrieval("results", resultCache);
            ExpectCallToProceed(new object[] {"one", "two", "three"});

            // return value should be added to cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(1, resultCache.Count);
            Assert.AreEqual(returnValue, resultCache.Get("CacheResultWithMethodInfo-5"));
        }

        [Test]
        public void CacheResultItemsWithMethodInfo()
        {
            MethodInfo method = new EnumerableResultMethod(cacheResultTarget.CacheResultItemsWithMethodInfo).Method;
            object expectedReturnValue = new object[] {"one", "two", "three"};

            ExpectAttributeRetrieval(method);
            ExpectCacheKeyGeneration(method, 5, expectedReturnValue);
            ExpectCallToProceed(new object[] {"one", "two", "three"});
            ExpectCacheInstanceRetrieval("items", itemCache);

            // return value should be added to result cache and each item to item cache
            object returnValue = advice.Invoke(mockInvocation);
            Assert.AreEqual(expectedReturnValue, returnValue);
            Assert.AreEqual(0, resultCache.Count);
            Assert.AreEqual(3, itemCache.Count);
            Assert.AreEqual("two", itemCache.Get("CacheResultItemsWithMethodInfo-two"));
        }

        private void ExpectAttributeRetrieval(MethodInfo method)
        {
            A.CallTo(() => mockInvocation.Method).Returns(method);
        }

        private void ExpectCacheKeyGeneration(MethodInfo method, params object[] arguments)
        {
            A.CallTo(() => mockInvocation.Arguments).Returns(arguments);
        }

        private void ExpectCacheInstanceRetrieval(string cacheName, ICache cache)
        {
            A.CallTo(() => mockContext.GetObject(cacheName)).Returns(cache);
        }

        private void ExpectCacheInstanceRetrieval(string cacheName, ICache cache, int repeatTimes)
        {
            A.CallTo(() => mockContext.GetObject(cacheName)).Returns(cache).NumberOfTimes(repeatTimes);
        }

        private void ExpectCallToProceed(object expectedReturnValue, int repeatTimes)
        {
            A.CallTo(() => mockInvocation.Proceed()).Returns(expectedReturnValue).NumberOfTimes(repeatTimes);
        }

        private void ExpectCallToProceed(object expectedReturnValue)
        {
            A.CallTo(() => mockInvocation.Proceed()).Returns(expectedReturnValue);
        }
    }

    #region Inner Class : CacheResultTarget

    public delegate void VoidMethod();
    public delegate int IntMethod();
    public delegate IEnumerable EnumerableResultMethod(int key, params object[] elements);

    public class EnumerableOnlyResult : IEnumerable
    {
        private object[] _args;

        public EnumerableOnlyResult(params object[] args)
        {
            _args = args;
        }

        public IEnumerator GetEnumerator()
        {
            return _args.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            Assert.AreEqual(_args, ((EnumerableOnlyResult)obj)._args);
            return true;
        }

        public override int GetHashCode()
        {
            return _args.GetHashCode();
        }

        public override string ToString()
        {
            return _args.ToString();
        }

        public int Length { get { return _args.Length; } }

        public object[] InnerArray
        {
            get { return _args; }
        }
    }

    public interface ICacheResultTarget
    {
        void ReturnsNothing();
        int ReturnsScalar();
        IEnumerable ReturnsCollection(int key, params object[] elements);
        IEnumerable ReturnsCollectionAndItems(int key, params object[] elements);
        IEnumerable ReturnsItems(int key, params object[] elements);
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
        public IEnumerable ReturnsCollection(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResult("results", "#key")]
        [CacheResultItems("items", "''+#this")]
        public IEnumerable ReturnsCollectionAndItems(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResult("results", "#key")]
        [CacheResultItems("items", "''+#this")]
        public IEnumerable ReturnsEnumerableOnlyAndItems(int key, params object[] elements)
        {
            return new EnumerableOnlyResult(elements);
        }

        [CacheResultItems("items", "#this")]
        public IEnumerable ReturnsItems(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResultItems("items", "#this")]
        [CacheResultItems("items", "#this.ToUpper()")]
        public IEnumerable MultipleCacheResultItems(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResult("results", "#CacheResultWithMethodInfo.Name + '-' + #key")]
        public IEnumerable CacheResultWithMethodInfo(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResultItems("items", "#CacheResultItemsWithMethodInfo.Name + '-' + #this")]
        public IEnumerable CacheResultItemsWithMethodInfo(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResultItems("items", "#this", Condition = "#this.StartsWith('t')")]
        public IEnumerable CacheResultItemsWithCondition(int key, params object[] elements)
        {
            return elements;
        }

        [CacheResult("results", "#key", Condition = "#this.Length > 0")]
        public IEnumerable CacheResultWithCondition(int key, params object[] elements)
        {
            return elements;
        }
    }

    #endregion

    class BinaryFormatterCache : NonExpiringCache
    {
        protected override void DoInsert(object key, object value, System.TimeSpan timeToLive)
        {
            BinaryFormatter fmt = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                fmt.Serialize(stream, value);
                stream.Seek(0, SeekOrigin.Begin);
                value = stream.ToArray();
            }

            base.DoInsert(key, value, timeToLive);
        }

        public override object Get(object key)
        {
            byte[] bytes = (byte[])base.Get(key);

            if (bytes == null)
            {
                return null;
            }

            BinaryFormatter fmt = new BinaryFormatter();
            return fmt.Deserialize(new MemoryStream(bytes));
        }
    }
}
