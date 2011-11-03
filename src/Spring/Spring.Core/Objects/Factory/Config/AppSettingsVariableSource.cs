#if NET_2_0

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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Implementation of <see cref="IVariableSource"/> that
    /// resolves variable name from an <code>appSettings</code> section in
    /// the standard .NET configuration file.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <author>Marijn van der Zee</author>
    [Serializable]
    public class AppSettingsVariableSource : IVariableSource
    {
        private Hashtable variables;

        #region Implementation of IVariableSource

        /// <summary>
        /// Before requesting a variable resolution, a client should
        /// ask, whether the source can resolve a particular variable name.
        /// </summary>
        /// <param name="name">the name of the variable to resolve</param>
        /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
        public bool CanResolveVariable(string name)
        {
            InitIfRequired();
            return variables.Contains(name);
        }

        /// <summary>
        /// Resolves variable value for the specified variable name.
        /// </summary>
        /// <param name="name">
        /// The name of the variable to resolve.
        /// </param>
        /// <returns>
        /// The variable value if able to resolve, <c>null</c> otherwise.
        /// </returns>
        public string ResolveVariable(string name)
        {
            InitIfRequired();
            return (string) variables[name];
        }

        #endregion

        private void InitIfRequired()
        {
            if(variables == null)
                InitVariables();
        }

        /// <summary>
        /// Initializes properties based on the specified 
        /// property file locations.
        /// </summary>
        private void InitVariables()
        {
            variables = CollectionsUtil.CreateCaseInsensitiveHashtable();
            NameValueCollection settings = ConfigurationManager.AppSettings;
            foreach (string key in settings.Keys)
                variables.Add(key, settings[key]);
        }
    }
}
#endif 
