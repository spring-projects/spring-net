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
using System.Threading;

using NUnit.Framework;

using Spring.Objects;
using AopAlliance.Intercept;
using Spring.Threading;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Unit tests for the AopContext class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class AopContextTests
	{
		[SetUp]
		public void SetUp()
		{
			// makes sure the context is always empty before any unit test...
			try
			{
				do
				{
					AopContext.PopProxy();
				} while (true);
			}
			catch (AopConfigException)
			{
			}
		}

		[Test]
		[ExpectedException(typeof (AopConfigException))]
		public void CurrentProxyChokesIfNoAopProxyIsOnTheStack()
		{
			AopContext.CurrentProxy.ToString();
		}

		[Test]
		public void CurrentProxyStackJustPeeksItDoesntPop()
		{
			string foo = "Foo";
			AopContext.PushProxy(foo);
			object fooref = AopContext.CurrentProxy;
			Assert.IsTrue(ReferenceEquals(foo, fooref),
			              "Not the exact same instance (must be).");
			// must not have been popped off the stack by looking at it...
			object foorefref = AopContext.CurrentProxy;
			Assert.IsTrue(ReferenceEquals(fooref, foorefref),
			              "Not the exact same instance (must be).");
		}

		[Test]
		[ExpectedException(typeof (AopConfigException))]
		public void PopProxyWithNothingOnStack()
		{
			AopContext.PopProxy();
        }

        #region CurrentProxyIsThreadSafe

        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-341")]
        public void CurrentProxyIsThreadSafe()
        {
            AsyncTestMethod t1 = new AsyncTestMethod(100, new ThreadStart(ProxyTestObjectAndExposeProxy));
            AsyncTestMethod t2 = new AsyncTestMethod(100, new ThreadStart(ProxyTestObjectAndExposeProxy));
            t1.Start();
            t2.Start();

            t1.AssertNoException();
            t2.AssertNoException();
        }

        private void ProxyTestObjectAndExposeProxy()
        {
            TestObject target = new TestObject();
            target.Age = 26;

            ProxyFactory pf = new ProxyFactory();
            pf.ExposeProxy = true;
            pf.Target = target;
            pf.AddAdvice(new TestAopContextInterceptor());

            ITestObject proxy = pf.GetProxy() as ITestObject;
            Assert.IsNotNull(proxy);
            Assert.AreEqual(target.Age, proxy.Age, "Incorrect age");
        }

        private class TestAopContextInterceptor : IMethodInterceptor
        {
            public object Invoke(IMethodInvocation invocation)
            {
                Assert.IsNotNull(AopContext.CurrentProxy);
                Object ret = invocation.Proceed();
                Assert.IsNotNull(AopContext.CurrentProxy);
                return ret;
            }
        }

        #endregion
	}
}