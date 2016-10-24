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

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
	/// <summary>
    /// Unit tests for the DefaultAopProxyFactory class.
	/// </summary>
	/// <author>Bruno Baia</author>
	[TestFixture]
    public class DefaultAopProxyFactoryTests
	{
        protected virtual IAopProxy CreateAopProxy(ProxyFactory advisedSupport)
        {
//            return (IAopProxy) advisedSupport.GetProxy();
            IAopProxyFactory apf = new DefaultAopProxyFactory();
            return apf.CreateAopProxy(advisedSupport);
        }

        [Test]
        public void NullConfig()
        {
            IAopProxyFactory apf = new DefaultAopProxyFactory();
            Assert.Throws<AopConfigException>(() => apf.CreateAopProxy(null),"Cannot create IAopProxy with null ProxyConfig" );
        }

        [Test]
        public void NoInterceptorsAndNoTarget()
        {
            ProxyFactory advisedSupport = new ProxyFactory(new Type[] { typeof(ITestObject) });
            Assert.Throws<AopConfigException>(() => CreateAopProxy(advisedSupport), "Cannot create IAopProxy with no advisors and no target source");
        }

        [Test]
        public void TargetDoesNotImplementAnyInterfaces()
        {
            ProxyFactory advisedSupport = new ProxyFactory();
            advisedSupport.AopProxyFactory = new DefaultAopProxyFactory();
            advisedSupport.ProxyTargetType = false;
            advisedSupport.Target = new DoesNotImplementAnyInterfacesTestObject();
            
            IAopProxy aopProxy = CreateAopProxy(advisedSupport);
            Assert.IsNotNull(aopProxy);
            Assert.IsTrue(AopUtils.IsDecoratorAopProxy(aopProxy));
        }

        [Test]
        public void TargetImplementsAnInterface()
        {
            ProxyFactory advisedSupport = new ProxyFactory(new TestObject());
            IAopProxy aopProxy = CreateAopProxy(advisedSupport);
            Assert.IsNotNull(aopProxy);

            Assert.IsTrue(AopUtils.IsCompositionAopProxy(aopProxy));
        }

        [Test]
        public void TargetImplementsAnInterfaceWithProxyTargetTypeSetToTrue()
        {
            ProxyFactory advisedSupport = new ProxyFactory();
            advisedSupport.ProxyTargetType = true;
            advisedSupport.Target = new TestObject();

            IAopProxy aopProxy = CreateAopProxy(advisedSupport);
            Assert.IsNotNull(aopProxy);
            Assert.IsTrue(AopUtils.IsDecoratorAopProxy(aopProxy));
        }

        #region Helper classes definitions

        public class DoesNotImplementAnyInterfacesTestObject
        {
            public virtual void SomeMethod()
            {
            }
        }

        #endregion
    }
}