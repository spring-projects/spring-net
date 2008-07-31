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

using System.Data;

using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Adapts Spring's <see cref="Data.Common.IDbProvider"/> to Quartz's
    /// <see cref="IDbProvider"/>.
    /// </summary>
    public class SpringDbProviderAdapter : IDbProvider
    {
        private readonly Data.Common.IDbProvider dbProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringDbProviderAdapter"/> class.
        /// </summary>
        /// <param name="dbProvider">The Spring db provider.</param>
        public SpringDbProviderAdapter(Data.Common.IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
        }

        public IDbCommand CreateCommand()
        {
            return dbProvider.CreateCommand();
        }

        public object CreateCommandBuilder()
        {
            return dbProvider.CreateCommandBuilder();
        }

        public IDbConnection CreateConnection()
        {
            return dbProvider.CreateConnection();
        }

        public IDbDataParameter CreateParameter()
        {
            return dbProvider.CreateParameter();
        }

        public void Shutdown()
        {
            // no-op
        }

        public string ConnectionString
        {
            get { return dbProvider.ConnectionString; }
            set { dbProvider.ConnectionString = value; }
        }

        public DbMetadata Metadata
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}