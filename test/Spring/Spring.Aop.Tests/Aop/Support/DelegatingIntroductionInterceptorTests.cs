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

using System;
using NUnit.Framework;
using AopAlliance.Aop;

using FakeItEasy;

using Spring.Aop.Framework;
using Spring.Objects;

namespace Spring.Aop.Support
{
	/// <summary>
	/// Translation of DelegatingIntroductionInterceptor unit tests to Spring.NET.
	/// </summary>
	/// <remarks>
	/// Spring.NET doesn't have a DelegatingIntroductionInterceptor because it handles
	/// introductions without using interception. So all the unit tests show how similar
	/// things can be done in Spring.NET.
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Choy Rim (.NET)</author>
	[TestFixture]
	public class DelegatingIntroductionInterceptorTests
	{
		private static readonly DateTime EXPECTED_TIMESTAMP = new DateTime(2004,8,1);

		[Test]
		public void testNullTarget()
		{
            Assert.Throws<ArgumentNullException>(() => new DefaultIntroductionAdvisor(null, typeof(ITimeStamped)));
		}

		public interface ITimeStampedIntroduction: ITimeStamped, IAdvice
		{
		}

		[Test]
		public void TestIntroductionInterceptorWithDelegation()
		{
			TestObject raw = new TestObject();
			Assert.IsTrue(! (raw is ITimeStamped));
			ProxyFactory factory = new ProxyFactory(raw);

			ITimeStampedIntroduction ts = A.Fake<ITimeStampedIntroduction>();
			A.CallTo(() => ts.TimeStamp).Returns(EXPECTED_TIMESTAMP);

			DefaultIntroductionAdvisor advisor = new DefaultIntroductionAdvisor(ts);
			factory.AddIntroduction(advisor);

			ITimeStamped tsp = (ITimeStamped) factory.GetProxy();
			Assert.IsTrue(tsp.TimeStamp == EXPECTED_TIMESTAMP);
		}

		// we have to mark the ISubTimeStamped interface with the IAdvice marker
		// in order to use it as an introduction.
		public interface ISubTimeStampedIntroduction: ISubTimeStamped, IAdvice
		{
		}

		[Test]
		public void TestIntroductionInterceptorWithInterfaceHierarchy()
		{
			TestObject raw = new TestObject();
			Assert.IsTrue(! (raw is ISubTimeStamped));
			ProxyFactory factory = new ProxyFactory(raw);

            ISubTimeStampedIntroduction ts = A.Fake<ISubTimeStampedIntroduction>();
            A.CallTo(() => ts.TimeStamp).Returns(EXPECTED_TIMESTAMP);

            DefaultIntroductionAdvisor advisor = new DefaultIntroductionAdvisor(ts);
			// we must add introduction, not an advisor
			factory.AddIntroduction(advisor);

			object proxy = factory.GetProxy();
			ISubTimeStamped tsp = (ISubTimeStamped) proxy;
			Assert.IsTrue(tsp.TimeStamp == EXPECTED_TIMESTAMP);
		}

		[Test]
		public void TestIntroductionInterceptorWithSuperInterface()
		{
			TestObject raw = new TestObject();
			Assert.IsTrue(! (raw is ITimeStamped));
			ProxyFactory factory = new ProxyFactory(raw);

            ISubTimeStampedIntroduction ts = A.Fake<ISubTimeStampedIntroduction>();
            A.CallTo(() => ts.TimeStamp).Returns(EXPECTED_TIMESTAMP);

            factory.AddIntroduction(0, new DefaultIntroductionAdvisor(
				ts,
				typeof(ITimeStamped))
				);

			ITimeStamped tsp = (ITimeStamped) factory.GetProxy();
			Assert.IsTrue(!(tsp is ISubTimeStamped));
			Assert.IsTrue(tsp.TimeStamp == EXPECTED_TIMESTAMP);
		}

		/// <summary>
		/// test introduction.
		/// <note>It must include the IAdvice marker interface to be a
		/// valid introduction.</note>
		/// </summary>
		private class Test : ITimeStamped, ITest, IAdvice
		{
			private DateTime _timestamp;

			public Test(DateTime timestamp)
			{
				_timestamp = timestamp;
			}
			public void Foo()
			{
			}
			public DateTime TimeStamp
			{
				get
				{
					return _timestamp;
				}
			}
		}

		public void TestAutomaticInterfaceRecognitionInDelegate()
		{
			IIntroductionAdvisor ia = new DefaultIntroductionAdvisor(new Test(EXPECTED_TIMESTAMP));

			TestObject target = new TestObject();
			ProxyFactory pf = new ProxyFactory(target);
			pf.AddIntroduction(0, ia);

			ITimeStamped ts = (ITimeStamped) pf.GetProxy();

			Assert.IsTrue(ts.TimeStamp == EXPECTED_TIMESTAMP);
			((ITest) ts).Foo();

			int age = ((ITestObject) ts).Age;
		}

		/*
		 * The rest of the tests in the original tested subclassing the
		 * DelegatingIntroductionInterceptor.
		 *
		 * Since we don't need to subclass anything to make a delegating
		 * introduction, the rest of the tests are not necessary.
		 */

		// must be public to be used for AOP
		// AOP creates a new assembly which must have access to the
		// interfaces that it intends to expose.
		public interface ITest
		{
			void Foo();
		}

		public interface ISubTimeStamped : ITimeStamped
		{
		}

	}
}
