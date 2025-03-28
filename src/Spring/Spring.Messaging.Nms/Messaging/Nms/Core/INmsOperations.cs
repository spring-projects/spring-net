#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

namespace Spring.Messaging.Nms.Core;

/// <summary>
/// Specifies a basic set of NMS operations.
/// </summary>
/// <remarks>
/// 	<p>Implemented by NmsTemplate. Not often used but a useful option
/// to enhance testability, as it can easily be mocked or stubbed.</p>
/// 	<p>Provides <code>NmsTemplate's</code>
/// 		<code>send(..)</code> and
/// <code>receive(..)</code> methods that mirror various NMS API methods.
/// See the NMS specification and NMS API docs for details on those methods.
/// </p>
/// </remarks>
/// <author>Mark Pollack</author>
/// <author>Juergen Hoeller</author>
/// <author>Mark Pollack (.NET)</author>
public interface INmsOperations
{
    /// <summary>
    /// Execute the action specified by the given action object within
    /// a NMS Session.
    /// </summary>
    /// <param name="action">callback object that exposes the session</param>
    /// <returns>
    /// the result object from working with the session
    /// </returns>
    /// <throws>NMSException if there is any problem </throws>
    object Execute(ISessionCallback action);

    /// <summary>
    /// Execute the action specified by the given action object within
    /// a NMS Session.
    /// </summary>
    /// <param name="del">delegate that exposes the session</param>
    /// <returns>
    /// the result object from working with the session
    /// </returns>
    /// <throws>NMSException if there is any problem </throws>
    object Execute(SessionDelegate del);

    /// <summary> Send a message to a NMS destination. The callback gives access to
    /// the NMS session and MessageProducer in order to do more complex
    /// send operations.
    /// </summary>
    /// <param name="del">delegate that exposes the session/producer pair
    /// </param>
    /// <returns> the result object from working with the session
    /// </returns>
    /// <throws>NMSException  if there is any problem </throws>
    object Execute(ProducerDelegate del);

    /// <summary> Send a message to a NMS destination. The callback gives access to
    /// the NMS session and MessageProducer in order to do more complex
    /// send operations.
    /// </summary>
    /// <param name="action">callback object that exposes the session/producer pair
    /// </param>
    /// <returns> the result object from working with the session
    /// </returns>
    /// <throws>NMSException  if there is any problem </throws>
    object Execute(IProducerCallback action);

    //-------------------------------------------------------------------------
    // Convenience methods for sending messages
    //-------------------------------------------------------------------------

    /// <summary> Send a message to the default destination.
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <param name="messageCreator">callback to create a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void Send(IMessageCreator messageCreator);

    /// <summary> Send a message to the specified destination.
    /// The IMessageCreator callback creates the message given a Session.
    /// </summary>
    /// <param name="destination">the destination to send this message to
    /// </param>
    /// <param name="messageCreator">callback to create a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void Send(IDestination destination, IMessageCreator messageCreator);

