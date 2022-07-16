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
    /// Called by the AdoTemplate class to allow implementations
    /// to set any necessary properties on the DbDataAdapter object.
    /// </summary>
    /// <remarks>
    /// <p>For example, this callback can be used to set the
    /// AcceptChangesDuringFill property, register an event handler
    /// for the FillErrors event, set update batch sizes if your provider
    /// supports such functionality, set the ContinueUpdateOnError property,
    /// or in .NET 2.0 to set the property AcceptChangesDuringUpdate.
    /// </p>
    /// <p>
    /// Downcast to the appropriate subtype to access vendor specific
    /// functionality.</p>
    /// <p>
    /// The DataAdapter SelectCommand will be already be
    /// populated with values for CommandType and Text properties
    /// along with Connection/Transaction properties based on the
    /// calling transaction context.
    /// </p>
    /// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public interface IDataAdapterSetter
	{
        void SetValues(IDbDataAdapter dataAdapter);
	}
}
