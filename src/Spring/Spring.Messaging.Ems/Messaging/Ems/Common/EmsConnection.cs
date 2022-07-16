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

using Common.Logging;

namespace Spring.Messaging.Ems.Common
{
    /// <summary>
    /// A Connection object is a client's active connection to TIBCO EMS Server.
    /// </summary>
    public class EmsConnection : IConnection
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(EmsConnection));

        #endregion

        private Connection nativeConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmsConnection"/> class.
        /// </summary>
        /// <param name="connection">The underlying TIBCO EMS connection.</param>
        public EmsConnection(Connection connection)
        {
            this.nativeConnection = connection;
            this.nativeConnection.ExceptionHandler += HandleEmsException;
        }



        #region Implementation of IConnection

        /// <summary>
        /// Gets the native TIBCO EMS connection.
        /// </summary>
        /// <value>The native connection.</value>
        public Connection NativeConnection
        {
            get { return this.nativeConnection; }
        }

        /// <summary>
        /// Occurs when the client library detects a problem with the connection.
        /// </summary>
        public event EMSExceptionHandler EMSExceptionHandler;

        /// <summary>
        /// Gets the URL of the server this connection is currently connected to
        /// </summary>
        /// <value>The active URL.</value>
        public string ActiveURL
        {
            get { return nativeConnection.ActiveURL; }
        }

        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        /// <value>The client ID.</value>
        public string ClientID
        {
            get { return nativeConnection.ClientID; }
            set { nativeConnection.ClientID = value; }
        }

        /// <summary>
        /// Gets the connection ID.
        /// </summary>
        /// <value>The connection ID.</value>
        public long ConnID
        {
            get { return nativeConnection.ConnID; }
        }

        /// <summary>
        /// Gets or sets the exception listener.
        /// </summary>
        /// <value>The exception listener.</value>
        public IExceptionListener ExceptionListener
        {
            get { return nativeConnection.ExceptionListener;  }
            set { nativeConnection.ExceptionListener = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the connection is closed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the connection is closed; otherwise, <c>false</c>.
        /// </value>
        public bool IsClosed
        {
            get { return nativeConnection.IsClosed; }
        }

        /// <summary>
        /// Gets a value indicating whether the connection communicates with a secure protocol
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the connection communicates with a secure protocol; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecure
        {
            get { return nativeConnection.IsSecure; }
        }

        /// <summary>
        /// Gets the metadata for this connection
        /// </summary>
        /// <value>The metadata for this connection.</value>
        public ConnectionMetaData MetaData
        {
            get { return nativeConnection.MetaData; }
        }

        /// <summary>
        /// Closes the connection and reclaims resources.
        /// </summary>
        public void Close()
        {
            nativeConnection.Close();
        }

        /// <summary>
        /// Creates the session.
        /// </summary>
        /// <param name="transacted">if set to <c>true</c> the session has transaction semantics.</param>
        /// <param name="acknowledgeMode">Indicates whether and how the consumer is to acknowledge received messages.
        /// This version of CreateSession accepts an integer value associated with the acknowledge mode described by a Session member and should only be used for backward compatibility.
        /// This parameter is ignored if the session is transacted.</param>
        /// <returns>A newly created session.</returns>
        public ISession CreateSession(bool transacted, int acknowledgeMode)
        {
            Session nativeSession = nativeConnection.CreateSession(transacted, acknowledgeMode);
            return new EmsSession(nativeSession);
        }

        /// <summary>
        /// Creates the session.
        /// </summary>
        /// <param name="transacted">if set to <c>true</c> [transacted].</param>
        /// <param name="acknowledgeMode">The acknowledge mode.
        /// When true, the new session has transaction semantics.
        /// Indicates whether and how the consumer is to acknowledge received messages.
        /// Legal values are listed under SessionMode.
        /// This parameter is ignored if the session is transacted.</param>
        /// <returns>A newly created session.</returns>
        public ISession CreateSession(bool transacted, SessionMode acknowledgeMode)
        {
            Session nativeSession = nativeConnection.CreateSession(transacted, acknowledgeMode);
            return new EmsSession(nativeSession);
        }

        /// <summary>
        /// Starts (or restarts) a connection's delivery of incoming messages.
        /// </summary>
        public void Start()
        {
            nativeConnection.Start();
        }

        /// <summary>
        /// Temporarily stops a connection's delivery of incoming messages.
        /// </summary>
        public void Stop()
        {
            nativeConnection.Stop();
        }

        #endregion

        private void HandleEmsException(object sender, EMSExceptionEventArgs arg)
        {
            if (EMSExceptionHandler != null)
            {
                EMSExceptionHandler(sender, arg);
            }
            else
            {
                logger.Error("No exception handler registered with EmsConnection wrapper class.", arg.Exception);
            }
        }
    }
}
