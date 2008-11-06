#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class NamespaceParserRegistryTests
    {
        private class TestNamespaceParser : INamespaceParser
        {
            public void Init()
            {
            }

            public IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
            {
                throw new System.NotImplementedException();
            }

            public ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition, ParserContext parserContext)
            {
                throw new System.NotImplementedException();
            }
        }

#if !NET_1_0
        /// <summary>
        /// This test doesn't work on .NET 1.0 because there are no XmlResolvers for schema loading...
        /// </summary>
        [Test]
        public void CanLoadSchemaImportingOtherSchemaByRelativePath()
        {
            string schemaLocation = TestResourceLoader.GetAssemblyResourceUri( this.GetType(), "NamespaceParserRegistryTests_TestSchema.xsd" );
            NamespaceParserRegistry.RegisterParser(new TestNamespaceParser(), "http://www.example.com/brief", schemaLocation);
            XmlReader vr = XmlUtils.CreateValidatingReader( new StringResource(
                                            @"<?xml version='1.0' encoding='UTF-8' ?>
                            <brief class='foo' />
                            ").InputStream, NamespaceParserRegistry.GetSchemas(), null);
            ConfigXmlDocument newDoc = new ConfigXmlDocument();
            newDoc.Load(vr);
        }
#endif
    }
}