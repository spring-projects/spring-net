#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

using System;
using Apache.NMS;
using Common.Logging;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Messaging.Nms.IConnections
{
    public class SingleConnectionFactory : IConnectionFactory, IInitializingObject, IDisposable
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (SingleConnectionFactory));

        #endregion

        private IConnectionFactory targetConnectionFactory;

        private string clientId;

        private ExceptionListener exceptionListenerDelegate;

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
        /// Synchronization monitor for the shared Connection
        /// </summary>
        private object connectionMonitor = new object();


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
            AssertUtils.ArgumentNotNull(target, "connection", "Target Connection must not be null");
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
                                        "Target ConnectionFactory must not be null");
            this.targetConnectionFactory = targetConnectionFactory;
        }

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
        /// Gets or sets the exception listener delegate that should be registered with
        /// the single connection created by this factory.
        /// </summary>
        /// <value>The exception listener delegate.</value>
        public ExceptionListener ExceptionListenerDelegate
        {
            get { return exceptionListenerDelegate; }
            set { exceptionListenerDelegate = value; }
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
        /// 	<c>true</c> if [reconnect on exception]; otherwise, <c>false</c>.
        /// </value>
        public bool ReconnectOnException
        {
            get { return reconnectOnException; }
            set { reconnectOnException = value; }
        }

        #region IConnectionFactory Members

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
                    LOG.Info("Established shared NMS Connection: " + this.target);
                }
                this.connection = GetSharedConnection(this.target);
            }
        }

        protected virtual void PrepareConnection(IConnection con)
        {
            if (ClientId != null)
            {
                con.ClientId = ClientId;
            }
            if (ExceptionListenerDelegate != null || ReconnectOnException)
            {
                ExceptionListener listenerToUse = ExceptionListenerDelegate;
                if (ReconnectOnException)
                {
                    InternalChainedExceptionListenerSupport chained = new InternalChainedExceptionListenerSupport(this, listenerToUse);
                    con.ExceptionListener += chained.OnException;
                }

            }
        }

        protected virtual IConnection DoCreateConnection()
        {
            return TargetConnectionFactory.CreateConnection();
        }

        private void CloseConnection(IConnection con)
        {
            try
            {
                try
                {
                    con.Stop();
                } finally
                {
                    con.Close();
                }
            } catch (Exception ex)
            {
                LOG.Warn("Could not close shared NMS connection.");
            }
        }

        public IConnection CreateConnection(string userName, string password)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IInitializingObject Members

        public void AfterPropertiesSet()
        {
            if (connection == null && TargetConnectionFactory == null)
            {
                throw new ArgumentException("Connection or 'TargetConnectionFactory' is required.");
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            ResetConnection();
        }

        public void ResetConnection()
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

        #endregion

        protected virtual IConnection GetSharedConnection(IConnection target)
        {
            lock (connectionMonitor)
            {
                return new CloseSupressingConnection(target);
            }
        }
    }

    internal class InternalChainedExceptionListenerSupport
    {
        private SingleConnectionFactory factory;
        private ExceptionListener listenerToUse;
        public InternalChainedExceptionListenerSupport(SingleConnectionFactory factory, ExceptionListener listenerToUse)
        {
            this.factory = factory;
            this.listenerToUse = listenerToUse;
        }

        public void OnException(Exception exception)
        {
            //TODO exception mgmt.
        }
    }

    internal class CloseSupressingConnection : IConnection
    {
        private IConnection target;

        public CloseSupressingConnection(IConnection target)
        {
            this.target = target;
        }

        public event ExceptionListener ExceptionListener
        {
            add { target.ExceptionListener += value; }
            remove { target.ExceptionListener -= value; }
        }

        public ISession CreateSession()
        {
            return target.CreateSession();
        }

        public ISession CreateSession(AcknowledgementMode acknowledgementMode)
        {
            return target.CreateSession(acknowledgementMode);
        }

        public void Close()
        {
            // don't pass the call to the target.
        }

        public AcknowledgementMode AcknowledgementMode
        {
            get { return target.AcknowledgementMode; }
            set { target.AcknowledgementMode = value; }
        }

        public string ClientId
        {
            get { return target.ClientId; }
            set { target.ClientId = value; }
        }

        public void Dispose()
        {
            target.Dispose();
        }

        public void Start()
        {
            target.Start();
        }

        public bool IsStarted
        {
            get { return target.IsStarted; }
        }

        public void Stop()
        {
            //don't pass the call to the target.
        }
    }
}