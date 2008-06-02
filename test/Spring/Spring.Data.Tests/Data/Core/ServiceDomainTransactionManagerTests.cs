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


#if (!NET_1_0)

#region Imports

using System;
using System.EnterpriseServices;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Data.Support;
using Spring.Transaction;
using Spring.Transaction.Support;

#endregion

namespace Spring.Data.Core
{
    /// <summary>
    /// This class contains tests for ServiceDomainTransactionManager
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ServiceDomainTransactionManagerTests
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
            #region Mock Setup
            IServiceDomainAdapter txAdapter = (IServiceDomainAdapter) mocks.CreateMock(typeof (IServiceDomainAdapter));
            using (mocks.Ordered())
            {
                Expect.Call(txAdapter.IsInTransaction).Return(false);
                SimpleServiceConfig serviceConfig = new SimpleServiceConfig();
                ConfigureServiceConfig(serviceConfig, true);
                txAdapter.Enter(serviceConfig);

                //ProcessCommit - status.GlobalRollbackOnly check
                Expect.Call(txAdapter.MyTransactionVote).Return(TransactionVote.Commit);
                //DoCommit      - status.GlobalRollbackOnly check
                Expect.Call(txAdapter.MyTransactionVote).Return(TransactionVote.Commit);

                Expect.Call(txAdapter.IsInTransaction).Return(true);
                //DoCommit      - check to call SetComplete or SetAbort
                Expect.Call(txAdapter.MyTransactionVote).Return(TransactionVote.Commit);
                txAdapter.SetComplete();
                Expect.Call(txAdapter.Leave()).Return(TransactionStatus.Commited);

            }
            #endregion

            mocks.ReplayAll();

            IPlatformTransactionManager tm = new ServiceDomainPlatformTransactionManager(txAdapter);
            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.Execute(new TransactionDelegate(TransactionCommitMethod));

            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);

            mocks.VerifyAll();

        }

        private object TransactionCommitMethod(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            return null;
        }

        [Test]
        public void TransactionRollback()
        {
            #region Mock Setup
            IServiceDomainAdapter txAdapter = (IServiceDomainAdapter)mocks.CreateMock(typeof(IServiceDomainAdapter));
            using (mocks.Ordered())
            {

                Expect.Call(txAdapter.IsInTransaction).Return(false);
                SimpleServiceConfig serviceConfig = new SimpleServiceConfig();
                ConfigureServiceConfig(serviceConfig, true);
                txAdapter.Enter(serviceConfig);
                Expect.Call(txAdapter.IsInTransaction).Return(true);
                txAdapter.SetAbort();
                Expect.Call(txAdapter.Leave()).Return(TransactionStatus.Commited);

            }
            #endregion

            mocks.ReplayAll();

            IPlatformTransactionManager tm = new ServiceDomainPlatformTransactionManager(txAdapter);
            TransactionTemplate tt = new TransactionTemplate(tm);

            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            Exception ex = new ArgumentException("test exception");
            try
            {
                tt.Execute(new TransactionRollbackTxCallback(ex));
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
            #region Mock Setup
            IServiceDomainAdapter txAdapter = (IServiceDomainAdapter)mocks.CreateMock(typeof(IServiceDomainAdapter));
            using (mocks.Ordered())
            {

                Expect.Call(txAdapter.IsInTransaction).Return(false);
                SimpleServiceConfig serviceConfig = new SimpleServiceConfig();
                ConfigureServiceConfig(serviceConfig, true);
                txAdapter.Enter(serviceConfig);


                Expect.Call(txAdapter.IsInTransaction).Return(true);
                // inner tx
                ConfigureServiceConfig(serviceConfig, false);
                serviceConfig.TransactionOption = TransactionOption.RequiresNew;
                serviceConfig.IsolationLevel = TransactionIsolationLevel.ReadCommitted;
                txAdapter.Enter(serviceConfig);
                Expect.Call(txAdapter.IsInTransaction).Return(true);
                txAdapter.SetAbort();
                Expect.Call(txAdapter.Leave()).Return(TransactionStatus.Aborted);
                // innter tx aborted


                //ProcessCommit - status.GlobalRollbackOnly check
                Expect.Call(txAdapter.MyTransactionVote).Return(TransactionVote.Commit);
                //DoCommit      - status.GlobalRollbackOnly check
                Expect.Call(txAdapter.MyTransactionVote).Return(TransactionVote.Commit);

                Expect.Call(txAdapter.IsInTransaction).Return(true);
                //DoCommit      - check to call SetComplete or SetAbort
                Expect.Call(txAdapter.MyTransactionVote).Return(TransactionVote.Commit);
                txAdapter.SetComplete();

                Expect.Call(txAdapter.Leave()).Return(TransactionStatus.Commited);

            }
            #endregion 

            mocks.ReplayAll();

            IPlatformTransactionManager tm = new ServiceDomainPlatformTransactionManager(txAdapter);
            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.Execute(new PropagationRequiresNewWithExistingTransactionCallbackSD(tt));
            mocks.VerifyAll();
        }

        #region Helper Methods
        private SimpleServiceConfig ConfigureServiceConfig(SimpleServiceConfig serviceConfig, bool standardIsolationAndProp)
        {
            serviceConfig.TransactionDescription = null;

            serviceConfig.TrackingEnabled = true;
            serviceConfig.TrackingAppName = "Spring.NET";
            serviceConfig.TrackingComponentName = "ServiceDomainPlatformTransactionManager";
            if (standardIsolationAndProp)
            {
                serviceConfig.TransactionOption = TransactionOption.Required;
                serviceConfig.IsolationLevel = TransactionIsolationLevel.ReadCommitted;
            }
            return serviceConfig;

        }
        #endregion


        #region Supporting class for TransactionRollback test

        internal class TransactionRollbackTxCallback : ITransactionCallback
        {
            private Exception exception;

            public TransactionRollbackTxCallback(Exception exception)
            {
                this.exception = exception;
            }

            public object DoInTransaction(ITransactionStatus status)
            {
                Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
                Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
                throw exception;
            }
        }

        #endregion

        #region Supporting class for PropagationRequiresNewWithExistingTransactionCallback test
        internal class PropagationRequiresNewWithExistingTransactionCallbackSD : ITransactionCallback
        {
            private TransactionTemplate tt;

            public PropagationRequiresNewWithExistingTransactionCallbackSD(TransactionTemplate tt)
            {
                this.tt = tt;
            }

            public object DoInTransaction(ITransactionStatus status)
            {
                Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
                Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
                Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
                tt.Execute(new TransactionDelegate(TransactionMethod));
                Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
                Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
                return null;
            }

            private object TransactionMethod(ITransactionStatus status)
            {
                Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
                Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
                Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
                status.RollbackOnly = true;
                return null; 
            }
        }

        #endregion
    }


}
#endif