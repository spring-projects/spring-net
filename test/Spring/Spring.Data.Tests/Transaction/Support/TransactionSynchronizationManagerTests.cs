using System;
using System.Collections;
using NUnit.Framework;

namespace Spring.Transaction.Support
{
	[TestFixture]
	public class TransactionSynchronizationManagerTests
	{
		[SetUp]
		public void Init()
		{
			if ( TransactionSynchronizationManager.SynchronizationActive )
			{
				TransactionSynchronizationManager.ClearSynchronization();
			}
		}
		[TearDown]
		public void Destory()
		{
			if ( TransactionSynchronizationManager.SynchronizationActive )
			{
				TransactionSynchronizationManager.ClearSynchronization();
			}
		}
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SynchronizationsInvalid()
		{
			IList syncs = TransactionSynchronizationManager.Synchronizations;
		}
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void InitSynchronizationsInvalid()
		{
			TransactionSynchronizationManager.InitSynchronization();
			TransactionSynchronizationManager.InitSynchronization();
		}
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SynchronizationsClearInvalid()
		{
			TransactionSynchronizationManager.ClearSynchronization();
		}
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void RegisterSyncsInvalid()
		{
			TransactionSynchronizationManager.RegisterSynchronization(new MockTxnSync());
		}
		[Test]
		public void SynchronizationsLifeCycle()
		{
			TransactionSynchronizationManager.InitSynchronization();
			IList syncs = TransactionSynchronizationManager.Synchronizations;
			Assert.AreEqual( 0, syncs.Count );
			TransactionSynchronizationManager.RegisterSynchronization( new MockTxnSync() );
			syncs = TransactionSynchronizationManager.Synchronizations;
			Assert.AreEqual( 1, syncs.Count );
			TransactionSynchronizationManager.ClearSynchronization();
		}
	}
}
