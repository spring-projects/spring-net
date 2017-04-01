using FakeItEasy;

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
        public void DefinitionInvalidTimeoutException()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.TransactionTimeout = -1000;
            Assert.Throws<InvalidTimeoutException>(() => _mockTxnMgr.GetTransaction(def), "Invalid transaction timeout");
        }

        [Test]
        public void DefinitionInvalidPropagationState()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Mandatory;
            Assert.Throws<IllegalTransactionStateException>(() => _mockTxnMgr.GetTransaction(def), "Transaction propagation 'mandatory' but no existing transaction found");
        }

        [Test]
        public void NeverPropagateState()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Never;
            SetGeneralGetTransactionExpectations();

            Assert.Throws<IllegalTransactionStateException>(() => _mockTxnMgr.GetTransaction(def), "Transaction propagation 'never' but existing transaction found.");
        }

        [Test]
        public void NoNestedTransactionsAllowed()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Nested;
            SetGeneralGetTransactionExpectations();

            Assert.Throws<NestedTransactionNotSupportedException>(() => _mockTxnMgr.GetTransaction(def), "Transaction manager does not allow nested transactions by default - specify 'NestedTransactionsAllowed' property with value 'true'");
        }

        [Test]
        public void TransactionSuspendedSuccessfully()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.NotSupported;
            def.ReadOnly = false;
            SetGeneralGetTransactionExpectations();

            DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(def);
            Assert.IsNull(status.Transaction);
            Assert.IsTrue(!status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNotNull(status.SuspendedResources);
            AssertVanillaGetTransactionExpectations();
        }

        [Test]
        public void TransactionCreatedSuccessfully()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.RequiresNew;
            def.ReadOnly = false;

            SetGeneralGetTransactionExpectations();

            DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(def);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsTrue(status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNotNull(status.SuspendedResources);
            AssertVanillaGetTransactionExpectations();
        }

        [Test]
        public void NestedTransactionSuccessfully()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Nested;
            def.ReadOnly = false;

            SetGeneralGetTransactionExpectations();
            _mockTxnMgr.Savepoints = false;
            _mockTxnMgr.NestedTransactionsAllowed = true;

            DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(def);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsTrue(status.IsNewTransaction);
            Assert.AreEqual(true, status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);

            AssertVanillaGetTransactionExpectations();
            Assert.AreEqual(1, _mockTxnMgr.DoBeginCallCount);
        }

        [Test]
        public void NestedTransactionWithSavepoint()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Nested;
            def.ReadOnly = false;
            ISavepointManager saveMgr = A.Fake<ISavepointManager>();
            _mockTxnMgr.SetTransaction(saveMgr);
            _mockTxnMgr.Savepoints = true;
            _mockTxnMgr.NestedTransactionsAllowed = true;

            DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(def);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsFalse(status.IsNewTransaction);
            Assert.IsFalse(status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);

            AssertVanillaGetTransactionExpectations();
            Assert.AreEqual(0, _mockTxnMgr.DoBeginCallCount);
        }

        [Test]
        public void DefaultPropagationBehavior()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Required;
            def.ReadOnly = true;
            SetGeneralGetTransactionExpectations();

            DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(def);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsTrue(!status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
            AssertVanillaGetTransactionExpectations();
        }

        [Test]
        public void DefaultPropagationBehaviorWithNullDefinition()
        {
            SetGeneralGetTransactionExpectations();

            DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(null);
            Assert.AreEqual(_mockTxnMgr.Transaction, status.Transaction);
            Assert.IsFalse(status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsFalse(status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
            AssertVanillaGetTransactionExpectations();
        }

        [Test]
        public void DefaultNoExistingTransaction()
        {
            DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(null);
            Assert.IsNotNull(status.Transaction);
            Assert.IsTrue(status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(!status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
            Assert.AreEqual(1, _mockTxnMgr.DoBeginCallCount);

            AssertVanillaGetTransactionExpectations();
        }

        [Test]
        public void DefaultBehaviorDefaultPropagationNoExistingTransaction()
        {
            MockTxnDefinition def = new MockTxnDefinition();
            def.PropagationBehavior = TransactionPropagation.Never;
            def.ReadOnly = true;

            DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(def);
            Assert.IsNull(status.Transaction);
            Assert.IsTrue(!status.IsNewTransaction);
            Assert.IsTrue(status.NewSynchronization);
            Assert.IsTrue(status.ReadOnly);
            Assert.IsNull(status.SuspendedResources);
            Assert.AreEqual(0, _mockTxnMgr.DoBeginCallCount);

            AssertVanillaGetTransactionExpectations();
        }

        private void SetGeneralGetTransactionExpectations()
        {
            _mockTxnMgr.SetTransaction(new object());
        }

        private void AssertVanillaGetTransactionExpectations()
        {
            Assert.AreEqual(1, _mockTxnMgr.DoGetTransactionCallCount);
            Assert.AreEqual(1, _mockTxnMgr.IsExistingTransactionCallCount);
        }
    }
}