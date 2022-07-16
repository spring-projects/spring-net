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
using Spring.Util;

namespace Spring.Data.Generic
{
	/// <summary>
	/// Adapter implementation of the ResultSetExtractor interface that delegates
	/// to a RowMapper which is supposed to create an object for each row.
	/// Each object is added to the results List of this ResultSetExtractor.
	/// </summary>
	/// <remarks>
	/// Useful for the typical case of one object per row in the database table.
	/// The number of entries in the results list will match the number of rows.
	/// <p>
	/// Note that a RowMapper object is typically stateless and thus reusable;
	/// just the RowMapperResultSetExtractor adapter is stateful.
	/// </p>
	/// <p>
	/// As an alternative consider subclassing MappingAdoQuery from the
	/// Spring.Data.Objects namespace:  Instead of working with separate
	/// AdoTemplate and IRowMapper objects you can have executable
	/// query objects (containing row-mapping logic) there.
	/// </p>
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public class RowMapperResultSetExtractor<T> : IResultSetExtractor<IList<T>>
	{
		#region Fields

	    private IRowMapper<T> rowMapper;

        private RowMapperDelegate<T> rowMapperDelegate;

	    private int rowsExpected;

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="Spring.Data.Core.RowMapperResultSetExtractor"/> class.
        /// </summary>
		public RowMapperResultSetExtractor(IRowMapper<T> rowMapper) : this(rowMapper,0, null)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="RowMapperResultSetExtractor&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="rowMapper">The row mapper.</param>
        /// <param name="rowsExpected">The rows expected.</param>
        public RowMapperResultSetExtractor(IRowMapper<T> rowMapper, int rowsExpected) : this(rowMapper, rowsExpected, null)
        {
        }
        public RowMapperResultSetExtractor(IRowMapper<T> rowMapper, int rowsExpected, IDataReaderWrapper dataReaderWrapper)
        {
            //TODO use datareaderwrapper
            AssertUtils.ArgumentNotNull(rowMapper, "rowMapper");

            this.rowMapper = rowMapper;
            this.rowsExpected = rowsExpected;
        }

        public RowMapperResultSetExtractor(RowMapperDelegate<T> rowMapperDelegate)
            : this(rowMapperDelegate, 0, null)
        {
        }

        public RowMapperResultSetExtractor(RowMapperDelegate<T> rowMapperDelegate, int rowsExpected)
            : this(rowMapperDelegate, rowsExpected, null)
        {
        }
        public RowMapperResultSetExtractor(RowMapperDelegate<T> rowMapperDelegate, int rowsExpected, IDataReaderWrapper dataReaderWrapper)
        {
            //TODO use datareaderwrapper

            AssertUtils.ArgumentNotNull(rowMapperDelegate, "rowMapperDelegate");

            this.rowMapperDelegate = rowMapperDelegate;
            this.rowsExpected = rowsExpected;
        }

		#endregion

        #region IResultSetExtractor Members

        public IList<T> ExtractData(IDataReader reader)
        {
            // Use the more efficient collection if we know how many rows to expect:
            // ArrayList in case of a known row count, LinkedList if unknown
            //IList<T> results = (rowsExpected > 0) ? new List<T>(rowsExpected) : new LinkedList<T>();

            //how come LinkedList<T> doesn't implement IList<T> ?!?!?!
            //some web entries claim slow indexer...  need to write our own again?  return ICollection instead?
            //http://blogs.msdn.com/kcwalina/archive/2005/09/23/Collections.aspx
            //We did not implement IList<T> on LinkedList because the indexer would be
            //very slow. If you really need the interface, you probably can inherit from
            //LinkedList<T> and implement the interface on the subtype.
            IList<T> results =  new List<T>();
		    int rowNum = 0;
            if (rowMapper != null)
            {
                while (reader.Read())
                {
                    results.Add(rowMapper.MapRow(reader, rowNum++));
                }
            }
            else
            {
                while (reader.Read())
                {
                    results.Add(rowMapperDelegate(reader, rowNum++));
                }
            }

            return results;
        }

        #endregion
    }
}
