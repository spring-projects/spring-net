using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Collections;

[TestFixture]
public class PriorityQueueTests
{
    protected static Int32 zero = Int32.Parse("0");
    protected static Int32 one = Int32.Parse("1");
    protected static Int32 two = Int32.Parse("2");
    protected static Int32 three = Int32.Parse("3");
    protected static Int32 four = Int32.Parse("4");
    protected static Int32 five = Int32.Parse("5");
    protected static Int32 six = Int32.Parse("6");
    protected static Int32 seven = Int32.Parse("7");
    protected static Int32 eight = Int32.Parse("8");
    protected static Int32 nine = Int32.Parse("9");
    protected static Int32 m1 = Int32.Parse("-1");
    protected static Int32 m2 = Int32.Parse("-2");
    protected static Int32 m3 = Int32.Parse("-3");
    protected static Int32 m4 = Int32.Parse("-4");
    protected static Int32 m5 = Int32.Parse("-5");
    protected static Int32 m10 = Int32.Parse("-10");
    private int SIZE = 20;

    internal class MyReverseComparator : IComparer
    {
        public virtual int Compare(Object x, Object y)
        {
            int i = (int) x;
            int j = (int) y;
            if (i < j)
                return 1;
            if (i > j)
                return -1;
            return 0;
        }
    }

    private PriorityQueue populatedQueue(int n)
    {
        PriorityQueue q = new PriorityQueue(n);
        Assert.That((q.Count == 0), Is.True);
        for (int i = n - 1; i >= 0; i -= 2)
            Assert.That(q.Offer(i), Is.True);
        for (int i = (n & 1); i < n; i += 2)
            Assert.That(q.Offer(i), Is.True);
        Assert.That((q.Count == 0), Is.False);
        Assert.That(q.Count, Is.EqualTo(n));
        return q;
    }

    [Test]
    public void CreateUnboundedQueue()
    {
        Assert.That(new PriorityQueue(SIZE).Count, Is.EqualTo(0));
    }

    [Test]
    public void ThrowsArgumentExceptionForZeroCapacity()
    {
        Assert.Throws<ArgumentException>(() => new PriorityQueue(0));
    }

    [Test]
    public void ThrowsArgumentNullExceptionForNullCollection()
    {
        Assert.Throws<ArgumentNullException>(() => new PriorityQueue(null));
    }

    [Test]
    public void ThrowsArgumentNullExceptionForNullCollectionElements()
    {
        object[] ints = new object[SIZE];
        Assert.Throws<ArgumentNullException>(() => new PriorityQueue(new ArrayList(ints)));
    }

    [Test]
    public void ThrowsArgumentNullExceptionForSomeNullCollectionElements()
    {
        object[] ints = new object[SIZE];
        for (int i = 0; i < SIZE - 1; ++i)
            ints[i] = i;
        Assert.Throws<ArgumentNullException>(() => new PriorityQueue(new ArrayList(ints)));
    }

    [Test]
    public void ConstructorFromExistingCollection()
    {
        Int32[] ints = new Int32[SIZE];
        for (int i = 0; i < SIZE; ++i)
            ints[i] = i;
        PriorityQueue q = new PriorityQueue(new ArrayList(ints));
        for (int i = 0; i < SIZE; ++i)
            Assert.That(q.Poll(), Is.EqualTo(ints[i]));
        Assert.That(q.IsEmpty, Is.True);
    }

    [Test]
    public void ConstructorUsingComparator()
    {
        MyReverseComparator cmp = new MyReverseComparator();
        PriorityQueue q = new PriorityQueue(SIZE, cmp);
        Assert.That(q.Comparator(), Is.EqualTo(cmp));
        Int32[] ints = new Int32[SIZE];
        for (int i = 0; i < SIZE; ++i)
            ints[i] = i;
        q.AddAll(new ArrayList(ints));
        for (int i = SIZE - 1; i >= 0; --i)
            Assert.That(q.Poll(), Is.EqualTo(ints[i]));
        Assert.That(q.IsEmpty, Is.True);
    }

    [Test]
    public void IsEmpty()
    {
        PriorityQueue q = new PriorityQueue(2);
        Assert.That(q.IsEmpty, Is.True);
        q.Add(1);
        Assert.That(q.IsEmpty, Is.False);
        q.Add(2);
        q.Remove();
        q.Remove();
        Assert.That(q.IsEmpty, Is.True);
    }

