using FakeItEasy;
using NUnit.Framework;

namespace Spring.Transaction.Support;

[TestFixture]
public class DefaultTransactionStatusTests
{
    [Test]
    public void DefaultConstructorTests()
    {
        ISmartTransactionObject txn = A.Fake<ISmartTransactionObject>();

        DefaultTransactionStatus stat = new DefaultTransactionStatus(txn, true, false, false, true, new object());
        Assert.That(stat.Transaction, Is.Not.Null);
        Assert.That(!stat.ReadOnly, Is.True);
        Assert.That(!stat.NewSynchronization, Is.True);
        Assert.That(stat.SuspendedResources, Is.Not.Null);
        Assert.That(stat.IsNewTransaction, Is.True);
        Assert.That(!stat.RollbackOnly, Is.True);
        stat.SetRollbackOnly();
        Assert.That(stat.RollbackOnly, Is.True);
    }

    [Test]
    public void CreateSavepointException()
    {
        ISmartTransactionObject transaction = A.Fake<ISmartTransactionObject>();
        DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
        Assert.Throws<NestedTransactionNotSupportedException>(() => stat.CreateSavepoint("mySavePoint"));
    }

    [Test]
    public void RollbackSavepointException()
    {
        ISmartTransactionObject transaction = A.Fake<ISmartTransactionObject>();
        DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
        Assert.Throws<NestedTransactionNotSupportedException>(() => stat.RollbackToSavepoint(null));
    }

    [Test]
    public void ReleaseSavepointException()
    {
        ISmartTransactionObject transaction = A.Fake<ISmartTransactionObject>();
        DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
        Assert.Throws<NestedTransactionNotSupportedException>(() => stat.ReleaseSavepoint(null));
    }

    [Test]
    public void CreateSaveAndHoldValidSavepoint()
    {
        ISavepointManager saveMgr = A.Fake<ISavepointManager>();
        DefaultTransactionStatus status = new DefaultTransactionStatus(saveMgr, true, false, false, true, new object());
        status.CreateAndHoldSavepoint("savepoint");
        Assert.That(status.HasSavepoint, Is.True);
        Assert.That(status.Savepoint, Is.EqualTo("savepoint"));
    }

    [Test]
    public void RollbackHeldSavepointException()
    {
        ISmartTransactionObject transaction = A.Fake<ISmartTransactionObject>();
        DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
        Assert.Throws<TransactionUsageException>(() => stat.RollbackToHeldSavepoint());
    }

    [Test]
    public void RollbackHeldSavepointSuccess()
    {
        ISavepointManager saveMgr = A.Fake<ISavepointManager>();
        string savepoint = "savepoint";
        DefaultTransactionStatus status = new DefaultTransactionStatus(saveMgr, true, false, false, true, new object());
        status.CreateAndHoldSavepoint(savepoint);
        Assert.That(status.HasSavepoint, Is.True);
        Assert.That(status.Savepoint, Is.EqualTo(savepoint));

        status.RollbackToHeldSavepoint();
        A.CallTo(() => saveMgr.RollbackToSavepoint(savepoint)).MustHaveHappened();
    }

    [Test]
    public void ReleaseHeldSavepointException()
    {
        ISmartTransactionObject transaction = A.Fake<ISmartTransactionObject>();
        DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
        Assert.Throws<TransactionUsageException>(() => stat.ReleaseHeldSavepoint());
    }

    [Test]
    public void ReleaseHeldSavepointSuccess()
    {
        ISavepointManager saveMgr = A.Fake<ISavepointManager>();
        string savepoint = "savepoint";
        DefaultTransactionStatus status = new DefaultTransactionStatus(saveMgr, true, false, false, true, new object());
        status.CreateAndHoldSavepoint(savepoint);
        Assert.That(status.HasSavepoint, Is.True);
        Assert.That(status.Savepoint, Is.EqualTo(savepoint));

        status.ReleaseHeldSavepoint();
        A.CallTo(() => saveMgr.CreateSavepoint(savepoint)).MustHaveHappened();
        A.CallTo(() => saveMgr.ReleaseSavepoint(savepoint)).MustHaveHappened();
    }
}
