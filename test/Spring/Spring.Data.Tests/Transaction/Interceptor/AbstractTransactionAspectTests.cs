#region License

/*
 * Copyright © 2002-2007 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using System;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Data;
using Spring.Objects;

#endregion

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// Mock object based tests for transaction aspects.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public abstract class AbstractTransactionAspectTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        public void CopyAttributes()
        {

            IPlatformTransactionManager ptm = PlatformTxManagerForNewTransaction();
            AttributesTransactionAttributeSource tas = new AttributesTransactionAttributeSource();
            TestObjectMgr to = new TestObjectMgr();
            ITestObjectMgr ito = (ITestObjectMgr)Advised(to, ptm, tas);

            ito.DeleteTwoTestObjects("foo", "bar");

        }

        [Test]
        public void CannotCommitTransaction()
        {
            ITransactionAttribute txatt = new DefaultTransactionAttribute();
            MethodInfo m = typeof (ITestObject).GetMethod("GetDescription");
            MethodMapTransactionAttributeSource tas = new MethodMapTransactionAttributeSource();
            tas.AddTransactionalMethod(m, txatt);


            IPlatformTransactionManager ptm = PlatformTxManagerForNewTransaction();

            ITransactionStatus status = TransactionStatusForNewTransaction();
            Expect.On(ptm).Call(ptm.GetTransaction(txatt)).Return(status);
            UnexpectedRollbackException ex = new UnexpectedRollbackException("foobar", null);
            ptm.Commit(status);
            LastCall.On(ptm).Throw(ex);
            mocks.ReplayAll();

            TestObject to = new TestObject();
            ITestObject ito = (ITestObject) Advised(to, ptm, tas);

            try
            {
                ito.GetDescription();
                Assert.Fail("Shouldn't have succeeded");
            } catch (UnexpectedRollbackException thrown)
            {
                Assert.IsTrue(thrown == ex);
            }

            mocks.VerifyAll();


            
        }

        private IPlatformTransactionManager PlatformTxManagerForNewTransaction()
        {
            return (IPlatformTransactionManager) mocks.DynamicMock(typeof(IPlatformTransactionManager));
        }


        [Test]
        public void ProgrammaticRollback()
        {
            ITransactionAttribute txatt = new DefaultTransactionAttribute();
            MethodInfo m = typeof(RollbackTestObject).GetMethod("GetDescription");

            MethodMapTransactionAttributeSource tas = new MethodMapTransactionAttributeSource();
            tas.AddTransactionalMethod(m, txatt);

            ITransactionStatus status = TransactionStatusForNewTransaction();

            IPlatformTransactionManager ptm = PlatformTxManagerForNewTransaction();

            Expect.Call(ptm.GetTransaction(txatt)).Return(status).Repeat.Once();
            ptm.Commit(status);
            LastCall.On(ptm).Repeat.Once();

            mocks.ReplayAll();
            
            RollbackTestObject to = new RollbackTestObject();

            ITestObject ito = (ITestObject) Advised(to, ptm, tas);

            Assert.AreEqual("test description", ito.GetDescription());

            mocks.VerifyAll();


        }

        [Test]
        public void RollbackOnException()
        {
            DoTestRollbackOnException(new Exception(), true, false);
        }

        [Test]
        public void NoRollbackOnException()
        {
            DoTestRollbackOnException(new Exception(), false, false);
        }

        [Test]
        public void RollbackOnExceptionWithRollbackException()
        {
            DoTestRollbackOnException(new Exception(), true, true);
        }

        [Test]
        public void NoRollbackOnExceptionWithRollbackException()
        {
            DoTestRollbackOnException(new Exception(), false, true);
        }

        private void DoTestRollbackOnException(Exception exception, bool shouldRollback, bool rollbackException)
        {
            ITransactionAttribute txatt = new ConfigurableTransactionAttribute(shouldRollback);


            MethodInfo mi = typeof (ITestObject).GetMethod("Exceptional");

            MethodMapTransactionAttributeSource tas = new MethodMapTransactionAttributeSource();
            tas.AddTransactionalMethod(mi, txatt);
            ITransactionStatus status = TransactionStatusForNewTransaction();

            IPlatformTransactionManager ptm =
                (IPlatformTransactionManager) mocks.DynamicMock(typeof (IPlatformTransactionManager));

            
            Expect.On(ptm).Call(ptm.GetTransaction(txatt)).Return(status);
            

            if (shouldRollback)
            {
                ptm.Rollback(status);
            }
            else
            {
                ptm.Commit(status);
            }
            TransactionSystemException tex = new TransactionSystemException("system exception");
            if (rollbackException)
            {
                LastCall.On(ptm).Throw(tex).Repeat.Once();
            }
            else
            {
                LastCall.On(ptm).Repeat.Once();
            }
            mocks.ReplayAll();

            TestObject to = new TestObject();
            ITestObject ito = (ITestObject) Advised(to, ptm, tas);

            try
            {
                ito.Exceptional(exception);
                Assert.Fail("Should have thrown exception");
            } catch (Exception e)
            {
                if (rollbackException)
                {
                    Assert.AreEqual(tex, e);
                }
                else
                {
                    Assert.AreEqual(exception, e);
                }
            }

            mocks.VerifyAll();

        }

        private ITransactionStatus TransactionStatusForNewTransaction()
        {
            return (ITransactionStatus) mocks.DynamicMock(typeof (ITransactionStatus));
        }

        protected abstract object Advised(object target, IPlatformTransactionManager ptm,
                                               ITransactionAttributeSource tas);
    }

    internal class RollbackTestObject : TestObject
    {
        public override string GetDescription()
        {
            ITransactionStatus txStatus = TransactionInterceptor.CurrentTransactionStatus;
            txStatus.SetRollbackOnly();
            return "test description";
        }

    }

    internal class ConfigurableTransactionAttribute : DefaultTransactionAttribute
    {
        private bool shouldRollback;
        public ConfigurableTransactionAttribute(bool shouldRollback)
        {
            this.shouldRollback = shouldRollback;
        }

        public override bool RollbackOn(Exception exception)
        {
            return shouldRollback;
        }
    }
}