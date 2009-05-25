#region License

/*
 * Copyright 2002-2009 the original author or authors.
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

using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// An <see cref="DefaultObjectDefinitionDocumentReader"/> capable of handling web objects (Pages,Controls).
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class WebObjectDefinitionDocumentReader : DefaultObjectDefinitionDocumentReader
    {
        private readonly IWebObjectNameGenerator webObjectNameGenerator;

        public WebObjectDefinitionDocumentReader(IWebObjectNameGenerator webObjectNameGenerator)
        {
            AssertUtils.ArgumentNotNull(webObjectNameGenerator, "webObjectNameGenerator");
            this.webObjectNameGenerator = webObjectNameGenerator;
        }

        /// <summary>
        /// Creates an <see cref="WebObjectDefinitionParserHelper"/> instance for the given 
        /// <paramref name="readerContext"/> and <paramref name="root"/> element.
        /// </summary>
        protected override ObjectDefinitionParserHelper CreateHelper(XmlReaderContext readerContext, System.Xml.XmlElement root)
        {
            return new WebObjectDefinitionParserHelper(webObjectNameGenerator, readerContext, root);
        }
    }
}