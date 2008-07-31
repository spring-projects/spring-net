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
    public class LocalDataSourceJobStore : JobStoreSupport
    {

        /**
         * Name used for the transactional ConnectionProvider for Quartz.
         * This provider will delegate to the local Spring-managed DataSource.
         * @see org.quartz.utils.DBConnectionManager#addConnectionProvider
         * @see SchedulerFactoryBean#setDataSource
         */
        public const string TX_DATA_SOURCE_PREFIX = "springTxDataSource.";

        private Data.Common.IDbProvider dbProvider;

        public Data.Common.IDbProvider DbProvider
        {
            get { return dbProvider; }
            set { dbProvider = value; }
        }


        public override void Initialize(ITypeLoadHelper loadHelper, ISchedulerSignaler signaler)
        {

            // Absolutely needs thread-bound DataSource to initialize.
            dbProvider = SchedulerFactoryObject.ConfigTimeDbProvider;
            if (dbProvider == null)
            {
                throw new SchedulerConfigException(
                    "No db provider found for configuration - " +
                    "'DbProvider' property must be set on SchedulerFactoryObject");
            }

            // Configure transactional connection settings for Quartz.
            DataSource = TX_DATA_SOURCE_PREFIX + InstanceName;
            DontSetAutoCommitFalse = true;

            // Register transactional ConnectionProvider for Quartz.
            DBConnectionManager.Instance.AddConnectionProvider(
                TX_DATA_SOURCE_PREFIX + InstanceName, new SpringDbProviderAdapter(dbProvider));

            base.Initialize(loadHelper, signaler);
        }

        protected override ConnectionAndTransactionHolder GetNonManagedTXConnection()
        {
            return GetConnection();
        }

        protected override object ExecuteInLock(string lockName, ITransactionCallback txCallback)
        {
            throw new System.NotImplementedException();
        }


        protected override void CloseConnection(ConnectionAndTransactionHolder connectionAndTransactionHolder)
        {
            // Will work for transactional and non-transactional connections.
            ConnectionUtils.DisposeConnection(connectionAndTransactionHolder.Connection, dbProvider);
        }

    }

}
