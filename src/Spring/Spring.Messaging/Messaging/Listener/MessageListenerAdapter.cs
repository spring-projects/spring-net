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
using Spring.Context;
using Spring.Expressions;
using Spring.Messaging.Core;
using Spring.Messaging.Support.Converters;
using Spring.Objects.Factory;
using Spring.Reflection.Dynamic;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

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

        /// <summary>
        /// Out-of-the-box value for the default listener method: "HandleMessage"
        /// </summary>
        public static readonly string ORIGINAL_DEFAULT_LISTENER_METHOD = "HandleMessage";

        #region Fields

        private IApplicationContext applicationContext;

        private object handlerObject;

        private string defaultHandlerMethod = ORIGINAL_DEFAULT_LISTENER_METHOD;

        private IExpression processingExpression;

        private string defaultResponseQueueName;

        private string messageConverterObjectName;

        private MessageQueueTemplate messageQueueTemplate;

        private IMessageQueueFactory messageQueueFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageListenerAdapter"/> class.
        /// </summary>
        public MessageListenerAdapter()
        {
            InitDefaultStrategies();
            handlerObject = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageListenerAdapter"/> class.
        /// </summary>
        /// <param name="handlerObject">The handler object.</param>
        public MessageListenerAdapter(object handlerObject)
        {
            InitDefaultStrategies();
            this.handlerObject = handlerObject;
        }

        #endregion

        /// <summary>
        /// Gets or sets the handler object to delegate message listening to.
        /// </summary>
        /// <remarks>
        /// Specified listener methods have to be present on this target object.
        /// If no explicit handler object has been specified, listener
        /// methods are expected to present on this adapter instance, that is,
        /// on a custom subclass of this adapter, defining listener methods.
        /// </remarks>
        /// <value>The handler object.</value>
        public object HandlerObject
        {
            get { return handlerObject; }
            set { handlerObject = value; }
        }

        /// <summary>
        /// Gets or sets the default handler method to delegate to,
        /// for the case where no specific listener method has been determined.
        /// Out-of-the-box value is "HandleMessage".
        /// </summary>
        /// <value>The default handler method.</value>
        public string DefaultHandlerMethod
        {
            get { return defaultHandlerMethod; }
            set {
                defaultHandlerMethod = value;
                ProcessingExpression = Expression.Parse(defaultHandlerMethod + "(#convertedObject)");
            }
        }

        /// <summary>
        /// Gets or sets the processing expression for use in custom subclasses
        /// </summary>
        /// <value>The processing expression.</value>
        protected IExpression ProcessingExpression
        {
            get { return processingExpression; }
            set { processingExpression = value; }
        }

        #region IInitializingObject Members

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// 	<p>
        /// Please do consult the class level documentation for the
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
        /// description of exactly <i>when</i> this method is invoked. In
        /// particular, it is worth noting that the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// callbacks will have been invoked <i>prior</i> to this method being
        /// called.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
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
            if (messageQueueTemplate == null)
            {
                messageQueueTemplate = new MessageQueueTemplate();
                messageQueueTemplate.ApplicationContext = ApplicationContext;
                messageQueueTemplate.AfterPropertiesSet();
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

        /// <summary>
        /// Gets or sets the message queue factory.
        /// </summary>
        /// <value>The message queue factory.</value>
        public IMessageQueueFactory MessageQueueFactory
        {
            get { return messageQueueFactory; }
            set { messageQueueFactory = value; }
        }

        /// <summary>
        /// Sets the name of the default response queue to send response messages to.
        /// This will be applied in case of a request message that does not carry a
        /// "ResponseQueue" value.
        /// <para>Alternatively, specify a response queue via the property
        /// <see cref="DefaultResponseQueue"/>.</para>
        /// </summary>
        /// <value>The name of the default response destination queue.</value>
        public string DefaultResponseQueueName
        {
            get { return defaultResponseQueueName; }
            set { defaultResponseQueueName = value; }
        }

        /// <summary>
        /// Sets the default destination to send response messages to. This will be applied
        /// in case of a request message that does not carry a "ResponseQueue" property
        /// Response destinations are only relevant for listener methods that return
        /// result objects, which will be wrapped in a response message and sent to a
        /// response destination.
        /// <para>
        /// Alternatively, specify a "DefaultResponseQueueName"
        /// to be dynamically resolved via the MessageQueueFactory.
        /// </para>
        /// </summary>
        /// <value>The default response destination.</value>
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
            }
        }


        /// <summary>
        /// Gets or sets the name of the message converter object used to resolved a <see cref="IMessageConverter"/>
        /// instance.
        /// </summary>
        /// <value>The name of the message converter object.</value>
        public string MessageConverterObjectName
        {
            get { return messageConverterObjectName; }
            set { messageConverterObjectName = value; }
        }

        /// <summary>
        /// Gets message converter that will convert incoming MSMQ messages to
        /// listener method arguments, and objects returned from listener
        /// methods back to MSMQ messages.
        /// </summary>
        /// <remarks>
        /// <para>The converter used is the one returned by CreateMessageConverter on MessageQueueFactory.
        /// </para>
        /// </remarks>
        /// <value>The message converter.</value>
        public IMessageConverter MessageConverter
        {
            get
            {
                return messageQueueFactory.CreateMessageConverter(MessageConverterObjectName);
            }
        }

        /// <summary>
        /// Sets the message queue template.
        /// </summary>
        /// <remarks>If not set, will create one for it own internal use whne MessageListenerAdapter is constructed.
        /// It maybe useful to share an existing instance if you have an extensively configured MessageQueueTemplate.
        /// </remarks>
        /// <value>The message queue template.</value>
        public MessageQueueTemplate MessageQueueTemplate
        {
            set { messageQueueTemplate = value; }
        }

        #region IMessageListener Members

        /// <summary>
        /// Called when message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void OnMessage(Message message)
        {
            object convertedMessage = ExtractMessage(message);

            //IDictionary vars = new Hashtable();
            //vars["convertedObject"] = convertedMessage;

            string methodName = GetHandlerMethodName(message, convertedMessage);
            object[] listenerArguments = BuildListenerArguments(convertedMessage);
            object result = InvokeListenerMethod(methodName, listenerArguments);


            //Invoke message handler method and get result.
            //object result = processingExpression.GetValue(handlerObject, vars);
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

        /// <summary>
        /// Gets the name of the handler method.
        /// </summary>
        /// <param name="originalMessage">The original message.</param>
        /// <param name="extractedMessage">The extracted message.</param>
        /// <returns>The name of the handler method.</returns>
        protected virtual string GetHandlerMethodName(Message originalMessage, object extractedMessage)
        {
            return DefaultHandlerMethod;
        }

        /// <summary>
        /// Builds an array of arguments to be passed into the taret listener method.
        /// </summary>
        /// <remarks>
        /// Allows for multiple method arguments to be built from a single message object.
        /// <p>The default implementation builds an array with the given message object
        /// as sole element. This means that the extracted message will always be passed
        /// into a <i>single</i> method argument, even if it is an array, with the target
        /// method having a corresponding single argument of the array's type declared.</p>
        /// <p>This can be overridden to treat special message content such as arrays
        /// differently, for example passing in each element of the message array
        /// as distinct method argument.</p>
        /// </remarks>
        /// <param name="convertedMessage">The converted message.</param>
        /// <returns>the array of arguments to be passed into the
        /// listener method (each element of the array corresponding
        /// to a distinct method argument)</returns>
        protected virtual object[] BuildListenerArguments(object convertedMessage)
        {
            return new object[] { convertedMessage };
        }

        /// <summary>
        /// Invokes the specified listener method.  This default implementation can only handle invoking a
        /// single argument method.
        /// </summary>
        /// <param name="methodName">Name of the listener method.</param>
        /// <param name="arguments">The arguments to be passed in. Only the first argument in the list is currently
        /// supported in this implementation.</param>
        /// <returns>The result returned from the listener method</returns>
        protected virtual object InvokeListenerMethod(string methodName, object[] arguments)
        {
            IDictionary<string,object> vars = new Dictionary<string, object>();
            vars["convertedObject"] = arguments[0];
            if (methodName.CompareTo(DefaultHandlerMethod) != 0)
            {
                //This is just to handle the case of overriding GetHandlerMethodName in a subclass and nothing else.
                return ExpressionEvaluator.GetValue(handlerObject, methodName + "(#convertedObject)", vars);
            }
            // the normal case of using the cached expression.
            return processingExpression.GetValue(handlerObject, vars);
        }

        /// <summary>
        /// Initialize the default implementations for the adapter's strategies.
        /// </summary>
        protected virtual void InitDefaultStrategies()
        {
            ProcessingExpression = Expression.Parse(defaultHandlerMethod + "(#convertedObject)");
        }

        /// <summary>
        /// Extracts the message body from the given message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>the content of the message, to be passed into the
        /// listener method as argument</returns>
        protected virtual object ExtractMessage(Message message)
        {
            IMessageConverter converter = MessageConverter;
            if (converter != null)
            {
                return converter.FromMessage(message);
            }
            return message;
        }

        /// <summary>
        /// Handles the result of a listener method.
        /// </summary>
        /// <param name="result">The result that was returned from listener.</param>
        /// <param name="request">The original request.</param>
        protected virtual void HandleResult(object result, Message request)
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

        /// <summary>
        /// Sends the given response message to the given destination.
        /// </summary>
        /// <param name="destination">The destination to send to.</param>
        /// <param name="response">The outgoing message about to be sent.</param>
        protected virtual void SendResponse(MessageQueue destination, Message response)
        {
            //Will send with appropriate transaction semantics
            if (logger.IsDebugEnabled)
            {
                logger.Debug("Sending response message to path = [" + destination.Path + "]");
            }
            messageQueueTemplate.Send(destination, response);
        }

        /// <summary>
        /// Builds a MSMQ message to be sent as response based on the given result object.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>the MSMQ <code>Message</code> (never <code>null</code>)</returns>
        /// <exception cref="MessagingException">If no messgae converter is specified.</exception>
        protected virtual Message BuildMessage(object result)
        {
            IMessageConverter converter = MessageConverter;
            if (converter != null)
            {
                // This is the default Message converter registered in QueueUtils.RegisterDefaultMessageConverter
                // and used by MessageQueueTemplate and the MessageListenerAdapter if no other Message converage is
                // set via the property MessageConverteryObjectName.
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

        /// <summary>
        /// Post-process the given response message before it will be sent. The default implementation
        /// sets the response's correlation id to the request message's correlation id.
        /// </summary>
        /// <param name="request">The original incoming message.</param>
        /// <param name="response">The outgoing MSMQ message about to be sent.</param>
        protected virtual void PostProcessResponse(Message request, Message response)
        {
            response.CorrelationId = request.CorrelationId;
        }

        /// <summary>
        /// Determine a response destination for the given message.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation first checks the MSMQ ResponseQueue
        ///  of the supplied request; if that is not <code>null</code>
        /// it is returned; if it is <code>null</code>, then the configured
        /// <see cref="DefaultResponseQueue"/> default response destination}
        /// is returned; if this too is <code>null</code>, then an
        /// <see cref="MessagingException"/>is thrown.
        /// </para>
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <returns></returns>
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
