using System;
using System.Collections;
using Common.Logging;
using Spring.Expressions;
using Spring.Messaging.Nms.Support;
using Spring.Messaging.Nms.Support.Converter;
using Spring.Messaging.Nms.Support.IDestinations;
using Spring.Util;
using Apache.NMS;

namespace Spring.Messaging.Nms.Listener.Adapter
{
    public class MessageListenerAdapter : IMessageListener
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof (MessageListenerAdapter));

        #endregion

        private object delegateObject;

        private string defaultListenerMethod = "HandleMessage";

        private IExpression processingExpression;

        private object defaultResponseDestination;

        private IDestinationResolver destinationResolver = new DynamicDestinationResolver();

        private IMessageConverter messageConverter;

        public MessageListenerAdapter()
        {
            InitDefaultStrategies();
            delegateObject = this;
            processingExpression = Spring.Expressions.Expression.Parse(defaultListenerMethod + "(#convertedObject)");
        }

        public MessageListenerAdapter(object delegateObject)
        {
            InitDefaultStrategies();
            this.delegateObject = delegateObject;
        }

        // TODO name change?

        public object DelegateObject
        {
            get { return delegateObject; }
            set { delegateObject = value; }
        }

        public string DefaultListenerMethod
        {
            get { return defaultListenerMethod; }
            set
            {
                defaultListenerMethod = value;
            }
        }


        public object DefaultResponseDestination
        {
            set { defaultResponseDestination = value; }
        }

        public string DefaultResponseDestinationQueueName
        {
            set { defaultResponseDestination = new DestinationNameHolder(value, false); }
        }

        public string DefaultResponseDestinationTopicName
        {
            set { defaultResponseDestination = new DestinationNameHolder(value, true); }
        }


        public IDestinationResolver DestinationResolver
        {
            get { return destinationResolver; }
            set
            {
                AssertUtils.ArgumentNotNull(value, "DestinationResolver must not be null");
                destinationResolver = value;
            }
        }

        public IMessageConverter MessageConverter
        {
            get { return messageConverter; }
            set { messageConverter = value; }
        }

        private void InitDefaultStrategies()
        {
            MessageConverter = new SimpleMessageConverter();
        }

        protected virtual void HandleListenerException(Exception e)
        {
            logger.Error("Listener execution failed", e);
        }

        protected virtual string GetListenerMethodName(IMessage originalIMessage, object extractedMessage)
        {
            return DefaultListenerMethod;
        }


        public void OnMessage(IMessage message)
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

        public void OnMessage(IMessage message, ISession session)
        {
            object convertedMessage = ExtractIMessage(message);


            IDictionary vars = new Hashtable();
            vars["convertedObject"] = convertedMessage;

            //Need to parse each time since have overloaded methods and
            //expression processor caches target of first invocation.
            //TODO - use regular reflection.
            processingExpression = Expression.Parse(defaultListenerMethod + "(#convertedObject)");
            
            //Invoke message handler method and get result.
            object result = processingExpression.GetValue(delegateObject, vars);
            if (result != null)
            {
                HandleResult(result, message, session);
            }
            else
            {
                logger.Debug("No result object given - no result to handle");
            }
        }

        private void HandleResult(object result, IMessage request, ISession session)
        {
            if (session != null)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Listener method returned result [" + result +
                                 "] - generating response message for it");
                }
                IMessage response = BuildMessage(session, result);
                PostProcessResponse(request, response);
                IDestination destination = GetResponseDestination(request, response, session);
                SendResponse(session, destination, response);
            }
            else
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Listener method returned result [" + result +
                                 "]: not generating response message for it because of no NMS ISession given");
                }
            }
        }

        protected virtual IMessage BuildMessage(ISession session, Object result)
        {
            IMessageConverter converter = MessageConverter;
            if (converter != null)
            {
                return converter.ToMessage(result, session);
            }
            else
            {
                IMessage msg = result as IMessage;
                if (msg == null)
                {
                    throw new IMessageConversionException(
                        "No IMessageConverter specified - cannot handle message [" + result + "]");
                }
                return msg;
            }
        }

        protected virtual void PostProcessResponse(IMessage request, IMessage response)
        {
            response.NMSCorrelationID = request.NMSCorrelationID;
        }
        
        protected virtual IDestination GetResponseDestination(IMessage request, IMessage response, ISession session)
        {
            IDestination replyTo = request.NMSReplyTo;
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
        
        protected virtual IDestination ResolveDefaultResponseDestination(ISession session)
        {
            IDestination dest = defaultResponseDestination as IDestination;
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
        
        protected virtual void SendResponse(ISession session, IDestination destination, IMessage response)
        {
            IMessageProducer producer = session.CreateProducer(destination);
            try
            {
                PostProcessProducer(producer, response);
                producer.Send(response);
            }
            finally
            {
                NmsUtils.CloseMessageProducer(producer);
            }
        }
        
        protected virtual void 	PostProcessProducer(IMessageProducer producer, IMessage response)
        {
            
        }
        
        
        private object ExtractIMessage(IMessage message)
        {
            IMessageConverter converter = MessageConverter;
            if (converter != null)
            {
                return converter.FromMessage(message);
            }
            return message;
        }
    }

    internal class DestinationNameHolder
    {
        private string name;

        private bool isTopic;

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
