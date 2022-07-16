/*
 * Copyright 2007 the original author or authors.
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

using System.Transactions;
using Spring.Data.Support;
using Spring.Objects.Factory;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.Core
{
    /// <summary>
    /// TransactionManager that uses TransactionScope provided by System.Transactions.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class TxScopeTransactionManager : AbstractPlatformTransactionManager, IInitializingObject
    {
        private readonly ITransactionScopeAdapter txAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TxScopeTransactionManager"/> class.
        /// </summary>
        public TxScopeTransactionManager()
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TxScopeTransactionManager"/> class.
        /// </summary>
        /// <remarks>This is indented only for unit testing purposes and should not be
        /// called by production application code.</remarks>
        /// <param name="txAdapter">The tx adapter.</param>
        public TxScopeTransactionManager(ITransactionScopeAdapter txAdapter)
        {
            this.txAdapter = txAdapter;
        }

        /// <summary>
        /// No-op initialization
        /// </summary>
        public void AfterPropertiesSet()
        {
            // placeholder for more advanced configurations.
        }

        protected override object DoGetTransaction()
        {
            PromotableTxScopeTransactionObject txObject = new PromotableTxScopeTransactionObject();

            if (txAdapter != null)
            {
                txObject.TxScopeAdapter = txAdapter;
            }

            return txObject;
        }

        protected override bool IsExistingTransaction(object transaction)
        {
            PromotableTxScopeTransactionObject txObject =
                (PromotableTxScopeTransactionObject) transaction;
            return txObject.TxScopeAdapter.IsExistingTransaction;
        }

        protected override void DoBegin(object transaction, ITransactionDefinition definition)
        {
            PromotableTxScopeTransactionObject txObject =
                (PromotableTxScopeTransactionObject) transaction;
            try
            {
                DoTxScopeBegin(txObject, definition);
            }
            catch (Exception e)
            {
                throw new CannotCreateTransactionException("Transaction Scope failure on begin", e);
            }
        }

        protected override object DoSuspend(object transaction)
        {
            // Passing the current TxScopeAdapter as the 'suspended resource', even though it is not used just to avoid passing null
            // TxScopeTransactionManager is not binding any resources to the local thread, instead delegating to
            // System.Transactions to handle thread local resources.
            PromotableTxScopeTransactionObject txMgrStateObject = (PromotableTxScopeTransactionObject) transaction;
            return txMgrStateObject.TxScopeAdapter;
        }

        protected override void DoResume(object transaction, object suspendedResources)
        {
        }

        protected override void DoCommit(DefaultTransactionStatus status)
        {
            PromotableTxScopeTransactionObject txObject =
                (PromotableTxScopeTransactionObject) status.Transaction;
            try
            {
                txObject.TxScopeAdapter.Complete();
                txObject.TxScopeAdapter.Dispose();
            }
            catch (TransactionAbortedException ex)
            {
                throw new UnexpectedRollbackException("Transaction unexpectedly rolled back (maybe due to a timeout)",
                    ex);
            }
            catch (TransactionInDoubtException ex)
            {
                throw new HeuristicCompletionException(TransactionOutcomeState.Unknown, ex);
            }
            catch (Exception ex)
            {
                throw new TransactionSystemException("Failure on Transaction Scope Commit", ex);
            }
        }

        protected override void DoRollback(DefaultTransactionStatus status)
        {
            PromotableTxScopeTransactionObject txObject =
                (PromotableTxScopeTransactionObject) status.Transaction;

            try
            {
                txObject.TxScopeAdapter.Dispose();
            }
            catch (Exception e)
            {
                throw new TransactionSystemException("Failure on Transaction Scope rollback.", e);
            }
        }

        protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
            if (status.Debug)
            {
                log.Debug("Setting transaction rollback-only");
            }

            try
            {
                System.Transactions.Transaction.Current.Rollback();
            }
            catch (Exception ex)
            {
                throw new TransactionSystemException("Failure on System.Transactions.Transaction.Current.Rollback", ex);
            }
        }

        protected override bool ShouldCommitOnGlobalRollbackOnly => true;

        private void DoTxScopeBegin(
            PromotableTxScopeTransactionObject txObject,
            ITransactionDefinition definition)
        {
            TransactionScopeOption txScopeOption = CreateTransactionScopeOptions(definition);
            TransactionOptions txOptions = CreateTransactionOptions(definition);
            txObject.TxScopeAdapter.CreateTransactionScope(
                txScopeOption,
                txOptions,
                definition.AsyncFlowOption);
        }

        private static TransactionOptions CreateTransactionOptions(ITransactionDefinition definition)
        {
            TransactionOptions txOptions = new TransactionOptions();
            switch (definition.TransactionIsolationLevel)
            {
                case System.Data.IsolationLevel.Chaos:
                    txOptions.IsolationLevel = IsolationLevel.Chaos;
                    break;
                case System.Data.IsolationLevel.ReadCommitted:
                    txOptions.IsolationLevel = IsolationLevel.ReadCommitted;
                    break;
                case System.Data.IsolationLevel.ReadUncommitted:
                    txOptions.IsolationLevel = IsolationLevel.ReadUncommitted;
                    break;
                case System.Data.IsolationLevel.RepeatableRead:
                    txOptions.IsolationLevel = IsolationLevel.RepeatableRead;
                    break;
                case System.Data.IsolationLevel.Serializable:
                    txOptions.IsolationLevel = IsolationLevel.Serializable;
                    break;
                case System.Data.IsolationLevel.Snapshot:
                    txOptions.IsolationLevel = IsolationLevel.Snapshot;
                    break;
                case System.Data.IsolationLevel.Unspecified:
                    txOptions.IsolationLevel = IsolationLevel.Unspecified;
                    break;
            }

            if (definition.TransactionTimeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT)
            {
                txOptions.Timeout = new TimeSpan(0, 0, definition.TransactionTimeout);
            }

            return txOptions;
        }

        private static TransactionScopeOption CreateTransactionScopeOptions(ITransactionDefinition definition)
        {
            TransactionScopeOption txScopeOption;
            if (definition.PropagationBehavior == TransactionPropagation.Required)
            {
                txScopeOption = TransactionScopeOption.Required;
            }
            else if (definition.PropagationBehavior == TransactionPropagation.RequiresNew)
            {
                txScopeOption = TransactionScopeOption.RequiresNew;
            }
            else if (definition.PropagationBehavior == TransactionPropagation.NotSupported)
            {
                txScopeOption = TransactionScopeOption.Suppress;
            }
            else
            {
                throw new TransactionSystemException("Transaction Propagation Behavior" +
                                                     definition.PropagationBehavior +
                                                     " not supported by TransactionScope.  Use Required or RequiredNew");
            }

            return txScopeOption;
        }

        /// <summary>
        /// The transaction resource object that encapsulates the state and functionality
        /// contained in TransactionScope and Transaction.Current via the ITransactionScopeAdapter
        /// property.
        /// </summary>
        public class PromotableTxScopeTransactionObject : ISmartTransactionObject
        {
            private ITransactionScopeAdapter txScopeAdapter;

            /// <summary>
            /// Initializes a new instance of the <see cref="PromotableTxScopeTransactionObject"/> class.
            /// Will create an instance of <see cref="DefaultTransactionScopeAdapter"/>.
            /// </summary>
            public PromotableTxScopeTransactionObject()
            {
                txScopeAdapter = new DefaultTransactionScopeAdapter();
            }

            /// <summary>
            /// Gets or sets the transaction scope adapter.
            /// </summary>
            /// <value>The transaction scope adapter.</value>
            public ITransactionScopeAdapter TxScopeAdapter
            {
                get => txScopeAdapter;
                set => txScopeAdapter = value;
            }

            /// <summary>
            /// Return whether the transaction is internally marked as rollback-only.
            /// </summary>
            /// <value></value>
            /// <returns>True of the transaction is marked as rollback-only.</returns>
            public bool RollbackOnly => txScopeAdapter.RollbackOnly;
        }
    }
}
