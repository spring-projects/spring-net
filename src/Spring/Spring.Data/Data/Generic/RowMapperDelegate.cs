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
    /// Callback delegate to process each row of data in a result set to an object.
    /// </summary>
    /// <typeparam name="T">The type of the object returned from the mapping operation.</typeparam>
    /// <param name="dataReader">The IDataReader to map</param>
    /// <param name="rowNum">the number of the current row.</param>
    /// <returns>An abrirary object, typically derived from data
    /// in the result set.</returns>
    public delegate T RowMapperDelegate<T>(IDataReader dataReader, int rowNum);
}

#endif
