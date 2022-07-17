#region License
// /*
//  * Copyright 2022 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
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
    /// <summary>
    /// Async version of NmsTemplate
    /// </summary>
    /// <see cref="NmsTemplate"/>
    public class NmsTemplateAsync : NmsDestinationAccessorAsync, INmsOperationsAsync
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

        private readonly NmsTemplateResourceFactoryAsync transactionalResourceFactory;

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
        public NmsTemplateAsync()
        {
            transactionalResourceFactory = new NmsTemplateResourceFactoryAsync(this);
            InitDefaultStrategies();
        }


        /// <summary> Create a new NmsTemplate, given a ConnectionFactory.</summary>
        /// <param name="connectionFactory">the ConnectionFactory to obtain IConnections from
        /// </param>
        public NmsTemplateAsync(IConnectionFactory connectionFactory)
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
        public virtual async Task<object> Execute(ISessionCallbackAsync action, bool startConnection)
        {
            AssertUtils.ArgumentNotNull(action, "Callback object must not be null");

            IConnection conToClose = null;
            ISession sessionToClose = null;
            try
            {
                ISession sessionToUse =
                    await ConnectionFactoryUtils.DoGetTransactionalSession(ConnectionFactory, transactionalResourceFactory,
                        startConnection).Awaiter();
                if (sessionToUse == null)
                {
                    conToClose = await CreateConnection().Awaiter();
                    sessionToClose = await CreateSession(conToClose).Awaiter();
                    if (startConnection)
                    {
                        await conToClose.StartAsync().Awaiter();
                    }
                    sessionToUse = sessionToClose;
                }
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Executing callback on NMS ISession [" + sessionToUse + "]");
                }
                return await action.DoInNms(sessionToUse).Awaiter();
            }
            finally
            {
                await NmsUtilsAsync.CloseSession(sessionToClose).Awaiter();
                await ConnectionFactoryUtils.ReleaseConnectionAsync(conToClose, ConnectionFactory, startConnection).Awaiter();
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
            return holder.GetConnection(); // TODO
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
            return holder.GetSession(); // TODO
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
        protected virtual async Task<IMessageProducer> CreateProducer(ISession session, IDestination destination)
        {
            IMessageProducer producer = await DoCreateProducer(session, destination).Awaiter();
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
        protected virtual Task<IMessageProducer> DoCreateProducer(ISession session, IDestination destination)
        {
            return session.CreateProducerAsync(destination);            
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
        protected virtual Task<IMessageConsumer> CreateConsumer(ISession session, IDestination destination,
                                                         string messageSelector)
        {
            // Only pass in the NoLocal flag in case of a Topic:
            // Some NMS providers, such as WebSphere MQ 6.0, throw IllegalStateException
            // in case of the NoLocal flag being specified for a Queue.
            if (PubSubDomain)
            {
                return session.CreateConsumerAsync(destination, messageSelector, PubSubNoLocal);
            }
            else
            {
                return session.CreateConsumerAsync(destination, messageSelector);
            }
        }

        /// <summary>
        /// Send the given message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to send to.</param>
        /// <param name="messageCreatorDelegate">The message creator delegate callback to create a Message.</param>
        protected internal virtual async Task DoSend(ISession session, IDestination destination, MessageCreatorDelegate messageCreatorDelegate)
        {
            AssertUtils.ArgumentNotNull(messageCreatorDelegate, "IMessageCreatorDelegate must not be null");
            await DoSend(session, destination, null, messageCreatorDelegate).Awaiter();
        }

        /// <summary>
        /// Send the given message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to send to.</param>
        /// <param name="messageCreator">The message creator callback to create a Message.</param>
        protected internal virtual async Task DoSend(ISession session, IDestination destination, IMessageCreator messageCreator)
        {
            AssertUtils.ArgumentNotNull(messageCreator, "IMessageCreator must not be null");
            await DoSend(session, destination, messageCreator, null).Awaiter();
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
        protected internal virtual async Task DoSend(ISession session, IDestination destination, IMessageCreator messageCreator,
                                               MessageCreatorDelegate messageCreatorDelegate)
        {
            IMessageProducer producer = await CreateProducer(session, destination).Awaiter();
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
                await DoSend(producer, message).Awaiter();

                // Check commit, avoid commit call is Session transaction is externally coordinated.
                if (session.Transacted && IsSessionLocallyTransacted(session))
                {
                    // Transacted session created by this template -> commit.
                    await NmsUtilsAsync.CommitIfNecessary(session).Awaiter();
                }
            }
            finally
            {
                await NmsUtilsAsync.CloseMessageProducer(producer).Awaiter();
            }
        }


        /// <summary> Actually send the given NMS message.</summary>
        /// <param name="producer">the NMS MessageProducer to send with
        /// </param>
        /// <param name="message">the NMS Message to send
        /// </param>
        /// <throws>  NMSException if thrown by NMS API methods </throws>
        protected virtual async Task DoSend(IMessageProducer producer, IMessage message)
        {
            if (ExplicitQosEnabled)
            {
                await producer.SendAsync(message, DeliveryMode, Priority, TimeToLive).Awaiter();
            }
            else
            {
                await producer.SendAsync(message).Awaiter();
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
        public Task<object> Execute(SessionDelegateAsync del)
        {
            return Execute(new ExecuteSessionCallbackUsingDelegateAsync(del));
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
        public Task<object> Execute(ISessionCallbackAsync action)
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
        public Task<object> Execute(IProducerCallbackAsync action)
        {
            return Execute(new ProducerCreatorCallbackAsync(this, action));
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
        public Task<object> Execute(ProducerDelegate del)
        {
            return Execute(new ProducerCreatorCallbackAsync(this, del));
        }

        /// <summary> Send a message to the default destination.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public async Task SendWithDelegate(MessageCreatorDelegate messageCreatorDelegate)
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                await SendWithDelegate(DefaultDestination, messageCreatorDelegate).Awaiter();
            }
            else
            {
                await SendWithDelegate(DefaultDestinationName, messageCreatorDelegate).Awaiter();
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
        public Task SendWithDelegate(IDestination destination, MessageCreatorDelegate messageCreatorDelegate)
        {
            return Execute(new SendDestinationCallbackAsync(this, destination, messageCreatorDelegate), false);
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
        public Task SendWithDelegate(string destinationName, MessageCreatorDelegate messageCreatorDelegate)
        {
            return Execute(new SendDestinationCallbackAsync(this, destinationName, messageCreatorDelegate), false);
        }

        
        /// <summary> Send a message to the default destination.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageCreator">callback to create a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public async Task Send(IMessageCreator messageCreator)
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                await Send(DefaultDestination, messageCreator).Awaiter();
            }
            else
            {
                await Send(DefaultDestinationName, messageCreator).Awaiter();
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
        public Task Send(IDestination destination, IMessageCreator messageCreator)
        {
            return Execute(new SendDestinationCallbackAsync(this, destination, messageCreator), false);
        }

        /// <summary> Send a message to the specified destination.
        /// The MessageCreator callback creates the message given a Session.
        /// </summary>
        /// <param name="destinationName">the destination to send this message to
        /// </param>
        /// <param name="messageCreator">callback to create a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public Task Send(string destinationName, IMessageCreator messageCreator)
        {
            return Execute(new SendDestinationCallbackAsync(this, destinationName, messageCreator), false);
        }
        /// <summary> Send the given object to the default destination, converting the object
        /// to a NMS message with a configured IMessageConverter.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="message">the object to convert to a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public async Task ConvertAndSend(object message)
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                await ConvertAndSend(DefaultDestination, message).Awaiter();
            }
            else
            {
                await ConvertAndSend(DefaultDestinationName, message).Awaiter();
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
        public Task ConvertAndSend(IDestination destination, object message)
        {
            CheckMessageConverter();
            return Send(destination, new SimpleMessageCreatorAsync(this, message));
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
        public Task ConvertAndSend(string destinationName, object message)
        {
            return Send(destinationName, new SimpleMessageCreatorAsync(this, message));
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
        public async Task ConvertAndSend(object message, IMessagePostProcessor postProcessor)
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                await ConvertAndSend(DefaultDestination, message, postProcessor).Awaiter();
            }
            else
            {
                await ConvertAndSend(DefaultDestinationName, message, postProcessor).Awaiter();
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
        public Task ConvertAndSend(IDestination destination, object message, IMessagePostProcessor postProcessor)
        {
            CheckMessageConverter();
            return Send(destination, new ConvertAndSendMessageCreatorAsync(this, message, postProcessor));
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
        public Task ConvertAndSend(string destinationName, object message, IMessagePostProcessor postProcessor)
        {
            CheckMessageConverter();
            return Send(destinationName, new ConvertAndSendMessageCreatorAsync(this, message, postProcessor));
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
        public async Task ConvertAndSendWithDelegate(object message, MessagePostProcessorDelegate postProcessor)
        {
            //Execute(new SendDestinationCallback(this, destination, messageCreatorDelegate), false);
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                await ConvertAndSendWithDelegate(DefaultDestination, message, postProcessor).Awaiter();
            }
            else
            {
                await ConvertAndSendWithDelegate(DefaultDestinationName, message, postProcessor).Awaiter();
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
        public Task ConvertAndSendWithDelegate(IDestination destination, object message,
                                               MessagePostProcessorDelegate postProcessor)
        {
            CheckMessageConverter();
            return Send(destination, new ConvertAndSendMessageCreatorAsync(this, message, postProcessor));
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
        public Task ConvertAndSendWithDelegate(string destinationName, object message,
                                               MessagePostProcessorDelegate postProcessor)
        {
            CheckMessageConverter();
            return Send(destinationName, new ConvertAndSendMessageCreatorAsync(this, message, postProcessor));
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
        public async Task<IMessage> Receive()
        {
            CheckDefaultDestination();
            if (DefaultDestination != null)
            {
                return await Receive(DefaultDestination).Awaiter();
            }
            else
            {
                return await Receive(DefaultDestinationName).Awaiter();
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
        public Task<IMessage> Receive(IDestination destination)
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
        public Task<IMessage> Receive(string destinationName)
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
        public async Task<IMessage> ReceiveSelected(string messageSelector)
        {
            CheckDefaultDestination();
            if (DefaultDestination!= null)
            {
                return await ReceiveSelected(DefaultDestination, messageSelector).Awaiter();
            }
            else
            {
                return await ReceiveSelected(DefaultDestinationName, messageSelector).Awaiter();
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
        public async Task<IMessage> ReceiveSelected(IDestination destination, string messageSelector)
        {
            return (await Execute(new ReceiveSelectedCallbackAsync(this, destination, messageSelector), true).Awaiter()) as IMessage;
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
        public async Task<IMessage> ReceiveSelected(string destinationName, string messageSelector)
        {
            return (await Execute(new ReceiveSelectedCallbackAsync(this, destinationName, messageSelector), true).Awaiter()) as IMessage;        
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to receive from.</param>
        /// <param name="messageSelector">The message selector for this consumer (can be <code>null</code></param>
        /// <returns>The Message received, or <code>null</code> if none.</returns>
        protected virtual async Task<IMessage> DoReceive(ISession session, IDestination destination, string messageSelector)
        {
            return await DoReceive(session, await CreateConsumer(session, destination, messageSelector).Awaiter()).Awaiter();
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="consumer">The consumer to receive with.</param>
        /// <returns>The Message received, or <code>null</code> if none</returns>
        protected virtual async Task<IMessage> DoReceive(ISession session, IMessageConsumer consumer)
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
                                      ? await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(timeout)).Awaiter()
                                      : await consumer.ReceiveAsync().Awaiter();
                if (session.Transacted)
                {
                    // Commit necessary - but avoid commit call is Session transaction is externally coordinated.
                    if (IsSessionLocallyTransacted(session))
                    {
                        // Transacted session created by this template -> commit.
                        await NmsUtilsAsync.CommitIfNecessary(session).Awaiter();
                    }
                }
                else if (IsClientAcknowledge(session))
                {
                    // Manually acknowledge message, if any.
                    if (message != null)
                    {
                        await message.AcknowledgeAsync().Awaiter();
                    }
                }

                return message;
                // return message;
            }
            finally
            {
                await NmsUtilsAsync.CloseMessageConsumer(consumer).Awaiter();
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
        public async Task<object> ReceiveAndConvert()
        {
            CheckMessageConverter();
            return DoConvertFromMessage(await Receive().Awaiter());
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
        public async Task<object> ReceiveAndConvert(IDestination destination)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(await Receive(destination).Awaiter());
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
        public async Task<object> ReceiveAndConvert(string destinationName)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(await Receive(destinationName).Awaiter());
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
        public async Task<object> ReceiveSelectedAndConvert(string messageSelector)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(await ReceiveSelected(messageSelector).Awaiter());
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
        public async Task<object> ReceiveSelectedAndConvert(IDestination destination, string messageSelector)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(await ReceiveSelected(destination, messageSelector).Awaiter());
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
        public async Task<object> ReceiveSelectedAndConvert(string destinationName, string messageSelector)
        {
            CheckMessageConverter();
            return DoConvertFromMessage(await ReceiveSelected(destinationName, messageSelector).Awaiter());
        }

        #endregion

        #region Supporting Internal Classes

        /// <summary>
        /// ResourceFactory implementation that delegates to this template's callback methods.
        /// </summary>
        private class NmsTemplateResourceFactoryAsync : ConnectionFactoryUtils.ResourceFactory
        {
            private NmsTemplateAsync enclosingTemplateInstance;
			
            public NmsTemplateResourceFactoryAsync(NmsTemplateAsync enclosingInstance)
            {
                InitBlock(enclosingInstance);
            }

            private void InitBlock(NmsTemplateAsync enclosingInstance)
            {
                enclosingTemplateInstance = enclosingInstance;
            }

            public NmsTemplateAsync EnclosingInstance
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
                return EnclosingInstance.CreateConnection().GetAsyncResult();
            }

            public virtual ISession CreateSession(IConnection con)
            {
                return EnclosingInstance.CreateSession(con).GetAsyncResult();
            }
            
            public virtual Task<IConnection> CreateConnectionAsync()
            {
                return EnclosingInstance.CreateConnection();
            }

            public virtual Task<ISession> CreateSessionAsync(IConnection con)
            {
                return EnclosingInstance.CreateSession(con);
            }

            public bool SynchedLocalTransactionAllowed
            {
                get { return EnclosingInstance.SessionTransacted; }
            }
        }

        private class ProducerCreatorCallbackAsync : ISessionCallbackAsync
        {
            private readonly NmsTemplateAsync jmsTemplate;
            private readonly IProducerCallbackAsync producerCallback;
            private readonly ProducerDelegate producerDelegate;

            public ProducerCreatorCallbackAsync(NmsTemplateAsync jmsTemplate, IProducerCallbackAsync producerCallback)
            {
                this.jmsTemplate = jmsTemplate;
				this.producerCallback = producerCallback;
            }

            public ProducerCreatorCallbackAsync(NmsTemplateAsync jmsTemplate, ProducerDelegate producerDelegate)
            {
                this.jmsTemplate = jmsTemplate;
                this.producerDelegate = producerDelegate;
            }

            public async Task<object> DoInNms(ISession session)
            {
                IMessageProducer producer = await jmsTemplate.CreateProducer(session, null).Awaiter();
                try
                {
                    if (producerCallback != null)
                    {
                        return await producerCallback.DoInNms(session, producer).Awaiter();
                    }
                    else
                    {
                        return producerDelegate(session, producer);
                    }
                }
                finally
                {
                    await NmsUtilsAsync.CloseMessageProducer(producer).Awaiter();
                }

            }
        }

        private class ReceiveCallbackAsync : ISessionCallbackAsync
        {
            private readonly NmsTemplateAsync jmsTemplate;
            private readonly IDestination destination;
            private readonly string destinationName;


            public ReceiveCallbackAsync(NmsTemplateAsync jmsTemplate, string destinationName)
            {
                this.jmsTemplate = jmsTemplate;
                this.destinationName = destinationName;
            }

            public ReceiveCallbackAsync(NmsTemplateAsync jmsTemplate, IDestination destination)
            {
                this.jmsTemplate = jmsTemplate;
                this.destination = destination;
            }

            public async Task<object> DoInNms(ISession session)
            {
                if (destination != null)
                {
                    return await jmsTemplate.DoReceive(session, destination, null).Awaiter();
                }
                else
                {
                    return await jmsTemplate.DoReceive(session,await 
                                                 jmsTemplate.ResolveDestinationName(session, destinationName).Awaiter(),
                                                 null).Awaiter();
                }
                
            }
        }

        private class ConvertAndSendMessageCreatorAsync : IMessageCreator
        {
            private readonly NmsTemplateAsync jmsTemplate;
            private readonly object objectToConvert;
            private readonly IMessagePostProcessor messagePostProcessor;
            private readonly MessagePostProcessorDelegate messagePostProcessorDelegate;

            public ConvertAndSendMessageCreatorAsync(NmsTemplateAsync jmsTemplate, object message, IMessagePostProcessor messagePostProcessor)
            {
                this.jmsTemplate = jmsTemplate;
                objectToConvert = message;
                this.messagePostProcessor = messagePostProcessor;
            }

            public ConvertAndSendMessageCreatorAsync(NmsTemplateAsync jmsTemplate, object message, MessagePostProcessorDelegate messagePostProcessorDelegate)
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

        private class ReceiveSelectedCallbackAsync : ISessionCallbackAsync
        {
            private readonly NmsTemplateAsync jmsTemplate;
            private readonly string messageSelector;
            private readonly string destinationName;
            private readonly IDestination destination;

            public ReceiveSelectedCallbackAsync(NmsTemplateAsync jmsTemplate,
                               IDestination destination,
                               string messageSelector)
            {
                this.jmsTemplate = jmsTemplate;
                this.destination = destination;
                this.messageSelector = messageSelector;
            }
            public ReceiveSelectedCallbackAsync(NmsTemplateAsync jmsTemplate,
                                           string destinationName,
                                           string messageSelector)
            {
                this.jmsTemplate = jmsTemplate;
                this.destinationName = destinationName;
                this.messageSelector = messageSelector;
            }

            public async Task<object> DoInNms(ISession session)
            {
                if (destination != null)
                {
                    return await jmsTemplate.DoReceive(session, destination, messageSelector).Awaiter();
                }
                else
                {
                    return await jmsTemplate.DoReceive(session,await 
                                                 jmsTemplate.ResolveDestinationName(session, destinationName).Awaiter(),
                                                 messageSelector).Awaiter();
                }

            }

        }
        
        #endregion

        private class ExecuteSessionCallbackUsingDelegateAsync : ISessionCallbackAsync
        {
            private readonly SessionDelegateAsync del;
            public ExecuteSessionCallbackUsingDelegateAsync(SessionDelegateAsync del)
            {
                this.del = del;
            }

            public Task<object> DoInNms(ISession session)
            {
                return del(session);
            }
        }
    }



    internal class SimpleMessageCreatorAsync : IMessageCreator
    {
        private readonly NmsTemplateAsync jmsTemplate;
        private readonly object objectToConvert;
        
        public SimpleMessageCreatorAsync(NmsTemplateAsync jmsTemplate, object objectToConvert)
        {
            this.jmsTemplate = jmsTemplate;
            this.objectToConvert = objectToConvert;
        }

        public IMessage CreateMessage(ISession session)
        {
            return jmsTemplate.MessageConverter.ToMessage(objectToConvert, session);
        }


    }



    internal class SendDestinationCallbackAsync : ISessionCallbackAsync
    {
        private readonly string destinationName;
        private IDestination destination;
        private readonly NmsTemplateAsync jmsTemplate;
        private readonly IMessageCreator messageCreator;
        private readonly MessageCreatorDelegate messageCreatorDelegate;

        public SendDestinationCallbackAsync(NmsTemplateAsync jmsTemplate, string destinationName, IMessageCreator messageCreator)
        {
            this.jmsTemplate = jmsTemplate;
            this.destinationName = destinationName;
            this.messageCreator = messageCreator;
        }

        public SendDestinationCallbackAsync(NmsTemplateAsync jmsTemplate, IDestination destination, IMessageCreator messageCreator)
        {
            this.jmsTemplate = jmsTemplate;
            this.destination = destination;
            this.messageCreator = messageCreator;
        }

        public SendDestinationCallbackAsync(NmsTemplateAsync jmsTemplate, string destinationName, MessageCreatorDelegate messageCreatorDelegate)
        {
            this.jmsTemplate = jmsTemplate;
            this.destinationName = destinationName;
            this.messageCreatorDelegate = messageCreatorDelegate;
        }

        public SendDestinationCallbackAsync(NmsTemplateAsync jmsTemplate, IDestination destination, MessageCreatorDelegate messageCreatorDelegate)
        {
            this.jmsTemplate = jmsTemplate;
            this.destination = destination;
            this.messageCreatorDelegate = messageCreatorDelegate;
        }


        public async Task<object> DoInNms(ISession session)
        {
            if (destination == null)
            {
                destination =await jmsTemplate.ResolveDestinationName(session, destinationName).Awaiter();
            }
            if (messageCreator != null)
            {
                await jmsTemplate.DoSend(session, destination, messageCreator).Awaiter();
            }
            else
            {
                await jmsTemplate.DoSend(session, destination, messageCreatorDelegate).Awaiter();
            }
            return null;
        }
    }
}
