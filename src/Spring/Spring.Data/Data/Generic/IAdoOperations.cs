#region License

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

#endregion

using System.Collections;
using System.Data;
using Spring.Data.Common;

namespace Spring.Data.Generic
{
    /// <summary>
    /// Interface that defines ADO.NET related database operations using generics
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public interface IAdoOperations : ICommonAdoOperations
    {

        #region General Execute Callback Methods


        /// <summary>
        /// Execute a ADO.NET operation on a command object using a generic interface based callback.
        /// </summary>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <param name="action">the callback to execute based on DbCommand</param>
        /// <returns>An object returned from callback</returns>
        T Execute<T>(ICommandCallback<T> action);

        /// <summary>
        /// Execute a ADO.NET operation on a command object using a generic interface based callback.
        /// </summary>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <param name="action">The callback to execute based on IDbCommand</param>
        /// <returns>An object returned from callback</returns>
        T Execute<T>(IDbCommandCallback<T> action);

        /// <summary>
        /// Execute a ADO.NET operation on a command object using a generic delegate callback.
        /// </summary>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <remarks>This allows for implementing arbitrary data access operations
        /// on a single command within Spring's managed ADO.NET environment.</remarks>
        /// <param name="del">The delegate called with a DbCommand object.</param>
        /// <returns>A result object returned by the action or null</returns>
        T Execute<T>(CommandDelegate<T> del);

        /// <summary>
        /// Execute a ADO.NET operation on a command object using a generic delegate callback.
        /// </summary>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <remarks>This allows for implementing arbitrary data access operations
        /// on a single command within Spring's managed ADO.NET environment.</remarks>
        /// <param name="del">The delegate called with a IDbCommand object.</param>
        /// <returns>A result object returned by the action or null</returns>
        T Execute<T>(IDbCommandDelegate<T> del);

        /// <summary>
        /// Executes ADO.NET operations on a command object, created by the provided IDbCommandCreator,
        /// using the interface based callback IDbCommandCallback.
        /// </summary>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <param name="commandCreator">The command creator.</param>
        /// <param name="action">The callback to execute based on IDbCommand</param>
        /// <returns>A result object returned by the action or null</returns>
        T Execute<T>(IDbCommandCreator commandCreator, IDbCommandCallback<T> action);

        /// <summary>
        /// Execute ADO.NET operations on a IDbDataAdapter object using an interface based callback.
        /// </summary>
        /// <remarks>This allows for implementing abritrary data access operations
        /// on a single DataAdapter within Spring's managed ADO.NET environment.
        /// </remarks>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <param name="dataAdapterCallback">The data adapter callback.</param>
        /// <returns>A result object returned by the callback or null</returns>
        T Execute<T>(IDataAdapterCallback<T> dataAdapterCallback);

        /// <summary>
        /// Execute ADO.NET operations on a IDbDataAdapter object using an delgate based callback.
        /// </summary>
        /// <remarks>This allows for implementing abritrary data access operations
        /// on a single DataAdapter within Spring's managed ADO.NET environment.
        /// </remarks>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <param name="del">The delegate called with a IDbDataAdapter object.</param>
        /// <returns>A result object returned by the callback or null</returns>
        T Execute<T>(DataAdapterDelegate<T> del);

        #endregion

        #region Queries with RowMapper<T>

        IList<T> QueryWithRowMapper<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper);

