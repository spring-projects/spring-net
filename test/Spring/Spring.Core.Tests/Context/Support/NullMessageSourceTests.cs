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

using NUnit.Framework;

namespace Spring.Context.Support
{
	/// <summary>
	/// Unit tests for the NullMessageSource class.
	/// </summary>
	/// <author></author>
	[TestFixture]
	public sealed class NullMessageSourceTests
	{
		[Test]
		public void CanonicalInstanceIsNotNull()
		{
			Assert.IsNotNull(NullMessageSource.Null);
		}

		[Test]
		public void ResolveMessageSpitsbackWhatItWasGiven()
		{
			const string expected = "foo";
			string message = NullMessageSource.Null.GetMessage(expected);
			Assert.AreEqual(expected, message);
		}

		[Test]
		public void ResolveObjectReturnsNull()
		{
			object anObject = NullMessageSource.Null.GetResourceObject("");
			Assert.IsNull(anObject);
		}

		[Test]
		public void ApplyResourcesDoesNothing()
		{
			NullMessageSource.Null.ApplyResources("", "foo", null);
		}
	}
}