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

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Allows for custom modification of new object instances, e.g.
	/// checking for marker interfaces or wrapping them with proxies.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Application contexts can auto-detect
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
	/// objects in their object definitions and apply them before any other
	/// objects get created. Plain object factories allow for programmatic
	/// registration of post-processors.
	/// </p>
	/// <p>
	/// Typically, post-processors that populate objects via marker interfaces
	/// or the like will implement
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessBeforeInitialization"/>,
	/// and post-processors that wrap objects with proxies will normally implement
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessAfterInitialization"/>.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <see cref="Spring.Objects.Factory.Config.IInstantiationAwareObjectPostProcessor"/>
	public interface IObjectPostProcessor
	{
		/// <summary>
		/// Apply this <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
		/// to the given new object instance <i>before</i> any object initialization callbacks.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The object will already be populated with property values.
		/// The returned object instance may be a wrapper around the original.
		/// </p>
		/// </remarks>
		/// <param name="instance">
		/// The new object instance.
		/// </param>
		/// <param name="name">
		/// The name of the object.
		/// </param>
		/// <returns>
		/// The object instance to use, either the original or a wrapped one.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of errors.
		/// </exception>
		object PostProcessBeforeInitialization(object instance, string name);

		/// <summary>
		/// Apply this <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/> to the
		/// given new object instance <i>after</i> any object initialization callbacks.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The object will already be populated with property values. The returned object
		/// instance may be a wrapper around the original.
		/// </p>
		/// </remarks>
		/// <param name="instance">
		/// The new object instance.
		/// </param>
		/// <param name="objectName">
		/// The name of the object.
		/// </param>
		/// <returns>
		/// The object instance to use, either the original or a wrapped one.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of errors.
		/// </exception>
		object PostProcessAfterInitialization(object instance, string objectName);
	}
}