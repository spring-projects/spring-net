#region License

/*
 * Copyright � 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports


using NUnit.Framework;
using Rhino.Mocks;
using Spring.Messaging.Ems.Common;
using Spring.Messaging.Ems.Core;
using Spring.Transaction;
using Spring.Transaction.Support;
using TIBCO.EMS;

#endregion

namespace Spring.Messaging.Ems.Connections
{
    /// <summary>
    /// This class contains tests for EmsTransactionManager
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class EmsTransactionManagerTests
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
            IConnectionFactory connectionFactory = (IConnectionFactory) mocks.CreateMock(typeof (IConnectionFactory));
            IConnection connection = (IConnection) mocks.CreateMock(typeof (IConnection));
            ISession session = (ISession) mocks.CreateMock(typeof (ISession));

            using (mocks.Ordered())
            {
                SetupCommitExpectations(connection, connectionFactory, session);
            }

            mocks.ReplayAll();

            EmsTransactionManager tm = new EmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            EmsTemplate nt = new EmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));
            tm.Commit(ts);

            mocks.VerifyAll();
                
        }

        [Test]
        public void TransactionRollback()
        {
            IConnectionFactory connectionFactory = (IConnectionFactory) mocks.CreateMock(typeof (IConnectionFactory));
            IConnection connection = (IConnection) mocks.CreateMock(typeof (IConnection));
            ISession session = (ISession) mocks.CreateMock(typeof (ISession));

            SetupRollbackExpectations(connection, connectionFactory, session);

            mocks.ReplayAll();

            EmsTransactionManager tm = new EmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            EmsTemplate nt = new EmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));
            tm.Rollback(ts);

            mocks.VerifyAll();
        }

        /**
         * TODO because using anonymous delegates - refactor to support .net 1.1 later.
         */
#if NET_2_0
        [Test]
        public void ParticipatingTransactionWithCommit()
        {
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));
            ISession session = (ISession)mocks.CreateMock(typeof(ISession));

            using (mocks.Ordered())
            {
                SetupCommitExpectations(connection, connectionFactory, session);
            }

            mocks.ReplayAll();


            EmsTransactionManager tm = new EmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            EmsTemplate nt = new EmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.Execute(delegate(ITransactionStatus status)
                           {
                               nt.Execute(new AssertSessionCallback(session));
                               return null;
                           });

            tm.Commit(ts);

            mocks.VerifyAll();

        }

        [Test]
        public void ParticipatingTransactionWithRollback()
        {
            IConnectionFactory connectionFactory = (IConnectionFactory) mocks.CreateMock(typeof (IConnectionFactory));
            IConnection connection = (IConnection) mocks.CreateMock(typeof (IConnection));
            ISession session = (ISession) mocks.CreateMock(typeof (ISession));

            using (mocks.Ordered())
            {
                SetupRollbackExpectations(connection, connectionFactory, session);
            }

            mocks.ReplayAll();

            EmsTransactionManager tm = new EmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            EmsTemplate nt = new EmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.Execute(delegate(ITransactionStatus status)
                           {
                               nt.Execute(new AssertSessionCallback(session));
                               status.SetRollbackOnly();
                               return null;
                           });
            try
            {
                tm.Commit(ts);
                Assert.Fail("Should have thrown UnexpectedRollbackException");
            } catch (UnexpectedRollbackException)
            {
                
            }

            mocks.VerifyAll();
        }

        [Test]
        public void SuspendedTransaction()
        {
            IConnectionFactory connectionFactory = (IConnectionFactory) mocks.CreateMock(typeof (IConnectionFactory));
            IConnection connection = (IConnection) mocks.CreateMock(typeof (IConnection));
            ISession session = (ISession) mocks.CreateMock(typeof (ISession));
            ISession session2 = (ISession)mocks.CreateMock(typeof(ISession));

            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Twice();
            Expect.Call(connection.CreateSession(true, Session.SESSION_TRANSACTED)).Return(session).Repeat.Once();
            Expect.Call(connection.CreateSession(false, Session.AUTO_ACKNOWLEDGE)).Return(session2).Repeat.Once();           
            
            session.Commit();
            LastCall.On(session).Repeat.Once();
            session.Close();
            LastCall.On(session).Repeat.Once();

            session2.Close();
            LastCall.On(session2).Repeat.Once();

            connection.Close();
            LastCall.On(connection).Repeat.Twice();

            mocks.ReplayAll();

            EmsTransactionManager tm = new EmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            EmsTemplate nt = new EmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.NotSupported;
            tt.Execute(delegate(ITransactionStatus status)
                           {
                               nt.Execute(new AssertNotSameSessionCallback(session));
                               return null;
                           });
            
            nt.Execute(new AssertSessionCallback(session));

            tm.Commit(ts);

            mocks.VerifyAll();
            
        }

        [Test]
        public void TransactionSuspension()
        {
            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));
            ISession session = (ISession)mocks.CreateMock(typeof(ISession));
            ISession session2 = (ISession)mocks.CreateMock(typeof(ISession));


            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Twice();
            Expect.Call(connection.CreateSession(true, Session.SESSION_TRANSACTED)).Return(session).Repeat.Once();
            Expect.Call(connection.CreateSession(true, Session.SESSION_TRANSACTED)).Return(session2).Repeat.Once();

            session.Commit();
            LastCall.On(session).Repeat.Once();
            session2.Commit();
            LastCall.On(session2).Repeat.Once();

            session.Close();
            LastCall.On(session).Repeat.Once();
            session2.Close();
            LastCall.On(session2).Repeat.Once();

            connection.Close();
            LastCall.On(connection).Repeat.Twice();

            mocks.ReplayAll();

            EmsTransactionManager tm = new EmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            EmsTemplate nt = new EmsTemplate(connectionFactory);


            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.Execute(delegate(ITransactionStatus status)
                           {
                               nt.Execute(new AssertNotSameSessionCallback(session));
                               return null;
                           });

            nt.Execute(new AssertSessionCallback(session));

            tm.Commit(ts);

            mocks.VerifyAll();

        }
