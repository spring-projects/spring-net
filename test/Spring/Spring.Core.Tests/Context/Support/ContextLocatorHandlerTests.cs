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

using System.Configuration;
using System.IO;
using System.Xml;
using NUnit.Framework;

using Spring.Objects.Factory;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// Unit tests for the ContextHandler class.
	/// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Rick Evans</author>
	[TestFixture]
	public sealed class ContextHandlerTests
	{
		private XmlElement configurationElement;

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
		public void CreateContextSuccessful()
		{
			const string xmlData =
				@"<context type='Spring.Context.Support.XmlApplicationContext, Spring.Core'>
	<resource uri='assembly://Spring.Core.Tests/Spring.Resources/SimpleAppContext.xml'/>
</context>";
			CreateConfigurationElement(xmlData);

			ContextHandler ctxHandler = new ContextHandler();
			IApplicationContext ctx = (IApplicationContext) ctxHandler.Create(null, null, configurationElement);
			Assert.AreEqual(ctx, ContextRegistry.GetContext());
			Assert.AreEqual(1, ContextRegistry.GetContext().ObjectDefinitionCount);
		}

        [Test]
        public void CreateRootContextFailure()
        {
            const string xmlData =
                      @"<context type='Spring.Context.Support.XmlApplicationContext, Spring.Core'>
	<resource uri='assembly://Spring.Core.Tests/DoesNotExist.xml'/>
</context>";
            CreateConfigurationElement(xmlData);

            ContextHandler ctxHandler = new ContextHandler();
            try
            {
                IApplicationContext ctx = (IApplicationContext) ctxHandler.Create(null, null, configurationElement);
                Assert.Fail("");
            }
            catch(ConfigurationException cfgex)
            {
                Assert.IsInstanceOf(typeof(ObjectDefinitionStoreException), cfgex.InnerException);
            }
        }

        [Test]
        public void CreateChildContextFailure()
        {
            const string xmlData =
                      @"<context type='Spring.Context.Support.XmlApplicationContext, Spring.Core'>
	<resource uri='assembly://Spring.Core.Tests/DoesNotExist.xml'/>
</context>";
            CreateConfigurationElement(xmlData);

            ContextHandler ctxHandler = new ContextHandler();
            try
            {
                IApplicationContext ctx = (IApplicationContext) ctxHandler.Create(new StaticApplicationContext(), null, configurationElement);
                Assert.Fail("");
            }
            catch(ConfigurationException cfgex)
            {
                Assert.IsInstanceOf(typeof(ObjectDefinitionStoreException), cfgex.InnerException);
            }
        }

		/// <summary>
		/// Expect failure when using a type that does not inherit from IApplicationContext
		/// </summary>
		[Test]
		public void ContextNotOfCorrectType()
		{
			const string xmlData =
				@"<context type='Spring.Objects.TestObject, Spring.Core.Tests'>
	<resource uri='assembly://Spring.Core.Tests/Spring.Resources/SimpleAppContext.xml'/>
</context>";
			CreateConfigurationElement(xmlData);
			ContextHandler ctxHandler = new ContextHandler();
            Assert.Throws<ConfigurationErrorsException>(() => ctxHandler.Create(null, null, configurationElement));
		}

		[Test]
		public void CreatedFromNullXmlElement()
		{
			ContextHandler ctxHandler = new ContextHandler();
            Assert.Throws<ConfigurationErrorsException>(() => ctxHandler.Create(null, null, null));
		}

		[Test]
		public void DefaultsToXmlApplicationContextType()
		{
			const string xmlData =
				@"<context>
	<resource uri='assembly://Spring.Core.Tests/Spring.Resources/SimpleAppContext.xml'/>
</context>";
			CreateConfigurationElement(xmlData);
			ContextHandler ctxHandler = new ContextHandler();
			IApplicationContext ctx = (IApplicationContext) ctxHandler.Create(null, null, configurationElement);
			Assert.AreEqual(typeof (XmlApplicationContext), ctx.GetType(),
			                "Default type is not the XmlApplicationContext type; it must be.");
		}

        [Test(Description="SPRNET-105")]
		public void ChokesIfChildContextsUseTheSameName()
		{
			const string xmlData =
				@"<context>
	<resource uri='assembly://Spring.Core.Tests/Spring.Resources/SimpleAppContext.xml'/>
	<context/>
</context>";
			CreateConfigurationElement(xmlData);
			ContextHandler ctxHandler = new ContextHandler();
            Assert.Throws<ConfigurationErrorsException>(() => ctxHandler.Create(null, null, configurationElement));
        }

        private void CreateConfigurationElement(string xmlData)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(xmlData));
            configurationElement = xmlDoc.DocumentElement;
        }

        // integration test; touches just about every class in the Spring.NET core...
        [Test]
        public void LoadParentChildContextsHierarchy()
        {
            //need a second section for another independent test as CongfigurationSettings.GetConfig will 
		    //not be called twice by .NET
            IApplicationContext ctx
                = (IApplicationContext) ConfigurationUtils.GetSection("spring2/context");

            Assert.IsNotNull(ctx);
            IApplicationContext parentCtx = ContextRegistry.GetContext("Parent");
            Assert.IsNotNull(parentCtx, "Parent context not registered.");
            Assert.AreEqual("Parent", parentCtx.Name, "Parent's DisplayName property not picked up from config file.");
            IApplicationContext childCtx = ContextRegistry.GetContext("Child");
            Assert.IsNotNull(childCtx, "Child context not registered.");
            Assert.AreEqual("Child", childCtx.Name, "Child's DisplayName property not picked up from config file.");
            Assert.AreEqual("Parent", childCtx.ParentContext.Name);
            IApplicationContext grandchildCtx = ContextRegistry.GetContext("Grandchild");
            Assert.IsNotNull(grandchildCtx, "Grandchild context not registered.");
            Assert.AreEqual("Grandchild", grandchildCtx.Name, "Grandchild's DisplayName property not picked up from config file.");
            Assert.AreEqual("Child", grandchildCtx.ParentContext.Name);

            // ensure proper objects have been loaded into the correct context...
            Assert.IsTrue(parentCtx.ContainsObjectDefinition("Parent"), "Parent context object not present (must be).");
            Assert.IsFalse(parentCtx.ContainsObjectDefinition("Child"), "Wrong (child context) object present in Parent context.");

            Assert.IsTrue(childCtx.ContainsObjectDefinition("Child"), "Child context object not present (must be).");
            Assert.IsFalse(childCtx.ContainsObjectDefinition("Parent"), "Wrong (parent context) object present in Child context.");

            Assert.IsTrue(grandchildCtx.ContainsObjectDefinition("Grandchild"), "Grandchild context object not present (must be).");
            Assert.IsFalse(grandchildCtx.ContainsObjectDefinition("Child"), "Wrong (parent context) object present in Grandchild context.");
            Assert.IsFalse(grandchildCtx.ContainsObjectDefinition("Parent"), "Wrong (parent context) object present in Grandchild context.");
        }
	}
}