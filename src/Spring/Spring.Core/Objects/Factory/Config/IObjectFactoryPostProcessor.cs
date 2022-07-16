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

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Allows for custom modification of an application context's object
	/// definitions, adapting the object property values of the context's
	/// underlying object factory.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Application contexts can auto-detect
    /// <c>IObjectFactoryPostProcessor</c> objects in their object definitions and
    /// apply them before any other objects get created.
    /// </p>
    /// <p>
    /// Useful for custom config files targeted at system administrators that
    /// override object properties configured in the application context.
    /// </p>
    /// <p>
    /// See PropertyResourceConfigurer and its concrete implementations for
    /// out-of-the-box solutions that address such configuration needs.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.Net)</author>
	public interface IObjectFactoryPostProcessor
    {
        /// <summary>
        /// Modify the application context's internal object factory after its
        /// standard initialization.
        /// </summary>
        /// <remarks>
        /// <p>
        /// All object definitions will have been loaded, but no objects will have
        /// been instantiated yet. This allows for overriding or adding properties
        /// even to eager-initializing objects.
        /// </p>
        /// </remarks>
        /// <param name="factory">
        /// The object factory used by the application context.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        void PostProcessObjectFactory (IConfigurableListableObjectFactory factory);
	}
}
