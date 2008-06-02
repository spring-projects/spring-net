#region Licence

/*
 * Copyright © 2002-2006 the original author or authors.
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

using System;
using System.Data;
using Spring.Expressions;
using Spring.Reflection.Dynamic;
using Spring.Util;

namespace Spring.Data.Common
{
    /// <summary>
    /// Implemenation of of DbProvider that uses metadata to create provider specific ADO.NET objects.
    /// </summary>
    public class DbProvider : IDbProvider
    {
        private string connectionString;
        private IDbMetadata dbMetadata;
        private IDynamicConstructor newCommand;
        private IDynamicConstructor newConnection;
        private IDynamicConstructor newCommandBuilder;
        private IDynamicConstructor newDataAdapter;
        private IDynamicConstructor newParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbProvider"/> class.
        /// </summary>
        /// <param name="dbMetadata">The db metadata.</param>
        public DbProvider(IDbMetadata dbMetadata)
        {
            this.dbMetadata = dbMetadata;
            newCommand = DynamicConstructor.Create(dbMetadata.CommandType.GetConstructor(Type.EmptyTypes));
            newConnection = DynamicConstructor.Create(dbMetadata.ConnectionType.GetConstructor(Type.EmptyTypes));
            newCommandBuilder = DynamicConstructor.Create(dbMetadata.CommandBuilderType.GetConstructor(Type.EmptyTypes));
            newDataAdapter = DynamicConstructor.Create(dbMetadata.DataAdapterType.GetConstructor(Type.EmptyTypes));
            newParameter = DynamicConstructor.Create(dbMetadata.ParameterType.GetConstructor(Type.EmptyTypes));
        }

        /// <summary>
        /// Returns a new command object for executing SQL statments/Stored Procedures
        /// against the database.
        /// </summary>
        /// <returns>An new <see cref="IDbCommand"/></returns>
        public IDbCommand CreateCommand()
        {
            return newCommand.Invoke(ObjectUtils.EmptyObjects) as IDbCommand;          
        }

        /// <summary>
        /// Returns a new instance of the providers CommandBuilder class.
        /// </summary>
        /// <returns>A new Command Builder</returns>
        /// <remarks>In .NET 1.1 there was no common base class or interface
        /// for command builders, hence the return signature is object to
        /// be portable (but more loosely typed) across .NET 1.1/2.0</remarks>
        public object CreateCommandBuilder()
        {
            return newCommandBuilder.Invoke(ObjectUtils.EmptyObjects);
        }

        /// <summary>
        /// Returns a new connection object to communicate with the database.
        /// </summary>
        /// <returns>A new <see cref="IDbConnection"/></returns>
        public IDbConnection CreateConnection()
        {
            IDbConnection conn = newConnection.Invoke(ObjectUtils.EmptyObjects) as IDbConnection;
            if (conn != null)
            {
                conn.ConnectionString = ConnectionString;
            }
            return conn;
        }

        /// <summary>
        /// Returns a new adapter objects for use with offline DataSets.
        /// </summary>
        /// <returns>A new <see cref="IDbDataAdapter"/></returns>
        public IDbDataAdapter CreateDataAdapter()
        {
            return newDataAdapter.Invoke(ObjectUtils.EmptyObjects) as IDbDataAdapter;
        }

        /// <summary>
        /// Returns a new parameter object for binding values to parameter
        /// placeholders in SQL statements or Stored Procedure variables.
        /// </summary>
        /// <returns>A new <see cref="IDbDataParameter"/></returns>
        public IDbDataParameter CreateParameter()
        {
            return newParameter.Invoke(ObjectUtils.EmptyObjects) as IDbDataParameter;
        }

        /// <summary>
        /// Creates the name of the parameter in the format appropriate to use inside IDbCommand.CommandText.
        /// </summary>
        /// <param name="name">The unformatted name of the parameter.</param>
        /// <returns>
        /// The parameter name formatted foran IDbCommand.CommandText.
        /// </returns>
        /// <remarks>In most cases this adds the parameter prefix to the name passed into this method.</remarks>
        public string CreateParameterName(string name)
        {
            if (dbMetadata.BindByName)
            {
                if (DbMetadata.UseParameterPrefixInSql)
                {
                    return DbMetadata.ParameterNamePrefix + name;
                }
                else
                {
                    return name;
                }
            }
            else
            {
                return DbMetadata.ParameterNamePrefix;
            }
        }

        /// <summary>
        /// Creates the name ofthe parameter in the format appropriate for an IDataParameter, i.e. to be
        /// part of a IDataParameterCollection.
        /// </summary>
        /// <param name="name">The unformatted name of the parameter.</param>
        /// <returns>
        /// The parameter name formatted for an IDataParameter
        /// </returns>
        public string CreateParameterNameForCollection(string name)
        {
            if (dbMetadata.BindByName)
            {
                if (DbMetadata.UseParameterNamePrefixInParameterCollection)
                {
                    return DbMetadata.ParameterNamePrefix + name;
                }
                else
                {
                    return name;
                }
            }
            else
            {
                return DbMetadata.ParameterNamePrefix;
            }
        }

        /// <summary>
        /// Return metadata information about the database provider
        /// </summary>
        /// <value></value>
        public IDbMetadata DbMetadata
        {
            get { return dbMetadata; }
        }

        /// <summary>
        /// Connection string used to create connections.
        /// </summary>
        /// <value></value>
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        /// <summary>
        /// Extracts the provider specific error code as a string.
        /// </summary>
        /// <param name="e">The data access exception.</param>
        /// <returns>The provider specific error code</returns>
        public string ExtractError(Exception e)
        {
    
            if (!StringUtils.IsNullOrEmpty(dbMetadata.ErrorCodeExceptionExpression))
            {
                return ExpressionEvaluator.GetValue(e, dbMetadata.ErrorCodeExceptionExpression).ToString();
            }  
            else
            {
                return "Could not extract error code exception type." + e.GetType(); 
            }

        }



        /// <summary>
        /// Determines whether the provided exception is in fact related
        /// to database access.  This can be provider dependent in .NET 1.1 since
        /// there isn't a common base class for ADO.NET exceptions.
        /// </summary>
        /// <param name="e">The exception thrown when performing data access
        /// operations.</param>
        /// <returns>
        /// 	<c>true</c> if is a valid data access exception for the specified
        /// exception; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDataAccessException(Exception e)
        {

#if NET_2_0
            if (e is System.Data.Common.DbException)
            {
                return true;
            }
            else
            {
                return false;
            }
#else       
            return IsDataAccessExceptionBCL11(e);
#endif
        }

        /// <summary>
        /// Determines whether is data access exception in .NET 1.1 for the specified exception.
        /// </summary>
        /// <param name="e">The candidate exception.</param>
        /// <returns>
        /// 	<c>true</c> if is data access exception in .NET 1.1 for the specified exception; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDataAccessExceptionBCL11(Exception e)
        {
            if (e == null)
            {
                return false;
            }
            return e.GetType().IsAssignableFrom(DbMetadata.ExceptionType);
        }
    }
}
