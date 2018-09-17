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

using Spring.Messaging.Support.Converters;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Core
{
    /// <summary>
    /// Specifies a basic set of helper MSMQ opertions.
    /// </summary>
    /// <remarks>
    /// <para>Implemented by <see cref="MessageQueueTemplate"/>.  Not often used but a useful option
    /// to enhance testability, as it can easily be mocked or stubbed.
    /// </para>
    /// <para>
    /// Provides <code>MessageQueueTemplate's</code> <code>
    /// Send(..)</code> and <code>receive(..)</code> methods that mirror various MSMQ MessageQueue
    /// API methods.
    /// </para>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public interface IMessageQueueOperations
    {
        /// <summary>
        /// Send the given object to the default destination, converting the object
        /// to a MSMQ message with a configured IMessageConverter.
        /// </summary>
        /// <remarks>This will only work with a default destination queue specified!</remarks>
        /// <param name="obj">The obj.</param>
        void ConvertAndSend(object obj);

        /// <summary> Send the given object to the default destination, converting the object
        /// to a MSMQ message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="obj">the object to convert to a message
        /// </param>
        /// <param name="messagePostProcessorDelegate">the callback to modify the message
        /// </param>
        /// <exception cref="MessagingException">if thrown by MSMQ API methods</exception>    
        void ConvertAndSend(object obj, MessagePostProcessorDelegate messagePostProcessorDelegate);

        /// <summary> Send the given object to the specified destination, converting the object
        /// to a MSMQ message with a configured <see cref="IMessageConverter"/> and resolving the 
        /// destination name to a <see cref="MessageQueue"/> using a <see cref="IMessageQueueFactory"/>
        /// </summary>
        /// <param name="messageQueueObjectName">the name of the destination queue
        /// to send this message to (to be resolved to an actual MessageQueue
        /// by a IMessageQueueFactory)
        /// </param>
        /// <param name="obj">the object to convert to a message
        /// </param>
        /// <throws>NMSException if there is any problem</throws>
        void ConvertAndSend(string messageQueueObjectName, object obj);

        /// <summary> Send the given object to the specified destination, converting the object
        /// to a MSMQ message with a configured <see cref="IMessageConverter"/> and resolving the 
        /// destination name to a <see cref="MessageQueue"/> with an <see cref="IMessageQueueFactory"/>
        /// The <see cref="MessagePostProcessorDelegate"/> callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="messageQueueObjectName">the name of the destination queue
        /// to send this message to (to be resolved to an actual MessageQueue
        /// by a IMessageQueueFactory)
        /// </param>
        /// <param name="obj">the object to convert to a message
        /// </param>
        /// <param name="messagePostProcessorDelegate">the callback to modify the message
        /// </param>
        /// <exception cref="MessagingException">if thrown by MSMQ API methods</exception>    
        void ConvertAndSend(string messageQueueObjectName, object obj, MessagePostProcessorDelegate messagePostProcessorDelegate);

        /*
        void ConvertAndSend(QueueIdentifierType queueIdentifierType, string destinationValue, string messageQueueObjectName, object obj);

        void ConvertAndSend(QueueIdentifierType queueIdentifierType, string destinationValue, string messageQueueObjectName, object obj, MessagePostProcessorDelegate messagePostProcessorDelegate);
        */
        /// <summary>
        /// Receive and convert a message synchronously from the default message queue.  
        /// </summary>
        /// <returns>The converted object</returns>
        /// <exception cref="MessageQueueException">if thrown by MSMQ API methods.  Note an
        /// exception will be thrown if the timeout of the syncrhonous recieve operation expires.
        /// </exception>
        object ReceiveAndConvert();


        /// <summary>
        /// Receives and convert a message synchronously from the specified message queue.
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <returns>the converted object</returns>
        /// <exception cref="MessageQueueException">if thrown by MSMQ API methods.  Note an
        /// exception will be thrown if the timeout of the syncrhonous recieve operation expires.
        /// </exception>
        object ReceiveAndConvert(string messageQueueObjectName);

        /// <summary>
        /// Receives a message on the default message queue using the transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction. 
        /// </summary>
        /// <returns>A message.</returns>
        Message Receive();

        /// <summary>
        /// Receives  a message on the specified queue using the transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction. 
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <returns></returns>
        Message Receive(string messageQueueObjectName);

        /// <summary>
        /// Sends the specified message to the default message queue using the
        /// transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction. 
        /// </summary>
        /// <param name="message">The message to send</param>
        void Send(Message message);


        /// <summary>
        /// Sends the specified message to the message queue using the
        /// transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction. 
        /// </summary>
        /// <param name="messageQueueObjectName">Name of the message queue object.</param>
        /// <param name="message">The message.</param>
        void Send(string messageQueueObjectName, Message message);

        /// <summary>
        /// Sends the specified message on the provided MessageQueue using the 
        /// transactional settings as dicted by MessageQueue's Transactional property and
        /// the current Spring managed ambient transaction.
        /// </summary>
        /// <para>
        /// Note that it is the callers responsibility to ensure that the MessageQueue instance
        /// passed into this not being access simultaneously by other threads.
        /// </para>
        /// <remarks>A transactional send (either local or DTC transaction) will be
        /// attempted for a transacitonal queue, falling back to a single-transaction send
        /// to a transactional queue if there is not ambient Spring managed transaction.</remarks>
        /// <param name="messageQueue">The DefaultMessageQueue to send a message to.</param>
        /// <param name="message">The message to send</param>
        void Send(MessageQueue messageQueue, Message message);
    }
}