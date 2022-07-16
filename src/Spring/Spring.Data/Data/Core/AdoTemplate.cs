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
using System.Globalization;
using System.Reflection;
using Common.Logging;
using Spring.Dao.Support;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Reflection.Dynamic;
using Spring.Util;

namespace Spring.Data.Core
{
    /// <summary>
    /// This is the central class in the Spring.Data namespace.
    /// It simplifies the use of ADO.NET and helps to avoid commons errors.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class AdoTemplate : AdoAccessor, IAdoOperations
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(AdoTemplate));

        #endregion

        #region Fields

        private IDbProvider dbProvider;

        private IAdoExceptionTranslator exceptionTranslator;

        private bool lazyInit = true;
        private Type dataReaderWrapperType;
        protected IDynamicConstructor newDataReaderWrapper;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoTemplate"/> class.
        /// </summary>
        public AdoTemplate()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoTemplate"/> class.
        /// </summary>
        /// <param name="provider">The database provider.</param>
        public AdoTemplate(IDbProvider provider)
        {
            DbProvider = provider;
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
            DbProvider = provider;
            LazyInit = lazyInit;
            AfterPropertiesSet();
        }

        #endregion

        #region Properties

        /// <summary>
        /// An instance of a DbProvider implementation.
        /// </summary>
        public override IDbProvider DbProvider
        {
            get { return dbProvider; }
            set
            {
                dbProvider = value;
            }
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
            get { return lazyInit; }
            set { lazyInit = value; }
        }

        /// <summary>
        /// Gets or sets the exception translator. If no custom translator is provided, a default
        /// <see cref="ErrorCodeExceptionTranslator"/> is used.
        /// </summary>
        /// <value>The exception translator.</value>
        public override IAdoExceptionTranslator ExceptionTranslator
        {
            get
            {
                InitExceptionTranslator();
                return exceptionTranslator;
            }
            set
            {
                exceptionTranslator = value;
            }
        }

        /// <summary>
        /// Gets or set the System.Type to use to create an instance of IDataReaderWrapper
        /// for the purpose of having defaults values to use in case of DBNull values read
        /// from IDataReader.
        /// </summary>
        /// <value>The type of the data reader wrapper.</value>
        public override Type DataReaderWrapperType
        {
            get { return dataReaderWrapperType; }
            set
            {
                if (dataReaderWrapperType == null)
                {
                    if (typeof(IDataReaderWrapper).IsAssignableFrom(value))
                    {
                        dataReaderWrapperType = value;
                        ConstructorInfo constructor = ObjectUtils.GetZeroArgConstructorInfo(dataReaderWrapperType);
                        newDataReaderWrapper = DynamicConstructor.Create(constructor);

                    }
                    else
                    {
                        throw new ArgumentException("DataReaderWrapper type must implement IDataReaderWrapper. Implemented interfaces on "
                                                    + value.GetType().Name + "are [" +
                                                    StringUtils.CollectionToCommaDelimitedString(ReflectionUtils.ToInterfaceArray(value)) + "]");

                    }
                }
                else
                {
                    LOG.Warn("Ignoring assignment of DataReaderWrapperType since it has already been assigned.");
                }

            }

        }

        #endregion


        public override void AfterPropertiesSet()
        {
            if (DbProvider == null)
            {
                throw new ArgumentException("DbProvider is required");
            }
            if (!LazyInit)
            {
                InitExceptionTranslator();
            }
        }


        #region General Execute Callback Methods
        /// <summary>
        /// Execute a ADO.NET operation on a command object using a delegate callback.
        /// </summary>
        /// <param name="del">The delegate called with a command object.</param>
        /// <returns>
        /// A result object returned by the action or null
        /// </returns>
        /// <remarks>This allows for implementing arbitrary data access operations
        /// on a single command within Spring's managed ADO.NET environment.</remarks>
        public virtual Object Execute(CommandDelegate del)
        {
            return Execute(new ExecuteCommandCallbackUsingDelegate(del));
        }

        /// <summary>
        /// Callback to execute a IDbCommand.
        /// </summary>
        /// <param name="action">the callback to execute</param>
        /// <returns>object returned from callback</returns>
        public virtual object Execute(ICommandCallback action)
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
                Object result = action.DoInCommand(command);
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
        /// Executes ADO.NET operations on a command object, created by the provided IDbCommandCreator,
        /// using the interface based callback IDbCommandCallback.
        /// </summary>
        /// <param name="commandCreator">The command creator.</param>
        /// <param name="action">The callback to execute based on IDbCommand</param>
        /// <returns>A result object returned by the action or null</returns>
        public virtual object Execute(IDbCommandCreator commandCreator, ICommandCallback action)
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
                ApplyCommandSettings(command);
                //TODO collect warnings...
                //RegisterEventHandlers(command.Connection);
                Object result = action.DoInCommand(command);

                //SqlWarnings sqlWarnings = GetSqlWarnings()
                //ThrowExceptionOnWarningIfNotIgnoringWarnings(sqlWarnings);

                return result;
            }
            catch (Exception e)
            {
                commandCreator = null;
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
        /// <remarks>This allows for implementing abritrary data access operations
        /// on a single DataAdapter within Spring's managed ADO.NET environment.
        /// </remarks>
        /// <param name="dataAdapterCallback">The data adapter callback.</param>
        /// <returns>A result object returned by the callback or null</returns>
        public virtual object Execute(IDataAdapterCallback dataAdapterCallback)
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
                object result = dataAdapterCallback.DoInDataAdapter(dataAdapter);
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
            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing NonQuery " + cmdType + "[" + cmdText + "]");
            }
            #endregion

            return (int)Execute(new ExecuteNonQueryCallbackWithParameters(cmdType, cmdText, null));
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
            return (int)Execute(new ExecuteNonQueryCallbackWithParameters(cmdType, cmdText,
                                                                          CreateDbParameters(parameterName, dbType, size, parameterValue)));


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
            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing NonQuery.  " + cmdType + "[" + cmdText + "]");
            }
            #endregion

            return (int)Execute(new ExecuteNonQueryCallbackWithParameters(cmdType, cmdText, parameters));

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
            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing NonQuery. " + cmdType + "[" + cmdText + "]");
            }
            #endregion

            return (int)Execute(new ExecuteNonQueryCallbackWithCommandSetter(cmdType, cmdText, commandSetter));
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

            return (IDictionary)Execute(commandCreator, new AdoNonQueryWithOutputParamsCommandCallback());

        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// Execute the query with the specified command text.
        /// </summary>
        /// <remarks>No parameters are used.  As with
        /// IDbCommand.ExecuteScalar, it returns the first column of the first row in the resultset
        /// returned by the query.  Extra columns or row are ignored.</remarks>
        /// <param name="cmdType">The command type</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <returns>The first column of the first row in the result set</returns>
        public virtual object ExecuteScalar(CommandType cmdType, string cmdText)
        {
            return Execute(new ExecuteScalarCallbackWithParameters(cmdType, cmdText, null));
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
            return Execute(new ExecuteScalarCallbackWithParameters(cmdType, cmdText,
                                                                   CreateDbParameters(parameterName, dbType, size, parameterValue)));

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
            return Execute(new ExecuteScalarCallbackWithParameters(cmdType, cmdText, parameters));

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
            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing ExecuteScalar. " + cmdType + "[" + cmdText + "]");
            }
            #endregion

            return Execute(new ExecuteScalarCallbackWithCommandSetter(cmdType, cmdText, commandSetter));

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
            return (IDictionary)Execute(commandCreator, new AdoStoredProcedureScalarCommandCallback());
        }

        #endregion

        #region Query with RowCallback

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
            QueryWithResultSetExtractor(cmdType, cmdText, new RowCallbackResultSetExtractor(rowCallback));
        }

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
            QueryWithResultSetExtractor(cmdType, cmdText, new RowCallbackResultSetExtractor(rowCallback), commandSetter);

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
            QueryWithResultSetExtractor(cmdType, cmdText, new RowCallbackResultSetExtractor(rowCallback), parameterName, dbType, size, parameterValue);
        }


        public virtual void QueryWithRowCallback(CommandType cmdType, string cmdText, IRowCallback rowCallback, IDbParameters parameters)
        {
            QueryWithResultSetExtractor(cmdType, cmdText, new RowCallbackResultSetExtractor(rowCallback), parameters);
        }

        #endregion

        // RowCallback with Delegate Questionable for 1.1 since no anonymous delegates and there isn't a collecting parameter
        // in the delegate method signature.

        #region Query with RowCallback Delegate

        public virtual void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate)
        {
            QueryWithResultSetExtractor(cmdType, sql, new RowCallbackResultSetExtractor(rowCallbackDelegate));

        }
        public virtual void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate, ICommandSetter commandSetter)
        {
            QueryWithResultSetExtractor(cmdType, sql, new RowCallbackResultSetExtractor(rowCallbackDelegate), commandSetter);

        }

        public virtual void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate,
                                                 string name, Enum dbType, int size, object parameterValue)
        {
            QueryWithResultSetExtractor(cmdType, sql, new RowCallbackResultSetExtractor(rowCallbackDelegate), name, dbType, size, parameterValue);

        }

        public virtual void QueryWithRowCallbackDelegate(CommandType cmdType, string sql, RowCallbackDelegate rowCallbackDelegate, IDbParameters parameters)
        {
            QueryWithResultSetExtractor(cmdType, sql, new RowCallbackResultSetExtractor(rowCallbackDelegate), parameters);

        }



        #endregion

        #region Query with RowMapper

        public virtual IList QueryWithRowMapper(CommandType cmdType, string cmdText, IRowMapper rowMapper)
        {
            return (IList)QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor(rowMapper));
        }

        public virtual IList QueryWithRowMapper(CommandType cmdType, string cmdText, IRowMapper rowMapper, ICommandSetter commandSetter)
        {

            return (IList)QueryWithResultSetExtractor(cmdType, cmdText,
                                                      new RowMapperResultSetExtractor(rowMapper), commandSetter);

        }


        public virtual IList QueryWithRowMapper(CommandType cmdType, string cmdText, IRowMapper rowMapper,
                                                string name, Enum dbType, int size, object parameterValue)
        {
            return (IList)QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor(rowMapper), name, dbType, size, parameterValue);
        }


        public virtual IList QueryWithRowMapper(CommandType cmdType, string cmdText, IRowMapper rowMapper, IDbParameters parameters)
        {
            return (IList)QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor(rowMapper), parameters);
        }


        #endregion

        #region Query With RowMapper Delegate

        public virtual IList QueryWithRowMapperDelegate(CommandType cmdType, string cmdText, RowMapperDelegate rowMapperDelegate)
        {
            return (IList)QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor(rowMapperDelegate));
        }

        public virtual IList QueryWithRowMapperDelegate(CommandType cmdType, string cmdText, RowMapperDelegate rowMapperDelegate, ICommandSetter commandSetter)
        {
            return (IList)QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor(rowMapperDelegate), commandSetter);
        }


        public virtual IList QueryWithRowMapperDelegate(CommandType cmdType, string cmdText, RowMapperDelegate rowMapperDelegate,
                                                        string parameterName, Enum dbType, int size, object parameterValue)
        {
            return (IList)QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor(rowMapperDelegate), parameterName, dbType, size, parameterValue);
        }


        public virtual IList QueryWithRowMapperDelegate(CommandType cmdType, string cmdText, RowMapperDelegate rowMapperDelegate,
                                                        IDbParameters parameters)
        {
            return (IList)QueryWithResultSetExtractor(cmdType, cmdText, new RowMapperResultSetExtractor(rowMapperDelegate), parameters);
        }

        #endregion

        #region Query with ResultSetExtractor

        /// <summary>
        /// Execute a query given static SQL/Stored Procedure name
        /// and process a single result set with an instance of IResultSetExtractor
        /// </summary>
        /// <param name="cmdType">The type of command.</param>
        /// <param name="sql">The SQL/Stored Procedure to execute</param>
        /// <param name="rse">Object that will extract all rows of a result set</param>
        /// <returns>An arbitrary result object, as returned by the IResultSetExtractor</returns>
        public virtual object QueryWithResultSetExtractor(CommandType cmdType, string sql, IResultSetExtractor rse)
        {
            AssertUtils.ArgumentNotNull(sql, "sql", "SQL must not be null");

            //TODO check for parameter placeholders...

            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing SQL [" + sql + "]");
            }

            return Execute(new QueryCallback(this, cmdType, sql, rse, null));

        }

        public virtual object QueryWithResultSetExtractor(CommandType cmdType, string cmdText, IResultSetExtractor resultSetExtractor, ICommandSetter commandSetter)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractor, "resultSetExtractor", "Result Set Extractor must not be null");
            return Execute(new QueryCallbackWithCommandSetter(this, cmdType, cmdText,
                                                              resultSetExtractor, commandSetter));
        }


        public virtual object QueryWithResultSetExtractor(CommandType cmdType, string cmdText, IResultSetExtractor resultSetExtractor,
                                                          string name, Enum dbType, int size, object parameterValue)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractor, "resultSetExtractor", "Result Set Extractor must not be null");
            return Execute(new QueryCallback(this, cmdType, cmdText, resultSetExtractor, CreateDbParameters(name, dbType, size, parameterValue)));
        }

        public virtual object QueryWithResultSetExtractor(CommandType cmdType, string cmdText, IResultSetExtractor resultSetExtractor,
                                                          IDbParameters parameters)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractor, "resultSetExtractor", "Result Set Extractor must not be null");
            return Execute(new QueryCallback(this, cmdType, cmdText, resultSetExtractor, parameters));
        }


        #endregion


        #region Query with ResultSetExtractorDelegate

        /// <summary>
        /// Execute a query given static SQL/Stored Procedure name
        /// and process a single result set with an instance of IResultSetExtractor
        /// </summary>
        /// <param name="cmdType">The type of command.</param>
        /// <param name="sql">The SQL/Stored Procedure to execute</param>
        /// <param name="resultSetExtractorDelegate">Delegate that will extract all rows of a result set</param>
        /// <returns>An arbitrary result object, as returned by the IResultSetExtractor</returns>
        public virtual object QueryWithResultSetExtractorDelegate(CommandType cmdType, string sql, ResultSetExtractorDelegate resultSetExtractorDelegate)
        {
            AssertUtils.ArgumentNotNull(sql, "sql", "SQL must not be null");

            //TODO check for parameter placeholders...

            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing SQL [" + sql + "]");
            }

            return Execute(new QueryCallback(this, cmdType, sql, resultSetExtractorDelegate, null));

        }

        public virtual object QueryWithResultSetExtractorDelegate(CommandType cmdType, string cmdText, ResultSetExtractorDelegate resultSetExtractorDelegate,
                                                          ICommandSetter commandSetter)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractorDelegate, "resultSetExtractorDelegate", "Result set extractor delegate must not be null");
            return Execute(new QueryCallbackWithCommandSetter(this, cmdType, cmdText,
                                                              resultSetExtractorDelegate, commandSetter));
        }


        public virtual object QueryWithResultSetExtractorDelegate(CommandType cmdType, string cmdText, ResultSetExtractorDelegate resultSetExtractorDelegate,
                                                                  string name, Enum dbType, int size, object parameterValue)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractorDelegate, "resultSetExtractorDelegate", "Result set extractor delegate must not be null");
            return Execute(new QueryCallback(this, cmdType, cmdText, resultSetExtractorDelegate, CreateDbParameters(name, dbType, size, parameterValue)));
        }

        public virtual object QueryWithResultSetExtractorDelegate(CommandType cmdType, string cmdText, ResultSetExtractorDelegate resultSetExtractorDelegate,
                                                                  IDbParameters parameters)
        {
            AssertUtils.ArgumentNotNull(resultSetExtractorDelegate, "resultSetExtractorDelegate", "Result set extractor delegate must not be null");
            return Execute(new QueryCallback(this, cmdType, cmdText, resultSetExtractorDelegate, parameters));
        }


        #endregion

        #region Query for Object

        /// <summary>
        /// Execute a query with the specified command text, mapping a single result
        /// row to an object via a RowMapper.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="rowMapper">object that will map one object per row</param>
        /// <returns>The single mapped object.</returns>
        /// <exception cref="Spring.Dao.IncorrectResultSizeDataAccessException">
        /// If the query does not return exactly one row.
        /// </exception>
        /// <exception cref="Spring.Dao.DataAccessException">
        /// If there is any problem executing the query.
        /// </exception>
        public virtual object QueryForObject(CommandType cmdType, string cmdText, IRowMapper rowMapper)
        {
            IList results = QueryWithRowMapper(cmdType, cmdText, rowMapper);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        /// <summary>
        /// Execute a query with the specified command text and parameters set via the
        /// command setter, mapping a single result row to an object via a RowMapper.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="rowMapper">object that will map one object per row</param>
        /// <param name="commandSetter">The command setter.</param>
        /// <returns>The single mapped object.</returns>
        /// <exception cref="Spring.Dao.IncorrectResultSizeDataAccessException">
        /// If the query does not return exactly one row.
        /// </exception>
        /// <exception cref="Spring.Dao.DataAccessException">
        /// If there is any problem executing the query.
        /// </exception>
        public virtual object QueryForObject(CommandType cmdType, string cmdText, IRowMapper rowMapper, ICommandSetter commandSetter)
        {
            IList results = QueryWithRowMapper(cmdType, cmdText, rowMapper, commandSetter);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        /// <summary>
        /// Execute a query with the specified command text and parameters, mapping a single result row
        /// to an object via a RowMapper.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="rowMapper">object that will map one object per row</param>
        /// <param name="parameters">The parameter collection to use in the query.</param>
        /// <returns>The single mapped object.</returns>
        /// <exception cref="Spring.Dao.IncorrectResultSizeDataAccessException">
        /// If the query does not return exactly one row.
        /// </exception>
        /// <exception cref="Spring.Dao.DataAccessException">
        /// If there is any problem executing the query.
        /// </exception>
        public virtual object QueryForObject(CommandType cmdType, string cmdText, IRowMapper rowMapper, IDbParameters parameters)
        {
            IList results = QueryWithRowMapper(cmdType, cmdText, rowMapper, parameters);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        /// <summary>
        /// Execute a query with the specified command text and parameter, mapping a single result row
        /// to an object via a RowMapper.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="rowMapper">object that will map one object per row</param>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the database parameter type enumerations.</param>
        /// <param name="size">The length of the parameter. 0 if not applicable to parameter type.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>The single mapped object.</returns>
        /// <exception cref="Spring.Dao.IncorrectResultSizeDataAccessException">
        /// If the query does not return exactly one row.
        /// </exception>
        /// <exception cref="Spring.Dao.DataAccessException">
        /// If there is any problem executing the query.
        /// </exception>
        public virtual object QueryForObject(CommandType cmdType, string cmdText, IRowMapper rowMapper, string parameterName, Enum dbType, int size,
                                             object parameterValue)
        {
            IList results = QueryWithRowMapper(cmdType, cmdText, rowMapper, parameterName, dbType, size, parameterValue);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        #endregion

        #region Query for ObjectDelegate

        /// <summary>
        /// Execute a query with the specified command text, mapping a single result
        /// row to an object via a RowMapper.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="rowMapperDelegate">delegate that will map one object per row</param>
        /// <returns>The single mapped object.</returns>
        /// <exception cref="Spring.Dao.IncorrectResultSizeDataAccessException">
        /// If the query does not return exactly one row.
        /// </exception>
        /// <exception cref="Spring.Dao.DataAccessException">
        /// If there is any problem executing the query.
        /// </exception>
        public virtual object QueryForObjectDelegate(CommandType cmdType, string cmdText, RowMapperDelegate rowMapperDelegate)
        {
            IList results = QueryWithRowMapperDelegate(cmdType, cmdText, rowMapperDelegate);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }


        /// <summary>
        /// Execute a query with the specified command text and parameters set via the
        /// command setter, mapping a single result row to an object via a RowMapper.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="rowMapperDelegate">delegate that will map one object per row</param>
        /// <param name="commandSetter">The command setter.</param>
        /// <returns>The single mapped object.</returns>
        /// <exception cref="Spring.Dao.IncorrectResultSizeDataAccessException">
        /// If the query does not return exactly one row.
        /// </exception>
        /// <exception cref="Spring.Dao.DataAccessException">
        /// If there is any problem executing the query.
        /// </exception>
        public virtual object QueryForObjectDelegate(CommandType cmdType, string cmdText, RowMapperDelegate rowMapperDelegate, ICommandSetter commandSetter)
        {
            IList results = QueryWithRowMapperDelegate(cmdType, cmdText, rowMapperDelegate, commandSetter);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        /// <summary>
        /// Execute a query with the specified command text and parameters, mapping a single result row
        /// to an object via a RowMapper.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="rowMapperDelegate">delegate that will map one object per row</param>
        /// <param name="parameters">The parameter collection to use in the query.</param>
        /// <returns>The single mapped object.</returns>
        /// <exception cref="Spring.Dao.IncorrectResultSizeDataAccessException">
        /// If the query does not return exactly one row.
        /// </exception>
        /// <exception cref="Spring.Dao.DataAccessException">
        /// If there is any problem executing the query.
        /// </exception>
        public virtual object QueryForObjectDelegate(CommandType cmdType, string cmdText, RowMapperDelegate rowMapperDelegate, IDbParameters parameters)
        {
            IList results = QueryWithRowMapperDelegate(cmdType, cmdText, rowMapperDelegate, parameters);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        /// <summary>
        /// Execute a query with the specified command text and parameter, mapping a single result row
        /// to an object via a RowMapper.
        /// </summary>
        /// <param name="cmdType">The command type.</param>
        /// <param name="cmdText">The command text to execute.</param>
        /// <param name="rowMapperDelegate">delegate that will map one object per row</param>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the database parameter type enumerations.</param>
        /// <param name="size">The length of the parameter. 0 if not applicable to parameter type.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>The single mapped object.</returns>
        /// <exception cref="Spring.Dao.IncorrectResultSizeDataAccessException">
        /// If the query does not return exactly one row.
        /// </exception>
        /// <exception cref="Spring.Dao.DataAccessException">
        /// If there is any problem executing the query.
        /// </exception>
        public virtual object QueryForObjectDelegate(CommandType cmdType, string cmdText, RowMapperDelegate rowMapperDelegate, string parameterName, Enum dbType, int size,
                                                     object parameterValue)
        {
            IList results = QueryWithRowMapperDelegate(cmdType, cmdText, rowMapperDelegate, parameterName, dbType, size, parameterValue);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        #endregion

        #region Query with CommandCreator

        public virtual object QueryWithCommandCreator(IDbCommandCreator cc, IResultSetExtractor rse)
        {
            return QueryWithCommandCreator(cc, rse, null);
        }

        public virtual void QueryWithCommandCreator(IDbCommandCreator cc, IRowCallback rowCallback)
        {
            QueryWithCommandCreator(cc, rowCallback, null);
        }

        public virtual IList QueryWithCommandCreator(IDbCommandCreator cc, IRowMapper rowMapper)
        {
            return QueryWithCommandCreator(cc, rowMapper, null);
        }

        public virtual object QueryWithCommandCreator(IDbCommandCreator cc, IResultSetExtractor rse, IDictionary returnedParameters)
        {
            if (rse == null)
            {
                throw new ArgumentNullException("Result Set Extractor must not be null");
            }

            return Execute(cc, new AdoResultSetExtractorWithOutputParamsCommandCallback(this, rse, returnedParameters));
        }

        public virtual void QueryWithCommandCreator(IDbCommandCreator cc, IRowCallback rowCallback, IDictionary returnedParameters)
        {
            if (rowCallback == null)
            {
                throw new ArgumentNullException("RowCallback must not be null");
            }
            Execute(cc, new AdoRowCallbackCommandCallback(this, rowCallback, returnedParameters));
        }

        public virtual IList QueryWithCommandCreator(IDbCommandCreator cc, IRowMapper rowMapper, IDictionary returnedParameters)
        {
            if (rowMapper == null)
            {
                throw new ArgumentNullException("rowMapper must not be null");
            }

            return (IList)Execute(cc, new AdoRowMapperQueryCommandCallback(this, rowMapper, returnedParameters));
        }




        public IDictionary QueryWithCommandCreator(IDbCommandCreator cc, IList namedResultSetProcessors)
        {
            return (IDictionary)Execute(cc, new AdoResultProcessorsQueryCommandCallback(this, namedResultSetProcessors));
        }

        #endregion

        // DataSet/DataTable related methods.

        #region DataTable Create operations without parameters

        public virtual DataTable DataTableCreate(CommandType commandType, string sql)
        {
            DataTable dataTable = CreateDataTable();
            DataTableFill(dataTable, commandType, sql);
            return dataTable;
        }

        public virtual DataTable DataTableCreate(CommandType commandType, string sql,
                                         string tableMappingName)
        {
            DataTable dataTable = CreateDataTable();
            DataTableFill(dataTable, commandType, sql, tableMappingName);
            return dataTable;
        }

        public virtual DataTable DataTableCreate(CommandType commandType, string sql,
                                                 ITableMapping tableMapping)
        {
            DataTable dataTable = CreateDataTable();
            DataTableFill(dataTable, commandType, sql, tableMapping);
            return dataTable;
        }

        public virtual DataTable DataTableCreate(CommandType commandType, string sql,
                                                 ITableMapping tableMapping,
                                                 IDataAdapterSetter setter)
        {
            DataTable dataTable = CreateDataTable();
            DataTableFill(dataTable, commandType, sql, tableMapping, setter);
            return dataTable;
        }

        #endregion

        #region DataTable Create operations with parameters

        public virtual DataTable DataTableCreateWithParams(CommandType commandType, string sql,
                                                           IDbParameters parameters)
        {
            DataTable dataTable = CreateDataTable();
            DataTableFillWithParams(dataTable, commandType, sql, parameters);
            return dataTable;
        }

        public virtual DataTable DataTableCreateWithParams(CommandType commandType, string sql,
                                                           IDbParameters parameters,
                                                           string tableMappingName)
        {
            DataTable dataTable = CreateDataTable();
            DataTableFillWithParams(dataTable, commandType, sql, parameters, tableMappingName);
            return dataTable;
        }

        public virtual DataTable DataTableCreateWithParams(CommandType commandType, string sql,
                                                           IDbParameters parameters,
                                                           ITableMapping tableMapping)
        {
            DataTable dataTable = CreateDataTable();
            DataTableFillWithParams(dataTable, commandType, sql, parameters, tableMapping);
            return dataTable;
        }

        public virtual DataTable DataTableCreateWithParams(CommandType commandType, string sql,
                                                           IDbParameters parameters,
                                                           ITableMapping tableMapping,
                                                           IDataAdapterSetter dataAdapterSetter)
        {
            DataTable dataTable = CreateDataTable();
            DataTableFillWithParams(dataTable, commandType, sql, parameters, tableMapping, dataAdapterSetter);
            return dataTable;
        }


        #endregion

        #region DataTable Fill operations without parameters
        /// <summary>
        /// Fill a  <see cref="DataTable"/> based on a select command that requires no parameters.
        /// </summary>
        /// <param name="dataTable">The <see cref="DataTable"/> to populate</param>
        /// <param name="commandType">The type of command</param>
        /// <param name="sql">SQL query to execute</param>
        /// <returns>The number of rows successfully added to or refreshed in the  <see cref="DataTable"/></returns>
        public virtual int DataTableFill(DataTable dataTable, CommandType commandType, string sql)
        {
            ValidateFillArguments(dataTable, sql);

            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing DataTableFill " + commandType + "[" + sql + "]");
            }
            #endregion

            ITableMappingCollection mappingCollection = DoCreateMappingCollection(null);
            return (int)Execute(new DataAdapterFillCallback(dataTable,
                                                            commandType, sql,
                                                            mappingCollection, null, null, null));
        }

        public virtual int DataTableFill(DataTable dataTable, CommandType commandType, string sql,
                                         string tableMappingName)
        {
            ValidateFillArguments(dataTable, sql);

            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing DataTableFill " + commandType + "[" + sql + "] with table mapping name " + tableMappingName);
            }
            #endregion

            if (tableMappingName == null)
            {
                tableMappingName = "Table";
            }
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableMappingName });
            return (int)Execute(new DataAdapterFillCallback(dataTable,
                                                            commandType, sql,
                                                            mappingCollection, null, null, null));

        }

        public virtual int DataTableFill(DataTable dataTable, CommandType commandType, string sql,
                                         ITableMapping tableMapping)
        {
            ValidateFillArguments(dataTable, sql, tableMapping);
            ITableMappingCollection mappingCollection = new DataTableMappingCollection();
            mappingCollection.Add((object)tableMapping);

            return (int)Execute(new DataAdapterFillCallback(dataTable,
                                                            commandType, sql,
                                                            mappingCollection, null, null, null));
        }

        public virtual int DataTableFill(DataTable dataTable, CommandType commandType, string sql,
                                         ITableMapping tableMapping,
                                         IDataAdapterSetter setter)
        {
            ValidateFillArguments(dataTable, sql, tableMapping);
            ITableMappingCollection mappingCollection = new DataTableMappingCollection();
            mappingCollection.Add((object)tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataTable,
                                                            commandType, sql,
                                                            mappingCollection, setter, null, null));
        }


        #endregion

        #region DataTable Fill operations with parameters

        public virtual int DataTableFillWithParams(DataTable dataTable, CommandType commandType, string sql,
                                                   IDbParameters parameters)
        {
            ValidateFillWithParameterArguments(dataTable, sql, parameters);
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(null);
            return (int)Execute(new DataAdapterFillCallback(dataTable,
                                                            commandType, sql,
                                                            mappingCollection, null, null,
                                                            parameters));
        }

        public virtual int DataTableFillWithParams(DataTable dataTable, CommandType commandType, string sql,
                                                   IDbParameters parameters,
                                                   string tableMappingName)
        {
            ValidateFillWithParameterArguments(dataTable, sql, parameters);
            if (tableMappingName == null)
            {
                tableMappingName = "Table";
            }
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableMappingName });
            return (int)Execute(new DataAdapterFillCallback(dataTable,
                                                            commandType, sql,
                                                            mappingCollection, null, null, parameters));
        }

        public virtual int DataTableFillWithParams(DataTable dataTable, CommandType commandType, string sql,
                                                   IDbParameters parameters,
                                                   ITableMapping tableMapping)
        {
            ValidateFillWithParameterArguments(dataTable, sql, parameters, tableMapping);
            ITableMappingCollection mappingCollection = new DataTableMappingCollection();
            mappingCollection.Add((object)tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataTable,
                                                            commandType, sql,
                                                            mappingCollection, null, null, parameters));
        }

        public virtual int DataTableFillWithParams(DataTable dataTable, CommandType commandType, string sql,
                                                   IDbParameters parameters,
                                                   ITableMapping tableMapping,
                                                   IDataAdapterSetter dataAdapterSetter)
        {
            ValidateFillWithParameterArguments(dataTable, sql, parameters, tableMapping);
            ITableMappingCollection mappingCollection = new DataTableMappingCollection();
            mappingCollection.Add((object)tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataTable,
                                                            commandType, sql,
                                                            mappingCollection, dataAdapterSetter, null, parameters));
        }


        #endregion

        #region DataTable Update operations
        //TODO conflict options...

        public virtual int DataTableUpdateWithCommandBuilder(DataTable dataTable,
                                                             CommandType commandType,
                                                             string selectSql,
                                                             IDbParameters parameters,
                                                             string tableName)
        {
            ValidateUpdateWithCommandBuilderArguments(dataTable, tableName, selectSql);
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableName });
            return (int)Execute(new DataAdapterUpdateWithCommandBuilderCallback(dataTable,
                                                                                DbProvider.CreateCommandBuilder(),
                                                                                mappingCollection,
                                                                                commandType,
                                                                                selectSql,
                                                                                parameters,
                                                                                null));
        }

        public virtual int DataTableUpdateWithCommandBuilder(DataTable dataTable,
                                                             CommandType commandType,
                                                             string selectSql,
                                                             IDbParameters parameters,
                                                             string tableName,
                                                             IDataAdapterSetter dataAdapterSetter)
        {
            ValidateUpdateWithCommandBuilderArguments(dataTable, tableName, selectSql);
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableName });
            return (int)Execute(new DataAdapterUpdateWithCommandBuilderCallback(dataTable,
                                                                                DbProvider.CreateCommandBuilder(),
                                                                                mappingCollection,
                                                                                commandType,
                                                                                selectSql,
                                                                                parameters,
                                                                                dataAdapterSetter));
        }


        public virtual int DataTableUpdateWithCommandBuilder(DataTable dataTable,
                                                             CommandType commandType,
                                                             string selectSql,
                                                             IDbParameters parameters,
                                                             ITableMapping tableMapping,
                                                             IDataAdapterSetter dataAdapterSetter)
        {
            ValidateUpdateWithCommandBuilderArguments(dataTable, tableMapping, selectSql);
            ITableMappingCollection mappingCollection = new DataTableMappingCollection();
            mappingCollection.Add(tableMapping);
            return (int)Execute(new DataAdapterUpdateWithCommandBuilderCallback(dataTable,
                                                                                DbProvider.CreateCommandBuilder(),
                                                                                mappingCollection,
                                                                                commandType,
                                                                                selectSql,
                                                                                parameters,
                                                                                dataAdapterSetter));
        }



        public virtual int DataTableUpdate(DataTable dataTable,
                                           string tableName,
                                           CommandType insertCommandtype, string insertSql, IDbParameters insertParameters,
                                           CommandType updateCommandtype, string updateSql, IDbParameters updateParameters,
                                           CommandType deleteCommandtype, string deleteSql, IDbParameters deleteParameters)
        {
            ValidateUpdateArguments(dataTable, tableName);
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableName });
            return DataTableUpdate(dataTable, mappingCollection,
                                   insertCommandtype, insertSql, insertParameters,
                                   updateCommandtype, updateSql, updateParameters,
                                   deleteCommandtype, deleteSql, deleteParameters,
                                   null);

        }

        public virtual int DataTableUpdate(DataTable dataTable,
                                           string tableName,
                                           CommandType insertCommandtype, string insertSql, IDbParameters insertParameters,
                                           CommandType updateCommandtype, string updateSql, IDbParameters updateParameters,
                                           CommandType deleteCommandtype, string deleteSql, IDbParameters deleteParameters,
                                           IDataAdapterSetter dataAdapterSetter)
        {
            ValidateUpdateArguments(dataTable, tableName);
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableName });
            return DataTableUpdate(dataTable, mappingCollection,
                                   insertCommandtype, insertSql, insertParameters,
                                   updateCommandtype, updateSql, updateParameters,
                                   deleteCommandtype, deleteSql, deleteParameters,
                                   dataAdapterSetter);
        }

        public virtual int DataTableUpdate(DataTable dataTable,
                                           ITableMapping tableMapping,
                                           CommandType insertCommandtype, string insertSql, IDbParameters insertParameters,
                                           CommandType updateCommandtype, string updateSql, IDbParameters updateParameters,
                                           CommandType deleteCommandtype, string deleteSql, IDbParameters deleteParameters,
                                           IDataAdapterSetter dataAdapterSetter)
        {
            ValidateUpdateArguments(dataTable, tableMapping);
            ITableMappingCollection mappingCollection = new DataTableMappingCollection();

            mappingCollection.Add((object)tableMapping);

            return DataTableUpdate(dataTable, mappingCollection,
                                   insertCommandtype, insertSql, insertParameters,
                                   updateCommandtype, updateSql, updateParameters,
                                   deleteCommandtype, deleteSql, deleteParameters,
                                   dataAdapterSetter);


        }



        #endregion

        #region DataSet Create operations without parameters

        public virtual DataSet DataSetCreate(CommandType commandType, string sql)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFill(dataSet, commandType, sql);
            return dataSet;
        }


        public virtual DataSet DataSetCreate(CommandType commandType, string sql,
                                             string[] tableNames)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFill(dataSet, commandType, sql, tableNames);
            return dataSet;
        }

        public virtual DataSet DataSetCreate(CommandType commandType, string sql,
                                             ITableMappingCollection tableMapping)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFill(dataSet, commandType, sql, tableMapping);
            return dataSet;
        }

        public virtual DataSet DataSetCreate(CommandType commandType, string sql,
                                             ITableMappingCollection tableMapping,
                                             IDataAdapterSetter setter)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFill(dataSet, commandType, sql, tableMapping, setter);
            return dataSet;
        }

        public virtual DataSet DataSetCreate(CommandType commandType, string sql,
                                             ITableMappingCollection tableMapping,
                                             IDataAdapterSetter setter,
                                             IDataSetFillLifecycleProcessor fillLifecycleProcessor)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFill(dataSet, commandType, sql, tableMapping, setter, fillLifecycleProcessor);
            return dataSet;
        }
        #endregion

        #region DataSet Create operations with parameters

        public virtual DataSet DataSetCreateWithParams(CommandType commandType, string sql,
                                                       IDbParameters parameters)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFillWithParameters(dataSet, commandType, sql, parameters);
            return dataSet;
        }

        public virtual DataSet DataSetCreateWithParams(CommandType commandType, string sql,
                                                       IDbParameters parameters,
                                                       string[] tableNames)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFillWithParameters(dataSet, commandType, sql, parameters, tableNames);
            return dataSet;
        }

        public virtual DataSet DataSetCreateWithParams(CommandType commandType, string sql,
                                                       IDbParameters parameters,
                                                       ITableMappingCollection tableMapping)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFillWithParameters(dataSet, commandType, sql, parameters, tableMapping);
            return dataSet;
        }

        public virtual DataSet DataSetCreateWithParams(CommandType commandType, string sql,
                                                       IDbParameters parameters,
                                                       ITableMappingCollection tableMapping,
                                                       IDataAdapterSetter dataAdapterSetter)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFillWithParameters(dataSet, commandType, sql, parameters, tableMapping, dataAdapterSetter);
            return dataSet;
        }

        public virtual DataSet DataSetCreateWithParams(CommandType commandType, string sql,
                                                       IDbParameters parameters,
                                                       ITableMappingCollection tableMapping,
                                                       IDataAdapterSetter dataAdapterSetter,
                                                       IDataSetFillLifecycleProcessor fillLifecycleProcessor)
        {
            DataSet dataSet = CreateDataSet();
            DataSetFillWithParameters(dataSet, commandType, sql, parameters, tableMapping, dataAdapterSetter, fillLifecycleProcessor);
            return dataSet;
        }

        #endregion

        #region DataSet Fill operations without parameters

        public virtual int DataSetFill(DataSet dataSet, CommandType commandType, string sql)
        {
            ValidateFillArguments(dataSet, sql);

            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing DataSetFill " + commandType + "[" + sql + "]");
            }
            #endregion


            ITableMappingCollection mappingCollection = DoCreateMappingCollection(null);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            mappingCollection, null, null, null));
        }



        public virtual int DataSetFill(DataSet dataSet, CommandType commandType, string sql, string[] tableNames)
        {
            ValidateFillArguments(dataSet, sql);

            #region Instrumentation
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Executing DataSetFill " + commandType + "[" + sql + "] with table names " + tableNames);
            }
            #endregion

            if (tableNames == null)
            {
                tableNames = new string[] { "Table" };
            }
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(tableNames);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            mappingCollection, null, null, null));
        }


        public virtual int DataSetFill(DataSet dataSet, CommandType commandType, string sql,
                                       ITableMappingCollection tableMapping)
        {
            ValidateFillArguments(dataSet, sql, tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            tableMapping, null, null, null));

        }

        public virtual int DataSetFill(DataSet dataSet, CommandType commandType, string sql,
                                       ITableMappingCollection tableMapping,
                                       IDataAdapterSetter setter)
        {
            ValidateFillArguments(dataSet, sql, tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            tableMapping, setter, null, null));

        }

        public virtual int DataSetFill(DataSet dataSet, CommandType commandType, string sql,
                                       ITableMappingCollection tableMapping,
                                       IDataAdapterSetter setter,
                                       IDataSetFillLifecycleProcessor fillLifecycleProcessor)
        {
            ValidateFillArguments(dataSet, sql, tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            tableMapping, setter, fillLifecycleProcessor, null));

        }

        #endregion

        #region DataSet Fill operations with parameters

        public virtual int DataSetFillWithParameters(DataSet dataSet, CommandType commandType, string sql,
                                                     IDbParameters parameters)
        {
            ValidateFillWithParameterArguments(dataSet, sql, parameters);
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(null);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            mappingCollection, null, null,
                                                            parameters));
        }

        public virtual int DataSetFillWithParameters(DataSet dataSet, CommandType commandType, string sql,
                                                     IDbParameters parameters,
                                                     string[] tableNames)
        {
            ValidateFillWithParameterArguments(dataSet, sql, parameters);
            if (tableNames == null)
            {
                tableNames = new string[] { "Table" };
            }
            ITableMappingCollection tableMapping = DoCreateMappingCollection(tableNames);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            tableMapping, null, null, parameters));

        }

        public virtual int DataSetFillWithParameters(DataSet dataSet, CommandType commandType, string sql,
                                                     IDbParameters parameters,
                                                     ITableMappingCollection tableMapping)
        {
            ValidateFillWithParameterArguments(dataSet, sql, parameters, tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            tableMapping, null, null, parameters));
        }

        public virtual int DataSetFillWithParameters(DataSet dataSet, CommandType commandType, string sql,
                                                     IDbParameters parameters,
                                                     ITableMappingCollection tableMapping,
                                                     IDataAdapterSetter dataAdapterSetter)
        {
            ValidateFillWithParameterArguments(dataSet, sql, parameters, tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            tableMapping, dataAdapterSetter, null, parameters));
        }

        public virtual int DataSetFillWithParameters(DataSet dataSet, CommandType commandType, string sql,
                                                     IDbParameters parameters,
                                                     ITableMappingCollection tableMapping,
                                                     IDataAdapterSetter dataAdapterSetter,
                                                     IDataSetFillLifecycleProcessor fillLifecycleProcessor)
        {
            ValidateFillWithParameterArguments(dataSet, sql, parameters, tableMapping);
            return (int)Execute(new DataAdapterFillCallback(dataSet,
                                                            commandType, sql,
                                                            tableMapping, dataAdapterSetter, fillLifecycleProcessor, parameters));
        }

        #endregion

        #region DataSet Update operations
        public virtual int DataSetUpdateWithCommandBuilder(DataSet dataSet,
                                                           CommandType commandType,
                                                           string selectSql,
                                                           IDbParameters selectParameters,
                                                           string tableName)
        {
            ValidateUpdateWithCommandBuilderArguments(dataSet, tableName, selectSql);
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableName });
            return (int)Execute(new DataAdapterUpdateWithCommandBuilderCallback(dataSet,
                                                                                DbProvider.CreateCommandBuilder(),
                                                                                mappingCollection,
                                                                                commandType,
                                                                                selectSql,
                                                                                selectParameters,
                                                                                null));
        }

        public virtual int DataSetUpdateWithCommandBuilder(DataSet dataSet,
                                                           CommandType commandType,
                                                           string selectSql,
                                                           IDbParameters selectParameters,
                                                           string tableName,
                                                           IDataAdapterSetter dataAdapterSetter)
        {
            ValidateUpdateWithCommandBuilderArguments(dataSet, tableName, selectSql);
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableName });
            return (int)Execute(new DataAdapterUpdateWithCommandBuilderCallback(dataSet,
                                                                                DbProvider.CreateCommandBuilder(),
                                                                                mappingCollection,
                                                                                commandType,
                                                                                selectSql,
                                                                                selectParameters,
                                                                                dataAdapterSetter));
        }

        public virtual int DataSetUpdateWithCommandBuilder(DataSet dataSet,
                                                           CommandType commandType,
                                                           string selectSql,
                                                           IDbParameters selectParameters,
                                                           ITableMappingCollection mappingCollection,
                                                           IDataAdapterSetter dataAdapterSetter)
        {
            ValidateUpdateWithCommandBuilderArguments(dataSet, mappingCollection, selectSql);
            return (int)Execute(new DataAdapterUpdateWithCommandBuilderCallback(dataSet,
                                                                                DbProvider.CreateCommandBuilder(),
                                                                                mappingCollection,
                                                                                commandType,
                                                                                selectSql,
                                                                                selectParameters,
                                                                                dataAdapterSetter));
        }

        public virtual int DataSetUpdate(DataSet dataSet,
                                         string tableName,
                                         IDbCommand insertCommand,
                                         IDbCommand updateCommand,
                                         IDbCommand deleteCommand)
        {
            ValidateUpdateArguments(dataSet, tableName);

            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableName });

            return (int)Execute(new DataAdapterUpdateCallback(dataSet,
                                                              mappingCollection,
                                                              insertCommand,
                                                              updateCommand,
                                                              deleteCommand,
                                                              null));
        }

        public virtual int DataSetUpdate(DataSet dataSet,
                                         string tableName,
                                         CommandType insertCommandtype, string insertSql, IDbParameters insertParameters,
                                         CommandType updateCommandtype, string updateSql, IDbParameters updateParameters,
                                         CommandType deleteCommandtype, string deleteSql, IDbParameters deleteParameters)
        {
            ValidateUpdateArguments(dataSet, tableName);
            IDbCommand insertCommand = null;
            if (insertSql != null)
            {
                insertCommand = DbProvider.CreateCommand();
                insertCommand.CommandType = insertCommandtype;
                insertCommand.CommandText = insertSql;
                ParameterUtils.CopyParameters(insertCommand, insertParameters);
            }
            IDbCommand updateCommand = null;
            if (updateSql != null)
            {
                updateCommand = DbProvider.CreateCommand();
                updateCommand.CommandType = updateCommandtype;
                updateCommand.CommandText = updateSql;
                ParameterUtils.CopyParameters(updateCommand, updateParameters);
            }
            IDbCommand deleteCommand = null;
            if (deleteSql != null)
            {
                deleteCommand = DbProvider.CreateCommand();
                deleteCommand.CommandType = deleteCommandtype;
                deleteCommand.CommandText = deleteSql;
                ParameterUtils.CopyParameters(deleteCommand, deleteParameters);
            }
            ITableMappingCollection mappingCollection = DoCreateMappingCollection(new string[] { tableName });

            int returnVal = (int)Execute(new DataAdapterUpdateCallback(dataSet, mappingCollection,
                                                                       insertCommand, updateCommand, deleteCommand, null));

            if (insertSql != null)
            {
                ParameterUtils.CopyParameters(insertParameters, insertCommand);
            }
            if (updateSql != null)
            {
                ParameterUtils.CopyParameters(updateParameters, updateCommand);
            }
            if (deleteSql != null)
            {
                ParameterUtils.CopyParameters(deleteParameters, deleteCommand);
            }
            return returnVal;
        }



        public virtual int DataSetUpdate(DataSet dataSet,
                                         string tableName,
                                         IDbCommand insertCommand,
                                         IDbCommand updateCommand,
                                         IDbCommand deleteCommand,
                                         IDataAdapterSetter dataAdapterSetter)
        {
            ValidateUpdateArguments(dataSet, tableName);
            ITableMappingCollection tableMapping = DoCreateMappingCollection(new string[] { tableName });
            return (int)Execute(new DataAdapterUpdateCallback(dataSet,
                                                              tableMapping,
                                                              insertCommand,
                                                              updateCommand,
                                                              deleteCommand,
                                                              dataAdapterSetter));
        }


        public virtual int DataSetUpdate(DataSet dataSet,
                                         ITableMappingCollection tableMapping,
                                         IDbCommand insertCommand,
                                         IDbCommand updateCommand,
                                         IDbCommand deleteCommand)
        {
            ValidateUpdateArguments(dataSet, tableMapping);
            return (int)Execute(new DataAdapterUpdateCallback(dataSet,
                                                              tableMapping,
                                                              insertCommand,
                                                              updateCommand,
                                                              deleteCommand,
                                                              null));
        }

        public virtual int DataSetUpdate(DataSet dataSet,
                                         ITableMappingCollection tableMapping,
                                         IDbCommand insertCommand,
                                         IDbCommand updateCommand,
                                         IDbCommand deleteCommand,
                                         IDataAdapterSetter dataAdapterSetter)
        {
            ValidateUpdateArguments(dataSet, tableMapping);
            return (int)Execute(new DataAdapterUpdateCallback(dataSet,
                                                              tableMapping,
                                                              insertCommand,
                                                              updateCommand,
                                                              deleteCommand,
                                                              dataAdapterSetter));
        }

        #endregion

        #region Parameter Creation Helper Methods

        public override IDataParameter[] DeriveParameters(string procedureName, bool includeReturnParameter)
        {
            return (IDataParameter[])Execute(new DeriveParametersCommandCallback(DbProvider, procedureName, includeReturnParameter));

        }

        #endregion

        #region Private Helper Methods

        private IDbParameters CreateDbParameters(IDbDataParameter parameter)
        {
            IDbParameters parameters = new DbParameters(DbProvider);
            parameters.AddParameter(parameter);
            return parameters;
        }

        private IDbParameters CreateDbParameters(IList parameterList)
        {
            IDbParameters parameters = null;
            if (parameterList != null)
            {
                parameters = new DbParameters(DbProvider);
                foreach (IDbDataParameter parameter in parameterList)
                {
                    parameters.AddParameter(parameter);
                }
            }
            return parameters;
        }

        private IDbParameters CreateDbParameters(object[] parameterValues)
        {
            IDbParameters parameters = null;
            if (parameterValues != null)
            {
                parameters = new DbParameters(DbProvider);
                foreach (object parameterValue in parameterValues)
                {
                    parameters.Add(parameterValue);
                }
            }
            return parameters;
        }

        public int DataTableUpdate(DataTable dataTable,
                                   ITableMappingCollection mappingCollection,
                                   CommandType insertCommandtype, string insertSql, IDbParameters insertParameters,
                                   CommandType updateCommandtype, string updateSql, IDbParameters updateParameters,
                                   CommandType deleteCommandtype, string deleteSql, IDbParameters deleteParameters,
                                   IDataAdapterSetter dataAdapterSetter)
        {
            //TODO - refactor to remove cut-n-pasted code.
            IDbCommand insertCommand = null;
            if (insertSql != null)
            {
                insertCommand = DbProvider.CreateCommand();
                insertCommand.CommandType = insertCommandtype;
                insertCommand.CommandText = insertSql;
                ParameterUtils.CopyParameters(insertCommand, insertParameters);
            }
            IDbCommand updateCommand = null;
            if (updateSql != null)
            {
                updateCommand = DbProvider.CreateCommand();
                updateCommand.CommandType = updateCommandtype;
                updateCommand.CommandText = updateSql;
                ParameterUtils.CopyParameters(updateCommand, updateParameters);
            }
            IDbCommand deleteCommand = null;
            if (deleteSql != null)
            {
                deleteCommand = DbProvider.CreateCommand();
                deleteCommand.CommandType = deleteCommandtype;
                deleteCommand.CommandText = deleteSql;
                ParameterUtils.CopyParameters(deleteCommand, deleteParameters);
            }

            int returnVal = (int)Execute(new DataAdapterUpdateCallback(dataTable, mappingCollection,
                                                                       insertCommand, updateCommand, deleteCommand, null));

            if (insertSql != null)
            {
                ParameterUtils.CopyParameters(insertParameters, insertCommand);
            }
            if (updateSql != null)
            {
                ParameterUtils.CopyParameters(updateParameters, updateCommand);
            }
            if (deleteSql != null)
            {
                ParameterUtils.CopyParameters(deleteParameters, deleteCommand);
            }
            return returnVal;
        }
        #endregion

        #region Protected Helper Methods

        protected virtual void InitExceptionTranslator()
        {
            if (exceptionTranslator == null)
            {
                IDbProvider provider = DbProvider;
                if (provider != null)
                {
                    exceptionTranslator = new ErrorCodeExceptionTranslator(provider);
                }
                else
                {
                    exceptionTranslator = new FallbackExceptionTranslator();
                }
            }
        }

        #endregion

        #region Protected DataAdapter Helper methods

        protected virtual DataTable CreateDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Locale = CultureInfo.InvariantCulture;
            return dataTable;
        }

        protected virtual DataSet CreateDataSet()
        {
            DataSet dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;
            return dataSet;
        }

        protected virtual void ValidateFillArguments(DataTable dataTable, string sql)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable", "DataTable argument can not be null");
            }
            if (sql == null)
            {
                throw new ArgumentNullException("sql", "SQL for DataSet Fill operation can not be null");
            }
        }
        protected virtual void ValidateFillArguments(DataSet dataSet, string sql)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet", "DataSet argument can not be null");
            }
            if (sql == null)
            {
                throw new ArgumentNullException("sql", "SQL for DataSet Fill operation can not be null");
            }
        }
        protected virtual void ValidateFillArguments(DataTable dataTable, string sql,
                                                     ITableMapping tableMapping)
        {
            ValidateFillArguments(dataTable, sql);
            if (tableMapping == null)
            {
                throw new ArgumentNullException("tableMapping", "ITableMapping for DataTable Fill operations can not be null");
            }
        }

        protected virtual void ValidateFillArguments(DataSet dataSet, string sql,
                                                     ITableMappingCollection tableMappingCollection)
        {
            ValidateFillArguments(dataSet, sql);
            if (tableMappingCollection == null)
            {
                throw new ArgumentNullException("tableMappingCollection", "ITableMappingCollection for DataSet Fill operations can not be null");
            }
        }
        protected virtual void ValidateFillWithParameterArguments(DataTable dataTable, string sql, IDbParameters parameters)
        {
            ValidateFillArguments(dataTable, sql);
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters", "IDbParameters for DataTable Fill operations can not be null");
            }
        }
        protected virtual void ValidateFillWithParameterArguments(DataSet dataSet, string sql, IDbParameters parameters)
        {
            ValidateFillArguments(dataSet, sql);
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters", "IDbParameters for DataSet Fill operations can not be null");
            }
        }
        protected virtual void ValidateFillWithParameterArguments(DataTable dataTable, string sql, IDbParameters parameters, ITableMapping tableMapping)
        {
            ValidateFillWithParameterArguments(dataTable, sql, parameters);
            if (tableMapping == null)
            {
                throw new ArgumentNullException("tableMapping", "ITableMappingCollection for DataTable Fill operations can not be null");
            }
        }
        protected virtual void ValidateFillWithParameterArguments(DataSet dataSet, string sql, IDbParameters parameters, ITableMappingCollection tableMapping)
        {
            ValidateFillWithParameterArguments(dataSet, sql, parameters);
            if (tableMapping == null)
            {
                throw new ArgumentNullException("tableMapping", "ITableMappingCollection for DataSet Fill operations can not be null");
            }
        }

        protected virtual void ValidateUpdateArguments(DataSet dataSet, ITableMappingCollection tableMapping)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet", "DataSet argument can not be null for update operation");
            }
            if (tableMapping == null)
            {
                throw new ArgumentNullException("tableMapping", "TableMappings for DataSet Update operation can not be null");
            }
        }

        protected virtual void ValidateUpdateArguments(DataSet dataSet, string tableName)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet", "DataSet argument can not be null for update operation");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName", "TableName for DataSet Update operation can not be null");
            }
        }

        protected virtual void ValidateUpdateArguments(DataTable dataTable, ITableMapping tableMapping)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable", "DataTable argument can not be null for DataTable update operation");
            }
            if (tableMapping == null)
            {
                throw new ArgumentNullException("tableMapping", "TableMapping for DataTable Update operation can not be null");
            }
        }
        protected virtual void ValidateUpdateArguments(DataTable dataTable, string tableName)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable", "DataTable argument can not be null for DataTable update operation");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName", "TableName for DataTable Update operation can not be null");
            }
        }
        protected virtual void ValidateUpdateWithCommandBuilderArguments(DataSet dataSet, ITableMappingCollection tableMapping, string selectSql)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet", "DataSet can not be null for update operation");
            }
            if (tableMapping == null)
            {
                throw new ArgumentNullException("tableMapping", "TableMapping for DataSet Update operation can not be null");
            }
            if (selectSql == null)
            {
                throw new ArgumentNullException("selectSql", "SelectSql for DataSet Update operations can not be null");
            }
        }

        protected virtual void ValidateUpdateWithCommandBuilderArguments(DataTable dataTable, string tableName, string selectSql)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable", "DataTable can not be null for update operation");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName", "TableName for DataSet Update operation can not be null");
            }
            if (selectSql == null)
            {
                throw new ArgumentNullException("selectSql", "SelectSql for DataSet Update operations can not be null");
            }
        }

        protected virtual void ValidateUpdateWithCommandBuilderArguments(DataTable dataTable, ITableMapping tableMapping, string selectSql)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable", "DataTable can not be null for update operation");
            }
            if (tableMapping == null)
            {
                throw new ArgumentNullException("tableMapping", "TableMapping for DataSet Update operation can not be null");
            }
            if (selectSql == null)
            {
                throw new ArgumentNullException("selectSql", "SelectSql for DataSet Update operations can not be null");
            }
        }
        protected virtual void ValidateUpdateWithCommandBuilderArguments(DataSet dataSet, string tableName, string selectSql)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet", "DataSet can not be null for update operation");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName", "TableName for DataSet Update operation can not be null");
            }
            if (selectSql == null)
            {
                throw new ArgumentNullException("selectSql", "SelectSql for DataSet Update operations can not be null");
            }
        }


        protected virtual ITableMappingCollection DoCreateMappingCollection(string[] dataSetTableNames)
        {
            DataTableMappingCollection mappingCollection;

            if (dataSetTableNames == null)
            {
                dataSetTableNames = new string[] { "Table" };
            }
            foreach (string tableName in dataSetTableNames)
            {
                if (StringUtils.IsNullOrEmpty(tableName))
                {
                    throw new ArgumentException("TableName for DataTable mapping can not be null or empty");
                }
            }
            mappingCollection = new DataTableMappingCollection();
            int counter = 0;
            bool isFirstTable = true;
            foreach (string dataSetTableName in dataSetTableNames)
            {
                string sourceTableName;
                if (isFirstTable)
                {
                    sourceTableName = "Table";
                    isFirstTable = false;
                }
                else
                {
                    sourceTableName = "Table" + ++counter;
                }
                mappingCollection.Add(sourceTableName, dataSetTableName);
            }
            return mappingCollection;
        }

        #endregion

        #region Private DataAdapter Helper callbacks



        private class DataAdapterFillCallback : IDataAdapterCallback
        {
            private bool containsDataSet;
            private DataSet dataSet;
            private DataTable dataTable;
            private CommandType commandType;
            private string sql;
            private ITableMappingCollection mappingCollection;
            private IDataAdapterSetter dataAdapterSetter;
            private IDataSetFillLifecycleProcessor fillLifecycleProcessor;
            private IDbParameters parameters;

            public DataAdapterFillCallback(DataSet dataSet,
                                           CommandType commandType,
                                           string sql,
                                           ITableMappingCollection mappingCollection,
                                           IDataAdapterSetter dataAdapterSetter,
                                           IDataSetFillLifecycleProcessor fillLifecycleProcessor,
                                           IDbParameters parameters)
            {
                containsDataSet = true;
                this.dataSet = dataSet;
                this.commandType = commandType;
                this.sql = sql;
                this.mappingCollection = mappingCollection;
                this.dataAdapterSetter = dataAdapterSetter;
                this.fillLifecycleProcessor = fillLifecycleProcessor;
                this.parameters = parameters;
            }

            public DataAdapterFillCallback(DataTable dataTable,
                                           CommandType commandType,
                                           string sql,
                                           ITableMappingCollection mappingCollection,
                                           IDataAdapterSetter dataAdapterSetter,
                                           IDataSetFillLifecycleProcessor fillLifecycleProcessor,
                                           IDbParameters parameters)
            {
                containsDataSet = false;
                this.dataTable = dataTable;
                this.commandType = commandType;
                this.sql = sql;
                this.mappingCollection = mappingCollection;
                this.dataAdapterSetter = dataAdapterSetter;
                this.fillLifecycleProcessor = fillLifecycleProcessor;
                this.parameters = parameters;
            }



            public object DoInDataAdapter(IDbDataAdapter dataAdapter)
            {
                dataAdapter.SelectCommand.CommandType = commandType;
                dataAdapter.SelectCommand.CommandText = sql;
                //TODO investigate performance of cloning....would need to change signature to
                //     DataTableMapping[] otherwise...
                foreach (DataTableMapping dataTableMapping in mappingCollection)
                {
                    dataAdapter.TableMappings.Add(((ICloneable)dataTableMapping).Clone());
                }

                ParameterUtils.CopyParameters(dataAdapter.SelectCommand, parameters);

                //TODO Review these lifecycle hooks...
                if (dataAdapterSetter != null)
                {
                    dataAdapterSetter.SetValues(dataAdapter);
                }
                if (fillLifecycleProcessor != null)
                {
                    fillLifecycleProcessor.BeforeFill(dataSet, dataAdapter.TableMappings);
                }

                int returnVal;
                if (containsDataSet)
                {
                    returnVal = dataAdapter.Fill(dataSet);
                }
                else
                {
                    //TODO should query metadata to see if supports filling dataTable directly.
                    if (dataAdapter is DbDataAdapter)
                    {
                        returnVal = ((DbDataAdapter)dataAdapter).Fill(dataTable);
                    }
                    else
                    {
                        //TODO could create DataSet and extract DataTable... for now just throw
                        throw new DataException("Provider does not support filling DataTable directly");
                    }
                }

                ParameterUtils.CopyParameters(parameters, dataAdapter.SelectCommand);

                if (fillLifecycleProcessor != null)
                {
                    fillLifecycleProcessor.AfterFill(dataSet, dataAdapter.TableMappings);
                }
                return returnVal;
            }
        }


        private class DataAdapterUpdateCallback : IDataAdapterCallback
        {
            private bool containsDataSet;
            private DataSet dataSet;
            private DataTable dataTable;
            private ITableMappingCollection mappingCollection;
            private IDbCommand insertCommand;
            private IDbCommand updateCommand;
            private IDbCommand deleteCommand;
            private IDataAdapterSetter dataAdapterSetter;

            public DataAdapterUpdateCallback(DataSet dataSet,
                                             ITableMappingCollection mappingCollection,
                                             IDbCommand insertCommand,
                                             IDbCommand updateCommand,
                                             IDbCommand deleteCommand,
                                             IDataAdapterSetter dataAdapterSetter)
            {
                containsDataSet = true;
                this.dataSet = dataSet;
                this.mappingCollection = mappingCollection;
                this.insertCommand = insertCommand;
                this.updateCommand = updateCommand;
                this.deleteCommand = deleteCommand;
                this.dataAdapterSetter = dataAdapterSetter;
            }

            public DataAdapterUpdateCallback(DataTable dataTable,
                                             ITableMappingCollection mappingCollection,
                                             IDbCommand insertCommand,
                                             IDbCommand updateCommand,
                                             IDbCommand deleteCommand,
                                             IDataAdapterSetter dataAdapterSetter)
            {
                containsDataSet = false;
                this.dataTable = dataTable;
                this.mappingCollection = mappingCollection;
                this.insertCommand = insertCommand;
                this.updateCommand = updateCommand;
                this.deleteCommand = deleteCommand;
                this.dataAdapterSetter = dataAdapterSetter;
            }
            #region IDataAdapterCallback Members

            public object DoInDataAdapter(IDbDataAdapter dataAdapter)
            {
                //TODO - did not make copies of parameters...
                if (insertCommand == null && updateCommand == null && deleteCommand == null)
                {
                    throw new ArgumentException("All commands for DataSet Update operation are null");
                }

                if (insertCommand != null)
                {
                    dataAdapter.InsertCommand = insertCommand;
                    ApplyConnectionAndTx(dataAdapter.InsertCommand, dataAdapter.SelectCommand);
                }
                if (updateCommand != null)
                {
                    dataAdapter.UpdateCommand = updateCommand;
                    ApplyConnectionAndTx(dataAdapter.UpdateCommand, dataAdapter.SelectCommand);
                }
                if (deleteCommand != null)
                {
                    dataAdapter.DeleteCommand = deleteCommand;
                    ApplyConnectionAndTx(dataAdapter.DeleteCommand, dataAdapter.SelectCommand);
                }
                foreach (DataTableMapping dataTableMapping in mappingCollection)
                {
                    dataAdapter.TableMappings.Add(((ICloneable)dataTableMapping).Clone());
                }

                if (dataAdapterSetter != null)
                {
                    dataAdapterSetter.SetValues(dataAdapter);
                }

                if (containsDataSet)
                {
                    return dataAdapter.Update(dataSet);
                }
                else
                {
                    //TODO should query metadata to see if supports filling dataTable directly.
                    if (dataAdapter is DbDataAdapter)
                    {
                        return ((DbDataAdapter)dataAdapter).Update(dataTable);
                    }
                    else
                    {
                        //TODO could create DataSet and extract DataTable... for now just throw
                        throw new DataException("Provider does not support filling DataTable directly");
                    }
                }
            }

            private static void ApplyConnectionAndTx(IDbCommand dbCommand, IDbCommand sourceCommand)
            {
                dbCommand.Connection = sourceCommand.Connection;
                dbCommand.Transaction = sourceCommand.Transaction;
            }

            #endregion

        }

        private class DataAdapterUpdateWithCommandBuilderCallback : IDataAdapterCallback
        {
            private bool containsDataSet;
            private DataSet dataSet;
            private DataTable dataTable;
            private object commandBuilder;
            private ITableMappingCollection mappingCollection;
            private CommandType selectCommandType;
            private string selectSql;
            private IDbParameters selectParameters;
            private IDataAdapterSetter dataAdapterSetter;

            public DataAdapterUpdateWithCommandBuilderCallback(DataSet dataSet,
                                                               object commandBuilder,
                                                               ITableMappingCollection mappingCollection,
                                                               CommandType selectCommandType,
                                                               string selectSql,
                                                               IDbParameters selectParameters,
                                                               IDataAdapterSetter dataAdapterSetter)
            {
                containsDataSet = true;
                this.dataSet = dataSet;
                this.commandBuilder = commandBuilder;
                this.mappingCollection = mappingCollection;
                this.selectCommandType = selectCommandType;
                this.selectSql = selectSql;
                this.selectParameters = selectParameters;
                this.dataAdapterSetter = dataAdapterSetter;
            }

            public DataAdapterUpdateWithCommandBuilderCallback(DataTable dataTable,
                                                               object commandBuilder,
                                                               ITableMappingCollection mappingCollection,
                                                               CommandType selectCommandType,
                                                               string selectSql,
                                                               IDbParameters selectParameters,
                                                               IDataAdapterSetter dataAdapterSetter)
            {
                containsDataSet = false;
                this.dataTable = dataTable;
                this.commandBuilder = commandBuilder;
                this.mappingCollection = mappingCollection;
                this.selectCommandType = selectCommandType;
                this.selectSql = selectSql;
                this.selectParameters = selectParameters;
                this.dataAdapterSetter = dataAdapterSetter;
            }
            #region IDataAdapterCallback Members

            public object DoInDataAdapter(IDbDataAdapter dataAdapter)
            {
                dataAdapter.SelectCommand.CommandType = selectCommandType;
                dataAdapter.SelectCommand.CommandText = selectSql;
                ParameterUtils.CopyParameters(dataAdapter.SelectCommand, selectParameters);


                foreach (DataTableMapping dataTableMapping in mappingCollection)
                {
                    dataAdapter.TableMappings.Add(((ICloneable)dataTableMapping).Clone());
                }

                if (dataAdapterSetter != null)
                {
                    dataAdapterSetter.SetValues(dataAdapter);
                }

                //TODO consider refactoring to put this inside IDbMetadata
                PropertyInfo selectCommandProperty = commandBuilder.GetType().GetProperty("DataAdapter",
                                                                                          BindingFlags.DeclaredOnly |
                                                                                          BindingFlags.GetProperty |
                                                                                          BindingFlags.Public |
                                                                                          BindingFlags.Instance
                    );

                selectCommandProperty.SetValue(commandBuilder, dataAdapter, null);

                ParameterUtils.CopyParameters(selectParameters, dataAdapter.SelectCommand);
                if (containsDataSet)
                {
                    return dataAdapter.Update(dataSet);
                }
                else
                {
                    //TODO should query metadata to see if supports filling dataTable directly.
                    if (dataAdapter is DbDataAdapter)
                    {
                        return ((DbDataAdapter)dataAdapter).Update(dataTable);
                    }
                    else
                    {
                        //TODO could create DataSet and extract DataTable... for now just throw
                        throw new DataException("Provider does not support filling DataTable directly");
                    }
                }
            }

            private static void ApplyConnectionAndTx(IDbCommand dbCommand, IDbCommand sourceCommand)
            {
                dbCommand.Connection = sourceCommand.Connection;
                dbCommand.Transaction = sourceCommand.Transaction;
            }

            #endregion

        }
        #endregion

        #region Private Helper Classes

        private class AdoStoredProcedureScalarCommandCallback : ICommandCallback
        {

            public object DoInCommand(IDbCommand command)
            {
                IDictionary returnedResults = new Hashtable();
                object scalar = command.ExecuteScalar();
                ParameterUtils.ExtractOutputParameters(returnedResults, command);
                returnedResults.Add("scalar", scalar);
                return returnedResults;

            }
        }

        private class AdoNonQueryWithOutputParamsCommandCallback : ICommandCallback
        {

            public object DoInCommand(IDbCommand command)
            {
                IDictionary returnedResults = new Hashtable();
                int rowsAffected = command.ExecuteNonQuery();
                ParameterUtils.ExtractOutputParameters(returnedResults, command);
                returnedResults.Add("rowsAffected", rowsAffected);
                return returnedResults;

            }
        }


        private class AdoResultProcessorsQueryCommandCallback : ICommandCallback
        {
            private AdoTemplate adoTemplate;
            private IList namedResultSetProcessors;
            //private IDbParameters declaredParameters;

            public AdoResultProcessorsQueryCommandCallback(AdoTemplate adoTemplate, IList namedResultSetProcessors)
            {
                //AssertUtils.ArgumentHasLength(namedResultSetProcessors, "namedResultSetProcessors");

                this.adoTemplate = adoTemplate;
                this.namedResultSetProcessors = namedResultSetProcessors;
                //this.declaredParameters = declaredParameters;
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
                        NamedResultSetProcessor namedResultSetProcessor = null;
                        try
                        {
                            namedResultSetProcessor
                                = namedResultSetProcessors[resultSetIndex] as NamedResultSetProcessor;
                            //Will only have possibility of run-time type error if using QueryWithCommandCreator
                            if (namedResultSetProcessor == null)
                            {
                                LOG.Error("NamedResultSetProcessor for result set index " + resultSetIndex +
                                          ", is not of expected type NamedResultSetProcessor.  Type = " +
                                          namedResultSetProcessors[resultSetIndex].GetType() +
                                          "; Skipping processing for this result set.");
                                continue;
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


                        string parameterName = namedResultSetProcessor.Name;
                        if (namedResultSetProcessor.ResultSetProcessor is IResultSetExtractor)
                        {
                            IResultSetExtractor rse = (IResultSetExtractor)namedResultSetProcessor.ResultSetProcessor;
                            object result = rse.ExtractData(reader);
                            returnedResults.Add(parameterName, result);
                        }
                        else if (namedResultSetProcessor.ResultSetProcessor is IRowMapper)
                        {
                            IRowMapper rowMapper = (IRowMapper)namedResultSetProcessor.ResultSetProcessor;
                            object result = (new RowMapperResultSetExtractor(rowMapper)).ExtractData(reader);
                            returnedResults.Add(parameterName, result);
                        }
                        else if (namedResultSetProcessor.ResultSetProcessor is IRowCallback)
                        {
                            IRowCallback rowCallback = (IRowCallback)namedResultSetProcessor.ResultSetProcessor;
                            (new RowCallbackResultSetExtractor(rowCallback)).ExtractData(reader);
                            returnedResults.Add(parameterName, "ResultSet returned was processed by an IRowCallback");
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


        private class DeriveParametersCommandCallback : ICommandCallback, ICommandTextProvider
        {
            private IDbProvider provider;
            private string procedureName;
            private bool includeReturnParameter;

            public DeriveParametersCommandCallback(IDbProvider provider, string procedureName, bool includeReturnParameter)
            {
                this.provider = provider;
                this.procedureName = procedureName;
                this.includeReturnParameter = includeReturnParameter;
            }

            public string CommandText
            {
                get { return procedureName; }
            }

            public object DoInCommand(IDbCommand command)
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedureName;

                //The DeriveParameter is static in all providers...it seems....
                Type commandBuilderType = provider.DbMetadata.CommandBuilderType;
                commandBuilderType.InvokeMember(provider.DbMetadata.CommandBuilderDeriveParametersMethod.Name,
                                                BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null,
                                                new object[] { command });

                if (command.Parameters.Count > 0)
                {
                    IDataParameter param = (IDataParameter)command.Parameters[0];
                    if (param.Direction == ParameterDirection.ReturnValue)
                    {
                        if (!includeReturnParameter)
                        {
                            command.Parameters.RemoveAt(0);
                        }
                    }
                }
                return ParameterUtils.CloneParameters(command);
            }
        }

        private class AdoRowCallbackCommandCallback : ICommandCallback
        {
            private AdoTemplate adoTemplate;
            private IRowCallback rowCallback;
            private IDictionary returnedParameters;

            public AdoRowCallbackCommandCallback(AdoTemplate adoTemplate, IRowCallback rowCallback, IDictionary returnedParameters)
            {
                this.adoTemplate = adoTemplate;
                this.rowCallback = rowCallback;
                this.returnedParameters = returnedParameters;
            }

            public object DoInCommand(IDbCommand command)
            {
                //Extract the single returned result set
                IDataReader reader = null;
                try
                {
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());
                    rowCallback.ProcessRow(reader);
                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                }
                ParameterUtils.ExtractOutputParameters(returnedParameters, command);
                return null;
            }
        }
        private class AdoResultSetExtractorWithOutputParamsCommandCallback : ICommandCallback
        {
            private AdoTemplate adoTemplate;
            private IResultSetExtractor rse;
            private IDictionary returnedParameters;

            public AdoResultSetExtractorWithOutputParamsCommandCallback(AdoTemplate adoTemplate, IResultSetExtractor rse, IDictionary returnedParameters)
            {
                this.adoTemplate = adoTemplate;
                this.rse = rse;
                this.returnedParameters = returnedParameters;
            }

            public object DoInCommand(IDbCommand command)
            {
                object returnVal = null;
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


        private class AdoRowMapperQueryCommandCallback : ICommandCallback
        {
            private AdoTemplate adoTemplate;
            private IRowMapper rowMapper;
            //private IDbParameters declaredParameters;
            private IDictionary returnedParameters;

            public AdoRowMapperQueryCommandCallback(AdoTemplate adoTemplate, IRowMapper rowMapper, IDictionary returnedParameters)
            {
                this.adoTemplate = adoTemplate;
                this.rowMapper = rowMapper;
                //this.declaredParameters = declaredParameters;
                this.returnedParameters = returnedParameters;
            }

            public object DoInCommand(IDbCommand command)
            {
                IList objectList = null;
                //Extract the single returned result set
                IDataReader reader = null;
                try
                {
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());
                    RowMapperResultSetExtractor rse = new RowMapperResultSetExtractor(rowMapper, 1);
                    objectList = (IList)rse.ExtractData(reader);
                }
                finally
                {
                    Support.AdoUtils.CloseReader(reader);
                }
                ParameterUtils.ExtractOutputParameters(returnedParameters, command);
                return objectList;
            }
        }


        private class ExecuteNonQueryCallbackWithParameters : ICommandCallback, ICommandTextProvider
        {
            private CommandType commandType;
            private string commandText;
            private IDbParameters parameters;

            public ExecuteNonQueryCallbackWithParameters(CommandType commandType, string commandText, IDbParameters dbParameters)
            {
                this.commandType = commandType;
                this.commandText = commandText;
                parameters = dbParameters;
            }

            public string CommandText
            {
                get { return commandText; }
            }

            public Object DoInCommand(IDbCommand command)
            {
                command.CommandType = commandType;
                command.CommandText = commandText;
                ParameterUtils.CopyParameters(command, parameters);
                Object returnValue = command.ExecuteNonQuery();
                ParameterUtils.CopyParameters(parameters, command);
                return returnValue;
            }

        }


        private class ExecuteNonQueryCallbackWithCommandSetter : ICommandCallback, ICommandTextProvider
        {
            private CommandType commandType;
            private string commandText;
            private ICommandSetter commandSetter;

            public ExecuteNonQueryCallbackWithCommandSetter(CommandType commandType, string commandText, ICommandSetter commandSetter)
            {
                this.commandType = commandType;
                this.commandText = commandText;
                this.commandSetter = commandSetter;
            }

            public string CommandText
            {
                get { return commandText; }
            }

            public Object DoInCommand(IDbCommand command)
            {
                command.CommandType = commandType;
                command.CommandText = commandText;
                if (commandSetter != null)
                {
                    commandSetter.SetValues(command);
                }
                Object rowsAffected = command.ExecuteNonQuery();
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("ExecuteNonQuery affected " + rowsAffected + " rows");
                }
                return rowsAffected;
            }

        }


        private class ExecuteScalarCallbackWithParameters : ICommandCallback, ICommandTextProvider
        {
            private CommandType commandType = CommandType.Text;
            private string commandText;
            private IDbParameters parameters;

            public ExecuteScalarCallbackWithParameters(CommandType cmdType, string cmdText, IDbParameters dbParameters)
            {
                commandType = cmdType;
                commandText = cmdText;
                parameters = dbParameters;
            }

            public string CommandText
            {
                get { return commandText; }
            }

            public Object DoInCommand(IDbCommand command)
            {
                command.CommandType = commandType;
                command.CommandText = commandText;
                ParameterUtils.CopyParameters(command, parameters);
                Object returnValue = command.ExecuteScalar();
                ParameterUtils.CopyParameters(parameters, command);
                return returnValue;
            }
        }

        private class ExecuteScalarCallbackWithCommandSetter : ICommandCallback, ICommandTextProvider
        {
            private CommandType commandType;
            private string commandText;
            private ICommandSetter commandSetter;

            public ExecuteScalarCallbackWithCommandSetter(CommandType commandType, string commandText, ICommandSetter commandSetter)
            {
                this.commandType = commandType;
                this.commandText = commandText;
                this.commandSetter = commandSetter;
            }

            public string CommandText
            {
                get { return commandText; }
            }

            public Object DoInCommand(IDbCommand command)
            {
                command.CommandType = commandType;
                command.CommandText = commandText;
                if (commandSetter != null)
                {
                    commandSetter.SetValues(command);
                }
                Object returnValue = command.ExecuteScalar();
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("ExecuteScalar return value = " + returnValue);
                }
                return returnValue;
            }

        }


        private class ExecuteCommandCallbackUsingDelegate : ICommandCallback, ICommandTextProvider
        {
            private CommandDelegate del;
            private string commandText;

            public ExecuteCommandCallbackUsingDelegate(CommandDelegate d)
            {
                del = d;
            }

            public string CommandText
            {
                get { return commandText; }
            }

            public Object DoInCommand(IDbCommand command)
            {
                try
                {
                    return del(command);
                }
                catch (Exception e)
                {
                    e.GetType();
                    commandText = command.CommandText;
                    throw;
                }
            }
        }



        private class QueryCallbackWithCommandSetter : ICommandCallback, ICommandTextProvider
        {
            private AdoTemplate adoTemplate;
            private IResultSetExtractor rse;
            private ResultSetExtractorDelegate resultSetExtractorDelegate;
            private ICommandSetter commandSetter;
            private CommandType commandType;
            private string commandText;

            public QueryCallbackWithCommandSetter(AdoTemplate adoTemplate, CommandType cmdType, string cmdText, IResultSetExtractor rse, ICommandSetter commandSetter)
            {
                this.adoTemplate = adoTemplate;
                commandType = cmdType;
                commandText = cmdText;
                this.rse = rse;
                this.commandSetter = commandSetter;
            }

            public QueryCallbackWithCommandSetter(AdoTemplate adoTemplate, CommandType cmdType, string cmdText, ResultSetExtractorDelegate resultSetExtractorDelegate, ICommandSetter commandSetter)
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

            public object DoInCommand(IDbCommand command)
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

        private class QueryCallback : ICommandCallback, ICommandTextProvider
        {
            private AdoTemplate adoTemplate;
            private IResultSetExtractor rse;
            private ResultSetExtractorDelegate resultSetExtractorDelegate;
            private CommandType commandType;
            private string commandText;
            private IDbParameters parameters;

            public QueryCallback(AdoTemplate adoTemplate, CommandType cmdType, string cmdText, IResultSetExtractor rse, IDbParameters dbParameters)
            {
                this.adoTemplate = adoTemplate;
                commandType = cmdType;
                commandText = cmdText;
                this.rse = rse;
                parameters = dbParameters;
            }

            public QueryCallback(AdoTemplate adoTemplate, CommandType cmdType, string cmdText, ResultSetExtractorDelegate resultSetExtractorDelegate, IDbParameters dbParameters)
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

            public object DoInCommand(IDbCommand command)
            {
                IDataReader reader = null;
                try
                {
                    command.CommandType = commandType;
                    command.CommandText = commandText;
                    ParameterUtils.CopyParameters(command, parameters);
                    reader = adoTemplate.CreateDataReaderWrapper(command.ExecuteReader());
                    object returnValue;
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

        #endregion

        /// <summary>
        /// Creates the data reader wrapper for use in AdoTemplate callback methods.
        /// </summary>
        /// <param name="readerToWrap">The reader to wrap.</param>
        /// <returns>The data reader used in AdoTemplate callbacks</returns>
        public override IDataReader CreateDataReaderWrapper(IDataReader readerToWrap)
        {
            if (dataReaderWrapperType != null && newDataReaderWrapper != null)
            {
                IDataReaderWrapper wrapper = (IDataReaderWrapper) newDataReaderWrapper.Invoke(ObjectUtils.EmptyObjects);
                wrapper.WrappedReader = readerToWrap;
                return wrapper;
            }
            else
            {
                return readerToWrap;
            }
        }
    }

}
