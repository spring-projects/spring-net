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

using System;
using System.IO;
using System.Xml;
using NUnit.Framework;

#endregion



namespace Spring.Context.Support
{
    public class WebContextHandlerTests
    {
        [SetUp]
        public void SetUp()
        {
            ContextRegistry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            ContextRegistry.Clear();
        }

        [Test]
        public void should_return_registered_context_with_ContextRegistry()
        {
			const string xmlData =
				@"<context type='Spring.Context.Support.XmlApplicationContext, Spring.Core'>
	                <resource uri='assembly://Spring.Web.Tests/Spring.Context.Support/WebContextHandlerTests.xml'/>
                 </context>";
            GenericApplicationContext expectedContext = new GenericApplicationContext(null, false, null);
            ContextRegistry.RegisterContext(expectedContext);
            WebContextHandler webContextHandler = new WebContextHandler();
            
            Object actualContext = webContextHandler.Create(null, null, CreateConfigurationElement(xmlData));

            Assert.AreSame(expectedContext, actualContext);
        }

        private XmlNode CreateConfigurationElement(string xmlData)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(xmlData));
            return xmlDoc.DocumentElement;
        }
    }
}
