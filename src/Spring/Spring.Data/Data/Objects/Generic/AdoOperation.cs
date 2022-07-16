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

using Spring.Dao;
using Spring.Data.Common;

namespace Spring.Data.Objects.Generic
{
    /// <summary>
    /// A "AdoOperation" is a thread-safe, reusable object representing
    /// an ADO Query, "NonQuery" (Create, Update, Delete), stored procedure
    /// or DataSet manipualtions.
    /// </summary>
    /// <remarks>
    /// Subclasses should set Command Text and add parameters before invoking
    /// Compile().  The order in which parameters are added is generally
    /// significant when using providers or functionality that do not use
    /// named parameters.
    ///
    /// </remarks>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class AdoOperation : AbstractAdoOperation
    {
        #region Fields

        private Data.Generic.AdoTemplate adoTemplate = new Data.Generic.AdoTemplate();

        #endregion

        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoOperation"/> class.
        /// </summary>
        /// <remarks>A DbProvider, SQL and any parameters must be supplied
        /// before invoking the compile method and using this object.
        /// </remarks>
        public AdoOperation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoOperation"/> class.
        /// </summary>
        /// <remarks>A DbProvider, SQL and any parameters must be supplied
        /// before invoking the compile method and using this object.
        /// </remarks>
        /// <param name="provider">Database provider to use.</param>
        public AdoOperation(IDbProvider provider)
            : this(provider, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoOperation"/> class.
        /// </summary>
        /// <remarks>A DbProvider, SQL and any parameters must be supplied
        /// before invoking the compile method and using this object.
        /// </remarks>
        /// <param name="provider">Database provider to use.</param>
        /// <param name="sql">SQL query or stored procedure name.</param>
        public AdoOperation(IDbProvider provider, String sql)
        {
            //intialized AdoTemplate with the DbProvider
            DbProvider = provider;
            Sql = sql;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the ADO template.
        /// </summary>
        /// <value>The ADO template.</value>
        public Data.Generic.AdoTemplate AdoTemplate
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("AdoTemplate");
                }
                adoTemplate = value;
            }
            get { return adoTemplate; }
        }

        /// <summary>
        /// Gets or sets the db provider.
        /// </summary>
        /// <value>The db provider.</value>
        public override IDbProvider DbProvider
        {
            set
            {
                adoTemplate.DbProvider = value;
                if (DeclaredParameters == null)
                {
                    DeclaredParameters = new DbParameters(value);
                }
            }
            get { return adoTemplate.DbProvider; }
        }

        /// <summary>
        /// Sets the command timeout for IDbCommands that this AdoTemplate executes.
        /// </summary>
        /// <remarks>Default is 0, indicating to use the database provider's default.
        /// Any timeout specified here will be overridden by the remaining
        /// transaction timeout when executing within a transaction that has a
        /// timeout specified at the transaction level.
        /// </remarks>
        /// <value>The command timeout.</value>
        public override int CommandTimeout
        {
            set { adoTemplate.CommandTimeout = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compiles this operation.  Ignores subsequent attempts to compile.
        /// </summary>
        public override void Compile()
        {
            if (!Compiled)
            {
                if (Sql == null)
                {
                    throw new InvalidDataAccessApiUsageException("Setting of Sql is required");
                }
                if (CommandType == 0)
                {
                    throw new InvalidDataAccessApiUsageException("Setting of CommandType is required");
                }

                try
                {
                    adoTemplate.AfterPropertiesSet();
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidDataAccessApiUsageException(ex.Message);
                }

                CompileInternal();
                Compiled = true;
            }
        }

        #endregion
    }
}
