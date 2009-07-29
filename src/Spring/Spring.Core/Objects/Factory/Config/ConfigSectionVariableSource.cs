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
using System.Collections.Specialized;
using System.Configuration;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Implementation of <see cref="IVariableSource"/> that
    /// resolves variable name against name-value sections in
    /// the standard .NET configuration file.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class ConfigSectionVariableSource : IVariableSource
    {
        private string[] sectionNames;
        private NameValueCollection variables;

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigSectionVariableSource"/>
        /// </summary>
        public ConfigSectionVariableSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigSectionVariableSource"/> from the given <paramref name="sectionName"/>
        /// </summary>
        public ConfigSectionVariableSource(string sectionName)
        {
            this.SectionName = sectionName;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigSectionVariableSource"/> from the given <paramref name="sectionNames"/>
        /// </summary>
        public ConfigSectionVariableSource(string[] sectionNames)
        {
            this.SectionNames = sectionNames;
        }

        /// <summary>
        /// Gets or sets a list of section names variables should be loaded from.
        /// </summary>
        /// <remarks>
        /// All sections specified need to be handled by the <see cref="NameValueSectionHandler"/>
        /// in order to be processed successfully.
        /// </remarks>
        /// <value>
        /// A list of section names variables should be loaded from.
        /// </value>
        public string[] SectionNames
        {
            get { return sectionNames; }
            set { sectionNames = value; }
        }

        /// <summary>
        /// Convinience property. Gets or sets a single section
        /// to read properties from.
        /// </summary>
        /// <remarks>
        /// The section specified needs to be handled by the <see cref="NameValueSectionHandler"/>
        /// in order to be processed successfully.
        /// </remarks>
        /// <value>
        /// A section to read properties from.
        /// </value>
        public string SectionName
        {
            set { sectionNames = new string[] { value }; }
        }

        /// <summary>
        /// Before requesting a variable resolution, a client should
        /// ask, whether the source can resolve a particular variable name.
        /// </summary>
        /// <param name="name">the name of the variable to resolve</param>
        /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
        public bool CanResolveVariable(string name)
        {
            if (variables == null)
            {
                InitVariables();
            }
            return CollectionUtils.Contains(variables.AllKeys, name);
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
            if (variables == null)
            {
                InitVariables();
            }
            return variables.Get(name);
        }

        /// <summary>
        /// Initializes properties based on the specified 
        /// property file locations.
        /// </summary>
        private void InitVariables()
        {
            variables = new NameValueCollection();
            foreach (string sectionName in sectionNames)
            {
                object section = ConfigurationUtils.GetSection(sectionName);
                if (section is NameValueCollection)
                {
                    variables.Add((NameValueCollection) section);
                }
#if NET_2_0
                else if (section is System.Configuration.ClientSettingsSection)
                {
                    System.Configuration.ClientSettingsSection clientSettingsSection = (System.Configuration.ClientSettingsSection)section;
                    foreach (SettingElement setting in clientSettingsSection.Settings)
                    {
                        variables.Add(setting.Name, setting.Value.ValueXml.InnerText);
                    }
                }
#endif
                else
                {
                    throw new ArgumentException("Section [" + sectionName +
                                                "] is not handled by the NameValueSectionHandler.");
                }
            }
        }
    }
}