        IList<T> QueryWithRowMapper<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper,
                                       ICommandSetter commandSetter);

        IList<T> QueryWithRowMapper<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper,
                                       string parameterName, Enum dbType, int size, object parameterValue);

        IList<T> QueryWithRowMapper<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper,
                                       IDbParameters parameters);


        #endregion

        #region Queries with RowMapperDelegate<T>

        IList<T> QueryWithRowMapperDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate);

        IList<T> QueryWithRowMapperDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate,
                                               ICommandSetter commandSetter);


        IList<T> QueryWithRowMapperDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate,
                                               string parameterName, Enum dbType, int size, object parameterValue);

        IList<T> QueryWithRowMapperDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate,
                                               IDbParameters parameters);


        #endregion

        #region Queries with ResultSetExtractor<T>

        T QueryWithResultSetExtractor<T>(CommandType cmdType, string cmdText, IResultSetExtractor<T> resultSetExtractor);

        T QueryWithResultSetExtractor<T>(CommandType cmdType, string cmdText, IResultSetExtractor<T> resultSetExtractor,
                                         string name, Enum dbType, int size, object parameterValue);

        T QueryWithResultSetExtractor<T>(CommandType cmdType, string cmdText, IResultSetExtractor<T> resultSetExtractor,
                                         IDbParameters parameters);

        T QueryWithResultSetExtractor<T>(CommandType cmdType, string cmdText, IResultSetExtractor<T> resultSetExtractor,
                                         ICommandSetter commandSetter);

        T QueryWithResultSetExtractor<T>(CommandType cmdType, string cmdText, IResultSetExtractor<T> resultSetExtractor,
                                         CommandSetterDelegate commandSetterDelegate);
        #endregion


        #region Queries with ResultSetExtractorDelegate<T>

        T QueryWithResultSetExtractorDelegate<T>(CommandType cmdType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractor);

        T QueryWithResultSetExtractorDelegate<T>(CommandType cmdType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractor,
                                                 string paramenterName, Enum dbType, int size, object parameterValue);

        T QueryWithResultSetExtractorDelegate<T>(CommandType cmdType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractor,
                                                 IDbParameters parameters);

        T QueryWithResultSetExtractorDelegate<T>(CommandType cmdType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractor,
                                                 ICommandSetter commandSetter);

        T QueryWithResultSetExtractorDelegate<T>(CommandType cmdType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractor,
                                                 CommandSetterDelegate commandSetterDelegate);
        #endregion

        #region Queries for Object<T>

        T QueryForObject<T>(CommandType cmdType, string sql, IRowMapper<T> rowMapper);


        T QueryForObject<T>(CommandType cmdType, string sql, IRowMapper<T> rowMapper, ICommandSetter commandSetter);


        T QueryForObject<T>(CommandType cmdType, string sql, IRowMapper<T> rowMapper, IDbParameters parameters);


        T QueryForObject<T>(CommandType cmdType, string sql, IRowMapper<T> rowMapper,
                            string name, Enum dbType, int size, object parameterValue);



        #endregion

        #region Queries for ObjectDelegate<T>

        T QueryForObjectDelegate<T>(CommandType cmdType, string sql, RowMapperDelegate<T> rowMapper);


        T QueryForObjectDelegate<T>(CommandType cmdType, string sql, RowMapperDelegate<T> rowMapper, ICommandSetter commandSetter);


        T QueryForObjectDelegate<T>(CommandType cmdType, string sql, RowMapperDelegate<T> rowMapper, IDbParameters parameters);


        T QueryForObjectDelegate<T>(CommandType cmdType, string sql, RowMapperDelegate<T> rowMapper,
                                    string name, Enum dbType, int size, object parameterValue);



        #endregion

        #region Query With CommandCreator

        T QueryWithCommandCreator<T>(IDbCommandCreator cc, IResultSetExtractor<T> rse);

        IList<T> QueryWithCommandCreator<T>(IDbCommandCreator cc, IRowMapper<T> rowMapper);

        T QueryWithCommandCreator<T>(IDbCommandCreator cc, IResultSetExtractor<T> rse, IDictionary returnedParameters);

        IList<T> QueryWithCommandCreator<T>(IDbCommandCreator cc, IRowMapper<T> rowMapper,
                                            IDictionary returnedParameters);


        IDictionary QueryWithCommandCreator<T>(IDbCommandCreator cc, IList namedResultSetProcessors);

        IDictionary QueryWithCommandCreator<T, U>(IDbCommandCreator cc, IList namedResultSetProcessors);

        #endregion

        #region Parameter Creation Helper Methods

        IDbParameters CreateDbParameters();

        /// <summary>
        /// Note that output parameters are marked input/output after derivation....
        /// (TODO - double check....)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        IDataParameter[] DeriveParameters(string procedureName);

        IDataParameter[] DeriveParameters(string procedureName, bool includeReturnParameter);

        #endregion

        #region Behavioral Control Properties

        /// <summary>
        /// An instance of a DbProvider implementation.
        /// </summary>
        IDbProvider DbProvider { get; set; }

        /// <summary>
        /// Gets or set the System.Type to use to create an instance of IDataReaderWrapper
        /// for the purpose of having defaults values to use in case of DBNull values read
        /// from IDataReader.
        /// </summary>
        /// <value>The type of the data reader wrapper.</value>
        Type DataReaderWrapperType { get; set; }

        /// <summary>
        /// Gets or sets the command timeout for IDbCommands that this AdoTemplate executes.
        /// </summary>
        /// <remarks>Default is 0, indicating to use the database provider's default.
        /// Any timeout specified here will be overridden by the remaining
        /// transaction timeout when executing within a transaction that has a
        /// timeout specified at the transaction level.
        /// </remarks>
        /// <value>The command timeout.</value>
        int CommandTimeout { get; set; }


        #endregion

    }
}
