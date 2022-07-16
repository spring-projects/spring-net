/* Copyright © 2002-2011 by Aidant Systems, Inc., and by Jason Smith. */

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

using System.Collections;

namespace Spring.Collections
{
    /// <summary>
    /// <see cref="Spring.Collections.DictionarySet"/> is an
    /// <see langword="abstract"/> class that supports the creation of new
    /// <see cref="Spring.Collections.ISet"/> types where the underlying data
    /// store is an <see cref="System.Collections.IDictionary"/> instance.
    /// </summary>
    /// <remarks>
    /// <p>
    /// You can use any object that implements the
    /// <see cref="System.Collections.IDictionary"/> interface to hold set
    /// data. You can define your own, or you can use one of the objects
    /// provided in the framework. The type of
    /// <see cref="System.Collections.IDictionary"/> you
    /// choose will affect both the performance and the behavior of the
    /// <see cref="Spring.Collections.ISet"/> using it.
    /// </p>
    /// <p>
    /// This object overrides the <see cref="System.Object.Equals(object)"/> method,
    /// but not the <see cref="System.Object.GetHashCode"/> method, because
    /// the <see cref="Spring.Collections.DictionarySet"/> class is mutable.
    /// Therefore, it is not safe to use as a key value in a dictionary.
    /// </p>
    /// <p>
    /// To make a <see cref="Spring.Collections.ISet"/> typed based on your
    /// own <see cref="System.Collections.IDictionary"/>, simply derive a new
    /// class with a constructor that takes no parameters. Some
    /// <see cref="Spring.Collections.ISet"/> implmentations cannot be defined
    /// with a default constructor. If this is the case for your class, you
    /// will need to override <b>clone</b> as well.
    /// </p>
    /// <p>
    /// It is also standard practice that at least one of your constructors
    /// takes an <see cref="System.Collections.ICollection"/> or an
    /// <see cref="Spring.Collections.ISet"/> as an argument.
    /// </p>
    /// </remarks>
    /// <seealso cref="Spring.Collections.ISet"/>
    [Serializable]
    public abstract class DictionarySet : Set
    {
        private IDictionary _internalDictionary;

        private static readonly object PlaceholderObject = new object();
        private static readonly object NullPlaceHolderKey = new object();

        /// <summary>
        /// Provides the storage for elements in the
        /// <see cref="Spring.Collections.ISet"/>, stored as the key-set
        /// of the <see cref="System.Collections.IDictionary"/> object.  
        /// </summary>
        /// <remarks>
        /// <p>
        /// Set this object in the constructor if you create your own
        /// <see cref="Spring.Collections.ISet"/> class.
        /// </p>
        /// </remarks>
        protected IDictionary InternalDictionary
        {
            get => _internalDictionary;
            set => _internalDictionary = value;
        }

        /// <summary>
        /// The placeholder object used as the value for the
        /// <see cref="System.Collections.IDictionary"/> instance.
        /// </summary>
        /// <remarks>
        /// There is a single instance of this object globally, used for all
        /// <see cref="Spring.Collections.ISet"/>s.
        /// </remarks>
        protected static object Placeholder => PlaceholderObject;

        /// <summary>
        /// Adds the specified element to this set if it is not already present.
        /// </summary>
        /// <param name="element">The object to add to the set.</param>
        /// <returns>
        /// <see langword="true"/> is the object was added,
        /// <see langword="false"/> if the object was already present.
        /// </returns>
        public override bool Add(object element)
        {
            element = MaskNull(element);
            if (_internalDictionary[element] != null)
            {
                return false;
            }

            //The object we are adding is just a placeholder.  The thing we are
            //really concerned with is 'o', the key.
            _internalDictionary.Add(element, PlaceholderObject);
            return true;
        }

        /// <summary>
        /// Adds all the elements in the specified collection to the set if
        /// they are not already present.
        /// </summary>
        /// <param name="collection">A collection of objects to add to the set.</param>
        /// <returns>
        /// <see langword="true"/> is the set changed as a result of this
        /// operation.
        /// </returns>
        public override bool AddAll(ICollection collection)
        {
            bool changed = false;
            foreach (object o in collection)
            {
                changed |= Add(o);
            }
            return changed;
        }
        
        /// <summary>
        /// Removes all objects from this set.
        /// </summary>
        public override void Clear()
        {
            _internalDictionary.Clear();
        }

