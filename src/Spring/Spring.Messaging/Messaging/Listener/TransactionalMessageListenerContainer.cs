#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using Spring.Data.Core;
using Spring.Messaging.Core;
using Spring.Transaction;
using Spring.Transaction.Support;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// A MessageListenerContainer that uses local (non-DTC) based transactions.  Exceptions are
    /// handled by instances of <see cref="IMessageTransactionExceptionHandler"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This container distinguishes between two types of <see cref="IPlatformTransactionManager"/>
    /// implementations.
    /// </para>
    /// <para>If you specify a <see cref="MessageQueueTransactionManager"/> then
    /// a MSMQ <see cref="MessageQueueTransaction"/> will be started
    /// before receiving the message and used as part of the container's recieve operation.  The
    /// <see cref="MessageQueueTransactionManager"/> binds the <see cref="MessageQueueTransaction"/>
    /// to thread local storage and as such will implicitly be used by
    /// <see cref="MessageQueueTemplate"/> send and receive operations to a transactional queue.
    /// </para>
    /// <para>
    /// Service layer operations that are called inside the message listener will typically
    /// be transactional based on using standard Spring declarative transaction management
    /// functionality.  In case of exceptions in the service layer, the database operation
    /// will have been rolled back and the <see cref="IMessageTransactionExceptionHandler"/>
    /// that is later invoked should decide to either commit the surrounding local
    /// MSMQ based transaction (removing the message from the queue) or to rollback
    /// (placing the message back on the queue for redelivery).
    /// </para>
    /// <para>
    /// The use of a transactional service layer in combination with
    /// a container managed <see cref="MessageQueueTransaction"/> is a powerful combination
    /// that can be used to achieve "exactly one" transaction message processing with
    /// database operations that are commonly associated with using transactional messaging and
    /// distributed transactions (i.e. both the messaging and database operation commit or rollback
    /// together).
    /// </para>
    /// <para>
    /// The additional programming logic needed to achieve this is to keep track of the Message.Id
    /// that has been processed successfully within the transactional service layer.
    /// This is needed as there may be a system failure (e.g. power goes off)
    /// between the 'inner' database commit and the 'outer' messaging commit, resulting
    /// in message redelivery.  The transactional service layer needs logic to detect if incoming
    /// message was processed successfully. It can do this by checking the database for an
    /// indication of successfull processing, perhaps by recording the Message.Id itself in a
    /// status table.  If the transactional service layer determines that the message has
    /// already been processed, it can throw a specific exception for thise case.  The
    /// container's exception handler will recognize this exception type and vote to commit
    /// (remove from the queue) the 'outer' messaging transaction.
    /// Spring provides an exception handler with this functionality,
    /// see <see cref="SendToQueueExceptionHandler"/> for more information.
    /// </para>
    /// <para>If you specify an implementation of <see cref="IResourceTransactionManager"/>
    /// (e.g. <see cref="AdoPlatformTransactionManager"/> or HibernateTransactionManager) then
    /// an local database transaction will be started before receiving the message.  By default,
    /// the container will also start a local <see cref="MessageQueueTransaction"/>
    /// after the local database transaction has started, but before the receiving the message.
    /// The <see cref="MessageQueueTransaction"/> will be used to receive the message.
    /// If you do not want his behavior set <see cref="UseContainerManagedMessageQueueTransaction"/>
    /// to false.  Also by default, the <see cref="MessageQueueTransaction"/>
    /// will be bound to thread local storage such that any <see cref="MessageQueueTemplate"/>
    /// send or recieve operations will participate transparently in the same
    /// <see cref="MessageQueueTransaction"/>.  If you do not want this behavior
    /// set the property <see cref="ExposeContainerManagedMessageQueueTransaction"/> to false.
    /// </para>
    /// <para>In case of exceptions during <see cref="IMessageListener"/> processing
    /// when using an implementation of
    /// <see cref="IResourceTransactionManager"/> (e.g. and starting a container managed
    /// <see cref="MessageQueueTransaction"/>) the container's
    /// <see cref="IMessageTransactionExceptionHandler"/> will determine if the
    /// <see cref="MessageQueueTransaction"/> should commit (removing it from the queue)
    /// or rollback (placing it back on the queue for redelivery).  The listener
    /// exception will always
    /// trigger a rollback in the 'outer' (e.g. <see cref="AdoPlatformTransactionManager"/>
    /// or HibernateTransactionManager) based transaction.
    /// </para>
    /// <para>
    /// PoisonMessage handing, that is endless redelivery of a message due to exceptions
    /// during processing, can be detected using implementatons of the
    /// <see cref="IMessageTransactionExceptionHandler"/> interface.  A specific implementation
    /// is provided that will move the poison message to another queue after a maximum number
    /// of redelivery attempts.  See <see cref="SendToQueueExceptionHandler"/> for more information.
    /// </para>
    /// </remarks>
    public class TransactionalMessageListenerContainer : AbstractTransactionalMessageListenerContainer
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (TransactionalMessageListenerContainer));

        #endregion

        #region Fields

        private bool useContainerManagedMessageQueueTransaction = false;

        private bool useMessageQueueTransactionManagerCalled = false;

        private bool exposeContainerManagedMessageQueueTransaction = true;

        private IMessageTransactionExceptionHandler messageTransactionExceptionHandler;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the MessageListenerContainer should be
        /// responsible for creating a MessageQueueTransaction
        /// when receiving a message.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Creating MessageQueueTransactions is usually the responsibility of the
        /// IPlatformTransactionManager, e.g. TxScopePlatformTransactionManager (when using DTC)
        /// or MessageQueueTransactionManager (when using local messaging transactions).
        /// </para>
        /// <para>
        /// For all other IPlatformTransactionManager implementations, including when none is
        /// specified, the MessageListenerContainer will itself create a MessageQueueTransaction
        /// (assuming the container is consuming from a transactional queue).
        /// </para>
        /// <para>
        /// Set the ExposeContainerManagedMessageQueueTransaction property to true if you want
        /// the MessageQueueTransaction to be exposed to Spring's MessageQueueTemplate class
        /// </para>
        /// </remarks>
        /// <value>
        /// 	<c>true</c> to use a container managed MessageQueueTransaction; otherwise, <c>false</c>.
        /// </value>
        public bool UseContainerManagedMessageQueueTransaction
        {
            get { return useContainerManagedMessageQueueTransaction; }
            set
            {
                useContainerManagedMessageQueueTransaction = value;
                useMessageQueueTransactionManagerCalled = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether expose the
        /// container managed <see cref="MessageQueueTransaction"/> to thread local storage
        /// where it will be automatically used by <see cref="MessageQueueTemplate"/> send
        /// and receive operations.
        /// </summary>
        /// <remarks>
        /// Using an <see cref="MessageQueueTransactionManager"/> will always exposes a
        /// <see cref="MessageQueueTransaction"/> to thread local storage.  This property
        /// only has effect when using a non-DTC based
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if [expose container managed message queue transaction]; otherwise, <c>false</c>.
        /// </value>
        public bool ExposeContainerManagedMessageQueueTransaction
        {
            get { return exposeContainerManagedMessageQueueTransaction; }
            set { exposeContainerManagedMessageQueueTransaction = value; }
        }

        /// <summary>
        /// Gets or sets the message transaction exception handler.
        /// </summary>
        /// <value>The message transaction exception handler.</value>
        public IMessageTransactionExceptionHandler MessageTransactionExceptionHandler
        {
            get { return messageTransactionExceptionHandler; }
            set { messageTransactionExceptionHandler = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determine if the container should create its own
        /// MessageQueueTransaction when a IResourceTransactionManager is specified.
        /// Set the transaction name to the name of the spring object.
        /// Call base class Initialize() funtionality
        /// </summary>
        public override void Initialize()
        {
            //using non-DTC based transaction manager?
            bool isRtm = PlatformTransactionManager is IResourceTransactionManager;
            //using MessageQueueTransactionManager?
            bool isQtm = PlatformTransactionManager is MessageQueueTransactionManager;

            if (!isRtm && !isQtm)
            {
                throw new ArgumentException("Can not use the provied IPlatformTransactionManager of type "
                    + PlatformTransactionManager.GetType()
                    + ".  It must implement IResourceTransactionManager or be a MessageQueueTransactionManager.");
            }

            //Set useContainerManagedMessageQueueTransaction = true when using
            // 1. non-DTC based transaction manager
            // 2. not the MessageQueueTransactionManager.
            if (!useMessageQueueTransactionManagerCalled && isRtm && !isQtm)
            {
                useContainerManagedMessageQueueTransaction = true;
            }

            // Use object name as default transaction name.
            if (TransactionDefinition.Name == null)
            {
                TransactionDefinition.Name = ObjectName;
            }

            // Proceed with superclass initialization.
            base.Initialize();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Does the receive and execute using platform transaction manager.
        /// </summary>
        /// <param name="mq">The message queue.</param>
        /// <param name="status">The transactional status.</param>
        /// <returns>
        /// true if should continue peeking, false otherwise.
        /// </returns>
        protected override bool DoReceiveAndExecuteUsingPlatformTransactionManager(MessageQueue mq,
                                                                                   ITransactionStatus status)
        {
            if (PlatformTransactionManager is MessageQueueTransactionManager)
            {
                return DoRecieveAndExecuteUsingMessageQueueTransactionManager(mq, status);
            }
            else if (PlatformTransactionManager is IResourceTransactionManager)
            {
                if (UseContainerManagedMessageQueueTransaction)
                {
                    return DoRecieveAndExecuteUsingResourceTransactionManagerWithTxQueue(mq, status);
                }
                else
                {
                    //recieve non-transactionally from transactional queue but
                    //use ResourceBasedTransactionManagement.
                    DoRecieveAndExecuteUsingResourceTransactionManager();
                }
            }
            return false;
        }

        private void DoRecieveAndExecuteUsingResourceTransactionManager()
        {
            //This is a bit of an odd case since really one is better off using
            //NonTransactionalMessageListenerContainer and having the database
            //transaction done in the service tier.

            throw new NotSupportedException("Try using NonTransactionalMessageListenerContainer instead.");
        }

        /// <summary>
        /// Does the recieve and execute using message queue transaction manager.
        /// </summary>
        /// <param name="mq">The message queue.</param>
        /// <param name="status">The transactional status.</param>
        /// <returns>true if should continue peeking, false otherise</returns>
        protected virtual bool DoRecieveAndExecuteUsingMessageQueueTransactionManager(MessageQueue mq,
                                                                                      ITransactionStatus status)
        {
            #region Logging

            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing DoRecieveAndExecuteUsingMessageQueueTransactionManager");
            }

            #endregion Logging

            Message message;

            #region Receive message

            try
            {
                message = mq.Receive(TimeSpan.Zero, QueueUtils.GetMessageQueueTransaction(null));
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    //expected to occur occasionally
                    if (LOG.IsTraceEnabled)
                    {
                        LOG.Trace("IOTimeout: Message to receive was already processed by another thread.");
                    }
                    status.SetRollbackOnly();
                    return false; // no more peeking unless this is the last listener thread
                }
                else
                {
                    // A real issue in receiving the message

                    #region Logging

                    if (LOG.IsErrorEnabled)
                    {
                        LOG.Error("Error receiving message from DefaultMessageQueue [" + mq.Path +
                                  "], closing queue and clearing connection cache.");
                    }

                    #endregion

                    lock (messageQueueMonitor)
                    {
                        mq.Close();
                        MessageQueue.ClearConnectionCache();
                    }
                    throw; // will cause rollback in MessageQueueTransactionManager and log exception
                }
            }

            #endregion

            if (message == null)
            {
                #region Logging

                if (LOG.IsTraceEnabled)
                {
                    LOG.Trace("Message recieved is null from Queue = [" + mq.Path + "]");
                }

                #endregion

                status.SetRollbackOnly();
                return false; // no more peeking unless this is the last listener thread
            }

            try
            {
                #region Logging

                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Received message [" + message.Id + "] on queue [" + mq.Path + "]");
                }

                #endregion

                MessageReceived(message);
                DoExecuteListener(message);

                #region Logging

                if (LOG.IsTraceEnabled)
                {
                    LOG.Trace("MessageListener executed");
                }

                #endregion
            }
            catch (Exception ex)
            {
                //Exception may indicate rollback of database transaction in service layer.
                //Let the handler determine if the message should be removed from the queue.
                TransactionAction action =
                    HandleTransactionalListenerException(ex, message, QueueUtils.GetMessageQueueTransaction(null));
                if (action == TransactionAction.Rollback)
                {
                    #region Logging

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug(
                            "Exception handler's TransactionAction has rolled back MessageQueueTransaction for queue [" +
                            mq.Path + "]");
                    }

                    #endregion

                    status.SetRollbackOnly();
                    return false; // no more peeking unless this is the last listener thread
                }
                else
                {
                    LOG.Info("Committing MessageQueueTransaction due to explicit commit request by exception handler.");
                }
            }
            finally
            {
                message.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Does the recieve and execute using a local MessageQueueTransaction.
        /// </summary>
        /// <param name="mq">The mqessage queue.</param>
        /// <param name="status">The transactional status.</param>
        /// <returns>true if should continue peeking, false otherwise.</returns>
        protected virtual bool DoRecieveAndExecuteUsingResourceTransactionManagerWithTxQueue(MessageQueue mq,
                                                                                             ITransactionStatus status)
        {
            #region Logging

            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing DoRecieveAndExecuteUsingResourceTransactionManagerWithTxQueue");
            }

            #endregion Logging

            using (MessageQueueTransaction messageQueueTransaction = new MessageQueueTransaction())
            {
                messageQueueTransaction.Begin();

                #region Logging

                if (LOG.IsTraceEnabled)
                {
                    LOG.Trace("Started MessageQueueTransaction for queue = [" + mq.Path + "]");
                }

                #endregion

                Message message;

                #region ReceiveMessage

                try
                {
                    #region Logging

                    if (LOG.IsTraceEnabled)
                    {
                        LOG.Trace("Receiving message with zero timeout for queue = [" + mq.Path + "]");
                    }

                    #endregion

                    message = mq.Receive(TimeSpan.Zero, messageQueueTransaction);
                }
                catch (MessageQueueException ex)
                {
                    if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                    {
                        //expected to occur occasionally

                        #region Logging

                        if (LOG.IsTraceEnabled)
                        {
                            LOG.Trace(
                                "MessageQueueErrorCode.IOTimeout: No message available to receive.  May have been processed by another thread.");
                        }

                        #endregion

                        status.SetRollbackOnly();
                        return false; // no more peeking unless this is the last listener thread
                    }
                    else
                    {
                        // A real issue in receiving the message

                        #region Logging

                        if (LOG.IsErrorEnabled)
                        {
                            LOG.Error("Error receiving message from DefaultMessageQueue [" + mq.Path +
                                      "], closing queue and clearing connection cache.");
                        }

                        #endregion

                        lock (messageQueueMonitor)
                        {
                            mq.Close();
                            MessageQueue.ClearConnectionCache();
                        }
                        throw; // will cause rollback in surrounding platform transaction manager and log exception
                    }
                }

                #endregion

                if (message == null)
                {
                    #region Logging

                    if (LOG.IsTraceEnabled)
                    {
                        LOG.Trace("Message recieved is null from Queue = [" + mq.Path + "]");
                    }

                    #endregion

                    status.SetRollbackOnly();
                    return false; // no more peeking unless this is the last listener thread
                }

                try
                {
                    #region Logging

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Received message [" + message.Id + "] on queue [" + mq.Path + "]");
                    }

                    #endregion

                    MessageReceived(message);

                    if (ExposeContainerManagedMessageQueueTransaction)
                    {
                        TransactionSynchronizationManager.BindResource(
                            MessageQueueTransactionManager.CURRENT_TRANSACTION_SLOTNAME,
                            new LocallyExposedMessageQueueResourceHolder(messageQueueTransaction));
                    }

                    DoExecuteListener(message);

                    #region Logging

                    if (LOG.IsTraceEnabled)
                    {
                        LOG.Trace("MessageListener executed");
                    }

                    #endregion

                    messageQueueTransaction.Commit();

                    #region Logging

                    if (LOG.IsTraceEnabled)
                    {
                        LOG.Trace("Committed MessageQueueTransaction for queue [" + mq.Path + "]");
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    TransactionAction action =
                        HandleTransactionalListenerException(ex, message, messageQueueTransaction);
                    if (action == TransactionAction.Rollback)
                    {
                        messageQueueTransaction.Abort();

                        #region Logging

                        if (LOG.IsDebugEnabled)
                        {
                            LOG.Debug(
                                "Exception handler's TransactionAction has rolled back MessageQueueTransaction for queue [" +
                                mq.Path + "]");
                        }

                        #endregion
                    }
                    else
                    {
                        // Will remove from the message queue
                        messageQueueTransaction.Commit();

                        #region Logging

                        if (LOG.IsDebugEnabled)
                        {
                            LOG.Debug(
                                "Exception handler's TransactionAction has committed MessageQueueTransaction for queue [" +
                                mq.Path + "]");
                        }

                        #endregion
                    }
                    //Outer db-tx will rollback
                    throw;
                }
                finally
                {
                    if (ExposeContainerManagedMessageQueueTransaction)
                    {
                        TransactionSynchronizationManager.UnbindResource(
                            MessageQueueTransactionManager.CURRENT_TRANSACTION_SLOTNAME);
                    }
                    message.Dispose();
                }
                return true;
            }
        }


        /// <summary>
        /// Handles the transactional listener exception.
        /// </summary>
        /// <param name="e">The exception thrown while processing the message.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageQueueTransaction">The message queue transaction.</param>
        /// <returns>The TransactionAction retruned by the TransactionalExceptionListener</returns>
        protected virtual TransactionAction HandleTransactionalListenerException(Exception e, Message message,
                                                                                 MessageQueueTransaction
                                                                                     messageQueueTransaction)
        {
            try
            {
                TransactionAction transactionAction =
                    InvokeTransactionalExceptionListener(e, message, messageQueueTransaction);
                if (Active)
                {
                    // Regular case: failed while active.
                    // Log at error level.
                    LOG.Error("Execution of message listener failed", e);
                }
                else
                {
                    // Rare case: listener thread failed after container shutdown.
                    // Log at debug level, to avoid spamming the shutdown log.
                    LOG.Debug("Listener exception after container shutdown", e);
                }
                return transactionAction;
            }
            catch (Exception ex)
            {
                LOG.Error("Exception invoking MessageTransactionExceptionHandler.  Rolling back transaction.", ex);
                return TransactionAction.Rollback;
            }
        }


        /// <summary>
        /// Invokes the transactional exception listener.
        /// </summary>
        /// <param name="e">The exception thrown during message processing.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageQueueTransaction">The message queue transaction.</param>
        /// <returns>TransactionAction.Rollback if no exception handler is defined, otherwise the
        /// TransactionAction returned by the exception handler</returns>
        protected virtual TransactionAction InvokeTransactionalExceptionListener(Exception e, Message message,
                                                                                 MessageQueueTransaction
                                                                                     messageQueueTransaction)
        {
            IMessageTransactionExceptionHandler exceptionHandler = MessageTransactionExceptionHandler;
            if (exceptionHandler != null)
            {
                return exceptionHandler.OnException(e, message, messageQueueTransaction);
            }
            else
            {
                LOG.Warn("No MessageTransactionExceptionHandler defined.  Defaulting to TransactionAction.Rollback.");
                return TransactionAction.Rollback;
            }
        }

        #endregion
    }
}
