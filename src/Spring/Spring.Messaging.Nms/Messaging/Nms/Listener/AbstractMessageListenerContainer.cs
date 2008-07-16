using System;
using System.Collections;
using Spring.Context;
using Spring.Messaging.Nms.Support;
using Spring.Messaging.Nms.Support.IDestinations;
using Spring.Util;
using Apache.NMS;

namespace Spring.Messaging.Nms.Listener
{
    public abstract class AbstractMessageListenerContainer : NmsDestinationAccessor, ILifecycle, IDisposable
    {
        private String clientId;

        private object destination;

        private String messageSelector;

        private object messageListener;

        private bool subscriptionDurable = false;

        private string durableSubscriptionName;

        private ExceptionListener exceptionListener;

        private bool exposeListenerISession = true;

        private bool autoStartup = true;

        private IConnection sharedConnection;


        private volatile bool active = false;

        private bool running = false;

        private IList pausedTasks = new Spring.Collections.LinkedList();

        private object lifecycleMonitor = new object();


        private object sharedConnectionMonitor = new object();

        #region Properties

        /// <summary> Set whether to automatically start the listener after initialization.
        /// <p>Default is "true"; set this to "false" to allow for manual startup.</p>
        /// </summary>
        virtual public bool AutoStartup
        {
            set { this.autoStartup = value; }
        }


        public string ClientId
        {
            set { clientId = value; }
            get { return clientId;  }
        }


        public IDestination Destination
        {
            get
            {
                return (this.destination is IDestination ? (IDestination) this.destination : null);
            }
            set
            {
                AssertUtils.ArgumentNotNull(value, "destination");
                destination = value;
                if (destination is ITopic && !(destination is IQueue))
                {
			        PubSubDomain = true;
		        }
                
            }
        }

        virtual public bool Active
        {
            get
            {
                lock (this.lifecycleMonitor)
                {
                    return this.active;
                }
            }

        }
        
        public string DestinationName
        {
            get
            {
                return (this.destination is string ? (string) this.destination : null);
	
            }
            set
            {
                AssertUtils.ArgumentNotNull(value, "destinationName must not be null");
                this.destination = value;
            }
        }


        public string MessageSelector
        {
            get { return messageSelector; }
            set { messageSelector = value; }
        }


        public object MessageListener
        {
            set
            {
                CheckMessageListener(value);
                if (durableSubscriptionName == null)
                {
                    // Use message listener class name as default name for a durable subscription.
                    durableSubscriptionName = value.GetType().FullName;
                }
                messageListener = value;
            }
            get
            {
                return messageListener;
            }
        }


        public bool SubscriptionDurable
        {
            get { return subscriptionDurable; }
            set { subscriptionDurable = value; }
        }


        public string DurableSubscriptionName
        {
            get
            {
                return durableSubscriptionName;
            }
            set
            {
                AssertUtils.ArgumentNotNull(value, "durableSubscriptionName must not be null");
                durableSubscriptionName = value;
            }
        }


        public ExceptionListener ExceptionListener
        {
            get { return exceptionListener; }
            set { exceptionListener = value; }
        }


        public bool ExposeListenerSession
        {
            get { return exposeListenerISession; }
            set { exposeListenerISession = value; }
        }


        public object LifecycleMonitor
        {
            get { return lifecycleMonitor; }
        }


        public IConnection SharedConnection
        {
            get
            {
                if (!SharedConnectionEnabled)
                {
                    throw new System.SystemException("This message listener container does not maintain a shared IConnection");
                }
                lock (this.sharedConnectionMonitor)
                {
                    if (this.sharedConnection == null)
                    {
                        //TODO SharedConnectionNotInitializedException
                        throw new ApplicationException("This message listener container's shared IConnection has not been initialized yet");
                    }
                    return this.sharedConnection;
                }
            }
        }

        #endregion

        public void Start()
        {
            DoStart();
        }

        private void DoStart()
        {
            lock (this.lifecycleMonitor)
            {
                running = true;
                System.Threading.Monitor.PulseAll(this.lifecycleMonitor);

                //TODO - PausedTasks
            }

            if (SharedConnectionEnabled)
            {
                StartSharedConnection();
            }
        }

        protected virtual void StartSharedConnection()
        {
            lock (sharedConnectionMonitor)
            {
                if (sharedConnection != null)
                {
                    try
                    {
                        sharedConnection.Start();
                    }
                    catch (Exception ex)
                    {
                        logger.Debug("Ignoring IConnection start exception - assuming already started", ex);
                    }
                }
            }
        }

        public void Stop()
        {
            DoStop();
        }

