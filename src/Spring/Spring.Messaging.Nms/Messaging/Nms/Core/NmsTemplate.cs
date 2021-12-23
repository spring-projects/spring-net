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

using Common.Logging;
using Spring.Messaging.Nms.Connections;
using Spring.Messaging.Nms.Support;
using Spring.Messaging.Nms.Support.Converter;
using Spring.Messaging.Nms.Support.Destinations;
using Spring.Transaction.Support;
using Spring.Util;
using Apache.NMS;

namespace Spring.Messaging.Nms.Core
{
    /// <summary> Helper class that simplifies NMS access code.</summary>
    /// <remarks>
    /// <para>If you want to use dynamic destination creation, you must specify
    /// the type of NMS destination to create, using the "pubSubDomain" property.
    /// For other operations, this is not necessary.
    /// Point-to-Point (Queues) is the default domain.</para>
    ///
    /// <para>Default settings for NMS Sessions is "auto-acknowledge".</para>
    ///
    /// <para>This template uses a DynamicDestinationResolver and a SimpleMessageConverter
    /// as default strategies for resolving a destination name or converting a message,
    /// respectively.</para>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class NmsTemplate : NmsDestinationAccessor, INmsOperations
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(NmsTemplate));


        #endregion

        #region Fields

        /// <summary>
        /// Timeout value indicating that a receive operation should
	    /// check if a message is immediately available without blocking.	 
        /// </summary>
        public static readonly long RECEIVE_TIMEOUT_NO_WAIT = -1;

        /// <summary>
        /// Timeout value indicating a blocking receive without timeout.
        /// </summary>
        public static readonly long RECEIVE_TIMEOUT_INDEFINITE_WAIT = 0;

        private NmsTemplateResourceFactory transactionalResourceFactory;

        private object defaultDestination;

        private IMessageConverter messageConverter;


        private bool messageIdEnabled = true;

        private bool messageTimestampEnabled = true;

        private bool pubSubNoLocal = false;

        private long receiveTimeout = RECEIVE_TIMEOUT_NO_WAIT;

        private bool explicitQosEnabled = false;

        private MsgPriority priority = NMSConstants.defaultPriority;	

        private TimeSpan timeToLive;

        private MsgDeliveryMode deliveryMode = NMSConstants.defaultDeliveryMode;

        #endregion

        #region Constructor (s)

        /// <summary> Create a new NmsTemplate.</summary>
        /// <remarks>
        /// <para>Note: The ConnectionFactory has to be set before using the instance.
        /// This constructor can be used to prepare a NmsTemplate via an ObjectFactory,
        /// typically setting the ConnectionFactory.</para>
        /// </remarks>
        public NmsTemplate()
        {
            transactionalResourceFactory = new NmsTemplateResourceFactory(this);
            InitDefaultStrategies();
        }


        /// <summary> Create a new NmsTemplate, given a ConnectionFactory.</summary>
        /// <param name="connectionFactory">the ConnectionFactory to obtain IConnections from
        /// </param>
        public NmsTemplate(IConnectionFactory connectionFactory)
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
                    "No defaultDestination or defaultDestinationName specified. Check configuration of NmsTemplate.");
            }
        }


        private void CheckMessageConverter()
        {
            if (MessageConverter == null)
            {
                throw new SystemException("No messageConverter registered. Check configuration of NmsTemplate.");
            }
        }

        /// <summary> Execute the action specified by the given action object within a
        /// NMS Session.
        /// </summary>
        /// <remarks> Generalized version of <code>execute(ISessionCallback)</code>,
        /// allowing the NMS Connection to be started on the fly.
        /// <p>Use <code>execute(ISessionCallback)</code> for the general case.
        /// Starting the NMS Connection is just necessary for receiving messages,
        /// which is preferably achieved through the <code>receive</code> methods.</p>
        /// </remarks>
        /// <param name="action">callback object that exposes the session
        /// </param>
        /// <param name="startConnection">Start the connection before performing callback action.
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <throws>  NMSException if there is any problem </throws>
        public virtual object Execute(ISessionCallback action, bool startConnection)
        {
            AssertUtils.ArgumentNotNull(action, "Callback object must not be null");

            IConnection conToClose = null;
            ISession sessionToClose = null;
            try
            {
                ISession sessionToUse =
                    ConnectionFactoryUtils.DoGetTransactionalSession(ConnectionFactory, transactionalResourceFactory,
                                                                     startConnection, true).GetAsyncResult();
                if (sessionToUse == null)
                {
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
                    logger.Debug("Executing callback on NMS ISession [" + sessionToUse + "]");
                }
                return action.DoInNms(sessionToUse);
            }
            finally
            {
                NmsUtils.CloseSession(sessionToClose);
                ConnectionFactoryUtils.ReleaseConnection(conToClose, ConnectionFactory, startConnection);
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
        virtual public IDestination DefaultDestination
        {
            get { return (defaultDestination as IDestination); }

            set { defaultDestination = value; }
        }


        /// <summary>
        /// Gets or sets the name of the default destination name
        /// to be used on send/receive operations that
        /// do not have a destination parameter.
        /// </summary>
        /// <remarks>
        /// Alternatively, specify a NMS IDestination object as "DefaultDestination"
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
        /// to handle IBytesMessages, ITextMessages and IObjectMessages.</p>
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
        /// Gets or sets the receive timeout to use for recieve calls (in milliseconds)
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
        /// Sets a value indicating whether message delivery should be persistent or non-persistent
        /// </summary>
        /// <remarks>
        /// This will set the delivery to persistent or non-persistent
        /// <p>Default it "true" aka delivery mode "PERSISTENT".</p>
        /// </remarks>
        /// <value><c>true</c> if [delivery persistent]; otherwise, <c>false</c>.</value>
        virtual public bool Persistent
        {
			get { return (deliveryMode == MsgDeliveryMode.Persistent); }
			
            set
            {
                if (value) {
                    deliveryMode = MsgDeliveryMode.Persistent;
                } else {
                    deliveryMode = MsgDeliveryMode.NonPersistent;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what DeliveryMode this <see cref="CachedMessageProducer"/> 
        /// should use, for example a persistent QOS
        /// </summary>
        /// <value><see cref="MsgDeliveryMode"/></value>
        virtual public MsgDeliveryMode DeliveryMode
        {
            get { return deliveryMode;  }
            set { deliveryMode = value; }
        }

        /// <summary>
        /// Gets or sets the priority when sending.
        /// </summary>
        /// <remarks>Since a default value may be defined administratively,
        /// this is only used when "isExplicitQosEnabled" equals "true".</remarks>
        /// <value>The priority.</value>
        virtual public MsgPriority Priority
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
        virtual public TimeSpan TimeToLive
        {
            get { return timeToLive; }

            set { timeToLive = value; }
        }


        #endregion

        /// <summary>
        /// Extract the content from the given JMS message.
        /// </summary>
        /// <param name="message">The Message to convert (can be <code>null</code>).</param>
        /// <returns>The content of the message, or <code>null</code> if none</returns>
        protected virtual object DoConvertFromMessage(IMessage message)
        {
            if (message != null)
            {
                return MessageConverter.FromMessage(message);
            }
            return null;
        }

        #region NMS Factory Methods

        /// <summary> Fetch an appropriate Connection from the given MessageResourceHolder.
        /// </summary>
        /// <param name="holder">the MessageResourceHolder
        /// </param>
        /// <returns> an appropriate IConnection fetched from the holder,
        /// or <code>null</code> if none found
        /// </returns>
        protected virtual IConnection GetConnection(NmsResourceHolder holder)
        {
            return holder.GetConnection();
        }

        /// <summary> Fetch an appropriate Session from the given MessageResourceHolder.
        /// </summary>
        /// <param name="holder">the MessageResourceHolder
        /// </param>
        /// <returns> an appropriate ISession fetched from the holder,
        /// or <code>null</code> if none found
        /// </returns>
        protected virtual ISession GetSession(NmsResourceHolder holder)
        {
            return holder.GetSession();
        }

        /// <summary> Create a NMS MessageProducer for the given Session and Destination,
        /// configuring it to disable message ids and/or timestamps (if necessary).
        /// <p>Delegates to <code>doCreateProducer</code> for creation of the raw
        /// NMS MessageProducer</p>
        /// </summary>
        /// <param name="session">the NMS Session to create a MessageProducer for
        /// </param>
        /// <param name="destination">the NMS Destination to create a MessageProducer for
        /// </param>
        /// <returns> the new NMS MessageProducer
        /// </returns>
        /// <throws>  NMSException if thrown by NMS API methods </throws>
        /// <seealso cref="DoCreateProducer">
        /// </seealso>
        /// <seealso cref="MessageIdEnabled">
        /// </seealso>
        /// <seealso cref="MessageTimestampEnabled">
        /// </seealso>
        protected virtual IMessageProducer CreateProducer(ISession session, IDestination destination)
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

        /// <summary> Create a raw NMS MessageProducer for the given Session and Destination.
        /// </summary>
        /// <param name="session">the NMS Session to create a MessageProducer for
        /// </param>
        /// <param name="destination">the NMS IDestination to create a MessageProducer for
        /// </param>
        /// <returns> the new NMS MessageProducer
        /// </returns>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        protected virtual IMessageProducer DoCreateProducer(ISession session, IDestination destination)
        {
            return session.CreateProducer(destination);            
        }

        /// <summary> Create a NMS MessageConsumer for the given Session and Destination.
        /// </summary>
        /// <param name="session">the NMS Session to create a MessageConsumer for
        /// </param>
        /// <param name="destination">the NMS Destination to create a MessageConsumer for
        /// </param>
        /// <param name="messageSelector">the message selector for this consumer (can be <code>null</code>)
        /// </param>
        /// <returns> the new NMS IMessageConsumer
        /// </returns>
        /// <throws>  NMSException if thrown by NMS API methods </throws>
        protected virtual IMessageConsumer CreateConsumer(ISession session, IDestination destination,
                                                         string messageSelector)
        {
            // Only pass in the NoLocal flag in case of a Topic:
            // Some NMS providers, such as WebSphere MQ 6.0, throw IllegalStateException
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

        /// <summary>
        /// Send the given message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to send to.</param>
        /// <param name="messageCreatorDelegate">The message creator delegate callback to create a Message.</param>
        protected internal virtual void DoSend(ISession session, IDestination destination, MessageCreatorDelegate messageCreatorDelegate)
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
        protected internal virtual void DoSend(ISession session, IDestination destination, IMessageCreator messageCreator)
        {
            AssertUtils.ArgumentNotNull(messageCreator, "IMessageCreator must not be null");
            DoSend(session, destination, messageCreator, null);
        }

        /// <summary> Send the given NMS message.</summary>
        /// <param name="session">the NMS Session to operate on
        /// </param>
        /// <param name="destination">the NMS Destination to send to
        /// </param>
        /// <param name="messageCreator">callback to create a NMS Message
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a NMS Message
        /// </param>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        protected internal virtual void DoSend(ISession session, IDestination destination, IMessageCreator messageCreator,
                                               MessageCreatorDelegate messageCreatorDelegate)
        {


            IMessageProducer producer = CreateProducer(session, destination);
            try
            {
                
                IMessage message = null;
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
                    NmsUtils.CommitIfNecessary(session);
                }
            }
            finally
            {
                NmsUtils.CloseMessageProducer(producer);
            }
        }


        /// <summary> Actually send the given NMS message.</summary>
        /// <param name="producer">the NMS MessageProducer to send with
        /// </param>
        /// <param name="message">the NMS Message to send
        /// </param>
        /// <throws>  NMSException if thrown by NMS API methods </throws>
        protected virtual void DoSend(IMessageProducer producer, IMessage message)
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

        #region IMessageOperations Implementation

        /// <summary>
        /// Execute the action specified by the given action object within
        /// a NMS Session.
        /// </summary>
        /// <param name="del">delegate that exposes the session</param>
        /// <returns>
        /// the result object from working with the session
        /// </returns>
        /// <remarks>
        /// 	<para>Note that the value of PubSubDomain affects the behavior of this method.
        /// If PubSubDomain equals true, then a Session is passed to the callback.
        /// If false, then a ISession is passed to the callback.</para>b
        /// </remarks>
        /// <throws>NMSException if there is any problem </throws>
        public object Execute(SessionDelegate del)
        {
            return Execute(new ExecuteSessionCallbackUsingDelegate(del));
        }

        /// <summary> Execute the action specified by the given action object within
        /// a NMS Session.
        /// <p>Note: The value of PubSubDomain affects the behavior of this method.
        /// If PubSubDomain equals true, then a Session is passed to the callback.
        /// If false, then a Session is passed to the callback.</p>
        /// </summary>
        /// <param name="action">callback object that exposes the session
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <throws>NMSException if there is any problem </throws>
        public object Execute(ISessionCallback action)
        {
            return Execute(action, false);
        }

        /// <summary> Send a message to a NMS destination. The callback gives access to
        /// the NMS session and MessageProducer in order to do more complex
        /// send operations.
        /// </summary>
        /// <param name="action">callback object that exposes the session/producer pair
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <throws>NMSException  if there is any problem </throws>
        public object Execute(IProducerCallback action)
        {
            return Execute(new ProducerCreatorCallback(this, action));
        }

        /// <summary> Send a message to a NMS destination. The callback gives access to
        /// the NMS session and MessageProducer in order to do more complex
        /// send operations.
        /// </summary>
        /// <param name="del">delegate that exposes the session/producer pair
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <throws>NMSException  if there is any problem </throws>
        public object Execute(ProducerDelegate del)
        {
            return Execute(new ProducerCreatorCallback(this, del));
        }

        /// <summary> Send a message to the default destination.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
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
        /// <throws>NMSException if there is any problem</throws>
        public void SendWithDelegate(IDestination destination, MessageCreatorDelegate messageCreatorDelegate)
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
        /// <throws>NMSException if there is any problem</throws>
        public void SendWithDelegate(string destinationName, MessageCreatorDelegate messageCreatorDelegate)
        {
            Execute(new SendDestinationCallback(this, destinationName, messageCreatorDelegate), false);
        }

        /// <summary> Send a message to the default destination.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageCreator">callback to create a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
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
        /// <throws>NMSException if there is any problem</throws>
        public void Send(IDestination destination, IMessageCreator messageCreator)
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
        /// <throws>NMSException if there is any problem</throws>
        public void Send(string destinationName, IMessageCreator messageCreator)
        {
            Execute(new SendDestinationCallback(this, destinationName, messageCreator), false);
        }
        /// <summary> Send the given object to the default destination, converting the object
        /// to a NMS message with a configured IMessageConverter.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
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
        /// to a NMS message with a configured IMessageConverter.
        /// </summary>
        /// <param name="destination">the destination to send this message to
        /// </param>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public void ConvertAndSend(IDestination destination, object message)
        {
            CheckMessageConverter();
            Send(destination, new SimpleMessageCreator(this, message));
        }

        /// <summary> Send the given object to the specified destination, converting the object
        /// to a NMS message with a configured IMessageConverter.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public void ConvertAndSend(string destinationName, object message)
        {
            Send(destinationName, new SimpleMessageCreator(this, message));
        }

        /// <summary> Send the given object to the default destination, converting the object
        /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <param name="postProcessor">the callback to modify the message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
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
        /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destination">the destination to send this message to
        /// </param>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <param name="postProcessor">the callback to modify the message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public void ConvertAndSend(IDestination destination, object message, IMessagePostProcessor postProcessor)
        {
            CheckMessageConverter();
            Send(destination, new ConvertAndSendMessageCreator(this, message, postProcessor));
            
        }

        /// <summary> Send the given object to the specified destination, converting the object
        /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="message">the object to convert to a message.
        /// </param>
        /// <param name="postProcessor">the callback to modify the message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public void ConvertAndSend(string destinationName, object message, IMessagePostProcessor postProcessor)
        {
            CheckMessageConverter();
            Send(destinationName, new ConvertAndSendMessageCreator(this, message, postProcessor));
  
        }


        /// <summary>
        /// Send the given object to the default destination, converting the object
        /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="message">the object to convert to a message</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <throws>NMSException if there is any problem</throws>
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
        /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destination">the destination to send this message to</param>
        /// <param name="message">the object to convert to a message</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <throws>NMSException if there is any problem</throws>
        public void ConvertAndSendWithDelegate(IDestination destination, object message,
                                               MessagePostProcessorDelegate postProcessor)
        {
            CheckMessageConverter();
            Send(destination, new ConvertAndSendMessageCreator(this, message, postProcessor));
        }

        /// <summary>
        /// Send the given object to the specified destination, converting the object
        /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="message">the object to convert to a message.</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <throws>NMSException if there is any problem</throws>
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
        /// <throws>NMSException if there is any problem</throws>
        public IMessage Receive()
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
        /// <throws>NMSException if there is any problem</throws>
        public IMessage Receive(IDestination destination)
        {            
            return ReceiveSelected(destination, null);
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
        /// <throws>NMSException if there is any problem</throws>
        public IMessage Receive(string destinationName)
        {
            return ReceiveSelected(destinationName, null);
        }

        /// <summary> Receive a message synchronously from the default destination, but only
        /// wait up to a specified time for delivery.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
        /// See the NMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <throws>NMSException if there is any problem</throws>
        public IMessage ReceiveSelected(string messageSelector)
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
        /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
        /// See the NMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <throws>NMSException if there is any problem</throws>
        public IMessage ReceiveSelected(IDestination destination, string messageSelector)
        {
            return Execute(new ReceiveSelectedCallback(this, destination, messageSelector), true) as IMessage;
        }

        /// <summary> Receive a message synchronously from the specified destination, but only
        /// wait up to a specified time for delivery.
        /// <p>This method should be used carefully, since it will block the thread
        /// until the message becomes available or until the timeout value is exceeded.</p>
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
        /// See the NMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
        /// </returns>
        /// <throws>NMSException if there is any problem</throws>
        public IMessage ReceiveSelected(string destinationName, string messageSelector)
        {
            return Execute(new ReceiveSelectedCallback(this, destinationName, messageSelector), true) as IMessage;        
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to receive from.</param>
        /// <param name="messageSelector">The message selector for this consumer (can be <code>null</code></param>
        /// <returns>The Message received, or <code>null</code> if none.</returns>
        protected virtual IMessage DoReceive(ISession session, IDestination destination, string messageSelector)
        {
            return DoReceive(session, CreateConsumer(session, destination, messageSelector));
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="consumer">The consumer to receive with.</param>
        /// <returns>The Message received, or <code>null</code> if none</returns>
        protected virtual IMessage DoReceive(ISession session, IMessageConsumer consumer)
        {
            try
            {
                long timeout = ReceiveTimeout;
                NmsResourceHolder resourceHolder =
                (NmsResourceHolder)TransactionSynchronizationManager.GetResource(ConnectionFactory);
                if (resourceHolder != null && resourceHolder.HasTimeout)
                {
                    timeout = Convert.ToInt64(resourceHolder.TimeToLiveInMilliseconds);
                }    
                IMessage message = (timeout > 0)
                                      ? consumer.Receive(TimeSpan.FromMilliseconds(timeout))
                                      : consumer.Receive();
                if (session.Transacted)
                {
                    // Commit necessary - but avoid commit call is Session transaction is externally coordinated.
                    if (IsSessionLocallyTransacted(session))
                    {
                        // Transacted session created by this template -> commit.
                        NmsUtils.CommitIfNecessary(session);
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
                NmsUtils.CloseMessageConsumer(consumer);
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
        /// <throws>NMSException if there is any problem</throws>
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
        /// <throws>NMSException if there is any problem</throws>
        public object ReceiveAndConvert(IDestination destination)
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
        /// <throws>NMSException if there is any problem</throws>
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
        /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
        /// See the NMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <throws>NMSException if there is any problem</throws>
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
        /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
        /// See the NMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <throws>NMSException if there is any problem</throws>
        public object ReceiveSelectedAndConvert(IDestination destination, string messageSelector)
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
        /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
        /// See the NMS specification for a detailed definition of selector expressions.
        /// </param>
        /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
        /// </returns>
        /// <throws>NMSException if there is any problem</throws>
        public object ReceiveSelectedAndConvert(string destinationName, string messageSelector)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(ReceiveSelected(destinationName, messageSelector));
        }

        #endregion

        #region Supporting Internal Classes

        /// <summary>
        /// ResourceFactory implementation that delegates to this template's callback methods.
        /// </summary>
        private class NmsTemplateResourceFactory : ConnectionFactoryUtils.ResourceFactory
        {
            private NmsTemplate enclosingTemplateInstance;
			
            public NmsTemplateResourceFactory(NmsTemplate enclosingInstance)
            {
                InitBlock(enclosingInstance);
            }

            private void InitBlock(NmsTemplate enclosingInstance)
            {
                enclosingTemplateInstance = enclosingInstance;
            }

            public NmsTemplate EnclosingInstance
            {
                get { return enclosingTemplateInstance; }
            }

            public virtual IConnection GetConnection(NmsResourceHolder holder)
            {
                return EnclosingInstance.GetConnection(holder);
            }

            public virtual ISession GetSession(NmsResourceHolder holder)
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

            public Task<IConnection> CreateConnectionAsync()
            {
                return Task.FromResult(CreateConnection());
            }

            public Task<ISession> CreateSessionAsync(IConnection con)
            {
                return Task.FromResult(CreateSession(con));
            }

            public bool SynchedLocalTransactionAllowed
            {
                get { return EnclosingInstance.SessionTransacted; }
            }
        }

        private class ProducerCreatorCallback : ISessionCallback
        {
            private NmsTemplate jmsTemplate;
            private IProducerCallback producerCallback;
            private ProducerDelegate producerDelegate;

            public ProducerCreatorCallback(NmsTemplate jmsTemplate, IProducerCallback producerCallback)
            {
                this.jmsTemplate = jmsTemplate;
				this.producerCallback = producerCallback;
            }

            public ProducerCreatorCallback(NmsTemplate jmsTemplate, ProducerDelegate producerDelegate)
            {
                this.jmsTemplate = jmsTemplate;
                this.producerDelegate = producerDelegate;
            }

            public object DoInNms(ISession session)
            {
                IMessageProducer producer = jmsTemplate.CreateProducer(session, null);
                try
                {
                    if (producerCallback != null)
                    {
                        return producerCallback.DoInNms(session, producer);
                    }
                    else
                    {
                        return producerDelegate(session, producer);
                    }
                }
                finally
                {
                    NmsUtils.CloseMessageProducer(producer);
                }

            }
        }

        private class ReceiveCallback : ISessionCallback
        {
            private NmsTemplate jmsTemplate;
            private IDestination destination;
            private string destinationName;


            public ReceiveCallback(NmsTemplate jmsTemplate, string destinationName)
            {
                this.jmsTemplate = jmsTemplate;
                this.destinationName = destinationName;
            }

            public ReceiveCallback(NmsTemplate jmsTemplate, IDestination destination)
            {
                this.jmsTemplate = jmsTemplate;
                this.destination = destination;
            }

            public object DoInNms(ISession session)
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
            private NmsTemplate jmsTemplate;
            private object objectToConvert;
            private IMessagePostProcessor messagePostProcessor;
            private MessagePostProcessorDelegate messagePostProcessorDelegate;

            public ConvertAndSendMessageCreator(NmsTemplate jmsTemplate, object message, IMessagePostProcessor messagePostProcessor)
            {
                this.jmsTemplate = jmsTemplate;
                objectToConvert = message;
                this.messagePostProcessor = messagePostProcessor;
            }

            public ConvertAndSendMessageCreator(NmsTemplate jmsTemplate, object message, MessagePostProcessorDelegate messagePostProcessorDelegate)
            {
                this.jmsTemplate = jmsTemplate;
                objectToConvert = message;
                this.messagePostProcessorDelegate = messagePostProcessorDelegate;
            }

            public IMessage CreateMessage(ISession session)
            {
                IMessage msg = jmsTemplate.MessageConverter.ToMessage(objectToConvert, session);
                if (messagePostProcessor != null)
                {
                    return messagePostProcessor.PostProcessMessage(msg);
                } else
                {
                    return messagePostProcessorDelegate(msg);
                }
            }

        }

        private class ReceiveSelectedCallback : ISessionCallback
        {
            private NmsTemplate jmsTemplate;
            private string messageSelector;
            private string destinationName;
            private IDestination destination;

            public ReceiveSelectedCallback(NmsTemplate jmsTemplate,
                               IDestination destination,
                               string messageSelector)
            {
                this.jmsTemplate = jmsTemplate;
                this.destination = destination;
                this.messageSelector = messageSelector;
            }
            public ReceiveSelectedCallback(NmsTemplate jmsTemplate,
                                           string destinationName,
                                           string messageSelector)
            {
                this.jmsTemplate = jmsTemplate;
                this.destinationName = destinationName;
                this.messageSelector = messageSelector;
            }

            public object DoInNms(ISession session)
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
        
        #endregion

        private class ExecuteSessionCallbackUsingDelegate : ISessionCallback
        {
            private readonly SessionDelegate del;
            public ExecuteSessionCallbackUsingDelegate(SessionDelegate del)
            {
                this.del = del;
            }

            public object DoInNms(ISession session)
            {
                return del(session);
            }
        }
    }



    internal class SimpleMessageCreator : IMessageCreator
    {
        private NmsTemplate jmsTemplate;
        private object objectToConvert;
        
        public SimpleMessageCreator(NmsTemplate jmsTemplate, object objectToConvert)
        {
            this.jmsTemplate = jmsTemplate;
            this.objectToConvert = objectToConvert;
        }

        public IMessage CreateMessage(ISession session)
        {
            return jmsTemplate.MessageConverter.ToMessage(objectToConvert, session);
        }


    }



    internal class SendDestinationCallback : ISessionCallback
    {
        private string destinationName;
        private IDestination destination;
        private NmsTemplate jmsTemplate;
        private IMessageCreator messageCreator;
        private MessageCreatorDelegate messageCreatorDelegate;

        public SendDestinationCallback(NmsTemplate jmsTemplate, string destinationName, IMessageCreator messageCreator)
        {
            this.jmsTemplate = jmsTemplate;
            this.destinationName = destinationName;
            this.messageCreator = messageCreator;
        }

        public SendDestinationCallback(NmsTemplate jmsTemplate, IDestination destination, IMessageCreator messageCreator)
        {
            this.jmsTemplate = jmsTemplate;
            this.destination = destination;
            this.messageCreator = messageCreator;
        }

        public SendDestinationCallback(NmsTemplate jmsTemplate, string destinationName, MessageCreatorDelegate messageCreatorDelegate)
        {
            this.jmsTemplate = jmsTemplate;
            this.destinationName = destinationName;
            this.messageCreatorDelegate = messageCreatorDelegate;
        }

        public SendDestinationCallback(NmsTemplate jmsTemplate, IDestination destination, MessageCreatorDelegate messageCreatorDelegate)
        {
            this.jmsTemplate = jmsTemplate;
            this.destination = destination;
            this.messageCreatorDelegate = messageCreatorDelegate;
        }


        public object DoInNms(ISession session)
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
