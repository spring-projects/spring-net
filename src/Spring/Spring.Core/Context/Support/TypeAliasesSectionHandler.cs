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

#region Imports

using System.Configuration;
using System.Globalization;
using System.Xml;

using Spring.Core.TypeResolution;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Configuration section handler for the Spring.NET <c>typeAliases</c>
    /// config section.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Type aliases can be used instead of fully qualified type names anywhere
    /// a type name is expected in a Spring.NET configuration file.
    /// </p>
    /// <p>
    /// This includes type names specified within an object definition, as well
    /// as values of the properties or constructor arguments that expect
    /// <see cref="System.Type"/> instances.
    /// </p>
    /// </remarks>
    /// <example>
    /// <p>
    /// The following example shows how to configure both this section handler and
    /// how to define type aliases within a Spring.NET config section:
    /// </p>
    /// <code escaped="true">
    /// <configuration>
    ///     <configSections>
    ///		    <sectionGroup name="spring">
    ///			    <section name="typeAliases" type="Spring.Context.Support.TypeAliasesSectionHandler, Spring.Core"/>
    ///		    </sectionGroup>
    ///     </configSections>
    ///     <spring>
    ///		    <typeAliases>
    ///			    <alias name="WebServiceExporter" type="Spring.Web.Services.WebServiceExporter, Spring.Web"/>
    ///			    <alias name="MyType" type="MyCompany.MyProject.MyNamespace.MyType, MyAssembly"/>
    ///			    ...
    ///		    </typeAliases>
    ///		    ...
    ///     </spring>
    /// </configuration>
    /// </code>
    /// </example>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="Spring.Core.TypeResolution.TypeRegistry"/>
    public class TypeAliasesSectionHandler : IConfigurationSectionHandler
    {
        private const string AliasElementName = "alias";
        private const string NameAttributeName = "name";
        private const string TypeAttributeName = "type";

        /// <summary>
        /// Populates <see cref="TypeRegistry"/> using values specified in
        /// the <c>typeAliases</c> config section.
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
        /// This method always returns <see langword="null"/>, because the
        /// <see cref="TypeRegistry"/> is populated as a side-effect of this
        /// object's execution and thus there is no need to return anything.
        /// </returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            if (section != null)
            {
                XmlNodeList aliases = ((XmlElement) section).GetElementsByTagName(AliasElementName);
                foreach (XmlElement aliasElement in aliases)
                {
                    string alias = GetRequiredAttributeValue(aliasElement, NameAttributeName, section);
                    string typeName = GetRequiredAttributeValue(aliasElement, TypeAttributeName, section);
                    TypeRegistry.RegisterType(alias, typeName);
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
                                                    "The '{0}' attribute is required for the (type) <alias/> element.", requiredAttributeName);
                throw ConfigurationUtils.CreateConfigurationException(errorMessage, section);
            }
            return attribute.Value;
        }
    }
}