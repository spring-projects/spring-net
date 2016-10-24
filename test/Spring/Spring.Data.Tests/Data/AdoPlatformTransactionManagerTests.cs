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

#region Imports

using System;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Dao;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Data.Support;
using Spring.Support;
using Spring.Transaction;
using Spring.Transaction.Support;

#endregion

namespace Spring.Data
{
    /// <summary>
    /// This class contains tests for AdoPlatformTransactionManager
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class AdoPlatformTransactionManagerTests
    {
        private MockRepository mocks;
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

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
            #region Mock setup
            IDbProvider dbProvider = (IDbProvider) mocks.CreateMock(typeof (IDbProvider));
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                //standard tx timeout.
                transaction.Commit();
                LastCall.On(transaction).Repeat.Once();
                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            tt.Execute(new TransactionCommitTxCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            mocks.VerifyAll();
        }

        [Test]
        public void TransactionRollback()
        {
            #region Mock Setup

            IDbProvider dbProvider = (IDbProvider) mocks.CreateMock(typeof (IDbProvider));
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                //standard tx timeout.
                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();

                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

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


            mocks.VerifyAll();
        }

        [Test]
        public void ParticipatingTransactionWithRollbackOnly()
        {
            IDbProvider dbProvider = (IDbProvider) mocks.CreateMock(typeof (IDbProvider));
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                //standard tx timeout.
                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();
                connection.Dispose();
            }
            mocks.ReplayAll();

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

            mocks.VerifyAll();
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
            #region Mock Setup
            IDbProvider dbProvider = (IDbProvider) mocks.CreateMock(typeof (IDbProvider));
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            Expect.Call(dbProvider.CreateConnection()).Return(connection).Repeat.Twice();
            connection.Open();
            LastCall.On(connection).Repeat.Twice();
            Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction).Repeat.Twice();
            //standard tx timeout.
            transaction.Rollback();
            LastCall.On(transaction).Repeat.Once();

            transaction.Commit();
            LastCall.On(transaction).Repeat.Once();

