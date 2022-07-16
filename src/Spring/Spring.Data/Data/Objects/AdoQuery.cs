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
using Spring.Dao.Support;
using Spring.Data.Common;

namespace Spring.Data.Objects
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

	    public AdoQuery(IDbProvider provider, string sql)
	    {
	        DbProvider = provider;
	        Sql = sql;
	    }

		#endregion

		#region Properties

		#endregion

		#region Methods

        public IList Query()
        {
            return QueryByNamedParam(null);
        }

	    public IList QueryByNamedParam(IDictionary inParams)
	    {
	        return QueryByNamedParam(inParams, null);
	    }

	    public IList QueryByNamedParam(IDictionary inParams, IDictionary returnedParameters)
	    {
            return QueryByNamedParam(inParams, returnedParameters, null);
	    }

        public IList QueryByNamedParam(IDictionary inParams, IDictionary returnedParameters, IDictionary callingContext)
        {
            ValidateNamedParameters(inParams);
            IRowMapper rowMapper = NewRowMapper(inParams, callingContext);
            return AdoTemplate.QueryWithCommandCreator(NewCommandCreator(inParams), rowMapper, returnedParameters);
        }

        public object QueryForObject(IDictionary inParams)
        {
            IList results = QueryByNamedParam(inParams);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        public object QueryForObject(IDictionary inParams, IDictionary returnedParameters)
        {
            IList results = QueryByNamedParam(inParams, returnedParameters);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }

        public object QueryForObject(IDictionary inParams, IDictionary returnedParameters, IDictionary callingContext)
        {
            IList results = QueryByNamedParam(inParams, returnedParameters, callingContext);
            return DataAccessUtils.RequiredUniqueResultSet(results);
        }


	    protected abstract IRowMapper NewRowMapper(IDictionary inParams, IDictionary callingContext);

		#endregion

	}
}
