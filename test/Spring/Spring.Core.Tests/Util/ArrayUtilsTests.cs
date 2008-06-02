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
using System.Collections;

using NUnit.Framework;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// Unit tests for the ArrayUtils class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class ArrayUtilsTests
    {
        [Test]
        public void AreEqual () 
        {
            object [] one = new string [] {"Foo", "Bar", "Baz"};
            object [] two = new string [] {"Foo", "Bar", "Baz"};
            Assert.IsTrue (ArrayUtils.AreEqual (one, two));
            object [] three = new string [] {"Foo", "Ben", "Baz"};
            Assert.IsFalse (ArrayUtils.AreEqual (one, three));
        }

        [Test]
        public void AreEqualWithBadArguments () 
        {
            Assert.IsTrue (ArrayUtils.AreEqual (null, null));
            object [] one = new string [] {"Foo", "Bar", "Baz"};
            object [] two = null;
            Assert.IsFalse (ArrayUtils.AreEqual (one, two));
            object [] three = new string [] {"Foo", "Bar"};
            Assert.IsFalse (ArrayUtils.AreEqual (one, three));
        }

        [Test]
        public void HasLengthTests()
        {
            Assert.IsFalse(ArrayUtils.HasLength(null));
            Assert.IsFalse(ArrayUtils.HasLength(new byte[0]));
            Assert.IsTrue(ArrayUtils.HasLength(new byte[1]));
        }
	}
}
