using System.Threading;

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Threading
{
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
                Assert.AreEqual(0, sync.Permits);
            }
            Assert.AreEqual(1, sync.Permits);

            sync = new MySemaphore(0);
            try
            {
                using (new SyncHolder(sync, 100))
                {
                    Assert.IsTrue(false, "wrongly entered sync block");
                }
            }
            catch (TimeoutException)
            {
                Assert.AreEqual(0, sync.Permits);
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
}