    /// <summary> Send a message to the specified destination.
    /// The IMessageCreator callback creates the message given a Session.
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)
    /// </param>
    /// <param name="messageCreator">callback to create a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void Send(string destinationName, IMessageCreator messageCreator);

    //-------------------------------------------------------------------------
    // Convenience methods for sending messages
    //-------------------------------------------------------------------------

    /// <summary> Send a message to the default destination.
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <param name="messageCreatorDelegate">delegate callback to create a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void SendWithDelegate(MessageCreatorDelegate messageCreatorDelegate);

    /// <summary> Send a message to the specified destination.
    /// The IMessageCreator callback creates the message given a Session.
    /// </summary>
    /// <param name="destination">the destination to send this message to
    /// </param>
    /// <param name="messageCreatorDelegate">delegate callback to create a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void SendWithDelegate(IDestination destination, MessageCreatorDelegate messageCreatorDelegate);

    /// <summary> Send a message to the specified destination.
    /// The IMessageCreator callback creates the message given a Session.
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)
    /// </param>
    /// <param name="messageCreatorDelegate">delegate callback to create a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void SendWithDelegate(string destinationName, MessageCreatorDelegate messageCreatorDelegate);

    //-------------------------------------------------------------------------
    // Convenience methods for sending auto-converted messages
    //-------------------------------------------------------------------------

    /// <summary> Send the given object to the default destination, converting the object
    /// to a NMS message with a configured IMessageConverter.
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <param name="message">the object to convert to a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSend(object message);

    /// <summary> Send the given object to the specified destination, converting the object
    /// to a NMS message with a configured IMessageConverter.
    /// </summary>
    /// <param name="destination">the destination to send this message to
    /// </param>
    /// <param name="message">the object to convert to a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSend(IDestination destination, object message);

    /// <summary> Send the given object to the specified destination, converting the object
    /// to a NMS message with a configured IMessageConverter.
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)
    /// </param>
    /// <param name="message">the object to convert to a message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSend(string destinationName, object message);

    /// <summary> Send the given object to the default destination, converting the object
    /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
    /// callback allows for modification of the message after conversion.
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <param name="message">the object to convert to a message
    /// </param>
    /// <param name="postProcessor">the callback to modify the message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSend(object message, IMessagePostProcessor postProcessor);

    /// <summary> Send the given object to the specified destination, converting the object
    /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
    /// callback allows for modification of the message after conversion.
    /// </summary>
    /// <param name="destination">the destination to send this message to
    /// </param>
    /// <param name="message">the object to convert to a message
    /// </param>
    /// <param name="postProcessor">the callback to modify the message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSend(IDestination destination, object message, IMessagePostProcessor postProcessor);

    /// <summary> Send the given object to the specified destination, converting the object
    /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
    /// callback allows for modification of the message after conversion.
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)
    /// </param>
    /// <param name="message">the object to convert to a message.
    /// </param>
    /// <param name="postProcessor">the callback to modify the message
    /// </param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSend(string destinationName, object message, IMessagePostProcessor postProcessor);

    /// <summary>
    /// Send the given object to the default destination, converting the object
    /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
    /// callback allows for modification of the message after conversion.
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <param name="message">the object to convert to a message</param>
    /// <param name="postProcessor">the callback to modify the message</param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSendWithDelegate(object message, MessagePostProcessorDelegate postProcessor);

    /// <summary>
    /// Send the given object to the specified destination, converting the object
    /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
    /// callback allows for modification of the message after conversion.
    /// </summary>
    /// <param name="destination">the destination to send this message to</param>
    /// <param name="message">the object to convert to a message</param>
    /// <param name="postProcessor">the callback to modify the message</param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSendWithDelegate(IDestination destination, object message, MessagePostProcessorDelegate postProcessor);

    /// <summary>
    /// Send the given object to the specified destination, converting the object
    /// to a NMS message with a configured IMessageConverter. The IMessagePostProcessor
    /// callback allows for modification of the message after conversion.
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)</param>
    /// <param name="message">the object to convert to a message.</param>
    /// <param name="postProcessor">the callback to modify the message</param>
    /// <throws>NMSException if there is any problem</throws>
    void ConvertAndSendWithDelegate(string destinationName, object message, MessagePostProcessorDelegate postProcessor);

    //-------------------------------------------------------------------------
    // Convenience methods for receiving messages
    //-------------------------------------------------------------------------

    /// <summary> Receive a message synchronously from the default destination, but only
    /// wait up to a specified time for delivery.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    IMessage Receive();

    /// <summary> Receive a message synchronously from the specified destination, but only
    /// wait up to a specified time for delivery.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// </summary>
    /// <param name="destination">the destination to receive a message from
    /// </param>
    /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    IMessage Receive(IDestination destination);

    /// <summary> Receive a message synchronously from the specified destination, but only
    /// wait up to a specified time for delivery.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)
    /// </param>
    /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    IMessage Receive(string destinationName);

    /// <summary> Receive a message synchronously from the default destination, but only
    /// wait up to a specified time for delivery.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
    /// See the NMS specification for a detailed definition of selector expressions.
    /// </param>
    /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    IMessage ReceiveSelected(string messageSelector);

    /// <summary> Receive a message synchronously from the specified destination, but only
    /// wait up to a specified time for delivery.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// </summary>
    /// <param name="destination">the destination to receive a message from
    /// </param>
    /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
    /// See the NMS specification for a detailed definition of selector expressions.
    /// </param>
    /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    IMessage ReceiveSelected(IDestination destination, string messageSelector);

    /// <summary> Receive a message synchronously from the specified destination, but only
    /// wait up to a specified time for delivery.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)
    /// </param>
    /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
    /// See the NMS specification for a detailed definition of selector expressions.
    /// </param>
    /// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    IMessage ReceiveSelected(string destinationName, string messageSelector);

    //-------------------------------------------------------------------------
    // Convenience methods for receiving auto-converted messages
    //-------------------------------------------------------------------------

    /// <summary> Receive a message synchronously from the default destination, but only
    /// wait up to a specified time for delivery. Convert the message into an
    /// object with a configured IMessageConverter.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    object ReceiveAndConvert();

    /// <summary> Receive a message synchronously from the specified destination, but only
    /// wait up to a specified time for delivery. Convert the message into an
    /// object with a configured IMessageConverter.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// </summary>
    /// <param name="destination">the destination to receive a message from
    /// </param>
    /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    object ReceiveAndConvert(IDestination destination);

    /// <summary> Receive a message synchronously from the specified destination, but only
    /// wait up to a specified time for delivery. Convert the message into an
    /// object with a configured IMessageConverter.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)
    /// </param>
    /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    object ReceiveAndConvert(string destinationName);

    /// <summary> Receive a message synchronously from the default destination, but only
    /// wait up to a specified time for delivery. Convert the message into an
    /// object with a configured IMessageConverter.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// <p>This will only work with a default destination specified!</p>
    /// </summary>
    /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
    /// See the NMS specification for a detailed definition of selector expressions.
    /// </param>
    /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    object ReceiveSelectedAndConvert(string messageSelector);

    /// <summary> Receive a message synchronously from the specified destination, but only
    /// wait up to a specified time for delivery. Convert the message into an
    /// object with a configured IMessageConverter.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// </summary>
    /// <param name="destination">the destination to receive a message from
    /// </param>
    /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
    /// See the NMS specification for a detailed definition of selector expressions.
    /// </param>
    /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    object ReceiveSelectedAndConvert(IDestination destination, string messageSelector);

    /// <summary> Receive a message synchronously from the specified destination, but only
    /// wait up to a specified time for delivery. Convert the message into an
    /// object with a configured IMessageConverter.
    /// <p>This method should be used carefully, since it will block the thread
    /// until the message becomes available or until the timeout value is exceeded.</p>
    /// </summary>
    /// <param name="destinationName">the name of the destination to send this message to
    /// (to be resolved to an actual destination by a DestinationResolver)
    /// </param>
    /// <param name="messageSelector">the NMS message selector expression (or <code>null</code> if none).
    /// See the NMS specification for a detailed definition of selector expressions.
    /// </param>
    /// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
    /// </returns>
    /// <throws>NMSException if there is any problem</throws>
    object ReceiveSelectedAndConvert(string destinationName, string messageSelector);
}
