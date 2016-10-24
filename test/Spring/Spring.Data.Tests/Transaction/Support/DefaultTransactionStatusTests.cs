using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Transaction.Support
{
    [TestFixture]
    public class DefaultTransactionStatusTests
    {
        [Test]
        public void DefaultConstructorTests()
        {
            ISmartTransactionObject txn = MockRepository.GenerateMock<ISmartTransactionObject>();

            DefaultTransactionStatus stat = new DefaultTransactionStatus(txn, true, false, false, true, new object());
            Assert.IsNotNull(stat.Transaction);
            Assert.IsTrue(!stat.ReadOnly);
            Assert.IsTrue(!stat.NewSynchronization);
            Assert.IsNotNull(stat.SuspendedResources);
            Assert.IsTrue(stat.IsNewTransaction);
            Assert.IsTrue(! stat.RollbackOnly);
            stat.SetRollbackOnly();
            Assert.IsTrue(stat.RollbackOnly);

            txn.AssertWasCalled(x => x.RollbackOnly, constraints => constraints.Repeat.Once());
        }

        [Test]
        public void CreateSavepointException()
        {
            ISmartTransactionObject transaction = MockRepository.GenerateMock<ISmartTransactionObject>();
            DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
            Assert.Throws<NestedTransactionNotSupportedException>(() => stat.CreateSavepoint("mySavePoint"));
        }

        [Test]
        public void RollbackSavepointException()
        {
            ISmartTransactionObject transaction = MockRepository.GenerateMock<ISmartTransactionObject>();
            DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
            Assert.Throws<NestedTransactionNotSupportedException>(() => stat.RollbackToSavepoint(null));
        }

        [Test]
        public void ReleaseSavepointException()
        {
            ISmartTransactionObject transaction = MockRepository.GenerateMock<ISmartTransactionObject>();
            DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
            Assert.Throws<NestedTransactionNotSupportedException>(() => stat.ReleaseSavepoint(null));
        }

        [Test]
        public void CreateSaveAndHoldValidSavepoint()
        {
            ISavepointManager saveMgr = MockRepository.GenerateMock<ISavepointManager>();
            DefaultTransactionStatus status = new DefaultTransactionStatus(saveMgr, true, false, false, true, new object());
            status.CreateAndHoldSavepoint("savepoint");
            Assert.IsTrue(status.HasSavepoint);
            Assert.AreEqual("savepoint", status.Savepoint);
        }

        [Test]
        public void RollbackHeldSavepointException()
        {
            ISmartTransactionObject transaction = MockRepository.GenerateMock<ISmartTransactionObject>();
            DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
            Assert.Throws<TransactionUsageException>(() => stat.RollbackToHeldSavepoint());
        }

        [Test]
        public void RollbackHeldSavepointSuccess()
        {
            ISavepointManager saveMgr = MockRepository.GenerateMock<ISavepointManager>();
            string savepoint = "savepoint";
            DefaultTransactionStatus status = new DefaultTransactionStatus(saveMgr, true, false, false, true, new object());
            status.CreateAndHoldSavepoint(savepoint);
            Assert.IsTrue(status.HasSavepoint);
            Assert.AreEqual(savepoint, status.Savepoint);

            status.RollbackToHeldSavepoint();
            saveMgr.AssertWasCalled(x => x.RollbackToSavepoint(savepoint));
        }

        [Test]
        public void ReleaseHeldSavepointException()
        {
            ISmartTransactionObject transaction = MockRepository.GenerateMock<ISmartTransactionObject>();
            DefaultTransactionStatus stat = new DefaultTransactionStatus(transaction, true, false, false, true, new object());
            Assert.Throws<TransactionUsageException>(() => stat.ReleaseHeldSavepoint());
        }

        [Test]
        public void ReleaseHeldSavepointSuccess()
        {
            ISavepointManager saveMgr = MockRepository.GenerateMock<ISavepointManager>();
            string savepoint = "savepoint";
            DefaultTransactionStatus status = new DefaultTransactionStatus(saveMgr, true, false, false, true, new object());
            status.CreateAndHoldSavepoint(savepoint);
            Assert.IsTrue(status.HasSavepoint);
            Assert.AreEqual(savepoint, status.Savepoint);

            status.ReleaseHeldSavepoint();
            saveMgr.AssertWasCalled(x => x.CreateSavepoint(savepoint));
            saveMgr.AssertWasCalled(x => x.ReleaseSavepoint(savepoint));
        }
    }
}