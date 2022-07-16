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
	/// This interface creates a IDbDataAdapterCommand.
	/// Implementations are responsible
	/// for configuring the created command with appropriate
	/// select and actions commands along with their parameters.
	/// </summary>
	/// <remarks>
	/// Generally used to to support the DataSet functionality in
	/// the Spring.Data.Objects namespace.
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public interface IDbDataAdapterCreator
	{
        /// <summary>
        /// Creates the data adapter.
        /// </summary>
        /// <returns>A new IDbDataAdapter instance</returns>
	    IDbDataAdapter CreateDataAdapter();
	}
}
