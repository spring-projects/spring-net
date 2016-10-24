using NUnit.Framework;

namespace Spring.Transaction.Support
{
	[TestFixture]
	public class TransactionSynchronizationAdapterTests : TransactionSynchronizationAdapter
	{
		[Test]
		public void CoverageTests()
		{
			AfterCompletion( TransactionSynchronizationStatus.Committed );
			BeforeCommit(false);
			BeforeCompletion();
			Resume();
			Suspend();
		}
	}
}
