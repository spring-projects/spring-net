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
using Spring.Data.Generic;

namespace Spring.Data.Objects.Generic
{
	/// <summary>
	/// Reusable query in which concrete subclasses must implement the
	/// abstract MapRow method to map each row of a single result set into an
	/// object.
	/// </summary>
	/// <remarks>The MapRow method is also passed the input parameters and
	/// a dictionary to provide the method with calling context information
	/// (if necessary).  If you do not need these additional parameters, use
	/// the subclass MappingAdoQuery.
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class MappingAdoQueryWithContext : AdoQuery
	{
		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="MappingAdoQueryWithContext"/> class.
        /// </summary>
		public MappingAdoQueryWithContext()
		{

		}

	    public MappingAdoQueryWithContext(IDbProvider dbProvider, string sql) : base(dbProvider, sql)
	    {

	    }
		#endregion

		#region Properties

		#endregion

		#region Methods

        protected override IRowMapper<T> NewRowMapper<T>(System.Collections.IDictionary inParams,
                                                         System.Collections.IDictionary callingContext)
        {
            return new RowMapperImpl<T>(this, inParams, callingContext);
        }

        protected abstract T MapRow<T>(IDataReader reader, int rowNum, IDictionary inParams, IDictionary callingContext);


	    #endregion

        private class RowMapperImpl<T> : IRowMapper<T>
        {
            private MappingAdoQueryWithContext outer;
            private IDictionary inParams;
            private IDictionary callingContext;

            public RowMapperImpl(MappingAdoQueryWithContext mappingAdoQueryWithContext, IDictionary inParams, IDictionary callingContext)
            {
                outer = mappingAdoQueryWithContext;
                this.inParams = inParams;
                this.callingContext = callingContext;
            }

            public T MapRow(IDataReader dataReader, int rowNum)
            {
                return outer.MapRow<T>(dataReader, rowNum, inParams, callingContext);
            }
        }

	}
}
