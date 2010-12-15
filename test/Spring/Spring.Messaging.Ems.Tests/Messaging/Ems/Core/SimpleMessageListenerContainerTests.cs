#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using NUnit.Framework;
using Rhino.Mocks;
using Spring.Messaging.Ems.Common;
using Spring.Messaging.Ems.Listener;
using Spring.Util;
using TIBCO.EMS;

#endregion

namespace Spring.Messaging.Ems.Core
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class SimpleMessageListenerContainerTests
    {
        private static string DESTINATION_NAME = "foo";

        private static Queue QUEUE_DESTINATION = new Queue("banjo");

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

            ISession session = (ISession) mocks.CreateMock(typeof (ISession));
            Expect.Call(session.CreateQueue(DESTINATION_NAME)).Return(QUEUE_DESTINATION);
            Expect.Call(session.CreateConsumer(QUEUE_DESTINATION, null)).Return(messageConsumer);
            // an exception is thrown, so the rollback logic is being applied here...
            Expect.Call(session.Transacted).Return(false);

            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));
            //connection.ExceptionListener += container.OnException;
            connection.ExceptionListener = container;
            Expect.Call(connection.CreateSession(false, container.SessionAcknowledgeMode)).Return(session);
            connection.Start();
            
            IConnectionFactory connectionFactory = (IConnectionFactory) mocks.CreateMock(typeof (IConnectionFactory));
            Expect.Call(connectionFactory.CreateConnection()).Return(connection);

            EMSException theException = new EMSException(EXCEPTION_MESSAGE);

            IExceptionListener exceptionListener = (IExceptionListener) mocks.CreateMock(typeof (IExceptionListener));
            exceptionListener.OnException(theException);

            //IMessage message = (IMessage) mocks.CreateMock(typeof (IMessage));
            TextMessage message = new TextMessage(null, "hello");
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

            ISession session = (ISession)mocks.CreateMock(typeof(ISession));
            Expect.Call(session.CreateQueue(DESTINATION_NAME)).Return(QUEUE_DESTINATION);
            Expect.Call(session.CreateConsumer(QUEUE_DESTINATION, null)).Return(messageConsumer);
            // an exception is thrown, so the rollback logic is being applied here...
            Expect.Call(session.Transacted).Return(false);

            IConnection connection = (IConnection)mocks.CreateMock(typeof(IConnection));
            //connection.ExceptionListener += container.OnException;
            connection.ExceptionListener = container;
            Expect.Call(connection.CreateSession(false, container.SessionAcknowledgeMode)).Return(session);
            connection.Start();

            IConnectionFactory connectionFactory = (IConnectionFactory)mocks.CreateMock(typeof(IConnectionFactory));
            Expect.Call(connectionFactory.CreateConnection()).Return(connection);

            IllegalStateException theException = new IllegalStateException(EXCEPTION_MESSAGE);

            IErrorHandler errorHandler = (IErrorHandler)mocks.CreateMock(typeof(IErrorHandler));
            errorHandler.HandleError(theException);

            //IMessage message = (IMessage) mocks.CreateMock(typeof (IMessage));
            TextMessage message = new TextMessage(null, "hello");


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
        private EMSException exception;
        public BadSessionAwareMessageListener(EMSException exception)
        {
            this.exception = exception;
        }

        public void OnMessage(Message message, ISession session)
        {
            throw exception;
        }
    }

    internal class SimpleMessageConsumer : IMessageConsumer
    {
        private IMessageListener messageListener;

        public void SendMessage(Message message)
        {
            messageListener.OnMessage(message);
        }

        public Message Receive()
        {
            throw new NotImplementedException();
        }

        public Message Receive(long timeout)
        {
            throw new NotImplementedException();
        }

        public Message ReceiveNoWait()
        {
            throw new NotImplementedException();
        }

        public MessageConsumer NativeMessageConsumer
        {
            get { throw new NotImplementedException(); }
        }

        public event EMSMessageHandler MessageHandler;

        public IMessageListener MessageListener
        {
            get { return messageListener; }
            set { messageListener = value; }
        }

        public string MessageSelector
        {
            get { throw new NotImplementedException(); }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }



    }
}