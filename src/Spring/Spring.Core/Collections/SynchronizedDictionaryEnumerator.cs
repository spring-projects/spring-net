#region License

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

#endregion

using System.Collections;

namespace Spring.Collections
{
    /// <summary>
    /// Synchronized <see cref="IDictionaryEnumerator"/> that should be returned by synchronized
    /// dictionary implementations in order to ensure that the enumeration is thread safe.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    internal class SynchronizedDictionaryEnumerator : SynchronizedEnumerator, IDictionaryEnumerator
    {
        public SynchronizedDictionaryEnumerator(object syncRoot, IDictionaryEnumerator enumerator)
            : base(syncRoot, enumerator)
        {
        }

        protected IDictionaryEnumerator Enumerator
        {
            get { return (IDictionaryEnumerator) enumerator; }
        }

        public object Key
        {
            get
            {
                lock (syncRoot)
                {
                    return Enumerator.Key;
                }
            }
        }

        public object Value
        {
            get
            {
                lock (syncRoot)
                {
                    return Enumerator.Value;
                }
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                lock (syncRoot)
                {
                    return Enumerator.Entry;
                }
            }
        }
    }
}