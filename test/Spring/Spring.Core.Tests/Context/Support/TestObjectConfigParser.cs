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

using System.Xml;
using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Support
{
    /// <summary>
    /// A simple parser to create TestObjects.
    /// </summary>
    public class TestObjectConfigParser : ObjectsNamespaceParser
    {
        /// <summary>
        /// Has the method been called?
        /// </summary>
        public static bool ParseElementCalled = false;

        /// <summary>
        /// Create an instance of the parser
        /// </summary>
        public TestObjectConfigParser()
        {
        }

        /// <summary>
        /// Parse the specified element and register any resulting
        /// IObjectDefinitions with the IObjectDefinitionRegistry that is
        /// embedded in the supplied ParserContext.
        /// </summary>
        /// <param name="element">The element to be parsed into one or more IObjectDefinitions</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing
        /// process.</param>
        /// <returns>
        /// The primary IObjectDefinition (can be null as explained above)
        /// </returns>
        /// <remarks>
        /// Implementations should return the primary IObjectDefinition
        /// that results from the parse phase if they wish to used nested
        /// inside (for example) a <code>&lt;property&gt;</code> tag.
        /// <para>Implementations may return null if they will not
        /// be used in a nested scenario.
        /// </para>
        /// </remarks>
        public override IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            ParseElementCalled = true;
            ObjectDefinitionHolder holder = ParseTestObjectDefinition(element, parserContext);
            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, parserContext.Registry);
            return null;
        }


        private ObjectDefinitionHolder ParseTestObjectDefinition(XmlElement rootElement, ParserContext parserContext)
        {
            MutablePropertyValues properties = new MutablePropertyValues();
            
            XmlNodeList childNodes = rootElement.ChildNodes;

            //Get all properties (from non whitespace nodes)
            foreach (XmlNode childNode in childNodes)
            {                
                if (XmlNodeType.Whitespace != childNode.NodeType)
                {
                    properties.Add(new PropertyValue(childNode.LocalName, childNode.InnerText));
                }
            }
            IConfigurableObjectDefinition od = new RootObjectDefinition(typeof (TestObject), null, properties);
            od.IsSingleton = false;

            //HardCoded for now.
            string id = "testObject";
            //id = ObjectDefinitionReaderUtils.GenerateObjectName(od, reader.ObjectReader.Registry);

            return new ObjectDefinitionHolder(od, id);


        }
    }
}