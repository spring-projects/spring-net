using System;
using NUnit.Framework;

namespace Spring.Transaction.Support
{
	[TestFixture]
	public class ResourceHolderSupportTests : ResourceHolderSupport
	{
		[TearDown]
		public void Destroy()
		{
			Clear();
		}
		[Test]
		public void PropertiesTest()
		{
			Assert.AreEqual( DateTime.MinValue, Deadline);	
			Assert.IsTrue( !SynchronizedWithTransaction );
			Assert.IsTrue( !RollbackOnly);
			
            //TODO investigate
            /*
			Deadline = DateTime.Now.AddDays(2);
			Assert.IsTrue( HasDeadline );
			Assert.AreEqual( DateTime.Now.AddDays(2).Date, Deadline.Date );
            */

			SynchronizedWithTransaction = true;
			Assert.IsTrue( SynchronizedWithTransaction );

			RollbackOnly = true;
			Assert.IsTrue( RollbackOnly);
		}
		[Test]
		public void InvalidDeadline()
		{
		    int temp;
            Assert.Throws<ArgumentException>(() => temp = TimeToLiveInSeconds);
		}

        /*
        [Ignore("investigate...")]
		[Test]
		public void ZeroMilliseconds()
		{
			Deadline = DateTime.Now.AddDays(-1);
			Assert.AreEqual( 0, TimeToLiveInMilliseconds );
		}
        */
        /*
		[Test]
		public void MillisecondToLive()
		{
			Deadline = DateTime.Now.AddDays(1);
			Assert.IsTrue( TimeToLiveInMilliseconds >= 0);
		}
        */
        /*
		[Test]
		public void SetDeadline()
		{
			SetDeadlineInSeconds( 10 );
			Assert.IsTrue( TimeToLiveInSeconds >= 0);
		}
        */
        /*
		[Test]
		public void SetDeadlineMillis()
		{
			SetDeadlineInMilliseconds(1000);
			Assert.IsTrue( TimeToLiveInMilliseconds >= 0);
		}
        */
	}
}
