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

namespace Spring.Data.Objects.Generic
{
    /// <summary>
    /// Reusable query in which concrete subclasses must implement the
    /// MapRow method to map each row of a single result set into an
    /// object.
    /// </summary>
    /// <remarks>
    /// Simplifies MappingSqlQueryWithContext API by dropping parameters and
    /// context. Most subclasses won't care about parameters. If you don't use
    /// contextual information, subclass this instead of MappingSqlQueryWithContext.
    /// </remarks>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class MappingAdoQuery : MappingAdoQueryWithContext
    {
        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingAdoQuery"/> class.
        /// </summary>
        public MappingAdoQuery()
        {
        }

        public MappingAdoQuery(IDbProvider dbProvider, string sql) : base(dbProvider, sql)
        {
        }

        #endregion

        #region Methods

        protected override T MapRow<T>(IDataReader reader, int rowNum, IDictionary inParams,
                                       IDictionary callingContext)
        {
            return MapRow<T>(reader, rowNum);
        }

        protected abstract T MapRow<T>(IDataReader reader, int num);

        #endregion
    }
}
