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
    /// Exception handler called when an exception occurs during
    /// non-transactional receive processing.
    /// </summary>
    /// <remarks>
    /// A non-transactional receive will remove the message from the queue.  Non-transactional
    /// receivers do not suffer from poison messages since there is no redelivery by MSMQ.
    /// Typical actions to perform are to log the message or place it in another queue.
    /// If placed in another queue, another message listener container can be used to
    /// process the message later, assuming the root cause of the original exception is
    /// transient in nature.
    /// </remarks>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Called when an exception is thrown in listener processing.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        void OnException(Exception exception, Message message);
    }
}
