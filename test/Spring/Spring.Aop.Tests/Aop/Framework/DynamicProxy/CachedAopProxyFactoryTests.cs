#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.Reflection;
using System.Collections;

using NUnit.Framework;
using Spring.Objects;
using Spring.Aop.Support;

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
	/// <summary>
    /// Unit tests for the CachedAopProxyFactoryTests class.
	/// </summary>
	/// <author>Bruno Baia</author>
	/// <version>$Id: CachedAopProxyFactoryTests.cs,v 1.1 2007/08/04 01:20:11 bbaia Exp $</version>
	[TestFixture]
    public sealed class CachedAopProxyFactoryTests : DefaultAopProxyFactoryTests
	{
        protected override IAopProxy CreateAopProxy(AdvisedSupport advisedSupport)
        {
            IAopProxyFactory apf = new CachedAopProxyFactory();
            return apf.CreateAopProxy(advisedSupport);
        }

        [SetUp]
        public void SetUp()
        {
            // Clear Aop proxy type cache
            Assert.IsNotNull(TypeCacheField);
            TypeCacheField.SetValue(null, new Hashtable());
        }

        [Test]
        public void DoesNotCacheWithDifferentBaseType()
        {
            // Decorated-based proxy (BaseType == TargetType)
            AdvisedSupport advisedSupport = new AdvisedSupport();
            advisedSupport.ProxyTargetType = true;
            advisedSupport.Target = new TestObject();
            CreateAopProxy(advisedSupport);

            // Composition-based proxy (BaseType = BaseCompositionAopProxy)
            advisedSupport = new AdvisedSupport();
            advisedSupport.ProxyTargetType = false;
            advisedSupport.Target = new TestObject();
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(2);
        }

        [Test]
        public void DoesNotCacheWithDifferentTargetType()
        {
            AdvisedSupport advisedSupport = new AdvisedSupport();
            advisedSupport.Target = new BadCommand();
            CreateAopProxy(advisedSupport);

            advisedSupport = new AdvisedSupport();
            advisedSupport.Target = new GoodCommand();
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(2);
        }

        [Test]
        public void DoesNotCacheWithDifferentInterfaces()
        {
            AdvisedSupport advisedSupport = new AdvisedSupport();
            advisedSupport.Target = new TestObject();
            CreateAopProxy(advisedSupport);

            advisedSupport = new AdvisedSupport();
            advisedSupport.Target = new TestObject();
            advisedSupport.AddInterface(typeof(IPerson));
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(2);

            // Same with Introductions
            advisedSupport = new AdvisedSupport();
            advisedSupport.Target = new TestObject();
            TimestampIntroductionInterceptor ti = new TimestampIntroductionInterceptor();
            ti.TimeStamp = new DateTime(666L);
            IIntroductionAdvisor introduction = new DefaultIntroductionAdvisor(ti, typeof(ITimeStamped));
            advisedSupport.AddIntroduction(introduction);
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(3);
        }

        [Test]
        public void DoesCacheWithTwoDecoratorBasedProxy()
        {
            AdvisedSupport advisedSupport = new AdvisedSupport();
            advisedSupport.ProxyTargetType = true;
            advisedSupport.Target = new TestObject();
            CreateAopProxy(advisedSupport);

            advisedSupport = new AdvisedSupport();
            advisedSupport.ProxyTargetType = true;
            advisedSupport.Target = new TestObject();
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(1);
        }

        [Test]
        public void DoesCacheWithTwoCompositionBasedProxy()
        {
            AdvisedSupport advisedSupport = new AdvisedSupport();
            advisedSupport.Target = new TestObject();
            CreateAopProxy(advisedSupport);

            advisedSupport = new AdvisedSupport();
            advisedSupport.Target = new TestObject();
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(1);
        }


        private static readonly FieldInfo TypeCacheField =
            typeof(CachedAopProxyFactory).GetField("typeCache", BindingFlags.Static | BindingFlags.NonPublic);

        private void AssertAopProxyTypeCacheCount(int count)
        {
            Assert.IsNotNull(TypeCacheField);
            Hashtable cache = TypeCacheField.GetValue(null) as Hashtable;
            Assert.IsNotNull(cache);
            Assert.AreEqual(count, cache.Count);
        }

        #region Helper classes definitions

        public interface ICommand
        {
            void Execute();
        }

        public sealed class BadCommand : ICommand
        {
            public void Execute()
            {
                throw new NotImplementedException();
            }
        }

        public sealed class GoodCommand : ICommand
        {
            public void Execute()
            {
            }
        }

        #endregion
    }
}