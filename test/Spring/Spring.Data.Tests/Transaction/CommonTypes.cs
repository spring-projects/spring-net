using System.Data;
using Spring.Transaction.Support;

namespace Spring.Transaction
{
    public class MockTxnDefinition : ITransactionDefinition
    {
        private int _transactionTimeout = DefaultTransactionDefinition.TIMEOUT_DEFAULT;
        private TransactionPropagation _transactionPropagation = TransactionPropagation.NotSupported;
        private bool _readOnly = false;
        private string _name = null;
        private System.Transactions.EnterpriseServicesInteropOption _esInteropOption;

        #region ITransactionDefinition Members

        public bool ReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; }
        }

        public int TransactionTimeout
        {
            get { return _transactionTimeout; }
            set { _transactionTimeout = value; }
        }

        public IsolationLevel TransactionIsolationLevel
        {
            get { return IsolationLevel.Unspecified; }
        }

        public TransactionPropagation PropagationBehavior
        {
            get { return _transactionPropagation; }
            set { _transactionPropagation = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public System.Transactions.EnterpriseServicesInteropOption EnterpriseServicesInteropOption
        {
            get { return _esInteropOption; }
            set { _esInteropOption = value; }
        }

        #endregion
    }

    public class MockTxnPlatformMgrAbstract : AbstractPlatformTransactionManager
    {
        private object _transaction;
        private bool _isExistingTransaction;

        private int _doBeginCalls;
        private int _doGetTxnCalls;
        private int _isExistingTxnCalls;


        private bool _useSavepointForNestedTransaction;

        public void SetTransaction(object transaction)
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

        public int DoBeginCallCount
        {
            get { return _doBeginCalls; }
        }

        public int DoGetTransactionCallCount
        {
            get { return _doGetTxnCalls; }
        }

        public int IsExistingTransactionCallCount
        {
            get { return _isExistingTxnCalls; }
        }

        #region AbstractPlatformTransactionManager Impls

        protected override void DoResume(object transaction, object suspendedResources)
        {
        }

        protected override void DoCommit(DefaultTransactionStatus status)
        {
        }

        protected override object DoGetTransaction()
        {
            _doGetTxnCalls++;
            if (null == _transaction)
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
            _doBeginCalls++;
        }

        protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
        }

        protected override void DoRollback(DefaultTransactionStatus status)
        {
        }

        protected override bool IsExistingTransaction(object transaction)
        {
            _isExistingTxnCalls++;
            return _isExistingTransaction;
        }

        protected override bool UseSavepointForNestedTransaction()
        {
            return _useSavepointForNestedTransaction;
        }

        #endregion
    }
}