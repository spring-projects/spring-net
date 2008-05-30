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
using Spring.Util;

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Unit tests for the TrueTypeFilter class.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <version>$Id: TrueTypeFilterTests.cs,v 1.3 2006/04/09 07:19:06 markpollack Exp $</version>
	[TestFixture]
	public sealed class TrueTypeFilterTests
	{
		[Test]
		public void Deserialization()
		{
			ITypeFilter deserializedVersion
				= (ITypeFilter) SerializationTestUtils.SerializeAndDeserialize(
					TrueTypeFilter.True);
			Assert.IsTrue(Object.ReferenceEquals(TrueTypeFilter.True, deserializedVersion),
			              "Singleton instance not being deserialized correctly");
		}

		[Test]
		public void IsSerializable()
		{
			Assert.IsTrue(SerializationTestUtils.IsSerializable(TrueTypeFilter.True),
			              "TrueClassFilter must be serializable.");
		}

		[Test]
		public void AlwaysMatchesEvenOnNullArgument()
		{
			Assert.IsTrue(TrueTypeFilter.True.Matches(null),
			              "Must always match (return true).");
		}

		[Test]
		public void AlwaysMatches()
		{
			Assert.IsTrue(TrueTypeFilter.True.Matches(GetType()),
			              "Must always match (return true).");
		}
		[Test]
			public void ToStringAlwaysTrue()
		{
			Assert.AreEqual("TrueTypeFilter.True", TrueTypeFilter.True.ToString() );
		}
	}
}