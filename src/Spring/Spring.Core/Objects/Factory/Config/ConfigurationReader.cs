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
using System.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

using Common.Logging;

using Spring.Core.IO;
using Spring.Core.TypeResolution;
using Spring.Util;
using ConfigurationException=Common.Logging.ConfigurationException;
using ConfigXmlDocument = Spring.Util.ConfigXmlDocument;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Various utility methods for .NET style .config files.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Currently supports reading custom configuration sections and returning them as
    /// <see cref="System.Collections.Specialized.NameValueCollection"/> objects.
    /// </p>
    /// </remarks>
    /// <author>Simon White</author>
    /// <author>Mark Pollack</author>
    public sealed class ConfigurationReader
    {
        private const string ConfigSectionTypeAttribute = "type";
        private const string ConfigurationElement = "configuration";
        private const string ConfigSectionsElement = "configSections";
        private const string ConfigSectionGroupElement = "sectionGroup";
        private const string ConfigSectionElement = "section";
        private const string ConfigSectionNameAttribute = "name";

        private static readonly ILog _log = LogManager.GetLogger(typeof(ConfigurationReader));

        /// <summary>
        /// Initializes the type members
        /// </summary>
        static ConfigurationReader()
        {}

        /// <summary>
        /// Reads the specified configuration section into a
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </summary>
        /// <param name="resource">The resource to read.</param>
        /// <param name="configSection">The section name.</param>
        /// <returns>
        /// A newly populated
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </returns>
        /// <exception cref="System.IO.IOException">
        /// If any errors are encountered while attempting to open a stream
        /// from the supplied <paramref name="resource"/>.
        /// </exception>
        /// <exception cref="System.Xml.XmlException">
        /// If any errors are encountered while loading or reading (this only applies to
        /// v1.1 and greater of the .NET Framework) the actual XML.
        /// </exception>
        /// <exception cref="System.Exception">
        /// If any errors are encountered while loading or reading (this only applies to
        /// v1.0 of the .NET Framework).
        /// </exception>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If the configuration section was otherwise invalid.
        /// </exception>
        public static NameValueCollection Read(IResource resource, string configSection)
        {
            return ConfigurationReader.Read(resource, configSection, new NameValueCollection());
        }

        /// <summary>
        /// Reads the specified configuration section into the supplied
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </summary>
        /// <param name="resource">The resource to read.</param>
        /// <param name="configSection">The section name.</param>
        /// <param name="properties">
        /// The collection that is to be populated. May be
        /// <see langword="null"/>.
        /// </param>
        /// <returns>
        /// A newly populated
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </returns>
        /// <exception cref="System.IO.IOException">
        /// If any errors are encountered while attempting to open a stream
        /// from the supplied <paramref name="resource"/>.
        /// </exception>
        /// <exception cref="System.Xml.XmlException">
        /// If any errors are encountered while loading or reading (this only applies to
        /// v1.1 and greater of the .NET Framework) the actual XML.
        /// </exception>
        /// <exception cref="System.Exception">
        /// If any errors are encountered while loading or reading (this only applies to
        /// v1.0 of the .NET Framework).
        /// </exception>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If the configuration section was otherwise invalid.
        /// </exception>
        public static NameValueCollection Read(
            IResource resource, string configSection, NameValueCollection properties)
        {
            return ConfigurationReader.Read(resource, configSection, properties, true);
        }

        /// <summary>
        /// Reads the specified configuration section into the supplied
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </summary>
        /// <param name="resource">The resource to read.</param>
        /// <param name="configSection">The section name.</param>
        /// <param name="properties">
        /// The collection that is to be populated. May be
        /// <see langword="null"/>.
        /// </param>
        /// <param name="overrideValues">
        /// If a key already exists, is its value to be appended to the current
        /// value or replaced?
        /// </param>
        /// <returns>
        /// The populated
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </returns>
        /// <exception cref="System.IO.IOException">
        /// If any errors are encountered while attempting to open a stream
        /// from the supplied <paramref name="resource"/>.
        /// </exception>
        /// <exception cref="System.Xml.XmlException">
        /// If any errors are encountered while loading or reading (this only applies to
        /// v1.1 and greater of the .NET Framework) the actual XML.
        /// </exception>
        /// <exception cref="System.Exception">
        /// If any errors are encountered while loading or reading (this only applies to
        /// v1.0 of the .NET Framework).
        /// </exception>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If the configuration section was otherwise invalid.
        /// </exception>
        public static NameValueCollection Read(
            IResource resource, string configSection, NameValueCollection properties, bool overrideValues)
        {
            if (properties == null)
            {
                properties = new NameValueCollection();
            }

            NameValueCollection newProperties = (NameValueCollection)GetSection(resource, configSection, typeof(NameValueFileSectionHandler));
            if (newProperties != null)
            {
                PopulateProperties(overrideValues, properties, newProperties);
            }

            return properties;
        }

        /// <summary>
        /// Read from the specified configuration from the supplied XML
        /// <paramref name="document"/> into a
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </summary>
        /// <remarks>
        /// <note>
        /// Does <b>not</b> support section grouping. The supplied XML
        /// <paramref name="document"/> must already be loaded.
        /// </note>
        /// </remarks>
        /// <param name="document">
        /// The <see cref="System.Xml.XmlDocument"/> to read from.
        /// </param>
        /// <param name="configSectionName">
        /// The configuration section name to read.
        /// </param>
        /// <returns>
        /// A newly populated
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </returns>
        /// <exception cref="System.Xml.XmlException">
        /// If any errors are encountered while reading (this only applies to
        /// v1.1 and greater of the .NET Framework).
        /// </exception>
        /// <exception cref="System.Exception">
        /// If any errors are encountered while reading (this only applies to
        /// v1.0 of the .NET Framework).
        /// </exception>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If the configuration section was otherwise invalid.
        /// </exception>
        public static NameValueCollection ReadFromXmlDocument(XmlDocument document,
                                                              string configSectionName)
        {
            // find the config section declaration (if one exists)...
            return (NameValueCollection)GetSectionFromXmlDocument(document, configSectionName);
        }

        /// <summary>
        /// Returns the section from the specified resource with the given section name
        /// </summary>
        public static object GetSection(IResource resource, string configSectionName)
        {
            return GetSection(resource, configSectionName, null);
        }

        /// <summary>
        /// Returns the section from the specified resource with the given section name. Use <paramref name="defaultConfigurationSectionHandlerType"/>
        /// in case no section handler is specified.
        /// </summary>
        public static object GetSection(IResource resource, string configSectionName, Type defaultConfigurationSectionHandlerType)
        {
            using (Stream istm = resource.InputStream)
            {
                ConfigXmlDocument doc = new ConfigXmlDocument();
                doc.Load(istm);
                return GetSectionFromXmlDocument(doc, configSectionName, defaultConfigurationSectionHandlerType);
            }
        }

        /// <summary>
        /// Returns the typed section from the specified resource with the given section name
        /// </summary>
        public static TResult GetSection<TResult>(IResource resource, string configSectionName)
        {
            return GetSection<TResult>(resource, configSectionName, null);
        }

        /// <summary>
        /// Returns the section from the specified resource with the given section name. Use <paramref name="defaultConfigurationSectionHandlerType"/>
        /// in case no section handler is specified.
        /// </summary>
        public static TResult GetSection<TResult>(IResource resource, string configSectionName, Type defaultConfigurationSectionHandlerType)
        {
            object result = GetSection(resource, configSectionName, defaultConfigurationSectionHandlerType);
            if (result != null && !(result is TResult))
            {
                throw new ArgumentException(string.Format("evaluating configuration section {0} does not result in an instance of type {1}", configSectionName, typeof(TResult)));
            }
            return (TResult) result;
        }

        /// <summary>
        /// Returns the typed result of evaluating the specified <paramref name="configSectionName"/>.
        /// </summary>
        /// <exception cref="ArgumentException">if the result's type does not match the expected type</exception>
        public static TResult GetSectionFromXmlDocument<TResult>(XmlDocument configDocument, string configSectionName)
        {
            object result = GetSectionFromXmlDocument(configDocument, configSectionName);
            if (result != null && !(result is TResult))
            {
                throw new ArgumentException(string.Format("evaluating configuration sectoin {0} does not result in an instance of type {1}", configSectionName, typeof(TResult)));
            }
            return (TResult)result;
        }

        /// <summary>
        /// Reads the specified configuration section from the given <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="document"></param>
        /// <param name="configSectionName"></param>
        /// <returns></returns>
        public static object GetSectionFromXmlDocument(XmlDocument document, string configSectionName)
        {
            return GetSectionFromXmlDocument(document, configSectionName, null);
        }

        /// <summary>
        /// Reads the specified configuration section from the given <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="document"></param>
        /// <param name="configSectionName"></param>
        /// <param name="defaultConfigurationSectionHandlerType"></param>
        /// <returns></returns>
        public static object GetSectionFromXmlDocument(XmlDocument document, string configSectionName, Type defaultConfigurationSectionHandlerType)
        {
            Type handlerType = GetSectionHandlerType(document, configSectionName, defaultConfigurationSectionHandlerType);

            // obtain Xml node with section content
            XmlNode sectionContent = document.SelectSingleNode(string.Format("//{0}/{1}", ConfigurationElement, configSectionName));
            if (sectionContent == null)
            {
                // TODO: review if we shouldn't better simply return null here to match the ConfigurationManager's behaviour?
                throw ConfigurationUtils.CreateConfigurationException("Cannot read config section '" + configSectionName + "' - section not found.");
            }

            // IConfigurationSectionHandler
            if (typeof(IConfigurationSectionHandler).IsAssignableFrom(handlerType))
            {
                IConfigurationSectionHandler handler = (IConfigurationSectionHandler)ObjectUtils.InstantiateType(handlerType);
                return handler.Create(null, null, sectionContent);
            }

            // NET 2.0 ConfigurationSection
            if (typeof(ConfigurationSection).IsAssignableFrom(handlerType))
            {
                ConfigurationSection section = CreateConfigurationSection(handlerType, new XmlNodeReader(sectionContent));
                return section;
            }

            // Not supported
            throw ConfigurationUtils.CreateConfigurationException("Configuration section '" + configSectionName + "' is neither of type IConfigurationSectionHandler nor ConfigurationSection.");
        }

        /// <summary>
        /// Determine the configuration section handler type
        /// </summary>
        private static Type GetSectionHandlerType(XmlDocument document, string configSectionName, Type defaultConfigurationSectionHandlerType)
        {
            string[] sectionNameParts = configSectionName.Split('/');
            string sectionHandlerPath = string.Format("//{0}/{1}", ConfigurationElement, ConfigSectionsElement);

            if (sectionNameParts.Length > 1)
            {
                // deal with sectionGroups
                for (int i = 0; i < sectionNameParts.Length - 1; i++)
                {
                    sectionHandlerPath = string.Format("{0}/{1}[@{2}='{3}']", sectionHandlerPath, ConfigSectionGroupElement, ConfigSectionNameAttribute, sectionNameParts[i]);
                }
            }
            sectionHandlerPath = string.Format("{0}/{1}[@{2}='{3}']", sectionHandlerPath, ConfigSectionElement, ConfigSectionNameAttribute, sectionNameParts[sectionNameParts.Length - 1]);

            XmlNode xmlConfig = document.SelectSingleNode(sectionHandlerPath);

            // create appropriate configuration section handler...
            Type handlerType = null;
            if (xmlConfig == null)
            {
                // none specified, use machine inherited
                try
                {
                    XmlDocument machineConfig = new XmlDocument();
                    machineConfig.Load(RuntimeEnvironment.SystemConfigurationFile);
                    xmlConfig = machineConfig.SelectSingleNode(sectionHandlerPath);
                    if (xmlConfig == null)
                    {
                        // TOOD: better throw a sensible exception in case of a missing handler configuration?
                        handlerType = defaultConfigurationSectionHandlerType;
                    }
                }
                catch (PlatformNotSupportedException)
                {
                    if (configSectionName == "connectionStrings")
                    {
                        handlerType = typeof(ConnectionStringsSectionHandler);
                    }
                    else
                    {
                        handlerType = typeof(NameValueSectionHandler);
                    }
                }
            }

            if (xmlConfig != null)
            {
                XmlAttribute xmlConfigType = xmlConfig.Attributes[ConfigSectionTypeAttribute];
                handlerType = TypeResolutionUtils.ResolveType(xmlConfigType.Value);
            }

            if (handlerType == null)
            {
                throw new ConfigurationException(string.Format("missing handler-'type' attribute on configuration section definition for section '{0}'", configSectionName));
            }
            return handlerType;
        }

        private class ConnectionStringsSectionHandler : IConfigurationSectionHandler
        {
            public object Create(object parent, object configContext, XmlNode section)
            {
                var data = new ConnectionStringsSection();
                foreach (XmlNode node in section.ChildNodes)
                {
                    var settings = new ConnectionStringSettings(
                        node.Attributes["name"].Value,
                        node.Attributes["connectionString"]?.Value,
                        node.Attributes["providerName"]?.Value);
                    data.ConnectionStrings.Add(settings);
                }
                return data;
            }
        }


        private delegate void DeserializeSectionMethod(ConfigurationSection section, XmlReader reader);
        private static DeserializeSectionMethod deserialized = (DeserializeSectionMethod)Delegate.CreateDelegate(typeof(DeserializeSectionMethod),
                                                                                                   typeof(ConfigurationSection).GetMethod("DeserializeSection",
                                                                                                                                           BindingFlags.Instance |
                                                                                                                                           BindingFlags.NonPublic));

        private static ConfigurationSection CreateConfigurationSection(Type handlerType, XmlReader reader)
        {
            ConfigurationSection section = (ConfigurationSection)ObjectUtils.InstantiateType(handlerType);
            deserialized(section, reader);
            return section;
        }

        /// <summary>
        /// Populates the supplied <paramref name="properties"/> with values from
        /// a .NET application configuration file.
        /// </summary>
        /// <param name="properties">
        /// The <see cref="System.Collections.Specialized.NameValueCollection"/>
        /// to add any key-value pairs to.
        /// </param>
        /// <param name="configSectionName">
        /// The configuration section name in the a .NET application configuration
        /// file.
        /// </param>
        /// <param name="overrideValues">
        /// If a key already exists, is its value to be appended to the current
        /// value or replaced?
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the supplied
        /// <paramref name="configSectionName"/> was found.
        /// </returns>
        public static bool PopulateFromAppConfig(
            NameValueCollection properties, string configSectionName, bool overrideValues)
        {
            bool sectionFound = false;

            NameValueCollection newProperties
                = ConfigurationUtils.GetSection(configSectionName) as NameValueCollection;

            if (newProperties != null)
            {
                sectionFound = true;
                PopulateProperties(overrideValues, properties, newProperties);
            }
            return sectionFound;
        }

        private static void PopulateProperties(
            bool overrideValues, NameValueCollection properties, NameValueCollection newProperties)
        {
            if (!overrideValues)
            {
                properties.Add(newProperties);
            }
            else
            {
                foreach (string key in newProperties.AllKeys)
                {
                    properties.Set(key, newProperties.Get(key));
                }
            }
        }

        #region Constructor (s) / Destructor

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the ConfigurationReader class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a utility class, and as such has no publicly visible
        /// constructors.
        /// </p>
        /// </remarks>
        private ConfigurationReader()
        {
        }

        // CLOVER:ON

        #endregion
    }
}
