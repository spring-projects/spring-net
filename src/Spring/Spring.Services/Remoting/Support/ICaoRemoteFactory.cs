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

namespace Spring.Remoting.Support
{
	/// <summary>
	/// Interface for a CAO based object factory.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Provides a well known location for clients to retrieve
	/// references to CAO references.
	/// </p>
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
	/// <author>Mark Pollack</author>
	/// <author>Bruno Baia</author>
    public interface ICaoRemoteFactory
	{
        /// <summary>
        /// Returns the CAO proxy.
        /// </summary>
        /// <returns>The remote object.</returns>
        object GetObject();

        /// <summary>
        /// Returns the CAO proxy using the
        /// argument list to call the constructor.
        /// </summary>
        /// <remarks>
        /// The matching of arguments to call the constructor is done
        /// by type. The alternative ways, by index and by constructor
        /// name are not supported.
        /// </remarks>
        /// <param name="constructorArguments">Constructor
        /// arguments used to create the object.</param>
        /// <returns>The remote object.</returns>
        object GetObject(object[] constructorArguments);
	}
}
