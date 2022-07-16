#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using Spring.Core.IO;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Implementation of <see cref="IVariableSource"/> that
    /// resolves variable name against Java-style property file.
    /// </summary>
    /// <seealso cref="Properties"/>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class PropertyFileVariableSource : IVariableSource
    {
        private IResource[] locations;
        protected Properties properties;
        private readonly object objectMonitor = new object();
        private bool ignoreMissingResources;

        /// <summary>
        /// Gets or sets the locations of the property files
        /// to read properties from.
        /// </summary>
        /// <value>
        /// The locations of the property files
        /// to read properties from.
        /// </value>
        public IResource[] Locations
        {
            get { return locations; }
            set { locations = value; }
        }

        /// <summary>
        /// Convinience property. Gets or sets a single location
        /// to read properties from.
        /// </summary>
        /// <value>
        /// A location to read properties from.
        /// </value>
        public IResource Location
        {
            set { locations = new IResource[] { value} ;}
        }

        /// <summary>
        /// Sets a value indicating whether to ignore resource locations that do not exist.  This will call
        /// the <see cref="IResource"/> Exists property.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if one should ignore missing resources; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreMissingResources
        {
            set { ignoreMissingResources = value; }
        }

        /// <summary>
        /// Before requesting a variable resolution, a client should
        /// ask, whether the source can resolve a particular variable name.
        /// </summary>
        /// <param name="name">the name of the variable to resolve</param>
        /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
        public bool CanResolveVariable(string name)
        {
            lock (objectMonitor)
            {
                if (properties == null)
                {
                    properties = new Properties();
                    InitProperties();
                }
                return properties.Contains(name);
            }
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
            lock (objectMonitor)
            {
                if (properties == null)
                {
                    properties = new Properties();
                    InitProperties();
                }
                return properties.GetProperty(name);
            }
        }

        /// <summary>
        /// Initializes properties based on the specified
        /// property file locations.
        /// </summary>
        protected virtual void InitProperties()
        {
            foreach (IResource location in locations)
            {
                bool exists = location.Exists;
                if (!exists && ignoreMissingResources)
                {
                    continue;
                }
                using (Stream input = location.InputStream)
                {
                    properties.Load(input);
                }
            }
        }
    }
}
