#region License

/*
 * Copyright 2004 the original author or authors.
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

using FakeItEasy;

using NUnit.Framework;

using Spring.Core.IO;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Remoting
{
    /// <summary>
    /// Unit tests for the SaoExporter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class SaoExporterTests : BaseRemotingTestFixture
    {
        [Test]
        public void BailsWhenNotConfigured()
        {
            SaoExporter exp = new SaoExporter();
            Assert.Throws<ArgumentException>(() => exp.AfterPropertiesSet());
        }

        [Test]
        public void BailsIfTargetNotFound()
        {
            using (DefaultListableObjectFactory of = new DefaultListableObjectFactory())
            {
                SaoExporter saoExporter = new SaoExporter();
                saoExporter.ObjectFactory = of;
                saoExporter.TargetName = "DOESNOTEXIST";
                saoExporter.ServiceName = "RemotedSaoSingletonCounter";
                Assert.Throws<NoSuchObjectDefinitionException>(() => saoExporter.AfterPropertiesSet());
            }
        }

        [Test]
        public void ExportSingleton()
        {
            using (DefaultListableObjectFactory of = new DefaultListableObjectFactory())
            {
                of.RegisterSingleton("simpleCounter", new SimpleCounter());
                SaoExporter saoExporter = new SaoExporter();
                saoExporter.ObjectFactory = of;
                saoExporter.TargetName = "simpleCounter";
                saoExporter.ServiceName = "RemotedSaoSingletonCounter";
                saoExporter.AfterPropertiesSet();
                of.RegisterSingleton("simpleCounterExporter", saoExporter); // also tests SaoExporter.Dispose()!

                AssertExportedService(saoExporter.ServiceName, 2);
            }
        }

        [Test]
        public void ExportSingleCall()
        {
            using (DefaultListableObjectFactory of = new DefaultListableObjectFactory())
            {
                of.RegisterObjectDefinition("simpleCounter", new RootObjectDefinition(typeof(SimpleCounter), false));
                SaoExporter saoExporter = new SaoExporter();
                saoExporter.ObjectFactory = of;
                saoExporter.TargetName = "simpleCounter";
                saoExporter.ServiceName = "RemotedSaoSingleCallCounter";
                saoExporter.AfterPropertiesSet();
                of.RegisterSingleton("simpleCounterExporter", saoExporter); // also tests SaoExporter.Dispose()!

                AssertExportedService(saoExporter.ServiceName, 0);
            }
        }

        /// <summary>
        /// Checks that we can also export if IFactoryObject.ObjectType returns an interface type,
        /// </summary>
        [Test(Description = "http://jira.springframework.org/browse/SPRNET-1251")]
        public void CanExportFromFactoryObjectIfObjectTypeIsInterface()
        {
            using (DefaultListableObjectFactory of = new DefaultListableObjectFactory())
            {
                IFactoryObject simpleCounterFactory = A.Fake<IFactoryObject>();
                A.CallTo(() => simpleCounterFactory.ObjectType).Returns(typeof (ISimpleCounter));
                A.CallTo(() => simpleCounterFactory.IsSingleton).Returns(true);
                A.CallTo(() => simpleCounterFactory.GetObject()).Returns(new SimpleCounter());


                of.RegisterSingleton("simpleCounter", simpleCounterFactory);
                SaoExporter saoExporter = new SaoExporter();
                saoExporter.ObjectFactory = of;
                saoExporter.TargetName = "simpleCounter";
                saoExporter.ServiceName = "RemotedSaoCallCounter";
                saoExporter.AfterPropertiesSet();
                of.RegisterSingleton("simpleCounterExporter", saoExporter); // also tests SaoExporter.Dispose()!

                AssertExportedService(saoExporter.ServiceName, 2);
            }
        }

        /// <summary>
        /// Checks that exp an IFactoryObject.ObjectType returns an interface type,
        /// </summary>
        [Test(Description = "http://jira.springframework.org/browse/SPRNET-1251")]
        public void ThrowsTypeLoadExceptionIfProxyInterfacesValueIsSpecifiedInsteadOfListElement()
        {
            using (DefaultListableObjectFactory of = new DefaultListableObjectFactory())
            {
                XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(of);
                reader.LoadObjectDefinitions(new StringResource(
                                                 @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:r='http://www.springframework.net/remoting'>

    <r:saoExporter id='ISimpleCounterExporter' targetName='ISimpleCounterProxy' serviceName='RemotedSaoCounterProxy' />

    <object id='ISimpleCounter' type='Spring.Remoting.SimpleCounter, Spring.Services.Tests' />

    <object id='ISimpleCounterProxy' type='Spring.Aop.Framework.ProxyFactoryObject, Spring.Aop'>
        <property name='proxyInterfaces' value='Spring.Remoting.ISimpleCounter, Spring.Services.Tests' />
        <property name='target' ref='ISimpleCounter'/>
    </object>
</objects>
"));
                try
                {
                    SaoExporter saoExporter = (SaoExporter) of.GetObject("ISimpleCounterExporter");
                    Assert.Fail();
                }
                catch (ObjectCreationException oce)
                {
                    TypeLoadException tle = (TypeLoadException) oce.GetBaseException();
                    Assert.AreEqual("Could not load type from string value ' Spring.Services.Tests'.", tle.Message);
                }
            }
        }

        private void AssertExportedService(string serviceName, int expectedCount)
        {
            ISimpleCounter client = (ISimpleCounter)Activator.GetObject(typeof(ISimpleCounter), "tcp://localhost:8005/" + serviceName);
            client.Count();
            client.Count();

            Assert.AreEqual(expectedCount, client.Counter);
        }
    }
}
