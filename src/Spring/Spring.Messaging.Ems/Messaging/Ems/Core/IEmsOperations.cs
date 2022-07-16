#region License

/*
 * Copyright � 2002-2010 the original author or authors.
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

namespace Spring.Messaging.Ems.Core
{
    /// <summary>Specifies a basic set of EMS operations.
	/// </summary>
	/// <remarks>
	/// <p>Implemented by EmsTemplate. Not often used but a useful option
	/// to enhance testability, as it can easily be mocked or stubbed.</p>
	///
	/// <p>Provides <code>EmsTemplate's</code> <code>send(..)</code> and
	/// <code>receive(..)</code> methods that mirror various EMS API methods.
	/// See the EMS specification and EMS API docs for details on those methods.
	/// </p>
	/// </remarks>
	/// <author>Mark Pollack</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
    public interface IEmsOperations
    {
        /// <summary> Execute the action specified by the given action object within
        /// a EMS Session.
        /// </summary>
        /// <param name="del">delegate that exposes the session</param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object Execute(SessionDelegate del);

        /// <summary> Execute the action specified by the given action object within
		/// a EMS Session.
		/// </summary>
		/// <param name="action">callback object that exposes the session
		/// </param>
		/// <returns> the result object from working with the session
		/// </returns>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		object Execute(ISessionCallback action);

		/// <summary> Send a message to a EMS destination. The callback gives access to
		/// the EMS session and MessageProducer in order to do more complex
		/// send operations.
		/// </summary>
		/// <param name="action">callback object that exposes the session/producer pair
		/// </param>
		/// <returns> the result object from working with the session
		/// </returns>
		/// <throws>EMSException  if there is any problem </throws>
		object Execute(IProducerCallback action);

        /// <summary> Send a message to a EMS destination. The callback gives access to
        /// the EMS session and MessageProducer in order to do more complex
        /// send operations.
        /// </summary>
        /// <param name="del">delegate that exposes the session/producer pair
        /// </param>
        /// <returns> the result object from working with the session
        /// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object Execute(ProducerDelegate del);


		//-------------------------------------------------------------------------
		// Convenience methods for sending messages
		//-------------------------------------------------------------------------

		/// <summary> Send a message to the default destination.
		/// <p>This will only work with a default destination specified!</p>
		/// </summary>
		/// <param name="messageCreator">callback to create a message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void Send(IMessageCreator messageCreator);

		/// <summary> Send a message to the specified destination.
		/// The IMessageCreator callback creates the message given a Session.
		/// </summary>
		/// <param name="destination">the destination to send this message to
		/// </param>
		/// <param name="messageCreator">callback to create a message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void Send(Destination destination, IMessageCreator messageCreator);

		/// <summary> Send a message to the specified destination.
		/// The IMessageCreator callback creates the message given a Session.
		/// </summary>
		/// <param name="destinationName">the name of the destination to send this message to
		/// (to be resolved to an actual destination by a DestinationResolver)
		/// </param>
		/// <param name="messageCreator">callback to create a message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void Send(string destinationName, IMessageCreator messageCreator);

        //-------------------------------------------------------------------------
        // Convenience methods for sending messages
        //-------------------------------------------------------------------------

        /// <summary> Send a message to the default destination.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        void SendWithDelegate(MessageCreatorDelegate messageCreatorDelegate);

        /// <summary> Send a message to the specified destination.
        /// The IMessageCreator callback creates the message given a Session.
        /// </summary>
        /// <param name="destination">the destination to send this message to
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        void SendWithDelegate(Destination destination, MessageCreatorDelegate messageCreatorDelegate);

        /// <summary> Send a message to the specified destination.
        /// The IMessageCreator callback creates the message given a Session.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)
        /// </param>
        /// <param name="messageCreatorDelegate">delegate callback to create a message
        /// </param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        void SendWithDelegate(string destinationName, MessageCreatorDelegate messageCreatorDelegate);

		//-------------------------------------------------------------------------
		// Convenience methods for sending auto-converted messages
		//-------------------------------------------------------------------------

		/// <summary> Send the given object to the default destination, converting the object
		/// to a EMS message with a configured IMessageConverter.
		/// <p>This will only work with a default destination specified!</p>
		/// </summary>
		/// <param name="message">the object to convert to a message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void ConvertAndSend(object message);

		/// <summary> Send the given object to the specified destination, converting the object
		/// to a EMS message with a configured IMessageConverter.
		/// </summary>
		/// <param name="destination">the destination to send this message to
		/// </param>
		/// <param name="message">the object to convert to a message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void ConvertAndSend(Destination destination, object message);

		/// <summary> Send the given object to the specified destination, converting the object
		/// to a EMS message with a configured IMessageConverter.
		/// </summary>
		/// <param name="destinationName">the name of the destination to send this message to
		/// (to be resolved to an actual destination by a DestinationResolver)
		/// </param>
		/// <param name="message">the object to convert to a message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void ConvertAndSend(string destinationName, object message);

		/// <summary> Send the given object to the default destination, converting the object
		/// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
		/// callback allows for modification of the message after conversion.
		/// <p>This will only work with a default destination specified!</p>
		/// </summary>
		/// <param name="message">the object to convert to a message
		/// </param>
		/// <param name="postProcessor">the callback to modify the message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void ConvertAndSend(object message, IMessagePostProcessor postProcessor);

		/// <summary> Send the given object to the specified destination, converting the object
		/// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
		/// callback allows for modification of the message after conversion.
		/// </summary>
		/// <param name="destination">the destination to send this message to
		/// </param>
		/// <param name="message">the object to convert to a message
		/// </param>
		/// <param name="postProcessor">the callback to modify the message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void ConvertAndSend(Destination destination, object message, IMessagePostProcessor postProcessor);

		/// <summary> Send the given object to the specified destination, converting the object
		/// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
		/// callback allows for modification of the message after conversion.
		/// </summary>
		/// <param name="destinationName">the name of the destination to send this message to
		/// (to be resolved to an actual destination by a DestinationResolver)
		/// </param>
		/// <param name="message">the object to convert to a message.
		/// </param>
		/// <param name="postProcessor">the callback to modify the message
		/// </param>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		void ConvertAndSend(string destinationName, object message, IMessagePostProcessor postProcessor);


        /// <summary>
        /// Send the given object to the default destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// <p>This will only work with a default destination specified!</p>
        /// </summary>
        /// <param name="message">the object to convert to a message</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        void ConvertAndSendWithDelegate(object message, MessagePostProcessorDelegate postProcessor);

        /// <summary>
        /// Send the given object to the specified destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destination">the destination to send this message to</param>
        /// <param name="message">the object to convert to a message</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        void ConvertAndSendWithDelegate(Destination destination, object message, MessagePostProcessorDelegate postProcessor);

        /// <summary>
        /// Send the given object to the specified destination, converting the object
        /// to a EMS message with a configured IMessageConverter. The IMessagePostProcessor
        /// callback allows for modification of the message after conversion.
        /// </summary>
        /// <param name="destinationName">the name of the destination to send this message to
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="message">the object to convert to a message.</param>
        /// <param name="postProcessor">the callback to modify the message</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
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
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		Message Receive();

		/// <summary> Receive a message synchronously from the specified destination, but only
		/// wait up to a specified time for delivery.
		/// <p>This method should be used carefully, since it will block the thread
		/// until the message becomes available or until the timeout value is exceeded.</p>
		/// </summary>
		/// <param name="destination">the destination to receive a message from
		/// </param>
		/// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
		/// </returns>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		Message Receive(Destination destination);

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
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		Message Receive(string destinationName);

		/// <summary> Receive a message synchronously from the default destination, but only
		/// wait up to a specified time for delivery.
		/// <p>This method should be used carefully, since it will block the thread
		/// until the message becomes available or until the timeout value is exceeded.</p>
		/// <p>This will only work with a default destination specified!</p>
		/// </summary>
		/// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
		/// See the EMS specification for a detailed definition of selector expressions.
		/// </param>
		/// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
		/// </returns>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		Message ReceiveSelected(string messageSelector);

		/// <summary> Receive a message synchronously from the specified destination, but only
		/// wait up to a specified time for delivery.
		/// <p>This method should be used carefully, since it will block the thread
		/// until the message becomes available or until the timeout value is exceeded.</p>
		/// </summary>
		/// <param name="destination">the destination to receive a message from
		/// </param>
		/// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
		/// See the EMS specification for a detailed definition of selector expressions.
		/// </param>
		/// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
		/// </returns>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		Message ReceiveSelected(Destination destination, string messageSelector);

		/// <summary> Receive a message synchronously from the specified destination, but only
		/// wait up to a specified time for delivery.
		/// <p>This method should be used carefully, since it will block the thread
		/// until the message becomes available or until the timeout value is exceeded.</p>
		/// </summary>
		/// <param name="destinationName">the name of the destination to send this message to
		/// (to be resolved to an actual destination by a DestinationResolver)
		/// </param>
		/// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
		/// See the EMS specification for a detailed definition of selector expressions.
		/// </param>
		/// <returns> the message received by the consumer, or <code>null</code> if the timeout expires
		/// </returns>
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		Message ReceiveSelected(string destinationName, string messageSelector);


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
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
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
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		object ReceiveAndConvert(Destination destination);

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
		/// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		object ReceiveAndConvert(string destinationName);

		/// <summary> Receive a message synchronously from the default destination, but only
		/// wait up to a specified time for delivery. Convert the message into an
		/// object with a configured IMessageConverter.
		/// <p>This method should be used carefully, since it will block the thread
		/// until the message becomes available or until the timeout value is exceeded.</p>
		/// <p>This will only work with a default destination specified!</p>
		/// </summary>
		/// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
		/// See the EMS specification for a detailed definition of selector expressions.
		/// </param>
		/// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
		/// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		object ReceiveSelectedAndConvert(string messageSelector);

		/// <summary> Receive a message synchronously from the specified destination, but only
		/// wait up to a specified time for delivery. Convert the message into an
		/// object with a configured IMessageConverter.
		/// <p>This method should be used carefully, since it will block the thread
		/// until the message becomes available or until the timeout value is exceeded.</p>
		/// </summary>
		/// <param name="destination">the destination to receive a message from
		/// </param>
		/// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
		/// See the EMS specification for a detailed definition of selector expressions.
		/// </param>
		/// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
		/// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		object ReceiveSelectedAndConvert(Destination destination, string messageSelector);

		/// <summary> Receive a message synchronously from the specified destination, but only
		/// wait up to a specified time for delivery. Convert the message into an
		/// object with a configured IMessageConverter.
		/// <p>This method should be used carefully, since it will block the thread
		/// until the message becomes available or until the timeout value is exceeded.</p>
		/// </summary>
		/// <param name="destinationName">the name of the destination to send this message to
		/// (to be resolved to an actual destination by a DestinationResolver)
		/// </param>
		/// <param name="messageSelector">the EMS message selector expression (or <code>null</code> if none).
		/// See the EMS specification for a detailed definition of selector expressions.
		/// </param>
		/// <returns> the message produced for the consumer or <code>null</code> if the timeout expires.
		/// </returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
		object ReceiveSelectedAndConvert(string destinationName, string messageSelector);


        /// <summary>
        /// Browses messages in the default EMS queue. The callback gives access to the EMS
        /// Session and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <returns>the result object from working with the session</returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object Browse(IBrowserCallback action);


        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queue">The queue to browse.</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <returns>the result object from working with the session</returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object Browse(Queue queue, IBrowserCallback action);

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queueName">Name of the queue to browse,
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object Browse(string queueName, IBrowserCallback action);


        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object BrowseSelected(string messageSelector, IBrowserCallback action);


        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queue">The queue to browse.</param>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <returns></returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object BrowseSelected(Queue queue, string messageSelector, IBrowserCallback action);

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queueName">Name of the queue to browse,
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback object that exposes the session/browser pair.</param>
        /// <returns></returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object BrowseSelected(string queueName, string messageSelector, IBrowserCallback action);

        /// <summary>
        /// Browses messages in the default EMS queue. The callback gives access to the EMS
        /// Session and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <returns>the result object from working with the session</returns>
        object BrowseWithDelegate(BrowserDelegate action);

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queue">The queue to browse.</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <returns>the result object from working with the session</returns>
        object BrowseWithDelegate(Queue queue, BrowserDelegate action);

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queueName">Name of the queue to browse,
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object BrowseWithDelegate(string queueName, BrowserDelegate action);


        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object BrowseSelectedWithDelegate(string messageSelector, BrowserDelegate action);

        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queue">The queue to browse.</param>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <returns></returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object BrowseSelectedWithDelegate(Queue queue, string messageSelector, BrowserDelegate action);


        /// <summary>
        /// Browses messages in a EMS queue. The callback gives access to the EMS Session
        /// and QueueBrowser in order to browse the queue and react to the contents.
        /// </summary>
        /// <param name="queueName">Name of the queue to browse,
        /// (to be resolved to an actual destination by a DestinationResolver)</param>
        /// <param name="messageSelector">The EMS message selector expression (or <code>null</code> if none).</param>
        /// <param name="action">The action callback delegate that exposes the session/browser pair.</param>
        /// <returns></returns>
        /// <exception cref="EMSException">If there is any problem accessing the EMS API</exception>
        object BrowseSelectedWithDelegate(string queueName, string messageSelector, BrowserDelegate action);

	}
}
