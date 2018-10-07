using System.Data;
using Spring.Transaction.Support;

namespace Spring.Transaction
{
    public class MockTxnDefinition : ITransactionDefinition
    {
        public bool ReadOnly { get; set; }

        public int TransactionTimeout { get; set; } = DefaultTransactionDefinition.TIMEOUT_DEFAULT;

        public IsolationLevel TransactionIsolationLevel => IsolationLevel.Unspecified;

        public TransactionPropagation PropagationBehavior { get; set; } = TransactionPropagation.NotSupported;

        public string Name { get; } = null;

        public System.Transactions.TransactionScopeAsyncFlowOption AsyncFlowOption { get; set; }
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

        public object Transaction => _transaction;

        public bool Savepoints
        {
            set => _useSavepointForNestedTransaction = value;
        }

        public int DoBeginCallCount => _doBeginCalls;

        public int DoGetTransactionCallCount => _doGetTxnCalls;

        public int IsExistingTransactionCallCount => _isExistingTxnCalls;

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
    }
}