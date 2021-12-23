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

using System.Collections;
using System.Threading.Tasks;
using Apache.NMS;
using FakeItEasy;
using NUnit.Framework;
using Spring.Messaging.Nms.Connections;
using Spring.Messaging.Nms.Support.Destinations;
using Spring.Transaction.Support;

#endregion

namespace Spring.Messaging.Nms.Core
{
    /// <summary>
    /// This class contains tests for NmsTemplate
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class MessageTemplateTests
    {
        private IDestinationResolver mockDestinationResolver;
        private IConnectionFactory mockConnectionFactory;
        private IConnection mockConnection;

        private ISession mockSession;

        [SetUp]
        public void Setup()
        {
            CreateMocks();
        }

        private NmsTemplate CreateTemplate()
        {
            NmsTemplate template = new NmsTemplate();
            template.DestinationResolver = mockDestinationResolver;
            template.SessionTransacted = UseTransactedTemplate;
            return template;
        }


        protected virtual bool UseTransactedSession
        {
            get { return false; }
        }

        protected virtual bool UseTransactedTemplate
        {
            get { return false; }
        }

        private void CreateMocks()
        {
            mockConnectionFactory = A.Fake<IConnectionFactory>();
            mockConnection = A.Fake<IConnection>();
            mockSession = A.Fake<ISession>();

            IQueue queue = A.Fake<IQueue>();

            A.CallTo(() => mockConnectionFactory.CreateConnection()).Returns(mockConnection).Once();
            A.CallTo(() => mockConnectionFactory.CreateConnectionAsync()).Returns( Task.FromResult(mockConnection)).Once();
            if (UseTransactedTemplate)
            {
                A.CallTo(() => mockConnection.CreateSession(AcknowledgementMode.Transactional)).Returns(mockSession).Once();
                A.CallTo(() => mockConnection.CreateSessionAsync(AcknowledgementMode.Transactional)).Returns( Task.FromResult(mockSession)).Once();
            }
            else
            {
                A.CallTo(() => mockConnection.CreateSession(AcknowledgementMode.AutoAcknowledge)).Returns(mockSession).Once();
                A.CallTo(() => mockConnection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge)).Returns( Task.FromResult(mockSession)).Once();
            }

            A.CallTo(() => mockSession.Transacted).Returns(true);

            mockDestinationResolver = A.Fake<IDestinationResolver>();
            A.CallTo(() => mockDestinationResolver.ResolveDestinationName(mockSession, "testDestination", false)).Returns(queue);
        }

        [Test]
        public void ProducerCallback()
        {
            NmsTemplate template = CreateTemplate();
            template.ConnectionFactory = mockConnectionFactory;

            IMessageProducer mockProducer = A.Fake<IMessageProducer>();
            A.CallTo(() => mockSession.CreateProducer(null)).Returns(mockProducer); 
            A.CallTo(() => mockProducer.Priority).Returns(MsgPriority.Normal);

            MsgPriority priority = MsgPriority.Highest;
            template.Execute((session, producer) =>
            {
                bool b = session.Transacted;
                priority = producer.Priority;
                return null;

            });

            Assert.AreEqual(priority, MsgPriority.Normal);
            AssertCloseProducerSessionConnection(mockProducer);
 }

        [Test]
        public void ProducerCallbackWithIdAndTimestampDisabled()
        {
            NmsTemplate template = CreateTemplate();
            template.ConnectionFactory = mockConnectionFactory;
            template.MessageIdEnabled = false;
            template.MessageTimestampEnabled = false;

            IMessageProducer mockProducer = A.Fake<IMessageProducer>();
            A.CallTo(() => mockSession.CreateProducer(null)).Returns(mockProducer);

            A.CallTo(() => mockProducer.Priority).Returns(MsgPriority.Normal);

            template.Execute((session, producer) =>
            {
                bool b = session.Transacted;
                MsgPriority priority = producer.Priority;
                return null;
            });

            AssertCloseProducerSessionConnection(mockProducer);
            A.CallToSet(() => mockProducer.DisableMessageID).WhenArgumentsMatch(x => x.Get<bool>(0) == true).MustHaveHappenedOnceExactly();
            A.CallToSet(() => mockProducer.DisableMessageTimestamp).WhenArgumentsMatch(x => x.Get<bool>(0) == true).MustHaveHappenedOnceExactly();
        }

        private void AssertCloseProducerSessionConnection(IMessageProducer mockProducer)
        {
            A.CallTo(() => mockProducer.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockSession.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockConnection.Close()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void SessionCallback()
        {
            NmsTemplate template = CreateTemplate();
            template.ConnectionFactory = mockConnectionFactory;

            template.Execute(session =>
                                 {
                                     bool b = session.Transacted;
                                     return null;
                                 });

            A.CallTo(() => mockSession.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockConnection.Close()).MustHaveHappenedOnceExactly();
        }

        [Ignore("TODO Fix / Investigate")]
        [Test]
        public void SessionCallbackWithinSynchronizedTransaction()
        {
            SingleConnectionFactory scf = new SingleConnectionFactory(mockConnectionFactory);
            NmsTemplate template = CreateTemplate();
            template.ConnectionFactory = scf;

            // We're gonna call getTransacted 3 times, i.e. 2 more times.
            A.CallTo(() => mockSession.Transacted).Returns(UseTransactedSession);

            TransactionSynchronizationManager.InitSynchronization();

            try
            {
                template.Execute(session =>
                                     {
                                         bool b = session.Transacted;
                                         return null;
                                     });
                template.Execute(session =>
                                     {
                                         bool b = session.Transacted;
                                         return null;
                                     });

                Assert.AreSame(mockSession, ConnectionFactoryUtils.GetTransactionalSession(scf, null, false));
                var session = ConnectionFactoryUtils.GetTransactionalSession(scf, scf.CreateConnection(), false);
                Assert.AreSame(mockSession,session);

                //In Java this test was doing 'double-duty' and testing TransactionAwareConnectionFactoryProxy, which has
                //not been implemented in .NET

                template.Execute(session =>
                                     {
                                         bool b = session.Transacted;
                                         return null;
                                     });

                IList synchs = TransactionSynchronizationManager.Synchronizations;
                Assert.AreEqual(1, synchs.Count);
                ITransactionSynchronization synch = (ITransactionSynchronization) synchs[0];
                synch.BeforeCommit(false);
                synch.BeforeCompletion();
                synch.AfterCommit();
                synch.AfterCompletion(TransactionSynchronizationStatus.Unknown);
            }
            finally
            {
                TransactionSynchronizationManager.ClearSynchronization();
                //Assert.IsTrue(TransactionSynchronizationManager.ResourceDictionary.Count == 0);
                //Assert.IsFalse(TransactionSynchronizationManager.SynchronizationActive);
                scf.Dispose();
            }
            Assert.IsTrue(TransactionSynchronizationManager.ResourceDictionary.Count == 0);

            A.CallTo(() => mockConnection.Start()).MustHaveHappenedTwiceExactly();

            if (UseTransactedTemplate)
            {
                A.CallTo(() => mockSession.Commit()).MustHaveHappenedOnceExactly();
            }

            A.CallTo(() => mockSession.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockConnection.Stop()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockConnection.Close()).MustHaveHappenedOnceExactly();
        }
    }
}