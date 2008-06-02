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

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Unit tests for the LookupMethodOverride class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class LookupMethodOverrideTests
	{
		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithNullMethodName()
		{
			new LookupMethodOverride(null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithEmptyMethodName()
		{
			new LookupMethodOverride(string.Empty, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithWhitespacedMethodName()
		{
			new LookupMethodOverride("  ", null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithNullMethodReplacerObjectName()
		{
			new LookupMethodOverride("foo", null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithEmptyMethodReplacerObjectName()
		{
			new LookupMethodOverride("foo", string.Empty);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithWhitespacedMethodReplacerObjectName()
		{
			new LookupMethodOverride("foo", "  ");
		}

		[Test]
		public void Matches_TotallyDifferentMethodName()
		{
			LookupMethodOverride methodOverride = new LookupMethodOverride("Bingo", "foo");
			Assert.IsFalse(methodOverride.Matches(typeof (Feeder).GetMethod("GetGrub")));
		}

		[Test]
		public void Matches_MatchingMethodName()
		{
			LookupMethodOverride methodOverride = new LookupMethodOverride("GetGrub", "foo");
			Assert.IsTrue(methodOverride.Matches(typeof (Feeder).GetMethod("GetGrub")));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void MatchesWithNullMethod()
		{
			LookupMethodOverride methodOverride = new LookupMethodOverride("Execute", "foo");
			methodOverride.Matches(null);
		}

		[Test]
		public void ToStringWorks()
		{
			LookupMethodOverride methodOverride = new LookupMethodOverride("GetGrub", "foo");
			Assert.AreEqual(typeof (LookupMethodOverride).Name + " for method 'GetGrub'; will return object 'foo'.", methodOverride.ToString());
		}

		private sealed class Feeder
		{
			public object GetGrub()
			{
				return null;
			}
		}
	}
}