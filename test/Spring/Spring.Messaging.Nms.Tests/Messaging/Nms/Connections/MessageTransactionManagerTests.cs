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

using Apache.NMS;

using FakeItEasy;

using NUnit.Framework;
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
        [Test]
        public void TransactionCommit()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();
            ISession session = A.Fake<ISession>();

            SetupCreateSession(connection, connectionFactory, session);

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));
            tm.Commit(ts);

            AssertCommitExpectations(connection, connectionFactory, session);
        }

        [Test]
        public void TransactionRollback()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();
            ISession session = A.Fake<ISession>();

            SetupCreateSession(connection, connectionFactory, session);

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));
            tm.Rollback(ts);

            AssertRollbackExpectations(connection, connectionFactory, session);
        }

        [Test]
        public void ParticipatingTransactionWithCommit()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();
            ISession session = A.Fake<ISession>();

            SetupCreateSession(connection, connectionFactory, session);

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.Execute(status =>
                           {
                               nt.Execute(new AssertSessionCallback(session));
                               return null;
                           });

            tm.Commit(ts);

            AssertCommitExpectations(connection, connectionFactory, session);
        }

        [Test]
        public void ParticipatingTransactionWithRollback()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();
            ISession session = A.Fake<ISession>();

            SetupCreateSession(connection, connectionFactory, session);

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.Execute(status =>
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

            AssertRollbackExpectations(connection, connectionFactory, session);
        }

        [Test]
        public void SuspendedTransaction()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();
            ISession session = A.Fake<ISession>();
            ISession session2 = A.Fake<ISession>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection).Twice();
            A.CallTo(() => connection.CreateSession(AcknowledgementMode.Transactional)).Returns(session).Once();
            A.CallTo(() => connection.CreateSession(AcknowledgementMode.AutoAcknowledge)).Returns(session2).Once();
            
            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);
            nt.Execute(new AssertSessionCallback(session));

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.NotSupported;
            tt.Execute(status =>
                           {
                               nt.Execute(new AssertNotSameSessionCallback(session));
                               return null;
                           });
            
            nt.Execute(new AssertSessionCallback(session));

            tm.Commit(ts);

            A.CallTo(() => session.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => session.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => session2.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedTwiceExactly();
        }

        [Test]
        public void TransactionSuspension()
        {
            IConnectionFactory connectionFactory = A.Fake<IConnectionFactory>();
            IConnection connection = A.Fake<IConnection>();
            ISession session = A.Fake<ISession>();
            ISession session2 = A.Fake<ISession>();
                                                                        
            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection).Twice();
            A.CallTo(() => connection.CreateSession(AcknowledgementMode.Transactional))
                .Returns(session).Once()
                .Then.Returns(session2).Once();

            NmsTransactionManager tm = new NmsTransactionManager(connectionFactory);
            ITransactionStatus ts = tm.GetTransaction(new DefaultTransactionDefinition());
            NmsTemplate nt = new NmsTemplate(connectionFactory);

            TransactionTemplate tt = new TransactionTemplate(tm);
            tt.PropagationBehavior = TransactionPropagation.RequiresNew;
            tt.Execute(status =>
                           {
                               nt.Execute(new AssertNotSameSessionCallback(session));
                               return null;
                           });

            nt.Execute(new AssertSessionCallback(session));

            tm.Commit(ts);

            A.CallTo(() => session.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => session2.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => session.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => session2.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedTwiceExactly();
        }

        private static void AssertRollbackExpectations(IConnection connection, IConnectionFactory connectionFactory, ISession session)
        {
            A.CallTo(() => session.Rollback()).MustHaveHappenedOnceExactly();
            A.CallTo(() => session.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedOnceExactly();
        }

        private static void SetupCreateSession(
            IConnection connection,
            IConnectionFactory connectionFactory,
            ISession session)
        {
            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection).Once();
            A.CallTo(() => connection.CreateSession(AcknowledgementMode.Transactional)).Returns(session).Once();
        }


        private static void AssertCommitExpectations(
            IConnection connection,
            IConnectionFactory connectionFactory,
            ISession session)
        {
            A.CallTo(() => session.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => session.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => connection.Close()).MustHaveHappenedOnceExactly();
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