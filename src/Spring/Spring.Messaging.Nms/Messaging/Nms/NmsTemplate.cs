#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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
using Spring.Messaging.Nms.Connection;
using Spring.Messaging.Nms.Support;
using Spring.Messaging.Nms.Support.Converter;
using Spring.Messaging.Nms.Support.IDestinations;
using Spring.Transaction.Support;
using Spring.Util;
using Apache.NMS;

namespace Spring.Messaging.Nms
{
    /// <summary> Helper class that simplifies NMS access code.</summary>
    /// <remarks>
    /// <para>If you want to use dynamic destination creation, you must specify
    /// the type of NMS destination to create, using the "pubSubDomain" property.
    /// For other operations, this is not necessary.
    /// Point-to-Point (Queues) is the default domain.</para>
    ///
    /// <para>Default settings for NMS ISessions are "not transacted" and "auto-acknowledge".</para>
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
        #region Fields

        public static readonly long DEFAULT_RECEIVE_TIMEOUT = -1;


        private NmsTemplateResourceFactory transactionalResourceFactory;


        private object defaultDestination;

        private IMessageConverter messageConverter;


        private bool messageIdEnabled = true;

        private bool messageTimestampEnabled = true;

        private bool pubSubNoLocal = false;

        private long receiveTimeout = DEFAULT_RECEIVE_TIMEOUT;

        private bool explicitQosEnabled = false;

        private byte priority = NMSConstants.defaultPriority;
		
		private bool persistent = NMSConstants.defaultPersistence;
		
		private TimeSpan timeToLive;
		
        private NmsResources jmsResources = new NmsResources();
        
        #endregion

        #region Constructor (s)

        /// <summary> Create a new NmsTemplate.</summary>
        /// <remarks>
        /// <para>Note: The IConnectionFactory has to be set before using the instance.
        /// This constructor can be used to prepare a NmsTemplate via an ObjectFactory,
        /// typically setting the IConnectionFactory.</para>
        /// </remarks>
        public NmsTemplate()
        {
            transactionalResourceFactory = new NmsTemplateResourceFactory(this);
            InitDefaultStrategies();
        }


        /// <summary> Create a new NmsTemplate, given a IConnectionFactory.</summary>
        /// <param name="connectionFactory">the IConnectionFactory to obtain IConnections from
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
        /// NMS ISession.
        /// </summary>
        /// <remarks> Generalized version of <code>execute(ISessionCallback)</code>,
        /// allowing the NMS IConnection to be started on the fly.
        /// <p>Use <code>execute(ISessionCallback)</code> for the general case.
        /// Starting the NMS IConnection is just necessary for receiving messages,
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
                    ConnectionFactoryUtils.DoGetTransactionalSession(ConnectionFactory, transactionalResourceFactory);
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
            //TODO make sure don't want to do exception translation.
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


        //TODO check tibco support for message id...prob yes..
        /// <summary>
        /// Gets or sets a value indicating whether IMessageIds are.
        /// </summary>
        /// <value><c>true</c> if [message id enabled]; otherwise, <c>false</c>.</value>
        virtual public bool MessageIdEnabled
        {
            get { return messageIdEnabled; }

            set { messageIdEnabled = value; }
        }

        //TODO check tibco support for message id...prob yes.., so don't really need it then.
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
        /// Sets a value indicating whether message delivery should be persistent or non-persistent
        /// </summary>
        /// <remarks>
        /// This will set the delivery to persistent or non-persistent
        /// <p>Default it "true" aka delivery mode "PERSISTENT".</p>
        /// </remarks>
        /// <value><c>true</c> if [delivery persistent]; otherwise, <c>false</c>.</value>
        virtual public bool Persistent
        {
			get { return persistent; }
			
            set { persistent = value; }
        }


        //TODO verify admin...

        /// <summary>
        /// Gets or sets the priority when sending.
        /// </summary>
        /// <remarks>Since a default value may be defined administratively,
        /// this is only used when "isExplicitQosEnabled" equals "true".</remarks>
        /// <value>The priority.</value>
        virtual public byte Priority
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

        protected virtual object DoConvertFromMessage(IMessage message)
        {
            if (message != null)
            {
                return MessageConverter.FromMessage(message);
            }
            return null;
        }

        #region NMS Factory Methods

        /// <summary> Fetch an appropriate IConnection from the given NmsResourceHolder.
        /// </summary>
        /// <param name="holder">the NmsResourceHolder
        /// </param>
        /// <returns> an appropriate IConnection fetched from the holder,
        /// or <code>null</code> if none found
        /// </returns>
        protected virtual IConnection GetConnection(NmsResourceHolder holder)
        {
            return holder.GetConnection();
        }

