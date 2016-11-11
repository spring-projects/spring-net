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

using NUnit.Framework;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Unit tests that test the event wiring features of the
    /// XmlObjectFactory.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    /// <author>Choy Rim (.NET)</author>
    [TestFixture]
    public class EventWiringTests
    {
        [Ignore("SPRNET-21")]
        [Test]
        public virtual void EventWiringInstanceSinkToPrototypeSource()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("event-wiring-prototypes.xml", GetType()));
            TestEventHandler instanceHandler = factory["instanceSink"] as TestEventHandler;
            ITestObject source = factory["source"] as ITestObject;
            // raise the event... handlers should be notified at this point (obviously)
            source.OnClick();
            Assert.IsTrue(instanceHandler.EventWasHandled,
                          "The instance handler did not get notified when the instance event was raised (and was probably not wired up in the first place).");
        }

        [Test]
        public virtual void SingletonSourcePrototypeSink()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("event-wiring.xml", GetType()));
            ITestObject source = factory["source"] as ITestObject;
            TestEventHandler prototypeEventHandler = factory["prototypeEventListener"] as TestEventHandler;
            // raise the event... handlers should be notified at this point (obviously)
            source.OnClick();
            Assert.IsTrue(prototypeEventHandler.EventWasHandled,
                          "The prototype instance handler did not get notified when the instance event was raised (and was probably not wired up in the first place).");
        }

        [Test]
        public virtual void InstanceEventWiring()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("event-wiring.xml", GetType()));
            ITestObject source = factory["source"] as ITestObject;
            TestEventHandler instanceHandler = factory["instanceEventListener"] as TestEventHandler;
            // raise the event... handlers should be notified at this point (obviously)
            source.OnClick();
            Assert.IsTrue(instanceHandler.EventWasHandled,
                          "The instance handler did not get notified when the instance event was raised (and was probably not wired up in the first place).");
        }

        [Test]
        public virtual void StaticEventWiring()
        {
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(factory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("event-wiring.xml", GetType()));
            TestEventHandler staticHandler = factory["staticEventListener"] as TestEventHandler;
            // raise the event... handlers should be notified at this point (obviously)
            TestObject.OnStaticClick();
            Assert.IsTrue(staticHandler.EventWasHandled,
                          "The instance handler did not get notified when the static event was raised (and was probably not wired up in the first place).");
        }

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            // enable (null appender) logging, to ensure that the logging code is exercised
            // XmlConfigurator.Configure ();
        }
    }
}