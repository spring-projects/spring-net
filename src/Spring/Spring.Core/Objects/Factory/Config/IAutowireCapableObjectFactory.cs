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

#region Imports

using System;
using System.Collections.Generic;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	///     Extension of the <see cref="Spring.Objects.Factory.IObjectFactory" />
	///     interface to be implemented by object factories that are capable of
	///     autowiring and expose this functionality for existing object instances.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public interface IAutowireCapableObjectFactory : IObjectFactory
    {
	    /// <summary>
	    ///     Create a new object instance of the given class with the specified
	    ///     autowire strategy.
	    /// </summary>
	    /// <param name="type">
	    ///     The <see cref="System.Type" /> of the object to instantiate.
	    /// </param>
	    /// <param name="autowireMode">
	    ///     The desired autowiring mode.
	    /// </param>
	    /// <param name="dependencyCheck">
	    ///     Whether to perform a dependency check for objects (not applicable to
	    ///     autowiring a constructor, thus ignored there).
	    /// </param>
	    /// <returns>The new object instance.</returns>
	    /// <exception cref="Spring.Objects.ObjectsException">
	    ///     If the wiring fails.
	    /// </exception>
	    /// <seealso cref="Spring.Objects.Factory.Config.AutoWiringMode" />
	    object Autowire(Type type, AutoWiringMode autowireMode, bool dependencyCheck);

	    /// <summary>
	    ///     Autowire the object properties of the given object instance by name or
	    ///     <see cref="System.Type" />.
	    /// </summary>
	    /// <param name="instance">
	    ///     The existing object instance.
	    /// </param>
	    /// <param name="autowireMode">
	    ///     The desired autowiring mode.
	    /// </param>
	    /// <param name="dependencyCheck">
	    ///     Whether to perform a dependency check for the object.
	    /// </param>
	    /// <exception cref="Spring.Objects.ObjectsException">
	    ///     If the wiring fails.
	    /// </exception>
	    /// <seealso cref="Spring.Objects.Factory.Config.AutoWiringMode" />
	    void AutowireObjectProperties(object instance, AutoWiringMode autowireMode, bool dependencyCheck);

	    /// <summary>
	    ///     Apply <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor" />s
	    ///     to the given existing object instance, invoking their
	    ///     <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessBeforeInitialization" />
	    ///     methods.
	    /// </summary>
	    /// <remarks>
	    ///     <p>
	    ///         The returned object instance may be a wrapper around the original.
	    ///     </p>
	    /// </remarks>
	    /// <param name="instance">
	    ///     The existing object instance.
	    /// </param>
	    /// <param name="name">
	    ///     The name of the object.
	    /// </param>
	    /// <returns>
	    ///     The object instance to use, either the original or a wrapped one.
	    /// </returns>
	    /// <exception cref="Spring.Objects.ObjectsException">
	    ///     If any post-processing failed.
	    /// </exception>
	    /// <seealso cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessBeforeInitialization" />
	    object ApplyObjectPostProcessorsBeforeInitialization(object instance, string name);

	    /// <summary>
	    ///     Apply <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor" />s
	    ///     to the given existing object instance, invoking their
	    ///     <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessAfterInitialization" />
	    ///     methods.
	    /// </summary>
	    /// <remarks>
	    ///     <p>
	    ///         The returned object instance may be a wrapper around the original.
	    ///     </p>
	    /// </remarks>
	    /// <param name="instance">
	    ///     The existing object instance.
	    /// </param>
	    /// <param name="name">
	    ///     The name of the object.
	    /// </param>
	    /// <returns>
	    ///     The object instance to use, either the original or a wrapped one.
	    /// </returns>
	    /// <exception cref="Spring.Objects.ObjectsException">
	    ///     If any post-processing failed.
	    /// </exception>
	    /// <seealso cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessAfterInitialization" />
	    object ApplyObjectPostProcessorsAfterInitialization(object instance, string name);

	    /// <summary>
	    ///     Resolve the specified dependency against the objects defined in this factory.
	    /// </summary>
	    /// <param name="descriptor">The descriptor for the dependency.</param>
	    /// <param name="objectName">Name of the object which declares the present dependency.</param>
	    /// <param name="autowiredObjectNames">
	    ///     A list that all names of autowired object (used for
	    ///     resolving the present dependency) are supposed to be added to.
	    /// </param>
	    /// <returns>the resolved object, or <code>null</code> if none found</returns>
	    /// <exception cref="ObjectsException">if dependency resolution failed</exception>
	    object ResolveDependency(
		    DependencyDescriptor descriptor,
		    string objectName,
		    IList<string> autowiredObjectNames);
    }
}