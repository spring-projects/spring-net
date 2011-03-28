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
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Aop.Interceptor;
using Spring.Objects;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Unit tests for the ControlFlowPointcut class.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	[TestFixture]
	public sealed class ControlFlowPointcutTests
	{
		[Test]
		[Category("Integration")]
		public void Matches()
		{
			SerializablePerson target = new SerializablePerson();
			target.SetAge(27);
			ControlFlowPointcut cflow = new ControlFlowPointcut(typeof(One), "GetAge");
			ProxyFactory factory = new ProxyFactory(target);
			NopInterceptor nop = new NopInterceptor();
			IPerson proxied = (IPerson) factory.GetProxy();
			factory.AddAdvisor(new DefaultPointcutAdvisor(cflow, nop));

			// not advised, not under One...
			Assert.AreEqual(target.GetAge(), proxied.GetAge());
			Assert.AreEqual(0, nop.Count, "Whoops, appear to be advising when not under One's cflow.");

			// will be advised...
			One one = new One();
			Assert.AreEqual(27, one.GetAge(proxied));
			Assert.AreEqual(1, nop.Count, "Not advising when under One's cflow (must be).");

			// won't be advised...
			Assert.AreEqual(target.GetAge(), new One().NoMatch(proxied));
			Assert.AreEqual(1, nop.Count, "Whoops, appear to be advising when under One's cflow scope, BUT NOT under a target method's cflow scope.");
			Assert.AreEqual(3, cflow.EvaluationCount, "Pointcut not invoked the correct number of times.");
		}

		private sealed class One
		{
            // attribute is required so that the delegated call is NOT jitted away...
            [MethodImpl(MethodImplOptions.NoInlining)]
			public int GetAge(IPerson proxied)
			{
				return proxied.GetAge();
			}

			public int NoMatch(IPerson proxied)
			{
				return proxied.GetAge();
			}

            // similarly, attribute is required so that the delegated call is NOT jitted away...
            [MethodImpl(MethodImplOptions.NoInlining)]
			public void Set(IPerson proxied)
			{
				proxied.SetAge(5);
			}
		}

		/// <summary>
		/// Check that we can use a cflow pointcut only in conjunction with
		/// a static pointcut: e.g. all setter methods that are invoked under
		/// a particular class.
		/// </summary>
		/// <remarks>
		/// This greatly reduces the number of calls to the cflow pointcut,
		/// meaning that it's not so prohibitively expensive.
		/// </remarks>
		[Test]
		[Category("Integration")]
		public void SelectiveApplication()
		{
			SerializablePerson target = new SerializablePerson();
			target.SetAge(27);
			NopInterceptor nop = new NopInterceptor();
			ControlFlowPointcut cflow = new ControlFlowPointcut(typeof (One));
			IPointcut settersUnderOne = Pointcuts.Intersection(SetterPointcut.Instance, cflow);
			ProxyFactory pf = new ProxyFactory(target);
			IPerson proxied = (IPerson) pf.GetProxy();
			pf.AddAdvisor(new DefaultPointcutAdvisor(settersUnderOne, nop));

			// Not advised, not under One
			target.SetAge(16);
			Assert.AreEqual(0, nop.Count);

			// Not advised; under One but not a setter
			Assert.AreEqual(16, new One().GetAge(proxied));
			Assert.AreEqual(0, nop.Count);

			// Won't be advised
			new One().Set(proxied);
			Assert.AreEqual(1, nop.Count);

			// We saved most evaluations
			Assert.AreEqual(1, cflow.EvaluationCount);
		}

		[Test]
		public void EvaluationCountIncrementedEvenIfPointcutDoesNotMatch()
		{
			ControlFlowPointcut cut = new ControlFlowPointcut(typeof(One));
			cut.Matches(null, null, null); // args are ingored in this impl...
			Assert.AreEqual(1, cut.EvaluationCount);
			cut.Matches(null, null, null); // args are ingored in this impl...
			Assert.AreEqual(2, cut.EvaluationCount);
		}

		[Test]
		public void EvaluationCountIncrementedOnEveryMatch()
		{
			Type oneType = typeof(One);
			ControlFlowPointcut cut = new ControlFlowPointcut(oneType);
			MethodInfo method = oneType.GetMethod("GetAge");
			cut.Matches(method, oneType, null);
			Assert.AreEqual(1, cut.EvaluationCount);
			cut.Matches(method, oneType, null);
			Assert.AreEqual(2, cut.EvaluationCount);
		}

		[Test]
		public void DefaultClassFilterImplAlwaysMatchesRegardless()
		{
			Type oneType = typeof(One);
			ControlFlowPointcut cut = new ControlFlowPointcut(oneType);
			ITypeFilter filter = cut.TypeFilter;
			Assert.IsTrue(filter.Matches(oneType),
				"Must always match regardless of the supplied argument Type.");
			Assert.IsTrue(filter.Matches(GetType()),
				"Must always match even if the supplied argument Type is not " +
				"a match for the Type supplied in the ctor.");
			Assert.IsTrue(filter.Matches(null), // args are ingored in this impl...
				"Must always match even if the supplied argument Type is null");
		}

		[Test]
		public void StaticMethodMatchImplAlwaysMatchesRegardless()
		{
			Type oneType = typeof(One);
			ControlFlowPointcut cut = new ControlFlowPointcut(oneType);
			IMethodMatcher filter = cut.MethodMatcher;
			MethodInfo method = oneType.GetMethod("GetAge");
			Assert.IsTrue(filter.Matches(method, oneType),
				"Must always match regardless of the supplied arguments.");
			Assert.IsTrue(filter.Matches(method, GetType()),
				"Must always match even if the supplied argument method and Type are not " +
				"a match for the name and Type supplied in the ctor.");
			Assert.IsTrue(filter.Matches(null, null), // args are ingored in this impl...
				"Must always match even if the supplied arguments are null");
		}

		[Test]
		public void DynamicMethodMatchWithJustTypeSpecifiedInCtor()
		{
			ControlFlowPointcut cut = new ControlFlowPointcut(GetType());
			IMethodMatcher filter = cut.MethodMatcher;
			Assert.IsTrue(filter.Matches(null, null, null), // args are ingored in this impl...
				"Must match - under cflow of Type specified in ctor");
		}

		[Test]
		public void DynamicMethodMatchWithTypeAndMethodNameSpecifiedInCtor()
		{
			ControlFlowPointcut cut = new ControlFlowPointcut(
				GetType(), "DynamicMethodMatchWithTypeAndMethodNameSpecifiedInCtor");
			IMethodMatcher filter = cut.MethodMatcher;
			Assert.IsTrue(filter.Matches(null, null, null), // args are ingored in this impl...
				"Must match - under cflow of Type specified in ctor");
		}

		[Test]
		public void DynamicMethodMatchWithTypeAndMethodNameSpecifiedInCtorNoMatch()
		{
			ControlFlowPointcut cut = new ControlFlowPointcut(GetType(), "KiloRiley");
			IMethodMatcher filter = cut.MethodMatcher;
			Assert.IsFalse(filter.Matches(null, null, null), // args are ingored in this impl...
				"Must not match - under cflow of Type specified in ctor, but no match on method name.");
		}

		#region Helper Classes

		/// <summary>
		/// Pointcut to catch all methods beginning with 'Set'.
		/// </summary>
		private class SetterPointcut : StaticMethodMatcherPointcut
		{
			public static SetterPointcut Instance = new SetterPointcut();

			public override bool Matches(MethodInfo methodBase, Type targetType)
			{
				return methodBase.Name.StartsWith("Set");
			}
		}

		#endregion
	}
}