/*
 * Copyright 2002-2010 the original author or authors.
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

using System;
using System.Collections;
using System.Collections.Specialized;
using Spring.Util;

namespace Spring.Collections
{
    /// <summary>
    /// Synchronized <see cref="Hashtable"/> that, unlike hashtable created
    /// using <see cref="Hashtable.Synchronized"/> method, synchronizes 
    /// reads from the underlying hashtable in addition to writes.
    /// </summary>
    /// <remarks>
    /// <p>
    /// In addition to synchronizing reads, this implementation also fixes
    /// IEnumerator/ICollection issue described at 
    /// http://msdn.microsoft.com/en-us/netframework/aa570326.aspx
    /// (search for SynchronizedHashtable for issue description), by implementing 
    /// <see cref="IEnumerator"/> interface explicitly, and returns thread safe enumerator
    /// implementations as well.
    /// </p>
    /// <p>
    /// This class should be used whenever a truly synchronized <see cref="Hashtable"/>
    /// is needed.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class SynchronizedHashtable : IDictionary, ICollection, IEnumerable, ICloneable
    {
        private readonly bool _ignoreCase;
        private readonly Hashtable _table;

        /// <summary>
        /// Initializes a new instance of <see cref="SynchronizedHashtable"/>
        /// </summary>
        public SynchronizedHashtable()
            : this(new Hashtable(), false)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="SynchronizedHashtable"/>
        /// </summary>
        public SynchronizedHashtable(bool ignoreCase)
            : this(new Hashtable(), ignoreCase)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="SynchronizedHashtable"/>, copying initial entries from <param name="dictionary"/>
        /// handling keys depending on <param name="ignoreCase"/>.
        /// </summary>        
        public SynchronizedHashtable(IDictionary dictionary, bool ignoreCase)
        {
            AssertUtils.ArgumentNotNull(dictionary, "dictionary");
            this._table = (ignoreCase) ? CollectionsUtil.CreateCaseInsensitiveHashtable(dictionary) : new Hashtable(dictionary);
            this._ignoreCase = ignoreCase;
        }

        /// <summary>
        /// Creates a <see cref="SynchronizedHashtable"/> instance that 
        /// synchronizes access to the underlying <see cref="Hashtable"/>.
        /// </summary>
        /// <param name="other">the hashtable to be synchronized</param>
        protected SynchronizedHashtable(Hashtable other)
        {
            AssertUtils.ArgumentNotNull(other, "other");
            this._table = other;
        }

        /// <summary>
        /// Creates a <see cref="SynchronizedHashtable"/> wrapper that synchronizes
        /// access to the passed <see cref="Hashtable"/>.
        /// </summary>
        /// <param name="other">the hashtable to be synchronized</param>
        public static SynchronizedHashtable Wrap(Hashtable other)
        {
            return new SynchronizedHashtable(other);
        }

        ///<summary>
        ///Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object is read-only.
        ///</summary>
        ///<returns>
        ///true if the <see cref="T:System.Collections.IDictionary"></see> object is read-only; otherwise, false.
        ///</returns>
        public bool IsReadOnly
        {
            get
            {
                lock (SyncRoot)
                {
                    return _table.IsReadOnly;
                }
            }
        }

        ///<summary>
        ///Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size.
        ///</summary>
        ///<returns>
        ///true if the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size; otherwise, false.
        ///</returns>
        public bool IsFixedSize
        {
            get
            {
                lock (SyncRoot)
                {
                    return _table.IsFixedSize;
                }
            }
        }

        ///<summary>
        ///Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
        ///</summary>
        ///<returns>
        ///true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.
        ///</returns>
        public bool IsSynchronized
        {
            get { return true; }
        }

        ///<summary>
        ///Gets an <see cref="T:System.Collections.ICollection"></see> object containing the keys of the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</summary>
        ///<returns>
        ///An <see cref="T:System.Collections.ICollection"></see> object containing the keys of the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</returns>
        public ICollection Keys
        {
            get
            {
                lock (SyncRoot)
                {
                    return _table.Keys;
                }
            }
        }

        ///<summary>
        ///Gets an <see cref="T:System.Collections.ICollection"></see> object containing the values in the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</summary>
        ///<returns>
        ///An <see cref="T:System.Collections.ICollection"></see> object containing the values in the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</returns>
        public ICollection Values
        {
            get
            {
                lock (SyncRoot)
                {
                    return _table.Values;
                }
            }
        }

        ///<summary>
        ///Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
        ///</summary>
        ///<returns>
        ///An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
        ///</returns>
        public object SyncRoot
        {
            get { return _table.SyncRoot; }
        }

        ///<summary>
        ///Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
        ///</summary>
        ///<returns>
        ///The number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
        ///</returns>
        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _table.Count;
                }
            }
        }

        ///<summary>
        ///Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</summary>
        ///<param name="value">The <see cref="T:System.Object"></see> to use as the value of the element to add. </param>
        ///<param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add. </param>
        ///<exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.IDictionary"></see> object. </exception>
        ///<exception cref="T:System.ArgumentNullException">key is null. </exception>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception><filterpriority>2</filterpriority>
        public void Add(object key, object value)
        {
            lock (SyncRoot)
            {
                _table.Add(key, value);
            }
        }

        ///<summary>
        ///Removes all elements from the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</summary>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> object is read-only. </exception><filterpriority>2</filterpriority>
        public void Clear()
        {
            lock (SyncRoot)
            {
                _table.Clear();
            }
        }

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        public object Clone()
        {
            lock (SyncRoot)
            {
                return new SynchronizedHashtable(this, _ignoreCase);
            }
        }

        ///<summary>
        ///Determines whether the <see cref="T:System.Collections.IDictionary"></see> object contains an element with the specified key.
        ///</summary>
        ///<returns>
        ///true if the <see cref="T:System.Collections.IDictionary"></see> contains an element with the key; otherwise, false.
        ///</returns>
        ///<param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"></see> object.</param>
        ///<exception cref="T:System.ArgumentNullException">key is null. </exception><filterpriority>2</filterpriority>
        public bool Contains(object key)
        {
            lock (SyncRoot)
            {
                return _table.Contains(key);
            }
        }

        ///<summary>
        /// Returns, whether this <see cref="IDictionary"/> contains an entry with the specified <paramref name="key"/>.
        ///</summary>
        ///<param name="key">The key to look for</param>
        ///<returns><see lang="true"/>, if this <see cref="IDictionary"/> contains an entry with this <paramref name="key"/></returns>
        public bool ContainsKey(object key)
        {
            lock (SyncRoot)
            {
                return _table.ContainsKey(key);
            }
        }

        ///<summary>
        /// Returns, whether this <see cref="IDictionary"/> contains an entry with the specified <paramref name="value"/>.
        ///</summary>
        ///<param name="value">The value to look for</param>
        ///<returns><see lang="true"/>, if this <see cref="IDictionary"/> contains an entry with this <paramref name="value"/></returns>
        public bool ContainsValue(object value)
        {
            lock (SyncRoot)
            {
                return _table.ContainsValue(value);
            }
        }

        ///<summary>
        ///Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        ///</summary>
        ///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing. </param>
        ///<param name="index">The zero-based index in array at which copying begins. </param>
        ///<exception cref="T:System.ArgumentNullException">array is null. </exception>
        ///<exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
        ///<exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception><filterpriority>2</filterpriority>
        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                _table.CopyTo(array, index);
            }
        }

        ///<summary>
        ///Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</summary>
        ///<returns>
        ///An <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</returns>
        public IDictionaryEnumerator GetEnumerator()
        {
            lock (SyncRoot)
            {
                return new SynchronizedDictionaryEnumerator(SyncRoot, _table.GetEnumerator());
            }
        }

        ///<summary>
        ///Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary"></see> object.
        ///</summary>
        ///<param name="key">The key of the element to remove. </param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> object is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception>
        ///<exception cref="T:System.ArgumentNullException">key is null. </exception><filterpriority>2</filterpriority>
        public void Remove(object key)
        {
            lock (SyncRoot)
            {
                _table.Remove(key);
            }
        }

        ///<summary>
        ///Returns an enumerator that iterates through a collection.
        ///</summary>
        ///<returns>
        ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        ///</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (SyncRoot)
            {
                return new SynchronizedEnumerator(SyncRoot, ((IEnumerable)_table).GetEnumerator());
            }
        }

        ///<summary>
        ///Gets or sets the element with the specified key.
        ///</summary>
        ///<returns>
        ///The element with the specified key.
        ///</returns>
        ///<param name="key">The key of the element to get or set. </param>
        ///<exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.IDictionary"></see> object is read-only.-or- The property is set, key does not exist in the collection, and the <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception>
        ///<exception cref="T:System.ArgumentNullException">key is null. </exception><filterpriority>2</filterpriority>
        public object this[object key]
        {
            get
            {
                lock (SyncRoot)
                {
                    return _table[key];
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    _table[key] = value;
                }
            }
        }
    }
}