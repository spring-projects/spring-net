#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

#if NET_2_0

using System.Data;

namespace Spring.Data.Generic
{
    /// <summary>
    /// Callback interface to process all results sets and rows in
    /// an AdoTemplate query method.
    /// </summary>
    /// <remarks>Implementations of this interface perform the work
    /// of extracting results but don't need worry about managing
    /// ADO.NET resources, such as closing the reader. 
    /// <p>
    /// This interface is mainly used within the ADO.NET
    /// framework.  An IResultSetExtractor is usually a simpler choice
    /// for result set (DataReader) processing, in particular a 
    /// RowMapperResultSetExtractor in combination with a IRowMapper.
    /// </p>
    /// <para>Note: in contracts to a IRowCallbackHandler, a ResultSetExtractor
    /// is usually stateless and thus reusable, as long as it doesn't access
    /// stateful resources or keep result state within the object.
    /// </para>
    /// </remarks>
    /// 
    public interface IResultSetExtractor<T>
    {
        /// <summary>
        /// Implementations must implement this method to process all
        /// result set and rows in the IDataReader.
        /// </summary>
        /// <param name="reader">The IDataReader to extract data from.
        /// Implementations should not close this: it will be closed
        /// by the AdoTemplate.</param>
        /// <returns>An arbitrary result object or null if none.  The
        /// extractor will typically be stateful in the latter case.</returns>
        T ExtractData(IDataReader reader);
        

    }
}

#endif