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

using Spring.Util.Generic;

namespace Spring.Dao.Support.Generic
{
	/// <summary>
	/// Miscellaneous utility methods for DAO implementations.
	/// Useful with any data access technology.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class DataAccessUtils
	{

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="DataAccessUtils"/> class.
                /// </summary>
		public 	DataAccessUtils()
		{

		}

		#endregion

		#region Methods

	    public static T RequiredUniqueResultSet<T>(IList<T> results)
	    {
	        int size = (results != null ? results.Count : 0);
	        if (size == 0)
	        {
	            throw new EmptyResultDataAccessException(1);
	        }
	        if ( ! CollectionUtils.HasUniqueObject(results))
	        {
	            throw new IncorrectResultSizeDataAccessException(1, size);
	        }
            IEnumerator<T> enumerator = results.GetEnumerator();
	        enumerator.MoveNext();
	        return enumerator.Current;
	    }

	    #endregion
	}

}
