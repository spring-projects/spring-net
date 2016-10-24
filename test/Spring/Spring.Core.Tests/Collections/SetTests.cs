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
	/// <b>Base</b> class for tests on the <see cref="Spring.Collections.ISet"/>
	/// interface.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This defines a set of tests that exercise the contract of the set interface.
    /// Subclasses need only supply a concrete ISET imnplementation instance... this
    /// instance must be set on the Set property exposed by this class in the
    /// SetUp method. The Set instance must be empty for the start of each new test.
    /// </p>
    /// <p>
    /// The SetForSetOps needs to be set during the SetUp method too.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans</author>
    public abstract class SetTests
    {
        protected static readonly object StuffOne = "This Is";

        protected static readonly object StuffTwo = "Uncle";

        protected static readonly object StuffThree = "Bob";

        protected readonly object [] UniqueStuff = new object [] {StuffOne, StuffTwo, StuffThree};

        protected readonly object [] DuplicatedStuff = new object [] {StuffOne, StuffTwo, StuffTwo};

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        public abstract void SetUp ();

        /// <summary>
        /// The tear down logic executed after the execution of each individual test.
        /// </summary>
        [TearDown]
        public virtual void TearDown () {
            Set = null;
            SetForSetOps = null;
        }

        [Test]
        public void Union() 
        {
            Set.AddAll (UniqueStuff);
            SetForSetOps.AddAll (new object [] {"Bar", StuffOne});
            ISet union = Set.Union (SetForSetOps);
            Assert.IsTrue (union.Count == UniqueStuff.Length + 1);
            Assert.IsFalse (object.ReferenceEquals (union, Set), "Got back same instance after set operation.");
            Assert.IsTrue (Set.Count == UniqueStuff.Length);
            Assert.IsTrue (SetForSetOps.Count == 2);
        }

        [Test]
        public void Intersect() 
        {
            Set.AddAll (UniqueStuff);
            SetForSetOps.AddAll (new object [] {"Bar", StuffOne});
            ISet intersection = Set.Intersect (SetForSetOps);
            Assert.IsTrue (intersection.Count == 1);
            Assert.IsFalse (object.ReferenceEquals (intersection, Set), "Got back same instance after set operation.");
            Assert.IsTrue (Set.Count == UniqueStuff.Length);
            Assert.IsTrue (SetForSetOps.Count == 2);
        }

        [Test]
        public void Minus()
        {
            Set.AddAll (UniqueStuff);
            SetForSetOps.AddAll (new object [] {"Bar", StuffOne});
            ISet minus = Set.Minus (SetForSetOps);
            Assert.IsTrue (minus.Count == UniqueStuff.Length - 1);
            Assert.IsFalse (object.ReferenceEquals (minus, Set), "Got back same instance after set operation.");
            Assert.IsTrue (Set.Count == UniqueStuff.Length);
            Assert.IsTrue (SetForSetOps.Count == 2);
        }

        [Test]
        public void ExclusiveOr()
        {
            Set.AddAll (UniqueStuff);
            SetForSetOps.AddAll (new object [] {"Bar", StuffOne});
            ISet xor = Set.ExclusiveOr (SetForSetOps);
            Assert.IsTrue (xor.Count == 3);
            Assert.IsTrue (xor.ContainsAll (new object [] {"Bar", StuffTwo, StuffThree}));
            Assert.IsFalse (object.ReferenceEquals (xor, Set), "Got back same instance after set operation.");
            Assert.IsTrue (Set.Count == UniqueStuff.Length);
            Assert.IsTrue (SetForSetOps.Count == 2);
        }

        [Test]
        public void Contains()
        {
            Set.AddAll (UniqueStuff);
            Assert.IsTrue (Set.Contains (StuffThree));
            Assert.IsFalse (Set.Contains ("SourDough"));
        }

        [Test]
        public void ContainsAll()
        {
            Set.AddAll (UniqueStuff);
            Assert.IsTrue (Set.ContainsAll (new object [] {StuffThree, StuffTwo, StuffOne}));
        }

        [Test]
        public void IsEmpty ()
        {
            Set.Add (StuffOne);
            Set.Remove (StuffOne);
            Assert.IsTrue (Set.IsEmpty);
        }

        [Test]
        public void Add()
        {
            Assert.IsTrue (Set.Add (StuffOne));
            Assert.IsTrue (Set.Count == 1, "Added 1 unique item, but the Count property wasn't sitting at 1.");
        }

        [Test]
        public void AddNull()
        {
            if (SupportsNull) 
            {
                Assert.IsTrue (Set.Add (StuffOne));
                Assert.IsTrue (Set.Add (null));
                Assert.IsTrue (Set.Count == 2, "Added 2 unique item (one null), but the Count property wasn't sitting at 2.");
                Assert.IsFalse (Set.Add (null));
                Assert.IsTrue (Set.Count == 2, "Added null to a set already containing null, but the Count property changed.");
            }
        }

        [Test]
        public void EnumeratesNull()
        {
            if (SupportsNull) 
            {
                Set.AddAll (new object [] {StuffOne, null, StuffTwo});
                bool gotNull = false;
                foreach (object o in Set) 
                {
                    if (o == null)
                    {
                        gotNull = true;
                        break;
                    }
                }
                Assert.IsTrue (gotNull, "Stuffed a null value into the set but didn't get it back when enumerating over the set.");
            }
        }

        [Test]
        public void CopiesNull () 
        {
            if (SupportsNull) 
            {
                object [] expected = new object [] {StuffOne, null, StuffTwo};
                Set.AddAll (expected);
                object [] actual = new object [expected.Length];
                Set.CopyTo (actual, 0);
                bool gotNull = false;
                foreach (object o in actual) 
                {
                    if (o == null)
                    {
                        gotNull = true;
                        break;
                    }
                }
                Assert.IsTrue (gotNull, "Copied a set with a null value into an array, but the resulting array did not contain a a null.");
            }
        }

        [Test]
        public void AddDuplicate()
        {
            Set.Add (StuffOne);
            Assert.IsFalse (Set.Add (StuffOne));
            Assert.IsTrue (Set.Count == 1, "Added 2 duplicate items, but the Count property wasn't sitting at 1.");
        }

        [Test]
        public void AddAll()
        {
            Assert.IsTrue (Set.AddAll (UniqueStuff));
            Assert.IsTrue (Set.Count == UniqueStuff.Length, "Added 3 unique items, but the Count property wasn't sitting at 3.");
        }

        [Test]
        public void AddAllDuplicated()
        {
            Assert.IsTrue (Set.AddAll (DuplicatedStuff));
            Assert.IsTrue (Set.Count == 2, "Added 3 (2 duplicated) items, but the Count property wasn't sitting at 2.");
        }

        [Test]
        public void Remove()
        {
            Set.AddAll (UniqueStuff);
            Set.Remove (StuffOne);
            Assert.IsTrue (Set.Count == (UniqueStuff.Length - 1));
            Assert.IsFalse (Set.Contains (StuffOne));
            Assert.IsTrue (Set.Contains (StuffTwo));
            Assert.IsTrue (Set.Contains (StuffThree));
        }

        [Test]
        public void RemoveNull()
        {
            if (SupportsNull) 
            {
                Set.AddAll (UniqueStuff);
                Assert.IsFalse (Set.Remove (null));
                Set.Add (null);
                Assert.IsTrue (Set.Remove (null));
            }
        }

        [Test]
        public void RemoveAll()
        {
            Set.AddAll (UniqueStuff);
            object [] removed = new object [] {StuffOne, StuffTwo};
            Set.RemoveAll (removed);
            Assert.IsTrue (Set.Count == (UniqueStuff.Length - removed.Length));
            Assert.IsFalse (Set.Contains (StuffOne));
            Assert.IsFalse (Set.Contains (StuffTwo));
            Assert.IsTrue (Set.Contains (StuffThree));
        }

        [Test]
        public void RetainAll()
        {
            Set.AddAll (UniqueStuff);
            Set.RetainAll (new object [] {StuffOne, StuffTwo});
            Assert.IsTrue (Set.Count == 2);
            Assert.IsTrue (Set.Contains (StuffOne));
            Assert.IsTrue (Set.Contains (StuffTwo));
            Assert.IsFalse (Set.Contains (StuffThree));
        }

        [Test]
        public void Clear()
        {
            Set.AddAll (UniqueStuff);
            Set.Clear ();
            Assert.IsTrue (Set.Count == 0, "Calling Clear () did not remove all of the elements.");
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

        /// <summary>
        /// An extra ISet instance for use during the set operation tests.
        /// </summary>
        protected virtual ISet SetForSetOps
        {
            get 
            {
                return _setForSetOps;
            }
            set 
            {
                _setForSetOps = value;
            }
        }

        /// <summary>
        /// Does the Set being tested support the addition of null values?
        /// </summary>
        /// <remarks>
        /// <p>
        /// If true, then the tests that test the handling of the null value
        /// will be executed.
        /// </p>
        /// </remarks>
        protected bool SupportsNull
        {
            get 
            {
                return _setSupportsNullValue;
            }
            set 
            {
                _setSupportsNullValue = value;
            }
        }

        private ISet _set;
        private ISet _setForSetOps;
        private bool _setSupportsNullValue = true;
	}
}
