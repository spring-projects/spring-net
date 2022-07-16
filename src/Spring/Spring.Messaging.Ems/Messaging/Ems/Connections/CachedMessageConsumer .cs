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
    /// EMS MessageConsumer decorator that adapts all calls
    /// to a shared MessageConsumer instance underneath.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET) </author>
    public class CachedMessageConsumer : IMessageConsumer
    {
        private IMessageConsumer target;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedMessageConsumer"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public CachedMessageConsumer(IMessageConsumer target)
        {
            this.target = target;
        }


        /// <summary>
        /// Gets the target MessageConsumer, the consumer we are 'wrapping'
        /// </summary>
        /// <value>The target MessageConsumer.</value>
        public IMessageConsumer Target
        {
            get { return target; }
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EMSMessageHandler MessageHandler
        {
            add
            {
                target.MessageHandler += value;
            }
            remove
            {
                target.MessageHandler -= value;
            }
        }


        public MessageConsumer NativeMessageConsumer
        {
            get { return target.NativeMessageConsumer; }
        }

        public IMessageListener MessageListener
        {
            get { return target.MessageListener; }
            set { target.MessageListener = value; }
        }

        public string MessageSelector
        {
            get { return target.MessageSelector; }
        }

        /// <summary>
        /// Receives the next message produced for this message consumer.
        /// </summary>
        /// <returns>the next message produced for this message consumer, , or null if this message consumer is concurrently closed</returns>
        public Message Receive()
        {
            return this.target.Receive();
        }

        /// <summary>
        /// Receives the next message that arrives within the specified timeout interval.
        /// </summary>
        /// <param name="timeout">The timeout value.</param>
        /// <returns>the next message produced for this message consumer, or null if the timeout expires or this message consumer is concurrently closed</returns>
        public Message Receive(long timeout)
        {
            return this.target.Receive(timeout);
        }

        /// <summary>
        /// Receives the next message if one is immediately available.
        /// </summary>
        /// <returns>the next message produced for this message consumer, or null if one is not available</returns>
        public Message ReceiveNoWait()
        {
            return this.target.ReceiveNoWait();
        }

        /// <summary>
        /// No-op implementation since it is caching.
        /// </summary>
        public void Close()
        {
            // It's a cached MessageConsumer...
        }




        /// <summary>
        /// Description that shows this is a cached MessageConsumer
        /// </summary>
        /// <returns>Description that shows this is a cached MessageConsumer</returns>
        public override string ToString()
        {
            return "Cached EMS MessageConsumer: " + this.target;
        }
    }
}
