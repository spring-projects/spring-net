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
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// An <see cref="ObjectDefinitionParserHelper"/> capable of handling web objects (Pages,Controls)
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class WebObjectDefinitionParserHelper : ObjectDefinitionParserHelper
    {
        private readonly IWebObjectNameGenerator webObjectNameGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebObjectDefinitionParserHelper"/> class.
        /// </summary>
        /// <param name="webObjectNameGenerator">used for generating object definition names from web object types (page, control)</param>
        /// <param name="readerContext">The reader context.</param>
        /// <param name="root">The root element of the xml document to parse</param>
        public WebObjectDefinitionParserHelper(IWebObjectNameGenerator webObjectNameGenerator, XmlReaderContext readerContext, XmlElement root)
            : base(readerContext, root)
        {
            AssertUtils.ArgumentNotNull(webObjectNameGenerator, "webObjectNameGenerator");
            this.webObjectNameGenerator = webObjectNameGenerator;
        }

        protected override string PostProcessObjectNameAndAliases(string objectName, List<string> aliases, XmlElement element, IObjectDefinition containingDefinition)
        {
            string url = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
            string strTypeName = url.ToLower();
            if (strTypeName.EndsWith(".aspx"))
            {
                if (!StringUtils.HasText(objectName))
                {
                    objectName = webObjectNameGenerator.CreatePageDefinitionName(url);
                }

                // strip leading homepath symbol ('~') from aliases if necessary
                for (int ai = 0; ai < aliases.Count; ai++)
                {
                    string alias = (string)aliases[ai];
                    if (alias != null && alias.Length > 0 && alias[0] == '~')
                    {
                        aliases[ai] = "/" + alias.Substring(1).TrimStart('/', '\\');
                    }
                }
            }
            else if (strTypeName.EndsWith(".ascx") || strTypeName.EndsWith(".master"))
            {
                string controlName = webObjectNameGenerator.CreateControlDefinitionName(url);
                if (!StringUtils.HasText(objectName))
                {
                    objectName = controlName;
                }
                else
                {
                    aliases.Add(controlName);
                }
            }

            return objectName;
        }

        protected override ObjectDefinitionHolder CreateObjectDefinitionHolder(
            XmlElement element,
            IConfigurableObjectDefinition definition,
            string objectName,
            IReadOnlyList<string> aliasesArray)
        {
            if (definition is IWebObjectDefinition webDefinition)
            {
                if (definition.IsSingleton
                    && element.HasAttribute(ObjectDefinitionConstants.ScopeAttribute))
                {
                    webDefinition.Scope = GetScope(element.GetAttribute(ObjectDefinitionConstants.ScopeAttribute));
                }

                // force request and session scoped objects to be lazily initialized...
                if (webDefinition.Scope == ObjectScope.Request
                    || webDefinition.Scope == ObjectScope.Session)
                {
                    definition.IsLazyInit = true;
                }

//                string typeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
                string typeName = definition.ObjectTypeName;
                if (typeName != null
                    && (typeName.EndsWith(".ascx") || typeName.EndsWith(".master")))
                {
                    definition.IsAbstract = true;
                }
            }

            ObjectDefinitionHolder holder = base.CreateObjectDefinitionHolder(element, definition, objectName, aliasesArray);
            return holder;
        }

        /// <summary>
        /// Gets the scope out of the supplied <paramref name="value"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="value"/> is invalid
        /// (i.e. it does not resolve to one of the
        /// <see cref="Spring.Objects.Factory.Support.ObjectScope"/> values),
        /// then the return value of this method call will be
        /// <see cref="Spring.Objects.Factory.Support.ObjectScope.Default"/>;
        /// no exception will be raised (although the value of the invalid
        /// scope <paramref name="value"/> will be logged).
        /// </p>
        /// </remarks>
        /// <param name="value">The string containing the scope name.</param>
        /// <returns>The scope.</returns>
        /// <seealso cref="Spring.Objects.Factory.Support.ObjectScope"/>
        private ObjectScope GetScope(string value)
        {
            ObjectScope scope = ObjectScope.Default;
            if (StringUtils.HasText(value))
            {
                try
                {
                    scope = (ObjectScope)Enum.Parse(typeof(ObjectScope), value, true);
                }
                catch (ArgumentException ex)
                {
                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(string.Format("Error while parsing object scope : '{0}' is an invalid value.",
                                                value), ex);
                    }

                    #endregion
                }
            }
            return scope;
        }
    }
}
