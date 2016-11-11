using System;
using System.Collections;
using NUnit.Framework;
using Spring.Objects;

namespace Spring.Util
{
    [TestFixture]
    public class CollectionUtilsTests
    {
        internal class NoContainsNoAddCollection : ICollection
        {
            internal class Iterator : IEnumerator
            {
                #region IEnumerator Members

                public void Reset()
                {
                    // TODO:  Add Iterator.Reset implementation
                }

                public object Current
                {
                    get
                    {
                        // TODO:  Add Iterator.Current getter implementation
                        return null;
                    }
                }

                public bool MoveNext()
                {
                    // TODO:  Add Iterator.MoveNext implementation
                    return false;
                }

                #endregion

            }

            public void CopyTo(Array array, int index)
            {
                return;
            }

            public int Count
            {
                get { return 0; }
            }

            public object SyncRoot
            {
                get { return this; }
            }

            public bool IsSynchronized
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerator GetEnumerator()
            {
                return new Iterator();
            }
        }
        [Test]
        public void ContainsNullCollection()
        {
            CollectionUtils.Contains(null, null);
        }
        [Test]
        public void ContainsNullObject()
        {
            ArrayList list = new ArrayList();
            list.Add(null);
            Assert.IsTrue(CollectionUtils.Contains(list, null));
        }
        [Test]
        public void ContainsCollectionThatDoesNotImplementContains()
        {
            NoContainsNoAddCollection noAddCollection = new NoContainsNoAddCollection();            
            CollectionUtils.Contains(noAddCollection, new object());
        }

        [Test]
        public void ContainsValidElement()
        {
            ArrayList list = new ArrayList();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            Assert.IsTrue(CollectionUtils.Contains(list, 3));
        }

        [Test]
        public void ContainsDoesNotContainElement()
        {
            ArrayList list = new ArrayList();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            Assert.IsFalse(CollectionUtils.Contains(list, 5));
        }

        [Test]
        public void AddNullCollection()
        {
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.Add(null, null));
        }

        [Test]
        public void AddNullObject()
        {
            ArrayList list = new ArrayList();
            CollectionUtils.Add(list, null);
            Assert.IsTrue(list.Count == 1);
        }

        [Test]
        public void AddCollectionDoesNotImplementAdd()
        {
            NoContainsNoAddCollection noAddCollection = new NoContainsNoAddCollection();
            Assert.Throws<InvalidOperationException>(() => CollectionUtils.Add(noAddCollection, null));
        }

        [Test]
        public void AddValidElement()
        {
            ArrayList list = new ArrayList();
            object obj1 = new object();
            CollectionUtils.Add(list, obj1);
            Assert.IsTrue(list.Count == 1);
        }

