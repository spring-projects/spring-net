#region Licence

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

using Spring.Dao;

namespace Spring.Data.Support
{
	/// <summary>
    /// Translates all exceptions to an UncategorizedAdoException.
    /// </summary>
    /// <remarks>
    /// This exception translator used when an exception is thrown using the
	/// "primary" implementation of IAdoExceptionTranslator, i.e. AdoExceptionTranslator.
    /// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public class FallbackExceptionTranslator : IAdoExceptionTranslator
	{


		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="FallbackExceptionTranslator"/> class.
        /// </summary>
		public FallbackExceptionTranslator()
		{
		}

		#endregion


	    /// <summary>
	    /// Translate the given <see cref="System.SystemException"/> into a generic data access exception.
	    /// </summary>
	    /// <param name="task">A readable string describing the task being attempted.</param>
	    /// <param name="sql">The SQL query or update that caused the problem. May be null.</param>
	    /// <param name="exception">
	    /// The <see cref="System.Exception"/> encountered by the ADO.NET implementation.
	    /// </param>
	    /// <returns>
	    /// A <see cref="Spring.Dao.DataAccessException"/> appropriate for the supplied
	    /// <paramref name="exception"/>.
	    /// </returns>
	    public DataAccessException Translate(string task, string sql, Exception exception)
	    {
	        return new UncategorizedAdoException(task, sql, "<no error code>", exception);
	    }
	}
}
