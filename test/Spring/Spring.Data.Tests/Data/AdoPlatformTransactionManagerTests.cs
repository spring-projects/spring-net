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
using System.Data;
using System.Threading;

using FakeItEasy;

using NUnit.Framework;

using Spring.Dao;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Data.Support;
using Spring.Support;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data
{
    /// <summary>
    /// This class contains tests for AdoPlatformTransactionManager
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class AdoPlatformTransactionManagerTests
    {
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

        [TearDown]
        public void TearDown()
        {
            Assert.IsTrue(TransactionSynchronizationManager.ResourceDictionary.Count == 0);
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);
        }

        [Test]
        public void TransactionCommit()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            tt.Execute(new TransactionCommitTxCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            //standard tx timeout.
            A.CallTo(() => transaction.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void TransactionRollback()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            Exception ex = new ArgumentException("test exception");
            try
            {
                tt.Execute(new TransactionRollbackTxCallback(dbProvider, ex));
                Assert.Fail("Should have thrown exception");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(ex, e);
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            //standard tx timeout.
            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void ParticipatingTransactionWithRollbackOnly()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            Assert.IsFalse(TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            TestTransactionSynchronization synch =
                new TestTransactionSynchronization(dbProvider, TransactionSynchronizationStatus.Rolledback);
            TransactionSynchronizationManager.RegisterSynchronization(synch);

            bool outerTransactionBoundaryReached = false;
            try
            {
                Assert.IsTrue(ts.IsNewTransaction);
                TransactionTemplate tt = new TransactionTemplate(tm);
                TransactionTemplate tt2 = new TransactionTemplate(tm);
                tt.Execute(new ParticipatingTxWithRollbackOnlyTxCallback(tt2, dbProvider));
                outerTransactionBoundaryReached = true;
                tm.Commit(ts);
                Assert.Fail("Should have thrown UnexpectedRollbackException");
            }
            catch (UnexpectedRollbackException)
            {
                // expected
                if (!outerTransactionBoundaryReached)
                {
                    tm.Rollback(ts);
                }

                Assert.IsTrue(outerTransactionBoundaryReached);
            }

            Assert.IsFalse(TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsFalse(synch.beforeCommitCalled);
            Assert.IsTrue(synch.beforeCompletionCalled);
            Assert.IsFalse(synch.afterCommitCalled);
            Assert.IsTrue(synch.afterCompletionCalled);

            //standard tx timeout.
            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void ParticipatingTransactionWithTransactionStartedFromSynch()
        {
        }

        [Test]
        public void ParticipatingTransactionWithRollbackOnlyAndInnerSynch()
        {
        }

        [Test]
        public void PropagationRequiresNewWithExistingTransaction()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;


            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            tt.Execute(new PropagationRequiresNewWithExistingTransactionCallback(tt, dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedTwiceExactly();
        }

        [Test]
        public void PropagationRequiresNewWithExistingTransactionAndUnrelatedDataSource()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);


            IDbProvider dbProvider2 = A.Fake<IDbProvider>();
            IDbConnection connection2 = A.Fake<IDbConnection>();
            IDbTransaction transaction2 = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider2.CreateConnection()).Returns(connection2);
            A.CallTo(() => connection2.BeginTransaction(DefaultIsolationLevel)).Returns(transaction2);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;

            AdoPlatformTransactionManager tm2 = new AdoPlatformTransactionManager(dbProvider2);
            tm2.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt2 = new TransactionTemplate(tm2);
            tt2.PropagationBehavior = TransactionPropagation.RequiresNew;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider2), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");


            tt.Execute(new PropagationRequiresNewWithExistingTransactionCallback(tt2, dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider2), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();

            A.CallTo(() => connection2.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction2.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection2.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void PropagationRequiresNewWithExistingTransactionAndUnrelatedFailingDataSource()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            IDbProvider dbProvider2 = A.Fake<IDbProvider>();
            IDbConnection connection2 = A.Fake<IDbConnection>();

            A.CallTo(() => dbProvider2.CreateConnection()).Returns(connection2);
            Exception failure = new Exception("can't open connection");
            A.CallTo(() => connection2.Open()).Throws(failure);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;

            AdoPlatformTransactionManager tm2 = new AdoPlatformTransactionManager(dbProvider2);
            tm2.TransactionSynchronization = TransactionSynchronizationState.Never;
            TransactionTemplate tt2 = new TransactionTemplate(tm2);
            tt2.PropagationBehavior = TransactionPropagation.RequiresNew;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider2), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            try
            {
                tt.Execute(
                    new PropagationRequiresNewWithExistingTransactionAndUnrelatedFailingDataSourceCallback(tt2));
                Assert.Fail("Should have thrown CannotCreateTransactionException");
            }
            catch (CannotCreateTransactionException ex)
            {
                Assert.AreSame(failure, ex.InnerException);
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider2), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void PropagationNotSupportedWithExistingTransaction()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            tt.Execute(new PropagationNotSupportedWithExistingTransactionCallback(tt, dbProvider));

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void PropagationNeverWithExistingTransaction()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            try
            {
                tt.Execute(new PropagationNeverWithExistingTransactionCallback(tt));
                Assert.Fail("Should have thrown IllegalTransactionStateException");
            }
            catch (IllegalTransactionStateException)
            {
                //expected.
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void PropagationRequiresNewWithExistingConnection()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbConnection connection2 = A.Fake<IDbConnection>();
            IDbTransaction transaction2 = A.Fake<IDbTransaction>();

            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection).Once()
                .Then.Returns(connection2).Once();

            A.CallTo(() => connection2.BeginTransaction(DefaultIsolationLevel)).Returns(transaction2);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Supports;
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            tt.Execute(new PropagationRequiresNewWithExistingConnectionCallback(tt, connection, connection2, dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection2.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection2.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void TransactionWithIsolation()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(IsolationLevel.Serializable)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.TransactionIsolationLevel = IsolationLevel.Serializable;
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            tt.Execute(new TransactionCommitTxCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void TransactionWithLongTimeout()
        {
            DoTransactionWithTimeout(10);
        }

        [Test]
        public void TransactionWithShortTimeout()
        {
            DoTransactionWithTimeout(1);
        }

        private void DoTransactionWithTimeout(int timeout)
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();
            IDbCommand command = A.Fake<IDbCommand>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);
            A.CallTo(() => connection.CreateCommand()).Returns(command);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.TransactionTimeout = timeout;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            try
            {
                tt.Execute(new TransactionWithTimeoutCallback(dbProvider));
                if (timeout <= 1)
                {
                    Assert.Fail("Should have thrown TransactionTimedOutException");
                }
            }
            catch (TransactionTimedOutException)
            {
                if (timeout <= 1)
                {
                    //expected
                }
                else
                {
                    throw;
                }
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
            A.CallToSet(() => command.CommandText).WhenArgumentsMatch(x => x.Get<string>(0) == "some SQL statement").MustHaveHappenedOnceExactly();

            if (timeout > 1)
            {
                A.CallToSet(() => command.CommandTimeout).WhenArgumentsMatch(x => (int) x[0] == (timeout - 1)).MustHaveHappenedOnceExactly();
                A.CallTo(() => transaction.Commit()).MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            }
        }

        [Test]
        public void TransactionWithExceptionOnBegin()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();

            // CreateConnection is called in AdoPlatformTransactionManager.DoBegin
            A.CallTo(() => dbProvider.CreateConnection()).Throws(new TestSqlException("Cannot begin", "314"));

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            try
            {
                tt.Execute(TransactionWithExceptionNoOp);
            }
            catch (CannotCreateTransactionException)
            {
                // expected
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
        }

        private object TransactionWithExceptionNoOp(ITransactionStatus status)
        {
            return null;
        }

        [Test]
        public void TransactionWithExceptionOnCommit()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);
            A.CallTo(() => transaction.Commit()).Throws(new TestSqlException("Cannot commit", "314"));

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);

            try
            {
                tt.Execute(TransactionWithExceptionNoOp);
            }
            catch (TransactionSystemException)
            {
                //expected
            }


            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }


        [Test]
        public void TransactionWithExceptionOnCommitAndRollbackOnCommitFailure()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);
            A.CallTo(() => transaction.Commit()).Throws(new TestSqlException("Cannot commit", "314"));

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            tm.RollbackOnCommitFailure = true;
            TransactionTemplate tt = new TransactionTemplate(tm);

            try
            {
                tt.Execute(TransactionWithExceptionNoOp);
            }
            catch (TransactionSystemException)
            {
                //expected
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void TransactionWithExceptionOnRollback()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);
            A.CallTo(() => transaction.Rollback()).Throws(new TestSqlException("Cannot commit", "314"));

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);

            try
            {
                tt.Execute(TransactionWithExceptionOnRollbackMethod);
            }
            catch (TransactionSystemException)
            {
                //expected
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        private object TransactionWithExceptionOnRollbackMethod(ITransactionStatus status)
        {
            status.SetRollbackOnly();
            return null;
        }

        [Test]
        public void TransactionWithPropagationSupports()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Supports;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            tt.Execute(new TransactionWithPropagationSupportsCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
        }

        [Test]
        public void TransactionWithPropagationNotSupported()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.NotSupported;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            tt.Execute(new TransactionWithPropagationNotSupportedCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
        }

        [Test]
        public void TransactionWithPropagationNever()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Never;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            tt.Execute(new TransactionWithPropagationNotSupportedCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
        }

        [Test]
        public void ExistingTransactionWithPropagationNestedNotSupported()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Nested;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            try
            {
                tt.Execute(new ExistingTransactionWithPropagationNestedCallback(dbProvider, tt));
                Assert.Fail("Should have thrown NestedTransactionNotSupportedException");
            }
            catch (NestedTransactionNotSupportedException)
            {
                // expected
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void TransactionWithPropagationNested()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Nested;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");


            tt.Execute(TransactionWithPropagationNestedMethod);

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        private object TransactionWithPropagationNestedMethod(ITransactionStatus status)
        {
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            return null;
        }

        [Test]
        public void TransactionWithPropagationNestedAndRollback()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            IDbTransaction transaction = A.Fake<IDbTransaction>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.BeginTransaction(DefaultIsolationLevel)).Returns(transaction);

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Nested;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");


            tt.Execute(TransactionWithPropagationNestedAndRollbackMethod);

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            A.CallTo(() => connection.Open()).MustHaveHappenedOnceExactly();
            A.CallTo(() => transaction.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Dispose()).MustHaveHappenedOnceExactly();
        }

        private object TransactionWithPropagationNestedAndRollbackMethod(ITransactionStatus status)
        {
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            status.SetRollbackOnly();
            return null;
        }
    }

    internal class ExistingTransactionWithPropagationNestedCallback : ITransactionCallback
    {
        private IDbProvider dbProvider;
        private TransactionTemplate tt;

        public ExistingTransactionWithPropagationNestedCallback(IDbProvider dbProvider, TransactionTemplate transactionTemplate)
        {
            this.dbProvider = dbProvider;
            tt = transactionTemplate;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            //TODO: Note no support for savepoints at this time (1.1), so can't check that a savepoint isn't present.

            tt.Execute(new ExistingTransactionWithPropagationNestedCallback2(dbProvider));

            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            //TODO: Note no support for savepoints at this time (1.1), so can't check that a savepoint isn't present.
            return null;
        }
    }

    internal class ExistingTransactionWithPropagationNestedCallback2 : ITransactionCallback
    {
        private IDbProvider dbProvider;

        public ExistingTransactionWithPropagationNestedCallback2(IDbProvider provider)
        {
            dbProvider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(dbProvider), "Has thread db provider");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronizations active");
            Assert.IsTrue(!status.IsNewTransaction, "Isn't new transaction");
            //TODO: Note no support for savepoints at this time (1.1), so can't check that a savepoint is present.
            return null;
        }
    }

    internal class TransactionWithPropagationNotSupportedCallback : ITransactionCallback
    {
        private IDbProvider provider;

        public TransactionWithPropagationNotSupportedCallback(IDbProvider provider)
        {
            this.provider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(provider), "Hasn't thread db provider");
            Assert.IsTrue(!status.IsNewTransaction, "Is not new transaction");
            return null;
        }
    }

    internal class TransactionWithPropagationSupportsCallback : ITransactionCallback
    {
        private IDbProvider provider;

        public TransactionWithPropagationSupportsCallback(IDbProvider provider)
        {
            this.provider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(provider), "Hasn't thread db provider");
            Assert.IsTrue(!status.IsNewTransaction, "Is not new transaction");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);
            return null;
        }
    }


    internal class TransactionWithTimeoutCallback : ITransactionCallback
    {
        private IDbProvider provider;

        public TransactionWithTimeoutCallback(IDbProvider provider)
        {
            this.provider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            try
            {
                Thread.Sleep(1500);
            }
            catch (Exception)
            {
            }

            try
            {
                IDbConnection con = ConnectionUtils.GetConnection(provider);
                IDbCommand cmd = con.CreateCommand();
                cmd.CommandText = "some SQL statement";
                ConnectionUtils.ApplyTransactionTimeout(cmd, provider);
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(TransactionTimedOutException))
                {
                    throw new DataAccessResourceFailureException("", e);
                }

                throw;
            }

            return null;
        }
    }

    internal class PropagationRequiresNewWithExistingConnectionCallback : ITransactionCallback
    {
        private TransactionTemplate tt;
        private IDbProvider dbProvider;
        private IDbConnection connection;
        private IDbConnection connection2;

        public PropagationRequiresNewWithExistingConnectionCallback(TransactionTemplate transactionTemplate, IDbConnection connection, IDbConnection connection2, IDbProvider provider)
        {
            tt = transactionTemplate;
            this.connection = connection;
            this.connection2 = connection2;
            dbProvider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronizations active");
            Assert.AreSame(connection, ConnectionUtils.GetConnection(dbProvider));
            Assert.AreSame(connection, ConnectionUtils.GetConnection(dbProvider));
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.ReadOnly = true;
            tt.Execute(new PropagationRequiresNewWithExistingConnectionCallback2(dbProvider, connection2));
            Assert.AreSame(connection, ConnectionUtils.GetConnection(dbProvider));
            return null;
        }
    }

    internal class PropagationRequiresNewWithExistingConnectionCallback2 : ITransactionCallback
    {
        private IDbProvider dbProvider;
        private IDbConnection connection2;


        public PropagationRequiresNewWithExistingConnectionCallback2(IDbProvider dbProvider, IDbConnection connection2)
        {
            this.dbProvider = dbProvider;
            this.connection2 = connection2;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(dbProvider), "Has thread db provider");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronizations active");
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            Assert.AreSame(connection2, ConnectionUtils.GetConnection(dbProvider));
            Assert.AreSame(connection2, ConnectionUtils.GetConnection(dbProvider));

            return null;
        }
    }

    internal class PropagationNeverWithExistingTransactionCallback : ITransactionCallback
    {
        private TransactionTemplate innerTxTemplate;

        public PropagationNeverWithExistingTransactionCallback(TransactionTemplate transactionTemplate)
        {
            innerTxTemplate = transactionTemplate;
        }


        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            innerTxTemplate.PropagationBehavior = TransactionPropagation.Never;
            innerTxTemplate.Execute(new PropagationNeverWithExistingTransactionCallback2());
            Assert.Fail("Should have thrown IllegalTransactionStateException");
            return null;
        }
    }

    internal class PropagationNeverWithExistingTransactionCallback2 : ITransactionCallback
    {
        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.Fail("Should have thrown IllegalTransactionStateException");
            return null;
        }
    }

    internal class PropagationNotSupportedWithExistingTransactionCallback : ITransactionCallback
    {
        private TransactionTemplate innerTxTemplate;
        private IDbProvider dbProvider;

        public PropagationNotSupportedWithExistingTransactionCallback(TransactionTemplate transactionTemplate, IDbProvider provider)
        {
            innerTxTemplate = transactionTemplate;
            dbProvider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            innerTxTemplate.PropagationBehavior = TransactionPropagation.NotSupported;
            innerTxTemplate.Execute(new PropagationNotSupportedWithExistingTransactionCallback2(dbProvider));
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            return null;
        }
    }

    internal class PropagationNotSupportedWithExistingTransactionCallback2 : ITransactionCallback
    {
        private IDbProvider provider;

        public PropagationNotSupportedWithExistingTransactionCallback2(IDbProvider provider)
        {
            this.provider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(provider), "Hasn't thread db provider");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsTrue(!status.IsNewTransaction, "Isn't new transaction");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);
            status.SetRollbackOnly();
            return null;
        }
    }

    internal class PropagationRequiresNewWithExistingTransactionAndUnrelatedFailingDataSourceCallback : ITransactionCallback
    {
        private TransactionTemplate innerTxTemplate;


        public PropagationRequiresNewWithExistingTransactionAndUnrelatedFailingDataSourceCallback(TransactionTemplate transactionTemplate)
        {
            innerTxTemplate = transactionTemplate;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            innerTxTemplate.Execute(new PropagationRequiresNewWithExistingTransactionAndUnrelatedFailingDataSourceCallback2());
            return null;
        }
    }

    internal class PropagationRequiresNewWithExistingTransactionAndUnrelatedFailingDataSourceCallback2 : ITransactionCallback
    {
        public object DoInTransaction(ITransactionStatus status)
        {
            status.SetRollbackOnly();
            return null;
        }
    }

    internal class PropagationRequiresNewWithExistingTransactionCallback : ITransactionCallback
    {
        private TransactionTemplate innerTxTemplate;
        private IDbProvider dbProvider;

        public PropagationRequiresNewWithExistingTransactionCallback(TransactionTemplate transactionTemplate,
            IDbProvider provider)
        {
            innerTxTemplate = transactionTemplate;
            dbProvider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            innerTxTemplate.Execute(new PropagationRequiresNewWithExistingTransactionCallback2(dbProvider));

            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            return null;
        }
    }

    internal class PropagationRequiresNewWithExistingTransactionCallback2 : ITransactionCallback
    {
        private IDbProvider dbProvider;

        public PropagationRequiresNewWithExistingTransactionCallback2(IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(dbProvider), "Has thread connection");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            status.SetRollbackOnly();
            return null;
        }
    }

    internal class TransactionCommitTxCallback : ITransactionCallback
    {
        private IDbProvider provider;

        public TransactionCommitTxCallback(IDbProvider provider)
        {
            this.provider = provider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(provider), "Has thread db provider");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            return null;
        }
    }

    internal class TransactionRollbackTxCallback : ITransactionCallback
    {
        private IDbProvider provider;
        private Exception exception;

        public TransactionRollbackTxCallback(IDbProvider provider, Exception exception)
        {
            this.provider = provider;
            this.exception = exception;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(provider), "Hasn't thread db provider");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive);
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            throw exception;
        }
    }

    internal class ParticipatingTxWithRollbackOnlyTxCallback : ITransactionCallback
    {
        private TransactionTemplate innerTxTemplate;
        private IDbProvider dbProvider;


        public ParticipatingTxWithRollbackOnlyTxCallback(TransactionTemplate transactionTemplate, IDbProvider dbProvider)
        {
            innerTxTemplate = transactionTemplate;
            this.dbProvider = dbProvider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(!status.IsNewTransaction, "Is existing transaction");
            Assert.IsFalse(status.RollbackOnly, "Is not rollback-only");
            innerTxTemplate.Execute(new ParticipatingTxWithRollbackOnlyTxCallback2(dbProvider));
            Assert.IsTrue(!status.IsNewTransaction, "Is existing transaction");
            Assert.IsTrue(status.RollbackOnly, "Is rollback-only");
            return null;
        }
    }

    internal class ParticipatingTxWithRollbackOnlyTxCallback2 : ITransactionCallback
    {
        private IDbProvider dbProvider;

        public ParticipatingTxWithRollbackOnlyTxCallback2(IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(dbProvider), "Has thread connection");
            Assert.IsTrue(TransactionSynchronizationManager.SynchronizationActive, "Synchronization active");
            Assert.IsTrue(!status.IsNewTransaction, "Is existing transaction");
            status.SetRollbackOnly();
            return null;
        }
    }


    internal class TestTransactionSynchronization : ITransactionSynchronization
    {
        private IDbProvider provider;
        private TransactionSynchronizationStatus status;

        public bool beforeCommitCalled;
        public bool beforeCompletionCalled;
        public bool afterCommitCalled;
        public bool afterCompletionCalled;

        public TestTransactionSynchronization(IDbProvider provider,
            TransactionSynchronizationStatus synchronizationStatus)
        {
            this.provider = provider;
            status = synchronizationStatus;
        }


        public void Suspend()
        {
        }

        public void Resume()
        {
        }

        public void BeforeCommit(bool readOnly)
        {
            if (status == TransactionSynchronizationStatus.Committed)
            {
                Assert.Fail("Should never be called");
            }

            Assert.IsFalse(beforeCommitCalled);
            beforeCommitCalled = true;
        }

        public void AfterCommit()
        {
            if (status != TransactionSynchronizationStatus.Committed)
            {
                Assert.Fail("Should never be called");
            }

            Assert.IsFalse(afterCommitCalled);
            afterCommitCalled = true;
        }

        public void BeforeCompletion()
        {
            Assert.IsFalse(beforeCompletionCalled);
            beforeCompletionCalled = true;
        }

        public void AfterCompletion(TransactionSynchronizationStatus syncStatus)
        {
            Assert.IsFalse(afterCompletionCalled);
            afterCompletionCalled = true;
            Assert.IsTrue(syncStatus == status);
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(provider));
        }
    }
}