        /// <summary> Fetch an appropriate ISession from the given NmsResourceHolder.
        /// </summary>
        /// <param name="holder">the NmsResourceHolder
        /// </param>
        /// <returns> an appropriate ISession fetched from the holder,
        /// or <code>null</code> if none found
        /// </returns>
        protected virtual ISession GetSession(NmsResourceHolder holder)
        {
            return holder.GetSession();
        }

        /// <summary> Create a NMS IMessageProducer for the given ISession and IDestination,
        /// configuring it to disable message ids and/or timestamps (if necessary).
        /// <p>Delegates to <code>doCreateProducer</code> for creation of the raw
        /// NMS IMessageProducer, which needs to be specific to NMS 1.1 or 1.0.2.</p>
        /// </summary>
        /// <param name="session">the NMS ISession to create a IMessageProducer for
        /// </param>
        /// <param name="destination">the NMS IDestination to create a IMessageProducer for
        /// </param>
        /// <returns> the new NMS IMessageProducer
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


        /// <summary> Create a raw NMS IMessageProducer for the given ISession and IDestination.
        /// <p>This implementation uses NMS 1.1 API.</p>
        /// </summary>
        /// <param name="session">the NMS ISession to create a IMessageProducer for
        /// </param>
        /// <param name="destination">the NMS IDestination to create a IMessageProducer for
        /// </param>
        /// <returns> the new NMS IMessageProducer
        /// </returns>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        protected virtual IMessageProducer DoCreateProducer(ISession session, IDestination destination)
        {
            return session.CreateProducer(destination);            
        }

        /// <summary> Create a NMS IMessageConsumer for the given ISession and IDestination.
        /// <p>This implementation uses NMS 1.1 API.</p>
        /// </summary>
        /// <param name="session">the NMS ISession to create a IMessageConsumer for
        /// </param>
        /// <param name="destination">the NMS IDestination to create a IMessageConsumer for
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

        //TODO refactor to not pass null as a 'switch' for behavior.
        
        protected internal virtual void DoSend(ISession session, IDestination destination, IMessageCreatorDelegate messageCreatorDelegate)
        {
            AssertUtils.ArgumentNotNull(messageCreatorDelegate, "IMessageCreatorDelegate must not be null");
            DoSend(session, destination, null, messageCreatorDelegate);
        }

        protected internal virtual void DoSend(ISession session, IDestination destination, IMessageCreator messageCreator)
        {
            AssertUtils.ArgumentNotNull(messageCreator, "IMessageCreator must not be null");
            DoSend(session, destination, messageCreator, null);
        }

