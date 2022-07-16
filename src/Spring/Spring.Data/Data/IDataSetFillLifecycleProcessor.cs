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
	/// Lifecycle callback methods that can be registered when
	/// performing Fill operations with AdoTemplate.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The methods let you set various properties and invoke
	/// methods on the DataSet before and after it gets filled by a DataAdapter.
	/// For example, EnforceConstraints, BeginLoadData, and EndLoadData
	/// can be called to optimize the loading of large DataSets with
	/// many related tables.
	/// </p>
	/// <p>Vendors may expose other propriety methods on their DataSet
	/// implementation, downcast to access this specific functionality.
	/// </p>
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public interface IDataSetFillLifecycleProcessor
	{
	    /// <summary>
	    /// Called before a DataAdapter is used to fill a DataSet
	    /// the the provided tablename.
	    /// </summary>
	    /// <param name="ds">The DataSet to be filled with a DataTable</param>
	    /// <param name="tableMappingCollection">The table collection to be filled</param>
        void BeforeFill(DataSet ds, ITableMappingCollection tableMappingCollection);

        /// <summary>
        /// Called after a DataAdapter is used to fill a DataSet
        /// the the provided tablename.
        /// </summary>
        /// <param name="ds">The DataSet to be filled with a DataTable</param>
        /// <param name="tableMappingCollection">The table collection to be filled</param>
        void AfterFill(DataSet ds, ITableMappingCollection tableMappingCollection);

	}
}
