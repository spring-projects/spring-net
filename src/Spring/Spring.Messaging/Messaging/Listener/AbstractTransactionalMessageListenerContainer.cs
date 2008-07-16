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

using System;
using System.Messaging;
using System.Threading;
using Common.Logging;
using Spring.Data.Core;
using Spring.Messaging.Core;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// An implementation of a Peeking based MessageListener container that starts a transaction 
    /// before recieving a message.  The <see cref="IPlatformTransactionManager"/> implementation determines
    /// the type of transaction that will be started.  An exception while processing the message will
    /// result in a rollback, otherwise a transaction commit will be performed.
    /// </summary>
    /// <remarks>
    /// The type of transaction that can be started can either be local transaction, 
    /// (e.g. <see cref="AdoPlatformTransactionManager"/>, a local messaging transaction
    /// (e.g. <see cref="MessageQueueTransactionManager"/> or a DTC based transaction, 
    /// (eg. <see cref="TxScopeTransactionManager"/>.
    /// <para>
    /// Transaction properties can be set using the property <see cref="TransactionDefinition"/>
    /// and the transaction timeout via the property <see cref="TransactionTimeout"/>.
    /// </para>
    /// </remarks>
    public abstract class AbstractTransactionalMessageListenerContainer : AbstractPeekingMessageListenerContainer
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (AbstractTransactionalMessageListenerContainer));

        #endregion

        private IPlatformTransactionManager platformTransactionManager;

        private DefaultTransactionDefinition transactionDefinition = new DefaultTransactionDefinition();


        public IPlatformTransactionManager PlatformTransactionManager
        {
            get { return platformTransactionManager; }
            set { platformTransactionManager = value; }
        }

        public DefaultTransactionDefinition TransactionDefinition
        {
            get { return transactionDefinition; }
            set { transactionDefinition = value; }
        }

        /// <summary>
        /// Sets the transaction timeout to use for transactional wrapping, in <b>seconds</b>.
        /// Default is none, using the transaction manager's default timeout.
        /// </summary>
        /// <value>The transaction timeout.</value>
        public int TransactionTimeout
        {
            set { transactionDefinition.TransactionTimeout = value; }
        }

        protected override bool DoReceiveAndExecute(MessageQueue mq)
        {
            bool messageReceived = false;
            // Execute receive within transaction.
            ITransactionStatus status = PlatformTransactionManager.GetTransaction(TransactionDefinition);
            try
            {
                messageReceived = DoReceiveAndExecuteUsingPlatformTransactionManager(mq, status);
            }
            catch (Exception ex)
            {
                RollbackOnException(status, ex);
                Thread.Sleep(RecoveryTimeSpan);
                throw;
            }
            //if status has indicated rollback only, will rollback.                    
            PlatformTransactionManager.Commit(status);
            return messageReceived;
        }

        protected abstract bool DoReceiveAndExecuteUsingPlatformTransactionManager(MessageQueue mq,
                                                                                   ITransactionStatus status);

        protected void RollbackOnException(ITransactionStatus status, Exception ex)
        {
            LOG.Debug("Initiating transaction rollback on listener exception", ex);
            try
            {
                PlatformTransactionManager.Rollback(status);
            }
            catch (Exception ex2)
            {
                LOG.Error("Listener exception overridden by rollback error", ex2);
                throw;
            }
        }
    }
}