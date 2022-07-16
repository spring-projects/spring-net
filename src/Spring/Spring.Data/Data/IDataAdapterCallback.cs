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
    /// Generic callback interface for code that operates on a
    /// IDbDataAdapter.
    /// </summary>
    /// <remarks>
    /// <p>Allows you to execute any number of operations
    /// on a IDbDataAdapter, for example to Fill a DataSet
    /// or other more advanced operations such as the transfering
    /// data between two different DataSets.
    /// </p>
    /// <p>Note that the passed in IDbDataAdapter
    /// has been created by the framework and its SelectCommand
    /// will be populated with values for CommandType and Text properties
    /// along with Connection/Transaction properties based on the
    /// calling transaction context.
    ///  </p>
    /// <see cref="Spring.Data.Core.AdoTemplate"/> Execute(IDataAdapterCallback dataAdapterCallback)
    /// method.
    /// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public interface IDataAdapterCallback
	{
        /// <summary>
        /// Called by AdoTemplate.Execute with an preconfigured
        /// ADO.NET IDbDataAdapter instance with its SelectCommand
        /// property populated with CommandType and Text values
        /// along with Connection/Transaction properties based on the
        /// calling transaction context.
        /// </summary>
        /// <param name="dataAdapter">An active IDbDataAdapter instance</param>
        /// <returns>The result object</returns>
        object DoInDataAdapter(IDbDataAdapter dataAdapter);
	}
}
