#region License

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

#endregion

using System.Data;
using Spring.Dao.Support;
using Spring.Data.Common;
using Spring.Data.Support;

namespace Spring.Data.Core
{
    /// <summary>
    /// Convenient super class for ADO.NET data access objects.
    /// </summary>
    /// <remarks>
    /// Requires a IDBProvider to be set, providing a
    /// AdoTemplate based on it to subclasses.
    /// This base class is mainly intended for AdoTemplate usage.
    /// </remarks>
    public class AdoDaoSupport : DaoSupport
    {
        private AdoTemplate adoTemplate;

        /// <summary>
        /// The DbProvider instance used by this DAO
        /// </summary>
        public IDbProvider DbProvider
        {
            set
            {
                adoTemplate = CreateAdoTemplate(value);
            }
            get
            {
                if (adoTemplate != null)
                {
                    return adoTemplate.DbProvider;
                }
                else
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// Set the AdoTemplate for this DAO explicity, as
        /// an alternative to specifying a IDbProvider
        /// </summary>
        public AdoTemplate AdoTemplate
        {
            set
            {
                adoTemplate = value;
            }
            get
            {
                return adoTemplate;
            }

        }

        protected override void CheckDaoConfig()
        {
            if (adoTemplate == null)
            {
                throw new ArgumentException("DbProvider or AdoTemplate is required");
            }
        }

        protected IDbConnection Connection
        {
            get
            {
                return ConnectionUtils.GetConnection(DbProvider);
            }
        }

        protected IAdoExceptionTranslator ExceptionTranslator
        {
            get
            {
                return null;  //Investigate AdoExceptionTranslator on AdoAccessor
            }
        }

        protected void DisposeConnection(IDbConnection conn, IDbProvider dbProvider)
        {
            ConnectionUtils.DisposeConnection(conn, dbProvider);
        }


        /// <summary>
        /// Create a AdoTemplate for a given DbProvider
        /// Only invoked if populating the DAO with a DbProvider reference.
        /// </summary>
        /// <remarks>
        /// Can be overriden in subclasses to provide AdoTemplate instances
        /// with a different configuration, or a cusotm AdoTemplate subclass.
        /// </remarks>
        /// <param name="dbProvider">The DbProvider to create a AdoTemplate for</param>
        protected virtual AdoTemplate CreateAdoTemplate(IDbProvider dbProvider)
        {
            return new AdoTemplate(dbProvider);
        }

        /// <summary>
        /// Convenience method to create a parameters builder.
        /// </summary>
        /// <remarks>Virtual for sublcasses to override with custom
        /// implementation.</remarks>
        /// <returns>A new DbParameterBuilder</returns>
        protected virtual IDbParametersBuilder CreateDbParametersBuilder()
        {
            return new DbParametersBuilder(DbProvider);
        }

        protected virtual IDbParameters CreateDbParameters()
        {
            return AdoTemplate.CreateDbParameters();
        }

    }
}
