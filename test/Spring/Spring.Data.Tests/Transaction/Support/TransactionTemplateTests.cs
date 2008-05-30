using System;
using NUnit.Framework;

namespace Spring.Transaction.Support
{
	[TestFixture]
	public class TransactionTemplateTests
	{
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void NoTxnMgr()
		{
			TransactionTemplate temp = new TransactionTemplate();
			temp.AfterPropertiesSet();
		}
		[Test]
			public void TxnMgr()
		{
			TransactionTemplate temp = new TransactionTemplate();
			temp.PlatformTransactionManager = new MockTxnPlatformMgr();
			temp.AfterPropertiesSet();
		}
		[Test]
		public void ExecuteException()
		{
			MockTxnPlatformMgr mock = new MockTxnPlatformMgr();
			mock.SetExpectedGetTxnCallCount(1);
			mock.SetExpectedRollbackCallCount(1);
			TransactionTemplate temp = new TransactionTemplate(mock);
			try 
			{
				temp.Execute(new TransactionDelegate(DummyExceptionMethod));
				Assert.Fail("Should throw exception");
			} catch
			{
				
			}
			mock.Verify();
		}
		[Test]
		public void ExecuteExceptionRollbackException()
		{
			MockTxnPlatformMgr mock = new MockTxnPlatformMgr();
			mock.SetExpectedGetTxnCallCount(1);
			mock.SetExpectedRollbackCallCount(1);
			mock.ThrowRollbackException = true;
			TransactionTemplate temp = new TransactionTemplate(mock);
			try 
			{
				temp.Execute(new TransactionDelegate(DummyExceptionMethod));
				Assert.Fail("Should throw exception");
			} 
			catch
			{
				
			}
			mock.Verify();
		}
		[Test]
		public void NullResult()
		{
			MockTxnPlatformMgr mock = new MockTxnPlatformMgr();
			mock.SetExpectedGetTxnCallCount(1);
			mock.SetExpectedCommitCallCount(1);
			TransactionTemplate temp = new TransactionTemplate(mock);
			temp.AfterPropertiesSet();
			Assert.AreEqual( mock, temp.PlatformTransactionManager);
			Assert.IsNull( temp.Execute(new TransactionDelegate(DummyTransactionMethod) ) );
			mock.Verify();
		}
		public object DummyTransactionMethod( ITransactionStatus status )
		{
			return status;
		}
		public object DummyExceptionMethod( ITransactionStatus status )
		{
			throw new Exception("Bad Error");
		}
	}
}
