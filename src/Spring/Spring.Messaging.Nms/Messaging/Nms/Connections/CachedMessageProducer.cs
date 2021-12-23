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

using Apache.NMS;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// MessageProducer decorator that adapts calls to a shared MessageProducer
    /// instance underneath, managing QoS settings locally within the decorator.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class CachedMessageProducer : IMessageProducer
    {
        private readonly IMessageProducer target;

        private object originalDisableMessageID = null;

        private object originalDisableMessageTimestamp = null;

        private MsgDeliveryMode msgDeliveryMode;

        private MsgPriority priority;

        private TimeSpan timeToLive;

        private TimeSpan requestTimeout;


        /// <summary>
        /// Initializes a new instance of the <see cref="CachedMessageProducer"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public CachedMessageProducer(IMessageProducer target)
        {
            this.target = target;
            this.msgDeliveryMode = target.DeliveryMode;
            this.priority = target.Priority;
            this.timeToLive = target.TimeToLive;
        }


        /// <summary>
        /// Gets the target MessageProducer, the producer we are 'wrapping'
        /// </summary>
        /// <value>The target MessageProducer.</value>
        public IMessageProducer Target
        {
            get { return target; }
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Send(IMessage message)
        {
            target.Send(message);
        }

        /// <summary>
        /// Sends a message to the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="deliveryMode">The QOS to use for sending <see cref="msgDeliveryMode"/>.</param>
        /// <param name="priority">The message priority.</param>
        /// <param name="timeToLive">The time to live.</param>
        public void Send(IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
           target.Send(message, deliveryMode, priority, timeToLive);
        }

        /// <summary>
        /// Sends a message to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="message">The message.</param>
        public void Send(IDestination destination, IMessage message)
        {
            target.Send(destination, message);
        }

        /// <summary>
        /// Sends a message the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="deliveryMode">The QOS to use for sending <see cref="msgDeliveryMode"/>.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="timeToLive">The time to live.</param>
        public void Send(IDestination destination, IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
            target.Send(destination, message, deliveryMode, priority, timeToLive);
        }

        public TimeSpan DeliveryDelay
        {
            get { return target.DeliveryDelay; }
            set { target.DeliveryDelay = value; }
        }
        
        public Task SendAsync(IMessage message)
        {
            return target.SendAsync(message);
        }

        public Task SendAsync(IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
            return target.SendAsync(message, deliveryMode, priority, timeToLive);
        }

        public Task SendAsync(IDestination destination, IMessage message)
        {
            return target.SendAsync(destination, message);
        }

        public Task SendAsync(IDestination destination, IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
            return target.SendAsync(destination, message, deliveryMode, priority, timeToLive);
        }

        #region Odd Message Creationg Methods on IMessageProducer - not in-line with JMS APIs.
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <returns>A new message</returns>
        public IMessage CreateMessage()
        {
            return target.CreateMessage();
        }

        public Task<IMessage> CreateMessageAsync()
        {
            return target.CreateMessageAsync();
        }

        /// <summary>
        /// Creates the text message.
        /// </summary>
        /// <returns>A new text message.</returns>
        public ITextMessage CreateTextMessage()
        {
            return target.CreateTextMessage();
        }

        public Task<ITextMessage> CreateTextMessageAsync()
        {
            return target.CreateTextMessageAsync();
        }

        /// <summary>
        /// Creates the text message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A texst message with the given text.</returns>
        public ITextMessage CreateTextMessage(string text)
        {
            return target.CreateTextMessage(text);
        }

        public Task<ITextMessage> CreateTextMessageAsync(string text)
        {
            return target.CreateTextMessageAsync(text);
        }

        /// <summary>
        /// Creates the map message.
        /// </summary>
        /// <returns>a new map message.</returns>
        public IMapMessage CreateMapMessage()
        {
            return target.CreateMapMessage();
        }

        public Task<IMapMessage> CreateMapMessageAsync()
        {
            return target.CreateMapMessageAsync();
        }

        /// <summary>
        /// Creates the object message.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>A new object message with the given body.</returns>
        public IObjectMessage CreateObjectMessage(object body)
        {
            return target.CreateObjectMessage(body);
        }

        public Task<IObjectMessage> CreateObjectMessageAsync(object body)
        {
            return target.CreateObjectMessageAsync(body);
        }

        /// <summary>
        /// Creates the bytes message.
        /// </summary>
        /// <returns>A new bytes message.</returns>
        public IBytesMessage CreateBytesMessage()
        {
            return target.CreateBytesMessage();
        }

        public Task<IBytesMessage> CreateBytesMessageAsync()
        {
            return target.CreateBytesMessageAsync();
        }

        /// <summary>
        /// Creates the bytes message.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>A new bytes message with the given body.</returns>
        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            return target.CreateBytesMessage(body);
        }

        public Task<IBytesMessage> CreateBytesMessageAsync(byte[] body)
        {
            return target.CreateBytesMessageAsync(body);
        }

        /// <summary>
        /// Creates the stream message.
        /// </summary>
        /// <returns>A new stream message.</returns>
        public IStreamMessage CreateStreamMessage()
        {
            return target.CreateStreamMessage();
        }

        public Task<IStreamMessage> CreateStreamMessageAsync()
        {
            return target.CreateStreamMessageAsync();
        }


        /// <summary>
        /// A delegate that is called each time a Message is sent from this Producer which allows
        /// the application to perform any needed transformations on the Message before it is sent.
        /// The Session instance sets the delegate on each Producer it creates.
        /// </summary>
        /// <value></value>
        public ProducerTransformerDelegate ProducerTransformer
        {
            get { return target.ProducerTransformer; }
            set { target.ProducerTransformer = value; }
        }

        #endregion

        /// <summary>
        /// Gets or sets a value indicating what DeliveryMode this <see cref="CachedMessageProducer"/> 
        /// should use, for example a persistent QOS
        /// </summary>
        /// <value><see cref="MsgDeliveryMode"/></value>
        public MsgDeliveryMode DeliveryMode
        {
            get { return msgDeliveryMode; }
            set { msgDeliveryMode = value; }
        }

        /// <summary>
        /// Gets or sets the time to live value for messages sent with this producer.
        /// </summary>
        /// <value>The time to live.</value>
        public TimeSpan TimeToLive
        {
            get { return timeToLive; }
            set { timeToLive = value; }
        }


        /// <summary>
        /// Gets or sets the request timeout for the message producer.
        /// </summary>
        /// <value>The request timeout.</value>
        public TimeSpan RequestTimeout
        {
            get { return requestTimeout; }
            set { requestTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the priority of messages sent with this producer.
        /// </summary>
        /// <value>The priority.</value>
        public MsgPriority Priority
        {
            get { return priority; }
            set { priority = value;}
        }

        /// <summary>
        /// Gets or sets a value indicating whether disable setting of the message ID property.
        /// </summary>
        /// <value><c>true</c> if disable message ID setting; otherwise, <c>false</c>.</value>
        public bool DisableMessageID
        {
            get
            {
                return target.DisableMessageID;
            }
            set
            {
                if (originalDisableMessageID == null)
                {
                    originalDisableMessageID = target.DisableMessageID;
                }
                target.DisableMessageID = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether disable setting the message timestamp property.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if disable message timestamp; otherwise, <c>false</c>.
        /// </value>
        public bool DisableMessageTimestamp
        {
            get
            {
                return target.DisableMessageTimestamp;
            }
            set
            {
                if (originalDisableMessageTimestamp == null)
                {
                    originalDisableMessageTimestamp = target.DisableMessageTimestamp;
                }
                target.DisableMessageTimestamp = value;
            }
        }

        /// <summary>
        /// Reset properties.
        /// </summary>
        public void Close()
        {
            // It's a cached MessageProducer... reset properties only.
            if (originalDisableMessageID != null)
            {
                target.DisableMessageID = (bool) originalDisableMessageID;
                originalDisableMessageID = null;
            }
            if (originalDisableMessageTimestamp != null)
            {
                target.DisableMessageTimestamp = (bool) originalDisableMessageTimestamp;
                originalDisableMessageTimestamp = null;
            }
        }
        
        public Task CloseAsync()
        {
            Close();
            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            target.Dispose();
        }


        /// <summary>
        /// Returns string indicated this is a wrapped MessageProducer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Cached NMS MessageProducer: " + this.target;
        }
    }
}