#region License
/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;
using AopAlliance.Intercept;

using NUnit.Framework;
#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// 
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	[TestFixture]
	public class PrototypeTargetTests
	{
		[Test]
		public void PrototypeProxyWithPrototypeTarget()
		{
			TestObjectImpl.constructionCount = 0;
			IObjectFactory iof = new XmlObjectFactory(new ReadOnlyXmlTestResource("prototypeTarget.xml", GetType()));
			for (int i = 0 ; i < 10 ; i++)
			{
				int crap = TestObjectImpl.constructionCount;
				TestObject to = (TestObject) iof.GetObject("testObjectPrototype");
				crap = TestObjectImpl.constructionCount;
				to.DoSomething();
			}
			TestInterceptor interceptor = (TestInterceptor) iof.GetObject("testInterceptor");
			Assert.AreEqual(10, TestObjectImpl.constructionCount);
			Assert.AreEqual(10, interceptor.invocationCount);
		}

		[Test]
		public void SingletonProxyWithPrototypeTarget() 
		{
			TestObjectImpl.constructionCount = 0;
			IObjectFactory iof = new XmlObjectFactory(new ReadOnlyXmlTestResource("prototypeTarget.xml", GetType()));
			for (int i = 0; i < 10; i++) 
			{
				TestObject to = (TestObject) iof.GetObject("testObjectSingleton");
				to.DoSomething();
			}
			TestInterceptor interceptor = (TestInterceptor) iof.GetObject("testInterceptor");
			Assert.AreEqual(1, TestObjectImpl.constructionCount);
			Assert.AreEqual(10, interceptor.invocationCount);
		}

		public interface TestObject
		{
			void DoSomething();
		}

		public class TestObjectImpl : TestObject
		{
			public static int constructionCount = 0;

			public TestObjectImpl() 
			{
				constructionCount++;
			}

			public void DoSomething() 
			{
			}
		}

		public class TestInterceptor : IMethodInterceptor 
		{
			public int invocationCount = 0;

			public object Invoke(IMethodInvocation methodInvocation)
			{
				invocationCount++;
				return methodInvocation.Proceed();
			}
		}
	}
}
