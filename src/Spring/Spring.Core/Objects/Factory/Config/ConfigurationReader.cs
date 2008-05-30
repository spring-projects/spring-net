#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

#region Imports

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml;

using Common.Logging;
using Spring.Core;
using Spring.Core.IO;
using Spring.Core.TypeResolution;
using Spring.Util;

#endregion

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
	/// <version>$Id: ConfigurationReader.cs,v 1.18 2007/08/08 17:47:13 bbaia Exp $</version>
	public sealed class ConfigurationReader
	{
		private const string ConfigSectionTypeAttribute = "type";
		private const string ConfigurationElement = "configuration";
		private const string ConfigSectionsElement = "configSections";
		private const string ConfigSectionElement = "section";
		private const string ConfigSectionNameAttribute = "name";

		private static readonly ILog _log = LogManager.GetLogger(typeof (ConfigurationReader));

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
			Stream stream = null;
			try
			{
				XmlDocument doc = new XmlDocument();
				stream = resource.InputStream;
				doc.Load(stream);
				NameValueCollection newProperties = ReadFromXmlDocument(doc, configSection);
				if(newProperties != null) 
				{
					PopulateProperties(overrideValues, properties, newProperties);
				}
			}
			finally
			{
				if (stream != null)
				{
					try
					{
						stream.Close();
					}
					catch (IOException ex)
					{
						#region Instrumentation

						if (_log.IsWarnEnabled)
						{
							_log.Warn("Could not close stream from resource " + resource.Description, ex);
						}

						#endregion
					}
				}
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
			XmlNode xmlConfig = document.SelectSingleNode(
				string.Format("//{0}//{1}//{2}[@{3}='{4}']",
					ConfigurationElement, ConfigSectionsElement,
					ConfigSectionElement, ConfigSectionNameAttribute, configSectionName));

			// create appropriate configuration section handler...
			NameValueSectionHandler handler = null;
			if (xmlConfig == null)
			{
				// none specified, so use the default...
				handler = new NameValueSectionHandler();
			}
			else
			{
				XmlAttribute xmlConfigType = xmlConfig.Attributes[ConfigSectionTypeAttribute];
                Type cshType = TypeResolutionUtils.ResolveType(xmlConfigType.Value);
				object o = ObjectUtils.InstantiateType(cshType);
				handler = o as NameValueSectionHandler;
				if (handler == null)
				{
					throw ConfigurationUtils.CreateConfigurationException("Configuration section '" + configSectionName + "' not of type NameValueCollection.");
				}
					
			}
			XmlNode collectionNode = document.SelectSingleNode(
				string.Format("//{0}//{1}", ConfigurationElement, configSectionName));
			if (collectionNode == null)
            {
                throw ConfigurationUtils.CreateConfigurationException("Cannot read properties; config section '" + configSectionName + "' not found.");
			}
			else
			{
				return (NameValueCollection) handler.Create(null, null, collectionNode);
			}
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