#region License

/*
 * Copyright 2004 the original author or authors.
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
using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Core
{
	/// <summary>
	/// Unit tests for the default IControlFlow implementation returned by the
	/// ControlFlowFactory class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class ControlFlowFactoryTests
	{
		[Test]
		public void CreateControlFlow()
		{
			IControlFlow cflow = ControlFlowFactory.CreateControlFlow();
			Assert.IsNotNull(cflow, "The ControlFlowFactory factory class is returning a " +
				"null IControlFlow instance (it, obviously, must not)");
		}

		[Test]
		public void CreateControlFlowReturnsDistinctInstance()
		{
			IControlFlow cflow1 = ControlFlowFactory.CreateControlFlow();
			IControlFlow cflow2 = ControlFlowFactory.CreateControlFlow();
			Assert.IsFalse(Object.ReferenceEquals(cflow1, cflow2), "The ControlFlowFactory " +
				"factory class is not returning distinct IControlFlow instances (its always " +
				"the same instance)");
		}

		[Test]
		public void DefaultCflowUnderThisTest()
		{
			IControlFlow cflow = ControlFlowFactory.CreateControlFlow();
			bool isUnder = cflow.Under(GetType());
			Assert.IsTrue(isUnder, string.Format(
				"The IControlFlow implementation created by the ControlFlowFactory factory class " +
					"would appear to have a faulty Under(Type) implementation : [{0}]", cflow.GetType()));
		}

		[Test]
		public void DefaultCflowIsNotUnderSomeArbitraryClass()
		{
			IControlFlow cflow = ControlFlowFactory.CreateControlFlow();
			bool isUnder = cflow.Under(typeof (TestObject));
			Assert.IsFalse(isUnder, string.Format(
				"The IControlFlow implementation created by the ControlFlowFactory factory class " +
					"would appear to have a faulty Under(Type) implementation : [{0}]", cflow.GetType()));
		}

		[Test]
		public void DefaultCflowUnderThisTestAndTestMethodName()
		{
			IControlFlow cflow = ControlFlowFactory.CreateControlFlow();
			bool isUnder = cflow.Under(GetType(), MethodBase.GetCurrentMethod().Name);
			Assert.IsTrue(isUnder, string.Format(
				"The IControlFlow implementation created by the ControlFlowFactory factory class " +
					"would appear to have a faulty Under(Type,string) implementation : [{0}]",
				cflow.GetType()));
		}

		[Test]
		public void DefaultCflowIsNotUnderThisTestAndSomeRandomMethodName()
		{
			IControlFlow cflow = ControlFlowFactory.CreateControlFlow();
			bool isUnder = cflow.Under(GetType(), "PlayingYouALikeTheFoolYouAre");
			Assert.IsFalse(isUnder, string.Format(
				"The IControlFlow implementation created by the ControlFlowFactory factory class " +
					"would appear to have a faulty Under(Type,string) implementation : [{0}]",
				cflow.GetType()));
		}

		[Test]
		public void DefaultCflowUnderToken()
		{
			IControlFlow cflow = ControlFlowFactory.CreateControlFlow();
			bool isUnder = cflow.UnderToken("Cflow");
			Assert.IsTrue(isUnder, string.Format(
				"The IControlFlow implementation created by the ControlFlowFactory factory " +
					"class would appear to have a faulty UnderToken(string) implementation : [{0}]",
				cflow.GetType()));
		}

		[Test]
		public void DefaultCflowIsNotUnderSomeArbitraryToken()
		{
			IControlFlow cflow = ControlFlowFactory.CreateControlFlow();
			bool isUnder = cflow.UnderToken("GoatsCheeseAndSoda");
			Assert.IsFalse(isUnder, string.Format(
				"The IControlFlow implementation created by the ControlFlowFactory factory class " +
					"would appear to have a faulty UnderToken(string) implementation : [{0}]",
				cflow.GetType()));
		}
	}
}