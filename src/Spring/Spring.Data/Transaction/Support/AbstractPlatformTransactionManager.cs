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

using System.Collections;
using System.Data;
using Common.Logging;

namespace Spring.Transaction.Support
{
    /// <summary>
    /// Abstract base class that allows for easy implementation of concrete platform transaction managers.
    /// </summary>
    /// <remarks>
    /// <p>Provides the following workflow handling:
    /// <ul>
    /// <li>Determines if there is an existing transaction</li>
    /// <li>Applies the appropriate propagation behavior</li>
    /// <li>Suspends and resumes transactions if necessary</li>
    /// <li>Checks the rollback-only flag on commit</li>
    /// <li>Applies the appropriate modification on rollback (actual rollback or setting rollback-only)</li>
    /// <li>Triggers registered synchronization callbacks (if transaction synchronization is active)</li>
    /// </ul>
    /// </p>
    /// <p>
    /// Transaction synchronization is a generic mechanism for registering
    /// callbacks that get invoked at transaction completion time. The same mechanism
    /// can also be used for custom synchronization efforts.
    /// </p>
    /// <p>
    /// The state of this class is serializable. It's up to subclasses if
    /// they wish to make their state to be serializable.
    /// They should implement <see cref="System.Runtime.Serialization.ISerializable"/> if they need
    /// to restore any transient state.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Griffin Caprio (.NET)</author>
    [Serializable]
    public abstract class AbstractPlatformTransactionManager : IPlatformTransactionManager
    {
        #region Private SuspendedResourcesHolder Helper class

        private class SuspendedResourcesHolder
        {
            private IList _suspendedSynchronizations;
            private object _suspendedResources;
            private string _name;
            private bool _readOnly;
            private IsolationLevel _isolationLevel;
            private bool _wasActive;


            public SuspendedResourcesHolder(object suspendedResources)
            {
                _suspendedResources = suspendedResources;
            }

            public SuspendedResourcesHolder(IList suspendedSynchronizations, object suspendedResources,
                                             string name, bool readOnly, IsolationLevel isolationLevel, bool wasActive)
            {
                _suspendedSynchronizations = suspendedSynchronizations;
                _suspendedResources = suspendedResources;
                _name = name;
                _readOnly = readOnly;
                _isolationLevel = isolationLevel;
                _wasActive = wasActive;
            }

            public IList SuspendedSynchronizations
            {
                get { return _suspendedSynchronizations; }
            }

            public object SuspendedResources
            {
                get { return _suspendedResources; }
            }


            public string Name
            {
                get { return _name; }
            }

            public bool ReadOnly
            {
                get { return _readOnly; }
            }

            public IsolationLevel IsolationLevel
            {
                get { return _isolationLevel; }
            }

            public bool WasActive
            {
                get { return _wasActive; }
            }
        }

        #endregion

        #region Private Variables

        private TransactionSynchronizationState _transactionSyncState = TransactionSynchronizationState.Never;
        private bool _nestedTransactionsAllowed;
        private bool _rollbackOnCommitFailure;
        private bool _failEarlyOnGlobalRollbackOnly;
        private int _defaultTimeout = DefaultTransactionDefinition.TIMEOUT_DEFAULT;

        #region Logging Definition

        [NonSerialized()]
        protected readonly ILog log;

        #endregion

