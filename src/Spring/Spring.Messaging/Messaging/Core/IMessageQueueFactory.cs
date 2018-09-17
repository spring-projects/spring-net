#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using Spring.Messaging.Support;
using Spring.Messaging.Support.Converters;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Core
{
    /// <summary>
    /// An interface for creating MessageQueue and IMessageConverter objects from object definitions 
    /// defined in the application context.
    /// </summary>
    /// <remarks>
    /// MessageQueue and IMessageConverter objects have methods that are generally not thread safe, 
    /// (IMessageConverter classes rely on IMessageFormatter objects that are not thread safe). 
    /// As such, a major reason to for this interface is to provide thread-local instances such that 
    /// appliation code need not be concerned with these resource management issues.
    /// </remarks>
    /// <author>Mark Pollack</author>
    public interface IMessageQueueFactory
    {
        /// <summary>
        /// Registers the message queue, its creation specified via the factory method 
        /// MessageQueueCreatorDelegate, with the provided name in the application context 
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <param name="messageQueueCreatorDelegate">The message queue creator delegate.</param>
        void RegisterMessageQueue(string messageQueueObjectName,
                                  MessageQueueCreatorDelegate messageQueueCreatorDelegate);

        /// <summary>
        /// Creates the message queue given its name in the application context.
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <returns>A MessageQueue instance configured via the application context</returns>
        MessageQueue CreateMessageQueue(string messageQueueObjectName);

        /// <summary>
        /// Determines whether the application context contains the message queue object definition.
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <returns>
        /// 	<c>true</c> if the application context contains the specified message queue object name; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsMessageQueue(string messageQueueObjectName);



        /// <summary>
        /// Registers the message converter, its creation specified via the factory method
        /// MessageConverterCreatorDelegate, with the provided name in the application context.
        /// </summary>
        /// <param name="messageConverterName">Name of the message converter.</param>
        /// <param name="MessageConverterCreatorDelegate">The message converter creator delegate.</param>
        void RegisterMessageConverter(string messageConverterName,
                                      MessageConverterCreatorDelegate MessageConverterCreatorDelegate);


        /// <summary>
        /// Creates the message converter given its name in the application context.
        /// </summary>
        /// <param name="messageConverterObjectName">Name of the message converter object.</param>
        /// <returns>A IMessageConverter instance configured via the application context</returns>
        IMessageConverter CreateMessageConverter(string messageConverterObjectName);

        /// <summary>
        /// Determines whether the application context contains the message queue object definition.
        /// </summary>
        /// <param name="messageConverterObjectName">Name of the message converter object.</param>
        /// <returns>
        /// 	<c>true</c> if the application context contains the specified message message converter object name; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsMessageConverter(string messageConverterObjectName);



    }
}