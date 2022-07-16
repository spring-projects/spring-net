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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// A very simple, hashtable-based implementation of <see cref="IVariableSource"/>
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class DictionaryVariableSource : IVariableSource, IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> variables;

        /// <summary>
        /// Creates a new, empty variable source
        /// </summary>
        public DictionaryVariableSource()
            : this(null, true)
        {
        }

        /// <summary>
        /// Creates a new, empty and case-insensitive variable source
        /// </summary>
        public DictionaryVariableSource(bool ignoreCase)
            : this(null, ignoreCase)
        {
        }

        /// <summary>
        /// Create a new variable source from a list of paired string values.
        /// </summary>
        /// <remarks>
        /// <example>
        /// The example below shows, how the dictionary is filled with { 'key1', 'value1' }, { 'key2', 'value2' } pairs:
        /// <code>
        /// new DictionaryVariableSource( new string[] { &quot;key1&quot;, &quot;value1&quot;, &quot;key2&quot;, &quot;value2&quot; } )
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="args">the argument list containing pairs, or <c>null</c></param>
        public DictionaryVariableSource(params string[] args)
            : this(true)
        {
            if (args != null)
            {
                if (args.Length % 2 != 0)
                {
                    throw new ArgumentOutOfRangeException("Unbalanced Key-Value pairs of strings detected.  Verify that args contains pairs of key strings and value strings.");
                }
                
                    for (int i = 0; i < args.Length; i += 2)
                    {
                        Add(args[i], args[i + 1]);
                    }
                
            }
        }

        /// <summary>
        /// Initializes a new instance of the DictionaryVariableSource class.
        /// </summary>
        public DictionaryVariableSource(IDictionary dictionary)
            : this(dictionary, true)
        {
        }


        /// <summary>
        /// Creates a new variable source, reading values from another dictionary
        /// and converting them to strings if necessary
        /// </summary>
        public DictionaryVariableSource(IDictionary dictionary, bool ignoreCase)
        {
            if (ignoreCase)
            {
                variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                variables = new Dictionary<string, string>();
            }

            if (dictionary != null)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    string key = "" + entry.Key;
                    string value = entry.Value != null ? "" + entry.Value : null;

                    variables[key] = value;
                }
            }
        }

        /// <summary>
        /// Adds a key/value pair
        /// </summary>
        /// <returns>this dictionary. allows for fluent config</returns>
        public DictionaryVariableSource Add(string key, string value)
        {
            variables.Add(key, value);
            return this;
        }

        /// <summary>
        /// Before requesting a variable resolution, a client should
        /// ask, whether the source can resolve a particular variable name.
        /// </summary>
        /// <param name="name">the name of the variable to resolve</param>
        /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
        public bool CanResolveVariable(string name)
        {
            return variables.ContainsKey(name);
        }

        /// <summary>
        /// Performs a variable name lookup
        /// </summary>
        public string ResolveVariable(string name)
        {
            string value;
            if (!variables.TryGetValue(name, out value))
            {
                throw new ArgumentException(string.Format("variable '{0}' cannot be resolved", name));
            }
            return value;
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            return variables.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return variables.GetEnumerator();
        }
    }
}
