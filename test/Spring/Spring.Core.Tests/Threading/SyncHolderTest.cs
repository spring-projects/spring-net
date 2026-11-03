using FakeItEasy;
using NUnit.Framework;

namespace Spring.Threading;

[TestFixture]
public class SyncHolderTest
{
    ISync sync;

    [SetUp]
    public void SetUp()
    {
        sync = A.Fake<ISync>();
    }

    [TearDown]
    public void TearDown()
    {
    }

    class MySemaphore : Semaphore
    {
        public MySemaphore(long initialPermits) : base(initialPermits)
        {
        }
    }

    [Test]
    public void CanBeUsedWithTheUsingCSharpIdiomToAttemptOnAnISync()
    {
        MySemaphore sync = new MySemaphore(1);
        using (new SyncHolder(sync, 100))
        {
            Assert.That(sync.Permits, Is.EqualTo(0));
        }

        Assert.That(sync.Permits, Is.EqualTo(1));

        sync = new MySemaphore(0);
        try
        {
            using (new SyncHolder(sync, 100))
            {
                Assert.Fail("wrongly entered sync block");
            }
        }
        catch (TimeoutException)
        {
            Assert.That(sync.Permits, Is.EqualTo(0));
        }
    }

    [Test]
    public void CanBeUsedWithTheUsingCSharpIdiomToAcquireAnIsync()
    {
        sync.Acquire();
        sync.Release();

        Assert.Throws<ThreadStateException>(() =>
        {
            using (new SyncHolder(sync))
            {
                throw new ThreadStateException();
            }
        });
    }
}
