#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
	/// Interface to be implemented by classes that can translate between properietary SQL exceptions
	/// and Spring.NET's data access strategy-agnostic <see cref="Spring.Dao.DataAccessException"/>.
	/// </summary>
	/// <remarks>
	///  <p>
	///  Implementations can be generic (for example, ADO.NET Exceptions) or proprietary (for example,
	///  using SQL Server or Oracle error codes) for greater precision.
	///  </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Mark Pollack</author>
	public interface IAdoExceptionTranslator
	{
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
		DataAccessException Translate( string task, string sql, Exception exception );
	}
}
