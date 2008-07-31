/*
 * Copyright 2002-2007 the original author or authors.
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

using System;
using System.Data;
using System.Data.SqlClient;

using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Spi;
using Quartz.Util;

using Spring.Data.Support;

namespace Spring.Scheduling.Quartz
{

    /// <summary>
    /// Subclass of Quartz's JobStoreCMT class that delegates to a Spring-managed
    /// DataSource instead of using a Quartz-managed connection pool. This JobStore
    /// will be used if SchedulerFactoryBean's "dbProvider" property is set.
    ///</summary>
    /// <remarks>
    /// <p>Supports both transactional and non-transactional DataSource access.
    /// With a non-XA DataSource and local Spring transactions, a single DataSource
    /// argument is sufficient. In case of an XA DataSource and global JTA transactions,
    /// SchedulerFactoryBean's "nonTransactionalDataSource" property should be set,
    /// passing in a non-XA DataSource that will not participate in global transactions.</p>
    ///
    /// <p>Operations performed by this JobStore will properly participate in any
    /// kind of Spring-managed transaction, as it uses Spring's DataSourceUtils
    /// connection handling methods that are aware of a current transaction.</p>
    ///
    /// <p>Note that all Quartz Scheduler operations that affect the persistent
    /// job store should usually be performed within active transactions,
    /// as they assume to get proper locks etc.</p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Marko Lahma (.NET)</author>
    /// <seealso cref="SchedulerFactoryObject#DbProvider" />
    /// <seealso cref="ConnectionUtils.GetConnection" />
    /// <seealso cref="ConnectionUtils.DisposeConnection" />
    public class LocalDataSourceJobStore : JobStoreCMT
    {
        /**
         * Name used for the transactional ConnectionProvider for Quartz.
         * This provider will delegate to the local Spring-managed DataSource.
         * @see org.quartz.utils.DBConnectionManager#addConnectionProvider
         * @see SchedulerFactoryBean#setDataSource
         */
        public const string TX_DATA_SOURCE_PREFIX = "springTxDataSource.";

        private Data.Common.IDbProvider dbProvider;

        /// <summary>
        /// Gets or sets the name of the instance.
        /// </summary>
        /// <value>The name of the instance.</value>
        public override string InstanceName
        {
            get { return base.InstanceName; }
            set
            {
                // use to catch property setting
                base.InstanceName = value;
                DataSource = TX_DATA_SOURCE_PREFIX + InstanceName;
                // Register transactional ConnectionProvider for Quartz.
                // Absolutely needs thread-bound DataSource to initialize.
                dbProvider = SchedulerFactoryObject.ConfigTimeDbProvider;
                if (dbProvider == null)
                {
                    throw new SchedulerConfigException(
                        "No db provider found for configuration - " +
                        "'DbProvider' property must be set on SchedulerFactoryObject");
                } 
                DBConnectionManager.Instance.AddConnectionProvider(
                    TX_DATA_SOURCE_PREFIX + InstanceName, new SpringDbProviderAdapter(dbProvider));

            }
        }

        protected override ConnectionAndTransactionHolder GetNonManagedTXConnection()
        {
            ConnectionTxPair pair = ConnectionUtils.GetConnectionTxPair(dbProvider);
            return new ConnectionAndTransactionHolder(pair.Connection, pair.Transaction);
        }

        protected override void CloseConnection(ConnectionAndTransactionHolder connectionAndTransactionHolder)
        {
            // Will work for transactional and non-transactional connections.
            ConnectionUtils.DisposeConnection(connectionAndTransactionHolder.Connection, dbProvider);
        }

    }

    ///<summary>
    ///</summary>
    public class JobStoreCMT : JobStoreSupport {

    ///<summary>
    ///</summary>
    ///<param name="loadHelper"></param>
    ///<param name="signaler"></param>
    public override void Initialize(ITypeLoadHelper loadHelper, ISchedulerSignaler signaler)
    {
        if (LockHandler == null) {
            // If the user hasn't specified an explicit lock handler, 
            // then we *must* use DB locks with CMT...
            UseDBLocks = true;
        }

        base.Initialize(loadHelper, signaler);

        Log.Info("JobStoreCMT initialized.");
    }
    
    ///<summary>
    ///</summary>
    public override void Shutdown() {

        base.Shutdown();
        
        try {
            DBConnectionManager.Instance.Shutdown(DataSource);
        } catch (SqlException sqle) {
            Log.Warn("Database connection shutdown unsuccessful.", sqle);
        }
    }

    protected override ConnectionAndTransactionHolder GetNonManagedTXConnection()
       {
        IDbConnection conn;
        try {
            conn = DBConnectionManager.Instance.GetConnection(DataSource);
        } catch (SqlException sqle) {
            throw new JobPersistenceException(
                "Failed to obtain DB connection from data source '"
                        + DataSource + "': "
                        + sqle, sqle);
        } catch (Exception e) {
            throw new JobPersistenceException(
                "Failed to obtain DB connection from data source '"
                        + DataSource + "': "
                        + e, e,
                SchedulerException.ErrorPersistenceCriticalFailure);
        }

        if (conn == null) { 
            throw new JobPersistenceException(
                "Could not get connection from DataSource '"
                        + DataSource + "'"); 
        }

        // Set any connection connection attributes we are to override.
        return new ConnectionAndTransactionHolder(conn, null);
    }
    
    /**
     * Execute the given callback having optionally aquired the given lock.  
     * Because CMT assumes that the connection is already part of a managed
     * transaction, it does not attempt to commit or rollback the 
     * enclosing transaction.
     * 
     * @param lockName The name of the lock to aquire, for example 
     * "TRIGGER_ACCESS".  If null, then no lock is aquired, but the
     * txCallback is still executed in a transaction.
     * 
     * @see JobStoreSupport#executeInNonManagedTXLock(String, TransactionCallback)
     * @see JobStoreTX#executeInLock(String, TransactionCallback)
     * @see JobStoreSupport#getNonManagedTXConnection()
     * @see JobStoreSupport#getConnection()
     */
    protected override object ExecuteInLock(
            string lockName, 
            ITransactionCallback txCallback){
        bool transOwner = false;
        ConnectionAndTransactionHolder conn = null;
        try {
            if (lockName != null) {
                // If we aren't using db locks, then delay getting DB connection 
                // until after aquiring the lock since it isn't needed.
                if (LockHandler.RequiresConnection) {
                    conn = GetNonManagedTXConnection();
                }
                
                transOwner = LockHandler.ObtainLock(DbMetadata, conn, lockName);
            }

            if (conn == null) {
                conn = GetNonManagedTXConnection();
            }

            return txCallback.Execute(conn);
        } finally {
            try {
                ReleaseLock(conn, LockTriggerAccess, transOwner);
            } finally {
                CleanupConnection(conn);
            }
        }
    }
}

}
