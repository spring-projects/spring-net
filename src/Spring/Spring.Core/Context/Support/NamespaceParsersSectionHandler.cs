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

#region Imports

using System.Configuration;
using System.Globalization;
using System.Xml;

using Spring.Core.TypeResolution;
using Spring.Objects.Factory.Xml;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// Configuration section handler for the (recommended, Spring.NET standard) <c>parsers</c>
	/// config section.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Spring.NET allows the registration of custom configuration parsers that
	/// can be used to create simplified configuration schemas that better
	/// describe object definitions.
	/// </p>
	/// <p>
	/// For example, Spring.NET uses this facility internally in order to
	/// define simplified schemas for various AOP, Data and Services definitions.
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// The following example shows how to configure both this section handler
	/// and how to define custom configuration parsers within a Spring.NET
	/// config section.
	/// </p>
	/// <code escaped="true">
	/// <configuration>
	///     <configSections>
	///		    <sectionGroup name="spring">
    ///			    <section name="parsers" type="Spring.Context.Support.NamespaceParsersSectionHandler, Spring.Core"/>
	///		    </sectionGroup>
	///     </configSections>
	///     <spring>
	///		    <parsers>
    ///			    <parser type="Spring.Aop.Config.AopNamespaceParser, Spring.Aop"/>
    ///			    <parser type="Spring.Data.Config.DatabaseNamespaceParser, Spring.Data"/>
	///			    ...
	///		    </parsers>
	///		    ...
	///     </spring>
	/// </configuration>
	/// </code>
	/// </example>
	/// <author>Aleksandar Seovic</author>
	/// <seealso cref="NamespaceParserRegistry"/>
    public class NamespaceParsersSectionHandler : IConfigurationSectionHandler
	{
		private const string ParserElementName = "parser";
        private const string TypeAttributeName = "type";
        private const string NamespaceAttributeName = "namespace";
		private const string SchemaLocationAttributeName = "schemaLocation";

		/// <summary>
		/// Registers parsers specified in the (recommended, Spring.NET standard)
		/// <c>parsers</c> config section with the <see cref="NamespaceParserRegistry"/>.
		/// </summary>
		/// <param name="parent">
		/// The configuration settings in a corresponding parent
		/// configuration section.
		/// </param>
		/// <param name="configContext">
		/// The configuration context when called from the ASP.NET
		/// configuration system. Otherwise, this parameter is reserved and
		/// is <see langword="null"/>.
		/// </param>
		/// <param name="section">
		/// The <see cref="System.Xml.XmlNode"/> for the section.
		/// </param>
		/// <returns>
		/// This method always returns <see langword="null"/>, because parsers
		/// are registered as a side-effect of this object's execution and there
		/// is thus no need to return anything.
		/// </returns>
		public object Create(object parent, object configContext, XmlNode section)
		{
            if (section != null)
            {
                XmlNodeList parsers = ((XmlElement)section).GetElementsByTagName(ParserElementName);
                foreach (XmlElement parserElement in parsers)
                {
                    string parserTypeName = GetRequiredAttributeValue(parserElement, TypeAttributeName, section);
                    string xmlNamespace = parserElement.GetAttribute(NamespaceAttributeName);
                    string schemaLocation = parserElement.GetAttribute(SchemaLocationAttributeName);

                    Type parserType = TypeResolutionUtils.ResolveType(parserTypeName);
                    NamespaceParserRegistry.RegisterParser(parserType, xmlNamespace, schemaLocation);
                }
            }
            return null;
		}

		private static string GetRequiredAttributeValue(
			XmlElement aliasElement, string requiredAttributeName, XmlNode section)
		{
			XmlAttribute attribute = aliasElement.GetAttributeNode(requiredAttributeName);
			if (attribute == null)
			{
                string errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "The '{0}' attribute is required for the <parser/> element.", requiredAttributeName);
				throw ConfigurationUtils.CreateConfigurationException(errorMessage, section);
			}
			return attribute.Value;
		}
	}
}
