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
using System.Collections;
using System.Messaging;
using Common.Logging;
using Spring.Context;
using Spring.Expressions;
using Spring.Messaging.Core;
using Spring.Messaging.Support;
using Spring.Messaging.Support.Converters;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.Reflection.Dynamic;

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// Message listener adapter that delegates the handling of messages to target
    /// listener methods via reflection <see cref="DynamicReflectionManager"/>, 
    /// with flexible message type conversion.
    /// Allows listener methods to operate on message content types, completely
    /// independent from the MSMQ API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, the content of incoming MSMQ messages gets extracted before
    /// being passed into the target handler method, to let the target method
    /// operate on message content types such as String or business object instead of
    /// <see cref="Message"/>. Message type conversion is delegated to a Spring
    /// <see cref="IMessageConverter"/>  By default, an <see cref="XmlMessageConverter"/> 
    /// with TargetType set to System.String is used.  If you do not want such automatic 
    /// message conversion taking place, then be sure to set the
    /// MessageConverter property to null.
    /// </para>
    /// <para>
    /// If a target handler method returns a non-null object (for example, with a
    /// message content type such as <code>String</code>), it will get
    /// wrapped in a MSMQ <code>Message</code> and sent to the response destination
    /// (either using the MSMQ Message.ResponseQueue property or 
    /// <see cref="DefaultResponseQueue"/>) specified default response queue
    /// destination).
    /// </para>
    /// <para>
    /// Find below some examples of method signatures compliant with this adapter class.
    /// This first example uses the default <see cref="XmlMessageConverter"/> that can
    /// marhsall/unmarshall string values from the MSMQ Message.
    /// </para>
    /// <example>
    /// public interface IMyHandler
    /// {
    ///   void HandleMessage(string text);
    /// }
    /// </example>
    /// <para>
    /// The next example indicates a similar method signature but the name of the
    /// handler method name has been changed to "DoWork", using the property
    /// <see cref="DefaultHandlerMethod"/>
    /// </para>
    /// <example>
    /// public interface IMyHandler
    /// {
    ///   void DoWork(string text);
    /// }
    /// </example>
    /// <para>If your <see cref="IMessageConverter"/> implementation will return multiple object
    /// types, overloading the handler method is perfectly acceptible, the most specific matching
    /// method will be used.  A method with an object signature would be consider a 
    /// 'catch-all' method
    /// </para>
    /// <example>
    /// public interface IMyHandler
    /// {
    ///   void DoWork(string text);
    ///   void DoWork(OrderRequest orderRequest);
    ///   void DoWork(InvoiceRequest invoiceRequest);
    ///   void DoWork(object obj);
    /// }
    /// </example>   
    /// <para>
    /// The last example shows how to send a message to the ResponseQueue for those
    /// methods that do not return void.
    /// <example>
    /// public interface MyHandler
    /// {
    ///   string DoWork(string text);
    ///   OrderResponse DoWork(OrderRequest orderRequest);
    ///   InvoiceResponse DoWork(InvoiceRequest invoiceRequest);
    ///   void DoWork(object obj);
    /// }
    /// </example>
    /// </para>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class MessageListenerAdapter : IMessageListener, IApplicationContextAware, IInitializingObject
    {
        #region Logging

        private static readonly ILog logger = LogManager.GetLogger(typeof (MessageListenerAdapter));

        #endregion

        private IApplicationContext applicationContext;

        private object handlerObject;

        private string defaultHandlerMethod = "HandleMessage";

        private IExpression processingExpression;

        private string defaultResponseQueueName;

        private string messageConverterObjectName;

        private MessageQueueTemplate messageQueueTemplate;

        private IMessageQueueFactory messageQueueFactory;


        public MessageListenerAdapter()
        {
            handlerObject = this;
            processingExpression = Expression.Parse(defaultHandlerMethod + "(#convertedObject)");
            messageQueueTemplate = new MessageQueueTemplate();
        }

        public MessageListenerAdapter(object handlerObject)
        {
            this.handlerObject = handlerObject;
        }

        public object HandlerObject
        {
            get { return handlerObject; }
            set { handlerObject = value; }
        }

        public string DefaultHandlerMethod
        {
            get { return defaultHandlerMethod; }
            set { defaultHandlerMethod = value; }
        }

        #region IInitializingObject Members

        public void AfterPropertiesSet()
        {
            if (messageQueueFactory == null)
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
            set { applicationContext = value; }
        }

        #endregion

        public IMessageQueueFactory MessageQueueFactory
        {
            get { return messageQueueFactory; }
            set { messageQueueFactory = value; }
        }

        public string DefaultResponseQueueName
        {
            get { return defaultResponseQueueName; }
            set { defaultResponseQueueName = value; }
        }

        public MessageQueue DefaultResponseQueue
        {
            get
            {
                if (DefaultResponseQueueName != null)
                {
                    return messageQueueFactory.CreateMessageQueue(DefaultResponseQueueName);
                }
                else
                {
                    return null;
                }
                /*
                DefaultMessageQueue mq = LogicalThreadContext.GetData(CURRENT_RESPONSEQUEUE_SLOTNAME) as DefaultMessageQueue;
                if (mq == null)
                {
                    mq = ApplicationContext.GetObject(DefaultResponseQueueName) as DefaultMessageQueue;
                    LogicalThreadContext.SetData(CURRENT_RESPONSEQUEUE_SLOTNAME, mq);
                }
                return mq;
                 */
            }
        }


        public string MessageConverterObjectName
        {
            get { return messageConverterObjectName; }
            set { messageConverterObjectName = value; }
        }

        public IMessageConverter MessageConverter
        {
            get
            {
                return messageQueueFactory.CreateMessageConverter(MessageConverterObjectName);
                /*
                if (messageConverter == null)
                {
                    throw new InvalidOperationException("No MessageConverter registered. Check configuration of MessageQueueTemplate.");
                }
                IMessageConverter mc = LogicalThreadContext.GetData(CURRENT_CONVERTER_SLOTNAME) as IMessageConverter;
                if (mc == null)
                {
                    mc = messageConverter.Clone() as IMessageConverter;
                    LogicalThreadContext.SetData(CURRENT_CONVERTER_SLOTNAME, mc);
                }
                return mc;*/
            }
        }


        protected virtual string GetHandlerMethodName(Message originalMessage, object extractedMessage)
        {
            return DefaultHandlerMethod;
        }

        #region IMessageListener Members

        public virtual void OnMessage(Message message)
        {
            object convertedMessage = ExtractMessage(message);

            IDictionary vars = new Hashtable();
            vars["convertedObject"] = convertedMessage;

            //Need to parse each time since have overloaded methods and
            //expression processor caches target of first invocation.
            //TODO - use regular reflection.
            processingExpression = Expression.Parse(defaultHandlerMethod + "(#convertedObject)");

            //Invoke message handler method and get result.
            object result = processingExpression.GetValue(handlerObject, vars);
            if (result != null)
            {
                HandleResult(result, message);
            }
            else
            {
                logger.Debug("No result object given - no result to handle");
            }
        }

        #endregion

        protected virtual object ExtractMessage(Message message)
        {
            IMessageConverter converter = MessageConverter;
            if (converter != null)
            {
                return converter.FromMessage(message);
            }
            return message;
        }

        private void HandleResult(object result, Message request)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug("Listener method returned result [" + result +
                             "] - generating response message for it");
            }
            Message response = BuildMessage(result);
            PostProcessResponse(request, response);
            MessageQueue destination = GetResponseDestination(request, response);
            SendResponse(destination, response);
        }

        protected virtual void SendResponse(MessageQueue destination, Message response)
        {
            //Will send with appropriate transaction semantics 
            messageQueueTemplate.Send(destination, response);
        }

        protected virtual Message BuildMessage(object result)
        {
            IMessageConverter converter = MessageConverter;
            if (converter != null)
            {
                if (messageConverterObjectName.Equals("__XmlMessageConverter__"))
                {
                    return converter.ToMessage(result.ToString());
                }
                else
                {
                    return converter.ToMessage(result);
                }
            }
            else
            {
                Message msg = result as Message;
                if (msg == null)
                {
                    throw new MessagingException("No MessageConverter specified - cannot handle message [" + result +
                                                 "]");
                }
                return msg;
            }
        }

        protected virtual void PostProcessResponse(Message request, Message response)
        {
            response.CorrelationId = request.CorrelationId;
        }

        protected virtual MessageQueue GetResponseDestination(Message request, Message response)
        {
            MessageQueue replyTo = request.ResponseQueue;
            if (replyTo == null)
            {
                replyTo = DefaultResponseQueue;
                if (replyTo == null)
                {
                    throw new MessagingException("Cannot determine response destination: " +
                                                 "Request message does not contain ResponseQueue destination, and no default response queue set.");
                }
            }
            return replyTo;
        }
    }
}