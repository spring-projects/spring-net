#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using Common.Logging;
using Spring.Collections;
using Spring.Messaging.Nms.Core;
using Spring.Messaging.Nms.Support;
using Apache.NMS;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Messaging.Nms.Listener
{
    /// <summary>
    /// Message listener container that uses the plain NMS client API's
    /// MessageConsumer.Listener method to create concurrent
    /// MessageConsumers for the specified listeners.
    /// </summary>
    /// <author>Mark Pollack</author>
    public class SimpleMessageListenerContainer : AbstractMessageListenerContainer, IExceptionListener
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(SimpleMessageListenerContainer));

        #endregion

        #region fields

        /// <summary>
        /// The default recovery time interval between connection reconnection attempts
        /// </summary>
        public static string DEFAULT_RECOVERY_INTERVAL = "5s";

        /// <summary>
        /// The total time connection recovery will be attempted.
        /// </summary>
        public static string DEFAULT_MAX_RECOVERY_TIME = "10m";

        private bool pubSubNoLocal = false;

        private int concurrentConsumers = 1;

        private ISet sessions;

        private ISet consumers;

        private object consumersMonitor = new object();

        private TimeSpan recoveryInterval = new TimeSpan(0, 0, 0, 5, 0);

        private TimeSpan maxRecoveryTime = new TimeSpan(0, 0, 10, 0, 0);


        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to inhibit the delivery of messages published by its own connection.
        /// Default is "false".
        /// </summary>
        /// <value><c>true</c> if should inhibit the delivery of messages published by its own connection; otherwise, <c>false</c>.</value>
        public bool PubSubNoLocal
        {
            get { return pubSubNoLocal; }
            set { pubSubNoLocal = value; }
        }

        /// <summary>
	    /// Specify the number of concurrent consumers to create. Default is 1.
        /// </summary>
        /// <remarks>
	    /// Raising the number of concurrent consumers is recommendable in order
	    /// to scale the consumption of messages coming in from a queue. However,
	    /// note that any ordering guarantees are lost once multiple consumers are
	    /// registered. In general, stick with 1 consumer for low-volume queues.
	    /// <para>Do not raise the number of concurrent consumers for a topic.
	    /// This would lead to concurrent consumption of the same message,
	    /// which is hardly ever desirable.
        /// </para>
        /// </remarks>
        /// <value>The concurrent consumers.</value>
        public int ConcurrentConsumers
        {
            set
            {
                AssertUtils.IsTrue(value > 0, "'ConcurrentConsumer' value must be at least 1 (one)");
                concurrentConsumers = value;
            }
        }


        /// <summary>
        /// Sets the time interval between connection recovery attempts.  The default is 5 seconds.
        /// </summary>
        /// <value>The recovery interval.</value>
        public TimeSpan RecoveryInterval
        {
            set { recoveryInterval = value; }
        }


        /// <summary>
        /// Sets the max recovery time to try reconnection attempts.  The default is 10 minutes.
        /// </summary>
        /// <value>The max recovery time.</value>
        public TimeSpan MaxRecoveryTime
        {
            set { maxRecoveryTime = value; }
        }

        /// <summary>
        /// Always use a shared NMS connection
        /// </summary>
        protected override bool SharedConnectionEnabled
        {
            get { return true; }
        }

        #endregion

        /// <summary>
        /// Call base class for valdation and then check that if the subscription is durable that the number of
        /// concurrent consumers is equal to one.
        /// </summary>
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


        /// <summary>
        /// <see cref="IExceptionListener"/> implementation, invoked by the NMS provider in
	    /// case of connection failures. Re-initializes this listener container's
	    /// shared connection and its sessions and consumers.
        /// </summary>
        /// <param name="exception">The reported connection exception.</param>
        public void OnException(Exception exception)
        {
            // First invoke the user-specific ExceptionListener, if any.
            InvokeExceptionListener(exception);
            // now try to recover the shared Connection and all consumers...
            if (logger.IsInfoEnabled)
            {
                logger.Info("Trying to recover from NMS Connection exception: " + exception);
            }
            try
            {
                lock (consumersMonitor)
                {
                    sessions = null;
                    consumers = null;
                }
                RefreshConnectionUntilSuccessful();
                InitializeConsumers();
                logger.Info("Successfully refreshed NMS Connection");
            } catch (RecoveryTimeExceededException)
            {
                throw;
            } catch (Exception recoverEx)
            {
                logger.Debug("Failed to recover NMS Connection", recoverEx);
                logger.Error("Encountered non-recoverable Exception", exception);
                throw;
            }
        }

        /// <summary>
	    /// Refresh the underlying Connection, not returning before an attempt has been
	    /// successful. Called in case of a shared Connection as well as without shared
	    /// Connection, so either needs to operate on the shared Connection or on a
	    /// temporary Connection that just gets established for validation purposes.
	    /// </summary>
	    /// <remarks>
	    /// The default implementation retries until it successfully established a
	    /// Connection, for as long as this message listener container is active.
	    /// Applies the specified recovery interval between retries.
        /// </remarks>
        protected virtual void RefreshConnectionUntilSuccessful()
        {
            TimeSpan totalTryTime = new TimeSpan();
            while (IsRunning)
            {
                try
                {
                    RefreshSharedConnection();
                    break;
                } catch (Exception ex)
                {
                    if (logger.IsInfoEnabled)
                    {
                        logger.Info("Could not refresh Connection - retrying in " + recoveryInterval, ex);
                    }
                }

                if (totalTryTime > maxRecoveryTime)
                {
                    logger.Info("Could not refresh Connection after " + totalTryTime  + ".  Stopping reconnection attempts.");
                    throw new RecoveryTimeExceededException("Could not recover after " + totalTryTime);
                }

                DateTime startTime = DateTime.Now;
                SleepInBetweenRecoveryAttempts();
                TimeSpan sleepTimeSpan = DateTime.Now - startTime;
                totalTryTime += sleepTimeSpan;
            }
        }

        /// <summary>
        /// The amount of time to sleep in between recovery attempts.
        /// </summary>
        protected virtual void SleepInBetweenRecoveryAttempts()
        {
            Thread.Sleep(recoveryInterval);
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
        /// <returns>the MessageConsumer"/></returns>
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
            // put in explicit registration with 'new' for compilation on .NET 1.1
            consumer.Listener += listener.OnMessage;
            return consumer;
        }

        /// <summary>
        /// Close the message consumers and sessions.
        /// </summary>
        /// <throws>NMSException if destruction failed </throws>
        protected override void DoShutdown()
        {
            lock (consumersMonitor)
            {
                if (consumers != null)
                {
                    logger.Debug("Closing NMS MessageConsumers");
                    foreach (IMessageConsumer messageConsumer in consumers)
                    {
                        NmsUtils.CloseMessageConsumer(messageConsumer);
                    }
                }
                if (sessions != null)
                {
                    logger.Debug("Closing NMS Sessions");
                    foreach (ISession session in sessions)
                    {
                        NmsUtils.CloseSession(session);
                    }
                }
                consumers = null;
                sessions = null;
            }
        }


        /// <summary>
        /// Creates a MessageConsumer for the given Session and Destination.
        /// </summary>
        /// <param name="session">The session to create a MessageConsumer for.</param>
        /// <param name="destination">The destination to create a MessageConsumer for.</param>
        /// <returns>The new MessageConsumer</returns>
        protected virtual IMessageConsumer CreateConsumer(ISession session, IDestination destination)
        {
            // Only pass in the NoLocal flag in case of a Topic:
            // Some NMS providers, such as WebSphere MQ 6.0, throw IllegalStateException
            // in case of the NoLocal flag being specified for a Queue.

            if (PubSubDomain && destination is ITopic)
            {
                if (SubscriptionShared)
                {
                    if (SubscriptionDurable)
                    {
                        return session.CreateSharedDurableConsumer((ITopic) destination, SubscriptionName, MessageSelector);
                    }

                    return session.CreateSharedConsumer((ITopic) destination, SubscriptionName, MessageSelector);
                }

                if (SubscriptionDurable)
                {
                    return session.CreateDurableConsumer((ITopic) destination, SubscriptionName, MessageSelector, PubSubNoLocal);
                }

                return session.CreateConsumer(destination, MessageSelector, PubSubNoLocal);
            }

            return session.CreateConsumer(destination, MessageSelector);
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
