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
using System.Collections;
using Apache.NMS;
using NUnit.Framework;
using Rhino.Mocks;
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
        private MockRepository mocks;
        private IDestinationResolver mockDestinationResolver;
        private IConnectionFactory mockConnectionFactory;
        private IConnection mockConnection;

        private ISession mockSession;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
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
            mockConnectionFactory = (IConnectionFactory) mocks.CreateMock(typeof (IConnectionFactory));
            mockConnection = (IConnection) mocks.CreateMock(typeof (IConnection));
            mockSession = (ISession) mocks.CreateMock(typeof (ISession));

            IQueue queue = (IQueue) mocks.CreateMock(typeof (IQueue));

            Expect.Call(mockConnectionFactory.CreateConnection()).Return(mockConnection).Repeat.Once();
            if (UseTransactedTemplate)
            {
                Expect.Call(mockConnection.CreateSession(AcknowledgementMode.Transactional)).Return(mockSession).Repeat.
                    Once();
            }
            else
            {
                Expect.Call(mockConnection.CreateSession(AcknowledgementMode.AutoAcknowledge)).Return(mockSession).
                    Repeat.
                    Once();
            }
            Expect.Call(mockSession.Transacted).Return(true);

            mockDestinationResolver =
                (IDestinationResolver) mocks.CreateMock(typeof (IDestinationResolver));
            mockDestinationResolver.ResolveDestinationName(mockSession, "testDestination", false);
            LastCall.Return(queue).Repeat.Any();
        }

        [Test]
        public void ProducerCallback()
        {
            NmsTemplate template = CreateTemplate();
            template.ConnectionFactory = mockConnectionFactory;

            IMessageProducer mockProducer = (IMessageProducer)mocks.CreateMock(typeof(IMessageProducer));
            Expect.Call(mockSession.CreateProducer(null)).Return(mockProducer);

            Expect.Call(mockProducer.Priority).Return(MsgPriority.Normal);
            CloseProducerSessionConnection(mockProducer);

            mocks.ReplayAll();

            MsgPriority priority = MsgPriority.Highest;
            template.Execute(delegate(ISession session, IMessageProducer producer)
            {
                bool b = session.Transacted;
                priority = producer.Priority;
                return null;

            });
            
            Assert.AreEqual(priority, MsgPriority.Normal);
            mocks.VerifyAll();

        }

        [Test]
        public void ProducerCallbackWithIdAndTimestampDisabled()
        {
            NmsTemplate template = CreateTemplate();
            template.ConnectionFactory = mockConnectionFactory;
            template.MessageIdEnabled = false;
            template.MessageTimestampEnabled = false;

            IMessageProducer mockProducer = (IMessageProducer) mocks.CreateMock(typeof (IMessageProducer));
            Expect.Call(mockSession.CreateProducer(null)).Return(mockProducer);

            mockProducer.DisableMessageID = true;
            LastCall.On(mockProducer).Repeat.Once();
            mockProducer.DisableMessageTimestamp = true;
            LastCall.On(mockProducer).Repeat.Once();

            Expect.Call(mockProducer.Priority).Return(MsgPriority.Normal);
            CloseProducerSessionConnection(mockProducer);

            mocks.ReplayAll();

            template.Execute(delegate(ISession session, IMessageProducer producer)
                                 {
                                     bool b = session.Transacted;
                                     MsgPriority priority = producer.Priority;
                                     return null;

                                 });

            mocks.VerifyAll();

        }

        private void CloseProducerSessionConnection(IMessageProducer mockProducer)
        {
            mockProducer.Close();
            LastCall.On(mockProducer).Repeat.Once();
            mockSession.Close();
            LastCall.On(mockSession).Repeat.Once();
            mockConnection.Close();
            LastCall.On(mockConnection).Repeat.Once();
        }

        [Test]
        public void SessionCallback()
        {
            NmsTemplate template = CreateTemplate();
            template.ConnectionFactory = mockConnectionFactory;
            mockSession.Close();
            LastCall.On(mockSession).Repeat.Once();
            mockConnection.Close();
            LastCall.On(mockConnection).Repeat.Once();

            mocks.ReplayAll();

            template.Execute(delegate(ISession session)
                                 {
                                     bool b = session.Transacted;
                                     return null;
                                 });
            mocks.VerifyAll();
        }

        [Test]
        public void SessionCallbackWithinSynchronizedTransaction()
        {
            SingleConnectionFactory scf = new SingleConnectionFactory(mockConnectionFactory);
            NmsTemplate template = CreateTemplate();
            template.ConnectionFactory = scf;

            mockConnection.Start();
            LastCall.On(mockConnection).Repeat.Times(2);
            // We're gonna call getTransacted 3 times, i.e. 2 more times.
            Expect.Call(mockSession.Transacted).Return(UseTransactedSession).Repeat.Twice();

            if (UseTransactedTemplate)
            {
                mockSession.Commit();
                LastCall.On(mockSession).Repeat.Once();
            }

            mockSession.Close();
            LastCall.On(mockSession).Repeat.Once();
            mockConnection.Stop();
            LastCall.On(mockConnection).Repeat.Once();
            mockConnection.Close();
            LastCall.On(mockConnection).Repeat.Once();

            mocks.ReplayAll();


            TransactionSynchronizationManager.InitSynchronization();

            try
            {
                template.Execute(delegate(ISession session)
                                     {
                                         bool b = session.Transacted;
                                         return null;
                                     });
                template.Execute(delegate(ISession session)
                                     {
                                         bool b = session.Transacted;
                                         return null;
                                     });

                Assert.AreSame(mockSession, ConnectionFactoryUtils.GetTransactionalSession(scf, null, false));
                Assert.AreSame(mockSession,
                               ConnectionFactoryUtils.GetTransactionalSession(scf, scf.CreateConnection(), false));

                //In Java this test was doing 'double-duty' and testing TransactionAwareConnectionFactoryProxy, which has
                //not been implemented in .NET

                template.Execute(delegate(ISession session)
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
            mocks.VerifyAll();
        }

 
    }
}