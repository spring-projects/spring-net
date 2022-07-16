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

using Spring.Dao.Support.Generic;
using Spring.Data.Common;
using Spring.Data.Generic;

namespace Spring.Data.Objects.Generic
{
	/// <summary>
	/// Place together the mapping logic to a plain .NET object
	/// for one result set within the same class.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class AdoQuery : AdoOperation
	{
		#region Fields

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="AdoQuery"/> class.
        /// </summary>
		public AdoQuery()
		{

		}

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoQuery"/> class.
        /// </summary>
        /// <param name="provider">The database provider.</param>
        /// <param name="sql">The SQL to execute</param>
	    public AdoQuery(IDbProvider provider, string sql)
	    {
	        DbProvider = provider;
	        Sql = sql;
	    }

		#endregion

		#region Properties

		#endregion

		#region Methods


        /// <summary>
        /// Convenient method to execute query without parameters or calling context.
        /// </summary>
        /// <typeparam name="T">The type parameter for the returned collection</typeparam>
        /// <returns>List of mapped objects</returns>
        public IList<T> Query<T>()
        {
            return QueryByNamedParam<T>(null);
        }

        /// <summary>
        /// Convenient method to execute query with parameters but without calling context.
        /// </summary>
        /// <typeparam name="T">The type parameter for the returned collection</typeparam>
        /// <param name="inParams">The parameters for the query.</param>
        /// <returns></returns>
        public IList<T> QueryByNamedParam<T>(System.Collections.IDictionary inParams)
	    {
	        return QueryByNamedParam<T>(inParams, null);
	    }

        /// <summary>
        /// Convenient method to execute query with parameters and access to output parameter values.
        /// </summary>
        /// <typeparam name="T">The type parameter for the returned collection</typeparam>
        /// <param name="inParams">The parameters for the query.</param>
        /// <param name="outParams">The returned output parameters.</param>
        /// <returns></returns>
        public IList<T> QueryByNamedParam<T>(System.Collections.IDictionary inParams,
                                             System.Collections.IDictionary outParams)
	    {
            return QueryByNamedParam<T>(inParams, outParams, null);
	    }

        /// <summary>
        /// Central execution method.  All named parameter execution goes through this method.
        /// </summary>
        /// <typeparam name="T">the type parameter for the returned collection</typeparam>
        /// <param name="inParams">The in params.</param>
        /// <param name="outParams">The out params.</param>
        /// <param name="callingContext">The calling context.</param>
        /// <returns></returns>
        public IList<T> QueryByNamedParam<T>(System.Collections.IDictionary inParams,
                                             System.Collections.IDictionary outParams,
                                             System.Collections.IDictionary callingContext)
        {
            ValidateNamedParameters(inParams);
            IRowMapper<T> rowMapper = NewRowMapper<T>(inParams, callingContext);
            return AdoTemplate.QueryWithCommandCreator(NewCommandCreator(inParams), rowMapper, outParams);
        }

        public T QueryForObject<T>(System.Collections.IDictionary inParams)
        {
            IList<T> results = QueryByNamedParam<T>(inParams);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        public T QueryForObject<T>(System.Collections.IDictionary inParams, System.Collections.IDictionary returnedParameters)
        {
            IList<T> results = QueryByNamedParam<T>(inParams, returnedParameters);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        public T QueryForObject<T>(System.Collections.IDictionary inParams, System.Collections.IDictionary returnedParameters, System.Collections.IDictionary callingContext)
        {
            IList<T> results = QueryByNamedParam<T>(inParams, returnedParameters, callingContext);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }


        protected abstract IRowMapper<T> NewRowMapper<T>(System.Collections.IDictionary inParams, System.Collections.IDictionary callingContext);

		#endregion

	}
}