        public bool IsRunning
        {
            get
            {
                lock (lifecycleMonitor)
                {
                    return running;
                }
            }
        }

        public void Dispose()
        {
            Shutdown();
        }

        public virtual void Shutdown()
        {
            logger.Debug("Shutting down message listener container");
            bool wasRunning = false;
            lock (this.lifecycleMonitor)
            {
                wasRunning = this.running;
                this.running = false;
                this.active = false;
                System.Threading.Monitor.PulseAll(this.lifecycleMonitor);
            }
            try
            {
                DestroyListener();
            }
            finally
            {
                lock (this.sharedConnectionMonitor)
                {
                    NmsUtils.CloseConnection(this.sharedConnection, wasRunning);
                }
            }
        }

        protected virtual void DoStop()
        {
            lock (this.lifecycleMonitor)
            {
                this.running = false;
                System.Threading.Monitor.PulseAll(this.lifecycleMonitor);
            }

            if (SharedConnectionEnabled)
            {
                StopSharedConnection();
            }
        }

        protected virtual void StopSharedConnection()
        {
            lock (this.sharedConnectionMonitor)
            {
                if (this.sharedConnection != null)
                {
                    try
                    {
                        this.sharedConnection.Stop();
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        logger.Debug("Ignoring IConnection stop exception - assuming already stopped", ex);
                    }
                }
            }
        }


        #region Template methods for listeners
        public virtual void ExecuteListener(ISession session, IMessage message)
        {
            try
            {
                DoExecuteListener(session, message);
            }
            //UPGRADE_NOTE: Exception 'java.lang.Throwable' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
            catch (System.Exception ex)
            {
                HandleListenerException(ex);
            }
        }
        

        protected virtual void DoExecuteListener(ISession session, IMessage message)
        {
            try
            {
                InvokeListener(session, message);
            }
            catch (Exception ex)
            {
                RollbackOnExceptionIfNecessary(session, ex);
                throw;
            }
            CommitIfNecessary(session, message);
        }

        private void CommitIfNecessary(ISession session, IMessage message)
        {
            //TODO
            //logger.Info("CommitIfNecessary not implemented");
        }

        private void RollbackOnExceptionIfNecessary(ISession session, Exception ex)
        {
            //TODO
            //logger.Info("RollbackOnExceptionIfNecessary not implemented");
        }


        protected internal virtual void InvokeListener(ISession session, IMessage message)
        {
            if (MessageListener is ISessionAwareMessageListener)
            {
                DoInvokeListener((ISessionAwareMessageListener) MessageListener, session, message);
            }
            else
            {
                if (MessageListener is IMessageListener)
                {
                   DoInvokeListener((IMessageListener) MessageListener, message);
                }
                else
                {
                    throw new System.ArgumentException("Only IMessageListener and ISessionAwareMessageListener supported");
                }
            }
        }

        /// <summary>
        /// Invoke the specified listener as standard JMS MessageListener.
        /// </summary>
        /// <remarks>Default implementation performs a plain invocation of the
        /// <code>OnMessage</code> methods</remarks>
        /// <param name="listener">The listener to invoke.</param>
        /// <param name="message">The received message.</param>
        /// <exception cref="NMSException">if thronw by the underlying NMS APIs</exception>
        protected virtual void DoInvokeListener(IMessageListener listener, IMessage message)
        {
            listener.OnMessage(message);
        }

        /// <summary>
        /// Invoke the specified listener as Spring SessionAwareMessageListener,
        /// exposing a new NMS Session (potentially with its own transaction)
        /// to the listener if demanded.
        /// </summary>
        /// <param name="listener">The Spring ISessionAwareMessageListener to invoke.</param>
        /// <param name="session">The session to operate on.</param>
        /// <param name="message">The received message.</param>
        protected virtual void DoInvokeListener(ISessionAwareMessageListener listener, ISession session, IMessage message)
        {
            IConnection conToClose = null;
            ISession sessionToClose = null;
            try
            {
                ISession sessionToUse = session;
                if (!ExposeListenerSession)
                {
                    //We need to expose a separate Session.
                    conToClose = CreateConnection();
                    sessionToClose = CreateSession(conToClose);
                    sessionToUse = sessionToClose;
                }
                // Actually invoke the message listener
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Invoking listener with message of type [" + message.GetType() + 
                        "] and session [" + sessionToUse + "]");
                }
                listener.OnMessage(message, sessionToUse);
                if (sessionToUse != session)
                {
                    if (sessionToUse.Transacted)
                    {
                        NmsUtils.CommitIfNecessary(sessionToUse);
                    }                        
                }
            } finally
            {
                NmsUtils.CloseSession(sessionToClose);
                NmsUtils.CloseConnection(conToClose);
            }
        }
        
