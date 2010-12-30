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
    public class NameValueCollection : IEnumerable<string>
    {
        private Dictionary<string, List<string>> innerCollection;

        public NameValueCollection()
        {
            innerCollection = new Dictionary<string, List<string>>();
        }

        public NameValueCollection(int capacity)
        {
            innerCollection = new Dictionary<string, List<string>>(capacity);
        }

        public NameValueCollection(IEqualityComparer<string> comparer)
        {
            innerCollection = new Dictionary<string, List<string>>(comparer);
        }

        public NameValueCollection(int capacity, IEqualityComparer<string> comparer)
        {
            innerCollection = new Dictionary<string, List<string>>(capacity, comparer);
        }

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

        public virtual string[] GetValues(string name)
        {
            List<string> list;
            if (this.innerCollection.TryGetValue(name, out list))
            {
                return list.ToArray();
            }
            return null;
        }

        public virtual void Set(string key, string value)
        {
            List<string> list = new List<string>();
            list.Add(value);
            this.innerCollection[key] = list;
        }

        public virtual bool Remove(string key)
        {
            return this.innerCollection.Remove(key);
        }

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

        public virtual int Count
        {
            get
            {
                return this.innerCollection.Count;
            }
        }

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