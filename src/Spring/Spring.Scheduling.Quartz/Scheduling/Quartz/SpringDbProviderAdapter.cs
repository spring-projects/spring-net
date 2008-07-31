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
using System.Reflection;

using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;

using IDbMetadata=Spring.Data.Common.IDbMetadata;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Adapts Spring's <see cref="Data.Common.IDbProvider"/> to Quartz's
    /// <see cref="IDbProvider"/>.
    /// </summary>
    public class SpringDbProviderAdapter : IDbProvider
    {
        private readonly Data.Common.IDbProvider dbProvider;
        private DbMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringDbProviderAdapter"/> class.
        /// </summary>
        /// <param name="dbProvider">The Spring db provider.</param>
        public SpringDbProviderAdapter(Data.Common.IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
            metadata = new SpringMetadataAdapter(dbProvider.DbMetadata);
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
            get { return metadata; }
        }
    }

    public class SpringMetadataAdapter : DbMetadata
    {
        private IDbMetadata metadata;

        public SpringMetadataAdapter(IDbMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override string ProductName
        {
            get { return metadata.ProductName; }
            set { throw new NotImplementedException(); }
        }

        public override Type ConnectionType
        {
            get { return metadata.ConnectionType; }
            set { throw new NotImplementedException(); }
        }

        public override Type CommandType
        {
            get { return metadata.CommandType; }
            set { throw new NotImplementedException(); }
        }

        public override Type ParameterType
        {
            get { return metadata.ParameterType; }
            set { throw new NotImplementedException(); }
        }

        public override Type CommandBuilderType
        {
            get { return metadata.CommandBuilderType; }
            set { throw new NotImplementedException(); }
        }

        public override MethodInfo CommandBuilderDeriveParametersMethod
        {
            get { return metadata.CommandBuilderDeriveParametersMethod; }
            set { throw new NotImplementedException(); }
        }

        public override string ParameterNamePrefix
        {
            get { return metadata.ParameterNamePrefix; }
            set { throw new NotImplementedException(); }
        }

        public override Type ExceptionType
        {
            get { return metadata.ExceptionType; }
            set { throw new NotImplementedException(); }
        }

        public override bool BindByName
        {
            get { return metadata.BindByName; }
            set { throw new NotImplementedException(); }
        }

        public override Type ParameterDbType
        {
            get { return metadata.ParameterDbType; }
            set { throw new NotImplementedException(); }
        }

        public override PropertyInfo ParameterDbTypeProperty
        {
            get { return metadata.ParameterDbTypeProperty; }
            set { throw new NotImplementedException(); }
        }

        public override PropertyInfo ParameterIsNullableProperty
        {
            get { return metadata.ParameterIsNullableProperty; }
            set { throw new NotImplementedException(); }
        }

        public override Enum DbBinaryType
        {
            get { return DbType.Binary; }
        }

        public override bool UseParameterNamePrefixInParameterCollection
        {
            get { return metadata.UseParameterNamePrefixInParameterCollection; }
            set { throw new NotImplementedException(); }
        }
    }

}