#if SILVERLIGHT
#region License

/*
 * Copyright 2002-2011 the original author or authors.
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Spring.Collections.Specialized
{
    /// <summary>
    /// Represents a collection of associated string keys and multiple string values.
    /// </summary>
    /// <remarks>
    /// Silverlight's implementation, based on a dictionary, of the .NET Framework NameValueCollection class.
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class NameValueCollection : IEnumerable<string>
    {
        private Dictionary<string, List<string>> innerCollection;

        /// <summary>
        /// Creates a new instance of the <see cref="NameValueCollection"/> class. 
        /// </summary>
        public NameValueCollection()
        {
            innerCollection = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NameValueCollection"/> class 
        /// with the specified initial capacity.
        /// </summary>
        public NameValueCollection(int capacity)
        {
            innerCollection = new Dictionary<string, List<string>>(capacity);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NameValueCollection"/> class 
        /// with the specified comparer.
        /// </summary>
        public NameValueCollection(IEqualityComparer<string> comparer)
        {
            innerCollection = new Dictionary<string, List<string>>(comparer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NameValueCollection"/> class 
        /// with the specified initial capacity and comparer.
        /// </summary>
        public NameValueCollection(int capacity, IEqualityComparer<string> comparer)
        {
            innerCollection = new Dictionary<string, List<string>>(capacity, comparer);
        }

        /// <summary>
        /// Adds the given single value to the current list of values for the given key.
        /// </summary>
        /// <param name="name">The key to use.</param>
        /// <param name="value">The value to add.</param>
        public virtual void Add(string name, string value)
        {
            List<string> list;
            if (!this.innerCollection.TryGetValue(name, out list))
            {
                list = new List<string>();
            }
            list.Add(value);
            this.innerCollection[name] = list;
        }

        /// <summary>
        /// Returns values for the given key as a comma-delimited string.
        /// </summary>
        /// <param name="name">The key that contains the values to get.</param>
        /// <returns>A comma-delimited string, if found; otherwise <see langword="null"/>.</returns>
        public virtual string Get(string name)
        {
            string str = null;
            List<string> list;
            if (this.innerCollection.TryGetValue(name, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0)
                    {
                        str = list[i];
                    }
                    else
                    {
                        str = str + list[i];
                    }
                    if (i != (list.Count - 1))
                    {
                        str = str + ",";
                    }
                }
            }
            return str;
        }

        /// <summary>
        /// Returns values for the given key as a string array.
        /// </summary>
        /// <param name="name">The key that contains the values to get.</param>
        /// <returns>A string array, if found; otherwise, <see langword="null"/>.</returns>
        public virtual string[] GetValues(string name)
        {
            List<string> list;
            if (this.innerCollection.TryGetValue(name, out list))
            {
                return list.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Sets the given single value under the given key.
        /// </summary>
        /// <param name="name">The key to use.</param>
        /// <param name="value">The value to set.</param>
        public virtual void Set(string name, string value)
        {
            List<string> list = new List<string>();
            list.Add(value);
            this.innerCollection[name] = list;
        }

        /// <summary>
        /// Removes the given key from the collection.
        /// </summary>
        /// <param name="name">The key to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the key have been found and removed from the collection; 
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool Remove(string name)
        {
            return this.innerCollection.Remove(name);
        }

        /// <summary>
        /// Gets all the keys in the collection.
        /// </summary>
        public virtual string[] AllKeys
        {
            get
            {
                int count = this.innerCollection.Count;
                string[] array = new string[count];
                this.innerCollection.Keys.CopyTo(array, 0);
                return array;
            }
        }

        /// <summary>
        /// Gets the number of keys contained in the collection.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return this.innerCollection.Count;
            }
        }

        /// <summary>
        /// Gets values as a comma-delimited string or sets a single value for the given key.
        /// </summary>
        /// <param name="name">The key to use.</param>
        /// <returns>A comma-delimited string, if found; otherwise <see langword="null"/>.</returns>
        public virtual string this[string name]
        {
            get
            {
                return this.Get(name);
            }
            set
            {
                this.Set(name, value);
            }
        }

        #region IEnumerable<string> Membres

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return this.innerCollection.Keys.GetEnumerator();
        }

        #endregion

        #region IEnumerable Membres

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.innerCollection.Keys.GetEnumerator();
        }

        #endregion
    }
}
#endif