        [Test]
        public void ContainsAllNullTargetCollection()
        {
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.ContainsAll(null, new ArrayList()));
        }

        [Test]
        public void ContainsAllSourceNullCollection()
        {
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.ContainsAll(new ArrayList(), null));
        }

        [Test]
        public void ContainsAllDoesNotImplementContains()
        {
            Assert.Throws<InvalidOperationException>(() => CollectionUtils.ContainsAll(new NoContainsNoAddCollection(), new ArrayList()));
        }

        [Test]
        public void DoesNotContainAllElements()
        {
            ArrayList target = new ArrayList();
            target.Add(1);
            target.Add(2);
            target.Add(3);

            ArrayList source = new ArrayList();
            source.Add(1);

            Assert.IsTrue(CollectionUtils.ContainsAll(target, source));
        }

        [Test]
        public void ContainsAllElements()
        {
            ArrayList target = new ArrayList();
            target.Add(1);
            target.Add(2);
            target.Add(3);

            ArrayList source = new ArrayList();
            source.Add(1);
            source.Add(2);
            source.Add(3);

            Assert.IsTrue(CollectionUtils.ContainsAll(target, source));
        }
        [Test]
        public void ContainsAllElementsWithNoElementsInSourceCollection()
        {
            ArrayList target = new ArrayList();
            target.Add(1);
            target.Add(2);
            target.Add(3);

            ArrayList source = new ArrayList();
            Assert.IsTrue(CollectionUtils.ContainsAll(target, source));
        }

        [Test]
        public void ContainsAllElementsWithNoElementsEitherCollection()
        {
            ArrayList target = new ArrayList();
            ArrayList source = new ArrayList();
            Assert.IsFalse(CollectionUtils.ContainsAll(target, source));
        }

        [Test]
        public void ToArrayNullTargetCollection()
        {
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.ToArrayList(null));
        }

        [Test]
        public void ToArrayAllElements()
        {
            ArrayList target = new ArrayList();
            target.Add(1);
            target.Add(2);
            target.Add(3);

            ArrayList source = CollectionUtils.ToArrayList(target);

            Assert.AreEqual(target.Count, source.Count);
        }

        [Test]
        public void EmptyArrayElements()
        {
            ArrayList source = CollectionUtils.ToArrayList(new NoContainsNoAddCollection());
            Assert.AreEqual(0, source.Count);
        }

        [Test]
        public void RemoveAllTargetNullCollection()
        {
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.RemoveAll(null, new ArrayList()));
        }

        [Test]
        public void RemoveAllSourceNullCollection()
        {
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.RemoveAll(new ArrayList(), null));
        }

        [Test]
        public void RemoveAllTargetCollectionDoesNotImplementContains()
        {
            Assert.Throws<InvalidOperationException>(() => CollectionUtils.RemoveAll(new NoContainsNoAddCollection(), new ArrayList()));
        }

        [Test]
        public void RemoveAllTargetCollectionDoesNotImplementRemove()
        {
            Assert.Throws<InvalidOperationException>(() => CollectionUtils.RemoveAll(new NoContainsNoAddCollection(), new ArrayList()));
        }

        [Test]
        public void RemoveAllNoElements()
        {
            ArrayList target = new ArrayList();
            target.Add(1);
            target.Add(2);
            target.Add(3);

            ArrayList source = new ArrayList();
            source.Add(4);
            source.Add(5);
            source.Add(6);

            CollectionUtils.RemoveAll(target, source);
            Assert.IsTrue(3 == target.Count);
        }

        [Test]
        public void RemoveAllSomeElements()
        {
            ArrayList target = new ArrayList();
            target.Add(1);
            target.Add(2);
            target.Add(3);
            target.Add(4);
            target.Add(5);

            ArrayList source = new ArrayList();
            source.Add(4);
            source.Add(5);
            source.Add(6);

            CollectionUtils.RemoveAll(target, source);
            Assert.IsTrue(3 == target.Count);
        }

        [Test]
        public void RemoveAllAllElements()
        {
            ArrayList target = new ArrayList();
            target.Add(1);
            target.Add(2);
            target.Add(3);
            target.Add(4);
            target.Add(5);

            ArrayList source = new ArrayList();
            source.Add(1);
            source.Add(2);
            source.Add(3);
            source.Add(4);
            source.Add(5);
            source.Add(6);

            CollectionUtils.RemoveAll(target, source);
            Assert.IsTrue(0 == target.Count);
        }

        [Test]
        public void IsCollectionEmptyOrNull()
        {
            ArrayList list = new ArrayList();
            Assert.IsTrue(CollectionUtils.IsEmpty(list));
            list.Add("foo");
            Assert.IsFalse(CollectionUtils.IsEmpty(list));
            list = null;
            Assert.IsTrue(CollectionUtils.IsEmpty(list));
        }

        [Test]
        public void IsDictionaryEmptyOrNull()
        {
            Hashtable t = new Hashtable();
            Assert.IsTrue(CollectionUtils.IsEmpty(t));
            t["foo"] = "bar";
            Assert.IsFalse(CollectionUtils.IsEmpty(t));
            t = null;
            Assert.IsTrue(CollectionUtils.IsEmpty(t));
        }

        [Test]
        public void FindValueOfType()
        {
            ArrayList list = new ArrayList();
            Assert.IsNull(CollectionUtils.FindValueOfType(list, typeof(String)));
            list.Add("foo");
            object obj = CollectionUtils.FindValueOfType(list, typeof(String));
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj as string);
            string val = obj as string;
            Assert.AreEqual("foo", val);

            list.Add(new TestObject("Joe", 34));
            obj = CollectionUtils.FindValueOfType(list, typeof(TestObject));
            Assert.IsNotNull(obj);
            TestObject to = obj as TestObject;
            Assert.IsNotNull(to);
            Assert.AreEqual("Joe", to.Name);

            list.Add(new TestObject("Mary", 33));
            try
            {
                obj = CollectionUtils.FindValueOfType(list, typeof(TestObject));
                Assert.Fail("Should have thrown exception");
            }
            catch (ArgumentException)
            {
                //ok
            }
        }

        [Test]
        public void ToArray()
        {
            ArrayList list = new ArrayList();
            list.Add("mystring");
            string[] strList = (string[]) CollectionUtils.ToArray(list, typeof(string));
            Assert.AreEqual(1, strList.Length);

            try
            {
                CollectionUtils.ToArray(list, typeof(Type));
                Assert.Fail("should fail");
            }
            catch (InvalidCastException)
            {
            }
        }

        [Test]
        public void StableSorting()
        {
            DictionaryEntry[] entries = new DictionaryEntry[]
            {
                new DictionaryEntry(5, 4),
                new DictionaryEntry(5, 5),
                new DictionaryEntry(3, 2),
                new DictionaryEntry(3, 3),
                new DictionaryEntry(1, 0),
                new DictionaryEntry(1, 1),
            };

            ICollection resultList = CollectionUtils.StableSort(entries, new CollectionUtils.CompareCallback(CompareEntries));
            DictionaryEntry[] resultEntries = (DictionaryEntry[]) CollectionUtils.ToArray(resultList, typeof(DictionaryEntry));

            Assert.AreEqual(0, resultEntries[0].Value);
            Assert.AreEqual(1, resultEntries[1].Value);
            Assert.AreEqual(2, resultEntries[2].Value);
            Assert.AreEqual(3, resultEntries[3].Value);
            Assert.AreEqual(4, resultEntries[4].Value);
            Assert.AreEqual(5, resultEntries[5].Value);
        }

        private int CompareEntries(object x, object y)
        {
            DictionaryEntry dex = (DictionaryEntry) x;
            DictionaryEntry dey = (DictionaryEntry) y;

            return ((int) dex.Key).CompareTo(dey.Key);
        }

        [Test]
        public void FindFirstMatchReturnsNullIfAnyInputIsEmpty()
        {
            Assert.IsNull(CollectionUtils.FindFirstMatch(null, null));
            Assert.IsNull(CollectionUtils.FindFirstMatch(new string[0], new string[0]));
            Assert.IsNull(CollectionUtils.FindFirstMatch(null, new string[] {"x"}));
            Assert.IsNull(CollectionUtils.FindFirstMatch(new string[] {"x"}, null));
        }

        [Test]
        public void FindFirstMatchReturnsFirstMatch()
        {
            ArrayList source = new ArrayList();
            string[] candidates = new string[] { "G", "B", "H" };
            source.AddRange( new string[] { "A", "B", "C" } );
            Assert.AreEqual( "B" , CollectionUtils.FindFirstMatch(source, candidates));
        }
    }
}
