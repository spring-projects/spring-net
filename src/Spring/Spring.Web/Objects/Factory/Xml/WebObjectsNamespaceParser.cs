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



#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// A custom implementation of the
    /// <see cref="INamespaceParser"/>
    /// interface that properly handles web application specific attributes,
    /// such as object scope.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Parses object definitions according to the standard Spring.NET schema.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <see cref="ObjectsNamespaceParser"/>
    public class WebObjectsNamespaceParser : ObjectsNamespaceParser
    {
//        private IObjectDefinitionFactory objectDefinitionFactory;

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="WebObjectsNamespaceParser"/> class.
        /// </summary>
        public WebObjectsNamespaceParser()
        {
//            objectDefinitionFactory = new WebObjectDefinitionFactory();
        }

        #endregion

//        /// <summary>
//        /// Parses an object definition and set various web related properties
//        /// if the definition is an <see cref="RootWebObjectDefinition"/>.
//        /// </summary>
//        /// <param name="element">The object definition element.</param>
//        /// <param name="id">The id / name of the object definition.</param>
//        /// <param name="parserContext">the parser helper</param>
//        /// <returns>The object (definition).</returns>
//        /// <remarks>
//        /// 	<p>
//        /// The <i>'various web related properties'</i> currently includes the
//        /// intended scope of the object.
//        /// </p>
//        /// </remarks>
//        /// <see cref="Spring.Objects.Factory.Support.ObjectScope"/>
//        /// <see cref="Spring.Objects.Factory.Support.IWebObjectDefinition"/>
//        protected override IConfigurableObjectDefinition ParseObjectDefinitionElement(
//            XmlElement element, string id, ParserContext parserContext)
//        {
//            IConfigurableObjectDefinition definition = base.ParseObjectDefinitionElement(element, id, parserContext);
//            IWebObjectDefinition webDefinition = definition as IWebObjectDefinition;
//            
//            if (webDefinition != null)
//            {
//                webDefinition.Scope = GetScope(element.GetAttribute(ObjectDefinitionConstants.ScopeAttribute));
//
//                // force request and session scoped objects to be lazily initialized...
//                if (webDefinition.Scope != ObjectScope.Application)
//                {
//                    definition.IsLazyInit = true;
//                }
//            	
//            	string typeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
//	        	if (typeName.EndsWith(".ascx") || typeName.EndsWith(".master"))
//	        	{
//	        		definition.IsAbstract = true;	
//	        	}				        	
//            }
//        	
//            return definition;
//        }

//        /// <summary>
//        /// Calculates an id for an object definition.
//        /// </summary>
//        /// <param name="element">
//        /// The element containing the object definition.
//        /// </param>
//        /// <param name="aliases">
//        /// The list of names defined for the object; may be <see lang="null"/>
//        /// or even empty.
//        /// </param>
//        /// <returns>
//        /// A calculated object definition id.
//        /// </returns>
//        /// <seealso cref="ObjectsNamespaceParser.CalculateId"/>.
//        protected override string CalculateId(XmlElement element, ArrayList aliases)
//        {
//            return null;
//            string id = null;
//        	string strTypeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute).ToLower();
//            if (strTypeName.EndsWith(".aspx"))
//            {
//                string url = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
//                //id = WebUtils.GetPageName(url);
//                //Type pageType = WebUtils.GetPageType(url);
//                System.Web.UI.Page page = (System.Web.UI.Page)WebObjectUtils.CreatePageInstance(url);
//#if NET_2_0
//                id = page.AppRelativeVirtualPath.Substring(1);
//#else
//				string appPath = HttpContext.Current.Request.ApplicationPath.TrimEnd('\\', '/');
//				id = page.TemplateSourceDirectory.TrimEnd('\\','/') + "/" + WebUtils.GetPageName(url) + ".aspx";
//				if (id.ToLower().StartsWith(appPath.ToLower()))
//				{
//					id = id.Substring(appPath.Length);
//				}
//#endif
//                for(int ai=0;ai<aliases.Count;ai++)
//                {
//                    string alias = (string)aliases[ai];
//                    if (alias != null && alias.Length>0 && alias[0]=='~')
//                    {
//                        aliases[ai] = "/"+alias.Substring(1).TrimStart('/','\\');
//                    }
//                }
//            }
//        	else if (strTypeName.EndsWith(".ascx") || strTypeName.EndsWith(".master"))
//        	{
//                id = WebObjectUtils.GetControlType(strTypeName).FullName;
//        	}
//        	else
//            {
//                id = base.CalculateId(element, aliases);
//            }
//            return id;
//        }

//        /// <summary>
//        /// Gets the scope out of the supplied <paramref name="value"/>.
//        /// </summary>
//        /// <remarks>
//        /// <p>
//        /// If the supplied <paramref name="value"/> is invalid
//        /// (i.e. it does not resolve to one of the 
//        /// <see cref="Spring.Objects.Factory.Support.ObjectScope"/> values),
//        /// then the return value of this method call will be
//        /// <see cref="Spring.Objects.Factory.Support.ObjectScope.Default"/>;
//        /// no exception will be raised (although the value of the invalid
//        /// scope <paramref name="value"/> will be logged).
//        /// </p>
//        /// </remarks>
//        /// <param name="value">The string containing the scope name.</param>
//        /// <returns>The scope.</returns>
//        /// <seealso cref="Spring.Objects.Factory.Support.ObjectScope"/>
//        private ObjectScope GetScope(string value)
//        {
//            ObjectScope scope = ObjectScope.Default;
//            if (StringUtils.HasText(value))
//            {
//                try
//                {
//                    scope = (ObjectScope) Enum.Parse(typeof(ObjectScope), value, true);
//                }
//                catch (ArgumentException ex)
//                {
//                    #region Instrumentation
//
//                    if (log.IsDebugEnabled)
//                    {
//                        log.Debug(string.Format("Error while parsing object scope : '{0}' is an invalid value.",
//                                                value), ex);
//                    }
//
//                    #endregion
//                }
//            }
//            return scope;
//        }
    }
}