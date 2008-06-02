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
        protected virtual IAopProxy CreateAopProxy(AdvisedSupport advisedSupport)
        {
            IAopProxyFactory apf = new DefaultAopProxyFactory();
            return apf.CreateAopProxy(advisedSupport);
        }

        [Test]
        [ExpectedException(typeof(AopConfigException), "Cannot create IAopProxy with null ProxyConfig")]
        public void NullConfig()
        {
            CreateAopProxy(null);
        }

        [Test]
        [ExpectedException(typeof(AopConfigException), "Cannot create IAopProxy with no advisors and no target source")]
        public void NoInterceptorsAndNoTarget()
        {
            AdvisedSupport advisedSupport = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            CreateAopProxy(advisedSupport);
        }

        [Test]
        public void TargetDoesNotImplementAnyInterfaces()
        {
            AdvisedSupport advisedSupport = new AdvisedSupport();
            advisedSupport.ProxyTargetType = false;
            advisedSupport.Target = new DoesNotImplementAnyInterfacesTestObject();
            
            IAopProxy aopProxy = CreateAopProxy(advisedSupport);
            Assert.IsNotNull(aopProxy);
            Assert.IsTrue(AopUtils.IsDecoratorAopProxy(aopProxy));
        }

        [Test]
        public void TargetImplementsAnInterface()
        {
            AdvisedSupport advisedSupport = new AdvisedSupport();
            advisedSupport.Target = new TestObject();

            IAopProxy aopProxy = CreateAopProxy(advisedSupport);
            Assert.IsNotNull(aopProxy);

            Assert.IsTrue(AopUtils.IsCompositionAopProxy(aopProxy));
        }

        [Test]
        public void TargetImplementsAnInterfaceWithProxyTargetTypeSetToTrue()
        {
            AdvisedSupport advisedSupport = new AdvisedSupport();
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