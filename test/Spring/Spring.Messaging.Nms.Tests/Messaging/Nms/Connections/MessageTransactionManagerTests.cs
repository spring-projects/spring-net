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
using Apache.NMS;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Messaging.Nms.Connections;
using Spring.Messaging.Nms.Core;
using Spring.Transaction;
using Spring.Transaction.Support;

#endregion

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class NmsTransactionManagerTests
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

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
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

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
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


            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
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

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.Execute(delegate(ITransactionStatus status)
                           {
                               nt.Execute(new AssertSessionCallback(session));
                               status.RollbackOnly = true;
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
            Expect.Call(connection.CreateSession(AcknowledgementMode.Transactional)).Return(session).Repeat.Once();
            Expect.Call(connection.CreateSession(AcknowledgementMode.AutoAcknowledge)).Return(session2).Repeat.Once();           
            
            session.Commit();
            LastCall.On(session).Repeat.Once();
            session.Close();
            LastCall.On(session).Repeat.Once();

            session2.Close();
            LastCall.On(session2).Repeat.Once();

            connection.Close();
            LastCall.On(connection).Repeat.Twice();

            mocks.ReplayAll();

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
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
            Expect.Call(connection.CreateSession(AcknowledgementMode.Transactional)).Return(session).Repeat.Once();
            Expect.Call(connection.CreateSession(AcknowledgementMode.Transactional)).Return(session2).Repeat.Once();

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

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);


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
            Expect.Call(connection.CreateSession(AcknowledgementMode.Transactional)).Return(session).Repeat.Once();
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
            Expect.Call(connection.CreateSession(AcknowledgementMode.Transactional)).Return(session).Repeat.Once();
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
            Assert.IsTrue(TransactionSynchronizationManager.ResourceDictionary.Count == 0);
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

        public object DoInNms(ISession session)
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

        public object DoInNms(ISession session)
        {
            Assert.IsTrue(this.session != session);
            return null;
        }

        #endregion
    }
}