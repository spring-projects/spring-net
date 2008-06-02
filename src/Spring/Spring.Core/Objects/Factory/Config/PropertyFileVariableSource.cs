#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.IO;
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
        private Properties properties;

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
            if (properties == null)
            {
                InitProperties();
            }
            return properties.GetProperty(name);
        }

        /// <summary>
        /// Initializes properties based on the specified 
        /// property file locations.
        /// </summary>
        private void InitProperties()
        {
            properties = new Properties();
            foreach (IResource location in locations)
            {
                using (Stream input = location.InputStream)
                {
                    properties.Load(input);
                }
            }
        }
    }
}