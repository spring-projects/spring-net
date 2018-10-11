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

using System;
using System.Collections;
using System.Collections.Generic;
using Spring.Collections;

namespace Spring.Data.Core
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
    public class RowMapperResultSetExtractor : IResultSetExtractor
    {
        private readonly IRowMapper rowMapper;
        private readonly RowMapperDelegate rowMapperDelegate;
        private readonly int rowsExpected;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowMapperResultSetExtractor"/> class.
        /// </summary>
        public RowMapperResultSetExtractor(IRowMapper rowMapper) : this(rowMapper,0, null)
        {
        }
	    
        public RowMapperResultSetExtractor(IRowMapper rowMapper, int rowsExpected) : this(rowMapper, rowsExpected, null)
        {
        }
        public RowMapperResultSetExtractor(IRowMapper rowMapper, int rowsExpected, IDataReaderWrapper dataReaderWrapper)
        {
            //TODO use datareaderwrapper
            this.rowMapper = rowMapper ?? throw new ArgumentNullException(nameof(rowMapper));
            this.rowsExpected = rowsExpected;
        }

        public RowMapperResultSetExtractor(RowMapperDelegate rowMapperDelegate)
            : this(rowMapperDelegate, 0, null)
        {
        }

        public RowMapperResultSetExtractor(RowMapperDelegate rowMapperDelegate, int rowsExpected)
            : this(rowMapperDelegate, rowsExpected, null)
        {
        }
        public RowMapperResultSetExtractor(RowMapperDelegate rowMapperDelegate, int rowsExpected, IDataReaderWrapper dataReaderWrapper)
        {
            //TODO use datareaderwrapper
            this.rowMapperDelegate = rowMapperDelegate ?? throw new ArgumentNullException(nameof(rowMapperDelegate));
            this.rowsExpected = rowsExpected;
        }

        public object ExtractData(System.Data.IDataReader reader)
        {
            // Use the more efficient collection if we know how many rows to expect:
            // ArrayList in case of a known row count, LinkedList if unknown
            var results = new List<object>(rowsExpected);
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
    }
}
