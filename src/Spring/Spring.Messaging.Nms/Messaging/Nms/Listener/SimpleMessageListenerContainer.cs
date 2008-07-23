#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

using System;
using Common.Logging;
using Spring.Collections;
using Spring.Messaging.Nms.Support;
using Apache.NMS;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Messaging.Nms.Listener
{
    /// <summary>
    /// Message listener container that uses the plain NMS client API's
    /// <see cref="IMessageConsumer.Listener"/> method to create concurrent
    /// MessageConsumers for the specified listeners.
    /// </summary>
    public class SimpleMessageListenerContainer : AbstractMessageListenerContainer, IExceptionListener
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(SimpleMessageListenerContainer));

        #endregion

        #region fields

        private bool pubSubNoLocal = false;

        private int concurrentConsumers = 1;

        private ISet sessions;

        private ISet consumers;

        private object consumersMonitor = new object();

        #endregion

        #region Properties

        public bool PubSubNoLocal
        {
            get { return pubSubNoLocal; }
            set { pubSubNoLocal = value; }
        }

        public int ConcurrentConsumers
        {
            set
            {
                AssertUtils.IsTrue(value > 0, "'ConcurrentConsumer' value must be at least 1 (one)");
                concurrentConsumers = value;
            }
        }

        /// <summary>
        /// Always use a shared NMS connection
        /// </summary>
        protected override bool SharedConnectionEnabled
        {
            get { return true; }
        }

        #endregion

        protected override void ValidateConfiguration()
        {
            base.ValidateConfiguration();
            if (SubscriptionDurable && concurrentConsumers !=1 )
            {
                throw new ArgumentException("Only 1 concurrent consumer supported for durable subscription");
            }
        }

        /// <summary>
        /// Creates the specified number of concurrent consumers,
        /// in the form of a JMS Session plus associated MessageConsumer
        /// </summary>
        /// <see cref="CreateListenerConsumer"/>
        protected override void DoInitialize()
        {
            EstablishSharedConnection();
            InitializeConsumers();
        }

        /// <summary>
        /// Re-initializes this container's NMS message consumers,
        /// if not initialized already.
        /// </summary>
        protected override void DoStart()
        {
            base.DoStart();
            InitializeConsumers();
        }

        /// <summary>
        /// Registers this listener container as NMS ExceptionListener on the shared connection.
        /// </summary>
        /// <param name="connection"></param>
        protected override void PrepareSharedConnection(IConnection connection)
        {
            base.PrepareSharedConnection(connection);
            connection.ExceptionListener += OnException;
        }

        public void OnException(Exception exception)
        {
            InvokeExceptionListener(exception);
            // now try to recover the shared Connection and all consumers...
            if (logger.IsInfoEnabled)
            {
                logger.Info("Trying to recover from NMS Connection exception: " + exception);
            }
            try
            {
                lock(consumersMonitor)
                {
                    sessions = null;
                    consumers = null;
                }
                RefreshSharedConnection();
                InitializeConsumers();
                logger.Info("Successfully refreshed NMS Connection");
            } catch (NMSException recoverEx)
            {
                logger.Debug("Failed to recover NMS Connection", recoverEx);
                logger.Error("Encountered non-recoverable NMSException", exception);
            }
        }

        /// <summary>
        /// Initialize the Sessions and MessageConsumers for this container.
        /// </summary>
        /// <exception cref="NMSException">in case of setup failure.</exception>
        protected virtual void InitializeConsumers()
        {
            // Register Sessions and MessageConsumers            
            lock (consumersMonitor)
            {
                if (this.consumers == null)
                {
                    logger.Debug("InitializingConsumers **********");
                    this.sessions = new HashedSet();
                    this.consumers = new HashedSet();
                    IConnection con = SharedConnection;
                    for (int i = 0; i < this.concurrentConsumers; i++)
                    {
                        ISession session = CreateSession(SharedConnection);
                        IMessageConsumer consumer = CreateListenerConsumer(session);
                        this.sessions.Add(session);
                        this.consumers.Add(consumer);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a MessageConsumer for the given Session,
        /// registering a MessageListener for the specified listener
        /// </summary>
        /// <param name="session">The session to work on.</param>
        /// <returns>the IMessageConsumer"/></returns>
        /// <exception cref="NMSException">if thrown by NMS methods</exception>
        private IMessageConsumer CreateListenerConsumer(ISession session)
        {
            IDestination destination = Destination;
            if (destination == null)
            {
                destination = ResolveDestinationName(session, DestinationName);
            }
            IMessageConsumer consumer = CreateConsumer(session, destination);
            
			
	        SimpleMessageListener listener = new SimpleMessageListener(this, session);
            consumer.Listener += new Apache.NMS.MessageListener(listener.OnMessage); 
            return consumer;
        }

        /// <summary>
        /// Close the message consumers and sessions.
        /// </summary>
        /// <throws>NMSException if destruction failed </throws>
        protected override void DoShutdown()
        {
            logger.Debug("Closing NMS MessageConsumers");
            foreach (IMessageConsumer messageConsumer in consumers)
            {
                NmsUtils.CloseMessageConsumer(messageConsumer);
            }
            logger.Debug("Closing NMS Sessions");
            foreach (ISession session in sessions)
            {
                NmsUtils.CloseSession(session);
            }
            consumers = null;
            sessions = null;
        }


        protected IMessageConsumer CreateConsumer(ISession session, IDestination destination)
        {
            // Only pass in the NoLocal flag in case of a Topic:
            // Some NMS providers, such as WebSphere MQ 6.0, throw IllegalStateException
            // in case of the NoLocal flag being specified for a Queue.
            if (PubSubDomain)
            {
                if (SubscriptionDurable && destination is ITopic)
                {
                    return session.CreateDurableConsumer(
                        (ITopic) destination, DurableSubscriptionName, MessageSelector, PubSubNoLocal);
                }
                else
                {
                    return session.CreateConsumer(destination, MessageSelector, PubSubNoLocal);
                }
            }
            else
            {
                return session.CreateConsumer(destination, MessageSelector);
            }
        }
    }

    internal class SimpleMessageListener : IMessageListener
    {
        private SimpleMessageListenerContainer container;
        private ISession session;

        public SimpleMessageListener(SimpleMessageListenerContainer container, ISession session)
        {
            this.container = container;
            this.session = session;
        }

        public void OnMessage(IMessage message)
        {
            bool exposeResource = container.ExposeListenerSession;
            if (exposeResource)
            {
                TransactionSynchronizationManager.BindResource(
                    container.ConnectionFactory, new LocallyExposedNmsResourceHolder(session));
            }
            try
            {
                container.ExecuteListener(session, message);
            } finally
            {
                if (exposeResource)
                {
                    TransactionSynchronizationManager.UnbindResource(container.ConnectionFactory);
                }
            }
        }
    }
}
