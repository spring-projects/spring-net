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

namespace Spring.Collections
{
	/// <summary>
	/// Unit tests for the ImmutableSet wrapper class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public class ImmutableSetTests
    {
        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp () {
            Set = new ImmutableSet (new HybridSet (new object [] {1, 2, 3}));
        }

        /// <summary>
        /// The tear down logic executed after the execution of each individual test.
        /// </summary>
        [TearDown]
        public void TearDown () {
            Set = null;
        }

        [Test]
        public void Add()
        {
            Assert.Throws<NotSupportedException>(() => Set.Add(1));
        }

        [Test]
        public void AddAll()
        {
            Assert.Throws<NotSupportedException>(() => Set.AddAll(new int[] {4, 5, 6}));
        }

        [Test]
        public void Remove()
        {
            Assert.Throws<NotSupportedException>(() => Set.Remove(1));
        }

        [Test]
        public void RemoveAll()
        {
            object[] removed = new object[] {1, 3};
            Assert.Throws<NotSupportedException>(() => Set.RemoveAll(removed));
        }

        [Test]
        public void RetainAll()
        {
            Assert.Throws<NotSupportedException>(() => Set.RetainAll(new object[] {1, 9}));
        }

        [Test]
        public void Clear()
        {
            Assert.Throws<NotSupportedException>(() => Set.Clear());
        }        

        [Test]
        public void IsEmpty() {
            Assert.IsFalse (Set.IsEmpty);
            ISet mySet = new ImmutableSet (new HybridSet ());
            Assert.IsTrue (mySet.IsEmpty);
        }       

        [Test]
        public void Contains() {
            Assert.IsFalse (Set.Contains ("Funk"));
            Assert.IsTrue (Set.Contains (1));
        }

        [Test]
        public void ContainsAll()
        {
            Assert.IsFalse(Set.ContainsAll(new object[] {"Funk", 1, 2, 3}));
            Assert.IsTrue(Set.ContainsAll(new object[] {1, 3, 2}));
        }

        [Test]
        public void CopyTo() {
            int [] expected = new int [] {1, 2, 3};
            int [] actual = new int [Set.Count];
            Set.CopyTo (actual, 0);
            Assert.AreEqual (expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i) {
                Assert.AreEqual (expected [i], actual [i]);
            }
        } 

        [Test]
        public void EnumeratesOk () {
            int [] expected = new int [] {1, 2, 3};
            int i = 0;
            foreach (int actual in Set) {
                Assert.AreEqual (expected [i++], actual);
            }
        }

        [Test]
        public void ClonedInstanceMustStillBeImmutable()
        {
            ISet clone = (ISet) Set.Clone();
            Assert.Throws<NotSupportedException>(() => clone.Add("bad chair, bad chair"));
        }

        [Test]
        public void MinusYieldsImmutableCone()
        {
            ISet mySet = new ListSet(new int[] {1, 2});
            ISet clone = Set.Minus(mySet);
            Assert.IsNotNull(clone);
            Assert.AreEqual(1, clone.Count);
            Assert.Throws<NotSupportedException>(() => clone.Add("bad chair, bad chair"));
        }

        [Test]
        public void UnionYieldsImmutableCone()
        {
            ISet mySet = new ListSet(new int[] {1, 4});
            ISet clone = Set.Union(mySet);
            Assert.IsNotNull(clone);
            Assert.AreEqual(4, clone.Count);
            Assert.Throws<NotSupportedException>(() => clone.Add("bad chair, bad chair"));
        }

        [Test]
        public void IntersectionYieldsImmutableCone()
        {
            ISet mySet = new ListSet(new int[] {1, 4});
            ISet clone = Set.Intersect(mySet);
            Assert.IsNotNull(clone);
            Assert.AreEqual(1, clone.Count);
            Assert.Throws<NotSupportedException>(() => clone.Add("bad chair, bad chair"));
        }

        [Test]
        public void ExclusiveOrYieldsImmutableCone()
        {
            ISet mySet = new ListSet(new int[] {1, 4});
            ISet clone = Set.ExclusiveOr(mySet);
            Assert.IsNotNull(clone);
            Assert.AreEqual(3, clone.Count);
            Assert.Throws<NotSupportedException>(() => clone.Add("bad chair, bad chair"));
        }

        /// <summary>
        /// The ISet being tested.
        /// </summary>
        protected virtual ISet Set 
        {
            get 
            {
                return _set;
            }
            set 
            {
                _set = value;
            }
        }

        private ISet _set;
	}
}
