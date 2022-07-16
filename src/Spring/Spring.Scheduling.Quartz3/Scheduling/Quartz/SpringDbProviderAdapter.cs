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

using System.Data;
using System.Data.Common;
using System.Reflection;
using Quartz.Impl.AdoJobStore.Common;
using IDbMetadata = Spring.Data.Common.IDbMetadata;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Adapts Spring's <see cref="Data.Common.IDbProvider"/> to Quartz's
    /// <see cref="IDbProvider"/>.
    /// </summary>
    public class SpringDbProviderAdapter : IDbProvider
    {
        private readonly Data.Common.IDbProvider dbProvider;
        private readonly DbMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringDbProviderAdapter"/> class.
        /// </summary>
        /// <param name="dbProvider">The Spring db provider.</param>
        public SpringDbProviderAdapter(Data.Common.IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
            metadata = new SpringMetadataAdapter(dbProvider.DbMetadata);
        }

        /// <summary>
        /// Initializes the db provider implementation.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        public DbCommand CreateCommand()
        {
            return (DbCommand) dbProvider.CreateCommand();
        }

        /// <summary>
        /// Creates the command builder.
        /// </summary>
        /// <returns></returns>
        public object CreateCommandBuilder()
        {
            return dbProvider.CreateCommandBuilder();
        }

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <returns></returns>
        public DbConnection CreateConnection()
        {
            return (DbConnection) dbProvider.CreateConnection();
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <returns></returns>
        public IDbDataParameter CreateParameter()
        {
            return dbProvider.CreateParameter();
        }

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        public void Shutdown()
        {
            // no-op
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString
        {
            get => dbProvider.ConnectionString;
            set => dbProvider.ConnectionString = value;
        }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        public DbMetadata Metadata => metadata;
    }

    /// <summary>
    /// Helper class to map between Quartz and Spring DB metadata.
    /// </summary>
    public class SpringMetadataAdapter : DbMetadata
    {
        private readonly IDbMetadata metadata;
        private readonly Enum dbTypeBinary;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringMetadataAdapter"/> class.
        /// </summary>
        /// <param name="metadata">The metadata to wrap and adapt.</param>
        public SpringMetadataAdapter(IDbMetadata metadata)
        {
            this.metadata = metadata;
            // determine correct binary enum type
            Type parameterType = metadata.ParameterDbType;
            FieldInfo blobField = parameterType.GetField("Blob");
            FieldInfo imageField = parameterType.GetField("Image");
            if (blobField != null)
            {
                // uses Blob, for example Oracle
                dbTypeBinary = (Enum) blobField.GetValue(Activator.CreateInstance(parameterType));
            }
            else if (imageField != null)
            {
                // uses Image, SQL Server
                dbTypeBinary = (Enum) imageField.GetValue(Activator.CreateInstance(parameterType));
            }
            else
            {
                // use standard binary type
                dbTypeBinary = DbType.Binary;
            }
        }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>The name of the product.</value>
        public override string ProductName
        {
            get => metadata.ProductName;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>The type of the connection.</value>
        public override Type ConnectionType
        {
            get => metadata.ConnectionType;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the type of the command.
        /// </summary>
        /// <value>The type of the command.</value>
        public override Type CommandType
        {
            get => metadata.CommandType;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        /// <value>The type of the parameter.</value>
        public override Type ParameterType
        {
            get => metadata.ParameterType;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the parameter name prefix.
        /// </summary>
        /// <value>The parameter name prefix.</value>
        public override string ParameterNamePrefix
        {
            get => metadata.ParameterNamePrefix;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the type of the exception.
        /// </summary>
        /// <value>The type of the exception.</value>
        public override Type ExceptionType
        {
            get => metadata.ExceptionType;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [bind by name].
        /// </summary>
        /// <value><c>true</c> if [bind by name]; otherwise, <c>false</c>.</value>
        public override bool BindByName
        {
            get => metadata.BindByName;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the type of the parameter db.
        /// </summary>
        /// <value>The type of the parameter db.</value>
        public override Type ParameterDbType
        {
            get => metadata.ParameterDbType;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the parameter db type property.
        /// </summary>
        /// <value>The parameter db type property.</value>
        public override PropertyInfo ParameterDbTypeProperty
        {
            get => metadata.ParameterDbTypeProperty;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the parameter is nullable property.
        /// </summary>
        /// <value>The parameter is nullable property.</value>
        public override PropertyInfo ParameterIsNullableProperty
        {
            get => metadata.ParameterIsNullableProperty;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the type of the db binary.
        /// </summary>
        /// <value>The type of the db binary.</value>
        public override Enum DbBinaryType => dbTypeBinary;

        /// <summary>
        /// Gets or sets a value indicating whether [use parameter name prefix in parameter collection].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use parameter name prefix in parameter collection]; otherwise, <c>false</c>.
        /// </value>
        public override bool UseParameterNamePrefixInParameterCollection
        {
            get => metadata.UseParameterNamePrefixInParameterCollection;
            set => throw new NotSupportedException();
        }
    }
}
