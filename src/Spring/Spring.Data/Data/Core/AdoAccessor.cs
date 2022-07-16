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

using System.Data;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Objects.Factory;

namespace Spring.Data.Core
{
    /// <summary>
    /// Base class for AdoTemplate and other ADO.NET DAO helper classes defining
    /// common properties like DbProvider.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Juergen Hoeller</author>
    public abstract class AdoAccessor : IInitializingObject
    {
        protected object AdoUtils;

        private int commandTimeout = -1;

        #region Properties

        /// <summary>
        /// An instance of a DbProvider implementation.
        /// </summary>
        public abstract IDbProvider DbProvider
        {
            get;
            set;
        }



        /// <summary>
        /// Gets or sets a value indicating whether to lazily initialize the 
        /// IAdoExceptionTranslator for this accessor, on first encounter of a 
        /// exception from the data provider.  Default is "true"; can be switched to
        /// "false" for initialization on startup.
        /// </summary>
        /// <value><c>true</c> if to lazy initialize the IAdoExceptionTranslator; 
        /// otherwise, <c>false</c>.</value>
        public abstract bool LazyInit
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the exception translator. If no custom translator is provided, a default
        /// <see cref="ErrorCodeExceptionTranslator"/> is used.
        /// </summary>
        /// <value>The exception translator.</value>
        public abstract IAdoExceptionTranslator ExceptionTranslator
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or set the System.Type to use to create an instance of IDataReaderWrapper 
        /// for the purpose of having defaults values to use in case of DBNull values read 
        /// from IDataReader.
        /// </summary>
        /// <value>The type of the data reader wrapper.</value>
        public abstract Type DataReaderWrapperType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the command timeout for IDbCommands that this AdoTemplate executes.
        /// </summary>
        /// <remarks>Default is 0, indicating to use the database provider's default.
        /// Any timeout specified here will be overridden by the remaining 
        /// transaction timeout when executing within a transaction that has a
        /// timeout specified at the transaction level. 
        /// </remarks>
        /// <value>The command timeout.</value>
        public virtual int CommandTimeout
        {
            get { return commandTimeout; }
            set { commandTimeout = value; }
        }

        #endregion

        /// <summary>
        /// Prepare the command setting the transaction timeout.
        /// </summary>
        /// <param name="command"></param>
        protected virtual void ApplyCommandSettings(IDbCommand command)
        {
            Support.ConnectionUtils.ApplyTransactionTimeout(command, DbProvider, CommandTimeout);
        }

        /// <summary>
        /// Dispose the command, if any
        /// </summary>
        protected virtual void DisposeCommand(IDbCommand command)
        {
            Support.AdoUtils.DisposeCommand(command);
        }

        /// <summary>
        /// Dispose the command, if any
        /// </summary>
        protected virtual void DisposeDataAdapterCommands(IDbDataAdapter adapter)
        {
            Support.AdoUtils.DisposeDataAdapterCommands(adapter);
        }

        /// <summary>
        /// Extract the command text from the given <see cref="ICommandTextProvider"/>, if any.
        /// </summary>
        protected virtual string GetCommandText(object cmdTextProvider)
        {
            ICommandTextProvider commandTextProvider = cmdTextProvider as ICommandTextProvider;
            if (commandTextProvider != null)
            {
                return commandTextProvider.CommandText;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Obtain a connection/transaction pair
        /// </summary>
        protected virtual ConnectionTxPair GetConnectionTxPair(IDbProvider provider)
        {
            return Support.ConnectionUtils.GetConnectionTxPair(provider);
        }

        protected virtual void DisposeConnection(IDbConnection connection, IDbProvider provider)
        {
            Support.ConnectionUtils.DisposeConnection(connection, provider);
        }

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// <p>
        /// Please do consult the class level documentation for the
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
        /// description of exactly <i>when</i> this method is invoked. In
        /// particular, it is worth noting that the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// callbacks will have been invoked <i>prior</i> to this method being
        /// called.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
        public abstract void AfterPropertiesSet();

        /// <summary>
        /// Creates the data reader wrapper for use in AdoTemplate callback methods.
        /// </summary>
        /// <param name="readerToWrap">The reader to wrap.</param>
        /// <returns>The data reader used in AdoTemplate callbacks</returns>
        public abstract IDataReader CreateDataReaderWrapper(IDataReader readerToWrap);

        /// <summary>
        /// Creates the a db parameters collection, adding to the collection a parameter created from
        /// the method parameters.
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="size">The size of the parameter, for use in defining lengths of string values.  Use
        /// 0 if not applicable.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>A collection of db parameters with a single parameter in the collection based
        /// on the method parameters</returns>
        protected IDbParameters CreateDbParameters(string name, Enum dbType, int size, object parameterValue)
        {
            IDbParameters parameters = new DbParameters(DbProvider);
            parameters.Add(name, dbType, size).Value = parameterValue;
            return parameters;
        }


        #region Parameter Creation Helper Methods

        /// <summary>
        /// Creates a new instance of <see cref="DbParameters"/>
        /// </summary>
        /// <returns>a new instance of <see cref="DbParameters"/></returns>
        public virtual IDbParameters CreateDbParameters()
        {
            return new DbParameters(DbProvider);
        }

        /// <summary>
        /// Derives the parameters of a stored procedure, not including the return parameter.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>The stored procedure parameters.</returns>
        public virtual IDataParameter[] DeriveParameters(string procedureName)
        {
            return DeriveParameters(procedureName, false);
        }

        /// <summary>
        /// Derives the parameters of a stored procedure including the return parameter
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="includeReturnParameter">if set to <c>true</c> to include return parameter.</param>
        /// <returns>The stored procedure parameters</returns>
        public abstract IDataParameter[] DeriveParameters(string procedureName, bool includeReturnParameter);

        #endregion
    }
}
