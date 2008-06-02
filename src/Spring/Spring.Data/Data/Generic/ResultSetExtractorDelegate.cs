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
    /// Callback delegate to process all result sets and row in an 
    /// AdoTemplate query method.
    /// </summary>
    /// <typeparam name="T">The type returned from the result set mapping.</typeparam>
    /// <param name="reader">The IDataReader to extract data from.
    /// Implementations should not close this: it will be closed
    /// by the AdoTemplate.</param>
    /// <returns>An arbitrary result object or null if none.</returns>
    public delegate T ResultSetExtractorDelegate<T>(IDataReader reader);
}

#endif
