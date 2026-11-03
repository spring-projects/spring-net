using FakeItEasy;
using NUnit.Framework;

namespace Spring.Transaction.Support;

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
        Assert.That(_mockTxnMgr.TransactionSynchronization, Is.EqualTo(TransactionSynchronizationState.Always));
        Assert.That(!_mockTxnMgr.NestedTransactionsAllowed, Is.True);
        Assert.That(!_mockTxnMgr.RollbackOnCommitFailure, Is.True);

        _mockTxnMgr.NestedTransactionsAllowed = true;
        _mockTxnMgr.RollbackOnCommitFailure = true;
        _mockTxnMgr.TransactionSynchronization = TransactionSynchronizationState.OnActualTransaction;

        Assert.That(_mockTxnMgr.TransactionSynchronization, Is.EqualTo(TransactionSynchronizationState.OnActualTransaction));
        Assert.That(_mockTxnMgr.NestedTransactionsAllowed, Is.True);
        Assert.That(_mockTxnMgr.RollbackOnCommitFailure, Is.True);
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
        Assert.That(status.Transaction, Is.Null);
        Assert.That(!status.IsNewTransaction, Is.True);
        Assert.That(status.NewSynchronization, Is.True);
        Assert.That(!status.ReadOnly, Is.True);
        Assert.That(status.SuspendedResources, Is.Not.Null);
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
        Assert.That(status.Transaction, Is.EqualTo(_mockTxnMgr.Transaction));
        Assert.That(status.IsNewTransaction, Is.True);
        Assert.That(status.NewSynchronization, Is.True);
        Assert.That(!status.ReadOnly, Is.True);
        Assert.That(status.SuspendedResources, Is.Not.Null);
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
        Assert.That(status.Transaction, Is.EqualTo(_mockTxnMgr.Transaction));
        Assert.That(status.IsNewTransaction, Is.True);
        Assert.That(status.NewSynchronization, Is.EqualTo(true));
        Assert.That(!status.ReadOnly, Is.True);
        Assert.That(status.SuspendedResources, Is.Null);

        AssertVanillaGetTransactionExpectations();
        Assert.That(_mockTxnMgr.DoBeginCallCount, Is.EqualTo(1));
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
        Assert.That(status.Transaction, Is.EqualTo(_mockTxnMgr.Transaction));
        Assert.That(status.IsNewTransaction, Is.False);
        Assert.That(status.NewSynchronization, Is.False);
        Assert.That(!status.ReadOnly, Is.True);
        Assert.That(status.SuspendedResources, Is.Null);

        AssertVanillaGetTransactionExpectations();
        Assert.That(_mockTxnMgr.DoBeginCallCount, Is.EqualTo(0));
    }

    [Test]
    public void DefaultPropagationBehavior()
    {
        MockTxnDefinition def = new MockTxnDefinition();
        def.PropagationBehavior = TransactionPropagation.Required;
        def.ReadOnly = true;
        SetGeneralGetTransactionExpectations();

        DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(def);
        Assert.That(status.Transaction, Is.EqualTo(_mockTxnMgr.Transaction));
        Assert.That(!status.IsNewTransaction, Is.True);
        Assert.That(status.NewSynchronization, Is.True);
        Assert.That(status.ReadOnly, Is.True);
        Assert.That(status.SuspendedResources, Is.Null);
        AssertVanillaGetTransactionExpectations();
    }

    [Test]
    public void DefaultPropagationBehaviorWithNullDefinition()
    {
        SetGeneralGetTransactionExpectations();

        DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(null);
        Assert.That(status.Transaction, Is.EqualTo(_mockTxnMgr.Transaction));
        Assert.That(status.IsNewTransaction, Is.False);
        Assert.That(status.NewSynchronization, Is.True);
        Assert.That(status.ReadOnly, Is.False);
        Assert.That(status.SuspendedResources, Is.Null);
        AssertVanillaGetTransactionExpectations();
    }

    [Test]
    public void DefaultNoExistingTransaction()
    {
        DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(null);
        Assert.That(status.Transaction, Is.Not.Null);
        Assert.That(status.IsNewTransaction, Is.True);
        Assert.That(status.NewSynchronization, Is.True);
        Assert.That(!status.ReadOnly, Is.True);
        Assert.That(status.SuspendedResources, Is.Null);
        Assert.That(_mockTxnMgr.DoBeginCallCount, Is.EqualTo(1));

        AssertVanillaGetTransactionExpectations();
    }

    [Test]
    public void DefaultBehaviorDefaultPropagationNoExistingTransaction()
    {
        MockTxnDefinition def = new MockTxnDefinition();
        def.PropagationBehavior = TransactionPropagation.Never;
        def.ReadOnly = true;

        DefaultTransactionStatus status = (DefaultTransactionStatus) _mockTxnMgr.GetTransaction(def);
        Assert.That(status.Transaction, Is.Null);
        Assert.That(!status.IsNewTransaction, Is.True);
        Assert.That(status.NewSynchronization, Is.True);
        Assert.That(status.ReadOnly, Is.True);
        Assert.That(status.SuspendedResources, Is.Null);
        Assert.That(_mockTxnMgr.DoBeginCallCount, Is.EqualTo(0));

        AssertVanillaGetTransactionExpectations();
    }

    private void SetGeneralGetTransactionExpectations()
    {
        _mockTxnMgr.SetTransaction(new object());
    }

    private void AssertVanillaGetTransactionExpectations()
    {
        Assert.That(_mockTxnMgr.DoGetTransactionCallCount, Is.EqualTo(1));
        Assert.That(_mockTxnMgr.IsExistingTransactionCallCount, Is.EqualTo(1));
    }
}
