using System;
using System.Data;
using DotNetMock;
using Spring.Transaction.Support;

namespace Spring.Transaction
{
	public class MyMockTxnObject : MockObject, ISmartTransactionObject
	{
		private ExpectationCounter _isRollbackOnlyCalls = new ExpectationCounter( "IsRollbackOnlyCalls" );
		private bool _isRollbackOnly = false;

		public MyMockTxnObject() : this( "MyMockTxnObject" ) {}
		public MyMockTxnObject( string name ) : base( name ) {}



	    public void SetExpectedRollbackOnlyCalls( int calls ) 
		{
			_isRollbackOnlyCalls.Expected = calls;
		}
		public void SetExpectedIsRollBackOnlyValue( bool isRollbackOnly ) 
		{
			_isRollbackOnly = isRollbackOnly;
		}
		#region ISmartTransactionObject Members

        public bool RollbackOnly
        {
            get
            {
                _isRollbackOnlyCalls.Inc();
                return _isRollbackOnly;
            }
        }

		#endregion
	}
	public class MyMockTxnObjectSavepointMgr : MockObject, ISavepointManager, ISmartTransactionObject		
	{
		private string _savepoint;
		private ExpectationString _expectedSavepoint = new ExpectationString("Savepoint");

		public void SetSavepointToReturn( string savepoint )
		{
			_savepoint = savepoint;
		}
		public void SetExpectedSavepoint( string savepoint )
		{
			_expectedSavepoint.Expected = savepoint;
		}
		#region ISmartTransactionObject Members

        public bool RollbackOnly
        {
            get
            {   // TODO:  Add MyMockTxnObjectSavepointMgr.IsRollbackOnly implementation
                return false;
            }
        }

		#endregion

		#region ISavepointManager Members
		public void ReleaseSavepoint(string savepoint)
		{
			_expectedSavepoint.Actual = savepoint;
		}

		public void CreateSavepoint( string savepoint )
		{
			_expectedSavepoint.Actual = savepoint;
		}

		public void RollbackToSavepoint(string savepoint)
		{
			_expectedSavepoint.Actual = savepoint;
		}

		#endregion
	}

	public class MockTxnPlatformMgr : MockObject, IPlatformTransactionManager
	{
		private ExpectationCounter _commitCalls = new ExpectationCounter("CommitCalls");
		private ExpectationCounter _getTransactionCalls = new ExpectationCounter("GetTxnCalls");
		private ExpectationCounter _rollbackCalls = new ExpectationCounter("RollbackCalls");
		private bool _throwRollbackException = false;

		#region IPlatformTransactionManager Members
		public void SetExpectedCommitCallCount( int count )
		{
			_commitCalls.Expected = count;
		}
		public void SetExpectedRollbackCallCount( int count )
		{
			_rollbackCalls.Expected = count;
		}
		public void SetExpectedGetTxnCallCount( int count )
		{
			_getTransactionCalls.Expected = count;
		}
		public bool ThrowRollbackException
		{
			set { _throwRollbackException = value; }	
		}
		public void Rollback(ITransactionStatus transactionStatus)
		{
			_rollbackCalls.Inc();
			if ( _throwRollbackException )
			{
				throw new Exception("Rollback");
			}
		}

		public void Commit(ITransactionStatus transactionStatus)
		{
			_commitCalls.Inc();
		}

		public ITransactionStatus GetTransaction(ITransactionDefinition definition)
		{
			_getTransactionCalls.Inc();
			return null;
		}

		#endregion

	}

	public class MockTxnSync : MockObject, ITransactionSynchronization
	{
		#region ITransactionSynchronization Members

		public void AfterCompletion(Spring.Transaction.Support.TransactionSynchronizationStatus status)
		{
			// TODO:  Add MockTxnSync.AfterCompletion implementation
		}

		public void BeforeCommit(bool readOnly)
		{
			// TODO:  Add MockTxnSync.BeforeCommit implementation
		}
	    
	    public void AfterCommit()
	    {
	        
	    }

		public void Resume()
		{
			// TODO:  Add MockTxnSync.Resume implementation
		}

		public void BeforeCompletion()
		{
			// TODO:  Add MockTxnSync.BeforeCompletion implementation
		}