        protected AbstractPlatformTransactionManager()
        {
            log = LogManager.GetLogger(this.GetType());
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Sets and gets when this transaction manager should activate the thread-bound
        /// transaction synchronization support. Default is "always".
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note that transaction synchronization isn't supported for
        /// multiple concurrent transactions by different transaction managers.
        /// Only one transaction manager is allowed to activate it at any time.
        /// </p>
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationState"/>
        /// </remarks>
        public TransactionSynchronizationState TransactionSynchronization
        {
            set { _transactionSyncState = value; }
            get { return _transactionSyncState; }
        }

        /// <summary>
        /// Sets and gets whether nested transactions are allowed. Default is false.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Typically initialized with an appropriate default by the
        /// concrete transaction manager subclass.
        /// </p>
        /// </remarks>
        public bool NestedTransactionsAllowed
        {
            get { return _nestedTransactionsAllowed; }
            set { _nestedTransactionsAllowed = value; }
        }

        /// <summary>
        /// Sets and gets a flag that determines whether or not the
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoRollback(DefaultTransactionStatus)"/>
        /// method must be invoked if a call to the
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoCommit(DefaultTransactionStatus)"/>
        /// method fails. Default is false.
        /// </summary>
        /// <remarks>
        /// Typically not necessary and thus to be avoided as it can override the
        /// commit exception with a subsequent rollback exception.
        /// </remarks>
        public bool RollbackOnCommitFailure
        {
            get { return _rollbackOnCommitFailure; }
            set { _rollbackOnCommitFailure = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to fail early in case of the transaction being
        /// globally marked as rollback-only.
        /// </summary>
        /// <remarks>
        ///  Default is "false", only causing an UnexpectedRollbackException at the
        ///  outermost transaction boundary. Switch this flag on to cause an
        ///  UnexpectedRollbackException as early as the global rollback-only marker
        ///  has been first detected, even from within an inner transaction boundary.
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if fail early on global rollback; otherwise, <c>false</c>.
        /// </value>
        public bool FailEarlyOnGlobalRollbackOnly
        {
            get { return _failEarlyOnGlobalRollbackOnly; }
            set { _failEarlyOnGlobalRollbackOnly = value; }
        }

        /// <summary>
        /// Gets or sets the default timeout that this transaction manager should apply if there
        /// is no timeout specified at the transaction level, in seconds.
        /// </summary>
        /// <remarks>Returns DefaultTransactionDefinition.TIMEOUT_DEFAULT to indicate the
        /// underlying transaction infrastructure's default timeout.</remarks>
        /// <value>The default timeout.</value>
        public int DefaultTimeout
        {
            get
            {
                return _defaultTimeout;
            }
            set
            {
                if (_defaultTimeout < DefaultTransactionDefinition.TIMEOUT_DEFAULT)
                {
                    throw new InvalidTimeoutException("Invalid default timeout", _defaultTimeout);
                }
                _defaultTimeout = value;
            }
        }

        #endregion

        #region Protected Methods

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
        protected abstract object DoGetTransaction();

        /// <summary>
        /// Check if the given transaction object indicates an existing transaction
        /// (that is, a transaction which has already started).
        /// </summary>
        /// <remarks>
        /// The result will be evaluated according to the specified propagation
        /// behavior for the new transaction. An existing transaction might get
        /// suspended (in case of PROPAGATION_REQUIRES_NEW), or the new transaction
        /// might participate in the existing one (in case of PROPAGATION_REQUIRED).
        /// Default implementation returns false, assuming that detection of or
        /// participating in existing transactions is generally not supported.
        /// Subclasses are of course encouraged to provide such support.</remarks>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <returns>True if there is an existing transaction.</returns>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected virtual bool IsExistingTransaction(object transaction)
        {
            return false;
        }

        /// <summary>
        /// Begin a new transaction with the given transaction definition.
        /// </summary>
        /// <remarks>
        /// Does not have to care about applying the propagation behavior,
        /// as this has already been handled by this abstract manager.
        /// </remarks>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <param name="definition">
        /// <see cref="Spring.Transaction.ITransactionDefinition"/> instance, describing
        /// propagation behavior, isolation level, timeout etc.
        /// </param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of creation or system errors.
        /// </exception>
        protected abstract void DoBegin(object transaction, ITransactionDefinition definition);

        /// <summary>
        /// Suspend the resources of the current transaction.
        /// </summary>
        /// <remarks>
        /// Transaction synchronization will already have been suspended.
        /// <para>
        /// Default implementation throws a TransactionSuspensionNotSupportedException,
        /// assuming that transaction suspension is generally not supported.
        /// </para>
        /// </remarks>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <returns>
        /// An object that holds suspended resources (will be kept unexamined for passing it into
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoResume"/>.)
        /// </returns>
        /// <exception cref="Spring.Transaction.IllegalTransactionStateException">
        /// If suspending is not supported by the transaction manager implementation.
        /// </exception>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// in case of system errors.
        /// </exception>
        protected virtual object DoSuspend(object transaction)
        {
            throw new TransactionSuspensionNotSupportedException(
                "Transaction manager [" + GetType().Name + "] does not support transaction suspension");
        }

        /// <summary>
        /// Resume the resources of the current transaction.
        /// </summary>
        /// <remarks>Transaction synchronization will be resumed afterwards.
        /// <para>
        /// Default implementation throws a TransactionSuspensionNotSupportedException,
        /// assuming that transaction suspension is generally not supported.
        /// </para>
        /// </remarks>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <param name="suspendedResources">
        /// The object that holds suspended resources as returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoSuspend"/>.
        /// </param>
        /// <exception cref="Spring.Transaction.IllegalTransactionStateException">
        /// If suspending is not supported by the transaction manager implementation.
        /// </exception>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected virtual void DoResume(object transaction, object suspendedResources)
        {
            throw new TransactionSuspensionNotSupportedException(
                "Transaction manager [" + GetType().Name + "] does not support transaction suspension");
        }

        /// <summary>
        /// Perform an actual commit on the given transaction.
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <remarks>
        /// <p>
        /// An implementation does not need to check the rollback-only flag.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected abstract void DoCommit(DefaultTransactionStatus status);

        /// <summary>
        /// Perform an actual rollback on the given transaction.
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <remarks>
        /// An implementation does not need to check the new transaction flag.
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected abstract void DoRollback(DefaultTransactionStatus status);

        /// <summary>
        /// Set the given transaction rollback-only. Only called on rollback
        /// if the current transaction takes part in an existing one.
        /// </summary>
        /// <remarks>Default implementation throws an IllegalTransactionStateException,
        /// assuming that participating in existing transactions is generally not
        /// supported. Subclasses are of course encouraged to provide such support.
        /// </remarks>
        /// <param name="status">The status representation of the transaction.</param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected virtual void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
            throw new IllegalTransactionStateException(
                "Participating in existing transactions is not supported - when 'IsExistingTransaction' " +
                "returns true, appropriate 'DoSetRollbackOnly' behavior must be provided");
        }

        /// <summary>
        /// Return whether to use a savepoint for a nested transaction. Default is true,
        /// which causes delegation to <see cref="Spring.Transaction.Support.DefaultTransactionStatus"/>
        /// for holding a savepoint.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <p>
        /// Subclasses can override this to return false, causing a further
        /// invocation of
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoBegin"/>
        /// despite an already existing transaction.
        /// </p>
        /// </remarks>
        protected virtual bool UseSavepointForNestedTransaction()
        {
            return true;
        }

        /// <summary>
        /// Register the given list of transaction synchronizations with the existing transaction.
        /// </summary>
        /// <remarks>
        /// Invoked when the control of the Spring transaction manager and thus all Spring
        /// transaction synchronizations end, without the transaction being completed yet. This
        /// is for example the case when participating in an existing System.Transactions or
        /// EnterpriseServices transaction invoked via their APIs.
        /// <para>
        /// The default implementation simply invokes the <code>AfterCompletion</code> methods
        /// immediately, passing in TransactionSynchronizationStatus.Unknown.
        /// This is the best we can do if there's no chance to determine the actual
        /// outcome of the outer transaction.
        /// </para>
        /// </remarks>
        /// <param name="transaction">The transaction transaction object returned by <code>DoGetTransaction</code>.</param>
        /// <param name="synchronizations">The lList of TransactionSynchronization objects.</param>
        /// <exception cref="TransactionException">In case of errors</exception>
        /// <seealso cref="InvokeAfterCompletion"/>
        /// <seealso cref="ITransactionSynchronization.AfterCompletion"/>
        /// <seealso cref="TransactionSynchronizationStatus.Unknown"/>
        protected virtual void RegisterAfterCompletionWithExistingTransaction(Object transaction, IList synchronizations)
        {

            log.Debug("Cannot register Spring after-completion synchronization with existing transaction - " +
                "processing Spring after-completion callbacks immediately, with outcome status 'unknown'");
            InvokeAfterCompletion(synchronizations, TransactionSynchronizationStatus.Unknown);
        }

        /// <summary>
        /// Cleanup resources after transaction completion.
        /// </summary>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// Called after <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoCommit"/>
        /// and
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoRollback"/>
        /// execution on any outcome.
        /// </para>
        /// <para>
        /// Should not throw any exceptions but just issue warnings on errors.
        /// </para>
        /// <para>
        /// Default implementation does nothing.
        /// </para>
        /// </remarks>
        protected virtual void DoCleanupAfterCompletion(object transaction)
        {
        }

        #endregion

        #region IPlatformTransactionManager Members

        /// <summary>
        /// Return a currently active transaction or create a new one.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation handles propagation behavior.
        /// </p>
        /// <p>
        /// Delegates to
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>,
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.IsExistingTransaction"/>,
        /// and
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoBegin"/>.
        /// </p>
        /// <p>
        /// Note that parameters like isolation level or timeout will only be applied
        /// to new transactions, and thus be ignored when participating in active ones.
        /// Furthermore, they aren't supported by every transaction manager:
        /// a proper implementation should throw an exception when custom values
        /// that it doesn't support are specified.
        /// </p>
        /// </remarks>
        /// <param name="definition">
        /// <see cref="Spring.Transaction.ITransactionDefinition"/> instance (can be null for
        /// defaults), describing propagation behavior, isolation level, timeout etc.
        /// </param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In case of lookup, creation, or system errors.
        /// </exception>
        /// <returns>
        /// <see cref="Spring.Transaction.ITransactionStatus"/> representing the new or current transaction.
        /// </returns>
        public ITransactionStatus GetTransaction(ITransactionDefinition definition)
        {
            object transaction = DoGetTransaction();
            bool debugEnabled = log.IsDebugEnabled;

            if (debugEnabled)
            {
                log.Debug("Using transaction object [" + transaction + "]");
            }

            if (definition == null)
            {
                definition = new DefaultTransactionDefinition();
            }
            if (IsExistingTransaction(transaction))
            {
                // Existing transaction found -> check propagation behavior to find out how to behave.
                return HandleExistingTransaction(definition, transaction, debugEnabled);
            }

            // Check definition settings for new transaction.
            if (definition.TransactionTimeout < DefaultTransactionDefinition.TIMEOUT_DEFAULT)
            {
                throw new InvalidTimeoutException("Invalid transaction timeout", definition.TransactionTimeout);
            }

            // No existing transaction found -> check propagation behavior to find out how to proceed.
            if (definition.PropagationBehavior == TransactionPropagation.Mandatory)
            {
                throw new IllegalTransactionStateException(
                    "Transaction propagation 'mandatory' but no existing transaction found");
            }
            else if (definition.PropagationBehavior == TransactionPropagation.Required ||
                     definition.PropagationBehavior == TransactionPropagation.RequiresNew ||
                      definition.PropagationBehavior == TransactionPropagation.Nested)
            {
                object suspendedResources = Suspend(null);
                if (debugEnabled)
                {
                    log.Debug("Creating new transaction with name [" + definition.Name + "]:" + definition);
                }
                try
                {
                    bool newSynchronization = (_transactionSyncState != TransactionSynchronizationState.Never);
                    DefaultTransactionStatus status = NewTransactionStatus(definition, transaction, true, newSynchronization, debugEnabled,
                                                                                             suspendedResources);
                    DoBegin(transaction, definition);
                    PrepareSynchronization(status, definition);
                    return status;
                }
                catch (TransactionException)
                {
                    Resume(null, suspendedResources);
                    throw;
                }
            }
            else
            {
                // Create "empty" transaction: no actual transaction, but potentially synchronization.
                bool newSynchronization = (_transactionSyncState == TransactionSynchronizationState.Always);
                return PrepareTransactionStatus(definition, null, true, newSynchronization, debugEnabled, null);

            }

        }

        protected DefaultTransactionStatus PrepareTransactionStatus(
            ITransactionDefinition definition, Object transaction, bool newTransaction,
            bool newSynchronization, bool debug, Object suspendedResources)
        {
            DefaultTransactionStatus status = NewTransactionStatus(
                    definition, transaction, newTransaction, newSynchronization, debug, suspendedResources);
            PrepareSynchronization(status, definition);
            return status;
        }


        protected void PrepareSynchronization(DefaultTransactionStatus status, ITransactionDefinition definition)
        {
            if (status.NewSynchronization)
            {
                TransactionSynchronizationManager.ActualTransactionActive = status.HasTransaction();
                TransactionSynchronizationManager.CurrentTransactionIsolationLevel = definition.TransactionIsolationLevel != System.Data.IsolationLevel.Unspecified ? definition.TransactionIsolationLevel : IsolationLevel.Unspecified;
                TransactionSynchronizationManager.CurrentTransactionReadOnly = definition.ReadOnly;
                TransactionSynchronizationManager.CurrentTransactionName = definition.Name;
                TransactionSynchronizationManager.InitSynchronization();
            }
        }

        private ITransactionStatus HandleExistingTransaction(ITransactionDefinition definition, object transaction, bool debugEnabled)
        {
            //bool newSynchronization;
            if (definition.PropagationBehavior == TransactionPropagation.Never)
            {
                throw new IllegalTransactionStateException(
                    "Transaction propagation 'never' but existing transaction found.");
            }
            if (definition.PropagationBehavior == TransactionPropagation.NotSupported)
            {
                if (debugEnabled)
                {
                    log.Debug("Suspending current transaction");
                }
                object suspendedResources = Suspend(transaction);
                bool newSynchronization = (_transactionSyncState == TransactionSynchronizationState.Always);
                return
                    PrepareTransactionStatus(definition, null, false, newSynchronization, debugEnabled,
                                         suspendedResources);
            }

            if (definition.PropagationBehavior == TransactionPropagation.RequiresNew)
            {
                if (debugEnabled)
                {
                    log.Debug("Suspending current transaction, creating new transaction with name [" +
                              definition.Name + "]:" + definition);
                }
                object suspendedResources = Suspend(transaction);
                try
                {
                    bool newSynchronization = (_transactionSyncState != TransactionSynchronizationState.Never);
                    DefaultTransactionStatus status = NewTransactionStatus(
                        definition, transaction, true, newSynchronization, debugEnabled, suspendedResources);
                    PrepareSynchronization(status, definition);
                    DoBegin(transaction, definition);
                    return status;
                }
                catch (TransactionException beginEx)
                {
                    try
                    {
                        Resume(transaction, suspendedResources);
                        //TODO: java code rethrows the ex here...should we do so as well?
                        //throw;
                    }
                    catch (TransactionException resumeEx)
                    {
                        log.Error(
                            "Inner transaction begin exception overridden by outer transaction resume exception");
                        log.Error("Begin Transaction Exception", beginEx);
                        log.Error("Resume Transaction Exception", resumeEx);
                        throw;
                    }
                    throw;
                }
            }
            if (definition.PropagationBehavior == TransactionPropagation.Nested)
            {
                if (!NestedTransactionsAllowed)
                {
                    throw new NestedTransactionNotSupportedException(
                        "Transaction manager does not allow nested transactions by default - " +
                        "specify 'NestedTransactionsAllowed' property with value 'true'");
                }
                if (debugEnabled)
                {
                    log.Debug("Creating nested transaction with name [" + definition.Name + "]:" + definition);
                }

                if (UseSavepointForNestedTransaction())
                {
                    DefaultTransactionStatus status =
                        PrepareTransactionStatus(definition, transaction, false, false, debugEnabled, null);
                    status.CreateAndHoldSavepoint(DateTime.Now.ToLongTimeString());
                    return status;
                }
                else
                {
                    bool newSynchronization = (_transactionSyncState != TransactionSynchronizationState.Never);
                    DefaultTransactionStatus status = NewTransactionStatus(definition, transaction, true, newSynchronization, debugEnabled, null);
                    PrepareSynchronization(status, definition);
                    DoBegin(transaction, definition);
                    return status;

                }
            }
            // Assumably PROPAGATION_SUPPORTS.
            if (debugEnabled)
            {
                log.Debug("Participating in existing transaction");
            }

            //TODO: this block related to un-ported java feature permitting setting the ValidateExistingTransaction flag
            //  default is FALSE anyway so skipping this validation block should have no effect on code excecution path
            /*if (isValidateExistingTransaction())
            {
                if (definition.getIsolationLevel() != TransactionDefinition.ISOLATION_DEFAULT)
                {
                    Integer currentIsolationLevel = TransactionSynchronizationManager.getCurrentTransactionIsolationLevel();
                    if (currentIsolationLevel == null || currentIsolationLevel != definition.getIsolationLevel())
                    {
                        Constants isoConstants = DefaultTransactionDefinition.constants;
                        throw new IllegalTransactionStateException("Participating transaction with definition [" +
                                definition + "] specifies isolation level which is incompatible with existing transaction: " +
                                (currentIsolationLevel != null ?
                                        isoConstants.toCode(currentIsolationLevel, DefaultTransactionDefinition.PREFIX_ISOLATION) :
                                        "(unknown)"));
                    }
                }*/

                if (!definition.ReadOnly)
                {
                    if (TransactionSynchronizationManager.CurrentTransactionReadOnly)
                    {
                        throw new IllegalTransactionStateException("Participating transaction with definition [" +
                                definition + "] is not marked as read-only but existing transaction is");
                    }
                }

            bool newSynch = (_transactionSyncState != TransactionSynchronizationState.Never);
            return PrepareTransactionStatus(definition, transaction, false, newSynch, debugEnabled, null);

        }

        /// <summary>
        /// This implementation of commit handles participating in existing transactions
        /// and programmatic rollback requests.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="transactionStatus">
        /// ITransactionStatus object returned by the
        /// <see cref="Spring.Transaction.IPlatformTransactionManager.GetTransaction"/>() method.
        /// </param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In case of commit or system errors.
        /// </exception>
        public void Commit(ITransactionStatus transactionStatus)
        {
            if (transactionStatus.Completed)
            {
                throw new IllegalTransactionStateException(
                    "Transaction is already completed - do not call commit or rollback more than once per transaction");
            }

            DefaultTransactionStatus defaultStatus = (DefaultTransactionStatus)transactionStatus;
            if (defaultStatus.LocalRollbackOnly)
            {
                if (defaultStatus.Debug)
                {
                    log.Debug("Transaction code has requested rollback");
                }
                ProcessRollback(defaultStatus);
                return;
            }
            if (!ShouldCommitOnGlobalRollbackOnly && defaultStatus.GlobalRollbackOnly)
            {
                if (defaultStatus.Debug)
                {
                    log.Debug("Global transaction is marked as rollback-only but transactional code requested commit");
                }
                ProcessRollback(defaultStatus);
                // Throw UnexpectedRollbackException only at outermost transaction boundary
                // or if explicitly asked to.
                if (defaultStatus.IsNewTransaction || FailEarlyOnGlobalRollbackOnly)
                {
                    throw new UnexpectedRollbackException(
                            "Transaction rolled back because it has been marked as rollback-only");
                }
                return;
            }
            ProcessCommit(defaultStatus);

        }

        protected virtual bool ShouldCommitOnGlobalRollbackOnly
        {
            get { return false; }
        }

        private void ProcessCommit(DefaultTransactionStatus status)
        {
            try
            {
                bool beforeCompletionInvoked = false;
                try
                {
                    TriggerBeforeCommit(status);
                    TriggerBeforeCompletion(status);
                    beforeCompletionInvoked = true;
                    bool globalRollbackOnly = false;
                    if (status.IsNewTransaction || FailEarlyOnGlobalRollbackOnly)
                    {
                        globalRollbackOnly = status.GlobalRollbackOnly;
                    }
                    if (status.HasSavepoint)
                    {
                        status.ReleaseHeldSavepoint();
                    }
                    else if (status.IsNewTransaction)
                    {
                        DoCommit(status);
                    }
                    // Throw UnexpectedRollbackException if we have a global rollback-only
                    // marker but still didn't get a corresponding exception from commit.
                    if (globalRollbackOnly)
                    {
                        throw new UnexpectedRollbackException(
                                "Transaction silently rolled back because it has been marked as rollback-only");
                    }
                }
                catch (UnexpectedRollbackException)
                {
                    TriggerAfterCompletion(status, TransactionSynchronizationStatus.Rolledback);
                    throw;
                }
                catch (TransactionException ex)
                {
                    if (RollbackOnCommitFailure)
                    {
                        DoRollbackOnCommitException(status, ex);
                    }
                    else
                    {
                        TriggerAfterCompletion(status, TransactionSynchronizationStatus.Unknown);
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    if (!beforeCompletionInvoked)
                    {
                        TriggerBeforeCompletion(status);
                    }
                    DoRollbackOnCommitException(status, ex);
                    throw;
                }
                // Trigger AfterCommit callbacks, with an exception thrown there
                // propagated to callers but the transaction still considered as commited.
                try
                {
                    TriggerAfterCommit(status);
                }
                finally
                {
                    TriggerAfterCompletion(status, TransactionSynchronizationStatus.Committed);
                }
            }
            finally
            {
                CleanupAfterCompletion(status);
            }
        }

        private void TriggerAfterCommit(DefaultTransactionStatus status)
        {
            if (status.NewSynchronization)
            {
                if (status.Debug)
                {
                    log.Debug("Trigger AfterCommit Synchronization");
                }
                IList synchronizations = TransactionSynchronizationManager.Synchronizations;
                foreach (ITransactionSynchronization currentTxnSynchronization in synchronizations)
                {
                    try
                    {
                        currentTxnSynchronization.AfterCommit();
                    }
                    catch (Exception e)
                    {
                        log.Error("TransactionSynchronization.AfterCommit thew exception", e);
                    }
                }
            }
        }

        /// <summary>
        /// Roll back the given transaction, with regard to its status.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation handles participating in existing transactions.
        /// </p>
        /// <p>
        /// Delegates to
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoRollback"/>,
        /// and
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoSetRollbackOnly"/>.
        /// </p>
        /// <p>
        /// If the transaction wasn't a new one, just set it rollback-only
        /// to take part in the surrounding transaction properly.
        /// </p>
        /// </remarks>
        /// <param name="transactionStatus">
        /// ITransactionStatusObject returned by the
        /// <see cref="Spring.Transaction.IPlatformTransactionManager.GetTransaction"/>() method.
        /// </param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In case of system errors.
        /// </exception>
        public void Rollback(ITransactionStatus transactionStatus)
        {
            if (transactionStatus.Completed)
            {
                throw new IllegalTransactionStateException(
                    "Transaction is already completed - do not call commit or rollback more than once per transaction");
            }
            DefaultTransactionStatus defaultStatus = (DefaultTransactionStatus)transactionStatus;
            ProcessRollback(defaultStatus);
        }

        private void ProcessRollback(DefaultTransactionStatus status)
        {
            try
            {
                try
                {
                    TriggerBeforeCompletion(status);
                    if (status.HasSavepoint)
                    {
                        if (status.Debug)
                        {
                            log.Debug("Rolling back transaction to savepoint.");
                        }
                        status.RollbackToHeldSavepoint();
                    }
                    else if (status.IsNewTransaction)
                    {
                        if (status.Debug)
                        {
                            log.Debug("Initiating transaction rollback");
                        }
                        DoRollback(status);
                    }
                    else if (status.HasTransaction())
                    {
                        if (status.LocalRollbackOnly)
                        {
                            if (status.Debug)
                            {
                                log.Debug("Participating transaction failed - marking existing transaction as rollback-only");
                            }
                        }
                        DoSetRollbackOnly(status);
                    }
                    else
                    {
                        log.Debug("Should roll back transaction but cannot - no transaction available.");
                    }
                }
                catch (Exception)
                {
                    TriggerAfterCompletion(status, TransactionSynchronizationStatus.Unknown);
                    throw;
                }
                TriggerAfterCompletion(status, TransactionSynchronizationStatus.Rolledback);
            }
            finally
            {
                CleanupAfterCompletion(status);
            }
        }

        #endregion

        #region Protected Method

        private DefaultTransactionStatus NewTransactionStatus(ITransactionDefinition definition,
                                              object transaction, bool newTransaction,
                                              bool newSynchronization, bool debug,
                                              object suspendedResources)
        {
            bool actualNewSynchronization = newSynchronization &&
                                !TransactionSynchronizationManager.SynchronizationActive;
            //            if (actualNewSynchronization)
            //            {
            //                TransactionSynchronizationManager.ActualTransactionActive = (transaction != null);
            //                TransactionSynchronizationManager.CurrentTransactionIsolationLevel =
            //                    definition.TransactionIsolationLevel;
            //                TransactionSynchronizationManager.CurrentTransactionReadOnly = definition.ReadOnly;
            //                TransactionSynchronizationManager.CurrentTransactionName = definition.Name;
            //                TransactionSynchronizationManager.InitSynchronization();
            //            }
            return
                new DefaultTransactionStatus(transaction, newTransaction, actualNewSynchronization, definition.ReadOnly, debug,
                                             suspendedResources);
        }

        /// <summary>
        /// Determines the timeout to use for the given definition.  Will fall back to this manager's default
        /// timeout if the transaction definition doesn't specify a non-default value.
        /// </summary>
        /// <param name="definition">The transaction definition.</param>
        /// <returns>the actual timeout to use.</returns>
        protected int DetermineTimeout(ITransactionDefinition definition)
        {
            if (definition.TransactionTimeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT)
            {
                return definition.TransactionTimeout;
            }
            return _defaultTimeout;
        }

        #endregion

        #region Private Methods





        /// <summary>
        /// Suspend the given transaction. Suspends transaction synchronization first,
        /// then delegates to the doSuspend template method.
        /// </summary>
        /// <param name="transaction">the current transaction object</param>
        /// <returns>an object that holds suspended resources</returns>
        private object Suspend(object transaction)
        {
            if (TransactionSynchronizationManager.SynchronizationActive)
            {
                IList suspendedSynchronizations = DoSuspendSynchronization();

                try
                {
                    object suspendedResources = null;
                    if (transaction != null)
                    {
                        suspendedResources = DoSuspend(transaction);
                    }

                    string name = TransactionSynchronizationManager.CurrentTransactionName;
                    TransactionSynchronizationManager.CurrentTransactionName = null;
                    bool readOnly = TransactionSynchronizationManager.CurrentTransactionReadOnly;
                    TransactionSynchronizationManager.CurrentTransactionReadOnly = false;
                    IsolationLevel isolationLevel = TransactionSynchronizationManager.CurrentTransactionIsolationLevel;
                    TransactionSynchronizationManager.CurrentTransactionIsolationLevel = IsolationLevel.Unspecified;
                    bool wasActive = TransactionSynchronizationManager.ActualTransactionActive;
                    TransactionSynchronizationManager.ActualTransactionActive = false;


                    return new SuspendedResourcesHolder(suspendedSynchronizations, suspendedResources,
                        name, readOnly, isolationLevel, wasActive);

                }
                catch (TransactionException)
                {
                    // DoSuspend failed - original transaction is still active
                    DoResumeSynchronization(suspendedSynchronizations);
                    throw;
                }
            }
            else if (transaction != null)
            {
                // Transaction active but no synchronization active.
                object suspendedResources = DoSuspend(transaction);
                return new SuspendedResourcesHolder(suspendedResources);
            }
            else
            {
                // Neither transaction nor synchronization active.
                return null;
            }

        }

        private IList DoSuspendSynchronization()
        {
            IList suspendedSynchronizations = TransactionSynchronizationManager.Synchronizations;
            foreach (ITransactionSynchronization currentTxnSynchronization in suspendedSynchronizations)
            {
                currentTxnSynchronization.Suspend();
            }
            TransactionSynchronizationManager.ClearSynchronization();
            return suspendedSynchronizations;
        }

        private void DoResumeSynchronization(IList suspendedSynchronizations)
        {
            TransactionSynchronizationManager.InitSynchronization();
            foreach (ITransactionSynchronization currentTxnSynchronization in suspendedSynchronizations)
            {
                currentTxnSynchronization.Resume();
                TransactionSynchronizationManager.RegisterSynchronization(currentTxnSynchronization);
            }
        }

        /// <summary>
        /// Resume the given transaction. Delegates to the doResume template method
        /// first, then resuming transaction synchronization.
        /// </summary>
        /// <param name="transaction">the current transaction object</param>
        /// <param name="suspendedResources"> the object that holds suspended resources, as returned by suspend</param>
        private void Resume(object transaction, object suspendedResources)
        {
            SuspendedResourcesHolder resourcesHolder = (SuspendedResourcesHolder)suspendedResources;
            if (resourcesHolder != null)
            {
                object suspendedResourcesObject = resourcesHolder.SuspendedResources;
                if (suspendedResourcesObject != null)
                {
                    DoResume(transaction, suspendedResourcesObject);
                }
                IList suspendedSynchronizations = resourcesHolder.SuspendedSynchronizations;
                if (suspendedSynchronizations != null)
                {
                    TransactionSynchronizationManager.ActualTransactionActive = resourcesHolder.WasActive;
                    TransactionSynchronizationManager.CurrentTransactionIsolationLevel = resourcesHolder.IsolationLevel;
                    TransactionSynchronizationManager.CurrentTransactionReadOnly = resourcesHolder.ReadOnly;
                    TransactionSynchronizationManager.CurrentTransactionName = resourcesHolder.Name;
                    DoResumeSynchronization(suspendedSynchronizations);
                }

            }
        }



        /// <summary>
        /// Invoke doRollback, handling rollback exceptions properly.
        /// </summary>
        /// <param name="status">object representing the transaction</param>
        /// <param name="exception">the thrown application exception or error</param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// in case of a rollback error
        /// </exception>
        private void DoRollbackOnCommitException(DefaultTransactionStatus status, Exception exception)
        {
            try
            {
                if (status.IsNewTransaction)
                {
                    if (status.Debug)
                    {
                        log.Debug("Initiating transaction rollback on commit exception.");
                    }
                    DoRollback(status);
                }
                else if (status.HasTransaction())
                {
                    if (status.Debug)
                    {
                        log.Debug("Marking existing transaction as rollback-only after commit exception", exception);
                    }
                    DoSetRollbackOnly(status);
                }
            }
            catch (Exception)
            {
                //TODO investigate rollback behavior...
                log.Error("Commit exception overridden by rollback exception", exception);
                TriggerAfterCompletion(status, TransactionSynchronizationStatus.Unknown);
                throw;
            }
            TriggerAfterCompletion(status, TransactionSynchronizationStatus.Rolledback);
        }

        /// <summary>
        /// Trigger beforeCommit callback.
        /// </summary>
        /// <param name="status">object representing the transaction</param>
        private void TriggerBeforeCommit(DefaultTransactionStatus status)
        {
            if (status.NewSynchronization)
            {
                IList synchronizations = TransactionSynchronizationManager.Synchronizations;
                foreach (ITransactionSynchronization currentTxnSynchronization in synchronizations)
                {
                    currentTxnSynchronization.BeforeCommit(status.ReadOnly);
                }
            }
        }

        /// <summary>
        /// Trigger beforeCompletion callback.
        /// </summary>
        /// <param name="status">object representing the transaction</param>
        private void TriggerBeforeCompletion(DefaultTransactionStatus status)
        {
            if (status.NewSynchronization)
            {
                if (status.Debug)
                {
                    log.Debug("Trigger BeforeCompletion Synchronization");
                }
                IList synchronizations = TransactionSynchronizationManager.Synchronizations;
                foreach (ITransactionSynchronization synchronization in synchronizations)
                {
                    try
                    {
                        synchronization.BeforeCompletion();
                    }
                    catch (Exception e)
                    {
                        log.Error("TransactionSynchronization.BeforeCompletion threw exception", e);
                    }
                }
            }
        }

        /// <summary>
        /// Trigger afterCompletion callback, handling exceptions properly.
        /// </summary>
        /// <param name="status">object representing the transaction</param>
        /// <param name="completionStatus">
        /// Completion status according to <see cref="Spring.Transaction.Support.TransactionSynchronizationStatus"/>
        /// </param>
        private void TriggerAfterCompletion(DefaultTransactionStatus status, TransactionSynchronizationStatus completionStatus)
        {
            if (status.NewSynchronization)
            {
                IList synchronizations = TransactionSynchronizationManager.Synchronizations;
                if (!status.HasTransaction() || status.IsNewTransaction)
                {
                    if (status.Debug)
                    {
                        log.Debug("Triggering afterCompletion synchronization");
                    }
                    InvokeAfterCompletion(synchronizations, completionStatus);
                }
                else
                {
                    //TODO investigate parallel of JTA/System.Txs
                    log.Info("Transaction controlled outside of spring tx manager.");
                    RegisterAfterCompletionWithExistingTransaction(status.Transaction, synchronizations);
                }
            }
        }

        private void InvokeAfterCompletion(IList synchronizations, TransactionSynchronizationStatus status)
        {
            foreach (ITransactionSynchronization synchronization in synchronizations)
            {
                try
                {
                    synchronization.AfterCompletion(status);
                }
                catch (Exception e)
                {
                    log.Error("TransactionSynchronization.AfterCompletion threw exception", e);
                }
            }
        }

        /// <summary>
        /// Clean up after completion, clearing synchronization if necessary,
        /// and invoking doCleanupAfterCompletion.
        /// </summary>
        /// <param name="status">object representing the transaction</param>
        private void CleanupAfterCompletion(DefaultTransactionStatus status)
        {
            status.Completed = true;
            if (status.NewSynchronization)
            {
                TransactionSynchronizationManager.Clear();
            }
            if (status.IsNewTransaction)
            {
                DoCleanupAfterCompletion(status.Transaction);
            }
            if (status.SuspendedResources != null)
            {
                if (status.Debug)
                {
                    log.Debug("Resuming suspended transaction");
                }
                Resume(status.Transaction, status.SuspendedResources);
            }
        }

        #endregion
    }
}
