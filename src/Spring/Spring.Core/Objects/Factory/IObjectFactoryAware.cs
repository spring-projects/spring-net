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



#endregion

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Interface to be implemented by objects that wish to be aware of their owning
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/>.
	/// </summary>
	/// <remarks>
	/// <p>
	/// For example, objects can look up collaborating objects via the factory.
	/// </p>
	/// <p>
	/// Note that most objects will choose to receive references to collaborating
	/// objects via respective properties and / or an appropriate constructor.
	/// </p>
	/// <p>
	/// For a list of all object lifecycle methods, see the
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/> API documentation.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	public interface IObjectFactoryAware
	{
		/// <summary>
		/// Callback that supplies the owning factory to an object instance.
		/// </summary>
		/// <value>
		/// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// (may not be <see langword="null"/>). The object can immediately
		/// call methods on the factory.
		/// </value>
		/// <remarks>
		/// <p>
		/// Invoked after population of normal object properties but before an init
		/// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
		/// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
		/// method or a custom init-method.
		/// </p>
		/// </remarks>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of initialization errors.
		/// </exception>
		IObjectFactory ObjectFactory { set; }
	}
}