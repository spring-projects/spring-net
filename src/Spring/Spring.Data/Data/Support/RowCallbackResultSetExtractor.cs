#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

namespace Spring.Data.Support
{
    /// <summary>
    /// Adapter to enable use of a IRowCallback inside a ResultSetExtractor.
    /// </summary>
    /// <remarks>We don't use it for navigating since this could lead to unpredictable consequences.</remarks>
    /// <author>Mark Pollack</author>
    public class RowCallbackResultSetExtractor : IResultSetExtractor
    {
        private IRowCallback rowCallback;

        private RowCallbackDelegate rowCallbackDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowCallbackResultSetExtractor"/> class.
        /// </summary>
        /// <param name="rowCallback">The row callback.</param>
        public RowCallbackResultSetExtractor(IRowCallback rowCallback)
        {
            AssertUtils.ArgumentNotNull(rowCallback, "rowCallback");
            this.rowCallback = rowCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowCallbackResultSetExtractor"/> class.
        /// </summary>
        /// <param name="rowCallbackDelegate">The row callback delegate.</param>
        public RowCallbackResultSetExtractor(RowCallbackDelegate rowCallbackDelegate)
        {
            AssertUtils.ArgumentNotNull(rowCallbackDelegate, "rowCallbackDelegate");
            this.rowCallbackDelegate = rowCallbackDelegate;
        }

        /// <summary>
        /// All rows of the data reader are passed to the IRowCallback
        /// associated with this class.
        /// </summary>
        /// <param name="reader">The IDataReader to extract data from.
        /// Implementations should not close this: it will be closed
        /// by the AdoTemplate.</param>
        /// <returns>
        /// Null is returned since IRowCallback manages its own state.
        /// </returns>
        public object ExtractData(IDataReader reader)
        {
            if (rowCallback != null)
            {
                while (reader.Read())
                {
                    rowCallback.ProcessRow(reader);
                }
            }
            else
            {
                while (reader.Read())
                {
                    rowCallbackDelegate(reader);
                }
            }

            return null;
        }
    }
}