#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
    /// IDbCommand. 
	/// </summary>
	/// <remarks>
	/// <p>Allows you to execute any number of operations
	/// on a single IDbCommand, for example a single ExecuteScalar
	/// call or repeated execute calls with varying parameters.
	/// </p>
	/// <p>Used internally by AdoTemplate, but also useful for 
	/// application code.  Note that the passed in IDbCommand
	/// has been created by the framework.  </p>
	/// </remarks>
	/// <author>Mark Pollack</author>
	public interface ICommandCallback
	{
        /// <summary>
        /// Called by AdoTemplate.Execute with an active ADO.NET IDbCommand.
        /// The calling code does not need to care about closing the 
        /// command or the connection, or
        /// about handling transactions:  this will all be handled by 
        /// Spring's AdoTemplate
        /// </summary>
        /// <param name="command">An active IDbCommand instance</param>
        /// <returns>The result object</returns>
        object DoInCommand(IDbCommand command);
	}
}
