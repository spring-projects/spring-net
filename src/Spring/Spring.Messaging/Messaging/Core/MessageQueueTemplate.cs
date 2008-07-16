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
using System.Messaging;
using Common.Logging;
using Spring.Context;
using Spring.Messaging.Support;
using Spring.Messaging.Support.Converters;
using Spring.Objects.Factory;

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
    /// that thread safe access to <see cref="System.Messaging.IMessageFormatter"/> instances are
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
        private IApplicationContext applicationContext;

        private TimeSpan timeout = MessageQueue.InfiniteTimeout;

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

        #region IApplicationContextAware Members


        /// <summary>
        /// Set the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>       
        public IApplicationContext ApplicationContext
        {
            get { return applicationContext; }
            set { applicationContext = value; }
        }

        #endregion

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
            if (DefaultMessageQueueObjectName == null)
            {
                throw new ArgumentException("DefaultMessageQueueObjectName is required.");
            }            
            if (MessageQueueFactory == null)
            {
                DefaultMessageQueueFactory mqf = new DefaultMessageQueueFactory();
                mqf.ApplicationContext = applicationContext;
                messageQueueFactory = mqf;
            }
            if (messageConverterObjectName == null)
            {
                messageConverterObjectName = QueueUtils.RegisterDefaultMessageConverter(applicationContext);
            }
        }

        #endregion

        #region IMessageQueueOperations Members

        public void ConvertAndSend(object obj)
        {
            CheckDefaultMessageQueue();
            ConvertAndSend(DefaultMessageQueueObjectName, obj);
        }

        public void ConvertAndSend(object obj, MessagePostProcessorDelegate messagePostProcessorDelegate)
        {
            CheckDefaultMessageQueue();
            ConvertAndSend(DefaultMessageQueueObjectName, obj, messagePostProcessorDelegate);
        }

        public void ConvertAndSend(string messageQueueObjectName, object obj)
        {
            Message msg = MessageConverter.ToMessage(obj);
            Send(MessageQueueFactory.CreateMessageQueue(messageQueueObjectName), msg);
        }

        public void ConvertAndSend(string messageQueueObjectName, object obj,
                                   MessagePostProcessorDelegate messagePostProcessorDelegate)
        {
            Message msg = MessageConverter.ToMessage(obj);
            Message msgToSend = messagePostProcessorDelegate(msg);
            Send(MessageQueueFactory.CreateMessageQueue(messageQueueObjectName), msgToSend);
        }

        public object ReceiveAndConvert()
        {
            MessageQueue mq = DefaultMessageQueue;
            Message m = mq.Receive(ReceiveTimeout);
            return DoConvertMessage(m);
        }

        public object ReceiveAndConvert(string messageQueueObjectName)
        {
            MessageQueue mq = MessageQueueFactory.CreateMessageQueue(messageQueueObjectName);
            Message m = mq.Receive(ReceiveTimeout);
            return DoConvertMessage(m);
        }

        public Message Receive()
        {
            return DefaultMessageQueue.Receive(ReceiveTimeout);
        }

        public Message Receive(string messageQueueObjectName)
        {
            return MessageQueueFactory.CreateMessageQueue(messageQueueObjectName).Receive(ReceiveTimeout);
        }

        public void Send(Message message)
        {
            Send(DefaultMessageQueue, message);
        }

        public void Send(string messageQueueObjectName, Message message)
        {
            Send(MessageQueueFactory.CreateMessageQueue(messageQueueObjectName), message);
        }

        public virtual void Send(MessageQueue messageQueue, Message message)
        {
            DoSend(messageQueue, message);
        }

        #endregion

        #region Protected Methods

        protected virtual void DoSend(MessageQueue messageQueue, Message message)
        {
            if (System.Transactions.Transaction.Current == null)
            {
                DoSendMessageQueueTransactional(messageQueue, message);
            }
            else
            {
                DoSendTxScope(messageQueue, message);
            }
        }

        protected virtual void DoSendMessageQueueTransactional(MessageQueue mq, Message msg)
        {
            MessageQueueTransaction transactionToUse = QueueUtils.GetMessageQueueTransaction(null);
            if (mq.Transactional)
            {
                // DefaultMessageQueue transaction is externally managed.
                if (transactionToUse != null)
                {
                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug(
                            "Sending messsage using externally managed MessageQueueTransction to transactional queue [" +
                            mq.QueueName + "].");
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
                    LOG.Warn("Sending message using implicit single-message transaction to transactional queue [" +
                             mq.QueueName + "].");
                    mq.Send(msg, MessageQueueTransactionType.Single);
                }
            }
            else
            {
                if (transactionToUse != null)
                {
                    LOG.Warn("Thread local message transaction ignored for sending to non-transactional queue.");
                    mq.Send(msg);
                }
                else
                {
                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Sending messsage without MSMQ transaction to non-TX-QUEUE.");
                    }
                    //Typical case, non TLS transaction, non-tx queue.
                    mq.Send(msg);
                }
            }
        }


        protected virtual void DoSendTxScope(MessageQueue mq, Message msg)
        {
            mq.Send(msg, MessageQueueTransactionType.Automatic);
        }

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