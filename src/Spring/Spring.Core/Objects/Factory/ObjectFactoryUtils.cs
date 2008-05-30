#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Collections;
using Spring.Collections;
using Spring.Core;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Convenience methods operating on object factories, returning object instances,
	/// names, or counts.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The nesting hierarchy of an object factory is taken into account by the various methods
	/// exposed by this class.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: ObjectFactoryUtils.cs,v 1.17 2007/07/31 03:47:39 markpollack Exp $</version>
	public sealed class ObjectFactoryUtils
	{
		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectFactoryUtils"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly visible
		/// constructors.
		/// </p>
		/// </remarks>
		private ObjectFactoryUtils()
		{
		}

		// CLOVER:ON

		#endregion

		/// <summary>
		/// Used to dereference an <see cref="Spring.Objects.Factory.IFactoryObject"/>
		/// and distinguish it from managed objects <i>created by</i> the factory.
		/// </summary>
		/// <remarks>
		/// <p>
		/// For example, if the managed object identified as <code>foo</code> is a
		/// factory, getting <code>&amp;foo</code> will return the factory, not the
		/// instance returned by the factory.
		/// </p>
		/// </remarks>
		public const string FactoryObjectPrefix = "&";

		/// <summary>
		/// Count all object definitions in any hierarchy in which this
		/// factory participates.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Includes counts of ancestor object factories.
		/// </p>
		/// <p>
		/// Objects that are "overridden" (specified in a descendant factory
		/// with the same name) are counted only once.
		/// </p>
		/// </remarks>
		/// <param name="factory">The object factory.</param>
		/// <returns>
		/// The count of objects including those defined in ancestor factories.
		/// </returns>
		public static int CountObjectsIncludingAncestors(IListableObjectFactory factory)
		{
			return ObjectNamesIncludingAncestors(factory).Length;
		}

		/// <summary>
		/// Return all object names in the factory, including ancestor factories.
		/// </summary>
		/// <param name="factory">The object factory.</param>
		/// <returns>The array of object names, or an empty array if none.</returns>
		public static string[] ObjectNamesIncludingAncestors(IListableObjectFactory factory)
		{
			Set result = new HashedSet();
			result.AddAll(factory.GetObjectDefinitionNames());
			IListableObjectFactory pof = GetParentFactoryIfAny(factory);
			if (pof != null)
			{
				string[] parentsResult = ObjectNamesIncludingAncestors(pof);
				result.AddAll(parentsResult);
			}
			return ToArrayOfObjectNames(result);
		}

		private static string[] ToArrayOfObjectNames(Set result)
		{
			Array resultArray = Array.CreateInstance(typeof (string), result.Count);
			result.CopyTo(resultArray, 0);
			return (string[]) resultArray;
		}

		/// <summary>
		/// Get all object names for the given type, including those defined in ancestor
		/// factories.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Will return unique names in case of overridden object definitions.
		/// </p>
		/// <p>
		/// Does consider objects created by <see cref="Spring.Objects.Factory.IFactoryObject"/>s 
        /// if <paramref name="includeFactoryObjects"/> is set to true, 
        /// which means that <see cref="Spring.Objects.Factory.IFactoryObject"/>s will get initialized.
		/// </p>
		/// </remarks>
		/// <param name="factory">
		/// If this isn't also an
		/// <see cref="Spring.Objects.Factory.IHierarchicalObjectFactory"/>,
		/// this method will return the same as it's own
		/// <see cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectDefinitionNames"/>
		/// method.
		/// </param>
		/// <param name="type">
		/// The <see cref="System.Type"/> that objects must match.
		/// </param>
        /// <param name="includePrototypes">
		/// Whether to include prototype objects too or just singletons
		/// (also applies to <see cref="Spring.Objects.Factory.IFactoryObject"/> instances).
		/// </param>
		/// <param name="includeFactoryObjects">
		/// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/> instances
		/// too or just normal objects.
		/// </param>
		/// <returns>
		/// The array of object names, or an empty array if none.
		/// </returns>
		public static string[] ObjectNamesForTypeIncludingAncestors(
			IListableObjectFactory factory, Type type,
            bool includePrototypes, bool includeFactoryObjects)
		{
			Set result = new HashedSet();
			result.AddAll(factory.GetObjectNamesForType(type, includePrototypes, includeFactoryObjects));
			IListableObjectFactory pof = GetParentFactoryIfAny(factory);
			if (pof != null)
			{
                string[] parentsResult = ObjectNamesForTypeIncludingAncestors(pof, type, includePrototypes, includeFactoryObjects);
				result.AddAll(parentsResult);
			}
			return ToArrayOfObjectNames(result);
		}

        /// <summary>
        /// Get all object names for the given type, including those defined in ancestor
        /// factories.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Will return unique names in case of overridden object definitions.
        /// </p>
        /// <p>
        /// Does consider objects created by <see cref="Spring.Objects.Factory.IFactoryObject"/>s,
        /// or rather it considers the type of objects created by
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> (which means that
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s will be instantiated).
        /// </p>
        /// </remarks>
        /// <param name="factory">
        /// If this isn't also an
        /// <see cref="Spring.Objects.Factory.IHierarchicalObjectFactory"/>,
        /// this method will return the same as it's own
        /// <see cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectDefinitionNames"/>
        /// method.
        /// </param>
        /// <param name="type">
        /// The <see cref="System.Type"/> that objects must match.
        /// </param>
        /// <returns>
        /// The array of object names, or an empty array if none.
        /// </returns>
        public static string[] ObjectNamesForTypeIncludingAncestors(
            IListableObjectFactory factory, Type type)
        {
            Set result = new HashedSet();
            result.AddAll(factory.GetObjectNamesForType(type));
            IListableObjectFactory pof = GetParentFactoryIfAny(factory);
            if (pof != null)
            {
                string[] parentsResult = ObjectNamesForTypeIncludingAncestors(pof, type);
                result.AddAll(parentsResult);
            }
            return ToArrayOfObjectNames(result);
        }

		private static IListableObjectFactory GetParentFactoryIfAny(IListableObjectFactory factory)
		{
			IHierarchicalObjectFactory hierFactory = factory as IHierarchicalObjectFactory;
			if (hierFactory != null)
			{
				return
					hierFactory.ParentObjectFactory as IListableObjectFactory;
			}
			return null;
		}

		/// <summary>
		/// Return all objects of the given type or subtypes, also picking up objects
		/// defined in ancestor object factories if the current object factory is an
		/// <see cref="Spring.Objects.Factory.IHierarchicalObjectFactory"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The return list will only contain objects of this type.
		/// Useful convenience method when we don't care about object names.
		/// </p>
		/// </remarks>
		/// <param name="factory">The object factory.</param>
		/// <param name="type">The <see cref="System.Type"/> of object to match.</param>
		/// <param name="includePrototypes">
		/// Whether to include prototype objects too or just singletons
		/// (also applies to <see cref="Spring.Objects.Factory.IFactoryObject"/> instances).
		/// </param>
		/// <param name="includeFactoryObjects">
		/// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/> instances
		/// too or just normal objects.
		/// </param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the objects could not be created.
		/// </exception>
		/// <returns>
		/// The <see cref="System.Collections.IDictionary"/> of object instances, or an
		/// empty <see cref="System.Collections.IDictionary"/> if none.
		/// </returns>
		public static IDictionary ObjectsOfTypeIncludingAncestors(
			IListableObjectFactory factory, Type type,
			bool includePrototypes, bool includeFactoryObjects)
		{
			Hashtable result = new Hashtable();
			foreach (DictionaryEntry entry in
				factory.GetObjectsOfType(type, includePrototypes, includeFactoryObjects))
			{
				result.Add(entry.Key, entry.Value);
			}
			IListableObjectFactory pof = GetParentFactoryIfAny(factory);
			if (pof != null)
			{
				IDictionary parentResult
					= ObjectsOfTypeIncludingAncestors(
						pof, type, includePrototypes, includeFactoryObjects);
				foreach (object instance in parentResult.Keys)
				{
					if (!result.ContainsKey(instance))
					{
						result.Add(instance, parentResult[instance]);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Return a single object of the given type or subtypes, also picking up objects defined
		/// in ancestor object factories if the current object factory is an
		/// <see cref="Spring.Objects.Factory.IHierarchicalObjectFactory"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Useful convenience method when we expect a single object and don't care
		/// about the object name.
		/// </p>
		/// </remarks>
		/// <param name="factory">The object factory.</param>
		/// <param name="type">The <see cref="System.Type"/> of object to match.</param>
		/// <param name="includePrototypes">
		/// Whether to include prototype objects too or just singletons
		/// (also applies to <see cref="Spring.Objects.Factory.IFactoryObject"/> instances).
		/// </param>
		/// <param name="includeFactoryObjects">
		/// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/> instances
		/// too or just normal objects.
		/// </param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the object could not be created.
		/// </exception>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If more than one instance of an object was found.
		/// </exception>
		/// <returns>
		/// A single object of the given type or subtypes.
		/// </returns>
		public static object ObjectOfTypeIncludingAncestors(
			IListableObjectFactory factory, Type type,
			bool includePrototypes, bool includeFactoryObjects)
		{
			IDictionary objectsOfType
				= ObjectsOfTypeIncludingAncestors(
					factory, type, includePrototypes, includeFactoryObjects);
			return GrabTheOnlyObject(objectsOfType, type);
		}

		private static object GrabTheOnlyObject(IDictionary objectsOfType, Type type)
		{
			if (objectsOfType.Count == 1)
			{
				return ObjectUtils.EnumerateFirstElement(objectsOfType.Values);
			}
			else
			{
				throw new NoSuchObjectDefinitionException(type, "Expected single object but found " + objectsOfType.Count);
			}
		}

		/// <summary>
		/// Return a single object of the given type or subtypes, not looking in
		/// ancestor factories.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Useful convenience method when we expect a single object and don't care
		/// about the object name.
		/// </p>
		/// </remarks>
		/// <param name="factory">The object factory.</param>
		/// <param name="type">The <see cref="System.Type"/> of object to match.</param>
		/// <param name="includePrototypes">
		/// Whether to include prototype objects too or just singletons
		/// (also applies to <see cref="Spring.Objects.Factory.IFactoryObject"/> instances).
		/// </param>
		/// <param name="includeFactoryObjects">
		/// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/> instances
		/// too or just normal objects.
		/// </param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the object could not be created.
		/// </exception>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If not exactly one instance of an object was found.
		/// </exception>
		/// <returns>
		/// A single object of the given type or subtypes.
		/// </returns>
		public static object ObjectOfType(IListableObjectFactory factory, Type type,
		                                  bool includePrototypes, bool includeFactoryObjects)
		{
			IDictionary objectsOfType
				= factory.GetObjectsOfType(type, includePrototypes, includeFactoryObjects);
			return GrabTheOnlyObject(objectsOfType, type);
		}

		/// <summary>
		/// Return a single object of the given type or subtypes, not looking in
		/// ancestor factories.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Useful convenience method when we expect a single object and don't care
		/// about the object name.
		/// This version of <c>ObjectOfType</c> automatically includes prototypes and
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> instances.
		/// </p>
		/// </remarks>
		/// <param name="factory">The object factory.</param>
		/// <param name="type">The <see cref="System.Type"/> of object to match.</param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the object could not be created.
		/// </exception>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If not exactly one instance of an object was found.
		/// </exception>
		/// <returns>
		/// A single object of the given type or subtypes.
		/// </returns>
		public static object ObjectOfType(IListableObjectFactory factory, Type type)
		{
			return ObjectOfType(factory, type, true, true);
		}

		/// <summary>
		/// Return the object name, stripping out the factory dereference prefix if necessary.
		/// </summary>
		/// <param name="name">The name of the object.</param>
		/// <returns>The object name sans any factory dereference prefix.</returns>
		public static string TransformedObjectName(string name)
		{
			AssertUtils.ArgumentNotNull(name, "name", "Object name must not be null.");
			string objectName = name;
			if (ObjectFactoryUtils.IsFactoryDereference(objectName))
			{
				objectName = objectName.Substring(ObjectFactoryUtils.FactoryObjectPrefix.Length);
			}
			return objectName;
		}

		/// <summary>
		/// Given an (object) name, builds a corresponding factory object name such that
		/// the return value can be used as a lookup name for a factory object.
		/// </summary>
		/// <param name="objectName">
		/// The name to be used to build the resulting factory object name.
		/// </param>
		/// <returns>
		/// The <paramref name="objectName"/> transformed into its factory object name
		/// equivalent.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.ObjectFactoryUtils.TransformedObjectName"/>
		/// <seealso cref="Spring.Objects.Factory.ObjectFactoryUtils.FactoryObjectPrefix"/>
		public static string BuildFactoryObjectName(string objectName)
		{
			return ObjectFactoryUtils.FactoryObjectPrefix + objectName;
		}

		/// <summary>
		/// Is the supplied <paramref name="name"/> a factory dereference?
		/// </summary>
		/// <remarks>
		/// <p>
		/// That is, does the supplied <paramref name="name"/> begin with
		/// the
		/// <see cref="Spring.Objects.Factory.ObjectFactoryUtils.FactoryObjectPrefix"/>?
		/// </p>
		/// </remarks>
		/// <param name="name">The name to check.</param>
		/// <returns>
		/// <see langword="true"/> if the supplied <paramref name="name"/> is a
		/// factory dereference; <see langword="false"/> if not, or the
		/// aupplied <paramref name="name"/> is <see langword="null"/> or
		/// consists solely of the
		/// <see cref="Spring.Objects.Factory.ObjectFactoryUtils.FactoryObjectPrefix"/>
		/// value.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.ObjectFactoryUtils.FactoryObjectPrefix"/>
		public static bool IsFactoryDereference(string name)
		{
			return name != null
				&& name.StartsWith(ObjectFactoryUtils.FactoryObjectPrefix)
				&& name.Length > ObjectFactoryUtils.FactoryObjectPrefix.Length;
		}
	}
}