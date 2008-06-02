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

using NUnit.Framework;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// Unit tests for the AssertUtils class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class AssertUtilsTests
	{
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StateTrue()
        {
            AssertUtils.State(false,"foo");
        }

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ArgumentNotNull () 
		{
			AssertUtils.ArgumentNotNull(null, "foo");
		}

		[Test]
        [ExpectedException(typeof(ArgumentNullException))]
		public void ArgumentNotNullWithMessage ()
		{
			AssertUtils.ArgumentNotNull(null, "foo", "Bang!");
		}

		[Test]
		public void ArgumentHasTextWithValidText()
		{
			AssertUtils.ArgumentHasText("... and no-one's getting fat 'cept Mama Cas!", "foo");
		}

		[Test]
		public void ArgumentHasTextWithValidTextAndMessage()
		{
			AssertUtils.ArgumentHasText("... and no-one's getting fat 'cept Mama Cas!", "foo", "Bang!");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ArgumentHasText () 
		{
			AssertUtils.ArgumentHasText(null, "foo");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ArgumentHasTextWithMessage () 
		{
			AssertUtils.ArgumentHasText(null, "foo", "Bang!");
		}

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasLengthArgumentIsNull()
        {
            AssertUtils.ArgumentHasLength(null, "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasLengthArgumentIsNullWithMessage()
        {
            AssertUtils.ArgumentHasLength(null, "foo", "Bang!");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasLengthArgumentIsEmpty()
        {
            AssertUtils.ArgumentHasLength(new byte[0], "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasLengthArgumentIsEmptyWithMessage()
        {
            AssertUtils.ArgumentHasLength(new byte[0], "foo", "Bang!");
        }

        [Test]
        public void ArgumentHasLengthArgumentHasElements()
        {
            AssertUtils.ArgumentHasLength(new byte[1], "foo");
        }

        [Test]
        public void ArgumentHasLengthArgumentHasElementsWithMessage()
        {
            AssertUtils.ArgumentHasLength(new byte[1], "foo", "Bang!");
        }
    }
}
