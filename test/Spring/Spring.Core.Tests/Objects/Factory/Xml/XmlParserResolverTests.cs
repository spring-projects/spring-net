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

#region Imports

using System;
using System.Xml;
using NUnit.Framework;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Objects.Factory.Xml
{
	/// <summary>
	/// Unit tests for the XmlParserResolver class.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <author>Aleksandar Seovic  2006.11.21</author>
	[TestFixture]
	public sealed class XmlParserResolverTests
	{
		[Test]
		public void RegisterParserWithNullType()
		{
            Assert.Throws<ArgumentNullException>(() => NamespaceParserRegistry.RegisterParser((Type) null));
		}

		[Test]
		public void RegisterParserWithBadParserType()
		{
            Assert.Throws<ArgumentException>(() => NamespaceParserRegistry.RegisterParser(GetType()));
		}

		[Test]
		public void RegisterParserWithNullNamespaceWithoutDefaultValues()
		{
            Assert.Throws<ArgumentNullException>(() => NamespaceParserRegistry.RegisterParser(typeof(NotImplementedXmlObjectDefinitionParser), null, null));
		}

		[Test]
        public void RegisterParserWithEmptyNamespaceWithoutDefaultValues()
		{
            Assert.Throws<ArgumentNullException>(() => NamespaceParserRegistry.RegisterParser(typeof(NotImplementedXmlObjectDefinitionParser), string.Empty, null));
		}

		#region Inner Class : NotImplementedXmlObjectDefinitionParser

		private sealed class NotImplementedXmlObjectDefinitionParser : INamespaceParser
		{
		    #region IXmlObjectDefinitionParser Members

		    /// <summary>
		    /// Invoked by <see cref="NamespaceParserRegistry"/> after construction but before any
		    /// elements have been parsed.
		    /// </summary>
		    public void Init()
		    {
		        throw new NotImplementedException();
		    }

		    #endregion

		    public IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
			{
				throw new NotImplementedException();
			}

		    public ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition,
		                                           ParserContext parserContext)
		    {
		        throw new NotImplementedException();
		    }
		    
		}

		#endregion
	}
}