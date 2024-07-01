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
using System.Data.Common;
using Common.Logging;
using Spring.Dao;
using Spring.Dao.Support.Generic;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Data.Support;
using Spring.Util;

namespace Spring.Data.Generic
{
    /// <summary>
    /// This is the central class in the Spring.Data.Generic namespace.
    /// It simplifies the use of ADO.NET and helps to avoid commons errors.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class AdoTemplate : AdoAccessor, IAdoOperations
    {

        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(AdoTemplate));

        #endregion

        #region Fields

        private Core.AdoTemplate classicAdoTemplate;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoTemplate"/> class.
        /// </summary>
        public AdoTemplate()
            : base()
        {
            classicAdoTemplate = new Core.AdoTemplate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoTemplate"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public AdoTemplate(IDbProvider provider)
        {
            classicAdoTemplate = new Core.AdoTemplate(provider);
            AfterPropertiesSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoTemplate"/> class.
        /// </summary>
        /// <param name="provider">The database provider.</param>
        /// <param name="lazyInit">if set to <c>false</c>
        /// lazily initialize the ErrorCodeExceptionTranslator.</param>
        public AdoTemplate(IDbProvider provider, bool lazyInit)
        {
            classicAdoTemplate = new Core.AdoTemplate(provider, lazyInit);
            AfterPropertiesSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoTemplate"/> class.
        /// </summary>
        /// <remarks>Useful if you have a subclass of Spring.Data.Core.AdoTemplate.</remarks>
        /// <param name="classicAdoTemplate">The classic ADO template.</param>
        public AdoTemplate(Core.AdoTemplate classicAdoTemplate)
        {
            this.classicAdoTemplate = classicAdoTemplate;
            AfterPropertiesSet();
        }
        #endregion

        #region Properties

        /// <summary>
        /// An instance of a DbProvider implementation.
        /// </summary>
        public override IDbProvider DbProvider
        {
            get { return classicAdoTemplate.DbProvider; }
            set { classicAdoTemplate.DbProvider = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to lazily initialize the
        /// IAdoExceptionTranslator for this accessor, on first encounter of a
        /// exception from the data provider.  Default is "true"; can be switched to
        /// "false" for initialization on startup.
        /// </summary>
        /// <value><c>true</c> if to lazy initialize the IAdoExceptionTranslator;
        /// otherwise, <c>false</c>.</value>
        public override bool LazyInit
        {
            get { return classicAdoTemplate.LazyInit; }
            set { classicAdoTemplate.LazyInit = value; }
        }

        /// <summary>
        /// Gets or sets the exception translator. If no custom translator is provided, a default
        /// <see cref="ErrorCodeExceptionTranslator"/> is used.
        /// </summary>
        /// <value>The exception translator.</value>
        public override IAdoExceptionTranslator ExceptionTranslator
        {
            get { return classicAdoTemplate.ExceptionTranslator; }
            set { classicAdoTemplate.ExceptionTranslator = value; }
        }


        /// <summary>
        /// Gets or set the System.Type to use to create an instance of IDataReaderWrapper
        /// for the purpose of having defaults values to use in case of DBNull values read
        /// from IDataReader.
        /// </summary>
        /// <value>The type of the data reader wrapper.</value>
        public override Type DataReaderWrapperType
        {
            get { return classicAdoTemplate.DataReaderWrapperType; }
            set { classicAdoTemplate.DataReaderWrapperType = value; }
        }

        /// <summary>
        /// Gets the 'Classic' AdoTemplate in Spring.Data
        /// </summary>
        /// <remarks>
        /// Useful to access methods that return non-generic collections.
        /// </remarks>
        /// <value>The 'Classic; AdoTemplate in Spring.Data.</value>
        public Core.AdoTemplate ClassicAdoTemplate
        {
            get { return classicAdoTemplate; }
        }

        /// <summary>
        /// Gets or sets the command timeout for IDbCommands that this AdoTemplate executes.  It also
        /// sets the value of the contained 'ClassicAdoTemplate'.
        /// </summary>
        /// <value>The command timeout.</value>
        /// <remarks>Default is 0, indicating to use the database provider's default.
        /// Any timeout specified here will be overridden by the remaining
        /// transaction timeout when executing within a transaction that has a
        /// timeout specified at the transaction level.
        /// </remarks>
        public override int CommandTimeout
        {
            get { return base.CommandTimeout; }
            set
            {
                base.CommandTimeout = value;
                classicAdoTemplate.CommandTimeout = value;
            }
        }

        #endregion

        #region General Execute Callback methods

        /// <summary>
        /// Execute a ADO.NET operation on a command object using a interface based callback.
        /// </summary>
        /// <param name="action">the callback to execute</param>
        /// <returns>object returned from callback</returns>
        public virtual T Execute<T>(ICommandCallback<T> action)
        {
            ConnectionTxPair connectionTxPairToUse = GetConnectionTxPair(DbProvider);

            IDbCommand command = null;
            try
            {
                command = DbProvider.CreateCommand();
                command.Connection = connectionTxPairToUse.Connection;
                command.Transaction = connectionTxPairToUse.Transaction;
                //TODO collect warnings...
                //RegisterEventHandlers(command.Connection);
                ApplyCommandSettings(command);
                //TODO explicit check to cast to base class.
                DbCommand commandToUse = command as DbCommand;
                if (commandToUse != null)
                {
                    T result = action.DoInCommand((DbCommand)command);
                    //SqlWarnings sqlWarnings = GetSqlWarnings()
                    //ThrowExceptionOnWarningIfNotIgnoringWarnings(sqlWarnings);
                    return result;
                }
                else
                {
                    throw new InvalidDataAccessApiUsageException(
                        "Providers implementation of IDbCommand does not inherit from System.Data.Common.DbCommand.  Use the alternative overloaded Execute method that specifies IDbCommandCallback as a parameter.");
                }

            }
            catch (Exception e)
            {
                DisposeCommand(command);
                command = null;
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
                connectionTxPairToUse.Connection = null;
                if (DbProvider.IsDataAccessException(e))
                {
                    throw ExceptionTranslator.Translate("CommandCallback", GetCommandText(action), e);
                }
                else
                {
                    throw;
                }


            }
            finally
            {
                DisposeCommand(command);
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
            }
        }

        /// <summary>
        /// Execute a ADO.NET operation on a command object using a delegate callback.
        /// </summary>
        /// <remarks>This allows for implementing arbitrary data access operations
        /// on a single command within Spring's managed ADO.NET environment.</remarks>
        /// <param name="del">The delegate called with a command object.</param>
        /// <returns>A result object returned by the action or null</returns>
        public virtual T Execute<T>(CommandDelegate<T> del)
        {
            ConnectionTxPair connectionTxPairToUse = GetConnectionTxPair(DbProvider);

            IDbCommand command = null;
            try
            {
                command = DbProvider.CreateCommand();
                command.Connection = connectionTxPairToUse.Connection;
                command.Transaction = connectionTxPairToUse.Transaction;
                //TODO collect warnings...
                //RegisterEventHandlers(command.Connection);
                ApplyCommandSettings(command);
                DbCommand commandToUse = command as DbCommand;
                if (commandToUse != null)
                {
                    T result = del((DbCommand) command);
                    //SqlWarnings sqlWarnings = GetSqlWarnings()
                    //ThrowExceptionOnWarningIfNotIgnoringWarnings(sqlWarnings);
                    return result;
                }
                else
                {
                    throw new InvalidDataAccessApiUsageException(
                        "Providers implementation of IDbCommand does not inherit from System.Data.Common.DbCommand.  Use the alternative overloaded Execute method that specifies IDbCommandDelegate as a parameter.");
                }
            }
            catch (Exception e)
            {
                string commandText = command.CommandText;
                DisposeCommand(command);
                command = null;
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
                connectionTxPairToUse.Connection = null;
                if (DbProvider.IsDataAccessException(e))
                {
                    throw ExceptionTranslator.Translate("CommandCallback", commandText, e);
                }
                else
                {
                    throw;
                }


            }
            finally
            {
                DisposeCommand(command);
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
            }
        }


        /// <summary>
        /// Execute a ADO.NET operation on a command object using a interface based callback.
        /// </summary>
        /// <param name="action">the callback to execute</param>
        /// <returns>object returned from callback</returns>
        public virtual T Execute<T>(IDbCommandCallback<T> action)
        {
            ConnectionTxPair connectionTxPairToUse = GetConnectionTxPair(DbProvider);

            IDbCommand command = null;
            try
            {
                command = DbProvider.CreateCommand();
                command.Connection = connectionTxPairToUse.Connection;
                command.Transaction = connectionTxPairToUse.Transaction;
                //TODO collect warnings...
                //RegisterEventHandlers(command.Connection);
                ApplyCommandSettings(command);
                T result = action.DoInCommand(command);
                //SqlWarnings sqlWarnings = GetSqlWarnings()
                //ThrowExceptionOnWarningIfNotIgnoringWarnings(sqlWarnings);
                return result;
            }
            catch (Exception e)
            {
                DisposeCommand(command);
                command = null;
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
                connectionTxPairToUse.Connection = null;
                if (DbProvider.IsDataAccessException(e))
                {
                    throw ExceptionTranslator.Translate("CommandCallback", GetCommandText(action), e);
                }
                else
                {
                    throw;
                }


            }
            finally
            {
                DisposeCommand(command);
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
            }
        }

        /// <summary>
        /// Execute a ADO.NET operation on a command object using a delegate callback.
        /// </summary>
        /// <remarks>This allows for implementing arbitrary data access operations
        /// on a single command within Spring's managed ADO.NET environment.</remarks>
        /// <param name="del">The delegate called with a command object.</param>
        /// <returns>A result object returned by the action or null</returns>
        public virtual T Execute<T>(IDbCommandDelegate<T> del)
        {
            ConnectionTxPair connectionTxPairToUse = GetConnectionTxPair(DbProvider);

            IDbCommand command = null;
            try
            {
                command = DbProvider.CreateCommand();
                command.Connection = connectionTxPairToUse.Connection;
                command.Transaction = connectionTxPairToUse.Transaction;
                //TODO collect warnings...
                //RegisterEventHandlers(command.Connection);
                ApplyCommandSettings(command);
                T result = del(command);
                //SqlWarnings sqlWarnings = GetSqlWarnings()
                //ThrowExceptionOnWarningIfNotIgnoringWarnings(sqlWarnings);
                return result;
            }
            catch (Exception e)
            {
                string commandText = command.CommandText;
                DisposeCommand(command);
                command = null;
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
                connectionTxPairToUse.Connection = null;
                if (DbProvider.IsDataAccessException(e))
                {
                    throw ExceptionTranslator.Translate("CommandCallback", commandText, e);
                }
                else
                {
                    throw;
                }


            }
            finally
            {
                DisposeCommand(command);
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
            }
        }


        /// <summary>
        /// Executes ADO.NET operations on a command object, created by the provided IDbCommandCreator,
        /// using the interface based callback IDbCommandCallback.
        /// </summary>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <param name="commandCreator">The command creator.</param>
        /// <param name="action">The callback to execute based on IDbCommand</param>
        /// <returns>
        /// A result object returned by the action or null
        /// </returns>
        public virtual T Execute<T>(IDbCommandCreator commandCreator, IDbCommandCallback<T> action)
        {
            AssertUtils.ArgumentNotNull(commandCreator, "commandCreator", "IDbCommandCreator must not be null");
            AssertUtils.ArgumentNotNull(action, "action", "Callback object must not be null");
            ConnectionTxPair connectionTxPairToUse = GetConnectionTxPair(DbProvider);

            IDbCommand command = null;
            try
            {
                command = commandCreator.CreateDbCommand(DbProvider);
                command.Connection = connectionTxPairToUse.Connection;
                command.Transaction = connectionTxPairToUse.Transaction;
                //TODO collect warnings...
                //RegisterEventHandlers(command.Connection);
                ApplyCommandSettings(command);
                T result = action.DoInCommand(command);
                //SqlWarnings sqlWarnings = GetSqlWarnings()
                //ThrowExceptionOnWarningIfNotIgnoringWarnings(sqlWarnings);
                return result;
            }
            catch (Exception e)
            {
                DisposeCommand(command);
                command = null;
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
                connectionTxPairToUse.Connection = null;
                if (DbProvider.IsDataAccessException(e))
                {
                    throw ExceptionTranslator.Translate("CommandCallback", GetCommandText(action), e);
                }
                else
                {
                    throw;
                }


            }
            finally
            {
                DisposeCommand(command);
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
            }

        }

        /// <summary>
        /// Execute ADO.NET operations on a IDbDataAdapter object using an interface based callback.
        /// </summary>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <param name="dataAdapterCallback">The data adapter callback.</param>
        /// <returns>
        /// A result object returned by the callback or null
        /// </returns>
        /// <remarks>This allows for implementing abritrary data access operations
        /// on a single DataAdapter within Spring's managed ADO.NET environment.
        /// </remarks>
        public virtual T Execute<T>(IDataAdapterCallback<T> dataAdapterCallback)
        {
            ConnectionTxPair connectionTxPairToUse = GetConnectionTxPair(DbProvider);
            IDbDataAdapter dataAdapter = null;
            try
            {
                dataAdapter = DbProvider.CreateDataAdapter();
                //TODO row updated event handling...
                dataAdapter.SelectCommand = DbProvider.CreateCommand();
                dataAdapter.SelectCommand.Connection = connectionTxPairToUse.Connection;
                //TODO register for warnings on connection.
                dataAdapter.SelectCommand.Transaction = connectionTxPairToUse.Transaction;
                ApplyCommandSettings(dataAdapter.SelectCommand);
                T result = dataAdapterCallback.DoInDataAdapter(dataAdapter);
                return result;
            }
            catch (Exception)
            {
                DisposeDataAdapterCommands(dataAdapter);
                //TODO set dataAdapter command's = null; ?
                //TODO exception translation? different hierarchy for data set operations.
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
                connectionTxPairToUse.Connection = null;
                throw;
            }
            finally
            {
                DisposeDataAdapterCommands(dataAdapter);
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
            }


        }

        /// <summary>
        /// Execute ADO.NET operations on a IDbDataAdapter object using an delgate based callback.
        /// </summary>
        /// <remarks>This allows for implementing abritrary data access operations
        /// on a single DataAdapter within Spring's managed ADO.NET environment.
        /// </remarks>
        /// <typeparam name="T">The type of object returned from the callback.</typeparam>
        /// <param name="dataAdapterCallback">The delegate called with a IDbDataAdapter object.</param>
        /// <returns>A result object returned by the callback or null</returns>
        public virtual T Execute<T>(DataAdapterDelegate<T> dataAdapterCallback)
        {
            ConnectionTxPair connectionTxPairToUse = GetConnectionTxPair(DbProvider);
            IDbDataAdapter dataAdapter = null;
            try
            {
                dataAdapter = DbProvider.CreateDataAdapter();
                //TODO row updated event handling...
                dataAdapter.SelectCommand = DbProvider.CreateCommand();
                dataAdapter.SelectCommand.Connection = connectionTxPairToUse.Connection;
                //TODO register for warnings on connection.
                dataAdapter.SelectCommand.Transaction = connectionTxPairToUse.Transaction;
                ApplyCommandSettings(dataAdapter.SelectCommand);
                T result = dataAdapterCallback(dataAdapter);
                return result;

            }
            catch (Exception)
            {
                DisposeDataAdapterCommands(dataAdapter);
                //TODO set dataAdapter command's = null; ?
                //TODO exception translation? different hierarchy for data set operations.
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
                connectionTxPairToUse.Connection = null;
                throw;
            }
            finally
            {
                DisposeDataAdapterCommands(dataAdapter);
                DisposeConnection(connectionTxPairToUse.Connection, DbProvider);
            }
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Executes a non query returning the number of rows affected.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteNonQuery(CommandType cmdType, string cmdText)
        {
            return classicAdoTemplate.ExecuteNonQuery(cmdType, cmdText);
        }


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
        public virtual int ExecuteNonQuery(CommandType cmdType, string cmdText,
                                   string parameterName, Enum dbType, int size, object parameterValue)
        {
            return classicAdoTemplate.ExecuteNonQuery(cmdType, cmdText, parameterName, dbType, size, parameterValue);


        }

        /// <summary>
        /// Executes a non query returning the number of rows affected.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="parameters">The parameter collection to map.</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteNonQuery(CommandType cmdType, string cmdText,
                                           IDbParameters parameters)
        {
            return classicAdoTemplate.ExecuteNonQuery(cmdType, cmdText, parameters);

        }

        /// <summary>
        /// Executes a non query with parameters set via the
        /// command setter, returning the number of rows affected.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="commandSetter">The command setter.</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteNonQuery(CommandType cmdType, string cmdText,
                                           ICommandSetter commandSetter)
        {
            return classicAdoTemplate.ExecuteNonQuery(cmdType, cmdText, commandSetter);
        }

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
        public virtual IDictionary ExecuteNonQuery(IDbCommandCreator commandCreator)
        {
            return classicAdoTemplate.ExecuteNonQuery(commandCreator);
        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// Execute the query with the specified command text.
        /// </summary>
        /// <remarks>No parameters are used.  As with
        /// IDbCommand.ExecuteScalar, it returns the first column of the first row in the result set
        /// returned by the query.  Extra columns or row are ignored.</remarks>
        /// <param name="cmdType">The command type</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <returns>The first column of the first row in the result set</returns>
        public virtual object ExecuteScalar(CommandType cmdType, string cmdText)
        {
            return classicAdoTemplate.ExecuteScalar(cmdType, cmdText);
        }

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
        public virtual object ExecuteScalar(CommandType cmdType, string cmdText,
                                            string parameterName, Enum dbType, int size, object parameterValue)
        {
            return classicAdoTemplate.ExecuteScalar(cmdType, cmdText, parameterName, dbType, size, parameterValue);

        }


        /// <summary>
        /// Execute the query with the specified command text and parameters returning a scalar result
        /// </summary>
        /// <param name="cmdType">The command type</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="parameters">The parameter collection to map.</param>
        /// <returns>The first column of the first row in the result set</returns>
        public virtual object ExecuteScalar(CommandType cmdType, string cmdText,
                                            IDbParameters parameters)
        {
            return classicAdoTemplate.ExecuteScalar(cmdType, cmdText, parameters);
        }

        /// <summary>
        /// Execute the query with the specified command text and parameters set via the
        /// command setter, returning a scalar result
        /// </summary>
        /// <param name="cmdType">The command type</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="commandSetter">The command setter.</param>
        /// <returns>The first column of the first row in the result set</returns>
        public virtual object ExecuteScalar(CommandType cmdType, string cmdText,
                                            ICommandSetter commandSetter)
        {
            return classicAdoTemplate.ExecuteScalar(cmdType, cmdText, commandSetter);
        }

        /// <summary>
        /// Execute the query with a command created via IDbCommandCreator and
        /// parameters
        /// </summary>
        /// <remarks>Output parameters can be retrieved via the returned
        /// dictionary.
        /// <para>
        /// More commonly used as a lower level support method within the framework,
        /// for example for StoredProcedure/AdoScalar.
        /// </para></remarks>
        /// <param name="commandCreator">The callback to create a IDbCommand.</param>
        /// <returns>A dictionary containing output parameters, if any</returns>
        public virtual IDictionary ExecuteScalar(IDbCommandCreator commandCreator)
        {
            return classicAdoTemplate.ExecuteScalar(commandCreator);
        }

        #endregion

        #region Queries with RowCallback

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
        public virtual void QueryWithRowCallback(CommandType cmdType, string cmdText, IRowCallback rowCallback,
                                                 ICommandSetter commandSetter)
        {
            classicAdoTemplate.QueryWithRowCallback(cmdType, cmdText, rowCallback, commandSetter);
        }
        /// <summary>
        /// Execute a query given IDbCommand's type and text, reading a
        /// single result set on a per-row basis with a <see cref="IRowCallback"/>.
        /// </summary>
        /// <param name="cmdType">The type of command</param>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="rowCallback">callback that will extract results
        /// one row at a time.
        /// </param>
        public virtual void QueryWithRowCallback(CommandType cmdType, string cmdText, IRowCallback rowCallback)
        {
            classicAdoTemplate.QueryWithRowCallback(cmdType, cmdText, rowCallback);
        }

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
        public virtual void QueryWithRowCallback(CommandType cmdType, string cmdText, IRowCallback rowCallback,
                                                 string parameterName, Enum dbType, int size, object parameterValue)
        {
            classicAdoTemplate.QueryWithRowCallback(cmdType, cmdText, rowCallback,
                                                    parameterName, dbType, size, parameterValue);
        }

        /// <summary>
        /// Execute a query given IDbCommand's type and text and provided IDbParameters,
        /// reading a single result set on a per-row basis with a <see cref="IRowCallback"/>.
        /// </summary>
        /// <param name="cmdType">Type of the command.</param>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="rowCallback">callback that will extract results
        /// one row at a time.</param>
        /// <param name="parameter">The parameter collection to map.</param>
        public virtual void QueryWithRowCallback(CommandType cmdType, string cmdText, IRowCallback rowCallback,
                                  IDbParameters parameter)
        {
            classicAdoTemplate.QueryWithRowCallback(cmdType, cmdText, rowCallback,
                                                    parameter);
        }

        #endregion

        #region Queries with RowCallback Delegate

        public virtual void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate)
        {
            classicAdoTemplate.QueryWithRowCallbackDelegate(cmdType, sql, rowCallbackDelegate);

        }
        public virtual void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate, ICommandSetter commandSetter)
        {
            classicAdoTemplate.QueryWithRowCallbackDelegate(cmdType, sql, rowCallbackDelegate, commandSetter);
        }

        public virtual void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate,
                        string parameterName, Enum dbType, int size, object parameterValue)
        {
            classicAdoTemplate.QueryWithRowCallbackDelegate(cmdType, sql, rowCallbackDelegate,
                                                            parameterName, dbType, size, parameterValue);

        }

        public virtual void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate, IDbParameters parameters)
        {
            classicAdoTemplate.QueryWithRowCallbackDelegate(cmdType, sql, rowCallbackDelegate, parameters);

        }

        #endregion

        #region Queries with RowMapper<T>

        public virtual IList<T> QueryWithRowMapper<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper)
        {
            return QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor<T>(rowMapper));

        }


        public virtual IList<T> QueryWithRowMapper<T>(CommandType cmdType, String cmdText, IRowMapper<T> rowMapper,
                                              ICommandSetter commandSetter)
        {
            return QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor<T>(rowMapper), commandSetter);

        }

        public virtual IList<T> QueryWithRowMapper<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper,
                                              string parameterName, Enum dbType, int size, object parameterValue)
        {
            return QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor<T>(rowMapper), parameterName, dbType, size, parameterValue);

        }

        public virtual IList<T> QueryWithRowMapper<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper, IDbParameters parameters)
        {
            return QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor<T>(rowMapper), parameters);

        }



        #endregion

        #region Queries with RowMapperDelegate<T>

        public virtual IList<T> QueryWithRowMapperDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate)
        {
            return QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor<T>(rowMapperDelegate));

        }

        public virtual IList<T> QueryWithRowMapperDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate,
                                                      ICommandSetter commandSetter)
        {
            return QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor<T>(rowMapperDelegate),
                                               commandSetter);
        }

        public virtual IList<T> QueryWithRowMapperDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate,
                                                      string parameterName, Enum dbType, int size, object parameterValue)
        {
            return QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor<T>(rowMapperDelegate),
                                               parameterName, dbType, size, parameterValue);
        }

        public virtual IList<T> QueryWithRowMapperDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate,
                                               IDbParameters parameters)
        {
            return QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor<T>(rowMapperDelegate),
                                               parameters);
        }
        #endregion

        #region Queries with ResultSetExtractor<T>

        public virtual T QueryWithResultSetExtractor<T>(CommandType cmdType,
                                                string cmdText,
                                                IResultSetExtractor<T> resultSetExtractor)
        {
            AssertUtils.ArgumentNotNull(cmdText, "cmdText", "CommandText must not be null");
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing CommandText [" + cmdText + "]");
            }
            return Execute<T>(new QueryCallback<T>(this, cmdType, cmdText, resultSetExtractor, null));
        }

        public virtual T QueryWithResultSetExtractor<T>(CommandType commandType, string cmdText, IResultSetExtractor<T> resultSetExtractor,
                                                string name, Enum dbType, int size, object parameterValue)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractor, "resultSetExtractor", "Result Set Extractor must not be null");
            return Execute<T>(new QueryCallback<T>(this, commandType, cmdText, resultSetExtractor,
                                                   CreateDbParameters(name, dbType, size, parameterValue)));
        }

        public virtual T QueryWithResultSetExtractor<T>(CommandType commandType, string cmdText,
                                                IResultSetExtractor<T> resultSetExtractor,
                                                IDbParameters parameters)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractor, "resultSetExtractor", "Result Set Extractor must not be null");
            return Execute<T>(new QueryCallback<T>(this, commandType, cmdText, resultSetExtractor, parameters));
        }

        public virtual T QueryWithResultSetExtractor<T>(CommandType cmdType, string cmdText,
                                                IResultSetExtractor<T> resultSetExtractor,
                                                ICommandSetter commandSetter)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractor, "resultSetExtractor", "Result Set Extractor must not be null");
            return Execute<T>(new QueryCallbackWithCommandSetter<T>(this, cmdType, cmdText,
                                                                    resultSetExtractor,
                                                                    commandSetter));
        }

        public virtual T QueryWithResultSetExtractor<T>(CommandType cmdType, string cmdText,
                                                IResultSetExtractor<T> resultSetExtractor,
                                                CommandSetterDelegate commandSetterDelegate)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractor, "resultSetExtractor", "Result Set Extractor must not be null");
            return Execute<T>(new QueryCallbackWithCommandSetterDelegate<T>(this, cmdType, cmdText,
                                                                    resultSetExtractor,
                                                                    commandSetterDelegate));
        }

        #endregion


        #region Queries with ResultSetExtractorDelegate<T>

        public virtual T QueryWithResultSetExtractorDelegate<T>(CommandType cmdType,
                                                        string cmdText,
                                                        ResultSetExtractorDelegate<T> resultSetExtractorDelegate)
        {
            AssertUtils.ArgumentNotNull(cmdText, "cmdText", "CommandText must not be null");
            AssertUtils.ArgumentNotNull(resultSetExtractorDelegate, "resultSetExtractorDelegate", "Result set extractor delegate must not be null");
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing CommandText [" + cmdText + "]");
            }

            return Execute<T>(new QueryCallback<T>(this, cmdType, cmdText, resultSetExtractorDelegate, null));
        }

        public virtual T QueryWithResultSetExtractorDelegate<T>(CommandType commandType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractorDelegate,
                                                        string paramenterName, Enum dbType, int size, object parameterValue)
        {
            AssertUtils.ArgumentNotNull(cmdText, "cmdText", "CommandText must not be null");
            AssertUtils.ArgumentNotNull(resultSetExtractorDelegate, "resultSetExtractorDelegate", "Result set extractor delegate must not be null");
            return Execute<T>(new QueryCallback<T>(this, commandType, cmdText, resultSetExtractorDelegate,
                                                   CreateDbParameters(paramenterName, dbType, size, parameterValue)));
        }

        public virtual T QueryWithResultSetExtractorDelegate<T>(CommandType commandType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractorDelegate,
                                                IDbParameters parameters)
        {
            AssertUtils.ArgumentNotNull(cmdText, "cmdText", "CommandText must not be null");
            AssertUtils.ArgumentNotNull(resultSetExtractorDelegate, "resultSetExtractorDelegate", "Result set extractor delegate  must not be null");
            return Execute<T>(new QueryCallback<T>(this, commandType, cmdText, resultSetExtractorDelegate, parameters));
        }

        public virtual T QueryWithResultSetExtractorDelegate<T>(CommandType cmdType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractorDelegate,
                                                        ICommandSetter commandSetter)
        {
            AssertUtils.ArgumentNotNull(cmdText, "cmdText", "CommandText must not be null");
            AssertUtils.ArgumentNotNull(resultSetExtractorDelegate, "resultSetExtractorDelegate", "Result set extractor delegate must not be null");
            return Execute<T>(new QueryCallbackWithCommandSetter<T>(this, cmdType, cmdText,
                                                                    resultSetExtractorDelegate,
                                                                    commandSetter));
        }

        public virtual T QueryWithResultSetExtractorDelegate<T>(CommandType cmdType, string cmdText, ResultSetExtractorDelegate<T> resultSetExtractorDelegate,
                                                        CommandSetterDelegate commandSetterDelegate)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractorDelegate, "resultSetExtractorDelegate", "Result set extractor delegate must not be null");
            return Execute<T>(new QueryCallbackWithCommandSetterDelegate<T>(this, cmdType, cmdText,
                                                                    resultSetExtractorDelegate,
                                                                    commandSetterDelegate));
        }

        #endregion

        #region Query for Object<T>

        public virtual T QueryForObject<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper)
        {
            IList<T> results = QueryWithRowMapper(cmdType, cmdText, rowMapper);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        public virtual T QueryForObject<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper, ICommandSetter commandSetter)
        {
            IList<T> results = QueryWithRowMapper(cmdType, cmdText, rowMapper, commandSetter);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        public virtual T QueryForObject<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper, IDbParameters parameters)
        {
            IList<T> results = QueryWithRowMapper(cmdType, cmdText, rowMapper, parameters);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }


        public virtual T QueryForObject<T>(CommandType cmdType, string cmdText, IRowMapper<T> rowMapper,
                                   string parameterName, Enum dbType, int size, object parameterValue)
        {
            IList<T> results = QueryWithRowMapper(cmdType, cmdText, rowMapper, parameterName, dbType, size, parameterValue);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }
        #endregion

        #region Query for ObjectDelegate<T>

        public virtual T QueryForObjectDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate)
        {
            IList<T> results = QueryWithRowMapperDelegate(cmdType, cmdText, rowMapperDelegate);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        public virtual T QueryForObjectDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate, ICommandSetter commandSetter)
        {
            IList<T> results = QueryWithRowMapperDelegate(cmdType, cmdText, rowMapperDelegate, commandSetter);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        public virtual T QueryForObjectDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate, IDbParameters parameters)
        {
            IList<T> results = QueryWithRowMapperDelegate(cmdType, cmdText, rowMapperDelegate, parameters);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }


        public virtual T QueryForObjectDelegate<T>(CommandType cmdType, string cmdText, RowMapperDelegate<T> rowMapperDelegate,
                                   string parameterName, Enum dbType, int size, object parameterValue)
        {
            IList<T> results = QueryWithRowMapperDelegate(cmdType, cmdText, rowMapperDelegate, parameterName, dbType, size, parameterValue);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }
        #endregion


        #region Query with CommandCreator

        public virtual T QueryWithCommandCreator<T>(IDbCommandCreator cc, IResultSetExtractor<T> rse)
        {
            return QueryWithCommandCreator<T>(cc, rse, null);
        }

        public virtual void QueryWithCommandCreator(IDbCommandCreator cc, IRowCallback rowCallback)
        {
            classicAdoTemplate.QueryWithCommandCreator(cc, rowCallback, null);
        }

        public virtual IList<T> QueryWithCommandCreator<T>(IDbCommandCreator cc, IRowMapper<T> rowMapper)
        {
            return QueryWithCommandCreator<T>(cc, rowMapper, null);
        }

        public virtual T QueryWithCommandCreator<T>(IDbCommandCreator cc, IResultSetExtractor<T> rse, IDictionary returnedParameters)
        {
            if (rse == null)
            {
                throw new ArgumentNullException("Result Set Extractor must not be null");
            }

            return Execute(cc, new AdoResultSetExtractorWithOutputParamsCommandCallback<T>(this, rse, returnedParameters));
        }


        public virtual void QueryWithCommandCreator(IDbCommandCreator cc, IRowCallback rowCallback, IDictionary returnedParameters)
        {
            classicAdoTemplate.QueryWithCommandCreator(cc, rowCallback, returnedParameters);
        }


        public virtual IList<T> QueryWithCommandCreator<T>(IDbCommandCreator cc, IRowMapper<T> rowMapper, IDictionary returnedParameters)
        {
            if (rowMapper == null)
            {
                throw new ArgumentNullException("rowMapper must not be null");
            }

            return Execute(cc, new AdoRowMapperQueryCommandCallback<T>(this, rowMapper, returnedParameters));
        }


        public virtual IDictionary QueryWithCommandCreator<T>(IDbCommandCreator cc, IList namedResultSetProcessors)
        {
            return classicAdoTemplate.Execute(cc, new AdoResultProcessorsQueryCommandCallback<T>(this, namedResultSetProcessors)) as IDictionary;
        }


        public virtual IDictionary QueryWithCommandCreator<T,U>(IDbCommandCreator cc, IList namedResultSetProcessors)
        {
            return classicAdoTemplate.Execute(cc, new AdoResultProcessorsQueryCommandCallback<T,U>(this, namedResultSetProcessors)) as IDictionary;
        }


        #endregion

        #region General Helper Methods

        /// <summary>
        /// Checks if DbProvider is not null and creates ExceptionTranslator if not LazyInit.
        /// </summary>
        public override void AfterPropertiesSet()
        {
            classicAdoTemplate.AfterPropertiesSet();
            classicAdoTemplate.CommandTimeout = CommandTimeout;
        }

        /// <summary>
        /// Creates the data reader wrapper for use in AdoTemplate callback methods.
        /// </summary>
        /// <param name="readerToWrap">The reader to wrap.</param>
        /// <returns>The data reader used in AdoTemplate callbacks</returns>
        public override IDataReader CreateDataReaderWrapper(IDataReader readerToWrap)
        {
            return classicAdoTemplate.CreateDataReaderWrapper(readerToWrap);
        }

        #endregion

        #region Parameter Creation Helper Methods

        /// <summary>
        /// Derives the parameters of a stored procedure including the return parameter
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="includeReturnParameter">if set to <c>true</c> to include return parameter.</param>
        /// <returns>The stored procedure parameters</returns>
        public override IDataParameter[] DeriveParameters(string procedureName, bool includeReturnParameter)
        {
            return classicAdoTemplate.DeriveParameters(procedureName, includeReturnParameter);
        }

        #endregion





        #region Internal Helper classes

        internal class QueryCallback<T> : IDbCommandCallback<T>, ICommandTextProvider
        {
            private AdoTemplate adoTemplate;
            private IResultSetExtractor<T> rse;
            private ResultSetExtractorDelegate<T> resultSetExtractorDelegate;
            private CommandType commandType;
            private string commandText;
            private IDbParameters parameters;

            public QueryCallback(AdoTemplate adoTemplate,
                                 CommandType cmdType,
                                 string cmdText,
                                 IResultSetExtractor<T> rse,
                                 IDbParameters dbParameters)
            {
                this.adoTemplate = adoTemplate;
                commandType = cmdType;
                commandText = cmdText;
                this.rse = rse;
                parameters = dbParameters;
            }

            public QueryCallback(AdoTemplate adoTemplate,
                         CommandType cmdType,
                         string cmdText,
                         ResultSetExtractorDelegate<T> resultSetExtractorDelegate,
                         IDbParameters dbParameters)
            {
                this.adoTemplate = adoTemplate;
                commandType = cmdType;
                commandText = cmdText;
                this.resultSetExtractorDelegate = resultSetExtractorDelegate;
                parameters = dbParameters;
            }

            public string CommandText
            {
                get { return commandText; }
            }

            public T DoInCommand(IDbCommand command)
            {
                IDataReader reader = null;
                try
                {
                    command.CommandType = commandType;
                    command.CommandText = commandText;
                    ParameterUtils.CopyParameters(command, parameters);
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());
                    T returnValue = default(T);
                    if (rse != null)
                    {
                        returnValue = rse.ExtractData(reader);
                    }
                    else
                    {
                        returnValue = resultSetExtractorDelegate(reader);
                    }
                    return returnValue;
                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                    ParameterUtils.CopyParameters(parameters, command);
                }
            }


        }

        internal class QueryCallbackWithCommandSetter<T> : IDbCommandCallback<T>, ICommandTextProvider
        {
            private AdoTemplate adoTemplate;
            private IResultSetExtractor<T> rse;
            private ResultSetExtractorDelegate<T> resultSetExtractorDelegate;
            private CommandType commandType;
            private string commandText;
            private ICommandSetter commandSetter;

            public QueryCallbackWithCommandSetter(AdoTemplate adoTemplate,
                                                     CommandType cmdType,
                                                     string cmdText,
                                                     IResultSetExtractor<T> rse,
                                                     ICommandSetter commandSetter)
            {
                this.adoTemplate = adoTemplate;
                commandType = cmdType;
                commandText = cmdText;
                this.rse = rse;
                this.commandSetter = commandSetter;
            }

            public QueryCallbackWithCommandSetter(AdoTemplate adoTemplate,
                                             CommandType cmdType,
                                             string cmdText,
                                             ResultSetExtractorDelegate<T> resultSetExtractorDelegate,
                                             ICommandSetter commandSetter)
            {
                this.adoTemplate = adoTemplate;
                commandType = cmdType;
                commandText = cmdText;
                this.resultSetExtractorDelegate = resultSetExtractorDelegate;
                this.commandSetter = commandSetter;
            }

            public string CommandText
            {
                get { return commandText; }
            }

            public T DoInCommand(IDbCommand command)
            {
                IDataReader reader = null;
                try
                {
                    command.CommandType = commandType;
                    command.CommandText = commandText;
                    if (commandSetter != null)
                    {
                        commandSetter.SetValues(command);
                    }
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());
                    if (rse != null)
                    {
                        return rse.ExtractData(reader);
                    }
                    else
                    {
                        return resultSetExtractorDelegate(reader);
                    }

                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                }
            }


        }

        internal class QueryCallbackWithCommandSetterDelegate<T> : IDbCommandCallback<T>, ICommandTextProvider
        {
            private AdoTemplate adoTemplate;
            private IResultSetExtractor<T> rse;
            private ResultSetExtractorDelegate<T> resultSetExtractorDelegate;
            private CommandType commandType;
            private string commandText;
            private CommandSetterDelegate commandSetterDelegate;

            public QueryCallbackWithCommandSetterDelegate(AdoTemplate adoTemplate,
                                                        CommandType cmdType,
                                                        string cmdText,
                                                        IResultSetExtractor<T> rse,
                                                        CommandSetterDelegate commandSetterDelegate)
            {
                this.adoTemplate = adoTemplate;
                commandType = cmdType;
                commandText = cmdText;
                this.rse = rse;
                this.commandSetterDelegate = commandSetterDelegate;
            }

            public QueryCallbackWithCommandSetterDelegate(AdoTemplate adoTemplate,
                                                CommandType cmdType,
                                                string cmdText,
                                                ResultSetExtractorDelegate<T> resultSetExtractorDelegate,
                                                CommandSetterDelegate commandSetterDelegate)
            {
                this.adoTemplate = adoTemplate;
                commandType = cmdType;
                commandText = cmdText;
                this.resultSetExtractorDelegate = resultSetExtractorDelegate;
                this.commandSetterDelegate = commandSetterDelegate;
            }

            public string CommandText
            {
                get { return commandText; }
            }

            public T DoInCommand(IDbCommand command)
            {
                IDataReader reader = null;
                try
                {
                    command.CommandType = commandType;
                    command.CommandText = commandText;
                    if (commandSetterDelegate != null)
                    {
                        commandSetterDelegate(command);
                    }
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());
                    if (rse != null)
                    {
                        return rse.ExtractData(reader);
                    }
                    else
                    {
                        return resultSetExtractorDelegate(reader);
                    }
                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                }
            }


        }

        internal class AdoRowMapperQueryCommandCallback<T> : IDbCommandCallback<IList<T>>
        {
            private AdoTemplate adoTemplate;
            private IRowMapper<T> rowMapper;
            private IDictionary returnedParameters;

            public AdoRowMapperQueryCommandCallback(AdoTemplate adoTemplate, IRowMapper<T> rowMapper, IDictionary returnedParameters)
            {
                this.adoTemplate = adoTemplate;
                this.rowMapper = rowMapper;
                this.returnedParameters = returnedParameters;
            }

            public IList<T> DoInCommand(IDbCommand command)
            {
                IList<T> objectList;
                //Extract the single returned result set
                IDataReader reader = null;
                try
                {
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());
                    RowMapperResultSetExtractor<T> rse = new RowMapperResultSetExtractor<T>(rowMapper, 1);
                    objectList = rse.ExtractData(reader);
                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                }
                ParameterUtils.ExtractOutputParameters(returnedParameters, command);
                return objectList;
            }
        }

        internal class AdoResultSetExtractorWithOutputParamsCommandCallback<T> : IDbCommandCallback<T>
        {
            private AdoTemplate adoTemplate;
            private IResultSetExtractor<T> rse;
            private IDictionary returnedParameters;

            public AdoResultSetExtractorWithOutputParamsCommandCallback(AdoTemplate adoTemplate, IResultSetExtractor<T> rse, IDictionary returnedParameters)
            {
                this.adoTemplate = adoTemplate;
                this.rse = rse;
                this.returnedParameters = returnedParameters;
            }

            public T DoInCommand(IDbCommand command)
            {
                T returnVal;
                //Extract the single returned result set
                IDataReader reader = null;
                try
                {
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());
                    returnVal = rse.ExtractData(reader);
                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                }
                ParameterUtils.ExtractOutputParameters(returnedParameters, command);
                return returnVal;
            }
        }

        internal class BaseAdoResultProcessorsQueryCommandCallback<T>
        {
            public static void ProcessNonGenericResultSetProcessor(NamedResultSetProcessor otherResultSetProcessor, IDataReader reader, IDictionary returnedResults)
            {
                string parameterName = otherResultSetProcessor.Name;
                if (otherResultSetProcessor.ResultSetProcessor is IResultSetExtractor)
                {
                    IResultSetExtractor rse = (IResultSetExtractor)otherResultSetProcessor.ResultSetProcessor;
                    object result = rse.ExtractData(reader);
                    returnedResults.Add(parameterName, result);
                }
                else if (otherResultSetProcessor.ResultSetProcessor is IRowMapper)
                {
                    IRowMapper rowMapper = (IRowMapper)otherResultSetProcessor.ResultSetProcessor;
                    object result = (new RowMapperResultSetExtractor(rowMapper)).ExtractData(reader);
                    returnedResults.Add(parameterName, result);
                }
                else if (otherResultSetProcessor.ResultSetProcessor is IRowCallback)
                {
                    IRowCallback rowCallback = (IRowCallback)otherResultSetProcessor.ResultSetProcessor;
                    (new RowCallbackResultSetExtractor(rowCallback)).ExtractData(reader);
                    returnedResults.Add(parameterName, "ResultSet returned was processed by an IRowCallback");
                }
            }

            public static void ProcessFirstGenericResultSetProcessor(NamedResultSetProcessor<T> firstResultSetProcessor, IDataReader reader, IDictionary returnedResults)
            {
                string parameterName = firstResultSetProcessor.Name;
                if (firstResultSetProcessor.ResultSetExtractor != null)
                {
                    IResultSetExtractor<T> rse = firstResultSetProcessor.ResultSetExtractor;
                    T result = rse.ExtractData(reader);
                    returnedResults.Add(parameterName, result);
                }
                else if (firstResultSetProcessor.RowMapper != null)
                {
                    IRowMapper<T> rowMapper = firstResultSetProcessor.RowMapper;
                    object result = (new RowMapperResultSetExtractor<T>(rowMapper)).ExtractData(reader);
                    returnedResults.Add(parameterName, result);
                }
                else if (firstResultSetProcessor.RowCallback != null)
                {
                    IRowCallback rowCallback = firstResultSetProcessor.RowCallback;
                    (new RowCallbackResultSetExtractor(rowCallback)).ExtractData(reader);
                    returnedResults.Add(parameterName, "ResultSet returned was processed by an IRowCallback");
                }
            }
        }

        internal class AdoResultProcessorsQueryCommandCallback<T> : BaseAdoResultProcessorsQueryCommandCallback<T>, ICommandCallback
        {
            private AdoTemplate adoTemplate;
            private IList namedResultSetProcessors;

            public AdoResultProcessorsQueryCommandCallback(AdoTemplate adoTemplate, IList namedResultSetProcessors)
            {
                this.adoTemplate = adoTemplate;
                this.namedResultSetProcessors = namedResultSetProcessors;
            }

            public object DoInCommand(IDbCommand command)
            {
                IDictionary returnedResults = new Hashtable();
                int resultSetIndex = 0;
                IDataReader reader = null;
                try
                {
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());

                    //TODO On >= .NET 2.0 platforms make use of DbDataReader.HasRows property to
                    //     see if there is a result set.  now currently assuming matching
                    //     NamedResultSetProcessor/ResultSet pairs.

                    do
                    {
                        if (namedResultSetProcessors.Count == 0)
                        {
                            //We could just have output parameters and/or return value, that is, no result sets
                            //If we didn't register a result set processor, it is likely that a result set wasn't expected.
                            break;
                        }
                        NamedResultSetProcessor<T> firstResultSetProcessor = null;
                        NamedResultSetProcessor otherResultSetProcessor = null;
                        try
                        {
                            if (resultSetIndex == 0)
                            {
                                firstResultSetProcessor
                                    = namedResultSetProcessors[resultSetIndex] as NamedResultSetProcessor<T>;
                                //Will have possibility of run-time type error if using QueryWithCommandCreator
                                if (firstResultSetProcessor == null)
                                {
                                    LOG.Error("NamedResultSetProcessor for result set index " + resultSetIndex +
                                              ", is not of expected type NamedResultSetProcessor<T>.  Type = " +
                                              namedResultSetProcessors[resultSetIndex].GetType() +
                                              "; Skipping processing for this result set.");
                                    continue;
                                }
                            }
                            else
                            {
                                otherResultSetProcessor =
                                    namedResultSetProcessors[resultSetIndex] as NamedResultSetProcessor;
                                if (otherResultSetProcessor == null)
                                {

                                    LOG.Error("NamedResultSetProcessor for result set index " + resultSetIndex +
                                              ", is not of expected type NamedResultSetProcessor  Type = " +
                                              namedResultSetProcessors[resultSetIndex].GetType() +
                                              "; Skipping processing for this result set.");
                                    continue;
                                }
                            }
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            LOG.Error("No NamedResultSetProcessor associated with result set index " + resultSetIndex, e);
                            continue;
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            LOG.Error("No NamedResultSetProcessor associated with result set index " + resultSetIndex, e);
                            continue;
                        }


                        if (resultSetIndex == 0)
                        {
                            ProcessFirstGenericResultSetProcessor(firstResultSetProcessor, reader, returnedResults);
                        }
                        else
                        {
                            ProcessNonGenericResultSetProcessor(otherResultSetProcessor, reader, returnedResults);
                        }
                        resultSetIndex++;

                    } while (reader.NextResult());
                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                }
                ParameterUtils.ExtractOutputParameters(returnedResults, command);
                return returnedResults;
            }
        }


        internal class AdoResultProcessorsQueryCommandCallback<T, U> : BaseAdoResultProcessorsQueryCommandCallback<T>, ICommandCallback
        {
            private AdoTemplate adoTemplate;
            private IList namedResultSetProcessors;

            public AdoResultProcessorsQueryCommandCallback(AdoTemplate adoTemplate, IList namedResultSetProcessors)
            {
                this.adoTemplate = adoTemplate;
                this.namedResultSetProcessors = namedResultSetProcessors;
            }

            public object DoInCommand(IDbCommand command)
            {
                IDictionary returnedResults = new Hashtable();
                int resultSetIndex = 0;
                IDataReader reader = null;
                try
                {
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());

                    //TODO On >= .NET 2.0 platforms make use of DbDataReader.HasRows property to
                    //     see if there is a result set.  now currently assuming matching
                    //     NamedResultSetProcessor/ResultSet pairs.

                    do
                    {
                        if (namedResultSetProcessors.Count == 0)
                        {
                            //We could just have output parameters and/or return value, that is, no result sets
                            //If we didn't register a result set processor, it is likely that a result set wasn't expected.
                            break;
                        }
                        NamedResultSetProcessor<T> firstResultSetProcessor = null;
                        NamedResultSetProcessor<U> secondResultSetProcessor = null;
                        NamedResultSetProcessor otherResultSetProcessor = null;
                        try
                        {
                            if (resultSetIndex == 0)
                            {
                                firstResultSetProcessor = namedResultSetProcessors[resultSetIndex] as NamedResultSetProcessor<T>;
                                //Will only have possibility of run-time type error if using QueryWithCommandCreator
                                if (firstResultSetProcessor == null)
                                {

                                    LOG.Error("NamedResultSetProcessor for result set index " + resultSetIndex +
                                              ", is not of expected type NamedResultSetProcessor<T>  Type = " +
                                              namedResultSetProcessors[resultSetIndex].GetType() +
                                              "; Skipping processing for this result set.");
                                    continue;
                                }
                            } else if (resultSetIndex == 1)
                            {
                                secondResultSetProcessor
                                    = namedResultSetProcessors[resultSetIndex] as NamedResultSetProcessor<U>;
                                if (secondResultSetProcessor == null)
                                {

                                    LOG.Error("NamedResultSetProcessor for result set index " + resultSetIndex +
                                              ", is not of expected type NamedResultSetProcessor<T>  Type = " +
                                              namedResultSetProcessors[resultSetIndex].GetType() +
                                              "; Skipping processing for this result set.");
                                    continue;
                                }
                            } else
                            {
                                otherResultSetProcessor =
                                    namedResultSetProcessors[resultSetIndex] as NamedResultSetProcessor;
                                if (otherResultSetProcessor == null)
                                {

                                    LOG.Error("NamedResultSetProcessor for result set index " + resultSetIndex +
                                              ", is not of expected type NamedResultSetProcessor  Type = " +
                                              namedResultSetProcessors[resultSetIndex].GetType() +
                                              "; Skipping processing for this result set.");
                                    continue;
                                }
                            }

                        }
                        catch (IndexOutOfRangeException e)
                        {
                            LOG.Error("No NamedResultSetProcessor associated with result set index " + resultSetIndex, e);
                            continue;
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            LOG.Error("No NamedResultSetProcessor associated with result set index " + resultSetIndex, e);
                            continue;
                        }

                        if (resultSetIndex == 0)
                        {
                            ProcessFirstGenericResultSetProcessor(firstResultSetProcessor, reader, returnedResults);
                        }
                        else if (resultSetIndex == 1)
                        {
                            string parameterName = secondResultSetProcessor.Name;
                            if (secondResultSetProcessor.ResultSetExtractor != null)
                            {
                                IResultSetExtractor<U> rse = secondResultSetProcessor.ResultSetExtractor;
                                U result = rse.ExtractData(reader);
                                returnedResults.Add(parameterName, result);
                            }
                            else if (secondResultSetProcessor.RowMapper != null)
                            {
                                IRowMapper<U> rowMapper = secondResultSetProcessor.RowMapper;
                                object result = (new RowMapperResultSetExtractor<U>(rowMapper)).ExtractData(reader);
                                returnedResults.Add(parameterName, result);
                            }
                            else if (secondResultSetProcessor.RowCallback != null)
                            {
                                //In this case a dummy type parameter would need to be specified a better alternative would
                                //be to to use the single type parameterization of this callback.
                                IRowCallback rowCallback = secondResultSetProcessor.RowCallback;
                                (new RowCallbackResultSetExtractor(rowCallback)).ExtractData(reader);
                                returnedResults.Add(parameterName, "ResultSet returned was processed by an IRowCallback");
                            }
                        }
                        else
                        {
                            ProcessNonGenericResultSetProcessor(otherResultSetProcessor, reader, returnedResults);
                        }
                        resultSetIndex++;

                    } while (reader.NextResult());
                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                }
                ParameterUtils.ExtractOutputParameters(returnedResults, command);
                return returnedResults;
            }


        }
        #endregion

    }
}
