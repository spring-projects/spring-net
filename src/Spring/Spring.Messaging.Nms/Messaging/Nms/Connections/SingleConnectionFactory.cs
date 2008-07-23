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

namespace Spring.Messaging.Nms.Connection
{
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
        /// Synchronization monitor for the shared Connection
        /// </summary>
        private object connectionMonitor = new object();

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
            connection = GetSharedConnection(this, target);
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
        /// 	<c>true</c> if [reconnect on exception]; otherwise, <c>false</c>.
        /// </value>
        public bool ReconnectOnException
        {
            get { return reconnectOnException; }
            set { reconnectOnException = value; }
        }

        #endregion

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

        public IConnection CreateConnection(string userName, string password)
        {
            throw new InvalidOperationException("SingleConnectionFactory does not support custom username and password.");
        }

        #endregion

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
                this.connection = GetSharedConnection(this, target);
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

        protected virtual void PrepareConnection(IConnection con)
        {
            if (ClientId != null)
            {
                con.ClientId = ClientId;
            }
            if (ExceptionListener != null || ReconnectOnException)
            {
                IExceptionListener listenerToUse = ExceptionListener;
                if (ReconnectOnException)
                {
                    InternalChainedExceptionListener chained = new InternalChainedExceptionListener(this, listenerToUse);
                    con.ExceptionListener += chained.OnException;
                }
                else
                {
                    if (ExceptionListener != null)
                    {
                        con.ExceptionListener += ExceptionListener.OnException;                                     
                    }
                }
            }
        }

        /// <summary>
        /// Template method for obtaining a (potentially cached) Session.
        /// </summary>
        /// <param name="con">The connection to operate on.</param>
        /// <param name="mode">The session ack mode.</param>
        /// <returns>the Session to use, or <code>null</code> to indicate
	    /// creation of a default Session</returns>  
        public virtual ISession GetSession(IConnection con, AcknowledgementMode mode)
        {
            return null;
        }

        protected virtual IConnection DoCreateConnection()
        {
            return TargetConnectionFactory.CreateConnection();
        }

        protected virtual void CloseConnection(IConnection con)
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
                LOG.Warn("Could not close shared NMS connection.", ex);
            }
        }

        #region IInitializingObject Members

        public void AfterPropertiesSet()
        {
            if (connection == null && TargetConnectionFactory == null)
            {
                throw new ArgumentException("Connection or 'TargetConnectionFactory' is required.");
            }
        }

        #endregion

        public void Dispose()
        {
            ResetConnection();
        }

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

        protected virtual IConnection GetSharedConnection(SingleConnectionFactory singleConnectionFactory, IConnection target)
        {
            lock (connectionMonitor)
            {
                return new CloseSupressingConnection(singleConnectionFactory, target);
            }
        }
    }

    internal class InternalChainedExceptionListener : ChainedExceptionListener, IExceptionListener
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

        public void Close()
        {
            // don't pass the call to the target.
        }

        public void Stop()
        {
            //don't pass the call to the target.
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
            return target.CreateSession();
        }

        #region Pass through implementations to the target connection


        public event ExceptionListener ExceptionListener
        {
            add { target.ExceptionListener += value; }
            remove { target.ExceptionListener -= value; }
        }


        public AcknowledgementMode AcknowledgementMode
        {
            get { return target.AcknowledgementMode; }
            set { target.AcknowledgementMode = value; }
        }

        public string ClientId
        {
            get { return target.ClientId; }
            set
            {
                string currentClientId = target.ClientId;
                if (currentClientId != null && currentClientId.Equals(value))
                {
                    //ok
                }
                else
                {
                    throw new ArgumentException(
                        "Setting of 'ClientID' property not supported on wrapper for shared Connection." +
                        "Set the 'ClientId' property on the SingleConnectionFactory instead.");    
                }
                
            }
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
        #endregion

    }
}