            connection.Dispose();
            LastCall.On(connection).Repeat.Twice();

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;


            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            tt.Execute(new PropagationRequiresNewWithExistingTransactionCallback(tt, dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            mocks.VerifyAll();
        }

        [Test]
        public void PropagationRequiresNewWithExistingTransactionAndUnrelatedDataSource()
        {
            IDbProvider dbProvider = (IDbProvider) mocks.CreateMock(typeof (IDbProvider));
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            Expect.Call(dbProvider.CreateConnection()).Return(connection);
            connection.Open();
            LastCall.On(connection).Repeat.Once();
            Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
            transaction.Commit();
            LastCall.On(transaction).Repeat.Once();
            connection.Dispose();

            IDbProvider dbProvider2 = (IDbProvider) mocks.CreateMock(typeof (IDbProvider));
            IDbConnection connection2 = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction2 = mocks.StrictMock<IDbTransaction>();

            Expect.Call(dbProvider2.CreateConnection()).Return(connection2);
            connection2.Open();
            LastCall.On(connection2).Repeat.Once();
            Expect.Call(connection2.BeginTransaction(DefaultIsolationLevel)).Return(transaction2);
            transaction2.Rollback();
            LastCall.On(transaction2).Repeat.Once();
            connection2.Dispose();

            mocks.ReplayAll();

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

            mocks.VerifyAll();
        }

        [Test]
        public void PropagationRequiresNewWithExistingTransactionAndUnrelatedFailingDataSource()
        {
            #region Mock Setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();            
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            Expect.Call(dbProvider.CreateConnection()).Return(connection);
            connection.Open();
            LastCall.On(connection).Repeat.Once();
            Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
            transaction.Rollback();
            LastCall.On(transaction).Repeat.Once();
            connection.Dispose();

            IDbProvider dbProvider2 = mocks.StrictMock<IDbProvider>();
            IDbConnection connection2 = mocks.StrictMock<IDbConnection>();
           
            Expect.Call(dbProvider2.CreateConnection()).Return(connection2);
            connection2.Open();
            Exception failure = new Exception("can't open connection");
            LastCall.On(connection2).Throw(failure);

            #endregion

            mocks.ReplayAll();

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
            } catch(CannotCreateTransactionException ex)
            {
                Assert.AreSame(failure, ex.InnerException);
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider2), "Hasn't thread db provider");
            mocks.VerifyAll();
        }

        [Test]
        public void PropagationNotSupportedWithExistingTransaction()
        {
            #region Mock Setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                //standard tx timeout.
                transaction.Commit();
                LastCall.On(transaction).Repeat.Once();
                connection.Dispose();
            }
            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            tt.Execute(new PropagationNotSupportedWithExistingTransactionCallback(tt, dbProvider));

            mocks.VerifyAll();
        }

        [Test]
        public void PropagationNeverWithExistingTransaction()
        {
            #region Mock Setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                //standard tx timeout.
                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();

                connection.Dispose();
            }
            #endregion

            mocks.ReplayAll();

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
            } catch (IllegalTransactionStateException)
            {
                //expected.
            }
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            mocks.VerifyAll();
        }

        [Test]
        public void PropagationRequiresNewWithExistingConnection()
        {
            #region Mock Setup

            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            
            Expect.Call(dbProvider.CreateConnection()).Return(connection);
            connection.Open();
            LastCall.On(connection).Repeat.Once();
            connection.Dispose();

            IDbConnection connection2 = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction2 = mocks.StrictMock<IDbTransaction>();

            Expect.Call(dbProvider.CreateConnection()).Return(connection2);
            connection2.Open();
            LastCall.On(connection2).Repeat.Once();
            Expect.Call(connection2.BeginTransaction(DefaultIsolationLevel)).Return(transaction2);
            transaction2.Commit();
            LastCall.On(transaction2).Repeat.Once();
            connection2.Dispose();

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Supports;
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            tt.Execute(new PropagationRequiresNewWithExistingConnectionCallback(tt, connection, connection2, dbProvider));


            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            mocks.VerifyAll();
        }

        [Test]
        public void TransactionWithIsolation()
        {
            #region Mock setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(IsolationLevel.Serializable)).Return(transaction);
                //standard tx timeout.
                transaction.Commit();
                LastCall.On(transaction).Repeat.Once();
                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.TransactionIsolationLevel = IsolationLevel.Serializable;
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            tt.Execute(new TransactionCommitTxCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            mocks.VerifyAll();
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
            #region Mock setup

            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();
            IDbCommand command = mocks.StrictMock<IDbCommand>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
               
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                Expect.Call(connection.CreateCommand()).Return(command);
                command.CommandText = "some SQL statement";
                LastCall.On(command).Repeat.Once();
                if (timeout > 1)
                {
                    command.CommandTimeout = (timeout - 1);
                    transaction.Commit();
                } else
                {
                    transaction.Rollback();
                }                
                LastCall.On(transaction).Repeat.Once();
                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

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
            } catch (TransactionTimedOutException)
            {
                if (timeout <=1 )
                {
                    //expected
                } else
                {
                    throw;
                }
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            mocks.VerifyAll();
        }

        [Test]
        public void TransactionWithExceptionOnBegin()
        {
            #region Mock setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();       
            
            // CreateConnection is called in AdoPlatformTransactionManager.DoBegin
            Expect.Call(dbProvider.CreateConnection()).Throw(new TestSqlException("Cannot begin", "314"));     
            

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            try
            {
                tt.Execute(new TransactionDelegate(TransactionWithExceptionNoOp));                
            } catch (CannotCreateTransactionException)
            {
                // expected
            }


            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            mocks.VerifyAll();
        }

        private object TransactionWithExceptionNoOp(ITransactionStatus status)
        {
            return null;
        }

        [Test]
        public void TransactionWithExceptionOnCommit()
        {
            #region Mock setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                //standard tx timeout.
                transaction.Commit();
                LastCall.On(transaction).Throw(new TestSqlException("Cannot commit", "314"));
                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            
            try
            {
                tt.Execute(new TransactionDelegate(TransactionWithExceptionNoOp));
            } catch (TransactionSystemException)
            {
                //expected
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            mocks.VerifyAll();
            
        }


        [Test]
        public void TransactionWithExceptionOnCommitAndRollbackOnCommitFailure()
        {
            #region Mock Setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                transaction.Commit();
                LastCall.On(transaction).Throw(new TestSqlException("Cannot commit", "314"));

                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();

                connection.Dispose();
            }
            #endregion
            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            tm.RollbackOnCommitFailure = true;
            TransactionTemplate tt = new TransactionTemplate(tm);

            try
            {
                tt.Execute(new TransactionDelegate(TransactionWithExceptionNoOp));
            }
            catch (TransactionSystemException)
            {
                //expected
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            mocks.VerifyAll();
        }

        [Test]
        public void TransactionWithExceptionOnRollback()
        {
            #region Mock Setup

            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                //standard tx timeout.
                transaction.Rollback();
                LastCall.On(transaction).Throw(new TestSqlException("Cannot commit", "314"));

                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);

            try
            {
                tt.Execute(new TransactionDelegate(TransactionWithExceptionOnRollbackMethod));
            }
            catch (TransactionSystemException)
            {
                //expected
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            mocks.VerifyAll();

        }

        private object TransactionWithExceptionOnRollbackMethod(ITransactionStatus status)
        {
            status.SetRollbackOnly();
            return null;
        }

        [Test]
        public void TransactionWithPropagationSupports()
        {
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Supports;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            tt.Execute(new TransactionWithPropagationSupportsCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            mocks.VerifyAll();
        }

        [Test]
        public void TransactionWithPropagationNotSupported()
        {
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.NotSupported;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            tt.Execute(new TransactionWithPropagationNotSupportedCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            mocks.VerifyAll();
        }

        [Test]
        public void TransactionWithPropagationNever()
        {
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Never;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");

            tt.Execute(new TransactionWithPropagationNotSupportedCallback(dbProvider));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            mocks.VerifyAll();
        }

        [Test]
        public void ExistingTransactionWithPropagationNestedNotSupported()
        {
            #region Mock setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                
                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();
                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

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
            } catch (NestedTransactionNotSupportedException)
            {
                // expected 
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            mocks.VerifyAll();
        }

        [Test]
        public void TransactionWithPropagationNested()
        {
            #region Mock setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);
                //standard tx timeout.
                transaction.Commit();
                LastCall.On(transaction).Repeat.Once();
                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Nested;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");


            tt.Execute(new TransactionDelegate(TransactionWithPropagationNestedMethod));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            mocks.VerifyAll();


        }

        private object TransactionWithPropagationNestedMethod(ITransactionStatus status)
        {
            Assert.IsTrue(status.IsNewTransaction, "Is new transaction");
            return null;
        }

        [Test]
        public void TransactionWithPropagationNestedAndRollback()
        {
            #region Mock setup
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbTransaction transaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                connection.Open();
                LastCall.On(connection).Repeat.Once();
                Expect.Call(connection.BeginTransaction(DefaultIsolationLevel)).Return(transaction);               
                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();
                connection.Dispose();
            }

            #endregion

            mocks.ReplayAll();

            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(dbProvider);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Nested;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");


            tt.Execute(new TransactionDelegate(TransactionWithPropagationNestedAndRollbackMethod));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(dbProvider), "Hasn't thread db provider");
            mocks.VerifyAll();


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

    #region Supporting class for TransactionWithPropagationNotSupported
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
    #endregion

    #region Supporting class for TransactionWithPropagationSupports
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
            Assert.IsTrue(!status.IsNewTransaction,"Is not new transaction");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);
            return null;
        }
    }
    #endregion


    #region Supporting class for TransactionWithTimeout

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
            } catch (Exception)
            {
                
            }
            try
            {
                IDbConnection con = ConnectionUtils.GetConnection(provider);
                IDbCommand cmd = con.CreateCommand();
                cmd.CommandText = "some SQL statement";
                ConnectionUtils.ApplyTransactionTimeout(cmd, provider);

            } catch (Exception e)
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
    #endregion

    #region Supporting class for PropagationRequiresNewWithExistingConnection

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

    #endregion

    #region Supporting classes for PropagationNeverWithExistingTransaction

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

    #endregion

    #region Supporting classes for PropagationNotSupportedWithExistingTransaction

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

    #endregion

    #region Supporting class for PropagationRequiresNewWithExistingTransactionAndUnrelatedFailingDataSource

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

    #endregion

    #region Supporting class for PropagationRequiresNewWithExistingTransaction

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

    #endregion

    #region Supporting class for TransactionCommit test

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

    #endregion

    #region Supporting class for TransactionRollback test

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

    #endregion

    #region Supporting class for ParticipatingTxWithRollbackOnly test

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

    #endregion



    #region Helper class

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

    #endregion
}