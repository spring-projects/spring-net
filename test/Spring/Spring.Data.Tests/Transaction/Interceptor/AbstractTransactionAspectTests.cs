#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System;
using System.Reflection;

using FakeItEasy;

using NUnit.Framework;

using Spring.Data;
using Spring.Objects;

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// Mock object based tests for transaction aspects.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public abstract class AbstractTransactionAspectTests
    {
        [Test]
        public void CopyAttributes()
        {
            IPlatformTransactionManager ptm = A.Fake<IPlatformTransactionManager>();
            AttributesTransactionAttributeSource tas = new AttributesTransactionAttributeSource();
            TestObjectMgr to = new TestObjectMgr();
            ITestObjectMgr ito = (ITestObjectMgr) Advised(to, ptm, tas);

            ito.DeleteTwoTestObjects("foo", "bar");
        }

        [Test]
        public void CannotCommitTransaction()
        {
            ITransactionAttribute txatt = new DefaultTransactionAttribute();
            MethodInfo m = typeof(ITestObject).GetMethod("GetDescription");
            MethodMapTransactionAttributeSource tas = new MethodMapTransactionAttributeSource();
            tas.AddTransactionalMethod(m, txatt);

            IPlatformTransactionManager ptm = A.Fake<IPlatformTransactionManager>();
            ITransactionStatus status = A.Fake<ITransactionStatus>();

            A.CallTo(() => ptm.GetTransaction(txatt)).Returns(status);
            UnexpectedRollbackException ex = new UnexpectedRollbackException("foobar", null);
            A.CallTo(() => ptm.Commit(status)).Throws(ex);

            TestObject to = new TestObject();
            ITestObject ito = (ITestObject) Advised(to, ptm, tas);

            try
            {
                ito.GetDescription();
                Assert.Fail("Shouldn't have succeeded");
            }
            catch (UnexpectedRollbackException thrown)
            {
                Assert.IsTrue(thrown == ex);
            }
        }


        [Test]
        public void ProgrammaticRollback()
        {
            ITransactionAttribute txatt = new DefaultTransactionAttribute();
            MethodInfo m = typeof(RollbackTestObject).GetMethod("GetDescription");

            MethodMapTransactionAttributeSource tas = new MethodMapTransactionAttributeSource();
            tas.AddTransactionalMethod(m, txatt);

            ITransactionStatus status = A.Fake<ITransactionStatus>();

            IPlatformTransactionManager ptm = A.Fake<IPlatformTransactionManager>();

            A.CallTo(() => ptm.GetTransaction(txatt)).Returns(status).Once();

            RollbackTestObject to = new RollbackTestObject();

            ITestObject ito = (ITestObject) Advised(to, ptm, tas);

            Assert.AreEqual("test description", ito.GetDescription());

            A.CallTo(() => ptm.Commit(status)).MustHaveHappenedOnceExactly();
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


            MethodInfo mi = typeof(ITestObject).GetMethod("Exceptional");

            MethodMapTransactionAttributeSource tas = new MethodMapTransactionAttributeSource();
            tas.AddTransactionalMethod(mi, txatt);
            ITransactionStatus status = A.Fake<ITransactionStatus>();

            IPlatformTransactionManager ptm = A.Fake<IPlatformTransactionManager>();
            A.CallTo(() => ptm.GetTransaction(txatt)).Returns(status);


            TransactionSystemException tex = new TransactionSystemException("system exception");
            if (rollbackException)
            {
                A.CallTo(() => ptm.Rollback(A<ITransactionStatus>._)).Throws(tex);
            }

            TestObject to = new TestObject();
            ITestObject ito = (ITestObject) Advised(to, ptm, tas);

            try
            {
                ito.Exceptional(exception);
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e)
            {
                if (rollbackException && shouldRollback)
                {
                    Assert.AreEqual(tex, e);
                }
                else
                {
                    Assert.AreEqual(exception, e);
                }
            }

            if (shouldRollback)
            {
                A.CallTo(() => ptm.Rollback(status)).MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => ptm.Commit(status)).MustHaveHappenedOnceExactly();
            }
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