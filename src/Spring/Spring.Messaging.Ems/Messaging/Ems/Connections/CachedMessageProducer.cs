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

using Spring.Messaging.Ems.Common;

namespace Spring.Messaging.Ems.Connections
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

        private object originalDisableMessageID;

        private object originalDisableMessageTimestamp;

        private MessageDeliveryMode deliveryMode;

        private int priority;

        private long timeToLive;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedMessageProducer"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public CachedMessageProducer(IMessageProducer target)
        {
            this.target = target;
            deliveryMode = target.MsgDeliveryMode;
            priority = target.Priority;
            timeToLive = target.TimeToLive;
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
        /// Gets or sets the producer's default delivery mode.
        /// </summary>
        /// <value>The message delivery mode for this message producer</value>
        public int DeliveryMode
        {
            get { return (int)this.deliveryMode; }
            set { this.deliveryMode = (MessageDeliveryMode) value; }
        }

        /// <summary>
        /// Gets or sets the MSG delivery mode.
        /// </summary>
        /// <value>The MSG delivery mode.</value>
        public MessageDeliveryMode MsgDeliveryMode
        {
            get { return this.deliveryMode; }
            set { this.deliveryMode = value; }
        }

        /// <summary>
        /// Gets or sets the priority of messages sent with this producer.
        /// </summary>
        /// <value>The priority.</value>
        public int Priority
        {
            get { return priority; }
            set { priority = value;}
        }

        /// <summary>
        /// Gets or sets the the default length of time in milliseconds from its dispatch time
        /// that a produced message should be retained by the message system.
        /// </summary>
        /// <remarks>Time to live is set to zero by default.</remarks>
        /// <value>The message time to live in milliseconds; zero is unlimited</value>
        public long TimeToLive
        {
            get { return timeToLive; }
            set { timeToLive = value; }
        }

        public Destination Destination
        {
            get { return target.Destination; }
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
        public void Send(Message message)
        {
            target.Send(message, this.deliveryMode, this.priority, this.timeToLive);
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="deliveryMode">The delivery mode.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="timeToLive">The time to live.</param>
        public void Send(Message message, int deliveryMode, int priority, long timeToLive)
        {
            target.Send(message, deliveryMode, priority, timeToLive);
        }

        /// <summary>
        /// Sends a message to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="message">The message.</param>
        public void Send(Destination destination, Message message)
        {
            target.Send(destination, message, this.deliveryMode, this.priority, this.timeToLive);
        }

        public void Send(Message message, MessageDeliveryMode deliveryMode, int priority, long timeToLive)
        {
            target.Send(message, deliveryMode, priority, timeToLive);
        }

        public void Send(Destination dest, Message message, int deliveryMode, int priority, long timeToLive)
        {
            target.Send(dest, message, deliveryMode, priority, timeToLive);
        }

        public void Send(Destination dest, Message message, MessageDeliveryMode deliveryMode, int priority, long timeToLive)
        {
            target.Send(dest, message, deliveryMode, priority, timeToLive);
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

        public MessageProducer NativeMessageProducer
        {
            get { return target.NativeMessageProducer; }
        }

        /// <summary>
        /// Returns string indicated this is a wrapped MessageProducer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Cached EMS MessageProducer: " + this.target;
        }
    }
}
