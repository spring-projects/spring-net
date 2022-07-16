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

#region Imports

using System.Collections;
using System.Reflection;

#endregion

namespace Spring.Util.Generic
{
	/// <summary>
	/// Miscellaneous generic collection utility methods.
	/// </summary>
	/// <remarks>
	/// Mainly for internal use within the framework.
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public sealed class CollectionUtils
	{
		#region Methods

		/// <summary>
		/// Determine whether a given collection only contains
		/// a single unique object
		/// </summary>
		/// <param name="coll"></param>
		/// <returns></returns>
		public static bool HasUniqueObject<T>(ICollection<T> coll)
		{
			if (coll.Count == 0)
			{
				return false;
			}
			object candidate = null;
			foreach (object elem in coll)
			{
				if (candidate == null)
				{
					candidate = elem;
				}
				else if (candidate != elem)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Determines whether the <paramref name="collection"/> contains the specified <paramref name="element"/>.
		/// </summary>
		/// <param name="collection">The collection to check.</param>
		/// <param name="element">The object to locate in the collection.</param>
		/// <returns><see lang="true"/> if the element is in the collection, <see lang="false"/> otherwise.</returns>
		public static bool Contains<T>(ICollection<T> collection, Object element)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("Collection cannot be null.");
			}
			MethodInfo method;
			method = collection.GetType().GetMethod("contains", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			if (null == method)
			{
				throw new InvalidOperationException("Collection type " + collection.GetType() + " does not implement a Contains() method.");
			}
			return (bool) method.Invoke(collection, new Object[] {element});
		}

		/// <summary>
		/// Determines whether the collection contains all the elements in the specified collection.
		/// </summary>
		/// <param name="targetCollection">The collection to check.</param>
		/// <param name="sourceCollection">Collection whose elements would be checked for containment.</param>
		/// <returns>true if the target collection contains all the elements of the specified collection.</returns>
		public static bool ContainsAll<T>(ICollection<T> targetCollection, ICollection<T> sourceCollection)
		{
			if (targetCollection == null || sourceCollection == null)
			{
				throw new ArgumentNullException("Collection cannot be null.");
			}
			if ( sourceCollection.Count == 0 && targetCollection.Count > 1 )
				return true;

			IEnumerator sourceCollectionEnumerator = sourceCollection.GetEnumerator();

			bool contains = false;

			MethodInfo method;
			method = targetCollection.GetType().GetMethod("containsAll", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

			if (method != null)
				contains = (bool) method.Invoke(targetCollection, new Object[] {sourceCollection});
			else
			{
				method = targetCollection.GetType().GetMethod("Contains", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
				if (method == null)
				{
					throw new InvalidOperationException("Target collection does not implment a Contains() or ContainsAll() method.");
				}
				while (sourceCollectionEnumerator.MoveNext())
				{
					if ((contains = (bool) method.Invoke(targetCollection, new Object[] {sourceCollectionEnumerator.Current})) == false)
						break;
				}
			}
			return contains;
		}
        /// <summary>
		/// Removes all the elements from the target collection that are contained in the source collection.
		/// </summary>
		/// <param name="targetCollection">Collection where the elements will be removed.</param>
		/// <param name="sourceCollection">Elements to remove from the target collection.</param>
        public static void RemoveAll<T>(ICollection<T> targetCollection, ICollection<T> sourceCollection)
        {
            if (targetCollection == null)
            {
                throw new ArgumentNullException("targetCollection", "Collection cannot be null.");
            }

            if (sourceCollection == null)
            {
                throw new ArgumentNullException("sourceCollection", "Collection cannot be null.");
            }
            foreach (T element in sourceCollection)
            {
                if (targetCollection.Contains(element))
                {
                    targetCollection.Remove(element);
                }
            }
        }
	    #endregion
	}
}
