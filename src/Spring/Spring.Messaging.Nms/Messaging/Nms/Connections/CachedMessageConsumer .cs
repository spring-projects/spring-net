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
using Apache.NMS;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// NMS MessageConsumer decorator that adapts all calls
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
        /// Register for message events. 
        /// </summary>
        public event MessageListener Listener
        {
            add
            {
                target.Listener += value;
            }
            remove
            {
                target.Listener -= value;
            }
        }

        /// <summary>
        /// Receives the next message produced for this message consumer.
        /// </summary>
        /// <returns>the next message produced for this message consumer, , or null if this message consumer is concurrently closed</returns>
        public IMessage Receive()
        {
            return this.target.Receive();
        }

        /// <summary>
        /// Receives the next message that arrives within the specified timeout interval.
        /// </summary>
        /// <param name="timeout">The timeout value.</param>
        /// <returns>the next message produced for this message consumer, or null if the timeout expires or this message consumer is concurrently closed</returns>
        public IMessage Receive(TimeSpan timeout)
        {
            return this.target.Receive(timeout);
        }

        /// <summary>
        /// Receives the next message if one is immediately available.
        /// </summary>
        /// <returns>the next message produced for this message consumer, or null if one is not available</returns>
        public IMessage ReceiveNoWait()
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
        /// Dispose of wrapped MessageConsumer
        /// </summary>
        public void Dispose()
        {
            this.target.Dispose();
        }


        /// <summary>
        /// Description that shows this is a cached MessageConsumer
        /// </summary>
        /// <returns>Description that shows this is a cached MessageConsumer</returns>
        public override string ToString()
        {
            return "Cached NMS MessageConsumer: " + this.target;
        }
    }
}