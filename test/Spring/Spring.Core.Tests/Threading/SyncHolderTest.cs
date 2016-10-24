using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Threading
{
    [TestFixture]
	public class SyncHolderTest
	{
        private MockRepository mocks;
        ISync sync;

        [SetUp]
        public void SetUp ()
        {
            mocks = new MockRepository();
            sync = mocks.StrictMock<ISync>();            
        }

        [TearDown]
        public void TearDown ()
        {
            mocks.VerifyAll();
        }

        class MySemaphore : Semaphore
        {
            public MySemaphore (long initialPermits) : base (initialPermits)
            {}
        }

        [Test]
        public void CanBeUsedWithTheUsingCSharpIdiomToAttemptOnAnISync ()
        {
            // no expectations
            mocks.ReplayAll();

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
            mocks.ReplayAll();

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
