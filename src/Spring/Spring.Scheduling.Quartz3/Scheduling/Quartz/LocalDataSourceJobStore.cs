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

using System.Data.Common;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Util;
using Spring.Data.Support;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Subclass of Quartz's JobStoreCMT class that delegates to a Spring-managed
    /// DataSource instead of using a Quartz-managed connection pool. This JobStore
    /// will be used if SchedulerFactoryObject's "dbProvider" property is set.
    ///</summary>
    /// <remarks>
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
    /// <seealso cref="ConnectionUtils.GetConnection" />
    /// <seealso cref="ConnectionUtils.DisposeConnection" />
    public class LocalDataSourceJobStore : JobStoreCMT
    {
        /// <summary>
        /// Name used for the transactional ConnectionProvider for Quartz.
        /// This provider will delegate to the local Spring-managed DataSource.
        /// <seealso cref="DBConnectionManager.AddConnectionProvider" />
        /// <seealso cref="SchedulerFactoryObject.DbProvider" />
        /// </summary>
        public const string TxDataSourcePrefix = "springTxDataSource.";

        private Data.Common.IDbProvider dbProvider;

        /// <summary>
        /// Gets or sets the name of the instance.
        /// </summary>
        /// <value>The name of the instance.</value>
        public override string InstanceName
        {
            get => base.InstanceName;
            set
            {
                // use to catch property setting
                base.InstanceName = value;
                DataSource = TxDataSourcePrefix + InstanceName;
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
                    TxDataSourcePrefix + InstanceName, new SpringDbProviderAdapter(dbProvider));
            }
        }

        /// <summary>
        /// Gets the non managed TX connection.
        /// </summary>
        /// <returns></returns>
        protected override ConnectionAndTransactionHolder GetNonManagedTXConnection()
        {
            ConnectionTxPair pair = ConnectionUtils.DoGetConnection(dbProvider);
            return new ConnectionAndTransactionHolder((DbConnection) pair.Connection, (DbTransaction) pair.Transaction);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param name="connectionAndTransactionHolder">The connection and transaction holder.</param>
        protected override void CloseConnection(ConnectionAndTransactionHolder connectionAndTransactionHolder)
        {
            // Will work for transactional and non-transactional connections.
            ConnectionUtils.DisposeConnection(connectionAndTransactionHolder.Connection, dbProvider);
        }
    }
}