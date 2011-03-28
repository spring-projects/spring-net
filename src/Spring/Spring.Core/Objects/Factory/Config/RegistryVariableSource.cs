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

using System.Collections.Specialized;
using Microsoft.Win32;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Implementation of <see cref="IVariableSource"/> that
    /// resolves variable name against registry key.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class RegistryVariableSource : IVariableSource
    {
        private static readonly object NULL = new object();
        private RegistryKey key;

        /// <summary>
        /// Gets or sets the registry key to obtain variable values from.
        /// </summary>
        /// <value>
        /// The registry key to obtain variable values from.
        /// </value>
        public RegistryKey Key
        {
            get { return key; }
            set { key = value; }
        }

        /// <summary>
        /// Before requesting a variable resolution, a client should
        /// ask, whether the source can resolve a particular variable name.
        /// </summary>
        /// <param name="name">the name of the variable to resolve</param>
        /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
        public bool CanResolveVariable(string name)
        {
            return (key != null && key.GetValue(name, NULL) != NULL);
        }

        /// <summary>
        /// Resolves variable value for the specified variable name.
        /// </summary>
        /// <param name="name">
        /// The name of the variable to resolve.
        /// </param>
        /// <remarks>
        /// This implementation resolves REG_SZ as well as REG_MULTI_SZ values. In case of a REG_MULTI_SZ value,
        /// strings are concatenated to a comma-separated list following <see cref="NameValueCollection.Get(string)"/>
        /// </remarks>
        /// <returns>
        /// The variable value if able to resolve, <c>null</c> otherwise.
        /// </returns>
        public virtual string ResolveVariable(string name)
        {
			object res = Key.GetValue(name);
			if (res is string)
			{
				return (string) res;
			}
			else if (res is string[])
			{
				NameValueCollection tmp = new NameValueCollection();
				foreach(string val in (string[])res)
				{
					tmp.Add(name, val);
				}
				return tmp.Get(name);
			}
            else if (res is int)
            {
                return res.ToString();
            }
            return null;
        }
    }
}