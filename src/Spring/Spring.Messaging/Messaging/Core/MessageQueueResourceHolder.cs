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

using Spring.Transaction.Support;


#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Core
{
    /// <summary>
    /// MessageQueue resource holder, wrapping a MessageQueueTransaction.
    /// MessageQueueTransactionManager binds instances of this class to the thread.  
    /// </summary>
    /// <remarks>
    /// This is an SPI class, not intended to be used by applications.
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class MessageQueueResourceHolder : ResourceHolderSupport
    {
        private MessageQueueTransaction messageQueueTransaction;


        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueResourceHolder"/> class.
        /// </summary>
        /// <param name="messageQueueTransaction">The message queue transaction.</param>
        public MessageQueueResourceHolder(MessageQueueTransaction messageQueueTransaction)
        {
            this.messageQueueTransaction = messageQueueTransaction;
        }


        /// <summary>
        /// Gets or sets the message queue transaction.
        /// </summary>
        /// <value>The message queue transaction.</value>
        public MessageQueueTransaction MessageQueueTransaction
        {
            get { return messageQueueTransaction; }
            set { messageQueueTransaction = value; }
        }
    }
}