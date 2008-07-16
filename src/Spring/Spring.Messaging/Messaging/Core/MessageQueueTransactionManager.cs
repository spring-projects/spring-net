#region License

/*
 * Copyright 2002-2008 the original author or authors.
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


using System.Messaging;
using Common.Logging;
using Spring.Data.Core;
using Spring.Transaction;
using Spring.Transaction.Support;
using Spring.Util;

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

        protected override object DoGetTransaction()
        {
            MessageQueueTransactionObject txObject = new MessageQueueTransactionObject();
            txObject.ResourceHolder =
                (MessageQueueResourceHolder) TransactionSynchronizationManager.GetResource(CURRENT_TRANSACTION_SLOTNAME);
            return txObject;
        }

        protected override bool IsExistingTransaction(object transaction)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) transaction;
            return (txObject.ResourceHolder != null);
        }

        protected override void DoBegin(object transaction, ITransactionDefinition definition)
        {
            //TODO check isolation level is different than default value?

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

        protected override object DoSuspend(object transaction)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) transaction;
            txObject.ResourceHolder = null;
            return TransactionSynchronizationManager.UnbindResource(CURRENT_TRANSACTION_SLOTNAME);
        }

        protected override void DoResume(object transaction, object suspendedResources)
        {
            MessageQueueResourceHolder queueHolder = (MessageQueueResourceHolder) suspendedResources;
            TransactionSynchronizationManager.BindResource(CURRENT_TRANSACTION_SLOTNAME, queueHolder);
        }

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

        protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
            MessageQueueTransactionObject txObject = (MessageQueueTransactionObject) status.Transaction;
            txObject.ResourceHolder.RollbackOnly = true;
        }

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