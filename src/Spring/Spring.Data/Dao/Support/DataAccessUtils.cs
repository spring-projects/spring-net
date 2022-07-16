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
using Spring.Util;

namespace Spring.Dao.Support
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

	    public static object RequiredUniqueResultSet(IList results)
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
	        IEnumerator enumerator = results.GetEnumerator();
	        enumerator.MoveNext();
	        return enumerator.Current;
	    }

        /// <summary>
        /// Return a translated exception if this is appropriate, or null if the exception could
        /// not be translated.
        /// </summary>
        /// <param name="rawException">The raw exception we may wish to translate.</param>
        /// <param name="pet">The PersistenceExceptionTranslator to use to perform the translation.</param>
        /// <returns>A translated exception if translation is possible, or or null if the exception could
        /// not be translated.</returns>
        public static DataAccessException TranslateIfNecessary(Exception rawException, IPersistenceExceptionTranslator pet)
        {
            AssertUtils.ArgumentNotNull(pet, "PersistenceExceptionTranslator must not be null");
            return pet.TranslateExceptionIfPossible(rawException);
        }

	    #endregion
	}

}
