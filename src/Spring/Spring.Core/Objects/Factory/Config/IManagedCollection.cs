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

using System.Collections;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Denotes a special placeholder collection that may contain
	/// <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>s or
	/// other placeholder objects that will need to be resolved.
	/// </summary>
	/// <remarks>
	/// <p>
	/// <c>'A special placeholder collection'</c> means that the elements of this
	/// collection can be placeholders for objects that will be resolved later by
	/// a Spring.NET IoC container, i.e. the elements themselves will be
	/// resolved at runtime by the enclosing IoC container.
	/// </p>
	/// <p>
	/// The core Spring.NET library already provides three implementations of this interface
	/// straight out of the box; they are...
	/// </p>
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// <see cref="Spring.Objects.Factory.Config.ManagedList"/>.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// <see cref="Spring.Objects.Factory.Config.ManagedDictionary"/>.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// <see cref="Spring.Objects.Factory.Config.ManagedSet"/>.
	/// </description>
	/// </item>
	/// </list>
	/// <p>
	/// If you have a custom collection class (i.e. a class that either implements the
	/// <see cref="System.Collections.ICollection"/> directly or derives from a class that does)
	/// that you would like to expose as a special placeholder collection (i.e. one that can
	/// have <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>s as elements
	/// that will be resolved at runtime by an appropriate Spring.NET IoC container, just
	/// implement this interface.
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// Lets say one has a <c>Bag</c> class (i.e. a collection that supports bag style semantics).
	/// </p>
	/// <code language="C#">
	/// using System;
	///
	/// using Spring.Objects.Factory.Support;
	///
	/// namespace MyNamespace
	/// {
	///		public sealed class Bag : ICollection
	///		{
	///			// ICollection implementation elided for clarity...
	///
	///			public void Add(object o)
	///			{
	///				// implementation elided for clarity...
	///			}
	///		}
	///
	///		public class ManagedBag : Bag, IManagedCollection
	///		{
	///			public ICollection Resolve(
	///				string objectName, RootObjectDefinition definition,
	///				string propertyName, ManagedCollectionElementResolver resolver)
	///			{
	///				Bag newBag = new Bag();
	///				string elementName = propertyName + "[bag-element]";
	///				foreach(object element in this)
	///				{
	///					object resolvedElement = resolver(objectName, definition, elementName, element);
	///					newBag.Add(resolvedElement);
	///				}
	///				return newBag;
	///			}
	///		}
	/// }
	/// </code>
	/// </example>
	/// <author>Rick Evans</author>
	public interface IManagedCollection : ICollection
	{
		/// <summary>
		/// Resolves this managed collection at runtime.
		/// </summary>
		/// <param name="objectName">
		/// The name of the top level object that is having the value of one of it's
		/// collection properties resolved.
		/// </param>
		/// <param name="definition">
		/// The definition of the named top level object.
		/// </param>
		/// <param name="propertyName">
		/// The name of the property the value of which is being resolved.
		/// </param>
		/// <param name="resolver">
		/// The callback that will actually do the donkey work of resolving
		/// this managed collection.
		/// </param>
		/// <returns>A fully resolved collection.</returns>
		ICollection Resolve(string objectName, IObjectDefinition definition,
			string propertyName, ManagedCollectionElementResolver resolver);
	}

	/// <summary>
	/// Resolves a single element value of a managed collection.
	/// </summary>
	/// <remarks>
	/// <p>
	/// If the <paramref name="element"/> does not need to be resolved or
	/// converted to an appropriate <see cref="System.Type"/>, the
	/// <paramref name="element"/> will be returned as-is.
	/// </p>
	/// </remarks>
	/// <param name="name">
	/// The name of the top level object that is having the value of one of it's
	/// collection properties resolved.
	/// </param>
	/// <param name="definition">
	/// The definition of the named top level object.
	/// </param>
	/// <param name="argumentName">
	/// The name of the property the value of which is being resolved.
	/// </param>
	/// <param name="element">
	/// That element of a managed collection that may need to be resolved
	/// to a concrete value.
	/// </param>
	/// <returns>A fully resolved element.</returns>
	public delegate object ManagedCollectionElementResolver(
		string name, IObjectDefinition definition, string argumentName, object element);
}
