#region License

/*
 * Copyright � 2002-2010 the original author or authors.
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
using Spring.Messaging.Ems.Common;
using Spring.Messaging.Ems.Connections;
using Spring.Messaging.Ems.Support;
using Spring.Messaging.Ems.Support.Converter;
using Spring.Messaging.Ems.Support.Destinations;
using Spring.Transaction.Support;
using Spring.Util;
using Queue=TIBCO.EMS.Queue;

namespace Spring.Messaging.Ems.Core
{
    /// <summary> Helper class that simplifies EMS access code.</summary>
    /// <remarks>
    /// <para>If you want to use dynamic destination creation, you must specify
    /// the type of EMS destination to create, using the "pubSubDomain" property.
    /// For other operations, this is not necessary.
    /// Point-to-Point (Queues) is the default domain.</para>
    ///
    /// <para>Default settings for EMS Sessions is "auto-acknowledge".</para>
    ///
    /// <para>This template uses a DynamicDestinationResolver and a SimpleMessageConverter
    /// as default strategies for resolving a destination name or converting a message,
    /// respectively.</para>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class EmsTemplate : EmsDestinationAccessor, IEmsOperations
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(EmsTemplate));


        #endregion
        #region Fields

        /// <summary>
        /// Timeout value indicating that a receive operation should
	    /// check if a message is immediately available without blocking.
        /// </summary>
        public static readonly long DEFAULT_RECEIVE_TIMEOUT = -1;

        private EmsTemplateResourceFactory transactionalResourceFactory;

        private object defaultDestination;

        private IMessageConverter messageConverter;


        private bool messageIdEnabled = true;

        private bool messageTimestampEnabled = true;

        private bool pubSubNoLocal = false;

        private long receiveTimeout = DEFAULT_RECEIVE_TIMEOUT;

        private bool explicitQosEnabled = false;

        private int priority = Message.DEFAULT_PRIORITY;

        private int deliveryMode = Message.DEFAULT_DELIVERY_MODE;

        private long timeToLive = Message.DEFAULT_TIME_TO_LIVE;

        #endregion

        #region Constructor (s)

        /// <summary> Create a new EmsTemplate.</summary>
        /// <remarks>
        /// <para>Note: The ConnectionFactory has to be set before using the instance.
        /// This constructor can be used to prepare a EmsTemplate via an ObjectFactory,
        /// typically setting the ConnectionFactory.</para>
        /// </remarks>
        public EmsTemplate()
        {
            transactionalResourceFactory = new EmsTemplateResourceFactory(this);
            InitDefaultStrategies();
        }


        /// <summary> Create a new EmsTemplate, given a ConnectionFactory.</summary>
        /// <param name="connectionFactory">the ConnectionFactory to obtain Connections from
        /// </param>
        public EmsTemplate(IConnectionFactory connectionFactory)
            : this()
        {
            ConnectionFactory = connectionFactory;
            AfterPropertiesSet();
        }

        #endregion

        #region Methods

        /// <summary> Initialize the default implementations for the template's strategies:
        /// DynamicDestinationResolver and SimpleMessageConverter.
        /// </summary>
        protected virtual void InitDefaultStrategies()
        {
            MessageConverter = new SimpleMessageConverter();
        }

        private void CheckDefaultDestination()
        {
            if (defaultDestination == null)
            {
                throw new SystemException(
                    "No defaultDestination or defaultDestinationName specified. Check configuration of EmsTemplate.");
            }
        }


        private void CheckMessageConverter()
        {
            if (MessageConverter == null)
            {
                throw new SystemException("No messageConverter registered. Check configuration of EmsTemplate.");
            }
        }

        /// <summary> Execute the action specified by the given action object within a
        /// EMS Session.
        /// </summary>
        /// <remarks> Generalized version of <code>execute(SessionCallback)</code>,
        /// allowing the EMS Connection to be started on the fly.
        /// <p>Use <code>execute(SessionCallback)</code> for the general case.
        /// Starting the EMS Connection is just necessary for receiving messages,
        /// which is preferably achieved through the <code>receive</code> methods.</p>
        /// </remarks>
        /// <param name="action">callback object that exposes the session
        /// </param>
        /// <param name="startConnection">Start the connection before performing callback action.
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public virtual object Execute(ISessionCallback action, bool startConnection)
        {
            return Execute(action.DoInEms, startConnection);
        }


        /// <summary> Execute the action specified by the given action object within a
        /// EMS Session.
        /// </summary>
        /// <remarks> Generalized version of <code>execute(SessionCallback)</code>,
        /// allowing the EMS Connection to be started on the fly.
        /// <p>Use <code>execute(SessionCallback)</code> for the general case.
        /// Starting the EMS Connection is just necessary for receiving messages,
        /// which is preferably achieved through the <code>receive</code> methods.</p>
        /// </remarks>
        /// <param name="action">callback object that exposes the session
        /// </param>
        /// <param name="startConnection">Start the connection before performing callback action.
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public virtual object Execute(SessionDelegate action, bool startConnection)
        {
            AssertUtils.ArgumentNotNull(action, "Callback object must not be null");

            IConnection conToClose = null;
            ISession sessionToClose = null;
            // bool sessionInTLS = true;

            //NOTE: Not closing session or connection unless session is not returned from
            //      ConnectionFactoryUtils.DoGetTransactionalSession and CacheEmsResources is set to false
            try
            {
                ISession sessionToUse =
                    ConnectionFactoryUtils.DoGetTransactionalSession(ConnectionFactory, transactionalResourceFactory,
                                                                     startConnection);
                if (sessionToUse == null)
                {
                    //sessionInTLS = false;
                    conToClose = CreateConnection();
                    sessionToClose = CreateSession(conToClose);
                    if (startConnection)
                    {
                        conToClose.Start();
                    }
                    sessionToUse = sessionToClose;
                }
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Executing callback on EMS Session [" + sessionToUse + "]");
                }
                return action(sessionToUse);
            }
            finally
            {
                EmsUtils.CloseSession(sessionToClose);
                ConnectionFactoryUtils.ReleaseConnection(conToClose, ConnectionFactory, startConnection);
                /*
                if (!sessionInTLS && !CacheEmsResources)
                {
                    EmsUtils.CloseSession(session);
                    ConnectionFactoryUtils.ReleaseConnection(con, ConnectionFactory, startConnection);
                }*/
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default destination to be used on send/receive operations that do not
        /// have a destination parameter.
        /// </summary>
        /// <remarks>Alternatively, specify a "defaultDestinationName", to be
        /// dynamically resolved via the DestinationResolver.</remarks>
        /// <value>The default destination.</value>
        virtual public Destination DefaultDestination
        {
            get { return (defaultDestination as Destination); }

            set { defaultDestination = value; }
        }


        /// <summary>
        /// Gets or sets the name of the default destination name
        /// to be used on send/receive operations that
        /// do not have a destination parameter.
        /// </summary>
        /// <remarks>
        /// Alternatively, specify a EMS Destination object as "DefaultDestination"
        /// </remarks>
        /// <value>The name of the default destination.</value>
        virtual public string DefaultDestinationName
        {
            get { return (defaultDestination as string); }

            set { defaultDestination = value; }
        }

        /// <summary>
        /// Gets or sets the message converter for this template.
        /// </summary>
        /// <remarks>
        /// Used to resolve
        /// Object parameters to convertAndSend methods and Object results
        /// from receiveAndConvert methods.
        /// <p>The default converter is a SimpleMessageConverter, which is able
        /// to handle BytesMessages, TextMessages and ObjectMessages.</p>
        /// </remarks>
        /// <value>The message converter.</value>
        virtual public IMessageConverter MessageConverter
        {
            get { return messageConverter; }

            set { messageConverter = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Message Ids are enabled.
        /// </summary>
        /// <value><c>true</c> if message id enabled; otherwise, <c>false</c>.</value>
        virtual public bool MessageIdEnabled
        {
            get { return messageIdEnabled; }

            set { messageIdEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether message timestamps are enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [message timestamp enabled]; otherwise, <c>false</c>.
        /// </value>
        virtual public bool MessageTimestampEnabled
        {
            get { return messageTimestampEnabled; }

            set { messageTimestampEnabled = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to inhibit the delivery of messages published by its own connection.
        ///
        /// </summary>
        /// <value><c>true</c> if inhibit the delivery of messages published by its own connection; otherwise, <c>false</c>.</value>
        virtual public bool PubSubNoLocal
        {
            get { return pubSubNoLocal; }

            set { pubSubNoLocal = value; }
        }

        /// <summary>
        /// Gets or sets the receive timeout to use for recieve calls.
        /// </summary>
        /// <remarks>The default is -1, which means no timeout.</remarks>
        /// <value>The receive timeout.</value>
        virtual public long ReceiveTimeout
        {
            get { return receiveTimeout; }

            set { receiveTimeout = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use explicit Quality of Service values.
        /// </summary>
        /// <remarks>If "true", then the values of deliveryMode, priority, and timeToLive
        /// will be used when sending a message. Otherwise, the default values,
        /// that may be set administratively, will be used</remarks>
        /// <value><c>true</c> if use explicit QoS values; otherwise, <c>false</c>.</value>
        virtual public bool ExplicitQosEnabled
        {
            get { return explicitQosEnabled; }

            set { explicitQosEnabled = value; }
        }

        /// <summary>
        /// Sets a value indicating the delivery mode QOS
        /// </summary>
        /// <remarks>
        /// This will set the delivery to persistent, non-persistent, or reliable delivery.
        /// Default value is Message.DEFAULT_DELIVERY_MODE (aka TIBCO.EMS.DeliveryMode.PERSISTENT)
        /// </remarks>
        /// <value>Integer value representing the delivery mode [delivery persistent]; otherwise, <c>false</c>.</value>
        virtual public int DeliveryMode
        {
			get { return deliveryMode; }

            set { deliveryMode = value; }
        }

        /// <summary>
        /// Gets or sets the priority when sending.
        /// </summary>
        /// <remarks>Since a default value may be defined administratively,
        /// this is only used when "isExplicitQosEnabled" equals "true".</remarks>
        /// <value>The priority.</value>
        virtual public int Priority
        {
            get { return priority; }

            set { priority = value; }
        }

        /// <summary>
        /// Gets or sets the time to live when sending
        /// </summary>
        /// <remarks>Since a default value may be defined administratively,
        /// this is only used when "isExplicitQosEnabled" equals "true".</remarks>
        /// <value>The time to live.</value>
        virtual public long TimeToLive
        {
            get { return timeToLive; }

            set { timeToLive = value; }
        }

        /*
        /// <summary>
        /// Gets or sets a value indicating whether the EmsTemplate should itself
        /// be responsible for caching EMS Connection/Session/MessageProducer as compared to
        /// creating new instances per operation (unless such resources are already
        /// present in Thread-Local storage either due to the use of EmsTransactionMananger or
        /// SimpleMessageListenerContainer at an outer calling layer.
        /// </summary>
        /// <remarks>Connection/Session/MessageProducer are thread-safe classes in TIBCO EMS.</remarks>
        /// <value><c>true</c> to locally cache ems resources; otherwise, <c>false</c>.</value>
        virtual public bool CacheEmsResources
        {
            get { return cacheEmsResources; }
            set { cacheEmsResources = value; }
        }*/


        #endregion

        /// <summary>
        /// Extract the content from the given JMS message.
        /// </summary>
        /// <param name="message">The Message to convert (can be <code>null</code>).</param>
        /// <returns>The content of the message, or <code>null</code> if none</returns>
        protected virtual object DoConvertFromMessage(Message message)
        {
            if (message != null)
            {
                return MessageConverter.FromMessage(message);
            }
            return null;
        }

        #region EMS Factory Methods

        /// <summary> Fetch an appropriate Connection from the given EmsResourceHolder.
        /// </summary>
        /// <param name="holder">the EmsResourceHolder
        /// </param>
        /// <returns> an appropriate Connection fetched from the holder,
        /// or <code>null</code> if none found
        /// </returns>
        protected virtual IConnection GetConnection(EmsResourceHolder holder)
        {
            return holder.GetConnection();
        }

        /// <summary> Fetch an appropriate Session from the given EmsResourceHolder.
        /// </summary>
        /// <param name="holder">the EmsResourceHolder
        /// </param>
        /// <returns> an appropriate Session fetched from the holder,
        /// or <code>null</code> if none found
        /// </returns>
        protected virtual ISession GetSession(EmsResourceHolder holder)
        {
            return holder.GetSession();
        }

        /// <summary> Create a EMS MessageProducer for the given Session and Destination,
        /// configuring it to disable message ids and/or timestamps (if necessary).
        /// <p>Delegates to <code>doCreateProducer</code> for creation of the raw
        /// EMS MessageProducer</p>
        /// </summary>
        /// <param name="session">the EMS Session to create a MessageProducer for
        /// </param>
        /// <param name="destination">the EMS Destination to create a MessageProducer for
        /// </param>
        /// <returns> the new EMS MessageProducer
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        /// <seealso cref="DoCreateProducer">
        /// </seealso>
        /// <seealso cref="MessageIdEnabled">
        /// </seealso>
        /// <seealso cref="MessageTimestampEnabled">
        /// </seealso>
        protected virtual IMessageProducer CreateProducer(ISession session, Destination destination)
        {
            IMessageProducer producer = DoCreateProducer(session, destination);
            if (!MessageIdEnabled)
            {
                producer.DisableMessageID = true;
            }
            if (!MessageTimestampEnabled)
            {
                producer.DisableMessageTimestamp = true;
            }
            return producer;
        }


        /// <summary>
        /// Determines whether the given Session is locally transacted, that is, whether
        /// its transaction is managed by this template class's Session handling
        /// and not by an external transaction coordinator.
        /// </summary>
        /// <remarks>
        /// The Session's own transacted flag will already have been checked
        /// before. This method is about finding out whether the Session's transaction
        /// is local or externally coordinated.
        /// </remarks>
        /// <param name="session">The session to check.</param>
        /// <returns>
        /// 	<c>true</c> if the session is locally transacted; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsSessionLocallyTransacted(ISession session)
        {
            return SessionTransacted &&
                !ConnectionFactoryUtils.IsSessionTransactional(session, ConnectionFactory);
        }

        /// <summary> Create a raw EMS MessageProducer for the given Session and Destination.
        /// </summary>
        /// <remarks>If CacheJmsResource is true, then the producer
        /// will be created upon the first invocation and will retrun the same
        /// producer (per destination) on all subsequent calls.
        /// </remarks>
        /// <param name="session">the EMS Session to create a MessageProducer for
        /// </param>
        /// <param name="destination">the EMS Destination to create a MessageProducer for
        /// </param>
        /// <returns> the new EMS MessageProducer
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        protected virtual IMessageProducer DoCreateProducer(ISession session, Destination destination)
        {
            return session.CreateProducer(destination);
            /*
            if (CacheEmsResources)
            {
                if (destination == null)
                {
                    if (emsResources.UnspecifiedDestinationMessageProducer == null)
                    {
                        emsResources.UnspecifiedDestinationMessageProducer = session.CreateProducer(destination);
                    }
                    return emsResources.UnspecifiedDestinationMessageProducer;
                }
                IMessageProducer producer = (IMessageProducer)emsResources.Producers[destination];
                if (producer != null)
                {
                    #region Logging

                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Found cached MessageProducer for destination [" + destination + "]");
                    }

                    #endregion
                }
                else
                {
                    producer = session.CreateProducer(destination);
                    emsResources.Producers.Add(destination, producer);
                    #region Logging

                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Created cached MessageProducer for destination [" + destination + "]");
                    }

                    #endregion
                }
                return producer;
            }
            else
            {
                return session.CreateProducer(destination);
            }*/
        }

        /// <summary> Create a EMS MessageConsumer for the given Session and Destination.
        /// </summary>
        /// <param name="session">the EMS Session to create a MessageConsumer for
        /// </param>
        /// <param name="destination">the EMS Destination to create a MessageConsumer for
        /// </param>
        /// <param name="messageSelector">the message selector for this consumer (can be <code>null</code>)
        /// </param>
        /// <returns> the new EMS MessageConsumer
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        protected virtual IMessageConsumer CreateConsumer(ISession session, Destination destination,
                                                          string messageSelector)
        {
            // Only pass in the NoLocal flag in case of a Topic:
            // Some EMS providers, such as WebSphere MQ 6.0, throw IllegalStateException
            // in case of the NoLocal flag being specified for a Queue.
            if (PubSubDomain)
            {
                return session.CreateConsumer(destination, messageSelector, PubSubNoLocal);
            }
            else
            {
                return session.CreateConsumer(destination, messageSelector);
            }
        }
        /*
        /// <summary>Create a EMS Connection via this template's ConnectionFactory.
        /// </summary>
        /// <remarks>If CacheJmsResource is true, then the connection
        /// will be created upon the first invocation and will retrun the same
        /// connection on all subsequent calls.
        /// </remarks>
        /// <returns>A EMS Connection
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        protected override IConnection CreateConnection()
        {
            if (CacheEmsResources)
            {
                if (emsResources.Connection == null)
                {
                    emsResources.Connection = ConnectionFactory.CreateConnection();
                }
                return emsResources.Connection;

            }
            else
            {
                return ConnectionFactory.CreateConnection();
            }
        }*/
        /*
        /// <summary> Create a EMS Session for the given Connection.
        /// </summary>
        /// <remarks>If CacheJmsResource is true, then the session
        /// will be created upon the first invocation and will retrun the same
        /// session on all subsequent calls.
        /// </remarks>
        /// <param name="con">the EMS Connection to create a Session for
        /// </param>
        /// <returns> the new EMS Session
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        protected override ISession CreateSession(IConnection con)
        {
            if (CacheEmsResources)
            {
                if (emsResources.Session == null)
                {
                    emsResources.Session = emsResources.Connection.CreateSession(SessionTransacted, SessionAcknowledgeMode);
                }
                return emsResources.Session;
            }
            else
            {
                return con.CreateSession(SessionTransacted, SessionAcknowledgeMode);
            }
        }*/

        /// <summary>
        /// Send the given message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to send to.</param>
        /// <param name="messageCreatorDelegate">The message creator delegate callback to create a Message.</param>
        protected internal virtual void DoSend(ISession session, Destination destination, MessageCreatorDelegate messageCreatorDelegate)
        {
            AssertUtils.ArgumentNotNull(messageCreatorDelegate, "IMessageCreatorDelegate must not be null");
            DoSend(session, destination, null, messageCreatorDelegate);
        }

        /// <summary>
        /// Send the given message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to send to.</param>
        /// <param name="messageCreator">The message creator callback to create a Message.</param>
        protected internal virtual void DoSend(ISession session, Destination destination, IMessageCreator messageCreator)
        {
            AssertUtils.ArgumentNotNull(messageCreator, "IMessageCreator must not be null");
            DoSend(session, destination, messageCreator, null);
        }

        /// <summary> Send the given EMS message.</summary>
        /// <param name="session">the EMS Session to operate on
        /// </param>
        /// <param name="destination">the EMS Destination to send to
        /// </param>
        /// <param name="messageCreator">callback to create a EMS Message
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a EMS Message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        protected internal virtual void DoSend(ISession session, Destination destination, IMessageCreator messageCreator,
                                               MessageCreatorDelegate messageCreatorDelegate)
        {


            IMessageProducer producer = CreateProducer(session, destination);
            try
            {

                Message message;
                if (messageCreator != null)
                {
                    message = messageCreator.CreateMessage(session) ;
                }
                else {
                    message = messageCreatorDelegate(session);
                }
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Sending created message [" + message + "]");
                }
                DoSend(producer, message);

                // Check commit, avoid commit call is Session transaction is externally coordinated.
                if (session.Transacted && IsSessionLocallyTransacted(session))
                {
                    // Transacted session created by this template -> commit.
                    EmsUtils.CommitIfNecessary(session);
                }
            }
            finally
            {
                EmsUtils.CloseMessageProducer(producer);
            }
        }


        /// <summary> Actually send the given EMS message.</summary>
        /// <param name="producer">the EMS MessageProducer to send with
        /// </param>
        /// <param name="message">the EMS Message to send
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        protected virtual void DoSend(IMessageProducer producer, Message message)
        {
            if (ExplicitQosEnabled)
            {
                producer.Send(message, DeliveryMode, Priority, TimeToLive);
            }
            else
            {
                producer.Send(message);
            }
        }


        #endregion

        #region IEmsOperations Implementation

        /// <summary>
        /// Execute the action specified by the given action object within
        /// a EMS Session.
        /// </summary>
        /// <param name="del">delegate that exposes the session</param>
        /// <returns>
        /// the result object from working with the session
        /// </returns>
        /// <remarks>
        /// 	<para>Note that the value of PubSubDomain affects the behavior of this method.
        /// If PubSubDomain equals true, then a Session is passed to the callback.
        /// If false, then a Session is passed to the callback.</para>b
        /// </remarks>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object Execute(SessionDelegate del)
        {
            return Execute(new ExecuteSessionCallbackUsingDelegate(del));
        }

        /// <summary> Execute the action specified by the given action object within
        /// a EMS Session.
        /// <p>Note: The value of PubSubDomain affects the behavior of this method.
        /// If PubSubDomain equals true, then a Session is passed to the callback.
        /// If false, then a Session is passed to the callback.</p>
        /// </summary>
        /// <param name="action">callback object that exposes the session
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object Execute(ISessionCallback action)
        {
            return Execute(action, false);
        }

        /// <summary> Send a message to a EMS destination. The callback gives access to
        /// the EMS session and MessageProducer in order to do more complex
        /// send operations.
        /// </summary>
        /// <param name="action">callback object that exposes the session/producer pair
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object Execute(IProducerCallback action)
        {
            return Execute(new ProducerCreatorCallback(this, action));
        }

        /// <summary> Send a message to a EMS destination. The callback gives access to
        /// the EMS session and MessageProducer in order to do more complex
        /// send operations.
        /// </summary>
        /// <param name="del">delegate that exposes the session/producer pair
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object Execute(ProducerDelegate del)
        {
            return Execute(new ProducerCreatorCallback(this, del));
        }

        /// <summary> Send a message to the default destination.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void SendWithDelegate(MessageCreatorDelegate messageCreatorDelegate)
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                SendWithDelegate(DefaultDestination, messageCreatorDelegate);
            }
            else
            {
                SendWithDelegate(DefaultDestinationName, messageCreatorDelegate);
            }
        }

        /// <summary> Send a message to the specified destination.
        /// The MessageCreator callback creates the message given a Session.
        /// </summary>
        /// <param name="destination">the destination to send this message to
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void SendWithDelegate(Destination destination, MessageCreatorDelegate messageCreatorDelegate)
        {
            Execute(new SendDestinationCallback(this, destination, messageCreatorDelegate), false);
        }

        /// <summary> Send a message to the specified destination.
        /// The MessageCreator callback creates the message given a Session.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void SendWithDelegate(string destinationName, MessageCreatorDelegate messageCreatorDelegate)
        {
            Execute(new SendDestinationCallback(this, destinationName, messageCreatorDelegate), false);
        }

        /// <summary> Send a message to the default destination.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageCreator">callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void Send(IMessageCreator messageCreator)
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                Send(DefaultDestination, messageCreator);
            }
            else
            {
                Send(DefaultDestinationName, messageCreator);
            }
        }

        /// <summary> Send a message to the specified destination.
        /// The MessageCreator callback creates the message given a Session.
        /// </summary>
        /// <param name="destination">the destination to send this message to
        /// </param>
        /// <param name="messageCreator">callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void Send(Destination destination, IMessageCreator messageCreator)
        {

            Execute(new SendDestinationCallback(this, destination, messageCreator), false);
        }

        /// <summary> Send a message to the specified destination.
        /// The MessageCreator callback creates the message given a Session.
        /// </summary>
        /// <param name="destinationName">the destination to send this message to
        /// </param>
        /// <param name="messageCreator">callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void Send(string destinationName, IMessageCreator messageCreator)
        {
            Execute(new SendDestinationCallback(this, destinationName, messageCreator), false);
        }
        /// <summary> Send the given object to the default destination, converting the object
        /// to a EMS message with a configured IMessageConverter.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSend(object message)
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                ConvertAndSend(DefaultDestination, message);
            }
            else
            {
                ConvertAndSend(DefaultDestinationName, message);
            }
        }

        /// <summary> Send the given object to the specified destination, converting the object
        /// to a EMS message with a configured IMessageConverter.
        /// </summary>
        /// <param name="destination">the destination to send this message to
        /// </param>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSend(Destination destination, object message)
        {
            CheckMessageConverter();
            Send(destination, new SimpleMessageCreator(this, message));
        }

        /// <summary> Send the given object to the specified destination, converting the object
        /// to a EMS message with a configured IMessageConverter.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSend(string destinationName, object message)
        {
            Send(destinationName, new SimpleMessageCreator(this, message));
        }

        /// <summary> Send the given object to the default destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <param name="postProcessor">the callback to modify the message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSend(object message, IMessagePostProcessor postProcessor)
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                ConvertAndSend(DefaultDestination, message, postProcessor);
            }
            else
            {
                ConvertAndSend(DefaultDestinationName, message, postProcessor);
            }
        }

        /// <summary> Send the given object to the specified destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destination">the destination to send this message to
        /// </param>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <param name="postProcessor">the callback to modify the message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSend(Destination destination, object message, IMessagePostProcessor postProcessor)
        {
            CheckMessageConverter();
            Send(destination, new ConvertAndSendMessageCreator(this, message, postProcessor));

        }

        /// <summary> Send the given object to the specified destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="message">the object to convert to a message.
        /// </param>
        /// <param name="postProcessor">the callback to modify the message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSend(string destinationName, object message, IMessagePostProcessor postProcessor)
        {
            CheckMessageConverter();
            Send(destinationName, new ConvertAndSendMessageCreator(this, message, postProcessor));
        }
        /// <summary>
        /// Send the given object to the default destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="message">the object to convert to a message</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSendWithDelegate(object message, MessagePostProcessorDelegate postProcessor)
        {
            //Execute(new SendDestinationCallback(this, destination, messageCreatorDelegate), false);
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                ConvertAndSendWithDelegate(DefaultDestination, message, postProcessor);
            }
            else
            {
                ConvertAndSendWithDelegate(DefaultDestinationName, message, postProcessor);
            }
        }

        /// <summary>
        /// Send the given object to the specified destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destination">the destination to send this message to</param>
        /// <param name="message">the object to convert to a message</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSendWithDelegate(Destination destination, object message,
                                               MessagePostProcessorDelegate postProcessor)
        {
            CheckMessageConverter();
            Send(destination, new ConvertAndSendMessageCreator(this, message, postProcessor));
        }

        /// <summary>
        /// Send the given object to the specified destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="message">the object to convert to a message.</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public void ConvertAndSendWithDelegate(string destinationName, object message,
                                               MessagePostProcessorDelegate postProcessor)
        {
            CheckMessageConverter();
            Send(destinationName, new ConvertAndSendMessageCreator(this, message, postProcessor));

        }

        /// <summary> Receive a message synchronously from the default destination, but only
        /// wait up to a specified time for delivery.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public Message Receive()
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                return Receive(DefaultDestination);
            }
            else
            {
                return Receive(DefaultDestinationName);
            }
        }

        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destination">the destination to receive a message from
        /// </param>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public Message Receive(Destination destination)
        {
            return Execute(new ReceiveCallback(this, destination)) as Message;
        }


        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public Message Receive(string destinationName)
        {
            return Execute(new ReceiveCallback(this, destinationName)) as Message;
        }

        /// <summary> Receive a message synchronously from the default destination, but only
        /// wait up to a specified time for delivery.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
        /// See the EMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public Message ReceiveSelected(string messageSelector)
        {
            CheckDefaultDestination();
            if (DefaultDestination!= null)
            {
                return ReceiveSelected(DefaultDestination, messageSelector);
            }
            else
            {
                return ReceiveSelected(DefaultDestinationName, messageSelector);
            }
        }

        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destination">the destination to receive a message from
        /// </param>
        /// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
        /// See the EMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public Message ReceiveSelected(Destination destination, string messageSelector)
        {
            return Execute(new ReceiveSelectedCallback(this, destination, messageSelector), true) as Message;
        }

        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
        /// See the EMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public Message ReceiveSelected(string destinationName, string messageSelector)
        {
            return Execute(new ReceiveSelectedCallback(this, destinationName, messageSelector), true) as Message;

        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to receive from.</param>
        /// <param name="messageSelector">The message selector for this consumer (can be <code>null</code></param>
        /// <returns>The Message received, or <code>null</code> if none.</returns>
        protected virtual Message DoReceive(ISession session, Destination destination, string messageSelector)
        {
            return DoReceive(session, CreateConsumer(session, destination, messageSelector));
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="consumer">The consumer to receive with.</param>
        /// <returns>The Message received, or <code>null</code> if none</returns>
        protected virtual Message DoReceive(ISession session, IMessageConsumer consumer)
        {
            try
            {
                long timeout = ReceiveTimeout;
                EmsResourceHolder resourceHolder =
                (EmsResourceHolder)TransactionSynchronizationManager.GetResource(ConnectionFactory);
                if (resourceHolder != null && resourceHolder.HasTimeout)
                {
                    timeout = Convert.ToInt64(resourceHolder.TimeToLiveInMilliseconds);
                }
                Message message = (timeout > 0)
                                      ? consumer.Receive(timeout)
                                      : consumer.Receive();
                if (session.Transacted)
                {
                    // Commit necessary - but avoid commit call is Session transaction is externally coordinated.
                    if (IsSessionLocallyTransacted(session))
                    {
                        // Transacted session created by this template -> commit.
                        EmsUtils.CommitIfNecessary(session);
                    }
                }
                else if (IsClientAcknowledge(session))
                {
                    // Manually acknowledge message, if any.
                    if (message != null)
                    {
                        message.Acknowledge();
                    }
                }
                return message;
            }
            finally
            {
                EmsUtils.CloseMessageConsumer(consumer);
            }
        }


        /// <summary> Receive a message synchronously from the default destination, but only
        /// wait up to a specified time for delivery. Convert the message into an
        /// object with a configured IMessageConverter.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object ReceiveAndConvert()
        {
            CheckMessageConverter();
            return DoConvertFromMessage(Receive());
        }

        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery. Convert the message into an
        /// object with a configured IMessageConverter.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destination">the destination to receive a message from
        /// </param>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object ReceiveAndConvert(Destination destination)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(Receive(destination));
        }


        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery. Convert the message into an
        /// object with a configured IMessageConverter.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object ReceiveAndConvert(string destinationName)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(Receive(destinationName));
        }

        /// <summary> Receive a message synchronously from the default destination, but only
        /// wait up to a specified time for delivery. Convert the message into an
        /// object with a configured IMessageConverter.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
        /// See the EMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object ReceiveSelectedAndConvert(string messageSelector)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(ReceiveSelected(messageSelector));
        }

        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery. Convert the message into an
        /// object with a configured IMessageConverter.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destination">the destination to receive a message from
        /// </param>
        /// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
        /// See the EMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object ReceiveSelectedAndConvert(Destination destination, string messageSelector)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(ReceiveSelected(destination, messageSelector));
        }

        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery. Convert the message into an
        /// object with a configured IMessageConverter.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
        /// See the EMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object ReceiveSelectedAndConvert(string destinationName, string messageSelector)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(ReceiveSelected(destinationName, messageSelector));
        }


        /// <summary>
        /// Browses messages in the default EMS queue. The callback gives access to the EMS
        /// Session and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <returns>the result object from working with the session</returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object Browse(IBrowserCallback action)
        {
            AssertUtils.ArgumentNotNull(action, "action");
            return BrowseWithDelegate(action.DoInEms);
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queue">The queue to browse.</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <returns>the result object from working with the session</returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object Browse(Queue queue, IBrowserCallback action)
        {
            AssertUtils.ArgumentNotNull(action, "action");
            return BrowseSelectedWithDelegate(queue, null, action.DoInEms);
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queueName">Name of the queue to browse,
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object Browse(string queueName, IBrowserCallback action)
        {
            AssertUtils.ArgumentNotNull(action, "action");
            return BrowseSelectedWithDelegate(queueName, null, action.DoInEms);
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object BrowseSelected(string messageSelector, IBrowserCallback action)
        {
            AssertUtils.ArgumentNotNull(action, "action");
            return BrowseSelectedWithDelegate(messageSelector, action.DoInEms);
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queue">The queue to browse.</param>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <returns></returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object BrowseSelected(Queue queue, string messageSelector, IBrowserCallback action)
        {
            AssertUtils.ArgumentNotNull(action, "action");
            return BrowseSelectedWithDelegate(queue, messageSelector, action.DoInEms);
        }


        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queueName">Name of the queue to browse,
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <returns></returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object BrowseSelected(string queueName, string messageSelector, IBrowserCallback action)
        {
            AssertUtils.ArgumentNotNull(action, "action");
            return BrowseSelectedWithDelegate(queueName, messageSelector, action.DoInEms);
        }



        /// <summary>
        /// Browses messages in the default EMS queue. The callback gives access to the EMS
        /// Session and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <returns>the result object from working with the session</returns>
        public object BrowseWithDelegate(BrowserDelegate action)
        {
            if (DefaultDestinationName != null)
            {
                return BrowseSelectedWithDelegate(DefaultDestinationName, action);
            }
            else
            {
                Destination destination = DefaultDestination;
                if (!(destination is Queue))
                {
                    throw new InvalidOperationException("defaultDestination does not correspond to a Queue. Check configuration of EmsTemplate.");
                }
                return BrowseWithDelegate((Queue)destination, action);
            }
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queue">The queue to browse.</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <returns>the result object from working with the session</returns>
        public object BrowseWithDelegate(Queue queue, BrowserDelegate action)
        {
            return BrowseSelectedWithDelegate(queue, null, action);
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queueName">Name of the queue to browse,
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object BrowseWithDelegate(string queueName, BrowserDelegate action)
        {
            return BrowseSelectedWithDelegate(queueName, null, action);
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object BrowseSelectedWithDelegate(string messageSelector, BrowserDelegate action)
        {
            if (DefaultDestinationName != null)
            {
                return BrowseSelectedWithDelegate(DefaultDestinationName, messageSelector, action);
            }
            else
            {
                Destination destination = DefaultDestination;
                if (!(destination is Queue))
                {
                    throw new InvalidOperationException("defaultDestination does not correspond to a Queue. Check configuration of EmsTemplate.");
                }
                return BrowseSelectedWithDelegate((Queue)destination, messageSelector, action);
            }
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queue">The queue to browse.</param>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <returns></returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object BrowseSelectedWithDelegate(Queue queue, string messageSelector, BrowserDelegate action)
        {
            AssertUtils.ArgumentNotNull(action, "action");
            return Execute(session =>
                               {
                                   QueueBrowser browser = CreateBrowser(session, queue, messageSelector);
                                   try
                                   {
                                       return action(session, browser);
                                   }
                                   finally
                                   {
                                       EmsUtils.CloseQueueBrowser(browser);
                                   }
                               }, true);
        }

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queueName">Name of the queue to browse,
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <returns></returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        public object BrowseSelectedWithDelegate(string queueName, string messageSelector, BrowserDelegate action)
        {
            AssertUtils.ArgumentNotNull(action, "action");
            return Execute(session =>
                               {
                                   Queue queue = (Queue)DestinationResolver.ResolveDestinationName(session, queueName, false);
                                   QueueBrowser browser = CreateBrowser(session, queue, messageSelector);
                                   try
                                   {
                                       return action(session, browser);
                                   }
                                   finally
                                   {
                                       EmsUtils.CloseQueueBrowser(browser);
                                   }
                               }, true);
        }

        #endregion

        /// <summary>
        /// Creates the queue browser.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="queue">The queue.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>A new queue browser</returns>
        protected virtual QueueBrowser CreateBrowser(ISession session, Queue queue, string selector)
        {
            return session.CreateBrowser(queue, selector);
        }


        #region Supporting Internal Classes

        /// <summary>
        /// ResourceFactory implementation that delegates to this template's callback methods.
        /// </summary>
        private class EmsTemplateResourceFactory : ConnectionFactoryUtils.ResourceFactory
        {
            private EmsTemplate enclosingTemplateInstance;

            public EmsTemplateResourceFactory(EmsTemplate enclosingInstance)
            {
                InitBlock(enclosingInstance);
            }

            private void InitBlock(EmsTemplate enclosingInstance)
            {
                enclosingTemplateInstance = enclosingInstance;
            }

            public EmsTemplate EnclosingInstance
            {
                get { return enclosingTemplateInstance; }
            }

            public virtual IConnection GetConnection(EmsResourceHolder holder)
            {
                return EnclosingInstance.GetConnection(holder);
            }

            public virtual ISession GetSession(EmsResourceHolder holder)
            {
                return EnclosingInstance.GetSession(holder);
            }

            public virtual IConnection CreateConnection()
            {
                return EnclosingInstance.CreateConnection();
            }

            public virtual ISession CreateSession(IConnection con)
            {
                return EnclosingInstance.CreateSession(con);
            }

            public bool SynchedLocalTransactionAllowed
            {
                get { return EnclosingInstance.SessionTransacted; }
            }
        }

        private class ProducerCreatorCallback : ISessionCallback
        {
            private EmsTemplate jmsTemplate;
            private IProducerCallback producerCallback;
            private ProducerDelegate producerDelegate;

            public ProducerCreatorCallback(EmsTemplate jmsTemplate, IProducerCallback producerCallback)
            {
                this.jmsTemplate = jmsTemplate;
				this.producerCallback = producerCallback;
            }

            public ProducerCreatorCallback(EmsTemplate jmsTemplate, ProducerDelegate producerDelegate)
            {
                this.jmsTemplate = jmsTemplate;
                this.producerDelegate = producerDelegate;
            }


            public object DoInEms(ISession session)
            {
                IMessageProducer producer = jmsTemplate.CreateProducer(session, null);
                try
                {
                    if (producerCallback != null)
                    {
                        return producerCallback.DoInEms(session, producer);
                    }
                    else
                    {
                        return producerDelegate(session, producer);
                    }
                }
                finally
                {
                    EmsUtils.CloseMessageProducer(producer);
                }

            }
        }

        private class ReceiveCallback : ISessionCallback
        {
            private EmsTemplate jmsTemplate;
            private Destination destination;
            private string destinationName;


            public ReceiveCallback(EmsTemplate jmsTemplate, string destinationName)
            {
                this.jmsTemplate = jmsTemplate;
                this.destinationName = destinationName;
            }

            public ReceiveCallback(EmsTemplate jmsTemplate, Destination destination)
            {
                this.jmsTemplate = jmsTemplate;
                this.destination = destination;
            }

            public object DoInEms(ISession session)
            {
                if (destination != null)
                {
                    return jmsTemplate.DoReceive(session, destination, null);
                }
                else
                {
                    return jmsTemplate.DoReceive(session,
                                                 jmsTemplate.ResolveDestinationName(session, destinationName),
                                                 null);
                }

            }
        }

        private class ConvertAndSendMessageCreator : IMessageCreator
        {
            private EmsTemplate jmsTemplate;
            private object objectToConvert;
            private IMessagePostProcessor messagePostProcessor;
            private MessagePostProcessorDelegate messagePostProcessorDelegate;

            public ConvertAndSendMessageCreator(EmsTemplate jmsTemplate, object message, IMessagePostProcessor messagePostProcessor)
            {
                this.jmsTemplate = jmsTemplate;
                objectToConvert = message;
                this.messagePostProcessor = messagePostProcessor;
            }

            public ConvertAndSendMessageCreator(EmsTemplate jmsTemplate, object message, MessagePostProcessorDelegate messagePostProcessorDelegate)
            {
                this.jmsTemplate = jmsTemplate;
                objectToConvert = message;
                this.messagePostProcessorDelegate = messagePostProcessorDelegate;
            }

            public Message CreateMessage(ISession session)
            {
                Message msg = jmsTemplate.MessageConverter.ToMessage(objectToConvert, session);
                if (messagePostProcessor != null)
                {
                    return messagePostProcessor.PostProcessMessage(msg);
                }
                else
                {
                    return messagePostProcessorDelegate(msg);
                }
            }
        }

        private class ReceiveSelectedCallback : ISessionCallback
        {
            private EmsTemplate jmsTemplate;
            private string messageSelector;
            private string destinationName;
            private Destination destination;

            public ReceiveSelectedCallback(EmsTemplate jmsTemplate,
                               Destination destination,
                               string messageSelector)
            {
                this.jmsTemplate = jmsTemplate;
                this.destination = destination;
                this.messageSelector = messageSelector;
            }
            public ReceiveSelectedCallback(EmsTemplate jmsTemplate,
                                           string destinationName,
                                           string messageSelector)
            {
                this.jmsTemplate = jmsTemplate;
                this.destinationName = destinationName;
                this.messageSelector = messageSelector;
            }

            public object DoInEms(ISession session)
            {
                if (destination != null)
                {
                    return jmsTemplate.DoReceive(session, destination, messageSelector);
                }
                else
                {
                    return jmsTemplate.DoReceive(session,
                                                 jmsTemplate.ResolveDestinationName(session, destinationName),
                                                 messageSelector);
                }

            }

        }

        private class ExecuteSessionCallbackUsingDelegate : ISessionCallback
        {
            private SessionDelegate del;
            public ExecuteSessionCallbackUsingDelegate(SessionDelegate del)
            {
                this.del = del;
            }

            public object DoInEms(ISession session)
            {
                return del(session);
            }
        }

        #endregion
    }

    /// <summary>
    /// This is a TIBCO specific class so that we can reuse connections, session, and
    /// message producers instead of creating/destroying them on each operation.
    /// </summary>
/*    internal class EmsResources
    {
        private IConnection connection;
        private ISession session;

        private IDictionary cachedProducers = new Hashtable();
        private IMessageProducer cachedUnspecifiedDestinationMessageProducer;

        public IConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }

        public ISession Session
        {
            get { return session; }
            set { session = value; }
        }

        public IMessageProducer UnspecifiedDestinationMessageProducer
        {
            get { return cachedUnspecifiedDestinationMessageProducer; }
            set { cachedUnspecifiedDestinationMessageProducer = value; }
        }


        public IDictionary Producers
        {
            get { return cachedProducers; }
            set { cachedProducers = value; }
        }
    }*/


    internal class SimpleMessageCreator : IMessageCreator
    {
        private EmsTemplate jmsTemplate;
        private object objectToConvert;

        public SimpleMessageCreator(EmsTemplate jmsTemplate, object objectToConvert)
        {
            this.jmsTemplate = jmsTemplate;
            this.objectToConvert = objectToConvert;
        }

        public Message CreateMessage(ISession session)
        {
            return jmsTemplate.MessageConverter.ToMessage(objectToConvert, session);
        }


    }



    internal class SendDestinationCallback : ISessionCallback
    {
        private string destinationName;
        private Destination destination;
        private EmsTemplate jmsTemplate;
        private IMessageCreator messageCreator;
        private MessageCreatorDelegate messageCreatorDelegate;

        public SendDestinationCallback(EmsTemplate jmsTemplate, string destinationName, IMessageCreator messageCreator)
        {
            this.jmsTemplate = jmsTemplate;
            this.destinationName = destinationName;
            this.messageCreator = messageCreator;
        }

        public SendDestinationCallback(EmsTemplate jmsTemplate, Destination destination, IMessageCreator messageCreator)
        {
            this.jmsTemplate = jmsTemplate;
            this.destination = destination;
            this.messageCreator = messageCreator;
        }

        public SendDestinationCallback(EmsTemplate jmsTemplate, string destinationName, MessageCreatorDelegate messageCreatorDelegate)
        {
            this.jmsTemplate = jmsTemplate;
            this.destinationName = destinationName;
            this.messageCreatorDelegate = messageCreatorDelegate;
        }

        public SendDestinationCallback(EmsTemplate jmsTemplate, Destination destination, MessageCreatorDelegate messageCreatorDelegate)
        {
            this.jmsTemplate = jmsTemplate;
            this.destination = destination;
            this.messageCreatorDelegate = messageCreatorDelegate;
        }


        public object DoInEms(ISession session)
        {
            if (destination == null)
            {
                destination = jmsTemplate.ResolveDestinationName(session, destinationName);
            }
            if (messageCreator != null)
            {
                jmsTemplate.DoSend(session, destination, messageCreator);
            }
            else
            {
                jmsTemplate.DoSend(session, destination, messageCreatorDelegate);
            }
            return null;
        }
    }
}
