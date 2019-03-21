#region License

/*
 * Copyright � 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
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
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Jndi
{
    /// <summary>
    /// Simple extension of the ILookupContext that always returns a given object.  Useful for testing.
    /// </summary>
    /// <author>Mark Pollack</author>
    public class ExpectedLookupContext : ILookupContext
    {
        private Hashtable jndiObjects = new Hashtable();

        private object monitor = new object();

        public ExpectedLookupContext(string name, object objValue)
        {
            lock (monitor)
            {
                jndiObjects.Add(name, objValue);
            }
        }
        #region Implementation of ILookupContext

        public void AddSettings(Hashtable prop)
        {
            lock (monitor)
            {
                Hashtable merged = new Hashtable();
                foreach (DictionaryEntry dictionaryEntry in prop)
                {
                    merged[dictionaryEntry.Key] = dictionaryEntry.Value;
                }
                foreach (DictionaryEntry entry in jndiObjects)
                {
                    merged[entry.Key] = entry.Value;
                }
                jndiObjects = merged;
            }
        }

        public object AddSettings(string propName, object propValue)
        {
            if ((propName == null) || (propValue == null))
            {
                throw new ArgumentException("propName or propValue cannot be null");
            }

            object originalValue = null;
            lock (monitor)
            {
                if (jndiObjects.ContainsKey(propName))
                {
                    originalValue = jndiObjects[propName];
                    jndiObjects.Remove(propValue);
                    jndiObjects.Add(propName, propValue);
                }
            }
            return originalValue;
        }

        public object RemoveSettings(string propName)
        {
            object originalValue = null;
            if (propName == null)
            {
                throw new ArgumentException("propName cannot be null");
            }
            lock (monitor)
            {
                if (this.jndiObjects.ContainsKey(propName))
                {
                    originalValue = jndiObjects[propName];
                    jndiObjects.Remove(propName);
                }
            }
            return originalValue;

        }

        public object Lookup(string name)
        {
            object foundObject = this.jndiObjects[name];
		    if (foundObject == null) {
			    throw new NamingException("Unexpected JNDI name '" + name + "': expecting one of" + this.jndiObjects.Keys);
		    }
            return foundObject;
        }

        public Hashtable Settings
        {
            get { return new Hashtable(); }
        }

        #endregion
    }
}