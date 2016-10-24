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
using System.Configuration;
using System.Xml;
using NUnit.Framework;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
    /// Unit tests for the NamespaceParsersSectionHandler class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
    public sealed class NamespaceParsersSectionHandlerTests
	{
		[Test]
		public void ParseSectionSunnyDay()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<parsers>
	<parser namespace='http://schemas.springframework.net/testobject' type='Spring.Context.Support.TestObjectConfigParser, Spring.Core.Tests' schemaLocation='assembly://Spring.Core.Tests/Spring.Context.Support/testobject.xsd'/>
</parsers>";

			NamespaceParsersSectionHandler handler = new NamespaceParsersSectionHandler();
			handler.Create(null, null, BuildConfigurationSection(xml));
		}

		[Test]
		public void ParseSectionWithHandlerThatDoesNotImplement_IXmlObjectDefinitionParser()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<parsers>
	<parser namespace='http://schemas.springframework.net/2' type='Spring.Context.Support.NamespaceParsersSectionHandlerTests, Spring.Core.Tests' schemaLocation='assembly://Spring.Core.Tests/Spring.Context.Support/testobject.xsd'/>
</parsers>";

			NamespaceParsersSectionHandler handler = new NamespaceParsersSectionHandler();
            Assert.Throws<ArgumentException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void ParseSectionWithBadTypeForHandler()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<parsers>
	<parser namespace='http://schemas.springframework.net/2' type='Rubbish' schemaLocation='assembly://Spring.Core.Tests/Spring.Context.Support/testobject.xsd'/>
</parsers>";

			NamespaceParsersSectionHandler handler = new NamespaceParsersSectionHandler();
			Assert.Throws<TypeLoadException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void ParseSectionWithNoChildParserNamespaceElements()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<parsers>
	<!-- now't in here -->
</parsers>";

			NamespaceParsersSectionHandler handler = new NamespaceParsersSectionHandler();
			handler.Create(null, null, BuildConfigurationSection(xml));
		}

		[Test]
		public void ParseSectionWithEmptyType()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<parsers>
	<parser namespace='http://schemas.springframework.net/3' type='' schemaLocation='assembly://Spring.Core.Tests/Spring.Context.Support/testobject.xsd'/>
</parsers>";

			NamespaceParsersSectionHandler handler = new NamespaceParsersSectionHandler();
			Assert.Throws<ArgumentNullException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void WithParserElementThatIsMissingTheTypeAttribute()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<parsers>
	<parser namespace='http://schemas.springframework.net/3'/>
</parsers>";

			NamespaceParsersSectionHandler handler = new NamespaceParsersSectionHandler();
            Assert.Throws<ConfigurationErrorsException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		private static XmlNode BuildConfigurationSection(string xml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			return doc.DocumentElement;
		}
	}
}