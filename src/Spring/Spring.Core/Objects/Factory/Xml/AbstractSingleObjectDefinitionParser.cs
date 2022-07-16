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

using System.Xml;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Base Type for those <see cref="IObjectDefinitionParser"/> implementations that
    /// need to parse and define just a single IObjectDefinition.
    /// </summary>
    /// <remarks>
    /// Extend this parser Type when you want to create a single object definition
    /// from an arbitrarily complex XML element. You may wish to consider extending
    /// the <see cref="AbstractSingleObjectDefinitionParser"/> when you want to create a
    /// single Object definition from a relatively simple custom XML element.
    /// <para>The resulting ObjectDefinition will be automatically registered
    /// with the ObjectDefinitionRegistry.  Your job simply is to parse the
    /// custom XML element into a single ObjectDefinition</para>
    /// </remarks>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans</author>
    /// <author>Mark Pollack (.NET)</author>
    public class AbstractSingleObjectDefinitionParser : AbstractObjectDefinitionParser
    {
        #region Methods

        /// <summary>
        /// Central template method to actually parse the supplied XmlElement
        /// into one or more IObjectDefinitions.
        /// </summary>
        /// <param name="element">The element that is to be parsed into one or more <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>s</param>
        /// <param name="parserContext">The the object encapsulating the current state of the parsing process;
        /// provides access to a <see cref="IObjectDefinitionRegistry"/></param>
        /// <returns>
        /// The primary IObjectDefinition resulting from the parsing of the supplied XmlElement
        /// </returns>
        protected override AbstractObjectDefinition ParseInternal(XmlElement element, ParserContext parserContext)
        {
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition();
            string parentName = GetParentName(element);
            if (parentName != null)
            {
                builder.RawObjectDefinition.ParentName = parentName;
            }

            Type objectType = GetObjectType(element);
            if (objectType != null)
            {
                builder.RawObjectDefinition.ObjectType = objectType;
            }
            else
            {
                string objectTypeName = GetObjectTypeName(element);
                if (objectTypeName != null)
                {
                    builder.RawObjectDefinition.ObjectTypeName = objectTypeName;
                }
            }

            // TODO (EE)
//            builder.getRawBeanDefinition().setSource(parserContext.extractSource(element));

            if (parserContext.IsNested)
            {
                // Inner object definition must receive same singleton status as containing object.
                builder.SetSingleton(parserContext.ContainingObjectDefinition.IsSingleton);
            }
            if (parserContext.IsDefaultLazyInit)
            {
                // Default-lazy-init applies to custom object definitions as well.
                builder.SetLazyInit(true);
            }
            DoParse(element, parserContext, builder);
            return builder.ObjectDefinition;

        }

        /// <summary>
        /// Determine the name for the parent of the currently parsed object,
        /// in case of the current object being defined as a child object.
        /// The default implementation returns <c>null</c>
        /// indicating a root object definition.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>the name of the parent object for the currently parsed object.</returns>
        protected virtual string GetParentName(XmlElement element)
        {
            return null;
        }

        /// <summary>
        /// Gets the type of the object corresponding to the supplied XmlElement.
        /// </summary>
        /// <remarks>Note that, for application classes, it is generally preferable to override
        /// <code>GetObjectTypeName</code> instad, in order to avoid a direct
        /// dependence on the object implementation class.  The ObjectDefinitionParser
        /// and its IXmlObjectDefinitionParser (namespace parser) can be used within an
        /// IDE add-in then, even if the application classses are not available in the add-ins
        /// AppDomain.
        /// </remarks>
        /// <param name="element">The element.</param>
        /// <returns>The Type of the class that is being defined via parsing the supplied
        /// Element.</returns>
        protected virtual Type GetObjectType(XmlElement element)
        {
            return null;
        }

        /// <summary>
        /// Gets the name of the object type name (FullName) corresponding to the supplied XmlElement.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The type name of the object that is being defined via parsing the supplied
        /// XmlElement.</returns>
        protected virtual string GetObjectTypeName(XmlElement element)
        {
            return null;
        }

        /// <summary>
        /// Parse the supplied XmlElement and populate the supplied ObjectDefinitionBuilder as required.
        /// </summary>
        /// <remarks>The default implementation delegates to the <code>DoParse</code> version without
        /// ParameterContext argument.</remarks>
        /// <param name="element">The element.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <param name="builder">The builder used to define the <code>IObjectDefinition</code>.</param>
        protected virtual void DoParse(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder)
        {
            DoParse(element, builder);
        }

        /// <summary>
        /// Parse the supplied XmlElement and populate the supplied ObjectDefinitionBuilder as required.
        /// </summary>
        /// <remarks>The default implementation does nothing.</remarks>
        /// <param name="element">The element.</param>
        /// <param name="builder">The builder used to define the <code>IObjectDefinition</code>.</param>
        protected virtual void DoParse(XmlElement element, ObjectDefinitionBuilder builder)
        {

        }

        #endregion
    }
}
