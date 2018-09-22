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

using System;

using Common.Logging;
using Common.Logging.Simple;

using FakeItEasy;

using NUnit.Framework;

using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;

namespace Spring.Aop.Target
{
    /// <summary>
    /// Unit tests for the PrototypeTargetSource class.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Federico Spinazzi</author>
    [TestFixture]
    public sealed class PrototypeTargetSourceTests
    {
        /// <summary>
        /// The setup logic executed before the execution of this test fixture.
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            // enable (null appender) logging, just to ensure that the logging code is correct
            LogManager.Adapter = new NoOpLoggerFactoryAdapter();
        }

        /// <summary>
        ///  Test that multiple invocations of the prototype object will result
        ///  in no change to visible state, as a new instance is used.
        ///  With the singleton, there will be change.
        /// </summary>
        [Test]
        public void PrototypeAndSingletonBehaveDifferently()
        {
            int initialCount = 10;
            IObjectFactory of = new XmlObjectFactory(new ReadOnlyXmlTestResource("prototypeTargetSourceTests.xml", GetType()));
            ISideEffectObject singleton = (ISideEffectObject)of.GetObject("singleton");
            Assert.AreEqual(initialCount, singleton.Count);
            singleton.doWork();
            Assert.AreEqual(initialCount + 1, singleton.Count);

            ISideEffectObject prototype = (ISideEffectObject)of.GetObject("prototype");
            Assert.AreEqual(initialCount, prototype.Count);
            singleton.doWork();
            Assert.AreEqual(initialCount, prototype.Count);

            ISideEffectObject prototypeByName = (ISideEffectObject)of.GetObject("prototypeByName");
            Assert.AreEqual(initialCount, prototypeByName.Count);
            singleton.doWork();
            Assert.AreEqual(initialCount, prototypeByName.Count);
        }

        [Test]
        public void TargetType()
        {
            SideEffectObject target = new SideEffectObject();

            IObjectFactory factory = A.Fake<IObjectFactory>();

            A.CallTo(() => factory.IsPrototype(null)).Returns(true);
            A.CallTo(() => factory.GetType(null)).Returns(typeof(SideEffectObject));

            PrototypeTargetSource source = new PrototypeTargetSource();
            source.ObjectFactory = factory;
            Assert.AreEqual(target.GetType(), source.TargetType, "Wrong TargetType being returned.");
        }

        [Test]
        public void IsStatic()
        {
            PrototypeTargetSource source = new PrototypeTargetSource();
            Assert.IsFalse(source.IsStatic, "Must not be static.");
        }

        [Test]
        public void WithNonSingletonTargetObject()
        {
            IObjectFactory factory = A.Fake<IObjectFactory>();
            const string objectName = "Foo";

            A.CallTo(() => factory.IsPrototype(objectName)).Returns(false);
            PrototypeTargetSource source = new PrototypeTargetSource();
            source.TargetObjectName = objectName;

            Assert.Throws<ObjectDefinitionStoreException>(delegate { source.ObjectFactory = factory; });
        }

        [Test]
        public void GetTarget()
        {
            IObjectFactory factory = A.Fake<IObjectFactory>();
            SideEffectObject target = new SideEffectObject();
            A.CallTo(() => factory.IsPrototype("foo")).Returns(true);
            A.CallTo(() => factory.GetObject("foo")).Returns(target);
            A.CallTo(() => factory.GetType("foo")).Returns(typeof(string));

            PrototypeTargetSource source = new PrototypeTargetSource();
            source.TargetObjectName = "foo";
            source.ObjectFactory = factory;
            Assert.IsTrue(object.ReferenceEquals(source.GetTarget(), target),
                "Initial target source reference not being returned by GetTarget().");
        }

        [Test]
        public void AfterPropertiesSetWithoutTargetObjectNameBeingSet()
        {
            PrototypeTargetSource source = new PrototypeTargetSource();
            Assert.Throws<ArgumentNullException>(() => source.AfterPropertiesSet());
        }
    }
}