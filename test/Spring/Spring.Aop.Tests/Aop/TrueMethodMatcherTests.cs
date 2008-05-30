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
using System.Reflection;
using NUnit.Framework;
using Spring.Util;

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Unit tests for the TrueMethodMatcher class.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: TrueMethodMatcherTests.cs,v 1.3 2006/04/09 07:19:06 markpollack Exp $</version>
	[TestFixture]
    public sealed class TrueMethodMatcherTests
    {
		[Test]
		public void Deserialization()
		{
			IMethodMatcher deserializedVersion
				= (IMethodMatcher) SerializationTestUtils.SerializeAndDeserialize(
				TrueMethodMatcher.True);
			Assert.IsTrue(Object.ReferenceEquals(TrueMethodMatcher.True, deserializedVersion),
				"Singleton instance not being deserialized correctly");
		}

		[Test]
		public void IsSerializable()
		{
			Assert.IsTrue(SerializationTestUtils.IsSerializable(TrueMethodMatcher.True),
				"TrueMethodMatcher must be serializable.");
		}

		[Test]
		public void AlwaysMatchesEvenOnNullArguments()
		{
			Assert.IsTrue(TrueMethodMatcher.True.Matches(null, null),
				"Must always match (return true).");
		}

		[Test]
		public void AlwaysMatches()
		{
			Assert.IsTrue(TrueMethodMatcher.True.Matches(
				(MethodInfo) MethodBase.GetCurrentMethod(), GetType()),
				"Must always match (return true).");
		}

		[Test]
		public void IsRuntime()
		{
			Assert.IsFalse(TrueMethodMatcher.True.IsRuntime, "Must NOT be runtime.");
		}
    }
}
