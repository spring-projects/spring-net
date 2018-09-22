using FakeItEasy;

using NUnit.Framework;

namespace Spring.Transaction.Support
{
    [TestFixture]
    public class DefaultTransactionStatusTests
    {
        [Test]
        public void DefaultConstructorTests()
        {
            ISmartTransactionObject txn = A.Fake<ISmartTransactionObject>();

            DefaultTransactionStatus stat = new DefaultTransactionStatus(txn, true, false, false, true, new object());
            Assert.IsNotNull(stat.Transaction);
            Assert.IsTrue(!stat.ReadOnly);
            Assert.IsTrue(!stat.NewSynchronization);
            Assert.IsNotNull(stat.SuspendedResources);
            Assert.IsTrue(stat.IsNewTransaction);
            Assert.IsTrue(! stat.RollbackOnly);
            stat.SetRollbackOnly();
            Assert.IsTrue(stat.RollbackOnly);
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
            Assert.IsTrue(status.HasSavepoint);
            Assert.AreEqual("savepoint", status.Savepoint);
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
            Assert.IsTrue(status.HasSavepoint);
            Assert.AreEqual(savepoint, status.Savepoint);

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
            Assert.IsTrue(status.HasSavepoint);
            Assert.AreEqual(savepoint, status.Savepoint);

            status.ReleaseHeldSavepoint();
            A.CallTo(() => saveMgr.CreateSavepoint(savepoint)).MustHaveHappened();
            A.CallTo(() => saveMgr.ReleaseSavepoint(savepoint)).MustHaveHappened();
        }
    }
}