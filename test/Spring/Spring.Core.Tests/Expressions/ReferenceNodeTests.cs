#region License

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

#endregion

using System;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;
using System.Collections.Generic;
using System.Text;

namespace Spring.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ReferenceNodeTests
    {
        private class MyTestObject
        {
            public object MyField { get; set; }
        }

        [TearDown]
        public void TearDown() => ContextRegistry.Clear();

        [Test]
        public void DoesNotCallContextRegistryForLocalObjectFactoryReferences()
        {
            var xml = $@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='foo' type='{typeof(MyTestObject).AssemblyQualifiedName}'>
        <property name='MyField' expression='@(theObject)' />
    </object>
</objects>";

            var objectFactory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
            var theObject = new object();
            objectFactory.RegisterSingleton("theObject", theObject);

            var to = (MyTestObject) objectFactory.GetObject("foo");
            Assert.That(theObject, Is.SameAs(to.MyField));
        }

        [Test]
        public void UseDefaultContextRegistryWhenNoContextProvided()
        {
            var defaultXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='theObject' type='{typeof(MyTestObject).AssemblyQualifiedName}'/>
</objects>";

            var defaultContext = GetContextFromXmlString(defaultXml, AbstractApplicationContext.DefaultRootContextName);
            ContextRegistry.RegisterContext(defaultContext);

            var expectedObject = defaultContext.GetObject("theObject");

            var expression = Expression.Parse("@(theObject)");
            var value = expression.GetValue(null, new Dictionary<string, object>());

            Assert.That(value, Is.SameAs(expectedObject));
        }

        [Test]
        public void ThrowsApplicationContextException_WhenContextNotRegistered()
        {
            var defaultXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='theObject' type='{typeof(MyTestObject).AssemblyQualifiedName}'/>
</objects>";

            var defaultContext = GetContextFromXmlString(defaultXml, AbstractApplicationContext.DefaultRootContextName);
            ContextRegistry.RegisterContext(defaultContext);

            var expression = Expression.Parse("@(anotherContext:theObject).Value");
            void Get() => expression.GetValue(null, new Dictionary<string, object>());

            Assert.That(Get, Throws.InstanceOf<ApplicationContextException>());
        }

        [Test]
        public void WhenContextNameSpecifiedInExpression_UseThatContext()
        {
            const string anotherContextName = "AnotherContext";

            var defaultXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='theObject' type='{typeof(MyTestObject).AssemblyQualifiedName}'/>
</objects>";

            var anotherXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='theObject' type='{typeof(MyTestObject).AssemblyQualifiedName}'/>
</objects>";

            var defaultContext = GetContextFromXmlString(defaultXml, AbstractApplicationContext.DefaultRootContextName);
            ContextRegistry.RegisterContext(defaultContext);

            var anotherContext = GetContextFromXmlString(anotherXml, anotherContextName);
            ContextRegistry.RegisterContext(anotherContext);

            var expectedObject = anotherContext.GetObject("theObject");

            var expression = Expression.Parse($"@({anotherContextName}:theObject)");
            var resolvedObject = expression.GetValue(null, new Dictionary<string, object>());

            Assert.That(resolvedObject, Is.SameAs(expectedObject));
        }

        [Test]
        public void UseObjectFactoryFromVariables()
        {
            const string anotherContextName = "AnotherContext";

            var defaultXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='theObject' type='{typeof(MyTestObject).AssemblyQualifiedName}'/>
</objects>";

            var anotherXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='theObject' type='{typeof(MyTestObject).AssemblyQualifiedName}'/>
</objects>";

            var defaultContext = GetContextFromXmlString(defaultXml, AbstractApplicationContext.DefaultRootContextName);
            ContextRegistry.RegisterContext(defaultContext);

            var anotherContext = GetContextFromXmlString(anotherXml, anotherContextName);
            var variables = new Dictionary<string, object> { [Expression.ReservedVariableNames.RESERVEDPREFIX + "CurrentObjectFactory"] = anotherContext.ObjectFactory };
            var expectedObject = anotherContext.GetObject("theObject");

            var expression = Expression.Parse("@(theObject)");
            var resolvedObject = expression.GetValue(null, variables);

            Assert.That(resolvedObject, Is.SameAs(expectedObject));
        }

        [Test]
        public void ShouldThrowException_WhenFactoryProvidedInVariables_IsNotOfTypeIObjectFactory()
        {
            var defaultXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='theObject' type='{typeof(MyTestObject).AssemblyQualifiedName}'/>
</objects>";

            var defaultContext = GetContextFromXmlString(defaultXml, AbstractApplicationContext.DefaultRootContextName);
            ContextRegistry.RegisterContext(defaultContext);

            var variables = new Dictionary<string, object> { [Expression.ReservedVariableNames.RESERVEDPREFIX + "CurrentObjectFactory"] = new object() };

            var expression = Expression.Parse("@(theObject)");

            void Get() => expression.GetValue(null, variables);

            Assert.That(Get, Throws.InstanceOf<InvalidCastException>());
        }

        private static GenericApplicationContext GetContextFromXmlString(string xmlString, string contextName)
        {
            var stringResource = new StringResource(xmlString, Encoding.UTF8);
            var objectFactory = new XmlObjectFactory(stringResource);

            return new GenericApplicationContext(objectFactory) { Name = contextName };
        }
    }
}
