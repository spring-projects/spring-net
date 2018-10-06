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
using System.Transactions;

using FakeItEasy;

using NUnit.Framework;

using Spring.Data.Support;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.Core
{
    /// <summary>
    /// This calss contains tests for
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class TxScopeTransactionManagerTests
    {

        [Test]
        public void TransactionCommit()
        {
            ITransactionScopeAdapter txAdapter = A.Fake<ITransactionScopeAdapter>();
            A.CallTo(() => txAdapter.IsExistingTransaction).Returns(false);

            A.CallTo(() => txAdapter.RollbackOnly).Returns(false);

            TxScopeTransactionManager tm = new TxScopeTransactionManager(txAdapter);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.Execute(status =>
            {
                Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
                Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                return null;
            });

            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);

            TransactionOptions txOptions = new TransactionOptions();
            txOptions.IsolationLevel = IsolationLevel.ReadCommitted;
            txAdapter.CreateTransactionScope(TransactionScopeOption.Required, txOptions, TransactionScopeAsyncFlowOption.Enabled);
            txAdapter.Complete();
            txAdapter.Dispose();
        }

        [Test]
        public void TransactionRollback()
        {
            ITransactionScopeAdapter txAdapter = A.Fake<ITransactionScopeAdapter>();

            A.CallTo(() => txAdapter.IsExistingTransaction).Returns(false);
            TransactionOptions txOptions = new TransactionOptions();
            txOptions.IsolationLevel = IsolationLevel.ReadCommitted;

            TxScopeTransactionManager tm = new TxScopeTransactionManager(txAdapter);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);

            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            Exception ex = new ArgumentException("test exception");
            try
            {
                tt.Execute(status =>
                {
                    Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
                    Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                    Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
                    if (ex != null) throw ex;
                    return null;
                });
                Assert.Fail("Should have thrown exception");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(ex, e);
            }

            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");
            A.CallTo(() => txAdapter.CreateTransactionScope(TransactionScopeOption.Required, txOptions, TransactionScopeAsyncFlowOption.Enabled)).MustHaveHappenedOnceExactly();
            A.CallTo(() => txAdapter.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void PropagationRequiresNewWithExistingTransaction()
        {
            ITransactionScopeAdapter txAdapter = A.Fake<ITransactionScopeAdapter>();
            A.CallTo(() => txAdapter.IsExistingTransaction).Returns(false).Once();

            TransactionOptions txOptions = new TransactionOptions();
            txOptions.IsolationLevel = IsolationLevel.ReadCommitted;

            //inner tx actions
            A.CallTo(() => txAdapter.IsExistingTransaction).Returns(true).Once();
            //end inner tx actions

            A.CallTo(() => txAdapter.RollbackOnly).Returns(false);

            TxScopeTransactionManager tm = new TxScopeTransactionManager(txAdapter);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.Execute(status =>
            {
                Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
                Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
                Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);

                tt.Execute(status2 =>
                {
                    Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
                    Assert.IsTrue(status2.IsNewTransaction, "Is new transaction");
                    Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                    Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
                    status2.SetRollbackOnly();
                    return null;
                });


                Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
                Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
                return null;
            });

            A.CallTo(() => txAdapter.CreateTransactionScope(TransactionScopeOption.RequiresNew, txOptions, TransactionScopeAsyncFlowOption.Enabled)).MustHaveHappenedTwiceExactly();
            A.CallTo(() => txAdapter.Dispose()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => txAdapter.Complete()).MustHaveHappenedOnceExactly();
        }
    }
}
