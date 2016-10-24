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
		public void InstantiationWithNullMethodName()
		{
            Assert.Throws<ArgumentNullException>(() => new LookupMethodOverride(null, null));
		}

		[Test]
		public void InstantiationWithEmptyMethodName()
		{
            Assert.Throws<ArgumentNullException>(() => new LookupMethodOverride(string.Empty, null));
		}

		[Test]
		public void InstantiationWithWhitespacedMethodName()
		{
            Assert.Throws<ArgumentNullException>(() => new LookupMethodOverride("  ", null));
		}

		[Test]
		public void InstantiationWithNullMethodReplacerObjectName()
		{
            Assert.Throws<ArgumentNullException>(() => new LookupMethodOverride("foo", null));
		}

		[Test]
		public void InstantiationWithEmptyMethodReplacerObjectName()
		{
            Assert.Throws<ArgumentNullException>(() => new LookupMethodOverride("foo", string.Empty));
		}

		[Test]
		public void InstantiationWithWhitespacedMethodReplacerObjectName()
		{
            Assert.Throws<ArgumentNullException>(() => new LookupMethodOverride("foo", "  "));
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
		public void MatchesWithNullMethod()
		{
			LookupMethodOverride methodOverride = new LookupMethodOverride("Execute", "foo");
            Assert.Throws<ArgumentNullException>(() => methodOverride.Matches(null));
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