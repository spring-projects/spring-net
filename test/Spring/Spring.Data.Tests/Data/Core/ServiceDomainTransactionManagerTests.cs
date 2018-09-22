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

using System;
using System.EnterpriseServices;

using FakeItEasy;

using NUnit.Framework;

using Spring.Data.Support;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.Core
{
    /// <summary>
    /// This class contains tests for ServiceDomainTransactionManager
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ServiceDomainTransactionManagerTests
    {
        [Test]
        public void TransactionCommit()
        {
            IServiceDomainAdapter txAdapter = A.Fake<IServiceDomainAdapter>();

            A.CallTo(() => txAdapter.IsInTransaction).Returns(false).Once().Then.Returns(true).Once();

            //ProcessCommit - status.GlobalRollbackOnly check
            //DoCommit      - status.GlobalRollbackOnly check
            //DoCommit      - check to call SetComplete or SetAbort
            A.CallTo(() => txAdapter.MyTransactionVote).Returns(TransactionVote.Commit).NumberOfTimes(3);

            A.CallTo(() => txAdapter.Leave()).Returns(TransactionStatus.Commited).Once();

            ServiceDomainPlatformTransactionManager tm = new ServiceDomainPlatformTransactionManager(txAdapter);
            TransactionTemplate tt = new TransactionTemplate(tm);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            tt.Execute(TransactionCommitMethod);

            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);

            SimpleServiceConfig serviceConfig = new SimpleServiceConfig();
            ConfigureServiceConfig(serviceConfig, true);
            A.CallTo(() => txAdapter.Enter(serviceConfig)).MustHaveHappenedOnceExactly();
            A.CallTo(() => txAdapter.SetComplete()).MustHaveHappenedOnceExactly();
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

            SimpleServiceConfig serviceConfig = new SimpleServiceConfig();
            ConfigureServiceConfig(serviceConfig, standardIsolationAndProp: true);

            IServiceDomainAdapter txAdapter = A.Fake<IServiceDomainAdapter>();
            A.CallTo(() => txAdapter.IsInTransaction).Returns(false).Once().Then.Returns(true);
            A.CallTo(() => txAdapter.Leave()).Returns(TransactionStatus.Commited);

            ServiceDomainPlatformTransactionManager tm = new ServiceDomainPlatformTransactionManager(txAdapter);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

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

            A.CallTo(() => txAdapter.SetAbort()).MustHaveHappenedOnceExactly();
            A.CallTo(() => txAdapter.Enter(serviceConfig)).MustHaveHappenedOnceExactly();
        }


        [Test]
        public void PropagationRequiresNewWithExistingTransaction()
        {
            IServiceDomainAdapter txAdapter = A.Fake<IServiceDomainAdapter>();

            A.CallTo(() => txAdapter.IsInTransaction)
                .Returns(false).Once()
                .Then.Returns(true).NumberOfTimes(3);

            A.CallTo(() => txAdapter.Leave())
                .Returns(TransactionStatus.Aborted).Once()
                .Then.Returns(TransactionStatus.Commited).Once();

            A.CallTo(() => txAdapter.MyTransactionVote).Returns(TransactionVote.Commit).NumberOfTimes(3);

            ServiceDomainPlatformTransactionManager tm = new ServiceDomainPlatformTransactionManager(txAdapter);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.Execute(new PropagationRequiresNewWithExistingTransactionCallbackSD(tt));

            SimpleServiceConfig serviceConfig = new SimpleServiceConfig();
            ConfigureServiceConfig(serviceConfig, false);
            serviceConfig.TransactionOption = TransactionOption.RequiresNew;
            serviceConfig.IsolationLevel = TransactionIsolationLevel.ReadCommitted;
            A.CallTo(() => txAdapter.Enter(serviceConfig)).MustHaveHappened();

            ConfigureServiceConfig(serviceConfig, false);
            serviceConfig.TransactionOption = TransactionOption.RequiresNew;
            serviceConfig.IsolationLevel = TransactionIsolationLevel.ReadCommitted;
            A.CallTo(() => txAdapter.Enter(serviceConfig)).MustHaveHappened();

            A.CallTo(() => txAdapter.SetAbort()).MustHaveHappenedOnceExactly();
            A.CallTo(() => txAdapter.SetComplete()).MustHaveHappenedOnceExactly();
        }

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
                status.SetRollbackOnly();
                return null;
            }
        }
    }
}