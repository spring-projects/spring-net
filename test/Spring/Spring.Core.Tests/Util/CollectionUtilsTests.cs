using System.Collections;
using NUnit.Framework;
using Spring.Objects;

namespace Spring.Util;

[TestFixture]
public class CollectionUtilsTests
{
    internal class NoContainsNoAddCollection : ICollection
    {
        internal class Iterator : IEnumerator
        {
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
        Assert.That(CollectionUtils.Contains(list, null), Is.True);
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

        Assert.That(CollectionUtils.Contains(list, 3), Is.True);
    }

    [Test]
    public void ContainsDoesNotContainElement()
    {
        ArrayList list = new ArrayList();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);

        Assert.That(CollectionUtils.Contains(list, 5), Is.False);
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
        Assert.That(list.Count == 1, Is.True);
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
        Assert.That(list.Count == 1, Is.True);
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

        Assert.That(CollectionUtils.ContainsAll(target, source), Is.True);
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

        Assert.That(CollectionUtils.ContainsAll(target, source), Is.True);
    }

    [Test]
    public void ContainsAllElementsWithNoElementsInSourceCollection()
    {
        ArrayList target = new ArrayList();
        target.Add(1);
        target.Add(2);
        target.Add(3);

        ArrayList source = new ArrayList();
        Assert.That(CollectionUtils.ContainsAll(target, source), Is.True);
    }

    [Test]
    public void ContainsAllElementsWithNoElementsEitherCollection()
    {
        ArrayList target = new ArrayList();
        ArrayList source = new ArrayList();
        Assert.That(CollectionUtils.ContainsAll(target, source), Is.False);
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

        Assert.That(source.Count, Is.EqualTo(target.Count));
    }

    [Test]
    public void EmptyArrayElements()
    {
        ArrayList source = CollectionUtils.ToArrayList(new NoContainsNoAddCollection());
        Assert.That(source.Count, Is.EqualTo(0));
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
        Assert.That(3 == target.Count, Is.True);
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
        Assert.That(3 == target.Count, Is.True);
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
        Assert.That(0 == target.Count, Is.True);
    }

    [Test]
    public void IsCollectionEmptyOrNull()
    {
        ArrayList list = new ArrayList();
        Assert.That(CollectionUtils.IsEmpty(list), Is.True);
        list.Add("foo");
        Assert.That(CollectionUtils.IsEmpty(list), Is.False);
        list = null;
        Assert.That(CollectionUtils.IsEmpty(list), Is.True);
    }

    [Test]
    public void IsDictionaryEmptyOrNull()
    {
        Hashtable t = new Hashtable();
        Assert.That(CollectionUtils.IsEmpty(t), Is.True);
        t["foo"] = "bar";
        Assert.That(CollectionUtils.IsEmpty(t), Is.False);
        t = null;
        Assert.That(CollectionUtils.IsEmpty(t), Is.True);
    }

    [Test]
    public void FindValueOfType()
    {
        ArrayList list = new ArrayList();
        Assert.That(CollectionUtils.FindValueOfType(list, typeof(String)), Is.Null);
        list.Add("foo");
        object obj = CollectionUtils.FindValueOfType(list, typeof(String));
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj as string, Is.Not.Null);
        string val = obj as string;
        Assert.That(val, Is.EqualTo("foo"));

        list.Add(new TestObject("Joe", 34));
        obj = CollectionUtils.FindValueOfType(list, typeof(TestObject));
        Assert.That(obj, Is.Not.Null);
        TestObject to = obj as TestObject;
        Assert.That(to, Is.Not.Null);
        Assert.That(to.Name, Is.EqualTo("Joe"));

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
        Assert.That(strList.Length, Is.EqualTo(1));

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
        DictionaryEntry[] entries = new DictionaryEntry[] { new DictionaryEntry(5, 4), new DictionaryEntry(5, 5), new DictionaryEntry(3, 2), new DictionaryEntry(3, 3), new DictionaryEntry(1, 0), new DictionaryEntry(1, 1), };

        ICollection resultList = CollectionUtils.StableSort(entries, new CollectionUtils.CompareCallback(CompareEntries));
        DictionaryEntry[] resultEntries = (DictionaryEntry[]) CollectionUtils.ToArray(resultList, typeof(DictionaryEntry));

        Assert.That(resultEntries[0].Value, Is.EqualTo(0));
        Assert.That(resultEntries[1].Value, Is.EqualTo(1));
        Assert.That(resultEntries[2].Value, Is.EqualTo(2));
        Assert.That(resultEntries[3].Value, Is.EqualTo(3));
        Assert.That(resultEntries[4].Value, Is.EqualTo(4));
        Assert.That(resultEntries[5].Value, Is.EqualTo(5));
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
        Assert.That(CollectionUtils.FindFirstMatch(null, null), Is.Null);
        Assert.That(CollectionUtils.FindFirstMatch(new string[0], new string[0]), Is.Null);
        Assert.That(CollectionUtils.FindFirstMatch(null, new string[] { "x" }), Is.Null);
        Assert.That(CollectionUtils.FindFirstMatch(new string[] { "x" }, null), Is.Null);
    }

    [Test]
    public void FindFirstMatchReturnsFirstMatch()
    {
        ArrayList source = new ArrayList();
        string[] candidates = new string[] { "G", "B", "H" };
        source.AddRange(new string[] { "A", "B", "C" });
        Assert.That(CollectionUtils.FindFirstMatch(source, candidates), Is.EqualTo("B"));
    }
}
