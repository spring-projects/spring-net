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

using System.Reflection;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Responsible for creating instances corresponding to a
	/// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	public interface IInstantiationStrategy
	{
		/// <summary>
		/// Instantiate an instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
		/// </summary>
		/// <param name="definition">
		/// The definition of the object that is to be instantiated.
		/// </param>
		/// <param name="name">
		/// The name associated with the object definition. The name can be the null
		/// or zero length string if we're autowiring an object that doesn't belong
		/// to the supplied <paramref name="factory"/>.
		/// </param>
		/// <param name="factory">
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// </param>
		/// <returns>
		/// An instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
		/// </returns>
		object Instantiate(
			RootObjectDefinition definition, string name, IObjectFactory factory);

		/// <summary>
		/// Instantiate an instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
		/// </summary>
		/// <param name="definition">
		/// The definition of the object that is to be instantiated.
		/// </param>
		/// <param name="name">
		/// The name associated with the object definition. The name can be the null
		/// or zero length string if we're autowiring an object that doesn't belong
		/// to the supplied <paramref name="factory"/>.
		/// </param>
		/// <param name="factory">
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// </param>
		/// <param name="constructor">
		/// The <see cref="System.Reflection.ConstructorInfo"/> to be used to instantiate
		/// the object.
		/// </param>
		/// <param name="arguments">
		/// Any arguments to the supplied <paramref name="constructor"/>. May be null.
		/// </param>
		/// <returns>
		/// An instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
		/// </returns>
		object Instantiate(
			RootObjectDefinition definition, string name, IObjectFactory factory,
			ConstructorInfo constructor, object[] arguments);

		/// <summary>
		/// Instantiate an instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
		/// </summary>
		/// <param name="definition">
		/// The definition of the object that is to be instantiated.
		/// </param>
		/// <param name="name">
		/// The name associated with the object definition. The name can be the null
		/// or zero length string if we're autowiring an object that doesn't belong
		/// to the supplied <paramref name="factory"/>.
		/// </param>
		/// <param name="factory">
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// </param>
		/// <param name="factoryMethod">
		/// The <see cref="System.Reflection.MethodInfo"/> to be used to get the object.
		/// </param>
		/// <param name="arguments">
		/// Any arguments to the supplied <paramref name="factoryMethod"/>. May be null.
		/// </param>
		/// <returns>
		/// An instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
		/// </returns>
		object Instantiate(
			RootObjectDefinition definition, string name, IObjectFactory factory,
			MethodInfo factoryMethod, object[] arguments);
	}
}
