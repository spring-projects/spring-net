using System;
using System.Collections;
using Spring.Context;
using Spring.Messaging.Nms.Connection;
using Spring.Messaging.Nms.Support;
using Spring.Messaging.Nms.Support.IDestinations;
using Spring.Util;
using Apache.NMS;

namespace Spring.Messaging.Nms.Listener
{
    public abstract class AbstractMessageListenerContainer : AbstractNmsListeningContainer
    {
        private object destination;

        private String messageSelector;

        private object messageListener;

        private bool subscriptionDurable = false;

        private string durableSubscriptionName;

        private ExceptionListener exceptionListener;

        private bool exposeListenerISession = true;




        private IList pausedTasks = new Spring.Collections.LinkedList();

        #region Properties

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




        #endregion



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







        #region Template methods to be implemented by subclasses




        #endregion

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
