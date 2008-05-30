using System.Threading;
using DotNetMock.Dynamic;
using NUnit.Framework;

namespace Spring.Threading
{
    [TestFixture]
	public class SyncHolderTest
	{
        DynamicMock mock;
        ISync sync;

        [SetUp]
        public void SetUp ()
        {
            mock = new DynamicMock(typeof(ISync));
            sync = (ISync) mock.Object;            
        }

        [TearDown]
        public void TearDown ()
        {
            mock.Verify();            
        }

        class MySemaphore : Semaphore
        {
            public MySemaphore (long initialPermits) : base (initialPermits)
            {}
        }

        [Test]
        public void CanBeUsedWithTheUsingCSharpIdiomToAttemptOnAnISync ()
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
        [ExpectedException(typeof(ThreadStateException))]
		public void CanBeUsedWithTheUsingCSharpIdiomToAcquireAnIsync()
		{
            mock.Expect("Acquire");
            mock.Expect("Release");
            using (new SyncHolder(sync))
            {
                throw new ThreadStateException();
            }
        }
	}
}
