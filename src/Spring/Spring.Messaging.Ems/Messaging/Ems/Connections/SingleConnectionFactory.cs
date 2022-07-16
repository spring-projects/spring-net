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

using System.Runtime.Serialization;
using Spring.Messaging.Ems.Support;
using Common.Logging;
using Spring.Messaging.Ems.Core;
using Spring.Messaging.Ems.Common;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Messaging.Ems.Connections
{
    /// <summary>
    /// A ConnectionFactory adapter that returns the same Connection
    /// from all CreateConnection() calls, and ignores calls to
    /// Connection.Close().  According to the JMS Connection
    /// model, this is perfectly thread-safe. The
    /// shared Connection can be automatically recovered in case of an Exception.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can either pass in a specific Connection directly or let this
    /// factory lazily create a Connection via a given target ConnectionFactory.
    /// </para>
    /// <para>
    /// Useful for testing and in applications when you want to keep using the
    /// same Connection for multiple <see cref="EmsTemplate"/>
    /// calls, without having a pooling ConnectionFactory underneath. This may span
    /// any number of transactions, even concurrently executing transactions.
    /// </para>
    /// <para>
    /// Note that Spring's message listener containers support the use of
    /// a shared Connection within each listener container instance. Using
    /// SingleConnectionFactory with a MessageListenerContainer only really makes sense for
    /// sharing a single Connection across multiple listener containers.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack</author>
    /// <author>Mark Pollack (.NET)</author>
    public class SingleConnectionFactory : IConnectionFactory, IExceptionListener, IInitializingObject, IDisposable
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (SingleConnectionFactory));

        #endregion

        #region Fields

        private IConnectionFactory targetConnectionFactory;

        private string clientId;

        private IExceptionListener exceptionListener;

        private bool reconnectOnException = false;

        /// <summary>
        /// Wrapped Connection
        /// </summary>
        private IConnection target;

        /// <summary>
        /// Proxy Connection
        /// </summary>
        private IConnection connection;


        /// <summary>
        /// Whether the shared Connection has been started
        /// </summary>
        private bool started = false;

        /// <summary>
        /// Synchronization monitor for the shared Connection
        /// </summary>
        private object connectionMonitor = new object();

        private bool sslProxyHostSet;
        private bool sslProxyPortSet;
        private bool sslProxyAuthPasswordSet;
        private bool sslProxyAuthUsernameSet;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleConnectionFactory"/> class.
        /// </summary>
        public SingleConnectionFactory()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SingleConnectionFactory"/> class
        /// that alwasy returns the given Connection.
        /// </summary>
        /// <param name="target">The single Connection.</param>
        public SingleConnectionFactory(IConnection target)
        {
            AssertUtils.ArgumentNotNull(target, "connection", "TargetSession Connection must not be null");
            this.target = target;
            connection = GetSharedConnection(target);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SingleConnectionFactory"/> class
        /// that alwasy returns a single Connection.
        /// </summary>
        /// <param name="targetConnectionFactory">The target connection factory.</param>
        public SingleConnectionFactory(IConnectionFactory targetConnectionFactory)
        {
            AssertUtils.ArgumentNotNull(targetConnectionFactory, "targetConnectionFactory",
                                        "TargetSession ConnectionFactory must not be null");
            this.targetConnectionFactory = targetConnectionFactory;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the target connection factory which will be used to create a single
        /// connection.
        /// </summary>
        /// <value>The target connection factory.</value>
        public IConnectionFactory TargetConnectionFactory
        {
            get { return targetConnectionFactory; }
            set { targetConnectionFactory = value; }
        }


        /// <summary>
        /// Sets the exception listener.
        /// </summary>
        /// <value>The exception listener.</value>
        public IExceptionListener ExceptionListener
        {
            get { return exceptionListener; }
            set { exceptionListener = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the single Connection
        /// should be reset (to be subsequently renewed) when a EMSException
        /// is reported by the underlying Connection.
        /// </summary>
        /// <remarks>
        /// Default is <code>false</code>.  Switch this to <code>true</code>
        /// to automatically trigger recover based on your messaging provider's
        /// exception notifications.
        /// <para>
        /// Internally, this will lead to a special ExceptionListener (this
        /// SingleConnectionFactory itself) being registered with the underlying
        /// Connection.  This can also be combined with a user-specified
        /// ExceptionListener, if desired.
        /// </para>
        /// </remarks>
        /// <value>
        /// 	<c>true</c> attempt to reconnect on exception during next access; otherwise, <c>false</c>.
        /// </value>
        public bool ReconnectOnException
        {
            get { return reconnectOnException; }
            set { reconnectOnException = value; }
        }

        /// <summary>
        /// Gets the connection monitor.
        /// </summary>
        /// <value>The connection monitor.</value>
        internal object ConnectionMonitor
        {
            get { return connectionMonitor;  }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is started.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is started; otherwise, <c>false</c>.
        /// </value>
        internal bool IsStarted
        {
            get { return started;}
            set { started = value; }
        }

        /// <summary>
        /// Gets the client id.
        /// </summary>
        /// <value>The client id.</value>
        internal string ClientId
        {
            get { return clientId; }
        }

        #endregion

        #region Implementation of IConnectionFactory

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <returns>A single shared connection</returns>
        public IConnection CreateConnection()
        {
            lock (connectionMonitor)
            {
                if (connection == null)
                {
                    InitConnection();
                }
                return connection;
            }
        }

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public IConnection CreateConnection(string userName, string password)
        {
            throw new InvalidOperationException("SingleConnectionFactory does not support custom username and password.");
        }

        public ConnectionFactory NativeConnectionFactory
        {
            get { return targetConnectionFactory.NativeConnectionFactory; }
        }

        public object CertificateStore
        {
            get { return TargetConnectionFactory.CertificateStore; }
        }


        public string SSLProxyPassword
        {
            get { return TargetConnectionFactory.SSLProxyPassword; }
        }

        public string SSLProxyUser
        {
            get { return TargetConnectionFactory.SSLProxyUser; }
        }

        public IEmsSSLStoreType CertificateStoreType
        {
            set { TargetConnectionFactory.CertificateStoreType = value; }
        }

        public string ClientID
        {
            set
            {
                this.clientId = value;
                TargetConnectionFactory.ClientID = value;
            }
        }

        public StreamWriter ClientTracer
        {
            set { TargetConnectionFactory.ClientTracer = value; }
        }

        public int ConnAttemptCount
        {
            set { TargetConnectionFactory.ConnAttemptCount = value; }
        }

        public int ConnAttemptDelay
        {
            set { TargetConnectionFactory.ConnAttemptDelay = value; }
        }

        public int ConnAttemptTimeout
        {
            set { TargetConnectionFactory.ConnAttemptTimeout = value; }
        }

        public EMSSSLHostNameVerifier HostNameVerifier
        {
            set { TargetConnectionFactory.HostNameVerifier = value; }
        }

        public int MetricAsInt
        {
            set { TargetConnectionFactory.MetricAsInt = value; }
        }

        public string MulticastDaemon
        {
            set { TargetConnectionFactory.MulticastDaemon = value; }
        }

        public bool MulticastEnabled
        {
            set { TargetConnectionFactory.MulticastEnabled = value; }
        }

        public int ReconnAttemptCount
        {
            set { TargetConnectionFactory.ReconnAttemptCount = value; }
        }

        public int ReconnAttemptDelay
        {
            set { TargetConnectionFactory.ReconnAttemptDelay = value; }
        }

        public int ReconnAttemptTimeout
        {
            set { TargetConnectionFactory.ReconnAttemptTimeout = value; }
        }

        public string ServerUrl
        {
            set { TargetConnectionFactory.ServerUrl = value; }
        }

        public bool SSLAuthOnly
        {
            set { TargetConnectionFactory.SSLAuthOnly = value; }
        }

        public string SSLProxyHost
        {
            get
            {
                return TargetConnectionFactory.SSLProxyHost;
            }
            set
            {
                TargetConnectionFactory.SSLProxyHost = value;
                sslProxyHostSet = true;
            }
        }

        public int SSLProxyPort
        {
            get { return TargetConnectionFactory.SSLProxyPort;}
            set
            {
                TargetConnectionFactory.SSLProxyPort = value;
                sslProxyPortSet = true;
            }
        }

        public string SSLProxyAuthUsername
        {
            set
            {
                TargetConnectionFactory.SSLProxyAuthUsername = value;
                sslProxyAuthUsernameSet = true;
            }
            get { return TargetConnectionFactory.SSLProxyAuthUsername;  }
        }

        public string SSLProxyAuthPassword
        {
            set
            {
                TargetConnectionFactory.SSLProxyAuthPassword = value;
                sslProxyAuthPasswordSet = true;
            }
            get { return TargetConnectionFactory.SSLProxyAuthPassword;  }
        }

        public bool SSLTrace
        {
            set { TargetConnectionFactory.SSLTrace = value; }
        }

        public string TargetHostName
        {
            set { TargetConnectionFactory.TargetHostName = value; }
        }

        public string UserName
        {
            set { TargetConnectionFactory.UserName = value; }
        }

        public string UserPassword
        {
            set { TargetConnectionFactory.UserPassword = value; }
        }

        public FactoryLoadBalanceMetric Metric
        {
            get { return TargetConnectionFactory.Metric; }
            set { TargetConnectionFactory.Metric = value; }
        }

        #endregion



        #region Implementation of ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            TargetConnectionFactory.GetObjectData(info, context);
        }

        #endregion

        #region Implementation of ICloneable

        public object Clone()
        {
            return TargetConnectionFactory.Clone();
        }

        #endregion

        /// <summary>
        /// Initialize the underlying shared Connection. Closes and reinitializes the Connection if an underlying
	    /// Connection is present already.
        /// </summary>
        public void InitConnection()
        {
            if (TargetConnectionFactory == null)
            {
                throw new ArgumentException(
                    "'TargetConnectionFactory' is required for lazily initializing a Connection");
            }
            lock (connectionMonitor)
            {
                if (this.target != null)
                {
                    CloseConnection(this.target);
                }
                this.target = DoCreateConnection();
                PrepareConnection(this.target);
                if (LOG.IsDebugEnabled)
                {
                    LOG.Info("Established shared EMS Connection: " + this.target);
                }
                this.connection = GetSharedConnection(target);
            }
        }

        /// <summary>
        /// Exception listener callback that renews the underlying single Connection.
        /// </summary>
        /// <param name="exception">The exception from the messaging infrastructure.</param>
        public void OnException(EMSException exception)
        {
            ResetConnection();
        }

        /// <summary>
        /// Prepares the connection before it is exposed.
	    /// The default implementation applies ExceptionListener and client id.
	    /// Can be overridden in subclasses.
        /// </summary>
        /// <param name="con">The Connection to prepare.</param>
        /// <exception cref="EMSException">if thrown by any EMS API methods.</exception>
        protected virtual void PrepareConnection(IConnection con)
        {
            if (ClientId != null)
            {
                con.ClientID = ClientId;
            }
            if (this.exceptionListener != null || this.reconnectOnException)
            {
                IExceptionListener listenerToUse = this.exceptionListener;
                if (reconnectOnException)
                {
                    listenerToUse = new InternalChainedExceptionListener(this, listenerToUse);
                }
                con.ExceptionListener = listenerToUse;
            }
        }

        /// <summary>
        /// Template method for obtaining a (potentially cached) Session.
        /// </summary>
        /// <param name="con">The connection to operate on.</param>
        /// <param name="mode">The session ack mode.</param>
        /// <returns>the Session to use, or <code>null</code> to indicate
	    /// creation of a raw standard Session</returns>
        public virtual ISession GetSession(IConnection con, SessionMode mode)
        {
            return null;
        }



        /// <summary>
        /// reate a JMS Connection via this template's ConnectionFactory.
        /// </summary>
        /// <returns></returns>
        protected virtual IConnection DoCreateConnection()
        {
            return TargetConnectionFactory.CreateConnection();
        }

        /// <summary>
        /// Closes the given connection.
        /// </summary>
        /// <param name="con">The connection.</param>
        protected virtual void CloseConnection(IConnection con)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Closing shared EMS Connection: " + this.target);
            }
            try
            {
                try
                {
                    if (this.started)
                    {
                        this.started = false;
                        con.Stop();
                    }
                } finally
                {
                    con.Close();
                }
            } catch (Exception ex)
            {
                LOG.Warn("Could not close shared EMS connection.", ex);
            }
        }

        #region IInitializingObject Members

        /// <summary>
        /// Ensure that the connection or TargetConnectionFactory are specified.
        /// </summary>
        public void AfterPropertiesSet()
        {
            if (connection == null && TargetConnectionFactory == null)
            {
                throw new ArgumentException("Connection or 'TargetConnectionFactory' is required.");
            }
            // Note this is repeated in EmsConnectionFactory as this class only refers to IConnectionFactory and
            // IConnectionFactory does not implement the spring specific lifecycle interface IInitializingObject
            if (sslProxyAuthUsernameSet || sslProxyAuthPasswordSet)
            {
                TargetConnectionFactory.NativeConnectionFactory.SetSSLProxyAuth(SSLProxyAuthUsername, SSLProxyAuthPassword);
            }
            if (sslProxyHostSet || sslProxyPortSet)
            {
                TargetConnectionFactory.NativeConnectionFactory.SetSSLProxy(SSLProxyHost, SSLProxyPort);
            }
        }

        #endregion


        /// <summary>
        /// Close the underlying shared connection. The provider of this ConnectionFactory needs to care for proper shutdown.
        /// As this object implements <see cref="IDisposable"/> an application context will automatically
        /// invoke this on distruction o
        /// </summary>
        public void Dispose()
        {
            ResetConnection();
        }

        /// <summary>
        /// Resets the underlying shared Connection, to be reinitialized on next access.
        /// </summary>
        public virtual void ResetConnection()
        {
            lock (connectionMonitor)
            {
                if (this.target != null)
                {
                    CloseConnection(this.target);
                }
                this.target = null;
                this.connection = null;
            }
        }

        /// <summary>
        /// Wrap the given Connection with a proxy that delegates every method call to it
	    /// but suppresses close calls. This is useful for allowing application code to
	    /// handle a special framework Connection just like an ordinary Connection from a
	    /// ConnectionFactory.
        /// </summary>
        /// <param name="target">The original connection to wrap.</param>
        /// <returns>the wrapped connection</returns>
        protected virtual IConnection GetSharedConnection(IConnection target)
        {
            lock (connectionMonitor)
            {
                return new CloseSupressingConnection(this, target);
            }
        }
    }

    /// <summary>
    /// Internal chained ExceptionListener for handling the internal recovery listener
    /// in combination with a user-specified listener.
    /// </summary>
    internal class InternalChainedExceptionListener : ChainedExceptionListener
    {
        private IExceptionListener userListener;

        public InternalChainedExceptionListener(IExceptionListener internalListener, IExceptionListener userListener)
        {
            AddListener(internalListener);
            if (userListener != null)
            {
                AddListener(userListener);
                this.userListener = userListener;
            }
        }

        public IExceptionListener UserListener
        {
            get { return userListener; }
        }
    }
    internal class CloseSupressingConnection : IConnection
    {
        private IConnection target;
        private SingleConnectionFactory singleConnectionFactory;

        public CloseSupressingConnection(SingleConnectionFactory singleConnectionFactory, IConnection target)
        {
            this.target = target;
            this.singleConnectionFactory = singleConnectionFactory;
        }

        /// <summary>
        /// Add information to show this is a shared EMS connection
        /// </summary>
        /// <returns>Description of connection wrapper</returns>
        public override string ToString()
        {
            return "Shared EMS Connection: " + this.target;
        }

        public string ClientID
        {
            get { return target.ClientID; }
            set
            {
                // Handle set ClientID property: throw exception if not compatible.
                string currentClientId = target.ClientID;
                if (currentClientId != null && currentClientId.Equals(value))
                {
                    //ok, the values are consistent.
                }
                else
                {
                    throw new IllegalStateException(
                        "Setting of 'ClientID' property not supported on wrapper for shared Connection since" +
                        "this is a shared connection that may serve any number of clients concurrently." +
                        "Set the 'ClientId' property on the SingleConnectionFactory instead.");
                }

            }
        }


        public void Start()
        {
            // Handle start method: track started state.
            target.Start();
            lock (singleConnectionFactory.ConnectionMonitor)
            {
                singleConnectionFactory.IsStarted = true;
            }
        }

        public void Stop()
        {
            //don't pass the call to the target as it would stop receiving for all clients sharing this connection.
        }

        public void Close()
        {
            // don't pass the call to the target.
        }

        public ISession CreateSession(bool transacted, int acknowledgementMode)
        {
            ISession session = singleConnectionFactory.GetSession(target, EmsUtils.ConvertAcknowledgementMode(acknowledgementMode));
            if (session != null)
            {
                return session;
            }
            return target.CreateSession(transacted, acknowledgementMode);
        }

        public ISession CreateSession(bool transacted, SessionMode acknowledgeMode)
        {
            ISession session = singleConnectionFactory.GetSession(target, acknowledgeMode);
            if (session != null)
            {
                return session;
            }
            return target.CreateSession(transacted, acknowledgeMode);
        }

        public IExceptionListener ExceptionListener
        {
            get
            {
                IExceptionListener currentExceptionListener = target.ExceptionListener;
                if (currentExceptionListener is InternalChainedExceptionListener)
                {
                    return ((InternalChainedExceptionListener) currentExceptionListener).UserListener;
                } else
                {
                    return currentExceptionListener;
                }
            }
            set
            {
                IExceptionListener currentExceptionListener = target.ExceptionListener;
                if (value != null && currentExceptionListener is InternalChainedExceptionListener)
                {
                    ((InternalChainedExceptionListener) currentExceptionListener).AddListener(value);
                } else
                {
                    throw new IllegalStateException(
                        "set ExceptionListener call not supported on proxy for shared Connection. " +
                        "Set the 'ExceptionListener' property on the SingleConnectionFactory instead. " +
                        "Alternatively, activate SingleConnectionFactory's 'reconnectOnException' feature, " +
                        "which will allow for registering further ExceptionListeners to the recovery chain.");
                }
            }
        }

        #region Pass through implementations to the target connection


        public event EMSExceptionHandler EMSExceptionHandler
        {
            add
            {
                target.EMSExceptionHandler += value;
            }
            remove
            {
                target.EMSExceptionHandler -= value;
            }
        }


        public Connection NativeConnection
        {
            get { return target.NativeConnection; }
        }

        public string ActiveURL
        {
            get { return target.ActiveURL; }
        }

        public long ConnID
        {
            get { return target.ConnID;  }
        }

        public bool IsClosed
        {
            get { return target.IsClosed; }
        }

        public bool IsSecure
        {
            get { return target.IsSecure; }
        }

        public ConnectionMetaData MetaData
        {
            get { return target.MetaData; }
        }

        #endregion


    }
}
