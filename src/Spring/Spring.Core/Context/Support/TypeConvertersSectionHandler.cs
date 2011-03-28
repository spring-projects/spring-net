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

using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Xml;

using Spring.Core.TypeConversion;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Configuration section handler for the Spring.NET <c>typeConverters</c>
    /// config section.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Type converters are used to convert objects from one type into another
    /// when injecting property values, evaluating expressions, performing data
    /// binding, etc.
    /// </p>
    /// <p>
    /// They are a very powerful mechanism as they allow Spring.NET to automatically
    /// convert string-based property values from the configuration file into the appropriate
    /// type based on the target property's type or to convert string values submitted
    /// via a web form into a type that is used by your data model when Spring.NET data
    /// binding is used. Because they offer such tremendous help, you should always provide
    /// a type converter implementation for your custom types that you want to be able to use
    /// for injected properties or for data binding.
    /// </p>
    /// <p>
    /// The standard .NET mechanism for specifying type converter for a particular type is
    /// to decorate the type with a <see cref="TypeConverterAttribute"/>, passing the type
    /// of the <see cref="TypeConverter"/>-derived class as a parameter.
    /// </p>
    /// <p>
    /// This mechanism will still work and is a preferred way of defining type converters if
    /// you control the source code for the type that you want to define a converter for. However, 
    /// this configuration section allows you to specify converters for the types that you don't 
    /// control and it also allows you to override some of the standard type converters, such as
    /// the ones that are defined for some of the types in the .NET Base Class Library.
    /// </p>
    /// </remarks>
    /// <example>
    /// <p>
    /// The following example shows how to configure both this section handler and
    /// how to define type converters within a Spring.NET config section:
    /// </p>
    /// <code escaped="true">
    /// <configuration>
    ///     <configSections>
    ///		    <sectionGroup name="spring">
    ///			    <section name="typeConverters" type="Spring.Context.Support.TypeConvertersSectionHandler, Spring.Core"/>
    ///		    </sectionGroup>
    ///     </configSections>
    ///     <spring>
    ///		    <typeConverters>
    ///			    <converter for="Spring.Expressions.IExpression, Spring.Core" type="Spring.Expressions.ExpressionConverter, Spring.Core"/>
    ///			    <converter for="MyTypeAlias" type="MyCompany.MyProject.Converters.MyTypeConverter, MyAssembly"/>
    ///			    ...
    ///		    </typeConverters>
    ///		    ...
    ///     </spring>
    /// </configuration>
    /// </code>
    /// </example>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="Spring.Core.TypeResolution.TypeRegistry"/>
    public class TypeConvertersSectionHandler : IConfigurationSectionHandler
    {
        private const string ConverterElementName = "converter";
        private const string ForAttributeName = "for";
        private const string TypeAttributeName = "type";

        /// <summary>
        /// Populates <see cref="TypeConverterRegistry"/> using values specified in
        /// the <c>typeConverters</c> config section.
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
        /// <see cref="TypeConverterRegistry"/> is populated as a side-effect of 
        /// its execution and thus there is no need to return anything.
        /// </returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            if (section != null)
            {
                XmlNodeList converters = ((XmlElement) section).GetElementsByTagName(ConverterElementName);
                foreach (XmlElement aliasElement in converters)
                {
                    string forType = GetRequiredAttributeValue(aliasElement, ForAttributeName, section);
                    string converterType = GetRequiredAttributeValue(aliasElement, TypeAttributeName, section);
                    TypeConverterRegistry.RegisterConverter(forType, converterType);
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
                                                    "The '{0}' attribute is required for the <converter> element.", requiredAttributeName);
                throw ConfigurationUtils.CreateConfigurationException(errorMessage, section);
            }
            return attribute.Value;
        }
    }
}