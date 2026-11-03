/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Configuration;
using System.Xml;
using NUnit.Framework;
using Spring.Objects.Factory;
using Spring.Util;
using ConfigurationException = System.Configuration.ConfigurationException;

namespace Spring.Context.Support;

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
        Assert.That(ContextRegistry.GetContext(), Is.EqualTo(ctx));
        Assert.That(ContextRegistry.GetContext().ObjectDefinitionCount, Is.EqualTo(1));
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
        catch (ConfigurationException cfgex)
        {
            Assert.That(cfgex.InnerException, Is.InstanceOf(typeof(ObjectDefinitionStoreException)));
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
        catch (ConfigurationException cfgex)
        {
            Assert.That(cfgex.InnerException, Is.InstanceOf(typeof(ObjectDefinitionStoreException)));
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
        Assert.That(ctx.GetType(), Is.EqualTo(typeof(XmlApplicationContext)),
            "Default type is not the XmlApplicationContext type; it must be.");
    }

    [Test(Description = "SPRNET-105")]
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

        Assert.That(ctx, Is.Not.Null);
        IApplicationContext parentCtx = ContextRegistry.GetContext("Parent");
        Assert.That(parentCtx, Is.Not.Null, "Parent context not registered.");
        Assert.That(parentCtx.Name, Is.EqualTo("Parent"), "Parent's DisplayName property not picked up from config file.");
        IApplicationContext childCtx = ContextRegistry.GetContext("Child");
        Assert.That(childCtx, Is.Not.Null, "Child context not registered.");
        Assert.That(childCtx.Name, Is.EqualTo("Child"), "Child's DisplayName property not picked up from config file.");
        Assert.That(childCtx.ParentContext.Name, Is.EqualTo("Parent"));
        IApplicationContext grandchildCtx = ContextRegistry.GetContext("Grandchild");
        Assert.That(grandchildCtx, Is.Not.Null, "Grandchild context not registered.");
        Assert.That(grandchildCtx.Name, Is.EqualTo("Grandchild"), "Grandchild's DisplayName property not picked up from config file.");
        Assert.That(grandchildCtx.ParentContext.Name, Is.EqualTo("Child"));

        // ensure proper objects have been loaded into the correct context...
        Assert.That(parentCtx.ContainsObjectDefinition("Parent"), Is.True, "Parent context object not present (must be).");
        Assert.That(parentCtx.ContainsObjectDefinition("Child"), Is.False, "Wrong (child context) object present in Parent context.");

        Assert.That(childCtx.ContainsObjectDefinition("Child"), Is.True, "Child context object not present (must be).");
        Assert.That(childCtx.ContainsObjectDefinition("Parent"), Is.False, "Wrong (parent context) object present in Child context.");

        Assert.That(grandchildCtx.ContainsObjectDefinition("Grandchild"), Is.True, "Grandchild context object not present (must be).");
        Assert.That(grandchildCtx.ContainsObjectDefinition("Child"), Is.False, "Wrong (parent context) object present in Grandchild context.");
        Assert.That(grandchildCtx.ContainsObjectDefinition("Parent"), Is.False, "Wrong (parent context) object present in Grandchild context.");
    }
}
