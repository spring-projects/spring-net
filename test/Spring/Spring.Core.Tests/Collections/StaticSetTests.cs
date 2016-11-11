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
	/// Unit tests for the static methods of the Set class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class StaticSetTests
    {
        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp ()
        {
            one = new ListSet (new object [] {1, "Foo", 2});
            two = new ListSet (new object [] {1, 3});
        }

        [Test]
        public void Xor ()
        {
            ISet actual = Set.ExclusiveOr (one, two);
            Assert.IsNotNull (actual);
            Assert.AreEqual (3, actual.Count);
            Assert.IsTrue (actual.ContainsAll (new object [] {"Foo", 2, 3}));
            CheckThatOriginalsHaveNotBeenModified ();

            Assert.IsNull (Set.ExclusiveOr (null, null));

            actual = Set.ExclusiveOr (null, two);
            Assert.AreEqual (two, actual);

            actual = Set.ExclusiveOr (one, null);
            Assert.AreEqual (one, actual);
        }

        [Test]
        public void Minus ()
        {
            ISet actual = Set.Minus (one, two);
            Assert.IsNotNull (actual);
            Assert.AreEqual (2, actual.Count);
            Assert.IsTrue (actual.ContainsAll (new object [] {"Foo", 2}));
            CheckThatOriginalsHaveNotBeenModified ();

            Assert.IsNull (Set.Minus (null, two));
        }

        [Test]
        public void Union ()
        {
            ISet actual = Set.Union (one, two);
            Assert.IsNotNull (actual);
            Assert.AreEqual (4, actual.Count);
            Assert.IsTrue (actual.ContainsAll (new object [] {1, "Foo", 2, 3}));
            CheckThatOriginalsHaveNotBeenModified ();

            Assert.IsNull (Set.Union (null, null));

            actual = Set.Union (null, two);
            Assert.AreEqual (two, actual);

            actual = Set.Union (one, null);
            Assert.AreEqual (one, actual);
        }

        [Test]
        public void Intersect ()
        {
            ISet actual = Set.Intersect (one, two);
            Assert.IsNotNull (actual);
            Assert.AreEqual (1, actual.Count);
            Assert.IsTrue (actual.Contains (1));
            CheckThatOriginalsHaveNotBeenModified ();

            Assert.IsNull (Set.Intersect (null, null));

            actual = Set.Intersect (null, two);
            Assert.AreEqual (0, actual.Count);

            actual = Set.Intersect (one, null);
            Assert.AreEqual (0, actual.Count);
        }

        private void CheckThatOriginalsHaveNotBeenModified () 
        {
            Assert.IsNotNull (one, "Set operation modified the original sets.");
            Assert.AreEqual (3, one.Count, "Set operation modified the original sets.");
            Assert.IsTrue (one.ContainsAll (new object [] {1, "Foo", 2}), "Set operation modified the original sets.");

            Assert.IsNotNull (two, "Set operation modified the original sets.");
            Assert.AreEqual (2, two.Count, "Set operation modified the original sets.");
            Assert.IsTrue (two.ContainsAll (new object [] {1, 3}), "Set operation modified the original sets.");
        }

        private ISet one;
        private ISet two;
	}
}
