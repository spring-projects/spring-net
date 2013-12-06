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
using System.Collections;
using System.Data;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Dao;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Support;
using Spring.Transaction;
using Spring.Transaction.Support;

#endregion

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// This class contains tests for the HibernateTransactionManager
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class HibernateTransactionManagerTests
    {
        private class TestableHibernateTransactionManager : HibernateTransactionManager
        {
            private IDbTransaction _stubbedTransactionWithExpectedConnection;

            public TestableHibernateTransactionManager()
            {
            }

            public TestableHibernateTransactionManager(ISessionFactory sessionFactory) : base(sessionFactory)
            {
            }

            public IDbTransaction StubbedTransactionThatReturnsExpectedConnection
            {
                set { _stubbedTransactionWithExpectedConnection = value; }
            }

            protected override IDbTransaction GetIDbTransaction(ITransaction hibernateTx)
            {
                return _stubbedTransactionWithExpectedConnection;
            }
        }

        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        public void TransactionCommit()
        {
            IDbProvider provider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();
            IQuery query = mocks.StrictMock<IQuery>();

            IList list = new ArrayList();
            list.Add("test");
            using (mocks.Ordered())
            {
                Expect.Call(sessionFactory.OpenSession()).Return(session);
                Expect.Call(session.Connection).Return(connection);
                Expect.Call(session.BeginTransaction(IsolationLevel.Serializable)).Return(transaction);
                Expect.Call(session.IsOpen).Return(true);
                Expect.Call(session.CreateQuery("some query string")).Return(query);
                Expect.Call(query.List()).Return(list);
                transaction.Commit();
                LastCall.On(transaction).Repeat.Once();
                Expect.Call(session.Close()).Return(null);
            }

            mocks.ReplayAll();


            LocalSessionFactoryObjectStub lsfo = new LocalSessionFactoryObjectStub(sessionFactory);
            lsfo.AfterPropertiesSet();

            ISessionFactory sfProxy = lsfo.GetObject() as ISessionFactory;
            Assert.IsNotNull(sfProxy);

            HibernateTransactionManager tm = new HibernateTransactionManager();
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;
            tm.AdoExceptionTranslator = new FallbackExceptionTranslator();
            tm.SessionFactory = sfProxy;
            tm.DbProvider = provider;
            TransactionTemplate tt = new TransactionTemplate(tm);

            tt.TransactionIsolationLevel = IsolationLevel.Serializable;

            Assert.IsFalse(TransactionSynchronizationManager.HasResource(sfProxy),"Hasn't thread session");
            Assert.IsFalse(TransactionSynchronizationManager.HasResource(provider), "Hasn't thread db provider");
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive, "Actual transaction not active");

            object result = tt.Execute(new TransactionCommitTxCallback(sfProxy, provider));

            Assert.IsTrue(result == list, "Incorrect result list");

            Assert.IsFalse(TransactionSynchronizationManager.HasResource(sfProxy), "Hasn't thread session");
            Assert.IsFalse(TransactionSynchronizationManager.HasResource(provider), "Hasn't thread db provider");
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive, "Actual transaction not active");


            mocks.VerifyAll();

        }


        [Test]
        public void TransactionRollback()
        {

            IDbProvider provider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();
            IDbTransaction adoTransaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(sessionFactory.OpenSession()).Return(session);
                Expect.Call(session.Connection).Return(connection);
                Expect.Call(session.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session.IsOpen).Return(true);

                Expect.Call(adoTransaction.Connection).Return(connection);
                LastCall.On(adoTransaction).Repeat.Once();

                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();

                Expect.Call(session.Close()).Return(null);
            }
            mocks.ReplayAll();

            TestableHibernateTransactionManager tm = new TestableHibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;
            tm.StubbedTransactionThatReturnsExpectedConnection = adoTransaction;

            TransactionTemplate tt = new TransactionTemplate(tm);

            Assert.IsFalse(TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            try
            {
                tt.Execute(new TransactionRollbackTxCallback(sessionFactory));
                Assert.Fail("Should have thrown exception");
            } catch (ArgumentException)
            {
                
            }

            Assert.IsFalse(TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");
            Assert.IsFalse(TransactionSynchronizationManager.HasResource(provider), "Hasn't thread db provider");
            
            mocks.VerifyAll();
        }

        [Test]
        public void TransactionRollbackOnly()
        {            
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();
            IDbTransaction adoTransaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(sessionFactory.OpenSession()).Return(session);
                Expect.Call(session.Connection).Return(connection);
                Expect.Call(session.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session.IsOpen).Return(true);
                Expect.Call(session.FlushMode).Return(FlushMode.Auto);
                session.Flush();
                LastCall.On(session).Repeat.Once();

                Expect.Call(adoTransaction.Connection).Return(connection);
                LastCall.On(adoTransaction).Repeat.Once();

                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();
                Expect.Call(session.Close()).Return(null);
            }
            mocks.ReplayAll();


            TestableHibernateTransactionManager tm = new TestableHibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;
            tm.StubbedTransactionThatReturnsExpectedConnection = adoTransaction;

            TransactionTemplate tt = new TransactionTemplate(tm);

            Assert.IsFalse(TransactionSynchronizationManager.HasResource(sessionFactory), "Shouldn't have a thread session");

            tt.Execute(new TransactionRollbackOnlyTxCallback(sessionFactory));

            Assert.IsFalse(TransactionSynchronizationManager.HasResource(sessionFactory), "Shouldn't have a thread session");
            
            mocks.VerifyAll();
            
        }

        [Test]
        public void ParticipatingTransactionWithCommit()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(sessionFactory.OpenSession()).Return(session);
                Expect.Call(session.Connection).Return(connection);
                Expect.Call(session.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session.IsOpen).Return(true);
                Expect.Call(session.FlushMode).Return(FlushMode.Auto);
                session.Flush();
                LastCall.On(session).Repeat.Once();
                transaction.Commit();
                LastCall.On(transaction).Repeat.Once();
                Expect.Call(session.Close()).Return(null);
            }

            mocks.ReplayAll();

            HibernateTransactionManager tm = new HibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            IList list = new ArrayList();
            list.Add("test");

            object result = tt.Execute(new ParticipatingTransactionWithCommitTxCallback(sessionFactory, list));
            Assert.IsTrue(result == list);

            mocks.VerifyAll();

        }

        [Test]
        public void ParticipatingTransactionWithRollback()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();
            IDbTransaction adoTransaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(sessionFactory.OpenSession()).Return(session);
                Expect.Call(session.Connection).Return(connection);
                Expect.Call(session.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session.IsOpen).Return(true);
                Expect.Call(session.FlushMode).Return(FlushMode.Auto);

                Expect.Call(adoTransaction.Connection).Return(connection);
                LastCall.On(adoTransaction).Repeat.Once();

                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();
                Expect.Call(session.Close()).Return(null);
            }
            mocks.ReplayAll();


            TestableHibernateTransactionManager tm = new TestableHibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;
            tm.StubbedTransactionThatReturnsExpectedConnection = adoTransaction;

            TransactionTemplate tt = new TransactionTemplate(tm);
            try
            {
                tt.Execute(new ParticipatingTransactionWithRollbackTxCallback(sessionFactory));
                Assert.Fail("Should have thrown exception");
            }
            catch (ArgumentException)
            {

            }

            mocks.VerifyAll();
        }

        [Test]
        public void ParticipatingTransactionWithRollbackOnly()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();
            IDbTransaction adoTransaction = mocks.StrictMock<IDbTransaction>();

            using (mocks.Ordered())
            {
                Expect.Call(sessionFactory.OpenSession()).Return(session);
                Expect.Call(session.Connection).Return(connection);
                Expect.Call(session.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session.IsOpen).Return(true);

                Expect.Call(adoTransaction.Connection).Return(connection);
                LastCall.On(adoTransaction).Repeat.Once();

                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();
                Expect.Call(session.Close()).Return(null);
            }
            mocks.ReplayAll();


            TestableHibernateTransactionManager tm = new TestableHibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;
            tm.StubbedTransactionThatReturnsExpectedConnection = adoTransaction;

            TransactionTemplate tt = new TransactionTemplate(tm);
            IList list = new ArrayList();
            list.Add("test");
            try
            {
                tt.Execute(new ParticipatingTransactionWithRollbackOnlyTxCallback(tt,sessionFactory,list));
                Assert.Fail("Should have thrown UnexpectedRollbackException");
            }
            catch (UnexpectedRollbackException)
            {

            }

            mocks.VerifyAll();
        }

        [Test]
        public void ParticipatingTransactionWithWithRequiresNew()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session1 = mocks.StrictMock<ISession>();
            ISession session2 = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();

            //using (mocks.Ordered())
            //{
                Expect.Call(sessionFactory.OpenSession()).Return(session1);
                Expect.Call(session1.Connection).Return(connection);
                Expect.Call(session1.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session1.IsOpen).Return(true);

                Expect.Call(sessionFactory.OpenSession()).Return(session2);
                Expect.Call(session2.Connection).Return(connection);
                Expect.Call(session2.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session2.IsOpen).Return(true);

                Expect.Call(session2.FlushMode).Return(FlushMode.Auto);
                session2.Flush();
                LastCall.On(session2).Repeat.Once();

                transaction.Commit();
                LastCall.On(transaction).Repeat.Twice();

                Expect.Call(session1.Close()).Return(null);
                Expect.Call(session2.Close()).Return(null);
            //}

            mocks.ReplayAll();

            HibernateTransactionManager tm = new HibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");

            tt.Execute(new ParticipatingTransactionWithWithRequiresNewTxCallback(tt, sessionFactory));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");
            
            mocks.VerifyAll();

        }

        [Test]
        public void ParticipatingTransactionWithWithNotSupported()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();

            //using (mocks.Ordered())
            //{
                Expect.Call(sessionFactory.OpenSession()).Return(session).Repeat.Twice();
                Expect.Call(session.Connection).Return(connection);


                Expect.Call(session.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session.IsOpen).Return(true);
                Expect.Call(session.FlushMode).Return(FlushMode.Auto).Repeat.Twice();
                session.Flush();
                LastCall.On(session).Repeat.Twice();
                transaction.Commit();
                LastCall.On(transaction).Repeat.Once();
                Expect.Call(session.Close()).Return(null).Repeat.Once();
            //}
            mocks.ReplayAll();


            HibernateTransactionManager tm = new HibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");

            tt.Execute(new ParticipatingTransactionWithWithNotSupportedTxCallback(tt, sessionFactory));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");

            mocks.VerifyAll();


        }

        [Test]
        public void TransactionWithPropagationSupports()
        {
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();

            Expect.Call(sessionFactory.OpenSession()).Return(session);
            Expect.Call(session.FlushMode).Return(FlushMode.Never);
            session.FlushMode = FlushMode.Auto;
            LastCall.IgnoreArguments();
            session.Flush();
            LastCall.IgnoreArguments();
            session.FlushMode = FlushMode.Never;
            Expect.Call(session.FlushMode).Return(FlushMode.Never);

            mocks.ReplayAll();


            LocalSessionFactoryObjectStub lsfo = new LocalSessionFactoryObjectStub(sessionFactory);
            lsfo.AfterPropertiesSet();
            ISessionFactory sfProxy = (ISessionFactory) lsfo.GetObject();
            Assert.IsNotNull(sfProxy);

            HibernateTransactionManager tm = new HibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Supports;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");

            tt.Execute(new TransactionWithPropagationSupportsTxCallback(sessionFactory));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");

            mocks.VerifyAll();

        }

        [Test]
        public void TransactionWithPropagationSupportsAndInnerTransaction()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session1 = mocks.StrictMock<ISession>();
            ISession session2 = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();

            Expect.Call(sessionFactory.OpenSession()).Return(session1);
            Expect.Call(session1.Connection).Return(connection);
            Expect.Call(session1.SessionFactory).Return(sessionFactory);
            Expect.Call(session1.FlushMode).Return(FlushMode.Auto).Repeat.Twice();

            session1.Flush();
            LastCall.IgnoreArguments().Repeat.Twice();
            Expect.Call(session1.Close()).Return(null);

            Expect.Call(sessionFactory.OpenSession()).Return(session2);
            Expect.Call(session2.Connection).Return(connection).Repeat.Twice();
            Expect.Call(session2.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
            Expect.Call(session2.FlushMode).Return(FlushMode.Auto);
            session2.Flush();
            LastCall.IgnoreArguments();
            Expect.Call(session2.IsOpen).Return(true);


            transaction.Commit();
            LastCall.On(transaction).Repeat.Once();


            Expect.Call(session2.Close()).Return(null);

            mocks.ReplayAll();

            LocalSessionFactoryObjectStub lsfo = new LocalSessionFactoryObjectStub(sessionFactory);
            lsfo.AfterPropertiesSet();
            ISessionFactory sfProxy = (ISessionFactory)lsfo.GetObject();
            Assert.IsNotNull(sfProxy);

            HibernateTransactionManager tm = new HibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;
            
            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.Supports;
            TransactionTemplate tt2 = new TransactionTemplate(tm);
            tt2.PropagationBehavior = TransactionPropagation.Required;

            HibernateTemplate ht = new HibernateTemplate(sessionFactory);
            ht.TemplateFlushMode = TemplateFlushMode.Eager;
            ht.ExposeNativeSession = true;

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");

            tt.Execute(new TransactionWithPropagationSupportsAndInnerTransactionTxCallback(tt2, sessionFactory, ht, session1, session2));

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");
            
            mocks.ReplayAll();
            
        }


        [Test]
        public void TransactionCommitWithFlushFailure()
        {
            DoTransactionCommitWithFlushFailure(false);
        }

        [Test]
        public void TransactionCommitWithFlushFailureAndFallbackTranslation()
        {
            DoTransactionCommitWithFlushFailure(true);
        }

        /// <summary>
        /// Does the test transaction commit with flush failure.
        /// </summary>
        /// <param name="fallbackTranslation">if set to <c>true</c> if the exception throw
        /// is of the type NHibernate.ADOException, in which case HibernateTransactionManager
        /// will 'fallback' to using the error codes in the underlying exception thrown by
        /// the provider, ie. a SqlException, MySqlException.  Otherwise, if it is 
        /// another subclass of HibernateException, then perform a direct maping as 
        /// found in SessionFactoryUtils.ConvertHibernateAccessException.</param>
        private void DoTransactionCommitWithFlushFailure(bool fallbackTranslation)
        {
            #region Mock Setup

            IDbProvider provider = new TestDbProvider();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            ISessionFactory sessionFactory = mocks.StrictMock<ISessionFactory>();
            ISession session = mocks.StrictMock<ISession>();
            ITransaction transaction = mocks.StrictMock<ITransaction>();
            IDbTransaction adoTransaction = mocks.StrictMock<IDbTransaction>();
            
            Exception rootCause = null;
            using (mocks.Ordered())
            {
                Expect.Call(sessionFactory.OpenSession()).Return(session);
                Expect.Call(session.Connection).Return(connection);
                Expect.Call(session.BeginTransaction(IsolationLevel.ReadCommitted)).Return(transaction);
                Expect.Call(session.IsOpen).Return(true);
                transaction.Commit();
                Exception sqlException = new TestSqlException("mymsg", "2627");
                if (fallbackTranslation)
                {
                    //error code 2627 will map to a DataAccessIntegrity exception in sqlserver, which is the metadata
                    //used by TestDbProvider.
                    rootCause = sqlException;
                    LastCall.On(transaction).Throw(new ADOException("mymsg", sqlException));
                }
                else
                {
                    rootCause = new PropertyValueException("mymsg", typeof(string).Name, "Name");
                    LastCall.On(transaction).Throw(rootCause);
                }

                Expect.Call(adoTransaction.Connection).Return(connection);
                LastCall.On(adoTransaction).Repeat.Once();

                transaction.Rollback();
                LastCall.On(transaction).Repeat.Once();
                Expect.Call(session.Close()).Return(null);
            }

            #endregion

            mocks.ReplayAll();


            TestableHibernateTransactionManager tm = new TestableHibernateTransactionManager(sessionFactory);
            tm.TransactionSynchronization = TransactionSynchronizationState.Always;
            tm.DbProvider = provider;
            tm.StubbedTransactionThatReturnsExpectedConnection = adoTransaction;

            TransactionTemplate tt = new TransactionTemplate(tm);
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            IList list = new ArrayList();
            list.Add("test");
            try
            {
                tt.Execute(new TransactionCommitWithFlushFailureCallback(sessionFactory, list));
                Assert.Fail("Should have thrown DataIntegrityViolationException");
            }
            catch (DataIntegrityViolationException ex)
            {
                Assert.AreEqual(rootCause, ex.InnerException);
                Assert.IsTrue(ex.Message.IndexOf("mymsg") != -1);
            }

            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sessionFactory), "Hasn't thread session");
            Assert.IsTrue(!TransactionSynchronizationManager.SynchronizationActive, "Synchronizations not active");

            mocks.VerifyAll();


        }

    }



    #region Supporting classes for test TransactionCommit

    public class TransactionCommitTxCallback : ITransactionCallback
    {
        private ISessionFactory sfProxy;
        private IDbProvider provider;
        public TransactionCommitTxCallback(ISessionFactory sessionFactory, IDbProvider provider)
        {
            sfProxy = sessionFactory;
            this.provider = provider;
        }
        

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sfProxy),"Has thread session");
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(provider), "Hasn't thread db provider");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            HibernateTemplate ht = new HibernateTemplate(sfProxy);
            return ht.Find("some query string");
        }
    }

    public class LocalSessionFactoryObjectStub  : LocalSessionFactoryObject
    {
        private ISessionFactory sf;
        public LocalSessionFactoryObjectStub(ISessionFactory sf)
        {
            this.sf = sf;
        }

        protected override ISessionFactory NewSessionFactory(Configuration config)
        {
            return sf;
        }
    }

    #endregion

    #region Supporting classes for test TransactionRollback

    public class TransactionRollbackTxCallback : ITransactionCallback
    {
        private ISessionFactory sf;
        public TransactionRollbackTxCallback(ISessionFactory sf)
        {
            this.sf = sf;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sf),"Has thread session");
            HibernateTemplate ht = new HibernateTemplate(sf);
            return ht.ExecuteFind(new ThrowExceptionHibernateCallback());
        }
    }

    public class ThrowExceptionHibernateCallback : IHibernateCallback
    {
        public object DoInHibernate(ISession session)
        {
            throw new ArgumentException("arg exception");
        }
    }

    #endregion

    #region Supporting classes for test TransactionRollbackOnly

    public class TransactionRollbackOnlyTxCallback : ITransactionCallback
    {
        private ISessionFactory sf;
        public TransactionRollbackOnlyTxCallback(ISessionFactory sf)
        {
            this.sf = sf;
        }


        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sf), "Has thread session");
            HibernateTemplate ht = new HibernateTemplate(sf);
            ht.TemplateFlushMode = TemplateFlushMode.Eager;
            ht.Execute(new HibernateDelegate(Del));
            status.SetRollbackOnly();
            return null;
        }

        private object Del(ISession session)
        {
            return null;
        }
    }
    #endregion

    #region Supporting classes for test ParticipatingTransactionWithCommit

    public class ParticipatingTransactionWithCommitTxCallback : ITransactionCallback
    {
        private ISessionFactory sf;
        private IList list;
        public ParticipatingTransactionWithCommitTxCallback(ISessionFactory sf, IList list)
        {
            this.sf = sf;
            this.list = list;
        }


        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sf), "Has thread session");
            HibernateTemplate ht = new HibernateTemplate(sf);
            ht.TemplateFlushMode = TemplateFlushMode.Eager;
            return ht.Execute(new HibernateDelegate(Del));
        }

        private object Del(ISession session)
        {
            return list;
        }
    }


    #endregion

    #region Supporting classes for test ParticipatingTransactionWithRollback
    public class ParticipatingTransactionWithRollbackTxCallback : ITransactionCallback
    {
        private ISessionFactory sf;
        public ParticipatingTransactionWithRollbackTxCallback(ISessionFactory sf)
        {
            this.sf = sf;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sf), "Has thread session");
            HibernateTemplate ht = new HibernateTemplate(sf);
            ht.TemplateFlushMode = TemplateFlushMode.Eager;
            return ht.ExecuteFind(new ThrowExceptionHibernateCallback());
        }

    }

    #endregion

    #region Supporting classes for test ParticipatingTransactionWithRollbackOnly

    public class ParticipatingTransactionWithRollbackOnlyTxCallback : ITransactionCallback
    {
        private TransactionTemplate tt;
        private ISessionFactory sf;
        private IList list;
        public ParticipatingTransactionWithRollbackOnlyTxCallback(TransactionTemplate tt, ISessionFactory sf, IList list)
        {
            this.tt = tt;
            this.sf = sf;
            this.list = list;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            return tt.Execute(new TransactionDelegate(TransactionMethod));
        }

        private object TransactionMethod(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sf), "Has thread session");
            HibernateTemplate ht = new HibernateTemplate(sf);
            object returnValue = ht.Execute(new HibernateDelegate(Del));
            status.SetRollbackOnly();
            return null;
        }

        private object Del(ISession session)
        {
            return list;
        }

    }
    #endregion

    #region Supporting classes for test ParticipatingTransactionWithWithRequiresNew

    public class ParticipatingTransactionWithWithRequiresNewTxCallback : ITransactionCallback
    {
        private TransactionTemplate tt;
        private ISessionFactory sf;

        public ParticipatingTransactionWithWithRequiresNewTxCallback(TransactionTemplate tt, ISessionFactory sf)
        {
            this.tt = tt;
            this.sf = sf;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            SessionHolder holder = (SessionHolder) TransactionSynchronizationManager.GetResource(sf);
            Assert.IsNotNull(holder,"Has thread session");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);

            tt.Execute(new RequiresNewTxCallback(sf, holder));

            Assert.IsTrue(holder.Session == SessionFactoryUtils.GetSession(sf, false),"Same thread session as before");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            return null;
        }


    }

    public class RequiresNewTxCallback : ITransactionCallback
    {
        private SessionHolder holder;
        private ISessionFactory sf;

        public RequiresNewTxCallback(ISessionFactory sf, SessionHolder holder)
        {
            this.holder = holder;
            this.sf = sf;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            HibernateTemplate ht = new HibernateTemplate(sf);
            ht.TemplateFlushMode = TemplateFlushMode.Eager;
            return ht.ExecuteFind(new RequiresNewTxCallbackInner(holder));
        }
    }

    public class RequiresNewTxCallbackInner : IHibernateCallback
    {
        private SessionHolder holder;

        public RequiresNewTxCallbackInner(SessionHolder holder)
        {
            this.holder = holder;
        }

        public object DoInHibernate(ISession session)
        {
            Assert.IsTrue(session != holder.Session,"Not enclosing session");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            return null;
        }
    }

    #endregion


    #region Supporting classes for test ParticipatingTransactionWithWithNotSupported

    public class ParticipatingTransactionWithWithNotSupportedTxCallback : ITransactionCallback
    {
        private TransactionTemplate tt;
        private ISessionFactory sf;

        public ParticipatingTransactionWithWithNotSupportedTxCallback(TransactionTemplate tt, ISessionFactory sf)
        {
            this.tt = tt;
            this.sf = sf;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            SessionHolder holder = (SessionHolder)TransactionSynchronizationManager.GetResource(sf);
            Assert.IsNotNull(holder, "Has thread session");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            tt.PropagationBehavior = TransactionPropagation.NotSupported;            
            tt.Execute(new NotSupportedTxCallback(sf));

            Assert.IsTrue(holder.Session == SessionFactoryUtils.GetSession(sf, false), "Same thread session as before");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
            return null;
        }


    }

    public class NotSupportedTxCallback : ITransactionCallback
    {
        private ISessionFactory sf;

        public NotSupportedTxCallback(ISessionFactory sf)
        {
            this.sf = sf;
        }

        public object DoInTransaction(ITransactionStatus status)
        {


            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sf), "Hasn't thread session");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);

            HibernateTemplate ht = new HibernateTemplate(sf);
            ht.TemplateFlushMode = TemplateFlushMode.Eager;
            return ht.ExecuteFind(new NotSupportedTxCallbackInner());
        }
    }

    public class NotSupportedTxCallbackInner : IHibernateCallback
    {

        public object DoInHibernate(ISession session)
        {
            return null;
        }
    }

    #endregion

    #region Supporting classes for test TransactionWithPropagationSupports

    public class TransactionWithPropagationSupportsTxCallback : ITransactionCallback
    {
        private ISessionFactory sf;
        public TransactionWithPropagationSupportsTxCallback(ISessionFactory sf)
        {

            this.sf = sf;

        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sf), "Hasn't thread session");
            Assert.IsTrue(!status.IsNewTransaction, "Is not new transaction");

            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);

            HibernateTemplate ht = new HibernateTemplate(sf);
            ht.TemplateFlushMode = TemplateFlushMode.Eager;
            object returnValue = ht.Execute(new HibernateDelegate(Del));
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sf), "Has thread session");
            return null;
        }

        private object Del(ISession session)
        {
            return null;
        }
    }

    #endregion


    #region Supporting classes for test ParticipatingTransactionWithWithNotSupported

    public class TransactionWithPropagationSupportsAndInnerTransactionTxCallback : ITransactionCallback
    {
        private TransactionTemplate tt;
        private ISessionFactory sf;
        private HibernateTemplate ht;
        private ISession session1;
        private ISession session2;

        public TransactionWithPropagationSupportsAndInnerTransactionTxCallback(TransactionTemplate tt, 
            ISessionFactory sf, HibernateTemplate ht, ISession session1, ISession session2)
        {
            this.tt = tt;
            this.sf = sf;
            this.ht = ht;
            this.session1 = session1;
            this.session2 = session2;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(!TransactionSynchronizationManager.HasResource(sf), "Hasn't thread session");
            Assert.IsTrue(!status.IsNewTransaction, "Is not new transaction");
            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);

            ht.Execute(new HibernateDelegate(HibernateDelegate));

            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sf), "Has thread session");

            tt.Execute(new PropagationSupportsTxCallback(ht, session2));

            Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
            Assert.IsFalse(TransactionSynchronizationManager.ActualTransactionActive);
  
            return null;
        }

        private object HibernateDelegate(ISession session)
        {
            Assert.AreSame(session1, session);
            return null;
        }

        public class PropagationSupportsTxCallback : ITransactionCallback
        {
            private HibernateTemplate ht;
            private ISession session2;

            public PropagationSupportsTxCallback(HibernateTemplate ht, ISession session2)
            {
                this.ht = ht;
                this.session2 = session2;
            }

            public object DoInTransaction(ITransactionStatus status)
            {
                return ht.Execute(new HibernateDelegate(HibernateDelegate));
            }

            private object HibernateDelegate(ISession session)
            {
                Assert.IsFalse(TransactionSynchronizationManager.CurrentTransactionReadOnly);
                Assert.IsTrue(TransactionSynchronizationManager.ActualTransactionActive);
                Assert.AreSame(session2, session);
                return null;
            }
        }


    }



    #endregion

    #region Supporting classes for DoTransactionCommitWithFlushFailure

    public class TransactionCommitWithFlushFailureCallback : ITransactionCallback
    {
        private ISessionFactory sessionFactory;
        private IList list;

        public TransactionCommitWithFlushFailureCallback(ISessionFactory sessionFactory, IList list)
        {
            this.sessionFactory = sessionFactory;
            this.list = list;
        }

        public object DoInTransaction(ITransactionStatus status)
        {
            Assert.IsTrue(TransactionSynchronizationManager.HasResource(sessionFactory), "Has thread session");
            HibernateTemplate ht = new HibernateTemplate(sessionFactory);
            return ht.ExecuteFind(new TransactionCommitWithFlushFailureHibernateCallback(list));
        }


    }

    public class TransactionCommitWithFlushFailureHibernateCallback : IHibernateCallback
    {
        private IList list;
        public TransactionCommitWithFlushFailureHibernateCallback(IList list)
        {
            this.list = list;
        }

        public object DoInHibernate(ISession session)
        {
            return list;
        }
    }

    #endregion

}