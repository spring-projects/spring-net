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

using System.ComponentModel;

namespace Spring.Messaging.Ems.Common
{
    /// <summary>
    /// An interface containing all methods and properties on the TIBCO.EMS.Connection class.
    /// Refer to the TIBCO EMS API documentation for more information.
    /// </summary>
    /// <author>Mark Pollack</author>
    public interface IConnection
    {
        /// <summary>
        /// Gets the native TIBCO EMS connection.
        /// </summary>
        /// <value>The native connection.</value>
        Connection NativeConnection { get; }

        /// <summary>
        /// Occurs when the client library detects a problem with the connection.
        /// </summary>
        event EMSExceptionHandler EMSExceptionHandler;

        /// <summary>
        /// Gets the URL of the server this connection is currently connected to
        /// </summary>
        /// <value>The active URL.</value>
        string ActiveURL { get; }


        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        /// <value>The client ID.</value>
        string ClientID { get; set; }

        /// <summary>
        /// Gets the connection ID.
        /// </summary>
        /// <value>The connection ID.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        long ConnID { get; }

        /// <summary>
        /// Gets or sets the exception listener.
        /// </summary>
        /// <value>The exception listener.</value>
        IExceptionListener ExceptionListener { get; set; }

        /// <summary>
        /// Gets a value indicating whether the connection is closed.
        /// </summary>
        /// <value><c>true</c> if the connection is closed; otherwise, <c>false</c>.</value>
        bool IsClosed { get; }

        /// <summary>
        /// Gets a value indicating whether the connection communicates with a secure protocol
        /// </summary>
        /// <value><c>true</c> if the connection communicates with a secure protocol; otherwise, <c>false</c>.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsSecure { get; }

        /// <summary>
        /// Gets the metadata for this connection
        /// </summary>
        /// <value>The metadata for this connection.</value>
        ConnectionMetaData MetaData { get; }

        /// <summary>
        /// Closes the connection and reclaims resources.
        /// </summary>
        void Close();

        /// <summary>
        /// Creates the session.
        /// </summary>
        /// <param name="transacted">if set to <c>true</c> the session has transaction semantcis.</param>
        /// <param name="acknowledgeMode">Indicates whether and how the consumer is to acknowledge received messages.
        /// This version of CreateSession accepts an integer value associated with the acknowledge mode described by a Session member and should only be used for backward compatibility.
        /// This parameter is ignored if the session is transacted.</param>
        /// <returns>A newly created session.</returns>
        ISession CreateSession(bool transacted, int acknowledgeMode);

        /// <summary>
        /// Creates the session.
        /// </summary>
        /// <param name="transacted">if set to <c>true</c> [transacted].</param>
        /// <param name="acknowledgeMode">The acknowledge mode.
        /// When true, the new session has transaction semantics.
        /// Indicates whether and how the consumer is to acknowledge received messages.
        /// Legal values are listed under SessionMode.
        /// This parameter is ignored if the session is transacted.
        /// </param>
        /// <returns>A newly created session.</returns>
        ISession CreateSession(bool transacted, SessionMode acknowledgeMode);

        /// <summary>
        /// Starts (or restarts) a connection's delivery of incoming messages.
        /// </summary>
        void Start();

        /// <summary>
        /// Temporarily stops a connection's delivery of incoming messages.
        /// </summary>
        void Stop();

        /// <summary>
        /// A String representation of the connection object
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        string ToString();
    }
}
