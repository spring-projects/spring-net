using System;
using NUnit.Framework;
using Rhino.Mocks;

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
            temp.PlatformTransactionManager = MockRepository.GenerateMock<IPlatformTransactionManager>();
            temp.AfterPropertiesSet();
        }

        [Test]
        public void ExecuteException()
        {
            IPlatformTransactionManager mock = MockRepository.GenerateMock<IPlatformTransactionManager>();

            TransactionTemplate temp = new TransactionTemplate(mock);
            try
            {
                temp.Execute(new TransactionDelegate(DummyExceptionMethod));
            }
            catch
            {
            }

            mock.AssertWasCalled(x => x.GetTransaction(Arg<ITransactionDefinition>.Is.Anything), constraints => constraints.Repeat.Once());
            mock.AssertWasCalled(x => x.Rollback(Arg<ITransactionStatus>.Is.Anything), constraints => constraints.Repeat.Once());
        }

        [Test]
        public void ExecuteExceptionRollbackException()
        {
            IPlatformTransactionManager mock = MockRepository.GenerateMock<IPlatformTransactionManager>();
            mock.Stub(x => x.Rollback(Arg<ITransactionStatus>.Is.Anything)).Throw(new Exception("Rollback"));

            TransactionTemplate temp = new TransactionTemplate(mock);
            try
            {
                temp.Execute(new TransactionDelegate(DummyExceptionMethod));
            }
            catch
            {
            }

            mock.AssertWasCalled(x => x.GetTransaction(Arg<ITransactionDefinition>.Is.Anything), constraints => constraints.Repeat.Once());
            mock.AssertWasCalled(x => x.Rollback(Arg<ITransactionStatus>.Is.Anything), constraints => constraints.Repeat.Once());
        }

        [Test]
        public void NullResult()
        {
            IPlatformTransactionManager mock = MockRepository.GenerateMock<IPlatformTransactionManager>();

            TransactionTemplate temp = new TransactionTemplate(mock);
            temp.AfterPropertiesSet();
            Assert.AreEqual(mock, temp.PlatformTransactionManager);
            Assert.IsNull(temp.Execute(new TransactionDelegate(DummyTransactionMethod)));

            mock.AssertWasCalled(x => x.GetTransaction(Arg<ITransactionDefinition>.Is.Anything), constraints => constraints.Repeat.Once());
            mock.AssertWasCalled(x => x.Commit(Arg<ITransactionStatus>.Is.Anything), constraints => constraints.Repeat.Once());
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