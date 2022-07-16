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

namespace Spring.Data
{
    public interface ICommonAdoOperations
    {
        #region ExecuteScalar

        /// <summary>
        /// Execute the query with the specified command text returning a scalar result
        /// </summary>
        /// <remarks>No parameters are used.  As with
        /// IDbCommand.ExecuteScalar, it returns the first column of the first row in the resultset
        /// returned by the query.  Extra columns or row are ignored.</remarks>
        /// <param name="cmdType">The command type</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        object ExecuteScalar(CommandType cmdType, string cmdText);


        /// <summary>
        /// Execute the query with the specified command text and parameter returning a scalar result
        /// </summary>
        /// <param name="cmdType">The command type</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the database parameter type enumerations.</param>
        /// <param name="size">The length of the parameter. 0 if not applicable to parameter type.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>The first column of the first row in the result set</returns>
        object ExecuteScalar(CommandType cmdType, string cmdText,
                             string parameterName, Enum dbType, int size, object parameterValue);

        /// <summary>
        /// Execute the query with the specified command text and parameters returning a scalar result
        /// </summary>
        /// <param name="cmdType">The command type</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="parameters">The parameter collection to map.</param>
        /// <returns>The first column of the first row in the result set</returns>
        object ExecuteScalar(CommandType cmdType, string cmdText,
                             IDbParameters parameters);

        /// <summary>
        /// Execute the query with the specified command text and parameters set via the
        /// command setter, returning a scalar result
        /// </summary>
        /// <param name="cmdType">The command type</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="commandSetter">The command setter.</param>
        /// <returns>The first column of the first row in the result set</returns>
        object ExecuteScalar(CommandType cmdType,
                             string cmdText,
                             ICommandSetter commandSetter);

        /// <summary>
        /// Execute the query with a command created via IDbCommandCreator and
        /// parameters
        /// </summary>
        /// <remarks>Output parameters can be retrieved via the returned
        /// dictionary.
        /// <para>
        /// More commonly used as a lower level support method within the framework,
        /// for example StoredProcedure/AdoScalar.
        /// </para>
        /// </remarks>
        /// <param name="commandCreator">The callback to create a IDbCommand.</param>
        /// <returns>A dictionary containing output parameters, if any</returns>
        IDictionary ExecuteScalar(IDbCommandCreator commandCreator);

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Executes a non query returning the number of rows affected.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <returns>The number of rows affected.</returns>
        int ExecuteNonQuery(CommandType cmdType, string cmdText);

        /// <summary>
        /// Executes a non query returning the number of rows affected.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the database parameter type enumerations.</param>
        /// <param name="size">The length of the parameter. 0 if not applicable to parameter type.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>The number of rows affected.</returns>
        int ExecuteNonQuery(CommandType cmdType, string cmdText,
                            string parameterName, Enum dbType, int size, object parameterValue);

        /// <summary>
        /// Executes a non query returning the number of rows affected.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="parameters">The parameter collection to map.</param>
        /// <returns>The number of rows affected.</returns>
        int ExecuteNonQuery(CommandType cmdType, string cmdText,
                            IDbParameters parameters);

        /// <summary>
        /// Executes a non query with parameters set via the
        /// command setter, returning the number of rows affected.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="commandSetter">The command setter.</param>
        /// <returns>The number of rows affected.</returns>
        int ExecuteNonQuery(CommandType cmdType, string cmdText,
                            ICommandSetter commandSetter);


        /// <summary>
        /// Executes a non query with a command created via IDbCommandCreator and
        /// parameters.
        /// </summary>
        /// <remarks>Output parameters can be retrieved via the returned
        /// dictionary.
        /// <para>
        /// More commonly used as a lower level support method within the framework,
        /// for example StoredProcedure/AdoScalar.
        /// </para>
        /// </remarks>
        /// <param name="commandCreator">The callback to create a IDbCommand.</param>
        /// <returns>The number of rows affected.</returns>
        IDictionary ExecuteNonQuery(IDbCommandCreator commandCreator);


        #endregion

        #region Query with RowCallback

        /// <summary>
        /// Execute a query given IDbCommand's type and text, reading a
        /// single result set on a per-row basis with a <see cref="IRowCallback"/>.
        /// </summary>
        /// <param name="cmdType">The type of command.</param>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="rowCallback">callback that will extract results
        /// one row at a time.
        /// </param>
        void QueryWithRowCallback(CommandType cmdType, string cmdText, IRowCallback rowCallback);


        /// <summary>
        /// Execute a query given IDbCommand's type and text and provided parameter
        /// information, reading a
        /// single result set on a per-row basis with a <see cref="IRowCallback"/>.
        /// </summary>
        /// <param name="cmdType">The type of command</param>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="rowCallback">callback that will extract results
        /// one row at a time.
        /// </param>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the database parameter type enumerations.</param>
        /// <param name="size">The length of the parameter. 0 if not applicable to parameter type.</param>
        /// <param name="parameterValue">The parameter value.</param>
        void QueryWithRowCallback(CommandType cmdType, string cmdText, IRowCallback rowCallback,
                                  string parameterName, Enum dbType, int size, object parameterValue);

        /// <summary>
        /// Execute a query given IDbCommand's type and text and provided IDbParameters,
        /// reading a single result set on a per-row basis with a <see cref="IRowCallback"/>.
        /// </summary>
        /// <param name="cmdType">Type of the command.</param>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="rowCallback">callback that will extract results
        /// one row at a time.</param>
        /// <param name="parameters">The parameter collection to map.</param>
        void QueryWithRowCallback(CommandType cmdType, string cmdText, IRowCallback rowCallback,
                                  IDbParameters parameters);

        /// <summary>
        /// Execute a query given IDbCommand's type and text by
        /// passing the created IDbCommand to a ICommandSetter implementation
        /// that knows how to bind values to the IDbCommand, reading a
        /// single result set on a per-row basis with a <see cref="IRowCallback"/>.
        /// </summary>
        /// <param name="cmdType">The type of command</param>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="rowCallback">callback that will extract results
        /// one row at a time.
        /// </param>
        /// <param name="commandSetter">The command setter.</param>
        void QueryWithRowCallback(CommandType cmdType, String cmdText, IRowCallback rowCallback,
                                  ICommandSetter commandSetter);


        #endregion

        #region Query with RowCallback Delegate

        void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate);

        void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate, ICommandSetter commandSetter);


        void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate,
                        string name, Enum dbType, int size, object parameterValue);


        void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate, IDbParameters parameters);

        #endregion

        #region Query with CommandCreator

        void QueryWithCommandCreator(IDbCommandCreator cc, IRowCallback rowCallback);

        void QueryWithCommandCreator(IDbCommandCreator cc, IRowCallback rowCallback, IDictionary returnedParameters);

        #endregion

    }
}
