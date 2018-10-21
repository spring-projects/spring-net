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
using System.Collections;
using System.Reflection;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Miscellaneous collection utility methods.
    /// </summary>
    /// <remarks>
    /// Mainly for internal use within the framework.
    /// </remarks>
    /// <author>Mark Pollack (.NET)</author>
    public sealed class CollectionUtils
    {
        /// <summary>
        /// Checks if the given array or collection has elements and none of the elements is null.
        /// </summary>
        /// <param name="collection">the collection to be checked.</param>
        /// <returns>true if the collection has a length and contains only non-null elements.</returns>
        public static bool HasElements(ICollection collection)
        {
            return ArrayUtils.HasElements(collection);
        }

        /// <summary>
        /// Checks if the given array or collection is null or has no elements.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool HasLength(ICollection collection)
        {
            return ArrayUtils.HasLength(collection);
        }

        /// <summary>
        /// Determine whether a given collection only contains
        /// a single unique object
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        public static bool HasUniqueObject(ICollection coll)
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
        public static bool Contains(IEnumerable collection, Object element)
        {
            if (collection == null)
            {
                return false;
            }

            if (collection is IList)
            {
                return ((IList) collection).Contains(element);
            }

            if (collection is IDictionary)
            {
                return ((IDictionary) collection).Contains(element);
            }

            MethodInfo method = collection.GetType().GetMethod("contains", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            if (null != method)
            {
                return (bool)method.Invoke(collection, new Object[] { element });
            }
            foreach (object item in collection)
            {
                if (object.Equals(item, element))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds the specified <paramref name="element"/> to the specified <paramref name="collection"/> .
        /// </summary>
        /// <param name="collection">The collection to add the element to.</param>
        /// <param name="element">The object to add to the collection.</param>
        public static void Add(ICollection collection, object element)
        {
            Add((IEnumerable)collection, element);
        }

        /// <summary>
        /// Adds the specified <paramref name="element"/> to the specified <paramref name="enumerable"/> .
        /// </summary>
        /// <param name="enumerable">The enumerable to add the element to.</param>
        /// <param name="element">The object to add to the collection.</param>
        public static void Add(IEnumerable enumerable, object element)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable", "Collection cannot be null.");
            }
            if (enumerable is IList)
            {
                ((IList)enumerable).Add(element);
                return;
            }
            MethodInfo method;
            method = enumerable.GetType().GetMethod("add", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            if (null == method)
            {
                throw new InvalidOperationException("Enumerable type " + enumerable.GetType() + " does not implement a Add() method.");
            }
            method.Invoke(enumerable, new Object[] { element });
        }

        /// <summary>
        /// Determines whether the collection contains all the elements in the specified collection.
        /// </summary>
        /// <param name="targetCollection">The collection to check.</param>
        /// <param name="sourceCollection">Collection whose elements would be checked for containment.</param>
        /// <returns>true if the target collection contains all the elements of the specified collection.</returns>
        public static bool ContainsAll(ICollection targetCollection, ICollection sourceCollection)
        {
            if (targetCollection == null)
            {
                throw new ArgumentNullException("targetCollection", "Collection cannot be null.");
            }
            if (sourceCollection == null)
            {
                throw new ArgumentNullException("sourceCollection", "Collection cannot be null.");
            }
            if (sourceCollection.Count == 0 && targetCollection.Count > 1)
                return true;

            IEnumerator sourceCollectionEnumerator = sourceCollection.GetEnumerator();

            bool contains = false;

            MethodInfo method;
            method = targetCollection.GetType().GetMethod("containsAll", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            if (method != null)
                contains = (bool)method.Invoke(targetCollection, new Object[] { sourceCollection });
            else
            {
                method = targetCollection.GetType().GetMethod("Contains", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                if (method == null)
                {
                    throw new InvalidOperationException("Target collection does not implment a Contains() or ContainsAll() method.");
                }
                while (sourceCollectionEnumerator.MoveNext() == true)
                {
                    if ((contains = (bool)method.Invoke(targetCollection, new Object[] { sourceCollectionEnumerator.Current })) == false)
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
        public static void RemoveAll(ICollection targetCollection, ICollection sourceCollection)
        {
            if (targetCollection == null || sourceCollection == null)
            {
                throw new ArgumentNullException("Collection cannot be null.");
            }
            ArrayList al = ToArrayList(sourceCollection);

            MethodInfo method;
            method = targetCollection.GetType().GetMethod("removeAll", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            if (method != null)
                method.Invoke(targetCollection, new Object[] { al });
            else
            {
                method = targetCollection.GetType().GetMethod("Remove", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(object) }, null);
                MethodInfo methodContains = targetCollection.GetType().GetMethod("Contains", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                if (method == null)
                {
                    throw new InvalidOperationException("Target Collection must implement either a RemoveAll() or Remove() method.");
                }
                if (methodContains == null)
                {
                    throw new InvalidOperationException("TargetCollection must implement a Contains() method.");
                }
                IEnumerator e = al.GetEnumerator();
                while (e.MoveNext() == true)
                {
                    while ((bool)methodContains.Invoke(targetCollection, new Object[] { e.Current }) == true)
                        method.Invoke(targetCollection, new Object[] { e.Current });
                }
            }
        }

        /// <summary>
        /// Converts an <see cref="System.Collections.ICollection"/>instance to an <see cref="System.Collections.ArrayList"/> instance.
        /// </summary>
        /// <param name="inputCollection">The <see cref="System.Collections.ICollection"/> instance to be converted.</param>
        /// <returns>An <see cref="System.Collections.ArrayList"/> instance in which its elements are the elements of the <see cref="System.Collections.ICollection"/> instance.</returns>
        /// <exception cref="System.ArgumentNullException">if the <paramref name="inputCollection"/> is null.</exception>
        public static ArrayList ToArrayList(ICollection inputCollection)
        {
            if (inputCollection == null)
            {
                throw new ArgumentNullException("Collection cannot be null.");
            }
            return new ArrayList(inputCollection);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection"/> to a 
        /// new array of the specified element type.
        /// </summary>
        /// <param name="inputCollection">The <see cref="System.Collections.ICollection"/> instance to be converted.</param>
        /// <param name="elementType">The element <see cref="Type"/> of the destination array to create and copy elements to</param>
        /// <returns>An array of the specified element type containing copies of the elements of the <see cref="ICollection"/>.</returns>
        public static Array ToArray(ICollection inputCollection, Type elementType)
        {
            Array array = Array.CreateInstance(elementType, inputCollection.Count);
            inputCollection.CopyTo(array, 0);
            return array;
        }

        /// <summary>
        /// Returns the first element contained in both, <paramref name="source"/> and <paramref name="candidates"/>.
        /// </summary>
        /// <remarks>The implementation assumes that <paramref name="candidates"/> &lt;&lt;&lt; <paramref name="source"/></remarks>
        /// <param name="source">the source enumerable. may be <c>null</c></param>
        /// <param name="candidates">the list of candidates to match against <paramref name="source"/> elements. may be <c>null</c></param>
        /// <returns>the first element found in both enumerables or <c>null</c></returns>
        public static object FindFirstMatch(IEnumerable source, IEnumerable candidates)
        {
            if (IsEmpty(source) || IsEmpty(candidates))
            {
                return null;
            }

            IList candidateList = candidates as IList;
            if (candidateList == null)
            {
                if (candidates is ICollection)
                {
                    candidateList = new ArrayList((ICollection)candidates);
                }
                else
                {
                    candidateList = new ArrayList();
                    foreach (object el in candidates)
                    {
                        candidateList.Add(el);
                    }
                }
            }

            foreach (object sourceElement in source)
            {
                if (candidateList.Contains(sourceElement))
                {
                    return sourceElement;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a value of the given type in the given collection.
        /// </summary>
        /// <param name="collection">The collection to search.</param>
        /// <param name="type">The type to look for.</param>
        /// <returns>a value of the given type found, or null if none.</returns>
        /// <exception cref="ArgumentException">If more than one value of the given type is found</exception>
        public static object FindValueOfType(ICollection collection, Type type)
        {
            if (IsEmpty(collection))
            {
                return null;
            }
            Type typeToUse = (type != null ? type : typeof(object));
            object val = null;
            foreach (object obj in collection)
            {
                if (typeToUse.IsAssignableFrom(obj.GetType()))
                {
                    if (val != null)
                    {
                        throw new ArgumentException("More than one value of type[" + typeToUse.Name + "] found.");
                    }
                    val = obj;
                }
            }
            return val;
        }

        /// <summary>
        /// Finds a value of the given type in the given collection.
        /// </summary>
        /// <param name="collection">The collection to search.</param>
        /// <param name="type">The type to look for.</param>
        /// <returns>a collection of matching values of the given type found, empty if none found, or null if the input collection was null.</returns>
        public static ICollection FindValuesOfType(IEnumerable collection, Type type)
        {
            if (IsEmpty(collection))
            {
                return null;
            }
            Type typeToUse = (type != null ? type : typeof(object));
            ArrayList results = new ArrayList();
            foreach (object obj in collection)
            {
                if (typeToUse.IsAssignableFrom(obj.GetType()))
                {
                    results.Add(obj);
                }
            }
            return results;
        }

        /// <summary>
        /// Find a value of one of the given types in the given Collection, 
        /// searching the Collection for a value of the first type, then
        /// searching for a value of the second type, etc.
        /// </summary>
        /// <param name="collection">The collection to search.</param>
        /// <param name="types">The types to look for, in prioritized order.</param>
        /// <returns>a value of the given types found, or <code>null</code> if none</returns>
        /// <exception cref="ArgumentException">If more than one value of the given type is found</exception>        
        public static object FindValueOfType(ICollection collection, Type[] types)
        {
            if (IsEmpty(collection) || ObjectUtils.IsEmpty(types))
            {
                return null;
            }
            foreach (Type type in types)
            {
                object val = FindValueOfType(collection, type);
                if (val != null)
                {
                    return val;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether the specified collection is null or empty.
        /// </summary>
        /// <param name="enumerable">The collection to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified collection is empty or null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(IEnumerable enumerable)
        {
            if (enumerable == null)
                return true;

            if (enumerable is ICollection)
            {
                return (0 == ((ICollection)enumerable).Count);
            }

            IEnumerator it = enumerable.GetEnumerator();
            if (!it.MoveNext())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified collection is empty or null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(ICollection collection)
        {
            return (collection == null || collection.Count == 0);
        }

        /// <summary>
        /// Determines whether the specified dictionary is null empty.
        /// </summary>
        /// <param name="dictionary">The dictionary to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified dictionary is empty or null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(IDictionary dictionary)
        {
            return (dictionary == null || dictionary.Count == 0);
        }

        /// <summary>
        /// A callback method used for comparing to items.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="left">the first object to compare</param>
        /// <param name="right">the second object to compare</param>
        /// <returns>Value Condition Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y.</returns>
        /// <seealso cref="IComparer.Compare"/>
        /// <seealso cref="CollectionUtils.StableSort(IEnumerable,CompareCallback)"/>
        public delegate int CompareCallback(object left, object right);

        /// <summary>
        /// A simple stable sorting routine - far from being efficient, only for small collections.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static ICollection StableSort(IEnumerable input, IComparer comparer)
        {
            return StableSort(input, new CompareCallback(comparer.Compare));
        }

        /// <summary>
        /// A simple stable sorting routine - far from being efficient, only for small collections.
        /// </summary>
        /// <remarks>
        /// Sorting is not(!) done in-place. Instead a sorted copy of the original input is returned.
        /// </remarks>
        /// <param name="input">input collection of items to sort</param>
        /// <param name="comparer">the <see cref="CompareCallback"/> for comparing 2 items in <paramref name="input"/>.</param>
        /// <returns>a new collection of stable sorted items.</returns>
        public static ICollection StableSort(IEnumerable input, CompareCallback comparer)
        {
            ArrayList ehancedInput = new ArrayList();
            IEnumerator it = input.GetEnumerator();
            int index = 0;
            while (it.MoveNext())
            {
                ehancedInput.Add(new Entry(index, it.Current));
                index++;
            }

            ehancedInput.Sort(Entry.GetComparer(comparer));

            for (int i = 0; i < ehancedInput.Count; i++)
            {
                ehancedInput[i] = ((Entry)ehancedInput[i]).Value;
            }

            return ehancedInput;
        }

        /// <summary>
        /// A simple stable sorting routine - far from being efficient, only for small collections.
        /// </summary>
        /// <remarks>
        /// Sorting is not(!) done in-place. Instead a sorted copy of the original input is returned.
        /// </remarks>
        /// <param name="input">input collection of items to sort</param>
        /// <param name="comparer">the <see cref="IComparer"/> for comparing 2 items in <paramref name="input"/>.</param>
        /// <returns>a new collection of stable sorted items.</returns>
        public static void StableSortInPlace(IList input, IComparer comparer)
        {
            StableSortInPlace(input, new CompareCallback(comparer.Compare));
        }

        /// <summary>
        /// A simple stable sorting routine - far from being efficient, only for small collections.
        /// </summary>
        /// <remarks>
        /// Sorting is not(!) done in-place. Instead a sorted copy of the original input is returned.
        /// </remarks>
        /// <param name="input">input collection of items to sort</param>
        /// <param name="comparer">the <see cref="CompareCallback"/> for comparing 2 items in <paramref name="input"/>.</param>
        /// <returns>a new collection of stable sorted items.</returns>
        public static void StableSortInPlace(IList input, CompareCallback comparer)
        {
            ArrayList ehancedInput = new ArrayList();
            IEnumerator it = input.GetEnumerator();
            int index = 0;
            while (it.MoveNext())
            {
                ehancedInput.Add(new Entry(index, it.Current));
                index++;
            }

            ehancedInput.Sort(Entry.GetComparer(comparer));

            for (int i = 0; i < ehancedInput.Count; i++)
            {
                input[i] = ((Entry)ehancedInput[i]).Value;
            }
        }

        #region StableSort Utility Classes

        private class Entry
        {
            private class EntryComparer : IComparer
            {
                private readonly CompareCallback innerComparer;

                public EntryComparer(CompareCallback innerComparer)
                {
                    this.innerComparer = innerComparer;
                }

                public int Compare(object x, object y)
                {
                    Entry ex = (Entry)x;
                    Entry ey = (Entry)y;
                    int result = innerComparer(ex.Value, ey.Value);
                    if (result == 0)
                    {
                        result = ex.Index.CompareTo(ey.Index);
                    }
                    return result;
                }
            }

            public static IComparer GetComparer(CompareCallback innerComparer)
            {
                return new EntryComparer(innerComparer);
            }

            public readonly int Index;
            public readonly object Value;

            public Entry(int index, object value)
            {
                Index = index;
                Value = value;
            }
        }

        #endregion
    }
}