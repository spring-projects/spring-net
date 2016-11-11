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

using NUnit.Framework;

#endregion

namespace Spring.Collections
{
	/// <summary>
	/// Unit tests for the SortedSet class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public class SortedSetTests : SetTests
    {
        [OneTimeSetUp]
        public void Init () 
        {
            SupportsNull = false;
        }

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public override void SetUp () 
        {
            Set = new SortedSet ();
            SetForSetOps = new SortedSet ();
        }

        [Test]
        public void IsOrdered () 
        {
            ISet mySet = new SortedSet (new int [] {2, 4, 5, 1, 3, 0});
            int j = 0;
            foreach (object o in mySet) 
            {
                Assert.AreEqual (j++, o, "Found element out of order while iterating over SortedSet");
            }
        }
    }
}