		public void Suspend()
		{
			// TODO:  Add MockTxnSync.Suspend implementation
		}

		#endregion
	}

	public class MockTxnDefinition : MockObject, ITransactionDefinition
	{
		private int _transactionTimeout = DefaultTransactionDefinition.TIMEOUT_DEFAULT;
		private TransactionPropagation _transactionPropagation = TransactionPropagation.NotSupported;
		private bool _readOnly = false;
        private string _name = null;
        private System.Transactions.EnterpriseServicesInteropOption _esInteropOption;

        #region ITransactionDefinition Members

		public bool ReadOnly
		{
			get
			{
				return _readOnly;
			}
			set
			{
				_readOnly = value;
			}
		}

		public int TransactionTimeout
		{
			get
			{
				return _transactionTimeout;
			}
			set
			{
				_transactionTimeout = value;
			}
		}

		public IsolationLevel TransactionIsolationLevel
		{
			get
			{
				return IsolationLevel.Unspecified;
			}
		}

		public TransactionPropagation PropagationBehavior
		{
			get
			{
				return _transactionPropagation;
			}
			set
			{
				_transactionPropagation = value;
			}
		}

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public System.Transactions.EnterpriseServicesInteropOption EnterpriseServicesInteropOption
        {
            get { return _esInteropOption; }
            set { _esInteropOption = value; }
        }

		#endregion

	}
	public class MockTxnPlatformMgrAbstract : AbstractPlatformTransactionManager, IMockObject
	{
		private object _transaction;
		private bool _isVerified;
		private string _mockName;
		private bool _isExistingTransaction;

        private ExpectationCounter _doBeginCalls = new ExpectationCounter("DoBegin Calls");
		private ExpectationCounter _doGetTxnCalls = new ExpectationCounter("DoGetTransaction Calls");
		private ExpectationCounter _isExistingTxnCalls = new ExpectationCounter("IsExistingTransaction Calls");

		
		private bool _useSavepointForNestedTransaction;

		public void SetExpectedCalls( string method, int calls )
		{
			switch ( method )
			{
				case "DoBegin":
					_doBeginCalls.Expected = calls;
					break;
				case "IsExistingTransaction":
					_isExistingTxnCalls.Expected = calls;
					break;
				case "DoGetTransaction":
					_doGetTxnCalls.Expected = calls;
					break;
				default: 
					break;
			}
		}
		public void SetTransaction( object transaction )
		{
			_transaction = transaction;
			_isExistingTransaction = true;
		}
		public object Transaction
		{
			get { return _transaction; }
		}
		public bool Savepoints
		{
			set { _useSavepointForNestedTransaction = value; }
		}
		#region IMockObject Members

		public void NotImplemented(string methodName)
		{
			// TODO:  Add MockTxnPlatformMgrAbstract.NotImplemented implementation
		}

		public string MockName
		{
			get { return _mockName; } 
			set { _mockName = value;}
		}

		#endregion

		#region IVerifiable Members

		public void Verify()
		{
			Verifier.Verify( this );
			_isVerified = true;
		}

		public bool IsVerified
		{
			get { return _isVerified; }
		}

		#endregion

		#region AbstractPlatformTransactionManager Impls
		protected override void DoResume(object transaction, object suspendedResources)
		{

		}
		protected override void DoCommit(DefaultTransactionStatus status)
		{

		}
		protected override object DoGetTransaction()
		{
			_doGetTxnCalls.Inc();
			if ( null == _transaction )
			{
				return new object();
			}
			else
			{
				return _transaction;
			}
		}
		protected override object DoSuspend(object transaction)
		{
			return null;
		}
		protected override void DoBegin(object transaction, ITransactionDefinition definition)
		{
			_doBeginCalls.Inc();
		}
		protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
		{

		}
		protected override void DoRollback(DefaultTransactionStatus status)
		{
			
		}
		protected override bool IsExistingTransaction(object transaction)
		{
			_isExistingTxnCalls.Inc();
			return _isExistingTransaction;
		}

		protected override bool UseSavepointForNestedTransaction()
		{
			return _useSavepointForNestedTransaction;
		}

		#endregion

	}

}
