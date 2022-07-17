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

using Apache.NMS;
using Common.Logging;
using Spring.Messaging.Nms.Core;
using Spring.Messaging.Nms.Support;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Messaging.Nms.Connections
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
    /// same Connection for multiple <see cref="NmsTemplate"/>
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

        private static readonly ILog LOG = LogManager.GetLogger(typeof(SingleConnectionFactory));

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
        private SemaphoreSlimLock connectionMonitor = new SemaphoreSlimLock();

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
        /// Gets or sets the client id for the single Connection created and exposed by
        /// this ConnectionFactory.
        /// </summary>
        /// <remarks>Note that the client IDs need to be unique among all active
        /// Connections of teh underlying provider.  Furthermore, a client ID can only
        /// be assigned if the original ConnectionFactory hasn't already assigned one.</remarks>
        /// <value>The client id.</value>
        public string ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }


        /// <summary>
        /// Gets or sets the exception listener implementation that should be registered
        /// with with the single Connection created by this factory, if any.
        /// </summary>
        /// <value>The exception listener.</value>
        public IExceptionListener ExceptionListener
        {
            get { return exceptionListener; }
            set { exceptionListener = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the single Connection
        /// should be reset (to be subsequently renewed) when a NMSException
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

        public INMSContext CreateContext(string userName, string password, AcknowledgementMode acknowledgementMode)
        {
            throw new InvalidOperationException("SingleConnectionFactory does not support custom username and password.");
        }

        public Task<INMSContext> CreateContextAsync()
        {
            return CreateContextAsync(AcknowledgementMode.AutoAcknowledge);
        }

        public async Task<INMSContext> CreateContextAsync(AcknowledgementMode acknowledgementMode)
        {
            var conn = await CreateConnectionAsync().Awaiter();
            return new NmsContext(conn, acknowledgementMode);
        }

        public Task<INMSContext> CreateContextAsync(string userName, string password)
        {
            throw new InvalidOperationException("SingleConnectionFactory does not support custom username and password.");
        }

        public Task<INMSContext> CreateContextAsync(string userName, string password, AcknowledgementMode acknowledgementMode)
        {
            throw new InvalidOperationException("SingleConnectionFactory does not support custom username and password.");
        }

        /// <summary>
        /// Get/or set the broker Uri.
        /// </summary>
        public Uri BrokerUri
        {
            get { return targetConnectionFactory.BrokerUri; }
            set { targetConnectionFactory.BrokerUri = value; }
        }

        /// <summary>
        /// Get/or set the redelivery policy that new IConnection objects are
        /// assigned upon creation.
        /// </summary>
        public IRedeliveryPolicy RedeliveryPolicy
        {
            get { return targetConnectionFactory.RedeliveryPolicy; }
            set { targetConnectionFactory.RedeliveryPolicy = value; }
        }

        /// <summary>
        /// A Delegate that is called each time a Message is dispatched to allow the client to do
        /// any necessary transformations on the received message before it is delivered.  The
        /// ConnectionFactory sets the provided delegate instance on each Connection instance that
        /// is created from this factory, each connection in turn passes the delegate along to each
        /// Session it creates which then passes that along to the Consumers it creates.
        /// </summary>
        /// <value></value>
        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get { return targetConnectionFactory.ConsumerTransformer; }
            set { targetConnectionFactory.ConsumerTransformer = value; }
        }

        /// <summary>
        /// A delegate that is called each time a Message is sent from this Producer which allows
        /// the application to perform any needed transformations on the Message before it is sent.
        /// The ConnectionFactory sets the provided delegate instance on each Connection instance that
        /// is created from this factory, each connection in turn passes the delegate along to each
        /// Session it creates which then passes that along to the Producers it creates.
        /// </summary>
        /// <value></value>
        public ProducerTransformerDelegate ProducerTransformer
        {
            get { return targetConnectionFactory.ProducerTransformer; }
            set { targetConnectionFactory.ProducerTransformer = value; }
        }

        /// <summary>
        /// Gets the connection monitor.
        /// </summary>
        /// <value>The connection monitor.</value>
        internal SemaphoreSlimLock ConnectionMonitor
        {
            get { return connectionMonitor; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is started.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is started; otherwise, <c>false</c>.
        /// </value>
        internal bool IsStarted
        {
            get { return started; }
            set { started = value; }
        }

        #endregion

        #region IConnectionFactory Members

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <returns>A single shared connection</returns>
        public IConnection CreateConnection()
        {
            return CreateConnectionAsync().GetAsyncResult();
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

        public async Task<IConnection> CreateConnectionAsync()
        {
            using(await connectionMonitor.LockAsync().Awaiter())
            {
                if (connection == null)
                {
                    await InitConnectionAsync(false).Awaiter();
                }

                return connection;
            }
        }

        public Task<IConnection> CreateConnectionAsync(string userName, string password)
        {
            throw new InvalidOperationException("SingleConnectionFactory does not support custom username and password.");
        }

        public INMSContext CreateContext()
        {
            return CreateContext(AcknowledgementMode.AutoAcknowledge);
        }

        public INMSContext CreateContext(AcknowledgementMode acknowledgementMode)
        {
            return CreateContextAsync(acknowledgementMode).GetAsyncResult();
        }

        public INMSContext CreateContext(string userName, string password)
        {
            throw new InvalidOperationException("SingleConnectionFactory does not support custom username and password.");
        }

        #endregion

        /// <summary>
        /// Initialize the underlying shared Connection. Closes and reinitializes the Connection if an underlying
        /// Connection is present already.
        /// </summary>
        public async Task InitConnectionAsync(bool acquireLock = true)
        {
            if (TargetConnectionFactory == null)
            {
                throw new ArgumentException(
                    "'TargetConnectionFactory' is required for lazily initializing a Connection");
            }

            using (await connectionMonitor.LockAsync(acquireLock).Awaiter())
            {
                if (this.target != null)
                {
                    CloseConnection(this.target);
                }

                this.target = DoCreateConnection();
                PrepareConnection(this.target);
                if (LOG.IsDebugEnabled)
                {
                    LOG.Info("Established shared NMS Connection: " + this.target);
                }

                this.connection = GetSharedConnection(target, acquireLock);
            }
        }

        /// <summary>
        /// Exception listener callback that renews the underlying single Connection.
        /// </summary>
        /// <param name="exception">The exception from the messaging infrastructure.</param>
        public void OnException(Exception exception)
        {
            ResetConnection();
        }

        /// <summary>
        /// Prepares the connection before it is exposed.
        /// The default implementation applies ExceptionListener and client id.
        /// Can be overridden in subclasses.
        /// </summary>
        /// <param name="con">The Connection to prepare.</param>
        /// <exception cref="NMSException">if thrown by any NMS API methods.</exception>
        protected virtual void PrepareConnection(IConnection con)
        {
            if (ClientId != null)
            {
                con.ClientId = ClientId;
            }

            if (reconnectOnException)
            {
                //add reconnect exception handler first to exception chain.
                con.ExceptionListener += this.OnException;
            }

            if (ExceptionListener != null)
            {
                con.ExceptionListener += ExceptionListener.OnException;
            }
        }

        /// <summary>
        /// Template method for obtaining a (potentially cached) Session.
        /// </summary>
        /// <param name="con">The connection to operate on.</param>
        /// <param name="mode">The session ack mode.</param>
        /// <returns>the Session to use, or <code>null</code> to indicate
        /// creation of a raw standard Session</returns>  
        public virtual ISession GetSession(IConnection con, AcknowledgementMode mode)
        {
            return null;
        } 
        
        public virtual Task<ISession> GetSessionAsync(IConnection con, AcknowledgementMode mode)
        {
            return Task.FromResult((ISession) null);
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
                LOG.Debug("Closing shared NMS Connection: " + this.target);
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
                }
                finally
                {
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                LOG.Warn("Could not close shared NMS connection.", ex);
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
            using(connectionMonitor.Lock())
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
        protected virtual IConnection GetSharedConnection(IConnection target, bool acquireLock = true)
        {
            using(connectionMonitor.Lock(acquireLock))
            {
                return new CloseSupressingConnection(this, target);
            }
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

        public string ClientId
        {
            get { return target.ClientId; }
            set
            {
                string currentClientId = target.ClientId;
                if (currentClientId != null && currentClientId.Equals(value))
                {
                    //ok, the values are consistent.
                }
                else
                {
                    throw new ArgumentException(
                        "Setting of 'ClientID' property not supported on wrapper for shared Connection since" +
                        "this is a shared connection that may serve any number of clients concurrently." +
                        "Set the 'ClientId' property on the SingleConnectionFactory instead.");
                }
            }
        }

    
        public void Close()
        {
            // don't pass the call to the target.
        }

        public Task CloseAsync()
        {
            return Task.FromResult(true); // no CompletedTask available
        }

        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get { return target.ConsumerTransformer; }
            set { target.ConsumerTransformer = value; }
        }

        public ProducerTransformerDelegate ProducerTransformer
        {
            get { return target.ProducerTransformer; }
            set { target.ProducerTransformer = value; }
        }

        public TimeSpan RequestTimeout
        {
            get { return target.RequestTimeout; }
            set { target.RequestTimeout = value; }
        }

        public void Start()
        {
            // Handle start method: track started state.
            target.Start();
            using(singleConnectionFactory.ConnectionMonitor.Lock())
            {
                singleConnectionFactory.IsStarted = true;
            }
        }

        public async Task StartAsync()
        {
            await target.StartAsync().Awaiter();
            using(await singleConnectionFactory.ConnectionMonitor.LockAsync().Awaiter())
            {
                singleConnectionFactory.IsStarted = true;
            }
        }

        public void Stop()
        {
            //don't pass the call to the target as it would stop receiving for all clients sharing this connection.
        }

        public Task StopAsync()
        {
            //don't pass the call to the target as it would stop receiving for all clients sharing this connection.
            return Task.FromResult(true);
        }

        public ISession CreateSession()
        {
            return CreateSession(AcknowledgementMode.AutoAcknowledge);
        }

        public ISession CreateSession(AcknowledgementMode acknowledgementMode)
        {
            ISession session = singleConnectionFactory.GetSession(target, acknowledgementMode);
            if (session != null)
            {
                return session;
            }

            return target.CreateSession(acknowledgementMode);
        }

        public Task<ISession> CreateSessionAsync()
        {
            return CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        }

        public async Task<ISession> CreateSessionAsync(AcknowledgementMode acknowledgementMode)
        {
            ISession session = await singleConnectionFactory.GetSessionAsync(target, acknowledgementMode).Awaiter();
            if (session != null)
            {
                return session;
            }

            return await target.CreateSessionAsync(acknowledgementMode).Awaiter();
        }


        #region Pass through implementations to the target connection

        public void PurgeTempDestinations()
        {
            target.PurgeTempDestinations();
        }

        public event ExceptionListener ExceptionListener
        {
            add { target.ExceptionListener += value; }
            remove { target.ExceptionListener -= value; }
        }

        public event ConnectionInterruptedListener ConnectionInterruptedListener
        {
            add { target.ConnectionInterruptedListener += value; }
            remove { target.ConnectionInterruptedListener -= value; }
        }

        public event ConnectionResumedListener ConnectionResumedListener
        {
            add { target.ConnectionResumedListener += value; }
            remove { target.ConnectionResumedListener -= value; }
        }


        public AcknowledgementMode AcknowledgementMode
        {
            get { return target.AcknowledgementMode; }
            set { target.AcknowledgementMode = value; }
        }

        public void Dispose()
        {
            target.Dispose();
        }


        public bool IsStarted
        {
            get { return target.IsStarted; }
        }


        public IConnectionMetaData MetaData
        {
            get { return target.MetaData; }
        }


        public IRedeliveryPolicy RedeliveryPolicy
        {
            get { return target.RedeliveryPolicy; }
            set { target.RedeliveryPolicy = value; }
        }
        #endregion

        /// <summary>
        /// Add information to show this is a shared NMS connection
        /// </summary>
        /// <returns>Description of connection wrapper</returns>
        public override string ToString()
        {
            return "Shared NMS Connection: " + this.target;
        }
    }
}