        /// <summary> Send the given NMS message.</summary>
        /// <param name="session">the NMS ISession to operate on
        /// </param>
        /// <param name="destination">the NMS IDestination to send to
        /// </param>
        /// <param name="messageCreator">callback to create a NMS IMessage
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a NMS IMessage
        /// </param>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        protected internal virtual void DoSend(ISession session, IDestination destination, IMessageCreator messageCreator,
                                               IMessageCreatorDelegate messageCreatorDelegate)
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

                // Check commit - avoid commit call within a JTA transaction.
                if (session.Transacted && !TransactionSynchronizationManager.HasResource(ConnectionFactory))
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
        /// <param name="producer">the NMS IMessageProducer to send with
        /// </param>
        /// <param name="message">the NMS IMessage to send
        /// </param>
        /// <throws>  NMSException if thrown by NMS API methods </throws>
        protected virtual void DoSend(IMessageProducer producer, IMessage message)
        {
            if (ExplicitQosEnabled)
            {
                producer.Send(message, Persistent, Priority, TimeToLive);
            }
            else
            {
                producer.Send(message);
            }
        }


        #endregion

        #region INmsOperations Implementation

        /// <summary> Execute the action specified by the given action object within
        /// a NMS ISession.
        /// <p>Note: The value of isPubSubDomain affects the behavior of this method.
        /// If isPubSubDomain equals true, then a ISession is passed to the callback.
        /// If false, then a ISession is passed to the callback.</p>
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
        /// the NMS session and IMessageProducer in order to do more complex
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

        /// <summary> Send a message to the default destination.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public void SendWithDelegate(IMessageCreatorDelegate messageCreatorDelegate)
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
        /// The IMessageCreator callback creates the message given a ISession.
        /// </summary>
        /// <param name="destination">the destination to send this message to
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public void SendWithDelegate(IDestination destination, IMessageCreatorDelegate messageCreatorDelegate)
        {
            Execute(new SendDestinationCallback(this, destination, messageCreatorDelegate), false);
        }

        /// <summary> Send a message to the specified destination.
        /// The IMessageCreator callback creates the message given a ISession.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        public void SendWithDelegate(string destinationName, IMessageCreatorDelegate messageCreatorDelegate)
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
        /// The IMessageCreator callback creates the message given a ISession.
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
        /// The IMessageCreator callback creates the message given a ISession.
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
            return Execute(new ReceiveCallback(this, destination)) as IMessage;
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
            return Execute(new ReceiveCallback(this, destinationName)) as IMessage;
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
        
        protected virtual IMessage DoReceive(ISession session, IDestination destination, string messageSelector)
        {
            return DoReceive(session, CreateConsumer(session, destination, messageSelector));
        }
        
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
                                      ? consumer.Receive(new TimeSpan(timeout))
                                      : consumer.Receive();
                if (session.Transacted)
                {
                    // Commit necessary - but avoid commit call within a JTA transaction.
                    if (resourceHolder == null)
                    {
                        // Transacted session created by this template -> commit.
                        NmsUtils.CommitIfNecessary(session);
                    }
                }
                else if (ClientAcknowledge(session))
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

        /// <summary>
        /// Returns whether the ISession is in client acknowledgement mode.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>true ifin client ack mode, false otherwise</returns>
        protected virtual bool ClientAcknowledge(ISession session)
        {
            return (session.AcknowledgementMode == AcknowledgementMode.ClientAcknowledge);
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
            return DoConvertFromMessage(ReceiveSelected(messageSelector)); ;
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

            public NmsTemplate Enclosing_Instance
            {
                get { return enclosingTemplateInstance; }
            }

            public virtual IConnection GetConnection(NmsResourceHolder holder)
            {
                return Enclosing_Instance.GetConnection(holder);
            }

            public virtual ISession GetSession(NmsResourceHolder holder)
            {
                return Enclosing_Instance.GetSession(holder);
            }

            public virtual IConnection CreateConnection()
            {
                return Enclosing_Instance.CreateConnection();
            }

            public virtual ISession CreateSession(IConnection con)
            {
                return Enclosing_Instance.CreateSession(con);
            }

            public bool SynchedLocalTransactionAllowed
            {
                get { return Enclosing_Instance.SessionTransacted; }
            }
        }

        private class ProducerCreatorCallback : ISessionCallback
        {
            private NmsTemplate jmsTemplate;
            private IProducerCallback producerCallback;

            public ProducerCreatorCallback(NmsTemplate jmsTemplate, IProducerCallback producerCallback)
            {
                this.jmsTemplate = jmsTemplate;
				this.producerCallback = producerCallback;
            }

            public object DoInNms(ISession session)
            {
                IMessageProducer producer = jmsTemplate.CreateProducer(session, null);
                try
                {
                    return producerCallback.DoInNms(session, producer);
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

            public ConvertAndSendMessageCreator(NmsTemplate jmsTemplate, object message, IMessagePostProcessor messagePostProcessor)
            {
                this.jmsTemplate = jmsTemplate;
                objectToConvert = message;
                this.messagePostProcessor = messagePostProcessor;
            }

            public IMessage CreateMessage(ISession session)
            {
                IMessage msg = jmsTemplate.MessageConverter.ToMessage(objectToConvert, session);
                return messagePostProcessor.PostProcessMessage(msg);
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
    }

    /// <summary>
    /// This is a TIBCO specific class so that we can reuse connections, session, and
    /// message producers instead of creating/destroying them on each operation.
    /// </summary>
    internal class NmsResources
    {
        private IConnection connection;
        private ISession session;
        private IMessageProducer messageProducer;

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

        public IMessageProducer MessageProducer
        {
            get { return messageProducer; }
            set { messageProducer = value; }
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
        private IMessageCreatorDelegate messageCreatorDelegate;

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

        public SendDestinationCallback(NmsTemplate jmsTemplate, string destinationName, IMessageCreatorDelegate messageCreatorDelegate)
        {
            this.jmsTemplate = jmsTemplate;
            this.destinationName = destinationName;
            this.messageCreatorDelegate = messageCreatorDelegate;
        }

        public SendDestinationCallback(NmsTemplate jmsTemplate, IDestination destination, IMessageCreatorDelegate messageCreatorDelegate)
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
