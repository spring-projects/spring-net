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
using Spring.Transaction;
using Spring.Transaction.Support;
using Spring.Util;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Core
{
    /// <summary>
    /// <see cref="IPlatformTransactionManager"/> implementation for MSMQ.  Binds a 
    /// MessageQueueTransaction to the thread.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This local strategy is an alternative to executing MSMQ operations within 
    /// DTC transactions. Its advantage is that multiple MSMQ operations can 
    /// easily participate within the same local MessagingTransaction transparently when
    /// using the <see cref="MessageQueueTemplate"/> class for send and recieve operations
    /// and not pay the overhead of a DTC transaction.  
    /// </para>
    /// <para>Transaction synchronization is turned off by default, as this manager might
    /// be used alongside a IDbProvider-based Spring transaction manager such as the
    /// ADO.NET <see cref="AdoPlatformTransactionManager"/>.
    /// which has stronger needs for synchronization.</para>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class MessageQueueTransactionManager : AbstractPlatformTransactionManager
    {
        /// <summary>
        /// Location where the message transaction is stored in thread local storage.
        /// </summary>
        public static readonly string CURRENT_TRANSACTION_SLOTNAME =
            UniqueKey.GetTypeScopedString(typeof (MessageQueueTransaction), "Current");

        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (MessageQueueTransactionManager));

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueTransactionManager"/> class.
        /// </summary>
        /// <remarks>
        /// Turns off transaction synchronization by default, as this manager might
        /// be used alongside a DbProvider-based Spring transaction manager like
        /// AdoPlatformTransactionManager, which has stronger needs for synchronization.
        /// Only one manager is allowed to drive synchronization at any point of time.
        /// </remarks>
        public MessageQueueTransactionManager()
        {
            TransactionSynchronization = TransactionSynchronizationState.Never;
        }

        /// <summary>
        /// Return the current transaction object.
        /// </summary>
        /// <returns>The current transaction object.</returns>
        /// <exception cref="Spring.Transaction.CannotCreateTransactionException">
        /// If transaction support is not available.
        /// </exception>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of lookup or system errors.
        /// </exception>
        protected override object DoGetTransaction()
        {
            MessageQueueTransactionObject txObject = new MessageQueueTransactionObject();
            txObject.ResourceHolder =
                (MessageQueueResourceHolder) TransactionSynchronizationManager.GetResource(CURRENT_TRANSACTION_SLOTNAME);
            return txObject;
        }

        /// <summary>
        /// Check if the given transaction object indicates an existing transaction
        /// (that is, a transaction which has already started).
        /// </summary>
        /// <param name="transaction">MessageQueueTransactionObject object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <returns>
        /// True if there is an existing transaction.
        /// </returns>
        protected override bool IsExistingTransaction(object transaction)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) transaction;
            return (txObject.ResourceHolder != null);
        }

        /// <summary>
        /// Begin a new transaction with the given transaction definition.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <param name="definition"><see cref="Spring.Transaction.ITransactionDefinition"/> instance, describing
        /// propagation behavior, isolation level, timeout etc.</param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of creation or system errors.
        /// </exception>
        protected override void DoBegin(object transaction, ITransactionDefinition definition)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) transaction;

            MessageQueueTransaction mqt = new MessageQueueTransaction();
            mqt.Begin();

            txObject.ResourceHolder = new MessageQueueResourceHolder(mqt);
            txObject.ResourceHolder.SynchronizedWithTransaction = true;

            int timeout = DetermineTimeout(definition);
            if (timeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT)
            {
                txObject.ResourceHolder.TimeoutInSeconds = timeout;
            }
            TransactionSynchronizationManager.BindResource(CURRENT_TRANSACTION_SLOTNAME, txObject.ResourceHolder);
        }

        /// <summary>
        /// Suspend the resources of the current transaction.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <returns>
        /// An object that holds suspended resources (will be kept unexamined for passing it into
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoResume"/>.)
        /// </returns>
        protected override object DoSuspend(object transaction)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) transaction;
            txObject.ResourceHolder = null;
            return TransactionSynchronizationManager.UnbindResource(CURRENT_TRANSACTION_SLOTNAME);
        }

        /// <summary>
        /// Resume the resources of the current transaction.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <param name="suspendedResources">The object that holds suspended resources as returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoSuspend"/>.</param>
        protected override void DoResume(object transaction, object suspendedResources)
        {
            MessageQueueResourceHolder queueHolder = (MessageQueueResourceHolder) suspendedResources;
            TransactionSynchronizationManager.BindResource(CURRENT_TRANSACTION_SLOTNAME, queueHolder);
        }

        /// <summary>
        /// Perform an actual commit on the given transaction.
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <remarks>
        /// 	<p>
        /// An implementation does not need to check the rollback-only flag.
        /// </p>
        /// </remarks>
        protected override void DoCommit(DefaultTransactionStatus status)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) status.Transaction;
            MessageQueueTransaction transaction = txObject.ResourceHolder.MessageQueueTransaction;
            try
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Committing MessageQueueTransaction");
                }
                transaction.Commit();
            }
            catch (MessageQueueException ex)
            {
                throw new TransactionSystemException("Could not commit DefaultMessageQueue transaction", ex);
            }
        }

        /// <summary>
        /// Perform an actual rollback on the given transaction, calls Transaction.Abort().
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <remarks>
        /// An implementation does not need to check the new transaction flag.
        /// </remarks>
        protected override void DoRollback(DefaultTransactionStatus status)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) status.Transaction;
            MessageQueueTransaction transaction = txObject.ResourceHolder.MessageQueueTransaction;
            try
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Committing MessageQueueTransaction");
                }
                transaction.Abort();
            }
            catch (MessageQueueException ex)
            {
                throw new TransactionSystemException("Could not roll back DefaultMessageQueue transaction", ex);
            }
        }

        /// <summary>
        /// Set the given transaction rollback-only. Only called on rollback
        /// if the current transaction takes part in an existing one.
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <remarks>Default implementation throws an IllegalTransactionStateException,
        /// assuming that participating in existing transactions is generally not
        /// supported. Subclasses are of course encouraged to provide such support.
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) status.Transaction;
            txObject.ResourceHolder.RollbackOnly = true;
        }

        /// <summary>
        /// Cleanup resources after transaction completion.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <remarks>
        /// <para>
        /// Called after <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoCommit"/>
        /// and
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoRollback"/>
        /// execution on any outcome.
        /// </para>
        /// </remarks>
        protected override void DoCleanupAfterCompletion(object transaction)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) transaction;
            TransactionSynchronizationManager.UnbindResource(CURRENT_TRANSACTION_SLOTNAME);
            txObject.ResourceHolder.Clear();
        }


        private class MessageQueueTransactionObject : ISmartTransactionObject
        {
            private MessageQueueResourceHolder resourceHolder;


            public MessageQueueResourceHolder ResourceHolder
            {
                get { return resourceHolder; }
                set { resourceHolder = value; }
            }

            #region ISmartTransactionObject Members

            public bool RollbackOnly
            {
                get { return resourceHolder.RollbackOnly; }
            }

            #endregion
        }
    }
}