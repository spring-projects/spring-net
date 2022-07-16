using System.Reflection;
using Common.Logging;
using Spring.Expressions;
using Spring.Messaging.Ems.Common;
using Spring.Messaging.Ems.Support;
using Spring.Messaging.Ems.Support.Converter;
using Spring.Messaging.Ems.Support.Destinations;
using Spring.Util;

namespace Spring.Messaging.Ems.Listener.Adapter
{
    /// <summary>
    /// Message listener adapter that delegates the handling of messages to target
    /// listener methods via reflection, with flexible message type conversion.
    /// Allows listener methods to operate on message content types, completely
    /// independent from the EMS API.
    /// </summary>
    /// <remarks>
    /// <para>By default, the content of incoming messages gets extracted before
    /// being passed into the target listener method, to let the target method
    /// operate on message content types such as String or byte array instead of
    /// the raw Message. Message type conversion is delegated to a Spring
    /// <see cref="IMessageConverter"/>. By default, a <see cref="SimpleMessageConverter"/>
    /// will be used. (If you do not want such automatic message conversion taking
    /// place, then be sure to set the <see cref="MessageConverter"/> property
    /// to <code>null</code>.)
    /// </para>
    /// <para>If a target listener method returns a non-null object (typically of a
    /// message content type such as <code>String</code> or byte array), it will get
    /// wrapped in a EMS <code>Message</code> and sent to the response destination
    /// (either the EMS "reply-to" destination or the <see cref="defaultResponseDestination"/>
    /// specified.
    /// </para>
    /// <para>
    /// The sending of response messages is only available when
    /// using the <see cref="ISessionAwareMessageListener"/> entry point (typically through a
    /// Spring message listener container). Usage as standard EMS MessageListener
    /// does <i>not</i> support the generation of response messages.
    /// </para>
    /// <para>Consult the reference documentation for examples of method signatures compliant with this
    /// adapter class.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class MessageListenerAdapter : IMessageListener, ISessionAwareMessageListener
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof (MessageListenerAdapter));

        #endregion

        /// <summary>
        /// The default handler method name.
        /// </summary>
        public static string ORIGINAL_DEFAULT_HANDLER_METHOD = "HandleMessage";

        #region Fields

        private object handlerObject;

        private string defaultHandlerMethod = ORIGINAL_DEFAULT_HANDLER_METHOD;

        private IExpression processingExpression;

        private object defaultResponseDestination;

        private IDestinationResolver destinationResolver = new DynamicDestinationResolver();

        private IMessageConverter messageConverter;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageListenerAdapter"/> class with default settings.
        /// </summary>
        public MessageListenerAdapter()
        {
            InitDefaultStrategies();
            handlerObject = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageListenerAdapter"/> class for the given handler object
        /// </summary>
        /// <param name="handlerObject">The delegate object.</param>
        public MessageListenerAdapter(object handlerObject)
        {
            InitDefaultStrategies();
            this.handlerObject = handlerObject;
        }

        #endregion

        #region Properties

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
	    /// Out-of-the-box value is <see cref="ORIGINAL_DEFAULT_HANDLER_METHOD"/> ("HandleMessage"}.
        /// </summary>
        /// <value>The default handler method.</value>
        public string DefaultHandlerMethod
        {
            get { return defaultHandlerMethod; }
            set
            {
                defaultHandlerMethod = value;
                processingExpression = Expression.Parse(defaultHandlerMethod + "(#convertedObject)");
            }
        }


        /// <summary>
        /// Sets the default destination to send response messages to. This will be applied
	    /// in case of a request message that does not carry a "JMSReplyTo" field.
	    /// Response destinations are only relevant for listener methods that return
	    /// result objects, which will be wrapped in a response message and sent to a
	    /// response destination.
	    /// <para>
	    /// Alternatively, specify a "DefaultResponseQueueName" or "DefaultResponseTopicName",
	    /// to be dynamically resolved via the DestinationResolver.
        /// </para>
        /// </summary>
        /// <value>The default response destination.</value>
        public object DefaultResponseDestination
        {
            set { defaultResponseDestination = value; }
        }

        /// <summary>
        /// Sets the name of the default response queue to send response messages to.
	    /// This will be applied in case of a request message that does not carry a
	    /// "EMSReplyTo" field.
	    /// <para>Alternatively, specify a JMS Destination object as "defaultResponseDestination".</para>
	    /// </summary>
        /// <value>The name of the default response destination queue.</value>
        public string DefaultResponseQueueName
        {
            set { defaultResponseDestination = new DestinationNameHolder(value, false); }
        }

        /// <summary>
        /// Sets the name of the default response topic to send response messages to.
	    /// This will be applied in case of a request message that does not carry a
	    /// "ReplyTo" field.
	    /// <para>Alternatively, specify a JMS Destination object as "defaultResponseDestination".</para>
        /// </summary>
        /// <value>The name of the default response destination topic.</value>
        public string DefaultResponseTopicName
        {
            set { defaultResponseDestination = new DestinationNameHolder(value, true); }
        }


        /// <summary>
        /// Gets or sets the destination resolver that should be used to resolve response
	    /// destination names for this adapter.
	    /// <para>The default resolver is a <see cref="DynamicDestinationResolver"/>.
	    /// Specify another implementation, for other strategies, perhaps from a directory service.</para>
        /// </summary>
        /// <value>The destination resolver.</value>
        public IDestinationResolver DestinationResolver
        {
            get { return destinationResolver; }
            set
            {
                AssertUtils.ArgumentNotNull(value, "DestinationResolver must not be null");
                destinationResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the message converter that will convert incoming JMS messages to
	    /// listener method arguments, and objects returned from listener
	    /// methods back to EMS messages.
	    /// </summary>
	    /// <remarks>
	    /// <para>The default converter is a {@link SimpleMessageConverter}, which is able
	    /// to handle BytesMessages}, TextMessages, MapMessages, and ObjectMessages.
	    /// </para>
        /// </remarks>
        /// <value>The message converter.</value>
        public IMessageConverter MessageConverter
        {
            get { return messageConverter; }
            set { messageConverter = value; }
        }

        #endregion


        /// <summary>
	    /// Standard JMS {@link MessageListener} entry point.
	    /// <para>Delegates the message to the target listener method, with appropriate
	    /// conversion of the message arguments
	    /// </para>
	    /// </summary>
	    /// <remarks>
	    /// In case of an exception, the <see cref="HandleListenerException"/> method will be invoked.
	    /// <b>Note</b>
	    /// Does not support sending response messages based on
	    /// result objects returned from listener methods. Use the
	    /// <see cref="ISessionAwareMessageListener"/> entry point (typically through a Spring
	    /// message listener container) for handling result objects as well.
        /// </remarks>
        /// <param name="message">The incoming message.</param>
        public void OnMessage(Message message)
        {
            try
            {
                OnMessage(message, null);
            }
            catch (Exception e)
            {
                HandleListenerException(e);
            }
        }

        /// <summary>
	    /// Spring <see cref="ISessionAwareMessageListener"/> entry point.
	    /// <para>
	    /// Delegates the message to the target listener method, with appropriate
	    /// conversion of the message argument. If the target method returns a
	    /// non-null object, wrap in a EMS message and send it back.
	    /// </para>
        /// </summary>
        /// <param name="message">The incoming message.</param>
        /// <param name="session">The session to operate on.</param>
        public void OnMessage(Message message, ISession session)
        {
            if (handlerObject != this)
            {
                if (typeof(ISessionAwareMessageListener).IsInstanceOfType(handlerObject))
                {
                    if (session != null)
                    {
                        ((ISessionAwareMessageListener)handlerObject).OnMessage(message, session);
                        return;
                    }
                    else if (!typeof(IMessageListener).IsInstanceOfType(handlerObject))
                    {
                        throw new InvalidOperationException("MessageListenerAdapter cannot handle a " +
                            "ISessionAwareMessageListener delegate if it hasn't been invoked with a Session itself");
                    }
                }
                if (typeof(IMessageListener).IsInstanceOfType(handlerObject))
                {
                    ((IMessageListener)handlerObject).OnMessage(message);
                    return;
                }
            }

            // Regular case: find a handler method reflectively.
            object convertedMessage = ExtractMessage(message);


            IDictionary<string, object> vars = new Dictionary<string, object>();
            vars["convertedObject"] = convertedMessage;

            //Invoke message handler method and get result.
            object result;
            try
            {
                result = processingExpression.GetValue(handlerObject, vars);
            }
            catch (EMSException)
            {
                throw;
            }
            // Will only happen if dynamic method invocation falls back to standard reflection.
            catch (TargetInvocationException ex)
            {
                Exception targetEx = ex.InnerException;
                if (ObjectUtils.IsAssignable(typeof(EMSException), targetEx))
                {
                    throw ReflectionUtils.UnwrapTargetInvocationException(ex);
                }
                else
                {
                    throw new ListenerExecutionFailedException("Listener method '" + defaultHandlerMethod + "' threw exception", targetEx);
                }
            }
            catch (Exception ex)
            {
                throw new ListenerExecutionFailedException("Failed to invoke target method '" + defaultHandlerMethod +
                                                           "' with argument " + convertedMessage, ex);
            }

            if (result != null)
            {
                HandleResult(result, message, session);
            }
            else
            {
                logger.Debug("No result object given - no result to handle");
            }
        }

        /// <summary>
        /// Initialize the default implementations for the adapter's strategies.
        /// </summary>
        protected virtual void InitDefaultStrategies()
        {
            MessageConverter = new SimpleMessageConverter();
            processingExpression = Expression.Parse(defaultHandlerMethod + "(#convertedObject)");
        }

        /// <summary>
        /// Handle the given exception that arose during listener execution.
	    /// The default implementation logs the exception at error level.
	    /// <para>This method only applies when used as standard EMS MessageListener.
	    /// In case of the Spring <see cref="ISessionAwareMessageListener"/> mechanism,
	    /// exceptions get handled by the caller instead.
	    /// </para>
        /// </summary>
        /// <param name="ex">The exception to handle.</param>
        protected virtual void HandleListenerException(Exception ex)
        {
            logger.Error("Listener execution failed", ex);
        }

        /// <summary>
        /// Extract the message body from the given message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>the content of the message, to be passed into the
        /// listener method as argument</returns>
        /// <exception cref="EMSException">if thrown by EMS API methods</exception>
        private object ExtractMessage(Message message)
        {
            IMessageConverter converter = MessageConverter;
            if (converter != null)
            {
                return converter.FromMessage(message);
            }
            return message;
        }

        /// <summary>
        /// Gets the name of the listener method that is supposed to
	    /// handle the given message.
	    /// The default implementation simply returns the configured
	    /// default listener method, if any.
        /// </summary>
        /// <param name="originalMessage">The EMS request message.</param>
        /// <param name="extractedMessage">The converted JMS request message,
        /// to be passed into the listener method as argument.</param>
        /// <returns>the name of the listener method (never <code>null</code>)</returns>
        /// <exception cref="EMSException">if thrown by EMS API methods</exception>
        protected virtual string GetHandlerMethodName(Message originalMessage, object extractedMessage)
        {
            return DefaultHandlerMethod;
        }

        /// <summary>
        /// Handles the given result object returned from the listener method, sending a response message back.
        /// </summary>
        /// <param name="result">The result object to handle (never <code>null</code>).</param>
        /// <param name="request">The original request message.</param>
        /// <param name="session">The session to operate on (may be <code>null</code>).</param>
        protected virtual void HandleResult(object result, Message request, ISession session)
        {
            if (session != null)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Listener method returned result [" + result +
                                 "] - generating response message for it");
                }
                Message response = BuildMessage(session, result);
                PostProcessResponse(request, response);
                Destination destination = GetResponseDestination(request, response, session);
                SendResponse(session, destination, response);
            }
            else
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Listener method returned result [" + result +
                                 "]: not generating response message for it because of no EMS Session given");
                }
            }
        }

        /// <summary>
        /// Builds a JMS message to be sent as response based on the given result object.
        /// </summary>
        /// <param name="session">The JMS Session to operate on.</param>
        /// <param name="result">The content of the message, as returned from the listener method.</param>
        /// <returns>the JMS <code>Message</code> (never <code>null</code>)</returns>
        /// <exception cref="MessageConversionException">If there was an error in message conversion</exception>
        /// <exception cref="EMSException">if thrown by EMS API methods</exception>
        protected virtual Message BuildMessage(ISession session, Object result)
        {
            IMessageConverter converter = MessageConverter;
            if (converter != null)
            {
                return converter.ToMessage(result, session);
            }
            else
            {
                Message msg = result as Message;
                if (msg == null)
                {
                    throw new MessageConversionException(
                        "No IMessageConverter specified - cannot handle message [" + result + "]");
                }
                return msg;
            }
        }

        /// <summary>
        /// Post-process the given response message before it will be sent. The default implementation
        /// sets the response's correlation id to the request message's correlation id.
        /// </summary>
        /// <param name="request">The original incoming message.</param>
        /// <param name="response">The outgoing JMS message about to be sent.</param>
        /// <exception cref="EMSException">if thrown by EMS API methods</exception>
        protected virtual void PostProcessResponse(Message request, Message response)
        {
            response.CorrelationID = request.CorrelationID;
        }

        /// <summary>
	    /// Determine a response destination for the given message.
	    /// </summary>
	    /// <remarks>
	    /// <para>The default implementation first checks the JMS Reply-To
	    /// Destination of the supplied request; if that is not <code>null</code>
	    /// it is returned; if it is <code>null</code>, then the configured
	    /// <see cref="DefaultResponseDestination"/> default response destination}
	    /// is returned; if this too is <code>null</code>, then an
	    /// <see cref="InvalidDestinationException"/>is thrown.
	    /// </para>
        /// </remarks>
        /// <param name="request">The original incoming message.</param>
        /// <param name="response">The outgoing message about to be sent.</param>
        /// <param name="session">The session to operate on.</param>
        /// <returns>the response destination (never <code>null</code>)</returns>
        /// <exception cref="EMSException">if thrown by EMS API methods</exception>
        /// <exception cref="InvalidDestinationException">if no destination can be determined.</exception>
        protected virtual Destination GetResponseDestination(Message request, Message response, ISession session)
        {
            Destination replyTo = request.ReplyTo;
            if (replyTo == null)
            {
                replyTo = ResolveDefaultResponseDestination(session);
                if (replyTo == null)
                {
                    throw new InvalidDestinationException("Cannot determine response destination: " +
                            "Request message does not contain reply-to destination, and no default response destination set.");
                }
            }
            return replyTo;
        }

        /// <summary>
        /// Resolves the default response destination into a Destination, using this
	    /// accessor's <see cref="DestinationResolver"/> in case of a destination name.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <returns>The located destination</returns>
        protected virtual Destination ResolveDefaultResponseDestination(ISession session)
        {
            Destination dest = defaultResponseDestination as Destination;
            if (dest != null)
            {
                return dest;
            }

            DestinationNameHolder destNameHolder = defaultResponseDestination as DestinationNameHolder;
            if (destNameHolder != null)
            {
                return DestinationResolver.ResolveDestinationName(session, destNameHolder.Name, destNameHolder.IsTopic);
            }

            return null;
        }

        /// <summary>
        /// Sends the given response message to the given destination.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="destination">The destination to send to.</param>
        /// <param name="response">The outgoing message about to be sent.</param>
        protected virtual void SendResponse(ISession session, Destination destination, Message response)
        {
            IMessageProducer producer = session.CreateProducer(destination);
            try
            {
                PostProcessProducer(producer, response);
                producer.Send(response);
            }
            finally
            {
                EmsUtils.CloseMessageProducer(producer);
            }
        }

        /// <summary>
        /// Post-process the given message producer before using it to send the response.
        /// The default implementation is empty.
        /// </summary>
        /// <param name="producer">The producer that will be used to send the message.</param>
        /// <param name="response">The outgoing message about to be sent.</param>
        protected virtual void PostProcessProducer(IMessageProducer producer, Message response)
        {

        }
    }

    /// <summary>
    /// Internal class combining a destination name and its target destination type (queue or topic).
    /// </summary>
    internal class DestinationNameHolder
    {
        private readonly string name;

        private readonly bool isTopic;

        public DestinationNameHolder(string name, bool isTopic)
        {
            this.name = name;
            this.isTopic = isTopic;
        }


        public string Name
        {
            get { return name; }
        }

        public bool IsTopic
        {
            get { return isTopic; }
        }
    }
}
