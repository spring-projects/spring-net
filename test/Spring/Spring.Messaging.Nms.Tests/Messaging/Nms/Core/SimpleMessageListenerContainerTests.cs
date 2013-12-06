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
using Apache.NMS;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Messaging.Nms.Listener;
using Spring.Util;

#endregion

namespace Spring.Messaging.Nms.Core
{
    /// <summary>
    /// This class contains tests for SimpleMessageListenerContainer
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class SimpleMessageListenerContainerTests
    {
        private static string DESTINATION_NAME = "foo";

        private static StubQueue QUEUE_DESTINATION = new StubQueue();

        private static string EXCEPTION_MESSAGE = "This.Is.It";

        private SimpleMessageListenerContainer container;

        private MockRepository mocks;


        [SetUp]
        public void Setup()
        {
           mocks = new MockRepository();
           container = new SimpleMessageListenerContainer();
        }

 

        [Test]
        public void RegisteredExceptionListenerIsInvokedOnException()
        {
            SimpleMessageConsumer messageConsumer = new SimpleMessageConsumer();

            ISession session = mocks.StrictMock<ISession>();
            Expect.Call(session.GetQueue(DESTINATION_NAME)).Return(QUEUE_DESTINATION);
            Expect.Call(session.CreateConsumer(QUEUE_DESTINATION, null)).Return(messageConsumer);
            // an exception is thrown, so the rollback logic is being applied here...
            Expect.Call(session.Transacted).Return(false);

            IConnection connection = mocks.StrictMock<IConnection>();
            connection.ExceptionListener += container.OnException;
            Expect.Call(connection.CreateSession(container.SessionAcknowledgeMode)).Return(session);
            connection.Start();
            
            IConnectionFactory connectionFactory = mocks.StrictMock<IConnectionFactory>();
            Expect.Call(connectionFactory.CreateConnection()).Return(connection);

            NMSException theException = new NMSException(EXCEPTION_MESSAGE);

            IExceptionListener exceptionListener = mocks.StrictMock<IExceptionListener>();
            exceptionListener.OnException(theException);

            IMessage message = mocks.StrictMock<IMessage>();

            mocks.ReplayAll();


            container.ConnectionFactory = connectionFactory;
            container.DestinationName = DESTINATION_NAME;
            container.MessageListener = new BadSessionAwareMessageListener(theException);
            container.ExceptionListener = exceptionListener;
            container.AfterPropertiesSet();

            // manually trigger an Exception with the above bad MessageListener...
            messageConsumer.SendMessage(message);
            mocks.VerifyAll();
        }

        [Test]
        public void RegisteredErrorHandlerIsInvokedOnException()
        {
            SimpleMessageConsumer messageConsumer = new SimpleMessageConsumer();

            ISession session = mocks.StrictMock<ISession>();
            Expect.Call(session.GetQueue(DESTINATION_NAME)).Return(QUEUE_DESTINATION);
            Expect.Call(session.CreateConsumer(QUEUE_DESTINATION, null)).Return(messageConsumer);
            // an exception is thrown, so the rollback logic is being applied here...
            Expect.Call(session.Transacted).Return(false);

            IConnection connection = mocks.StrictMock<IConnection>();
            connection.ExceptionListener += container.OnException;
            Expect.Call(connection.CreateSession(container.SessionAcknowledgeMode)).Return(session);
            connection.Start();

            IConnectionFactory connectionFactory = mocks.StrictMock<IConnectionFactory>();
            Expect.Call(connectionFactory.CreateConnection()).Return(connection);

            IllegalStateException theException = new IllegalStateException(EXCEPTION_MESSAGE);

            IErrorHandler errorHandler = mocks.StrictMock<IErrorHandler>();
            errorHandler.HandleError(theException);

            IMessage message = mocks.StrictMock<IMessage>();

            mocks.ReplayAll();


            container.ConnectionFactory = connectionFactory;
            container.DestinationName = DESTINATION_NAME;
            container.MessageListener = new BadSessionAwareMessageListener(theException);
            container.ErrorHandler = errorHandler;
            container.AfterPropertiesSet();

            // manually trigger an Exception with the above bad MessageListener...
            messageConsumer.SendMessage(message);

            mocks.VerifyAll();
        }
    }

    internal class BadSessionAwareMessageListener : ISessionAwareMessageListener
    {
        private NMSException exception;
        public BadSessionAwareMessageListener(NMSException exception)
        {
            this.exception = exception;
        }

        public void OnMessage(IMessage message, ISession session)
        {
            throw exception;
        }
    }

    internal class SimpleMessageConsumer : IMessageConsumer
    {
        public event MessageListener Listener;

        public void SendMessage(IMessage message)
        {
            Listener(message);
        }
        public IMessage Receive()
        {
            throw new NotImplementedException();
        }

        public IMessage Receive(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IMessage ReceiveNoWait()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


    }
}