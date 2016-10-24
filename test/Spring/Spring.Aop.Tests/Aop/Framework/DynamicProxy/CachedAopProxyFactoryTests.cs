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
using Spring.Objects;
using Spring.Aop.Support;

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
	/// <summary>
    /// Unit tests for the CachedAopProxyFactoryTests class.
	/// </summary>
	/// <author>Bruno Baia</author>
	[TestFixture]
    public sealed class CachedAopProxyFactoryTests : DefaultAopProxyFactoryTests
	{
        protected override IAopProxy CreateAopProxy(ProxyFactory advisedSupport)
        {
            //            return (IAopProxy) advisedSupport.GetProxy();
            IAopProxyFactory apf = new CachedAopProxyFactory();
            return apf.CreateAopProxy(advisedSupport);
        }

        [SetUp]
        public void SetUp()
        {
            CachedAopProxyFactory.ClearCache();
        }

        [Test]
        public void DoesNotCacheWithDifferentBaseType()
        {
            // Decorated-based proxy (BaseType == TargetType)
            ProxyFactory advisedSupport = new ProxyFactory(new TestObject());
            advisedSupport.ProxyTargetType = true;
            CreateAopProxy(advisedSupport);

            // Composition-based proxy (BaseType = BaseCompositionAopProxy)
            advisedSupport = new ProxyFactory(new TestObject());
            advisedSupport.ProxyTargetType = false;
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(2);
        }

        [Test]
        public void DoesNotCacheWithDifferentTargetType()
        {
            ProxyFactory advisedSupport = new ProxyFactory(new BadCommand());
            CreateAopProxy(advisedSupport);

            advisedSupport = new ProxyFactory(new GoodCommand());
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(2);
        }

        [Test]
        public void DoesNotCacheWithDifferentProxyTargetAttributes()
        {
            ProxyFactory advisedSupport = new ProxyFactory(new GoodCommand());
            advisedSupport.ProxyTargetAttributes = true;
            CreateAopProxy(advisedSupport);

            advisedSupport = new ProxyFactory(new GoodCommand());
            advisedSupport.ProxyTargetAttributes = false;
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(2);
        }

        [Test]
        public void DoesNotCacheWithDifferentInterfaces()
        {
            ProxyFactory advisedSupport = new ProxyFactory(new TestObject());
            CreateAopProxy(advisedSupport);

            advisedSupport = new ProxyFactory(new TestObject());
            advisedSupport.AddInterface(typeof(IPerson));
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(2);

            // Same with Introductions
            advisedSupport = new ProxyFactory(new TestObject());
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
            ProxyFactory advisedSupport = new ProxyFactory(new TestObject());
            advisedSupport.ProxyTargetType = true;
            CreateAopProxy(advisedSupport);

            advisedSupport = new ProxyFactory(new TestObject());
            advisedSupport.ProxyTargetType = true;
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(1);
        }

        [Test]
        public void DoesCacheWithTwoCompositionBasedProxy()
        {
            ProxyFactory advisedSupport = new ProxyFactory(new TestObject());
            CreateAopProxy(advisedSupport);

            advisedSupport = new ProxyFactory(new TestObject());
            CreateAopProxy(advisedSupport);

            AssertAopProxyTypeCacheCount(1);
        }

        private void AssertAopProxyTypeCacheCount(int count)
        {
            Assert.AreEqual(count, CachedAopProxyFactory.CountCachedTypes);
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