#endif

        private static void SetupRollbackExpectations(IConnection connection, IConnectionFactory connectionFactory, ISession session)
        {
            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();
            Expect.Call(connection.CreateSession(true, Session.SESSION_TRANSACTED)).Return(session).Repeat.Once();
            session.Rollback();
            LastCall.On(session).Repeat.Once();
            session.Close();
            LastCall.On(session).Repeat.Once();
            connection.Close();
            LastCall.On(connection).Repeat.Once();
        }

        private static void SetupCommitExpectations(IConnection connection, IConnectionFactory connectionFactory, ISession session)
        {
            Expect.Call(connectionFactory.CreateConnection()).Return(connection).Repeat.Once();
            Expect.Call(connection.CreateSession(true, Session.SESSION_TRANSACTED)).Return(session).Repeat.Once();
            session.Commit();
            LastCall.On(session).Repeat.Once();
            session.Close();
            LastCall.On(session).Repeat.Once();
            connection.Close();
            LastCall.On(connection).Repeat.Once();
        }

        [TearDown]
        public void TearDown()
        {
            Assert.AreEqual(0, TransactionSynchronizationManager.ResourceDictionary.Count);
            Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);            
        }

        
    }

    internal class AssertSessionCallback : ISessionCallback
    {
        private ISession session;
        public AssertSessionCallback(ISession session)
        {
            this.session = session;
        }

        #region ISessionCallback Members

        public object DoInEms(ISession session)
        {
            Assert.IsTrue(this.session == session);
            return null;
        }

        #endregion
    }

    internal class AssertNotSameSessionCallback : ISessionCallback
    {
        private ISession session;
        public AssertNotSameSessionCallback(ISession session)
        {
            this.session = session;
        }

        #region ISessionCallback Members

        public object DoInEms(ISession session)
        {
            Assert.IsTrue(this.session != session);
            return null;
        }

        #endregion
    }
}