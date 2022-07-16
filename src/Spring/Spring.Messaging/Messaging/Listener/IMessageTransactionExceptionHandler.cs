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
    /// The exception handler within a transactional context.
    /// </summary>
    /// <remarks>
    /// The return value indicates to the invoker (typically a
    /// <see cref="TransactionalMessageListenerContainer"/>) whether the
    /// MessageTransaction that is associated with the delivery of this message
    /// should be rolled back (placing the message back on the transactional queue
    /// for redelivery) or commited (removing the message from the transactional queue)
    /// </remarks>
    /// <author>Mark Pollack</author>
    public interface IMessageTransactionExceptionHandler
    {
        /// <summary>
        /// Called when an exception is thrown during listener processing under the
        /// scope of a <see cref="MessageQueueTransaction"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageQueueTransaction">The message queue transaction.</param>
        /// <returns>An action indicating if the caller should commit or rollback the
        /// <see cref="MessageQueueTransaction"/>
        /// </returns>
        TransactionAction OnException(Exception exception, Message message,
                                      MessageQueueTransaction messageQueueTransaction);
    }
}
