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

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// Exception handler for use with DTC based message listener container. 
    /// such as <see cref="DistributedTxMessageListenerContainer"/>. 
    /// See <see cref="SendToQueueDistributedTransactionExceptionHandler"/> for 
    /// an implementation that detects poison messages and send them to another queue.
    /// </summary>
    public interface IDistributedTransactionExceptionHandler
    {
        /// <summary>
        /// Determines whether the incoming message is a poison message.  This method is
        /// called before the <see cref="IMessageListener"/> is invoked.
        /// </summary>
        /// <remarks>
        /// The <see cref="DistributedTxMessageListenerContainer"/> will call 
        /// <see cref="HandlePoisonMessage"/> if this method returns true and will
        /// then commit the distibuted transaction (removing the message from the queue).
        /// </remarks>
        /// <param name="message">The incoming message.</param>
        /// <returns>
        /// 	<c>true</c> if it is a poison message; otherwise, <c>false</c>.
        /// </returns>
        bool IsPoisonMessage(Message message);

        /// <summary>
        /// Handles the poison message. 
        /// </summary>
        /// <remarks>Typical implementations will move the message to another queue.
        /// The <see cref="DistributedTxMessageListenerContainer"/> will call this
        /// method while still within a DTC-based transaction.
        /// </remarks>
        /// <param name="poisonMessage">The poison message.</param>
        void HandlePoisonMessage(Message poisonMessage);

        /// <summary>
        /// Called when an exception is thrown in listener processing.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        void OnException(Exception exception, Message message);
    }
}
