#if NET_2_0

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
using System.Transactions;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Data.Support;
using Spring.Transaction;
using Spring.Transaction.Support;

#endregion

namespace Spring.Data.Core
{
    /// <summary>
    /// This calss contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class TxScopeTransactionManagerTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]

        public void TransactionCommit()
        {
            ITransactionScopeAdapter txAdapter = (ITransactionScopeAdapter) mocks.CreateMock(typeof(ITransactionScopeAdapter));

            using (mocks.Ordered())
            {
                Expect.Call(txAdapter.IsExistingTransaction).Return(false);
                TransactionOptions txOptions = new TransactionOptions();
                txOptions.IsolationLevel = IsolationLevel.ReadCommitted;
                txAdapter.CreateTransactionScope(TransactionScopeOption.Required, txOptions, EnterpriseServicesInteropOption.None);

                Expect.Call(txAdapter.RollbackOnly).Return(false);
                txAdapter.Complete();
                txAdapter.Dispose();
            }
            mocks.ReplayAll();

            IPlatformTransactionManager tm = new TxScopeTransactionManager(txAdapter);
            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.Execute(delegate(ITransactionStatus status)
                           {
                               Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
                               Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                               return null;
                           });

            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);   

            mocks.VerifyAll();


        }

        [Test]
        public void TransactionRollback()
        {
            ITransactionScopeAdapter txAdapter = (ITransactionScopeAdapter)mocks.CreateMock(typeof(ITransactionScopeAdapter));

            using (mocks.Ordered())
            {
                Expect.Call(txAdapter.IsExistingTransaction).Return(false);
                TransactionOptions txOptions = new TransactionOptions();
                txOptions.IsolationLevel = IsolationLevel.ReadCommitted;
                txAdapter.CreateTransactionScope(TransactionScopeOption.Required, txOptions, EnterpriseServicesInteropOption.None);
                txAdapter.Dispose();
            }
            mocks.ReplayAll();

            IPlatformTransactionManager tm = new TxScopeTransactionManager(txAdapter);
            TransactionTemplate tt = new TransactionTemplate(tm);

            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            Exception ex = new ArgumentException("test exception");
            try
            {
                tt.Execute(delegate(ITransactionStatus status)
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


            mocks.VerifyAll();
        }

        [Test]
        public void PropagationRequiresNewWithExistingTransaction()
        {

            ITransactionScopeAdapter txAdapter = (ITransactionScopeAdapter)mocks.CreateMock(typeof(ITransactionScopeAdapter));

            using (mocks.Ordered())
            {
                Expect.Call(txAdapter.IsExistingTransaction).Return(false);
                TransactionOptions txOptions = new TransactionOptions();
                txOptions.IsolationLevel = IsolationLevel.ReadCommitted;
                txAdapter.CreateTransactionScope(TransactionScopeOption.RequiresNew, txOptions, EnterpriseServicesInteropOption.None);

                //inner tx actions
                Expect.Call(txAdapter.IsExistingTransaction).Return(true);
                txAdapter.CreateTransactionScope(TransactionScopeOption.RequiresNew, txOptions, EnterpriseServicesInteropOption.None);
                txAdapter.Dispose();
                //end inner tx actions

                Expect.Call(txAdapter.RollbackOnly).Return(false);
                txAdapter.Complete();
                txAdapter.Dispose();
                
            }
            mocks.ReplayAll();

            IPlatformTransactionManager tm = new TxScopeTransactionManager(txAdapter);
            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.Execute(delegate(ITransactionStatus status)
                           {
                               Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
                               Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
                               Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                               Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);

                               tt.Execute(delegate(ITransactionStatus status2)
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

            mocks.VerifyAll();
        }
    }
}
#endif