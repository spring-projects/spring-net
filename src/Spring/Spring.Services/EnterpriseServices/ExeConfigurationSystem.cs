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

namespace Spring.EnterpriseServices
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Xml;
    using Spring.Core.TypeResolution;
    using Spring.Util;

    using System.Configuration.Internal;

    /// <summary>
    /// SUBJECT TO CHANGE -FOR INTERNAL USE ONLY!<br/>
    /// Holds configuration information from a given configuration file, obtained by <see cref="ConfigurationManager.OpenExeConfiguration(string)"/>.
    /// You may use <see cref="ConfigurationUtils.SetConfigurationSystem"/> to replace the active configuration system.
    /// </summary>
    /// <seealso cref="ConfigurationManager.OpenExeConfiguration(string)"/>
    /// <seealso cref="ConfigurationUtils.SetConfigurationSystem"/>
    public class ExeConfigurationSystem : IChainableConfigSystem
    {
        private string _configPath;
        private Configuration _configuration;
        private IInternalConfigSystem _next;

        /// <summary>
        /// initializes this instance with a path to be passed into <see cref="ConfigurationManager.OpenExeConfiguration(string)"/>
        /// </summary>
        /// <param name="configPath"></param>
        public ExeConfigurationSystem(string configPath)
        {
            _configPath = configPath;
        }

        /// <summary>
        /// Purges cached configuration
        /// </summary>
        public void RefreshConfig(string sectionName)
        {
            if (_next != null)
            {
                _next.RefreshConfig(sectionName);
            }
            _configuration = null;
        }

        ///<summary>
        /// Only true if the underlying config system supports this.
        ///</summary>
        public bool SupportsUserConfig
        {
            get
            {
                EnsureInit();
                if (_next != null)
                {
                    return _next.SupportsUserConfig;
                }
                return false;
            }
        }

        /// <summary>
        /// Set the nested configuration system to delegate calls in case we can't resolve a config section ourselves
        /// </summary>
        public void SetInnerConfigurationSystem(IInternalConfigSystem innerConfigSystem)
        {
            _next = innerConfigSystem;
        }

        private void EnsureInit()
        {
            if (_configuration == null)
            {
                lock (this)
                {
                    if (_configuration == null)
                    {
                        _configuration = ConfigurationManager.OpenExeConfiguration(_configPath);
                    }
                }
            }
        }

        private delegate object ResolveSectionRuntimeObject(ConfigurationSection section);

        private static ResolveSectionRuntimeObject resolveSectionRuntimeObject =
            (ResolveSectionRuntimeObject)Delegate.CreateDelegate(typeof(ResolveSectionRuntimeObject),
                                                                  typeof (ConfigurationSection).GetMethod("GetRuntimeObject",
                                                                                                          BindingFlags.Instance |
                                                                                                          BindingFlags.NonPublic));

        /// <summary>
        /// Get the specified section
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public object GetSection(string sectionName)
        {
            EnsureInit();
            ConfigurationSection thisSection = _configuration.GetSection(sectionName);

            object parent = null;
            if (_next != null)
            {
                parent = _next.GetSection(sectionName);
            }
            if (thisSection == null)
            {
                return parent;
            }

            object result = resolveSectionRuntimeObject(thisSection);
            if (result is DefaultSection)
            {
                string rawXml = thisSection.SectionInformation.GetRawXml();
                if (string.IsNullOrEmpty(rawXml))
                {
                    return null;
                }

                Type t = TypeResolutionUtils.ResolveType(thisSection.SectionInformation.Type);
                if (typeof(IConfigurationSectionHandler).IsAssignableFrom(t))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(thisSection.SectionInformation.GetRawXml());
                    IConfigurationSectionHandler handler = (IConfigurationSectionHandler) Activator.CreateInstance(t);
                    return handler.Create(parent, null, xmlDoc.DocumentElement );
                }
                throw new ConfigurationErrorsException(string.Format("missing <section> declaration for section '{0}'", sectionName));
            }
            return result;
        }
    }
}
