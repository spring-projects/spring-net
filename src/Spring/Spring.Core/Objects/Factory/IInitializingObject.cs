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

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Defines a simple initialization callback for objects that need to to some
	/// post-initialization logic after all of their dependencies have been injected.
	/// </summary>
	/// <remarks>
	/// <p>
	/// An implementation of the
	/// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet()"/>
	/// method might perform some additional custom initialization (over and above that
	/// performed by the constructor), or merely check that all mandatory properties
	/// have been set (this last example is a very typical use case of this interface).
	/// </p>
	/// <note>
	/// The use of the
	/// <see cref="Spring.Objects.Factory.IInitializingObject"/> interface
	/// by non-Spring.NET framework code can be avoided (and is generally
	/// discouraged). The Spring.NET container provides support for a generic
	/// initialization method given to the object definition in the object
	/// configuration store (be it XML, or a database, etc). This requires
	/// slightly more configuration (one attribute-value pair in the case of
	/// XML configuration), but removes any dependency on Spring.NET from the
	/// class definition.
	/// </note>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	/// <seealso cref="Spring.Objects.Factory.IObjectFactory"/>
	public interface IInitializingObject
	{
		/// <summary>
		/// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// after it has injected all of an object's dependencies.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This method allows the object instance to perform the kind of
		/// initialization only possible when all of it's dependencies have
		/// been injected (set), and to throw an appropriate exception in the
		/// event of misconfiguration.
		/// </p>
		/// <p>
		/// Please do consult the class level documentation for the
		/// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
		/// description of exactly <i>when</i> this method is invoked. In
		/// particular, it is worth noting that the
		/// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
		/// and <see cref="Spring.Context.IApplicationContextAware"/>
		/// callbacks will have been invoked <i>prior</i> to this method being
		/// called.
		/// </p>
		/// </remarks>
		/// <exception cref="System.Exception">
		/// In the event of misconfiguration (such as the failure to set a
		/// required property) or if initialization fails.
		/// </exception>
		void AfterPropertiesSet();
	}
}