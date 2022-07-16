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

namespace Spring.Data
{
	/// <summary>
    /// Callback to process each row of data in a result set to an object.
	/// </summary>
	/// <remarks>Implementations of this interface perform the actual work
    /// of mapping rows, but don't need worry about managing
    /// ADO.NET resources, such as closing the reader.
	/// <p>
	/// Typically used for AdoTemplate's query methods (with RowMapperResultSetExtractor
	/// adapters).  RowMapper objects are typically stateless and thus
	/// reusable; they are ideal choices for implementing row-mapping logic in a single
	/// place.
	/// </p>
	/// <p>Alternatively, consider subclassing MappingSqlQuery from the Spring.Data.Object
	/// namespace:  Instead of working with separate AdoTemplate and RowMapper objects,
	/// you can have executable query objects (containing row-mapping logic) there.
	/// </p>
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public interface IRowMapper
	{
	    /// <summary>
	    /// Implementations must implement this method to map each row of data in the
	    /// result set (DataReader).  This method should not call Next() on the
	    /// DataReader; it should only extract the values of the current row.
	    /// </summary>
	    /// <param name="reader">The IDataReader to map (pre-initialized for the current row)</param>
	    /// <param name="rowNum">The number of the current row.</param>
	    /// <returns>The result object for the current row.</returns>
	    object MapRow(IDataReader reader, int rowNum);
	}
}
