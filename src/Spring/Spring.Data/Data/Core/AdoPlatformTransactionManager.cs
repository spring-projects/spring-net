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

using System.Data;
using Common.Logging;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Objects.Factory;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.Core
{
    /// <summary>
    /// ADO.NET based implementation of the <see cref="Spring.Transaction.IPlatformTransactionManager"/>
    /// interface.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class AdoPlatformTransactionManager : AbstractPlatformTransactionManager, IResourceTransactionManager, IInitializingObject
    {

        private IDbProvider dbProvider;

        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (AdoPlatformTransactionManager));

        #endregion

        public AdoPlatformTransactionManager()
        {
            NestedTransactionsAllowed = true;
        }

        public AdoPlatformTransactionManager(IDbProvider dbProvider) : this()
        {
            DbProvider = dbProvider;

        }

        #region Propeties

        public IDbProvider DbProvider
        {
            get { return dbProvider; }
            set { dbProvider = value; }
        }

        #endregion

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
            DbProviderTransactionObject txMgrStateObject =
                new DbProviderTransactionObject();
            txMgrStateObject.SavepointAllowed = NestedTransactionsAllowed;
            ConnectionHolder conHolder =
                (ConnectionHolder) TransactionSynchronizationManager.GetResource(DbProvider);
            txMgrStateObject.SetConnectionHolder(conHolder, false);
            return txMgrStateObject;
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
            DbProviderTransactionObject txMgrStateObject =
                (DbProviderTransactionObject)transaction;
            return (txMgrStateObject.ConnectionHolder != null
                    &&
                    txMgrStateObject.ConnectionHolder.TransactionActive);

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
            DbProviderTransactionObject txMgrStateObject =
                (DbProviderTransactionObject)transaction;
            IDbConnection con = null;

            if (dbProvider == null)
            {
                throw new ArgumentException("DbProvider is required to be set on AdoPlatformTransactionManager");
            }

            try
            {
                if (txMgrStateObject.ConnectionHolder == null || txMgrStateObject.ConnectionHolder.SynchronizedWithTransaction)
                {
                    IDbConnection newCon = DbProvider.CreateConnection();
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Acquired Connection [" + newCon + ", " + newCon.ConnectionString + "] for ADO.NET transaction");
                    }
                    newCon.Open();

                    //TODO isolation level mgmt - will need to abstract out SQL used to specify this in DbMetaData
                    //MSDN docs...
                    //With one exception, you can switch from one isolation level to another at any time during a transaction. The exception occurs when changing from any isolation level to SNAPSHOT isolation


                    //IsolationLevel previousIsolationLevel =

                    IDbTransaction newTrans = newCon.BeginTransaction(definition.TransactionIsolationLevel);

                    txMgrStateObject.SetConnectionHolder(new ConnectionHolder(newCon, newTrans), true);

                }
                txMgrStateObject.ConnectionHolder.SynchronizedWithTransaction = true;
                con = txMgrStateObject.ConnectionHolder.Connection;


                txMgrStateObject.ConnectionHolder.TransactionActive = true;

                int timeout = DetermineTimeout(definition);
                if (timeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT)
                {
                    txMgrStateObject.ConnectionHolder.TimeoutInSeconds = timeout;
                }


                //Bind transactional resources to thread
                if (txMgrStateObject.NewConnectionHolder)
                {
                    TransactionSynchronizationManager.BindResource(DbProvider,
                                                                   txMgrStateObject.ConnectionHolder);
                }

            }
                //TODO catch specific exception
            catch (Exception e)
            {
                ConnectionUtils.DisposeConnection(con, DbProvider);
                throw new CannotCreateTransactionException("Could not create ADO.NET connection for transaction", e);
            }

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
            DbProviderTransactionObject txMgrStateObject = (DbProviderTransactionObject)transaction;
            txMgrStateObject.ConnectionHolder = null;
            ConnectionHolder conHolder = (ConnectionHolder) TransactionSynchronizationManager.UnbindResource(DbProvider);
            return conHolder;
        }


        /// <summary>
        /// Resume the resources of the current transaction.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <param name="suspendedResources">The object that holds suspended resources as returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoSuspend"/>.</param>
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
            ConnectionHolder conHolder = (ConnectionHolder)suspendedResources;
            TransactionSynchronizationManager.BindResource(DbProvider, conHolder);
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
            DbProviderTransactionObject txMgrStateObject =
                (DbProviderTransactionObject)status.Transaction;
            IDbTransaction trans = txMgrStateObject.ConnectionHolder.Transaction;
            if (status.Debug)
            {
                IDbConnection conn = txMgrStateObject.ConnectionHolder.Connection;
                log.Debug("Committing ADO.NET transaction on Connection [" + conn + ", " + conn.ConnectionString + "]");
            }
            try
            {
                trans.Commit();
            }
            catch (Exception e)
            {
                throw new TransactionSystemException("Could not commit ADO.NET transaction", e);
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
            DbProviderTransactionObject txMgrStateObject =
                (DbProviderTransactionObject)status.Transaction;
            IDbConnection conn = txMgrStateObject.ConnectionHolder.Connection;
            IDbTransaction trans = txMgrStateObject.ConnectionHolder.Transaction;
            if (status.Debug)
            {
                log.Debug("Rolling back ADO.NET transaction on Connection [" + conn + ", " + conn.ConnectionString + "]" );
            }
            try
            {
                trans.Rollback();
            }
            catch (Exception e)
            {
                throw new TransactionSystemException("Could not rollback ADO.NET transaction", e);
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
            DbProviderTransactionObject txMgrStateObject =
                        (DbProviderTransactionObject)status.Transaction;
            if (status.Debug)
            {
                IDbConnection conn = txMgrStateObject.ConnectionHolder.Connection;
                log.Debug("Setting ADO.NET transaction [" + conn + ", " + conn.ConnectionString + "] rollback-only.");
            }
            txMgrStateObject.SetRollbackOnly();

        }

        protected override void DoCleanupAfterCompletion(object transaction)
        {
            DbProviderTransactionObject txMgrStateObject =
                (DbProviderTransactionObject)transaction;
            if (txMgrStateObject.NewConnectionHolder)
            {
                TransactionSynchronizationManager.UnbindResource(DbProvider);
            }
            IDbConnection con = txMgrStateObject.ConnectionHolder.Connection;

            if (log.IsDebugEnabled)
            {
                log.Debug("Releasing ADO.NET Connection [" + con + ", " + con.ConnectionString + "] after transaction");
            }

            ConnectionUtils.DisposeConnection(con, DbProvider);
            //TODO clear out IDbTransaction object?

            txMgrStateObject.ConnectionHolder.Clear();


        }



        /// <summary>
        /// DbProvider transaction (state) object, representing a ConnectionHolder.
        /// Used as a transaction object by AdoPlatformTransactionManager
        /// </summary>
        /// <remarks>Derives from AdoTransactionObjectSupport to inherit the capability
        /// to manage Savepoints.
        /// </remarks>
        /// <seealso cref="ConnectionHolder"/>
        private class DbProviderTransactionObject : AdoTransactionObjectSupport
        {
            private bool newConnectionHolder;

            public void SetConnectionHolder(ConnectionHolder connectionHolder,
                                            bool newConnection)
            {
                ConnectionHolder = connectionHolder;
                newConnectionHolder = newConnection;
            }

            public bool NewConnectionHolder
            {
                get
                {
                    return newConnectionHolder;
                }
            }

            public bool HasTransaction
            {
                get
                {
                    return (ConnectionHolder != null && ConnectionHolder.TransactionActive);
                }
            }

            /// <summary>
            /// Sets the rollback only.
            /// </summary>
            public void SetRollbackOnly()
            {
                ConnectionHolder.RollbackOnly = true;
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
                    return ConnectionHolder.RollbackOnly;
                }
            }

        }

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// If DbProvider is null.
        /// </exception>
        public void AfterPropertiesSet()
        {
            if (dbProvider == null)
            {
                throw new ArgumentException("DbProvider is required");
            }
        }

        /// <summary>
        /// Gets the resource factory that this transaction manager operates on,
        /// For the AdoPlatformTransactionManager this is the DbProvider
        /// </summary>
        /// <value>The DbProvider.</value>
        public object ResourceFactory
        {
            get
            {
                return dbProvider;
            }
        }
    }
}
