/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Data;
using System.Reflection;
using System.Transactions;
using NHibernate;
using NHibernate.Transaction;

using Spring.Core.TypeResolution;
using Spring.Dao;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Data.Support;
using Spring.Objects.Factory;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// PlatformTransactionManager implementation for a single Hibernate SessionFactory.
    /// Binds a Hibernate Session from the specified factory to the thread, potentially
    /// allowing for one thread Session per factory
    /// </summary>
    /// <remarks>
    /// SessionFactoryUtils and HibernateTemplate are aware of thread-bound Sessions and participate in such
    /// transactions automatically. Using either of those is required for Hibernate
    /// access code that needs to support this transaction handling mechanism.
    /// <para>
    /// Supports custom isolation levels at the start of the transaction
    /// , and timeouts that get applied as appropriate
    /// Hibernate query timeouts. To support the latter, application code must either use
    /// <code>HibernateTemplate</code> (which by default applies the timeouts) or call
    /// <code>SessionFactoryUtils.applyTransactionTimeout</code> for each created
    /// Hibernate Query object.
    /// </para>
    /// <para>Note that you can specify a Spring IDbProvider instance which if shared with
    /// a corresponding instance of AdoTemplate will allow for mixing ADO.NET/NHibernate
    /// operations within a single transaction.</para>
    /// </remarks>
    /// <author>Mark Pollack (.NET)</author>
    public class HibernateTxScopeTransactionManager : AbstractPlatformTransactionManager, IResourceTransactionManager, IObjectFactoryAware, IInitializingObject
    {
        private ISessionFactory sessionFactory;

        private IDbProvider dbProvider;

        private bool autodetectDbProvider = true;

        private Object entityInterceptor;

        private IAdoExceptionTranslator adoExceptionTranslator;

        private IAdoExceptionTranslator defaultExceptionTranslator;

        /// <summary>
        /// Just needed for entityInterceptorBeanName. 
        /// </summary>
        private IObjectFactory objectFactory;

        private TxScopeTransactionManager txScopeTranactionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateTxScopeTransactionManager"/> class.
        /// </summary>
        public HibernateTxScopeTransactionManager()
        {
            txScopeTranactionManager = new TxScopeTransactionManager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateTxScopeTransactionManager"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public HibernateTxScopeTransactionManager(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
            AfterPropertiesSet();
        }

        /// <summary>
        /// Gets or sets the db provider.
        /// </summary>
        /// <value>The db provider.</value>
        public IDbProvider DbProvider
        {
            get { return dbProvider; }
            set { dbProvider = value; }
        }

        /// <summary>
        /// Gets or sets a Hibernate entity interceptor that allows to inspect and change
        /// property values before writing to and reading from the database.
        /// When getting, return the current Hibernate entity interceptor, or <code>null</code> if none.
        /// </summary>
        /// <value>The entity interceptor.</value>
        /// <remarks>
        /// Resolves an entity interceptor object name via the object factory,
        /// if necessary.
        /// Will get applied to any new Session created by this transaction manager.
        /// Such an interceptor can either be set at the SessionFactory level,
        /// i.e. on LocalSessionFactoryObject, or at the Session level, i.e. on
        /// HibernateTemplate, HibernateInterceptor, and HibernateTransactionManager.
        /// It's preferable to set it on LocalSessionFactoryObject or HibernateTransactionManager
        /// to avoid repeated configuration and guarantee consistent behavior in transactions.
        /// </remarks>
        /// <exception cref="InvalidOperationException">If object factory is null and need to get entity interceptor via object name.</exception>
        public IInterceptor EntityInterceptor
        {
            get
            {
                if (this.entityInterceptor is IInterceptor)
                {
                    return (IInterceptor)entityInterceptor;
                }
                else if (this.entityInterceptor is string)
                {
                    if (this.objectFactory == null)
                    {
                        throw new InvalidOperationException("Cannot get entity interceptor via object name if no object factory set");
                    }
                    String objectName = (String)this.entityInterceptor;
                    return (IInterceptor)this.objectFactory.GetObject(objectName, typeof(IInterceptor));
                }
                else
                {
                    return null;
                }
            }
            set
            {
                entityInterceptor = value;
            }
        }

        /// <summary>
        /// Sets the object name of a Hibernate entity interceptor that
        /// allows to inspect and change property values before writing to and reading from the database.
        /// </summary>
        /// <value>The name of the entity interceptor object.</value>
        /// <remarks>
        /// Will get applied to any new Session created by this transaction manager.
        /// <p>Requires the object factory to be known, to be able to resolve the object
        /// name to an interceptor instance on session creation. Typically used for
        /// prototype interceptors, i.e. a new interceptor instance per session.
        /// </p>
        /// 	<p>Can also be used for shared interceptor instances, but it is recommended
        /// to set the interceptor reference directly in such a scenario.
        /// </p>
        /// </remarks>
        public string EntityInterceptorObjectName
        {
            set
            {
                entityInterceptor = value;
            }
        }

        /// <summary>
        /// Gets or sets the ADO.NET exception translator for this transaction manager.
        /// </summary>
        /// <remarks>
        /// Applied to ADO.NET Exceptions (wrapped by Hibernate's ADOException)
        /// </remarks>
        /// <value>The ADO exception translator.</value>
        public IAdoExceptionTranslator AdoExceptionTranslator
        {
            get { return adoExceptionTranslator; }
            set { adoExceptionTranslator = value; }
        }

        /// <summary>
        /// Gets the default IAdoException translator, lazily creating it if nece
        /// </summary>
        /// <value>The default IAdoException translator.</value>
        public IAdoExceptionTranslator DefaultAdoExceptionTranslator
        {
            get
            {
                lock (this)
                {
                    if (defaultExceptionTranslator == null)
                    {
                        if (dbProvider != null)
                        {
                            defaultExceptionTranslator = new ErrorCodeExceptionTranslator(dbProvider);
                        }
                        else
                        {
                            defaultExceptionTranslator = SessionFactoryUtils.NewAdoExceptionTranslator(SessionFactory);
                        }
                    }
                    return defaultExceptionTranslator;
                }
            }
        }

        /// <summary>
        /// Gets or sets the SessionFactory that this instance should manage transactions for.
        /// </summary>
        /// <value>The session factory.</value>
        public ISessionFactory SessionFactory
        {
            get { return sessionFactory; }
            set { sessionFactory = value; }
        }


        /// <summary>
        /// Gets the resource factory that this transaction manager operates on,
        /// For the HibenratePlatformTransactionManager this the SessionFactory
        /// </summary>
        /// <value>The SessionFactory.</value>
        public object ResourceFactory
        {
            get { return sessionFactory; }
        }

        /// <summary>
        /// Set whether to autodetect a ADO.NET connection used by the Hibernate SessionFactory,
        /// if set via LocalSessionFactoryObject's <code>DbProvider</code>. Default is "true".
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [autodetect data source]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// 	<p>Can be turned off to deliberately ignore an available IDbProvider,
        /// to not expose Hibernate transactions as ADO.NET transactions for that IDbProvider.
        /// </p>
        /// </remarks>
        public bool AutodetectDbProvider
        {
            set { autodetectDbProvider = value; }
        }

        /// <summary>
        /// The object factory just needs to be known for resolving entity interceptor
        /// It does not need to be set for any other mode of operation.
        /// </summary>
        /// <value>
        /// Owning <see cref="T:Spring.Objects.Factory.IObjectFactory"/>
        /// (may not be <see langword="null"/>). The object can immediately
        /// call methods on the factory.
        /// </value>
        public IObjectFactory ObjectFactory
        {
            set
            {
                objectFactory = value;
            }
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


            HibernateTransactionObject txObject = new HibernateTransactionObject();
            txObject.SavepointAllowed = NestedTransactionsAllowed;
            if (TransactionSynchronizationManager.HasResource(SessionFactory))
            {
                SessionHolder sessionHolder =
                    (SessionHolder)TransactionSynchronizationManager.GetResource(SessionFactory);
                if (log.IsDebugEnabled)
                {
                    log.Debug("Found thread-bound Session [" + sessionHolder.Session +
                        "] for Hibernate transaction");
                }
                txObject.SetSessionHolder(sessionHolder, false);
                if (DbProvider != null)
                {
                    ConnectionHolder conHolder = (ConnectionHolder)
                        TransactionSynchronizationManager.GetResource(DbProvider);
                    txObject.ConnectionHolder = conHolder;
                }
            }
            txObject.PromotableTxScopeTransactionObject = new TxScopeTransactionManager.PromotableTxScopeTransactionObject();

            return txObject;
        }

        /// <summary>
        /// Check if the given transaction object indicates an existing,
        /// i.e. already begun, transaction.
        /// </summary>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <returns>True if there is an existing transaction.</returns>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override bool IsExistingTransaction(object transaction)
        {
            var hibernateTransactionObject = ((HibernateTransactionObject) transaction);

            var hasExistingPromotableTxScopeTransaction = hibernateTransactionObject.PromotableTxScopeTransactionObject.TxScopeAdapter.IsExistingTransaction;
            var hasExistingTransaction = hibernateTransactionObject.HasTransaction();

            return hasExistingPromotableTxScopeTransaction && hasExistingTransaction; 
        }

        /// <summary>
        /// Begin a new transaction with the given transaction definition.
        /// </summary>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <param name="definition">
        /// <see cref="Spring.Transaction.ITransactionDefinition"/> instance, describing
        /// propagation behavior, isolation level, timeout etc.
        /// </param>
        /// <remarks>
        /// Does not have to care about applying the propagation behavior,
        /// as this has already been handled by this abstract manager.
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of creation or system errors.
        /// </exception>
        protected override void DoBegin(object transaction, ITransactionDefinition definition)
        {

            TxScopeTransactionManager.PromotableTxScopeTransactionObject promotableTxScopeTransactionObject =
                ((HibernateTransactionObject)transaction).PromotableTxScopeTransactionObject;
            try
            {
                DoTxScopeBegin(promotableTxScopeTransactionObject, definition);
            }
            catch (Exception e)
            {
                throw new CannotCreateTransactionException("Transaction Scope failure on begin", e);
            }

            HibernateTransactionObject txObject = (HibernateTransactionObject)transaction;

            if (DbProvider != null && TransactionSynchronizationManager.HasResource(DbProvider)
                && !txObject.ConnectionHolder.SynchronizedWithTransaction)
            {
                throw new IllegalTransactionStateException(
                    "Pre-bound ADO.NET Connection found - HibernateTransactionManager does not support " +
                    "running within AdoTransactionManager if told to manage the DbProvider itself. " +
                    "It is recommended to use a single HibernateTransactionManager for all transactions " +
                    "on a single DbProvider, no matter whether Hibernate or ADO.NET access.");
            }
            ISession session = null;
            try
            {

                if (txObject.SessionHolder == null || txObject.SessionHolder.SynchronizedWithTransaction)
                {
                    IInterceptor interceptor = EntityInterceptor;
                    ISession newSession = (interceptor != null ?
                            SessionFactory.OpenSession(interceptor) : SessionFactory.OpenSession());

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Opened new Session [" + newSession + "] for Hibernate transaction");
                    }
                    txObject.SetSessionHolder(new SessionHolder(newSession), true);

                }
                txObject.SessionHolder.SynchronizedWithTransaction = true;
                session = txObject.SessionHolder.Session;

                IDbConnection con = session.Connection;
                //TODO isolation level mgmt
                //IsolationLevel previousIsolationLevel = 

                if (definition.ReadOnly && txObject.NewSessionHolder)
                {
                    // Just set to NEVER in case of a new Session for this transaction.
                    session.FlushMode = FlushMode.Never;
                }

                if (!definition.ReadOnly && !txObject.NewSessionHolder)
                {
                    // We need AUTO or COMMIT for a non-read-only transaction.
                    FlushMode flushMode = session.FlushMode;
                    if (FlushMode.Never == flushMode)
                    {
                        session.FlushMode = FlushMode.Auto;
                        txObject.SessionHolder.PreviousFlushMode = flushMode;
                    }
                }

                // Add the Hibernate transaction to the session holder.
                // for now pass in tx options isolation level.
                ITransaction hibernateTx = session.BeginTransaction(definition.TransactionIsolationLevel);
                IDbTransaction adoTx = GetIDbTransaction(hibernateTx);

                // Add the Hibernate transaction to the session holder.
                txObject.SessionHolder.Transaction = hibernateTx;

                // Register transaction timeout.
                int timeout = DetermineTimeout(definition);
                if (timeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT)
                {
                    txObject.SessionHolder.TimeoutInSeconds = timeout;
                }

                // Register the Hibernate Session's ADO.NET Connection/TX pair for the DbProvider, if set.
                if (DbProvider != null)
                {
                    //investigate passing null for tx.
                    ConnectionHolder conHolder = new ConnectionHolder(con, adoTx);
                    if (timeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT)
                    {
                        conHolder.TimeoutInSeconds = timeout;
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Exposing Hibernate transaction as ADO transaction [" + con + "]");
                    }
                    TransactionSynchronizationManager.BindResource(DbProvider, conHolder);
                    txObject.ConnectionHolder = conHolder;
                }

                // Bind the session holder to the thread.
                if (txObject.NewSessionHolder)
                {
                    TransactionSynchronizationManager.BindResource(SessionFactory, txObject.SessionHolder);
                }

            }
            catch (Exception ex)
            {
                SessionFactoryUtils.CloseSession(session);
                throw new CannotCreateTransactionException("Could not open Hibernate Session for transaction", ex);
            }


        }

        private void DoTxScopeBegin(TxScopeTransactionManager.PromotableTxScopeTransactionObject txObject,
                                    Spring.Transaction.ITransactionDefinition definition)
        {

            TransactionScopeOption txScopeOption = CreateTransactionScopeOptions(definition);
            TransactionOptions txOptions = CreateTransactionOptions(definition);
            txObject.TxScopeAdapter.CreateTransactionScope(txScopeOption, txOptions, definition.AsyncFlowOption);

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
                throw new Spring.Transaction.TransactionSystemException("Transaction Propagation Behavior" +
                                                                        definition.PropagationBehavior +
                                                                        " not supported by TransactionScope.  Use Required or RequiredNew");
            }
            return txScopeOption;
        }


        private static TransactionOptions CreateTransactionOptions(ITransactionDefinition definition)
        {
            TransactionOptions txOptions = new TransactionOptions();
            switch (definition.TransactionIsolationLevel)
            {
                case System.Data.IsolationLevel.Chaos:
                    txOptions.IsolationLevel = System.Transactions.IsolationLevel.Chaos;
                    break;
                case System.Data.IsolationLevel.ReadCommitted:
                    txOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                    break;
                case System.Data.IsolationLevel.ReadUncommitted:
                    txOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                    break;
                case System.Data.IsolationLevel.RepeatableRead:
                    txOptions.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                    break;
                case System.Data.IsolationLevel.Serializable:
                    txOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
                    break;
                case System.Data.IsolationLevel.Snapshot:
                    txOptions.IsolationLevel = System.Transactions.IsolationLevel.Snapshot;
                    break;
                case System.Data.IsolationLevel.Unspecified:
                    txOptions.IsolationLevel = System.Transactions.IsolationLevel.Unspecified;
                    break;
            }

            if (definition.TransactionTimeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT)
            {
                txOptions.Timeout = new TimeSpan(0, 0, definition.TransactionTimeout);
            }
            return txOptions;
        }



        /// <summary>
        /// Suspend the resources of the current transaction.
        /// </summary>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <returns>
        /// An object that holds suspended resources (will be kept unexamined for passing it into
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoResume"/>.)
        /// </returns>
        /// <remarks>
        /// Transaction synchronization will already have been suspended.
        /// </remarks>
        /// <exception cref="Spring.Transaction.IllegalTransactionStateException">
        /// If suspending is not supported by the transaction manager implementation.
        /// </exception>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// in case of system errors.
        /// </exception>
        protected override object DoSuspend(object transaction)
        {
            HibernateTransactionObject txObject = (HibernateTransactionObject)transaction;
            txObject.SetSessionHolder(null, false);
            SessionHolder sessionHolder =
                (SessionHolder)TransactionSynchronizationManager.UnbindResource(SessionFactory);
            ConnectionHolder connectionHolder = null;
            if (DbProvider != null)
            {
                connectionHolder = (ConnectionHolder)TransactionSynchronizationManager.UnbindResource(DbProvider);
            }
            return new SuspendedResourcesHolder(sessionHolder, connectionHolder);

        }

        /// <summary>
        /// Resume the resources of the current transaction.
        /// </summary>
        /// <param name="transaction">
        /// Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.
        /// </param>
        /// <param name="suspendedResources">
        /// The object that holds suspended resources as returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoSuspend"/>.
        /// </param>
        /// <remarks>
        /// Transaction synchronization will be resumed afterwards.
        /// </remarks>
        /// <exception cref="Spring.Transaction.IllegalTransactionStateException">
        /// If suspending is not supported by the transaction manager implementation.
        /// </exception>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override void DoResume(object transaction, object suspendedResources)
        {
            SuspendedResourcesHolder resourcesHolder = (SuspendedResourcesHolder)suspendedResources;
            if (TransactionSynchronizationManager.HasResource(SessionFactory))
            {
                // From non-transactional code running in active transaction synchronization
                // -> can be safely removed, will be closed on transaction completion.
                TransactionSynchronizationManager.UnbindResource(SessionFactory);
            }
            TransactionSynchronizationManager.BindResource(SessionFactory, resourcesHolder.SessionHolder);
            if (DbProvider != null)
            {
                TransactionSynchronizationManager.BindResource(DbProvider, resourcesHolder.ConnectionHolder);
            }
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
        protected override void DoCommit(DefaultTransactionStatus status)
        {
            HibernateTransactionObject txObject = (HibernateTransactionObject)status.Transaction;
            if (status.Debug)
            {
                log.Debug("Committing Hibernate transaction on Session [" +
                    txObject.SessionHolder.Session + "]");
            }
            try
            {
                txObject.SessionHolder.Transaction.Commit();
            }
            // Note, unfortunate collision of namespaces/classname for NHibernate.TransactionException
            // and Spring.Data.NHibernate requires this wierd construct.
            catch (Exception ex)
            {
                Type nhibTxExceptiontype = TypeResolutionUtils.ResolveType("NHibernate.TransactionException, NHibernate");
                if (ex.GetType().Equals(nhibTxExceptiontype))
                {
                    // assumably from commit call to the underlying ADO.NET connection
                    throw new TransactionSystemException("Could not commit Hibernate transaction", ex);
                }
                HibernateException hibEx = ex as HibernateException;
                if (hibEx != null)
                {
                    // assumably failed to flush changes to database
                    throw ConvertHibernateAccessException(hibEx);
                }
                throw;
            }
            finally
            {
                DoTxScopeCommit(status);
            }



        }

        /// <summary>
        /// Does the tx scope commit.
        /// </summary>
        /// <param name="status">The status.</param>
        protected void DoTxScopeCommit(DefaultTransactionStatus status)
        {
            TxScopeTransactionManager.PromotableTxScopeTransactionObject txObject =
                ((HibernateTransactionObject)status.Transaction).PromotableTxScopeTransactionObject;
            try
            {
                txObject.TxScopeAdapter.Complete();
                txObject.TxScopeAdapter.Dispose();
            }
            catch (TransactionAbortedException ex)
            {
                throw new UnexpectedRollbackException("Transaction unexpectedly rolled back (maybe due to a timeout)", ex);
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
        protected override void DoRollback(DefaultTransactionStatus status)
        {
            HibernateTransactionObject txObject = (HibernateTransactionObject)status.Transaction;

            if (!txObject.NewSessionHolder)
            {
                // Clear all pending inserts/updates/deletes in the Session.
                // Necessary for pre-bound Sessions, to avoid inconsistent state.
                txObject.SessionHolder.Session.Clear();
            }
            
            DoTxScopeRollback(status);
            return;


/*            HibernateTransactionObject txObject = (HibernateTransactionObject)status.Transaction;

            if (status.Debug)
            {
                log.Debug("Rolling back Hibernate transaction on Session [" +
                    txObject.SessionHolder.Session + "]");
            }
            try
            {
                if (txObject.SessionHolder.Session != null && txObject.SessionHolder.Transaction != null && !txObject.SessionHolder.Transaction.IsActive)
                {
                    return;
                }

                IDbTransaction adoTx = GetIDbTransaction(txObject.SessionHolder.Transaction);

                if (adoTx != null && adoTx.Connection != null)
                {
                    txObject.SessionHolder.Transaction.Rollback();
                }
                else
                {
                    if (status.Debug)
                    {
                        log.Debug("Unable to RollBack Hibernate transaction; connection for Hibernate transaction on Session [" +
                            txObject.SessionHolder.Session + "] was null");
                    }
                }

            }
            catch (HibernateTransactionException ex)
            {
                throw new TransactionSystemException("Could not roll back Hibernate transaction", ex);
            }
            catch (HibernateException ex)
            {
                // Shouldn't really happen, as a rollback doesn't cause a flush.
                throw ConvertHibernateAccessException(ex);
            }
            finally
            {
                if (!txObject.NewSessionHolder)
                {
                    // Clear all pending inserts/updates/deletes in the Session.
                    // Necessary for pre-bound Sessions, to avoid inconsistent state.
                    txObject.SessionHolder.Session.Clear();
                }
                DoTxScopeRollback(status);
            }*/
        }

        /// <summary>
        /// Does the tx scope rollback.
        /// </summary>
        /// <param name="status">The status.</param>
        protected void DoTxScopeRollback(DefaultTransactionStatus status)
        {
            TxScopeTransactionManager.PromotableTxScopeTransactionObject txObject =
                ((HibernateTransactionObject)status.Transaction).PromotableTxScopeTransactionObject;

            try
            {

                txObject.TxScopeAdapter.Dispose();
            }
            catch (Exception e)
            {
                throw new Spring.Transaction.TransactionSystemException("Failure on Transaction Scope rollback.", e);
            }
        }

        /// <summary>
        /// Set the given transaction rollback-only. Only called on rollback
        /// if the current transaction takes part in an existing one.
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
            HibernateTransactionObject txObject = (HibernateTransactionObject)status.Transaction;
            if (status.Debug)
            {
                log.Debug("Setting Hibernate transaction on Session [" +
                    txObject.SessionHolder.Session + "] rollback-only");
            }
            txObject.SetRollbackOnly();

            DoTxScopeSetRollbackOnly(status);
        }

        /// <summary>
        /// Does the tx scope set rollback only.
        /// </summary>
        /// <param name="status">The status.</param>
        protected void DoTxScopeSetRollbackOnly(DefaultTransactionStatus status)
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


        /// <summary>
        /// Gets the ADO.NET IDbTransaction object from the NHibernate ITransaction object.
        /// </summary>
        /// <param name="hibernateTx">The hibernate transaction.</param>
        /// <returns>The ADO.NET transaction.  Null if could not get the transaction.  Warning
        /// messages will be logged in that case.</returns>
        protected IDbTransaction GetIDbTransaction(ITransaction hibernateTx)
        {
            AdoTransaction hibernateAdoTx = hibernateTx as AdoTransaction;

            IDbTransaction adoTransaction = null;
            if (hibernateAdoTx != null)
            {
                try
                {
                    FieldInfo fi = hibernateAdoTx.GetType().GetField("trans", BindingFlags.Instance | BindingFlags.NonPublic);
                    adoTransaction = fi.GetValue(hibernateAdoTx) as IDbTransaction;
                }
                catch (Exception e)
                {
                    log.Warn("Could not extract IDbTransaction from Hibernate AdoTransaction using field name trans.", e);
                }
            }
            else
            {
                log.Warn("Hibernate ITransaction not of expected type AdoTransaction.  Could not extract IDbTransaction from Hibernate AdoTransaction.");
            }
            return adoTransaction;
        }

        /// <summary>
        /// Convert the given HibernateException to an appropriate exception from
        /// the Spring.Dao hierarchy. Can be overridden in subclasses.
        /// </summary>
        /// <param name="ex">The HibernateException that occured.</param>
        /// <returns>The corresponding DataAccessException instance</returns>
        protected virtual DataAccessException ConvertHibernateAccessException(HibernateException ex)
        {
            if (AdoExceptionTranslator != null && ex is ADOException)
            {
                return ConvertAdoAccessException((ADOException)ex, AdoExceptionTranslator);
            }
            else if (ex is ADOException)
            {
                return ConvertAdoAccessException((ADOException)ex, DefaultAdoExceptionTranslator);
            }
            return SessionFactoryUtils.ConvertHibernateAccessException(ex);
        }

        /// <summary>
        /// Convert the given ADOException to an appropriate exception from the
        /// the Spring.Dao hierarchy. Can be overridden in subclasses.
        /// </summary>
        /// <param name="ex">The ADOException that occured, wrapping the underlying
        /// ADO.NET thrown exception.</param>
        /// <param name="translator">The translator to convert hibernate ADOExceptions.</param>
        /// <returns>
        /// The corresponding DataAccessException instance
        /// </returns>
        protected virtual DataAccessException ConvertAdoAccessException(ADOException ex, IAdoExceptionTranslator translator)
        {
            return translator.Translate("Hibernate flushing: " + ex.Message, null, ex.InnerException);
        }

        /// <summary>
        /// Cleanup resources after transaction completion.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="M:Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <remarks>
        /// <note>
        /// This implemenation unbinds the SessionFactory and 
        /// DbProvider from thread local storage and closes the 
        /// ISession.
        /// </note>
        /// 	<p>
        /// Called after <see cref="M:Spring.Transaction.Support.AbstractPlatformTransactionManager.DoCommit(Spring.Transaction.Support.DefaultTransactionStatus)"/>
        /// and
        /// <see cref="M:Spring.Transaction.Support.AbstractPlatformTransactionManager.DoRollback(Spring.Transaction.Support.DefaultTransactionStatus)"/>
        /// execution on any outcome.
        /// </p>
        /// 	<p>
        /// Should not throw any exceptions but just issue warnings on errors.
        /// </p>
        /// </remarks>
        protected override void DoCleanupAfterCompletion(object transaction)
        {
            HibernateTransactionObject txObject = (HibernateTransactionObject)transaction;

            // Remove the session holder from the thread.
            if (txObject.NewSessionHolder)
            {
                TransactionSynchronizationManager.UnbindResource(SessionFactory);
            }
            // Remove the ADO.NET connection holder from the thread, if exposed.
            if (DbProvider != null)
            {
                TransactionSynchronizationManager.UnbindResource(DbProvider);
            }
            /*
            try 
            {
                //TODO investigate isolation level settings...
                //IDbConnection con = txObject.SessionHolder.Session.Connection;
                //AdoUtils.ResetConnectionAfterTransaction(con, txObject.PreviousIsolationLevel);
            }
            catch (HibernateException ex) 
            {
                log.Info("Could not access ADO.NET IDbConnection of Hibernate Session", ex);
            }
            */
            ISession session = txObject.SessionHolder.Session;
            
            if (txObject.NewSessionHolder)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Closing Hibernate Session [" + session + "] after transaction");
                }

                SessionFactoryUtils.CloseSessionOrRegisterDeferredClose(session, SessionFactory);
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Not closing pre-bound Hibernate Session [" + session + "] after transaction");
                }
                if (txObject.SessionHolder.AssignedPreviousFlushMode)
                {
                    session.FlushMode = txObject.SessionHolder.PreviousFlushMode;
                }
            }

            txObject.SessionHolder.Clear();
        }

        private class HibernateTransactionObject : AdoTransactionObjectSupport
        {

            private SessionHolder sessionHolder;

            private bool newSessionHolder;

            private TxScopeTransactionManager.PromotableTxScopeTransactionObject promotableTxScopeTransactionObject;


            public void SetSessionHolder(SessionHolder sessionHolder, bool newSessionHolder)
            {
                this.sessionHolder = sessionHolder;
                this.newSessionHolder = newSessionHolder;
            }

            public TxScopeTransactionManager.PromotableTxScopeTransactionObject PromotableTxScopeTransactionObject
            {
                get { return promotableTxScopeTransactionObject; }
                set { this.promotableTxScopeTransactionObject = value; }
            }

            public SessionHolder SessionHolder
            {
                get
                {
                    return sessionHolder;
                }
            }

            public bool NewSessionHolder
            {
                get
                {
                    return newSessionHolder;
                }
            }

            public bool HasTransaction()
            {
                return (this.sessionHolder != null && this.sessionHolder.Transaction != null);
            }

            public void SetRollbackOnly()
            {
                if (SessionHolder != null)
                {
                    SessionHolder.RollbackOnly = true;
                }
                if (ConnectionHolder != null)
                {
                    ConnectionHolder.RollbackOnly = true;
                }
            }

            /// <summary>
            /// Return whether the transaction is internally marked as rollback-only.
            /// </summary>
            /// <value></value>
            /// <returns>True of the transaction is marked as rollback-only.</returns>
            public override bool RollbackOnly
            {
                get
                {
                    return ((SessionHolder != null && SessionHolder.RollbackOnly) ||
                           (ConnectionHolder != null && ConnectionHolder.RollbackOnly));
                }
            }
        }

        private class SuspendedResourcesHolder
        {

            private readonly SessionHolder sessionHolder;

            private readonly ConnectionHolder connectionHolder;

            public SuspendedResourcesHolder(SessionHolder sessionHolder, ConnectionHolder conHolder)
            {
                this.sessionHolder = sessionHolder;
                this.connectionHolder = conHolder;
            }

            public SessionHolder SessionHolder
            {
                get
                {
                    return sessionHolder;
                }

            }

            public ConnectionHolder ConnectionHolder
            {
                get
                {
                    return connectionHolder;
                }

            }
        }

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// <p>
        /// Please do consult the class level documentation for the
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
        /// description of exactly <i>when</i> this method is invoked. In
        /// particular, it is worth noting that the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// callbacks will have been invoked <i>prior</i> to this method being
        /// called.
        /// </p>
        /// </remarks>
        /// <exception cref="System.ArgumentException">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
        public void AfterPropertiesSet()
        {
            if (SessionFactory == null)
            {
                throw new ArgumentException("sessionFactory is required");
            }
            if (this.entityInterceptor is string && this.objectFactory == null)
            {
                throw new ArgumentException("objectFactory is required for entityInterceptorBeanName");
            }

            // Try to derive a DbProvider given the SessionFactory.
            if (this.autodetectDbProvider && DbProvider == null)
            {
                IDbProvider sfDbProvider = SessionFactoryUtils.GetDbProvider(SessionFactory);
                if (sfDbProvider != null)
                {
                    // Use the SessionFactory's DataSource for exposing transactions to ADO.NET code.
                    if (log.IsInfoEnabled)
                    {
                        log.Info("Derived DbProvider [" + sfDbProvider.DbMetadata.ProductName +
                                "] of Hibernate SessionFactory for HibernateTransactionManager");
                    }
                    DbProvider = sfDbProvider;
                }
                else
                {
                    log.Info("Could not auto detect DbProvider from SessionFactory configuration");
                }

            }
        }
    }


}
