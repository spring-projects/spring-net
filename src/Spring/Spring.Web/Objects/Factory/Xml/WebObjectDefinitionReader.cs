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
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// An <see cref="XmlObjectDefinitionReader"/> capable of handling web object definitions (Pages, Controls)
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class WebObjectDefinitionReader : XmlObjectDefinitionReader, IWebObjectNameGenerator
    {
        private readonly string contextVirtualPath;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/> class.
        /// </summary>
        /// <param name="contextVirtualPath">the (rooted) virtual path to resolve relative virtual paths.</param>
        /// <param name="registry">
        /// The <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
        /// instance that this reader works on.
        /// </param>
        /// <param name="resolver">the <see cref="XmlResolver"/> to use for resolving entities.</param>
        public WebObjectDefinitionReader(string contextVirtualPath, IObjectDefinitionRegistry registry, XmlResolver resolver)
            : base(registry, resolver, new WebObjectDefinitionFactory())
        {
            this.contextVirtualPath = contextVirtualPath;
        }

        /// <summary>
        /// Creates the <see cref="IObjectDefinitionDocumentReader"/> to use for actually
        /// reading object definitions from an XML document.
        /// </summary>
        protected override IObjectDefinitionDocumentReader CreateObjectDefinitionDocumentReader()
        {
            return new WebObjectDefinitionDocumentReader(this);
        }

        string IWebObjectNameGenerator.CreatePageDefinitionName(string virtualPath)
        {
            return CreatePageDefinitionName(virtualPath);
        }

        string IWebObjectNameGenerator.CreateControlDefinitionName(string virtualPath)
        {
            return CreateControlDefinitionName(virtualPath);
        }

        /// <summary>
        /// Create an object definition name for the given control path
        /// </summary>
        protected virtual string CreateControlDefinitionName(string virtualPath)
        {
            string objectName;
            objectName = WebObjectUtils.GetControlType(virtualPath).FullName;
            return objectName;
        }

        /// <summary>
        /// Create an object definition name for the given page path
        /// </summary>
        protected virtual string CreatePageDefinitionName(string url)
        {
            string objectName;
            objectName = WebUtils.CombineVirtualPaths(VirtualEnvironment.CurrentExecutionFilePath, url);
            string appPath = VirtualEnvironment.ApplicationVirtualPath;
            if (objectName.ToLower().StartsWith(appPath.ToLower()))
            {
                objectName = objectName.Substring(appPath.Length-1);
            }

            return objectName;
        }
    }
}