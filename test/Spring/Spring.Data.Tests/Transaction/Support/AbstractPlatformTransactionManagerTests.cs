using System;
using NUnit.Framework;

namespace Spring.Transaction.Support
{
    [TestFixture]
    public class AbstractPlatformTransactionManagerTests
    {
        private MockTxnPlatformMgrAbstract _mockTxnMgr;

        [SetUp]
        public void Init()
        {
            _mockTxnMgr = new MockTxnPlatformMgrAbstract();
            _mockTxnMgr.TransactionSynchronization = TransactionSynchronizationState.Always;
            if (TransactionSynchronizationManager.SynchronizationActive)
            {
                TransactionSynchronizationManager.ClearSynchronization();
            }
        }
        [TearDown]
        public void Destroy()
        {
            _mockTxnMgr.Verify();
            _mockTxnMgr = null;
            if (TransactionSynchronizationManager.SynchronizationActive)
            {
                TransactionSynchronizationManager.Clear();
            }
        }
        [Test]
        public void VanillaProperties()
        {
            Assert.AreEqual(TransactionSynchronizationState.Always, _mockTxnMgr.TransactionSynchronization);
            Assert.IsTrue(!_mockTxnMgr.NestedTransactionsAllowed);
            Assert.IsTrue(!_mockTxnMgr.RollbackOnCommitFailure);

            _mockTxnMgr.NestedTransactionsAllowed = true;
            _mockTxnMgr.RollbackOnCommitFailure = true;
            _mockTxnMgr.TransactionSynchronization = TransactionSynchronizationState.OnActualTransaction;

            Assert.AreEqual(TransactionSynchronizationState.OnActualTransaction, _mockTxnMgr.TransactionSynchronization);
            Assert.IsTrue(_mockTxnMgr.NestedTransactionsAllowed);
            Assert.IsTrue(_mockTxnMgr.RollbackOnCommitFailure);
        }
        [Test]
        [ExpectedException(typeof(InvalidTimeoutException), ExpectedMessage = "Invalid transaction timeout")]
        public void DefinitionInvalidTimeoutException()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.TransactionTimeout = -1000;
            _mockTxnMgr.GetTransaction(def);
        }
        [Test]
        [ExpectedException(typeof(IllegalTransactionStateException), ExpectedMessage = "Transaction propagation 'mandatory' but no existing transaction found")]
        public void DefinitionInvalidPropagationState()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Mandatory;
            _mockTxnMgr.GetTransaction(def);
        }

        [Test]
        [ExpectedException(typeof(IllegalTransactionStateException), ExpectedMessage = "Transaction propagation 'never' but existing transaction found.")]
        public void NeverPropagateState()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Never;
            setGeneralGetTransactionExpectations();
            _mockTxnMgr.GetTransaction(def);
        }
        [Test]
        [ExpectedException(typeof(NestedTransactionNotSupportedException), ExpectedMessage = "Transaction manager does not allow nested transactions by default - specify 'NestedTransactionsAllowed' property with value 'true'")]
        public void NoNestedTransactionsAllowed()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Nested;
            setGeneralGetTransactionExpectations();
            _mockTxnMgr.GetTransaction(def);
        }
        [Test]
        public void TransactionSuspendedSuccessfully()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.NotSupported;
            def.ReadOnly = false;
            setGeneralGetTransactionExpectations();

            DefaultTransactionStatus status = (DefaultTransactionStatus)_mockTxnMgr.GetTransaction(def);
            Assert.IsNull(status.Transaction);
            Assert.IsTrue(!status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNotNull(status.SuspendedResources);
        }
        [Test]
        public void TransactionCreatedSuccessfully()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.RequiresNew;
            def.ReadOnly = false;

            setGeneralGetTransactionExpectations();

            DefaultTransactionStatus status = (DefaultTransactionStatus)_mockTxnMgr.GetTransaction(def);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsTrue(status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNotNull(status.SuspendedResources);
        }
        [Test]
        public void NestedTransactionSuccessfully()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Nested;
            def.ReadOnly = false;

            setGeneralGetTransactionExpectations();
            _mockTxnMgr.SetExpectedCalls("DoBegin", 1);
            _mockTxnMgr.Savepoints = false;
            _mockTxnMgr.NestedTransactionsAllowed = true;

            DefaultTransactionStatus status = (DefaultTransactionStatus)_mockTxnMgr.GetTransaction(def);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsTrue(status.IsNewTransaction);
            Assert.AreEqual(true, status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
        }

        [Test]
        public void NestedTransactionWithSavepoint()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Nested;
            def.ReadOnly = false;
            setVanillaGetTransactionExpectations();
            _mockTxnMgr.SetTransaction(new MyMockTxnObjectSavepointMgr());
            _mockTxnMgr.SetExpectedCalls("DoBegin", 0);
            _mockTxnMgr.Savepoints = true;
            _mockTxnMgr.NestedTransactionsAllowed = true;

            DefaultTransactionStatus status = (DefaultTransactionStatus)_mockTxnMgr.GetTransaction(def);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsFalse(status.IsNewTransaction);
            Assert.IsFalse(status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
        }
        [Test]
        public void DefaultPropagationBehavior()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Required;
            def.ReadOnly = true;
            setGeneralGetTransactionExpectations();

            DefaultTransactionStatus status = (DefaultTransactionStatus)_mockTxnMgr.GetTransaction(def);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsTrue(!status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
        }
        [Test]
        public void DefaultPropagationBehaviorWithNullDefinition()
        {
            setGeneralGetTransactionExpectations();

            DefaultTransactionStatus status = (DefaultTransactionStatus)_mockTxnMgr.GetTransaction(null);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsFalse(status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsFalse(status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
        }
        [Test]
        public void DefaultNoExistingTransaction()
        {
            setVanillaGetTransactionExpectations();
            _mockTxnMgr.SetExpectedCalls("DoBegin", 1);

            DefaultTransactionStatus status = (DefaultTransactionStatus)_mockTxnMgr.GetTransaction(null);
            Assert.IsNotNull(status.Transaction);
            Assert.IsTrue(status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
        }
        [Test]
        public void DefaultBehaviorDefaultPropagationNoExistingTransaction()
        {
            setVanillaGetTransactionExpectations();
            _mockTxnMgr.SetExpectedCalls("DoBegin", 0);

            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Never;
            def.ReadOnly = true;

            DefaultTransactionStatus status = (DefaultTransactionStatus)_mockTxnMgr.GetTransaction(def);
            Assert.IsNull(status.Transaction);
            Assert.IsTrue(!status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
        }
        private void setGeneralGetTransactionExpectations()
        {
            _mockTxnMgr.SetTransaction(new object());
            setVanillaGetTransactionExpectations();
        }

        private void setVanillaGetTransactionExpectations()
        {
            _mockTxnMgr.SetExpectedCalls("DoGetTransaction", 1);
            _mockTxnMgr.SetExpectedCalls("IsExistingTransaction", 1);
        }

    }
}