        /// <summary>
        /// Returns <see langword="true"/> if this set contains the specified
        /// element.
        /// </summary>
        /// <param name="element">The element to look for.</param>
        /// <returns>
        /// <see langword="true"/> if this set contains the specified element.
        /// </returns>
        public override bool Contains(object element)
        {
            element = MaskNull(element);
            return _internalDictionary[element] != null;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the set contains all the
        /// elements in the specified collection.
        /// </summary>
        /// <param name="collection">A collection of objects.</param>
        /// <returns>
        /// <see langword="true"/> if the set contains all the elements in the
        /// specified collection; also <see langword="false"/> if the
        /// supplied <paramref name="collection"/> is <see langword="null"/>.
        /// </returns>
        public override bool ContainsAll(ICollection collection)
        {
            if (collection == null)
            {
                return false;
            }
            foreach (object o in collection)
            {
                if (!Contains(MaskNull(o)))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns <see langword="true"/> if this set contains no elements.
        /// </summary>
        public override bool IsEmpty => _internalDictionary.Count == 0;

        /// <summary>
        /// Removes the specified element from the set.
        /// </summary>
        /// <param name="element">The element to be removed.</param>
        /// <returns>
        /// <see langword="true"/> if the set contained the specified element.
        /// </returns>
        public override bool Remove(object element)
        {
            element = MaskNull(element);
            bool contained = Contains(element);
            if (contained)
            {
                _internalDictionary.Remove(element);
            }
            return contained;
        }

        /// <summary>
        /// Remove all the specified elements from this set, if they exist in
        /// this set.
        /// </summary>
        /// <param name="collection">A collection of elements to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the set was modified as a result of this
        /// operation.
        /// </returns>
        public override bool RemoveAll(ICollection collection)
        {
            bool changed = false;
            foreach (object o in collection)
            {
                changed |= Remove(o);
            }
            return changed;
        }

        /// <summary>
        /// Retains only the elements in this set that are contained in the
        /// specified collection.
        /// </summary>
        /// <param name="collection">
        /// The collection that defines the set of elements to be retained.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this set changed as a result of this
        /// operation.
        /// </returns>
        public override bool RetainAll(ICollection collection)
        {
            //Put data from C into a set so we can use the Contains() method.
            Set cSet = new HybridSet(collection);

            //We are going to build a set of elements to remove.
            Set removeSet = new HybridSet();

            foreach (object o in this)
            {
                //If C does not contain O, then we need to remove O from our
                //set.  We can't do this while iterating through our set, so
                //we put it into RemoveSet for later.
                if (!cSet.Contains(o))
                {
                    removeSet.Add(o);
                }
            }
            return RemoveAll(removeSet);
        }

        /// <summary>
        /// Copies the elements in the <see cref="Spring.Collections.ISet"/> to
        /// an array.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The type of array needs to be compatible with the objects in the
        /// <see cref="Spring.Collections.ISet"/>, obviously.
        /// </p>
        /// </remarks>
        /// <param name="array">
        /// An array that will be the target of the copy operation.
        /// </param>
        /// <param name="index">
        /// The zero-based index where copying will start.
        /// </param>
        public override void CopyTo(Array array, int index)
        {
            int i = index;
            foreach (object o in this)
            {
                array.SetValue(UnmaskNull(o), i++);
            }
        }

        /// <summary>
        /// The number of elements currently contained in this collection.
        /// </summary>
        public override int Count => _internalDictionary.Count;

        /// <summary>
        /// Returns <see langword="true"/> if the
        /// <see cref="Spring.Collections.ISet"/> is synchronized across
        /// threads.
        /// </summary>
        /// <seealso cref="Spring.Collections.Set.IsSynchronized"/>
        public override bool IsSynchronized => false;

        /// <summary>
        /// An object that can be used to synchronize this collection to make
        /// it thread-safe.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize this collection to make
        /// it thread-safe.
        /// </value>
        /// <seealso cref="Spring.Collections.Set.SyncRoot"/>
        public override object SyncRoot => _internalDictionary.SyncRoot;

        /// <summary>
        /// Gets an enumerator for the elements in the
        /// <see cref="Spring.Collections.ISet"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"/> over the elements
        /// in the <see cref="Spring.Collections.ISet"/>.
        /// </returns>
        public override IEnumerator GetEnumerator()
        {
            return new DictionarySetEnumerator(_internalDictionary.Keys.GetEnumerator());
        }
        
        

        private static object MaskNull(object key)
        {
            return key ?? NullPlaceHolderKey;
        }

        private static object UnmaskNull(object key)
        {
            return key == NullPlaceHolderKey ? null : key;
        }

        private struct DictionarySetEnumerator : IEnumerator
        {
            public DictionarySetEnumerator(IEnumerator enumerator)
            {
                _enumerator = enumerator;
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public object Current => UnmaskNull(_enumerator.Current);

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            private readonly IEnumerator _enumerator;
        }
    }
}
