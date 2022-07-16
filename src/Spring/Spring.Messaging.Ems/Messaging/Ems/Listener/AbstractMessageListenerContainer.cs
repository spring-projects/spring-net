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
using Spring.Messaging.Ems.Common;
using Spring.Messaging.Ems.Core;
using Spring.Messaging.Ems.Support;
using Spring.Util;

namespace Spring.Messaging.Ems.Listener
{
    /// <summary>
    /// Abstract base class for message listener containers. Can either host
    /// a standard EMS MessageListener or a Spring-specific
    /// <see cref="ISessionAwareMessageListener"/>
    /// </summary>
    public abstract class AbstractMessageListenerContainer : AbstractListenerContainer
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(AbstractMessageListenerContainer));

        #endregion

        #region Fields

        private object destination;

        private String messageSelector;

        private object messageListener;

        private bool subscriptionDurable = false;

        private string durableSubscriptionName;

        private IExceptionListener exceptionListener;

        private IErrorHandler errorHandler;

        private bool exposeListenerSession = true;

        private bool acceptMessagesWhileStopping = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the destination to receive messages from. Will be <code>null</code>
        /// if the configured destination is not an actual Destination type;
        /// c.f. <see cref="DestinationName"/> when the destination is a String.
        /// </summary>
        /// <value>The destination.</value>
        public Destination Destination
        {
            get
            {
                return (this.destination is Destination ? (Destination) this.destination : null);
            }
            set
            {
                AssertUtils.ArgumentNotNull(value, "destination");
                destination = value;
                if (destination is Topic && !(destination is Queue))
                {
			        PubSubDomain = true;
		        }

            }
        }


        /// <summary>
        /// Gets or sets the name of the destination to receive messages from.
	    /// Will be <code>null</code> if the configured destination is not a
	    /// string  type; c.f. <see cref="Destination"/> when it is an actual Destination object.
        /// </summary>
        /// <value>The name of the destination.</value>
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


        /// <summary>
        /// Gets or sets the message selector.
        /// </summary>
        /// <value>The message selector expression (or <code>null</code> if none)..</value>
        public string MessageSelector
        {
            get { return messageSelector; }
            set { messageSelector = value; }
        }


        /// <summary>
        /// Gets or sets the message listener to register.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// This can be either a standard EMS MessageListener object or a
        /// Spring <see cref="ISessionAwareMessageListener"/> object.
        /// </para>
        /// </remarks>
        /// <value>The message listener.</value>
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


        /// <summary>
        /// Gets or sets a value indicating whether the subscription is durable.
        /// </summary>
        /// <remarks>
        /// Set whether to make the subscription durable. The durable subscription name
	    /// to be used can be specified through the "DurableSubscriptionName" property.
	    /// <para>Default is "false". Set this to "true" to register a durable subscription,
	    /// typically in combination with a "DurableSubscriptionName" value (unless
	    /// your message listener class name is good enough as subscription name).
	    /// </para>
	    /// <para>Only makes sense when listening to a topic (pub-sub domain).</para>
        /// </remarks>
        /// <value><c>true</c> if the subscription is durable; otherwise, <c>false</c>.</value>
        public bool SubscriptionDurable
        {
            get { return subscriptionDurable; }
            set { subscriptionDurable = value; }
        }


        /// <summary>
        /// Gets or sets the name of the durable subscription to create.
        /// </summary>
        /// <remarks>
        /// To be applied in case of a topic (pub-sub domain) with subscription durability activated.
	    /// The durable subscription name needs to be unique within this client's
	    /// client id. Default is the class name of the specified message listener.
	    /// <para>Note: Only 1 concurrent consumer (which is the default of this
	    /// message listener container) is allowed for each durable subscription.
	    /// </para>
        /// </remarks>
        /// <value>The name of the durable subscription.</value>
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


        /// <summary>
        /// Gets or sets the exception listener to notify in case of a EMSException thrown
	    /// by the registered message listener or the invocation infrastructure.
        /// </summary>
        /// <value>The exception listener.</value>
        public IExceptionListener ExceptionListener
        {
            get { return exceptionListener; }
            set { exceptionListener = value; }
        }

        /// <summary>
        /// Sets an ErrorHandler to be invoked in case of any uncaught exceptions thrown
        /// while processing a Message. By default there will be no ErrorHandler
        /// so that error-level logging is the only result.
        /// </summary>
        /// <value>The error handler.</value>
        public IErrorHandler ErrorHandler
        {
            set { errorHandler = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to expose listener session to a registered
        /// <see cref="ISessionAwareMessageListener"/> as well as to <see cref="EmsTemplate"/> calls.
        /// </summary>
        /// <remarks>
	    /// Default is "true", reusing the listener's Session.
	    /// Turn this off to expose a fresh Session fetched from the same
	    /// underlying Connection instead, which might be necessary
	    /// on some messaging providers.
	    /// <para>Note that Sessions managed by an external transaction manager will
	    /// always get exposed to <see cref="EmsTemplate"/>
	    /// calls. So in terms of EmsTemplate exposure, this setting only affects
	    /// locally transacted Sessions.
	    /// </para>
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if expose listener session; otherwise, <c>false</c>.
        /// </value>
        public bool ExposeListenerSession
        {
            get { return exposeListenerSession; }
            set { exposeListenerSession = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to accept messages while
        /// the listener container is in the process of stopping.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Return whether to accept received messages while the listener container
        /// receive attempt. Switch this flag on to fully process such messages
        /// even in the stopping phase, with the drawback that even newly sent
        /// messages might still get processed (if coming in before all receive
        /// timeouts have expired).
        /// </para>
        /// <para>
        /// Aborting receive attempts for such incoming messages
        /// might lead to the provider's retry count decreasing for the affected
        /// messages. If you have a high number of concurrent consumers, make sure
        /// that the number of retries is higher than the number of consumers,
        /// to be on the safe side for all potential stopping scenarios.
        /// </para>
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if accept messages while in the process of stopping; otherwise, <c>false</c>.
        /// </value>
        public bool AcceptMessagesWhileStopping
        {
            get { return acceptMessagesWhileStopping; }
            set { acceptMessagesWhileStopping = value; }
        }


        #endregion



        /// <summary>
        /// Validate that the destination is not null and that if the subscription is durable, then we are not
        /// using the Pub/Sub domain.
        /// </summary>
        protected override void ValidateConfiguration()
        {
            if (this.destination == null)
            {
                throw new ArgumentException("Property 'destination' or 'DestinationName' is required");
            }
            if (SubscriptionDurable && !PubSubDomain)
            {
                throw new ArgumentException("A durable subscription requires a topic (pub-sub domain)");
            }
        }

        #region Template methods for listeners



        /// <summary>
        /// Executes the specified listener,
        /// committing or rolling back the transaction afterwards (if necessary).
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="message">The received message.</param>
        /// <see cref="InvokeListener"/>
        /// <see cref="CommitIfNecessary"/>
        /// <see cref="RollbackOnExceptionIfNecessary"/>
        /// <see cref="HandleListenerException"/>
        public virtual void ExecuteListener(ISession session, Message message)
        {
            try
            {
                DoExecuteListener(session, message);
            }
            catch (Exception ex)
            {
                HandleListenerException(ex);
            }
        }

        /// <summary>
        /// Executes the specified listener,
        /// committing or rolling back the transaction afterwards (if necessary).
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="message">The received message.</param>
        /// <exception cref="EMSException">If thrown by EMS API methods.</exception>
        /// <see cref="InvokeListener"/>
        /// <see cref="CommitIfNecessary"/>
        /// <see cref="RollbackOnExceptionIfNecessary"/>
        protected virtual void DoExecuteListener(ISession session, Message message)
        {
            if (!AcceptMessagesWhileStopping && !IsRunning)
            {
                #region Logging
                if (logger.IsWarnEnabled)
                {
                    logger.Warn("Rejecting received message because of the listener container " +
                        "having been stopped in the meantime: " + message);
                }
                #endregion
                RollbackIfNecessary(session);
                throw new MessageRejectedWhileStoppingException();
            }

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

        /// <summary>
        /// Invokes the specified listener: either as standard EMS MessageListener
        /// or (preferably) as Spring ISessionAwareMessageListener.
        /// </summary>
        /// <param name="session">The session to operate on.</param>
        /// <param name="message">The received message.</param>
        /// <exception cref="EMSException">If thrown by EMS API methods.</exception>
        /// <see cref="MessageListener"/>
        protected virtual void InvokeListener(ISession session, Message message)
        {
            object listener = MessageListener;
            if (listener is ISessionAwareMessageListener)
            {
                DoInvokeListener((ISessionAwareMessageListener) listener, session, message);
            }

            else if (listener is IMessageListener)
            {
                DoInvokeListener((IMessageListener)listener, message);
            }
            else if (listener != null)
            {
                throw new ArgumentException("Only MessageListener and ISessionAwareMessageListener supported");
            }
            else
            {
                throw new InvalidOperationException("No message listener specified - see property MessageListener");
            }
        }

        /// <summary>
        /// Invoke the specified listener as Spring ISessionAwareMessageListener,
        /// exposing a new EMS Session (potentially with its own transaction)
        /// to the listener if demanded.
        /// </summary>
        /// <param name="listener">The Spring ISessionAwareMessageListener to invoke.</param>
        /// <param name="session">The session to operate on.</param>
        /// <param name="message">The received message.</param>
        /// <exception cref="EMSException">If thrown by EMS API methods.</exception>
        /// <see cref="ISessionAwareMessageListener"/>
        /// <see cref="ExposeListenerSession"/>
        protected virtual void DoInvokeListener(ISessionAwareMessageListener listener, ISession session, Message message)
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
                // Clean up specially exposed Session, if any
                if (sessionToUse != session)
                {
                    if (sessionToUse.Transacted && SessionTransacted)
                    {
                        // Transacted session created by this container -> commit.
                        EmsUtils.CommitIfNecessary(sessionToUse);
                    }
                }
            } finally
            {
                EmsUtils.CloseSession(sessionToClose);
                EmsUtils.CloseConnection(conToClose);
            }
        }

        /// <summary>
        /// Invoke the specified listener as standard JMS MessageListener.
        /// </summary>
        /// <remarks>Default implementation performs a plain invocation of the
        /// <code>OnMessage</code> methods</remarks>
        /// <param name="listener">The listener to invoke.</param>
        /// <param name="message">The received message.</param>
        /// <exception cref="EMSException">if thrown by the EMS API methods</exception>
        protected virtual void DoInvokeListener(IMessageListener listener, Message message)
        {
            listener.OnMessage(message);
        }

        /// <summary>
        /// Perform a commit or message acknowledgement, as appropriate
        /// </summary>
        /// <param name="session">The session to commit.</param>
        /// <param name="message">The message to acknowledge.</param>
        /// <exception cref="EMSException">In case of commit failure</exception>
        protected virtual void CommitIfNecessary(ISession session, Message message)
        {
            // Commit session or acknowledge message
            if (session.Transacted)
            {
                // Commit necessary - but avoid commit call is Session transaction is externally coordinated.
                if (IsSessionLocallyTransacted(session))
                {
                    EmsUtils.CommitIfNecessary(session);
                }
            }
            else if (IsClientAcknowledge(session))
            {
                message.Acknowledge();
            }
        }

        /// <summary>
        /// Determines whether the given Session is locally transacted, that is, whether
        /// its transaction is managed by this listener container's Session handling
        /// and not by an external transaction coordinator.
        /// </summary>
        /// <remarks>
        /// The Session's own transacted flag will already have been checked
        /// before. This method is about finding out whether the Session's transaction
        /// is local or externally coordinated.
        /// </remarks>
        /// <param name="session">The session to check.</param>
        /// <returns>
        /// 	<c>true</c> if the is session locally transacted; otherwise, <c>false</c>.
        /// </returns>
        /// <see cref="EmsAccessor.SessionTransacted"/>
        protected virtual bool IsSessionLocallyTransacted(ISession session)
        {
            return SessionTransacted;
        }


        /// <summary>
        /// Perform a rollback, if appropriate.
        /// </summary>
        /// <param name="session">The session to rollback.</param>
        /// <exception cref="EMSException">In case of a rollback error</exception>
        protected virtual void RollbackIfNecessary(ISession session)
        {
            if (session.Transacted && IsSessionLocallyTransacted(session))
            {
                // Transacted session created by this container -> rollback
                EmsUtils.RollbackIfNecessary(session);
            }
            else if (IsClientAcknowledge(session))
            {
                session.Recover();
            }
        }
        /// <summary>
        /// Perform a rollback, handling rollback excepitons properly.
        /// </summary>
        /// <param name="session">The session to rollback.</param>
        /// <param name="ex">The thrown application exception.</param>
        /// <exception cref="EMSException">in case of a rollback error.</exception>
        protected virtual void RollbackOnExceptionIfNecessary(ISession session, Exception ex)
        {
            try
            {
                if (session.Transacted && IsSessionLocallyTransacted(session))
                {
                    // Transacted session created by this container -> rollback
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Initiating transaction rollback on application exception");
                    }
                    EmsUtils.RollbackIfNecessary(session);
                }
                else if (IsClientAcknowledge(session))
                {
                    session.Recover();
                }
            }
            catch (EMSException)
            {
                logger.Error("Application exception overriden by rollback exception", ex);
                throw;
            }
        }


        /// <summary>
        /// Handle the given exception that arose during listener execution.
        /// </summary>
        /// <remarks>
        /// The default implementation logs the exception at error level,
        /// not propagating it to the JMS provider - assuming that all handling of
        /// acknowledgement and/or transactions is done by this listener container.
        /// This can be overridden in subclasses.
        /// </remarks>
        /// <param name="ex">The exceptin to handle</param>
        protected virtual void HandleListenerException(Exception ex)
        {
            if (ex is MessageRejectedWhileStoppingException)
            {
                // Internal exception - has been handled before.
                return;
            }
            if (ex is EMSException)
            {
                InvokeExceptionListener((EMSException)ex);
            }
            if (Active)
            {
                // Regular case: failed while active.
                // Invoke ErrorHandler if available.
                InvokeErrorHandler(ex);
            }
            else
            {
                // Rare case: listener thread failed after container shutdown.
                // Log at debug level, to avoid spamming the shutdown log.
                logger.Debug("Listener exception after container shutdown", ex);
            }
        }

        protected virtual void InvokeErrorHandler(Exception exception)
        {
            if (errorHandler != null)
            {
                errorHandler.HandleError(exception);
            }
            else if (logger.IsWarnEnabled)
            {
                logger.Warn("Execution of EMS message listener failed, and no ErrorHandler has been set.", exception);
            }
        }

        /// <summary>
        /// Invokes the registered exception listener, if any.
        /// </summary>
        /// <param name="ex">The exception that arose during EMS processing.</param>
        /// <see cref="ExceptionListener"/>
        protected virtual void InvokeExceptionListener(EMSException ex)
        {
            IExceptionListener exListener = ExceptionListener;
            if (exListener != null)
            {
                exListener.OnException(ex);
            }
        }

        #endregion

        /// <summary>
        /// Checks the message listener, throwing an exception
	    /// if it does not correspond to a supported listener type.
	    /// By default, only a standard JMS MessageListener object or a
	    /// Spring <see cref="ISessionAwareMessageListener"/> object will be accepted.
        /// </summary>
        /// <param name="messageListener">The message listener.</param>
        protected virtual void CheckMessageListener(object messageListener)
        {
            AssertUtils.ArgumentNotNull(messageListener, "Message Listener can not be null");
            if (!(messageListener is IMessageListener || messageListener is ISessionAwareMessageListener))
            {
                throw new ArgumentException("messageListener needs to be of type [" + typeof(IMessageListener).FullName + "] or [" + typeof(ISessionAwareMessageListener).FullName + "]");
            }
        }
    }

    /// <summary>
    /// Internal exception class that indicates a rejected message on shutdown.
    /// Used to trigger a rollback for an external transaction manager in that case.
    /// </summary>
    internal class MessageRejectedWhileStoppingException : ApplicationException
    {
    }
}
