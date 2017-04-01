#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using FakeItEasy;

using NUnit.Framework;

using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.Core
{
    /// <summary>
    /// This class contains tests for TxScopeTransactionManager and will directly a real TransactionScope object
    /// but does not access any database
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class TxScopeTransactionManagerIntegrationTests
    {
        [TearDown]
        public void TearDown()
        {
            Assert.IsTrue(TransactionSynchronizationManager.ResourceDictionary.Count == 0);
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsNull(TransactionSynchronizationManager.CurrentTransactionName);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.AreEqual(System.Data.IsolationLevel.Unspecified, TransactionSynchronizationManager.CurrentTransactionIsolationLevel);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);
        }

        [Test]
        public void Commit()
        {
            TxScopeTransactionManager tm = new TxScopeTransactionManager();
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);

            //tt.Name = "txName";

            Assert.AreEqual(TransactionSynchronizationState.Always, tm.TransactionSynchronization);
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsNull(TransactionSynchronizationManager.CurrentTransactionName);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            tt.Execute(CommitTxDelegate);
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsNull(TransactionSynchronizationManager.CurrentTransactionName);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
        }

        public object CommitTxDelegate(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);

            return null;
        }

        [Test]
        public void TransactionInformation()
        {
            TxScopeTransactionManager tm = new TxScopeTransactionManager();
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.TransactionIsolationLevel = System.Data.IsolationLevel.ReadUncommitted;
            tt.Execute(TransactionInformationTxDelegate);
        }

        public object TransactionInformationTxDelegate(ITransactionStatus status)
        {
            Assert.AreEqual(System.Transactions.IsolationLevel.ReadUncommitted,
                System.Transactions.Transaction.Current.IsolationLevel);

            Assert.AreEqual(System.Data.IsolationLevel.ReadUncommitted,
                TransactionSynchronizationManager.CurrentTransactionIsolationLevel);
            return null;
        }


        [Test]
        public void Rollback()
        {
            ITransactionSynchronization sync = A.Fake<ITransactionSynchronization>();

            TxScopeTransactionManager tm = new TxScopeTransactionManager();
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.TransactionTimeout = 10;
            tt.Name = "txName";

            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsNull(TransactionSynchronizationManager.CurrentTransactionName);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            tt.Execute(status =>
                {
                    Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
                    TransactionSynchronizationManager.RegisterSynchronization(sync);
                    Assert.AreEqual("txName", TransactionSynchronizationManager.CurrentTransactionName);
                    Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                    status.SetRollbackOnly();
                    return null;
                }
            );

            A.CallTo(() => sync.BeforeCompletion()).MustHaveHappenedOnceExactly();
            A.CallTo(() => sync.AfterCompletion(TransactionSynchronizationStatus.Rolledback)).MustHaveHappenedOnceExactly();
        }
    }
}