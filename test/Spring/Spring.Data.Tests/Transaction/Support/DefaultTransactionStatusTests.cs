using System;
using NUnit.Framework;
using Spring.Transaction;

namespace Spring.Transaction.Support
{
	[TestFixture]
	public class DefaultTransactionStatusTests
	{
		[Test]
		public void DefaultConstructorTests() 
		{
			MyMockTxnObject txn = new MyMockTxnObject();
			txn.SetExpectedIsRollBackOnlyValue( false );
			txn.SetExpectedRollbackOnlyCalls( 1 );

			DefaultTransactionStatus stat = new DefaultTransactionStatus( txn, true, false, false, true, new object() );
			Assert.IsNotNull( stat.Transaction );
			Assert.IsTrue( !stat.ReadOnly );
			Assert.IsTrue( !stat.NewSynchronization );
			Assert.IsNotNull( stat.SuspendedResources );
			Assert.IsTrue( stat.IsNewTransaction );
			Assert.IsTrue( ! stat.RollbackOnly );
			stat.SetRollbackOnly();
			Assert.IsTrue( stat.RollbackOnly );
			txn.Verify();
		}
		[Test]
		[ExpectedException(typeof(NestedTransactionNotSupportedException))]
		public void CreateSavepointException()
		{
			DefaultTransactionStatus stat = new DefaultTransactionStatus( new MyMockTxnObject(), true, false, false, true, new object() );
			stat.CreateSavepoint( "mySavePoint" );
		}
		[Test]
		[ExpectedException(typeof(NestedTransactionNotSupportedException))]
		public void RollbackSavepointException()
		{
			DefaultTransactionStatus stat = new DefaultTransactionStatus( new MyMockTxnObject(), true, false, false, true, new object() );
			stat.RollbackToSavepoint(null);
		}
		[Test]
		[ExpectedException(typeof(NestedTransactionNotSupportedException))]
		public void ReleaseSavepointException()
		{
			DefaultTransactionStatus stat = new DefaultTransactionStatus( new MyMockTxnObject(), true, false, false, true, new object() );
			stat.ReleaseSavepoint(null);
		}
		[Test]
		public void CreateSaveAndHoldValidSavepoint()
		{
			MyMockTxnObjectSavepointMgr saveMgr = new MyMockTxnObjectSavepointMgr();
			saveMgr.SetSavepointToReturn( "savepoint" );
			DefaultTransactionStatus status = new DefaultTransactionStatus( saveMgr , true, false, false, true, new object());
			status.CreateAndHoldSavepoint( "savepoint" );
			Assert.IsTrue( status.HasSavepoint );
			Assert.AreEqual( "savepoint", status.Savepoint );
		}
		[Test]
		[ExpectedException(typeof(TransactionUsageException))]
		public void RollbackHeldSavepointException()
		{
			DefaultTransactionStatus stat = new DefaultTransactionStatus( new MyMockTxnObject(), true, false, false, true, new object() );
			stat.RollbackToHeldSavepoint();
		}
		[Test]
		public void RollbackHeldSavepointSuccess()
		{
			MyMockTxnObjectSavepointMgr saveMgr = new MyMockTxnObjectSavepointMgr();
			string savepoint = "savepoint";
			saveMgr.SetExpectedSavepoint( savepoint );
			saveMgr.SetSavepointToReturn( savepoint );
			DefaultTransactionStatus status = new DefaultTransactionStatus( saveMgr , true, false, false, true, new object());
			status.CreateAndHoldSavepoint( savepoint );
			Assert.IsTrue( status.HasSavepoint );
			Assert.AreEqual( savepoint, status.Savepoint );

			status.RollbackToHeldSavepoint();
			saveMgr.Verify();
		}
		[Test]
		[ExpectedException(typeof(TransactionUsageException))]
		public void ReleaseHeldSavepointException()
		{
			DefaultTransactionStatus stat = new DefaultTransactionStatus( new MyMockTxnObject(), true, false, false, true, new object() );
			stat.ReleaseHeldSavepoint();
		}
		[Test]
		public void ReleaseHeldSavepointSuccess()
		{
			MyMockTxnObjectSavepointMgr saveMgr = new MyMockTxnObjectSavepointMgr();
			string savepoint = "savepoint";
			saveMgr.SetExpectedSavepoint( savepoint );
			saveMgr.SetSavepointToReturn( savepoint );
			DefaultTransactionStatus status = new DefaultTransactionStatus( saveMgr , true, false, false, true, new object());
			status.CreateAndHoldSavepoint( savepoint );
			Assert.IsTrue( status.HasSavepoint );
			Assert.AreEqual( savepoint, status.Savepoint );

			status.ReleaseHeldSavepoint();
			saveMgr.Verify();
		}
	}
}