        protected  virtual void HandleListenerException(System.Exception ex)
        {
            if (ex is NMSException)
            {
                InvokeExceptionListener((NMSException)ex);
            }
            if (Active)
            {
                // Regular case: failed while active.
                // Log at error level.
                logger.Error("Execution of NMS message listener failed", ex);
            }
            else
            {
                // Rare case: listener thread failed after container shutdown.
                // Log at debug level, to avoid spamming the shutdown log.
                logger.Debug("Listener exception after container shutdown", ex);
            }
        }

        protected virtual void InvokeExceptionListener(NMSException ex)
        {
            ExceptionListener exceptionListener = ExceptionListener;
            if (exceptionListener != null)
            {
               exceptionListener(ex);
            }
        }
        
        #endregion

        public override void AfterPropertiesSet()
        {
            base.AfterPropertiesSet();

            if (this.destination == null)
            {
                throw new System.ArgumentException("destination or destinationName is required");
            }
            if (this.messageListener == null)
            {
                throw new System.ArgumentException("messageListener is required");
            }
            if (SubscriptionDurable && !PubSubDomain)
            {
                throw new System.ArgumentException("A durable subscription requires a topic (pub-sub domain)");
            }

            Initialize();
        }

        public virtual void Initialize()
        {
            try
            {
                lock (this.lifecycleMonitor)
                {
                    this.active = true;
                    System.Threading.Monitor.PulseAll(this.lifecycleMonitor);
                }

                if (SharedConnectionEnabled)
                {
                    EstablishSharedConnection();
                }

                if (this.autoStartup)
                {
                    DoStart();
                }

                RegisterListener();
            }
            catch (Exception)
            {
                lock (this.sharedConnectionMonitor)
                {
                    NmsUtils.CloseConnection(this.sharedConnection);
                }
                throw;
            }
        }

        protected virtual void EstablishSharedConnection()
        {
            RefreshSharedConnection();
        }

        protected void RefreshSharedConnection()
        {
            bool running = IsRunning;
            lock (this.sharedConnectionMonitor)
            {
                NmsUtils.CloseConnection(this.sharedConnection, running);
                
                IConnection con = CreateConnection();
                try
                {
                    PrepareSharedConnection(con);
                }
                catch (Exception)
                {
                    NmsUtils.CloseConnection(con);
                    throw;
                }
                this.sharedConnection = con;
            }
        }

        protected virtual void PrepareSharedConnection(IConnection connection)
        {
            if (ClientId != null)
            {
               connection.ClientId = ClientId;
            }
        }
        #region Template methods to be implemented by subclasses

        /// <summary> Return whether a shared NMS IConnection should be maintained
        /// by this listener container base class.
        /// </summary>
        /// <seealso cref="SharedConnection">
        /// </seealso>
        protected abstract bool SharedConnectionEnabled
        {
            get;
        }

        /// <summary> Register the specified listener on the underlying NMS IConnection.
        /// <p>Subclasses need to implement this method for their specific
        /// listener management process.</p>
        /// </summary>
        /// <throws>  NMSException if registration failed </throws>
        /// <seealso cref="IMessageListener">
        /// </seealso>
        /// <seealso cref="SharedConnection">
        /// </seealso>
        protected abstract void RegisterListener();

        /// <summary> Destroy the registered listener.
        /// The NMS IConnection will automatically be closed <i>afterwards</i>
        /// <p>Subclasses need to implement this method for their specific
        /// listener management process.</p>
        /// </summary>
        /// <throws>  NMSException if destruction failed </throws>
        protected abstract void DestroyListener();

        #endregion

        protected virtual IConnection CreateConnection()
        {
            return ConnectionFactory.CreateConnection();
        }

        protected virtual ISession CreateSession(IConnection con)
        {
            return con.CreateSession(SessionAcknowledgeMode);
        }

        protected virtual bool IsClientAcknowledge(ISession session)
        {
            return (session.AcknowledgementMode == AcknowledgementMode.ClientAcknowledge);
        }

        protected virtual void CheckMessageListener(System.Object messageListener)
        {
            AssertUtils.ArgumentNotNull(messageListener, "IMessage Listener can not be null");
            if (!(messageListener is IMessageListener || messageListener is ISessionAwareMessageListener))
            {
                throw new System.ArgumentException("messageListener needs to be of type [" + typeof(IMessageListener).FullName + "] or [" + typeof(ISessionAwareMessageListener).FullName + "]");
            }
        }
    }
}
