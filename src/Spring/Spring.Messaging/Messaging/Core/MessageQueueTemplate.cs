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

using Spring.Context;
using Spring.Messaging.Support.Converters;
using Spring.Objects.Factory;
using Spring.Util;

using Common.Logging;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif


namespace Spring.Messaging.Core
{
    /// <summary>
    /// Helper class that simplifies MSMQ access code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Using the System.Messaging.MessageQueue class directly in application code has a number of
    /// shortcomings, namely that most operations are not thread safe (in particular Send) and
    /// IMessageFormatter classes are not thread safe either.
    /// </para>
    /// <para>
    /// The MessageQueueTemplate class overcomes these limitations letting you use a single instance
    /// of MessageQueueTemplate across multiple threads to perform standard MessageQueue opertations.
    /// Classes that are not thread safe are obtained and cached in thread local storage via an
    /// implementation of the <see cref="IMessageQueueFactory"/> interface, specifically
    /// <see cref="DefaultMessageQueueFactory"/>.
    /// </para>
    /// <para>
    /// You can access the thread local instance of the MessageQueue associated with this template
    /// via the Property DefaultMessageQueue.
    /// </para>
    /// <para>
    /// The template's Send methods will select an appropriate transaction delivery settings so
    /// calling code does not need to explicitly manage this responsibility themselves and thus
    /// allowing for greater portability of code across different, but common, transactional usage scenarios.
    /// </para>
    /// <para>A transactional send (either local or DTC transaction) will be
    /// attempted for a transacitonal queue, falling back to a single-transaction send
    /// to a transactional queue if there is not ambient Spring managed transaction.
    /// </para>
    /// <para>The overloaded ConvertAndSend and ReceiveAndConvert methods inherit the transactional
    /// semantics of the previously described Send method but more importantly, they help to ensure
    /// that thread safe access to <see cref="IMessageFormatter"/> instances are
    /// used as well as providing additional central location to put programmic logic that translates
    /// between the MSMQ Message object and the your business objects.  This for example is useful if you
    /// need to perform additional translation operations after calling a IMessageFormatter instance or
    /// want to directly extract and process the Message body contents.
    /// </para>
    /// </remarks>
    public class MessageQueueTemplate : IMessageQueueOperations, IInitializingObject, IApplicationContextAware
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (MessageQueueTemplate));

        #endregion

        #region Fields

        private string defaultMessageQueueObjectName;
        private string messageConverterObjectName;

        private IMessageQueueFactory messageQueueFactory;
        private IConfigurableApplicationContext applicationContext;

        private TimeSpan timeout = MessageQueue.InfiniteTimeout;

        private MessageQueueMetadataCache metadataCache;

        /// <summary>
        /// The name that is used from cache registration inside the application context.
        /// </summary>
        public const string METADATA_CACHE_NAME = "__MessageQueueMetadataCache__";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueTemplate"/> class.
        /// </summary>
        public MessageQueueTemplate()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueTemplate"/> class.
        /// </summary>
        /// <param name="messageQueueName">Name of the message queue as registered in the Spring container.</param>
        public MessageQueueTemplate(string messageQueueName)
        {
            defaultMessageQueueObjectName = messageQueueName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the message queue factory to use for creating MessageQueue and IMessageConverters.
        /// Default value is one that support thread local instances.
        /// </summary>
        /// <value>The message queue factory.</value>
        public IMessageQueueFactory MessageQueueFactory
        {
            get { return messageQueueFactory; }
            set { messageQueueFactory = value; }
        }

        /// <summary>
        /// Gets or sets the name of the default message queue as identified in the Spring container.
        /// </summary>
        /// <value>The name of the message queue as identified in the Spring container.</value>
        public string DefaultMessageQueueObjectName
        {
            get { return defaultMessageQueueObjectName; }
            set { defaultMessageQueueObjectName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the message converter object.  The name will be passed to
        /// the <see cref="IMessageQueueFactory"/> class to resolve it to an actual MessageQueue
        /// instance.
        /// </summary>
        /// <remarks>The default name is internally generated and will register an XmlMessageConverter
        /// that uses an <see cref="XmlMessageFormatter"/> and a simple System.String as its TargetType.</remarks>
        /// <value>The name of the message converter object.</value>
        public string MessageConverterObjectName
        {
            get { return messageConverterObjectName; }
            set { messageConverterObjectName = value; }
        }

        /// <summary>
        /// Gets the default message queue to be used on send/receive operations that do not
	    /// have a destination parameter.  The MessageQueue instance is resolved using
        /// the template's <see cref="IMessageQueueFactory"/>, the default implementaion
        /// <see cref="DefaultMessageQueueFactory"/> will return an unique instance per thread.
        /// </summary>
        /// <value>The default message queue.</value>
        public MessageQueue DefaultMessageQueue
        {
            get
            {
                return MessageQueueFactory.CreateMessageQueue(DefaultMessageQueueObjectName);
            }
        }


        /// <summary>
        /// Gets the message converter to use for this template. Used to resolve
        /// object parameters to ConvertAndSend methods and object results
        /// from ReceiveAndConvert methods.
        /// </summary>
        /// <remarks>
        /// The default
        /// </remarks>
        /// <value>The message converter.</value>
        public IMessageConverter MessageConverter
        {
            get
            {
                if (messageConverterObjectName == null)
                {
                    throw new InvalidOperationException(
                        "No MessageConverter registered. Check configuration of MessageQueueTemplate.");
                }
                return messageQueueFactory.CreateMessageConverter(MessageConverterObjectName);
            }
        }


        /// <summary>
        /// Gets or sets the receive timeout to be used on recieve operations.  Default value is
        /// MessageQueue.InfiniteTimeout (which is actually ~3 months).
        /// </summary>
        /// <value>The receive timeout.</value>
        public TimeSpan ReceiveTimeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// Gets or sets the metadata cache.
        /// </summary>
        /// <value>The metadata cache.</value>
        public MessageQueueMetadataCache MetadataCache
        {
            get { return metadataCache; }
            set { metadataCache = value; }
        }

        #endregion

        #region IApplicationContextAware Members

        /// <summary>
        /// Gets or sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Normally this call will be used to initialize the object.
        /// </p>
        /// <p>
        /// Invoked after population of normal object properties but before an
        /// init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// or a custom init-method. Invoked after the setting of any
        /// <see cref="Spring.Context.IResourceLoaderAware"/>'s
        /// <see cref="Spring.Context.IResourceLoaderAware.ResourceLoader"/>
        /// property.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// In the case of application context initialization errors.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If thrown by any application context methods.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
        public IApplicationContext ApplicationContext
        {
            get { return applicationContext; }
            set {
                AssertUtils.ArgumentNotNull(value, "An ApplicationContext instance is required");
                var ctx = value as IConfigurableApplicationContext;
                if (ctx == null)
                {
                    throw new InvalidOperationException(
                        "Implementations of IApplicationContext must also implement IConfigurableApplicationContext");
                }

                applicationContext = ctx;
            }
        }

        #endregion

        #region IInitializingObject Members


        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// Ensure that the DefaultMessageQueueObjectName property is set, creates
        /// a default implementation of the <see cref="IMessageQueueFactory"/> interface
        /// (<see cref="DefaultMessageQueueFactory"/>) that retrieves instances on a per-thread
        /// basis, and registers in the Spring container a default implementation of
        /// <see cref="IMessageConverter"/> (<see cref="XmlMessageConverter"/>) with a
        /// simple System.String as its TargetType.  <see cref="QueueUtils.RegisterDefaultMessageConverter"/>
        /// </remarks>
        public void AfterPropertiesSet()
        {
            if (MessageQueueFactory == null)
            {
                AssertUtils.ArgumentNotNull(applicationContext, "MessageQueueTemplate requires the ApplicationContext property to be set if the MessageQueueFactory property is not set for automatic create of the DefaultMessageQueueFactory");

                DefaultMessageQueueFactory mqf = new DefaultMessageQueueFactory();
                mqf.ApplicationContext = applicationContext;
                messageQueueFactory = mqf;
            }
            if (messageConverterObjectName == null)
            {
                messageConverterObjectName = QueueUtils.RegisterDefaultMessageConverter(applicationContext);
            }
            //If it has not been set by the user explicitly, then initialize.
            CreateDefaultMetadataCache();
        }

        /// <summary>
        /// Constructs the metadata cache with default options.
        /// </summary>
        protected virtual void CreateDefaultMetadataCache()
        {
            if (metadataCache == null)
            {
                if (applicationContext.ContainsObject(METADATA_CACHE_NAME))
                {
                    metadataCache = applicationContext.GetObject(METADATA_CACHE_NAME) as MessageQueueMetadataCache;
                }
                else
                {
                    metadataCache = new MessageQueueMetadataCache();
                    if (ApplicationContext != null)
                    {
                        metadataCache.ApplicationContext = ApplicationContext;
                        metadataCache.AfterPropertiesSet();
                        metadataCache.Initialize();
                        applicationContext.ObjectFactory.RegisterSingleton("__MessageQueueMetadataCache__",
                                                                           metadataCache);
                    }
                    else
                    {
                        #region Logging

                        if (LOG.IsWarnEnabled)
                        {
                            LOG.Warn(
                                "The ApplicationContext property has not been set, so the MessageQueueMetadataCache can not be automatically generated.  " +
                                "Please explictly set the MessageQueueMetadataCache using the property MetadataCache or set the ApplicationContext property.  " +
                                "This will only effect the use of MessageQueueTemplate when publishing to remote queues.");
                        }

                        #endregion
                    }
                }
            }
        }

        #endregion

        #region IMessageQueueOperations Members

        /// <summary>
        /// Send the given object to the default destination, converting the object
        /// to a MSMQ message with a configured IMessageConverter.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <remarks>This will only work with a default destination queue specified!</remarks>
        public void ConvertAndSend(object obj)
        {
            CheckDefaultMessageQueue();
            ConvertAndSend(DefaultMessageQueueObjectName, obj);
        }

        /// <summary>
        /// Send the given object to the default destination, converting the object
        /// to a MSMQ message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="obj">the object to convert to a message</param>
        /// <param name="messagePostProcessorDelegate">the callback to modify the message</param>
        /// <exception cref="MessagingException">if thrown by MSMQ API methods</exception>
        public void ConvertAndSend(object obj, MessagePostProcessorDelegate messagePostProcessorDelegate)
        {
            CheckDefaultMessageQueue();
            ConvertAndSend(DefaultMessageQueueObjectName, obj, messagePostProcessorDelegate);
        }

        /// <summary>
        /// Send the given object to the specified destination, converting the object
        /// to a MSMQ message with a configured <see cref="IMessageConverter"/> and resolving the
        /// destination name to a <see cref="MessageQueue"/> using a <see cref="IMessageQueueFactory"/>
        /// </summary>
        /// <param name="messageQueueObjectName">the name of the destination queue
        /// to send this message to (to be resolved to an actual MessageQueue
        /// by a IMessageQueueFactory)</param>
        /// <param name="obj">the object to convert to a message</param>
        /// <throws>NMSException if there is any problem</throws>
        public void ConvertAndSend(string messageQueueObjectName, object obj)
        {
            Message msg = MessageConverter.ToMessage(obj);
            Send(MessageQueueFactory.CreateMessageQueue(messageQueueObjectName), msg);
        }

        /// <summary>
        /// Send the given object to the specified destination, converting the object
        /// to a MSMQ message with a configured <see cref="IMessageConverter"/> and resolving the
        /// destination name to a <see cref="MessageQueue"/> with an <see cref="IMessageQueueFactory"/>
        /// The <see cref="MessagePostProcessorDelegate"/> callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="messageQueueObjectName">the name of the destination queue
        /// to send this message to (to be resolved to an actual MessageQueue
        /// by a IMessageQueueFactory)</param>
        /// <param name="obj">the object to convert to a message</param>
        /// <param name="messagePostProcessorDelegate">the callback to modify the message</param>
        /// <exception cref="MessagingException">if thrown by MSMQ API methods</exception>
        public void ConvertAndSend(string messageQueueObjectName, object obj,
                                   MessagePostProcessorDelegate messagePostProcessorDelegate)
        {
            Message msg = MessageConverter.ToMessage(obj);
            Message msgToSend = messagePostProcessorDelegate(msg);
            Send(MessageQueueFactory.CreateMessageQueue(messageQueueObjectName), msgToSend);
        }

        /// <summary>
        /// Receive and convert a message synchronously from the default message queue.
        /// </summary>
        /// <returns>The converted object</returns>
        /// <exception cref="MessageQueueException">if thrown by MSMQ API methods.  Note an
        /// exception will be thrown if the timeout of the syncrhonous recieve operation expires.
        /// </exception>
        public object ReceiveAndConvert()
        {
            MessageQueue mq = DefaultMessageQueue;
            Message m = mq.Receive(ReceiveTimeout);
            return DoConvertMessage(m);
        }

        /// <summary>
        /// Receives and convert a message synchronously from the specified message queue.
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <returns>the converted object</returns>
        /// <exception cref="MessageQueueException">if thrown by MSMQ API methods.  Note an
        /// exception will be thrown if the timeout of the syncrhonous recieve operation expires.
        /// </exception>
        public object ReceiveAndConvert(string messageQueueObjectName)
        {
            MessageQueue mq = MessageQueueFactory.CreateMessageQueue(messageQueueObjectName);
            Message m = mq.Receive(ReceiveTimeout);
            return DoConvertMessage(m);
        }

        /// <summary>
        /// Receives a message on the default message queue using the transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction.
        /// </summary>
        /// <returns>A message.</returns>
        public Message Receive()
        {
            return DefaultMessageQueue.Receive(ReceiveTimeout);
        }

        /// <summary>
        /// Receives  a message on the specified queue using the transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction.
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <returns></returns>
        public Message Receive(string messageQueueObjectName)
        {
            return MessageQueueFactory.CreateMessageQueue(messageQueueObjectName).Receive(ReceiveTimeout);
        }

        /// <summary>
        /// Sends the specified message to the default message queue using the
        /// transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(Message message)
        {
            Send(DefaultMessageQueue, message);
        }

        /// <summary>
        /// Sends the specified message to the message queue using the
        /// transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction.
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <param name="message">The message.</param>
        public void Send(string messageQueueObjectName, Message message)
        {
            Send(MessageQueueFactory.CreateMessageQueue(messageQueueObjectName), message);
        }

        /// <summary>
        /// Sends the specified message on the provided MessageQueue using the
        /// transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction.
        /// </summary>
        /// <param name="messageQueue">The DefaultMessageQueue to send a message to.</param>
        /// <param name="message">The message to send</param>
        /// <para>
        /// Note that it is the callers responsibility to ensure that the MessageQueue instance
        /// passed into this not being access simultaneously by other threads.
        /// </para>
        /// <remarks>A transactional send (either local or DTC transaction) will be
        /// attempted for a transacitonal queue, falling back to a single-transaction send
        /// to a transactional queue if there is not ambient Spring managed transaction.</remarks>
        public virtual void Send(MessageQueue messageQueue, Message message)
        {
            DoSend(messageQueue, message);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sends the message to the given message queue.
        /// </summary>
        /// <remarks>If System.Transactions.Transaction.Current is null, then send based on
        /// the transaction semantics of the queue definition.  See <see cref="DoSendMessageQueue"/> </remarks>
        /// <param name="messageQueue">The message queue.</param>
        /// <param name="message">The message.</param>
        protected virtual void DoSend(MessageQueue messageQueue, Message message)
        {
            if (System.Transactions.Transaction.Current == null)
            {
                DoSendMessageQueue(messageQueue, message);
            }
            else
            {
                DoSendTxScope(messageQueue, message);
            }
        }

        /// <summary>
        /// Send the message queue selecting the appropriate transactional delivery options.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the message queue is transactional and there is an ambient MessageQueueTransaction
        /// in thread local storage (put there via the use of Spring's MessageQueueTransactionManager
        /// or TransactionalMessageListenerContainer), the message will be sent transactionally using the
        /// MessageQueueTransaction object in thread local storage. This lets you group together multiple
        /// messaging operations within the same transaction without having to explicitly pass around the
        /// MessageQueueTransaction object.
        /// </para>
        /// <para>
        /// If the message queue is transactional but there is no ambient MessageQueueTransaction,
        /// then a single message transaction is created on each messaging operation.
        /// (MessageQueueTransactionType = Single).
        /// </para>
        /// <para>
        /// If there is an ambient System.Transactions transaction then that transaction will
        /// be used (MessageQueueTransactionType = Automatic).
        /// </para>
        /// <para>
        /// If the queue is not transactional, then a non-transactional send
        /// (MessageQueueTransactionType = None) is used.
        /// </para>
        /// </remarks>
        /// <param name="mq">The mq.</param>
        /// <param name="msg">The MSG.</param>
        protected virtual void DoSendMessageQueue(MessageQueue mq, Message msg)
        {
            MessageQueueTransaction transactionToUse = QueueUtils.GetMessageQueueTransaction(null);
            if (metadataCache != null)
            {
                MessageQueueMetadata mqMetadata = metadataCache.Get(mq.Path);
                if (mqMetadata != null)
                {
                    if (mqMetadata.RemoteQueue)
                    {
                        if (mqMetadata.RemoteQueueIsTransactional)
                        {
                            // DefaultMessageQueue transaction is externally managed.
                            DoSendMessageTransaction(mq, transactionToUse, msg);
                            return;
                        }
                        DoSendMessageQueueNonTransactional(mq, transactionToUse, msg);
                        return;
                    }
                }
            } else
            {
                if (LOG.IsWarnEnabled)
                {
                    LOG.Warn("MetadataCache has not been initialized.  Set the MetadataCache explicitly in standalone usage and/or " +
                             "configure the MessageQueueTemplate in an ApplicationContext.  If deployed in an ApplicationContext by default " +
                             "the MetadataCache will automaticaly populated.");
                }
            }
            // Handle assuming these are local queues.

            if (mq.Transactional)
            {
                // DefaultMessageQueue transaction is externally managed.
                DoSendMessageTransaction(mq, transactionToUse, msg);
            }
            else
            {
                DoSendMessageQueueNonTransactional(mq, transactionToUse, msg);
            }
        }

        /// <summary>
        /// Does the send message transaction.
        /// </summary>
        /// <param name="mq">The mq.</param>
        /// <param name="transactionToUse">The transaction to use.</param>
        /// <param name="msg">The MSG.</param>
        protected virtual void DoSendMessageTransaction(MessageQueue mq, MessageQueueTransaction transactionToUse, Message msg)
        {
            if (transactionToUse != null)
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug(
                        "Sending messsage using externally managed MessageQueueTransction to transactional queue with path [" + mq.Path + "].");
                }
                mq.Send(msg, transactionToUse);
            }
            else
            {
                /* From MSDN documentation
                         * If a non-transactional message is sent to a transactional queue,
                         * this component creates a single-message transaction for it,
                         * except in the case of referencing a queue on a remote computer
                         * using a direct format name. In this situation, if you do not specify a
                         * transaction context when sending a message, one is not created for you
                         * and the message will be sent to the dead-letter queue.*/
                LOG.Warn("Sending message using implicit single-message transaction to transactional queue queue with path [" + mq.Path + "].");
                mq.Send(msg, MessageQueueTransactionType.Single);
            }
        }

        /// <summary>
        /// Does the send message queue non transactional.
        /// </summary>
        /// <param name="mq">The mq.</param>
        /// <param name="transactionToUse">The transaction to use.</param>
        /// <param name="msg">The MSG.</param>
        protected virtual void DoSendMessageQueueNonTransactional(MessageQueue mq, MessageQueueTransaction transactionToUse, Message msg)
        {
            if (transactionToUse != null)
            {
                LOG.Warn("Thread local message transaction ignored for sending to non-transactional queue with path [" + mq.Path + "].");
                mq.Send(msg);
            }
            else
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Sending messsage without MSMQ transaction to non-transactional queue with path [" + mq.Path + "].");
                }
                //Typical case, non TLS transaction, non-tx queue.
                mq.Send(msg);
            }
        }


        /// <summary>
        /// Sends using MessageQueueTransactionType.Automatic transaction type
        /// </summary>
        /// <param name="mq">The message queue.</param>
        /// <param name="msg">The message.</param>
        protected virtual void DoSendTxScope(MessageQueue mq, Message msg)
        {
            mq.Send(msg, MessageQueueTransactionType.Automatic);
        }

        /// <summary>
        /// Template method to convert the message if it is not null.
        /// </summary>
        /// <param name="m">The message.</param>
        /// <returns>The converted message ,or null if no message converter is set.</returns>
        protected virtual object DoConvertMessage(Message m)
        {
            if (m != null)
            {
                return MessageConverter.FromMessage(m);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if the default message queue if defined.
        /// </summary>
        protected virtual void CheckDefaultMessageQueue()
        {
            if (DefaultMessageQueueObjectName == null)
            {
                throw new SystemException("No DefaultMessageQueueObjectName specified. Check configuration of MessageQueueTemplate.");
            }
        }

        #endregion
    }
}
