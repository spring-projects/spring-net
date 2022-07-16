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

using System.Transactions;
using Common.Logging;
using Spring.Transaction;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// A MessageListenerContainer that uses distributed (DTC) based transactions.  Exceptions are
    /// handled by instances of <see cref="IDistributedTransactionExceptionHandler"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Starts a DTC based transaction before receiving the message.  The transaction is
    /// automaticaly promoted to 2PC to avoid the default behaivor of transactional promotion.
    /// Database and messaging operations will commit or rollback together.
    /// </para>
    /// <para>
    /// If you only want local message based transactions use the
    /// <see cref="TransactionalMessageListenerContainer"/>.  With some simple programming
    /// you may also achieve 'exactly once' processing using the
    /// <see cref="TransactionalMessageListenerContainer"/>.
    /// </para>
    /// <para>
    /// Poison messages can be detected and sent to another queue using Spring's
    /// <see cref="SendToQueueDistributedTransactionExceptionHandler"/>.
    /// </para>
    /// </remarks>
    public class DistributedTxMessageListenerContainer : AbstractTransactionalMessageListenerContainer
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (DistributedTxMessageListenerContainer));

        #endregion

        private IDistributedTransactionExceptionHandler distributedTransactionExceptionHandler;


        /// <summary>
        /// Gets or sets the distributed transaction exception handler.
        /// </summary>
        /// <value>The distributed transaction exception handler.</value>
        public IDistributedTransactionExceptionHandler DistributedTransactionExceptionHandler
        {
            get { return distributedTransactionExceptionHandler; }
            set { distributedTransactionExceptionHandler = value; }
        }

        /// <summary>
        /// Set the transaction name to be the spring object name.
        /// Call base class Initialize() functionality.
        /// </summary>
        public override void Initialize()
        {
            // Use object name as default transaction name.
            if (TransactionDefinition.Name == null)
            {
                TransactionDefinition.Name = ObjectName;
            }

            // Proceed with superclass initialization.
            base.Initialize();
        }

        /// <summary>
        /// Does the receive and execute using TxPlatformTransactionManager.  Starts a distributed
        /// transaction before calling Receive.
        /// </summary>
        /// <param name="mq">The message queue.</param>
        /// <param name="status">The transactional status.</param>
        /// <returns>
        /// true if should continue peeking, false otherwise.
        /// </returns>
        protected override bool DoReceiveAndExecuteUsingPlatformTransactionManager(MessageQueue mq,
                                                                                   ITransactionStatus status)
        {
            #region Logging

            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing DoReceiveAndExecuteUsingTxScopeTransactionManager");
            }

            #endregion Logging

            //We are sure to be talking to a second resource manager, so avoid going through
            //the promotable transaction and force a distributed transaction right from the start.
            TransactionInterop.GetTransmitterPropagationToken(System.Transactions.Transaction.Current);

            Message message;
            try
            {
                message = mq.Receive(TimeSpan.Zero, MessageQueueTransactionType.Automatic);
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
                    lock (messageQueueMonitor)
                    {
                        mq.Close();
                        MessageQueue.ClearConnectionCache();
                    }
                    throw; // will cause rollback in surrounding platform transaction manager and log exception
                }
            }

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
                if (DistributedTransactionExceptionHandler != null)
                {
                    if (DistributedTransactionExceptionHandler.IsPoisonMessage(message))
                    {
                        DistributedTransactionExceptionHandler.HandlePoisonMessage(message);
                        return true; // will remove from queue and continue receive loop.
                    }
                }
                DoExecuteListener(message);
            }
            catch (Exception ex)
            {
                HandleDistributedTransactionListenerException(ex, message);
                throw; // will rollback and keep message on the queue.
            }
            finally
            {
                message.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Handles the distributed transaction listener exception by calling the
        /// <see cref="IDistributedTransactionExceptionHandler"/> if not null.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        protected virtual void HandleDistributedTransactionListenerException(Exception exception, Message message)
        {
            IDistributedTransactionExceptionHandler exceptionHandler = DistributedTransactionExceptionHandler;
            if (exceptionHandler != null)
            {
                exceptionHandler.OnException(exception, message);
            }
        }
    }
}