    [Test]
    public void Size()
    {
        PriorityQueue q = populatedQueue(SIZE);
        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That(q.Count, Is.EqualTo(SIZE - i));
            q.Remove();
        }

        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That(q.Count, Is.EqualTo(i));
            q.Add(i);
        }
    }

    [Test]
    public void OfferWithNullObject()
    {
        PriorityQueue q = new PriorityQueue(1);
        Assert.Throws<ArgumentNullException>(() => q.Offer(null));
    }

    [Test]
    public void AddWithNullObject()
    {
        PriorityQueue q = new PriorityQueue(1);
        Assert.Throws<ArgumentNullException>(() => q.Add(null));
    }

    [Test]
    public void OfferWithComparableElements()
    {
        PriorityQueue q = new PriorityQueue(1);
        Assert.That(q.Offer(zero), Is.True);
        Assert.That(q.Offer(one), Is.True);
    }

    [Test]
    public void OfferNonComparable()
    {
        PriorityQueue q = new PriorityQueue(1);
        q.Offer(new Object());
        Assert.Throws<InvalidCastException>(() => q.Offer(new Object()));
    }

    [Test]
    public void AddWithComparableElements()
    {
        PriorityQueue q = new PriorityQueue(SIZE);
        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That(q.Count, Is.EqualTo(i));
            Assert.That(q.Add(i), Is.True);
        }
    }

    [Test]
    public void AddAllWithNullElements()
    {
        PriorityQueue q = new PriorityQueue(1);
        Assert.Throws<ArgumentNullException>(() => q.AddAll(null));
    }

    [Test]
    public void AddAllWithCollectionWithNullElements()
    {
        PriorityQueue q = new PriorityQueue(SIZE);
        object[] ints = new object[SIZE];
        Assert.Throws<ArgumentNullException>(() => q.AddAll(new ArrayList(ints)));
    }

    [Test]
    public void AddAllWithCollectionWithSomeNullElements()
    {
        PriorityQueue q = new PriorityQueue(SIZE);
        object[] ints = new object[SIZE];
        for (int i = 0; i < SIZE - 1; ++i)
            ints[i] = i;
        Assert.Throws<ArgumentNullException>(() => q.AddAll(new ArrayList(ints)));
    }

    [Test]
    public void AddAllWithCollection()
    {
        Int32[] empty = new Int32[0];
        Int32[] ints = new Int32[SIZE];
        for (int i = 0; i < SIZE; ++i)
            ints[i] = (SIZE - 1 - i);
        PriorityQueue q = new PriorityQueue(SIZE);
        Assert.That(q.AddAll(new ArrayList(empty)), Is.False);
        Assert.That(q.AddAll(new ArrayList(ints)), Is.True);
        for (int i = 0; i < SIZE; ++i)
            Assert.That(q.Poll(), Is.EqualTo(i));
    }

    [Test]
    public void Poll()
    {
        PriorityQueue q = populatedQueue(SIZE);
        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That((q.Poll()), Is.EqualTo(i));
        }

        Assert.That(q.Poll(), Is.Null);
    }

    [Test]
    public void Peek()
    {
        PriorityQueue q = populatedQueue(SIZE);
        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That((q.Peek()), Is.EqualTo(i));
            q.Poll();
            Assert.That(q.Peek() == null || i != (int) (q.Peek()), Is.True);
        }

        Assert.That(q.Peek(), Is.Null);
    }

    [Test]
    public void Element()
    {
        PriorityQueue q = populatedQueue(SIZE);
        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That((q.Element()), Is.EqualTo(i));
            q.Poll();
        }

        Assert.Throws<NoElementsException>(() => q.Element());
    }

    [Test]
    public void Remove()
    {
        PriorityQueue q = populatedQueue(SIZE);
        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That((q.Remove()), Is.EqualTo(i));
        }

        Assert.Throws<NoElementsException>(() => q.Remove());
    }

    [Test]
    public void RemoveElement()
    {
        PriorityQueue q = populatedQueue(SIZE);
        for (int i = 1; i < SIZE; i += 2)
        {
            Assert.That(q.Remove(i), Is.True);
        }

        for (int i = 0; i < SIZE; i += 2)
        {
            Assert.That(q.Remove(i), Is.True);
            Assert.That(q.Remove((i + 1)), Is.False);
        }

        Assert.That((q.Count == 0), Is.True);
    }

    [Test]
    public void Contains()
    {
        PriorityQueue q = populatedQueue(SIZE);
        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That(CollectionUtils.Contains(q, i), Is.True);
            q.Poll();
            Assert.That(CollectionUtils.Contains(q, i), Is.False);
        }
    }

    [Test]
    public void Clear()
    {
        PriorityQueue q = populatedQueue(SIZE);
        q.Clear();
        Assert.That((q.Count == 0), Is.True);
        Assert.That(q.Count, Is.EqualTo(0));
        q.Add(1);
        Assert.That((q.Count == 0), Is.False);
        q.Clear();
        Assert.That((q.Count == 0), Is.True);
    }

    [Test]
    public void ContainsAll()
    {
        PriorityQueue q = populatedQueue(SIZE);
        PriorityQueue p = new PriorityQueue(SIZE);
        for (int i = 0; i < SIZE; ++i)
        {
            Assert.That(CollectionUtils.ContainsAll(q, p), Is.True);
            Assert.That(CollectionUtils.ContainsAll(p, q), Is.False);
            p.Add(i);
        }

        Assert.That(CollectionUtils.ContainsAll(p, q), Is.True);
    }

    [Test]
    public void RemoveAll()
    {
        for (int i = 1; i < SIZE; ++i)
        {
            PriorityQueue q = populatedQueue(SIZE);
            PriorityQueue p = populatedQueue(i);
            CollectionUtils.RemoveAll(q, p);
            Assert.That(q.Count, Is.EqualTo(SIZE - i));
            for (int j = 0; j < i; ++j)
            {
                Int32 I = (int) p.Remove();
                Assert.That(CollectionUtils.Contains(q, I), Is.False);
            }
        }
    }

    [Test]
    public void ToArrayObject()
    {
        PriorityQueue q = populatedQueue(SIZE);
        Object[] o = (Object[]) CollectionUtils.ToArrayList(q).ToArray(typeof(object));
        Array.Sort(o);
        for (int i = 0; i < o.Length; i++)
            Assert.That(q.Poll(), Is.EqualTo(o[i]));
    }

    [Test]
    public void ToArrayNonObject()
    {
        PriorityQueue q = populatedQueue(SIZE);
        Int32[] ints = (Int32[]) CollectionUtils.ToArrayList(q).ToArray(typeof(int));
        Array.Sort(ints);
        for (int i = 0; i < ints.Length; i++)
            Assert.That(q.Poll(), Is.EqualTo(ints[i]));
    }

    [Test]
    public void Iterator()
    {
        PriorityQueue q = populatedQueue(SIZE);
        int i = 0;
        IEnumerator it = q.GetEnumerator();
        while (it.MoveNext())
        {
            Assert.That(CollectionUtils.Contains(q, it.Current), Is.True);
            ++i;
        }

        Assert.That(SIZE, Is.EqualTo(i));
    }

    [Test]
    public void Serialization()
    {
        PriorityQueue q = populatedQueue(SIZE);
        MemoryStream bout = new MemoryStream(10000);

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(bout, q);

        MemoryStream bin = new MemoryStream(bout.ToArray());
        BinaryFormatter formatter2 = new BinaryFormatter();

        PriorityQueue r = (PriorityQueue) formatter2.Deserialize(bin);

        Assert.That(r.Count, Is.EqualTo(q.Count));
        while (!(q.Count == 0))
            Assert.That(r.Remove(), Is.EqualTo(q.Remove()));
    }

    [Test]
    public void QueueCopyToArrayWithSmallerDestinationArray()
    {
        PriorityQueue source = populatedQueue(SIZE);
        object[] dest = new object[SIZE / 2];
        Assert.Throws<ArgumentException>(() => source.CopyTo(dest));
    }

    [Test]
    public void QueueCopyToArrayWithIndexOutOfDestinationArrayRange()
    {
        PriorityQueue source = populatedQueue(SIZE);
        object[] dest = new object[SIZE / 2];
        Assert.Throws<ArgumentException>(() => source.CopyTo(dest, 11));
    }

    [Test]
    public void QueueCopyToArrayWithValidSizeButInvalidStartingIndex()
    {
        PriorityQueue source = populatedQueue(SIZE);
        object[] dest = new object[SIZE];
        Assert.Throws<IndexOutOfRangeException>(() => source.CopyTo(dest, 10));
    }

    [Test]
    public void CompleteQueueCopyToArrayWithValidSize()
    {
        PriorityQueue source = populatedQueue(SIZE);
        object[] dest = new object[SIZE];
        source.CopyTo(dest);
        for (int i = 0; i < SIZE; i++)
        {
            Assert.That(dest[i], Is.EqualTo(source.Poll()));
        }
    }

    [Test]
    public void PartialQueueCopyToArrayWithValidSize()
    {
        PriorityQueue source = populatedQueue(SIZE);
        object[] dest = new object[SIZE * 2];
        source.CopyTo(dest, SIZE / 2);
        for (int i = 0; i < SIZE; i++)
        {
            Assert.That(dest[i], Is.Null);
        }

        for (int i = SIZE; i < SIZE / 2; i++)
        {
            Assert.That(i - SIZE / 2, Is.EqualTo(dest[i]));
        }
    }
}
