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
using Spring.Core.TypeResolution;
using Spring.Objects;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// Unit tests for the TypeAliasesSectionHandler class.
	/// </summary>
	[TestFixture]
	public sealed class TypeAliasesSectionHandlerTests
	{
		[Test]
		public void ParseSectionSunnyDay()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='fiona.apple' type='Spring.Objects.TestObject, Spring.Core.Tests'/>
</typeAliases>";

			Assert.IsNull(TypeRegistry.ResolveType("fiona.apple"), "TypeRegistry already contains an alias for the type being tested (it must not).");

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
			handler.Create(null, null, BuildConfigurationSection(xml));

			Type type = TypeRegistry.ResolveType("fiona.apple");
			Assert.AreEqual(typeof (TestObject), type, "The type alias was not registered by the TypeAliasesSectionHandler.");
		}

        [Test]
        public void WithGenericType()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='myDictionary' type='System.Collections.Generic.Dictionary&lt;int,string>'/>
</typeAliases>";

            TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
            handler.Create(null, null, BuildConfigurationSection(xml));
        }

        [Test]
        public void WithGenericTypeDefinition()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='gDictionary' type='System.Collections.Generic.Dictionary&lt;,>'/>
    <alias name='myDictionary' type='gDictionary&lt;string, int>'/>
</typeAliases>";

            TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
            handler.Create(null, null, BuildConfigurationSection(xml));
        }

		[Test]
		public void WithNonExistentType()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='fiona.apple' type='schwing'/>
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
            Assert.Throws<TypeLoadException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void WithEmptyTypeName()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='fiona.apple' type=''/>
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
            Assert.Throws<ArgumentNullException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void WithWhitespacedTypeName()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='fiona.apple' type=' 
  
'/>
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
            Assert.Throws<ArgumentNullException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void WithEmptyAlias()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='' type='Spring.Objects.TestObject, Spring.Core.Tests'/>
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
            Assert.Throws<ArgumentNullException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void WithWhitespacedAlias()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='
 ' type='Spring.Objects.TestObject, Spring.Core.Tests'/>
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
            Assert.Throws<ArgumentNullException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void ParseSectionWithNoChildParserNamespaceElements()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<!-- now't in here -->
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
			handler.Create(null, null, BuildConfigurationSection(xml));
		}

		[Test]
		public void ParseSectionWithGuffChildParserNamespaceElementsIsAllowed()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<!-- some guff elements -->
	<so/>
	<hot/>
	<right/>
	<now/>
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
			handler.Create(null, null, BuildConfigurationSection(xml));
		}

		[Test]
		public void WithAliasElementThatIsMissingTheNameAttribute()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias type='Spring.Objects.TestObject, Spring.Core.Tests'/>
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
            Assert.Throws<ConfigurationErrorsException>(() => handler.Create(null, null, BuildConfigurationSection(xml)));
		}

		[Test]
		public void WithAliasElementThatIsMissingTheTypeAttribute()
		{
			const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<typeAliases>
	<alias name='bing'/>
</typeAliases>";

			TypeAliasesSectionHandler handler = new TypeAliasesSectionHandler();
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