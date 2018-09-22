using System;

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Transaction.Support
{
    [TestFixture]
    public class TransactionTemplateTests
    {
        [Test]
        public void NoTxnMgr()
        {
            TransactionTemplate temp = new TransactionTemplate();
            Assert.Throws<ArgumentException>(() => temp.AfterPropertiesSet());
        }

        [Test]
        public void TxnMgr()
        {
            TransactionTemplate temp = new TransactionTemplate();
            temp.PlatformTransactionManager = A.Fake<IPlatformTransactionManager>();
            temp.AfterPropertiesSet();
        }

        [Test]
        public void ExecuteException()
        {
            IPlatformTransactionManager mock = A.Fake<IPlatformTransactionManager>();

            TransactionTemplate temp = new TransactionTemplate(mock);
            try
            {
                temp.Execute(DummyExceptionMethod);
            }
            catch
            {
            }

            A.CallTo(() => mock.GetTransaction(A<ITransactionDefinition>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => mock.Rollback(A<ITransactionStatus>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void ExecuteExceptionRollbackException()
        {
            IPlatformTransactionManager mock = A.Fake<IPlatformTransactionManager>();
            A.CallTo(() => mock.Rollback(A<ITransactionStatus>._)).Throws(new Exception("Rollback"));

            TransactionTemplate temp = new TransactionTemplate(mock);
            try
            {
                temp.Execute(DummyExceptionMethod);
            }
            catch
            {
            }

            A.CallTo(() => mock.GetTransaction(A<ITransactionDefinition>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => mock.Rollback(A<ITransactionStatus>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NullResult()
        {
            IPlatformTransactionManager mock = A.Fake<IPlatformTransactionManager>();
            A.CallTo(() => mock.GetTransaction(A<ITransactionDefinition>._)).Returns(null);

            TransactionTemplate temp = new TransactionTemplate(mock);
            temp.AfterPropertiesSet();
            Assert.AreEqual(mock, temp.PlatformTransactionManager);
            Assert.IsNull(temp.Execute(DummyTransactionMethod));

            A.CallTo(() => mock.GetTransaction(A<ITransactionDefinition>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => mock.Commit(A<ITransactionStatus>._)).MustHaveHappenedOnceExactly();
        }

        public object DummyTransactionMethod(ITransactionStatus status)
        {
            return status;
        }

        public object DummyExceptionMethod(ITransactionStatus status)
        {
            throw new Exception("Bad Error");
        }
    }
}