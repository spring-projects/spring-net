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

using System;
using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Additional and overridden tests for the composition-based proxy.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class CompositionAopProxyTests : AbstractAopProxyTests
    {
        protected override object CreateProxy(AdvisedSupport advisedSupport)
        {
            Assert.IsFalse(advisedSupport.ProxyTargetType, "Not forcible decorator-based proxy");
            object proxy = CreateAopProxy(advisedSupport).GetProxy();
            Assert.IsTrue(AopUtils.IsCompositionAopProxy(proxy), "Should be a composition-based proxy: " + proxy.GetType());
            return proxy;
        }

        protected override Type CreateAopProxyType(AdvisedSupport advisedSupport)
        {
            return new CompositionAopProxyTypeBuilder(advisedSupport).BuildProxyType();
        }

        [Test]
        public void ProxyIsJustInterface()
        {
            TestObject target = new TestObject();
            target.Age = 32;

            AdvisedSupport advised = new AdvisedSupport();
            advised.Target = target;
            advised.Interfaces = new Type[] { typeof(ITestObject) };

            object proxy = CreateProxy(advised);

            Assert.IsTrue(proxy is ITestObject);
            Assert.IsFalse(proxy is TestObject);
        }

        #region ReturnsThisWhenProxyIsIncompatible

        [Test]
        public void ReturnsThisWhenProxyIsIncompatible() 
        {
		    FooBar obj = new FooBar();

		    AdvisedSupport advised = new AdvisedSupport();
            advised.Target = obj;
            advised.Interfaces = new Type[] { typeof(IFoo) };

            IFoo proxy = (IFoo)CreateProxy(advised);

            Assert.AreSame(obj, proxy.GetBarThis(),
                "Target should be returned when return types are incompatible");
		    Assert.AreSame(proxy, proxy.GetFooThis(),
                "Proxy should be returned when return types are compatible");
	    }

    	public interface IFoo 
        {
		    IBar GetBarThis();
		    IFoo GetFooThis();
        }

	    public interface IBar 
        {
	    }

	    public class FooBar : IFoo, IBar 
        {
		    public IBar GetBarThis() 
            {
			    return this;
		    }

		    public IFoo GetFooThis() 
            {
			    return this;
		    }
        }

        #endregion
    }
}