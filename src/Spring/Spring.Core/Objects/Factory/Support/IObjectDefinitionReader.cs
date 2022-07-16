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

using Spring.Core.IO;

namespace Spring.Objects.Factory.Support {

	/// <summary>
	/// Simple interface for object definition readers.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans</author>
	public interface IObjectDefinitionReader
    {
        /// <summary>
        /// Gets the
        /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
        /// instance that this reader works on.
        /// </summary>
        IObjectDefinitionRegistry Registry
        {
            get;
        }

        /// <summary>
        /// The <see cref="System.AppDomain"/> against which any class names
        /// will be resolved into <see cref="System.Type"/> instances.
        /// </summary>
        AppDomain Domain
        {
            get;
        }

        /// <summary>
        /// The <see cref="IObjectNameGenerator"/> to use for anonymous
        /// objects (wihtout explicit object name specified).
        /// </summary>
        IObjectNameGenerator ObjectNameGenerator
        {
            get;
        }

	    /// <summary>
	    /// Gets the resource loader to use for resource locations.
	    /// </summary>
	    /// <remarks>There is also a <see cref="LoadObjectDefinitions(string)"/> method
	    /// available for loading object definitions from a resource location.  This is
	    /// a convenience to avoid explicit ResourceLoader handling.</remarks>
	    /// <value>The resource loader.</value>
        IResourceLoader ResourceLoader
        {
            get;
        }

        /// <summary>
        /// Load object definitions from the supplied <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">
        /// The resource for the object definitions that are to be loaded.
        /// </param>
        /// <returns>
        /// The number of object definitions found
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of loading or parsing errors.
        /// </exception>
        int LoadObjectDefinitions (IResource resource);

        /// <summary>
        /// Load object definitions from the supplied <paramref name="resources"/>.
        /// </summary>
        /// <param name="resources">
        /// The resources for the object definitions that are to be loaded.
        /// </param>
        /// <returns>
        /// The number of object definitions found
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of loading or parsing errors.
        /// </exception>
	    int LoadObjectDefinitions(IResource[] resources);


        /// <summary>
        /// Loads the object definitions from the specified resource location.
        /// </summary>
        /// <param name="location">The resource location, to be loaded with the
        /// IResourceLoader location .</param>
        /// <returns>
        /// The number of object definitions found
        /// </returns>
        int LoadObjectDefinitions(string location);

        /// <summary>
        /// Loads the object definitions from the specified resource locations.
        /// </summary>
        /// <param name="locations">The the resource locations to be loaded with the
        /// IResourceLoader of this object definition reader.</param>
        /// <returns>
        /// The number of object definitions found
        /// </returns>
        int LoadObjectDefinitions(string[] locations);
	}
}
