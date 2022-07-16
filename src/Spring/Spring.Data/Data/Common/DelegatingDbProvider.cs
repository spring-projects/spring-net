#region Licence

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
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Data.Common
{
    /// <summary>
    /// IDbProvider implementation that delegates all calls to a given target
    /// IDbProvider
    /// </summary>
    /// <author>Mark Pollack</author>
    public class DelegatingDbProvider : IDbProvider, IInitializingObject
    {
        private IDbProvider targetDbProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingDbProvider"/> class.
        /// </summary>
        public DelegatingDbProvider()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingDbProvider"/> class.
        /// </summary>
        /// <param name="targetDbProvider">The target db provider.</param>
        public DelegatingDbProvider(IDbProvider targetDbProvider)
        {
            this.targetDbProvider = targetDbProvider;
        }


        /// <summary>
        /// Gets or sets the target IDbProvider that this IDbProvider should delegate to
        /// </summary>
        /// <value>The target db provider.</value>
        public IDbProvider TargetDbProvider
        {
            get { return targetDbProvider; }
            set
            {
                AssertUtils.ArgumentNotNull(value,"TargetDbProvider");
                targetDbProvider = value;
            }
        }

        #region IDbProvider Members

        /// <summary>
        /// Returns a new command object for executing SQL statments/Stored Procedures
        /// against the database.
        /// </summary>
        /// <returns>An new <see cref="IDbCommand"/></returns>
        public virtual IDbCommand CreateCommand()
        {
            return TargetDbProvider.CreateCommand();
        }

        /// <summary>
        /// Returns a new instance of the providers CommandBuilder class.
        /// </summary>
        /// <remarks>In .NET 1.1 there was no common base class or interface
        /// for command builders, hence the return signature is object to
        /// be portable (but more loosely typed) across .NET 1.1/2.0</remarks>
        /// <returns>A new Command Builder</returns>
        public virtual object CreateCommandBuilder()
        {
            return TargetDbProvider.CreateCommandBuilder();
        }

        /// <summary>
        /// Returns a new connection object to communicate with the database.
        /// </summary>
        /// <returns>A new <see cref="IDbConnection"/></returns>
        public virtual IDbConnection CreateConnection()
        {
            return TargetDbProvider.CreateConnection();
        }

        /// <summary>
        /// Returns a new adapter objects for use with offline DataSets.
        /// </summary>
        /// <returns>A new <see cref="IDbDataAdapter"/></returns>
        public virtual IDbDataAdapter CreateDataAdapter()
        {
            return TargetDbProvider.CreateDataAdapter();
        }

        /// <summary>
        /// Returns a new parameter object for binding values to parameter
        /// placeholders in SQL statements or Stored Procedure variables.
        /// </summary>
        /// <returns>A new <see cref="IDbDataParameter"/></returns>
        public virtual IDbDataParameter CreateParameter()
        {
            return TargetDbProvider.CreateParameter();
        }

        /// <summary>
        /// Creates the name of the parameter in the format appropriate to use inside IDbCommand.CommandText.
        /// </summary>
        /// <remarks>In most cases this adds the parameter prefix to the name passed into this method.</remarks>
        /// <param name="name">The unformatted name of the parameter.</param>
        /// <returns>The parameter name formatted foran IDbCommand.CommandText.</returns>
        public virtual string CreateParameterName(string name)
        {
            return TargetDbProvider.CreateParameterName(name);
        }

        /// <summary>
        /// Creates the name ofthe parameter in the format appropriate for an IDataParameter, i.e. to be
        /// part of a IDataParameterCollection.
        /// </summary>
        /// <param name="name">The unformatted name of the parameter.</param>
        /// <returns>The parameter name formatted for an IDataParameter</returns>
        public virtual string CreateParameterNameForCollection(string name)
        {
            return TargetDbProvider.CreateParameterNameForCollection(name);
        }

        /// <summary>
        /// Return metadata information about the database provider
        /// </summary>
        public virtual IDbMetadata DbMetadata
        {
            get { return TargetDbProvider.DbMetadata; }
        }

        /// <summary>
        /// Connection string used to create connections.
        /// </summary>
        public virtual string ConnectionString
        {
            get { return TargetDbProvider.ConnectionString; }
            set { TargetDbProvider.ConnectionString = value; }
        }

        /// <summary>
        /// Extracts the provider specific error code as a string.
        /// </summary>
        /// <param name="e">The data access exception.</param>
        /// <returns>The provider specific error code</returns>
        public virtual string ExtractError(Exception e)
        {
            return TargetDbProvider.ExtractError(e);
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
        public virtual bool IsDataAccessException(Exception e)
        {
            return TargetDbProvider.IsDataAccessException(e);
        }

        #endregion

        #region IInitializingObject Members

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
        public virtual void AfterPropertiesSet()
        {
            if (TargetDbProvider == null)
            {
                throw new ArgumentNullException("Property 'TargetDbProvider' is required.");
            }
        }

        #endregion
    }
}
