#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using Common.Logging;
using Common.Logging.Simple;
using DotNetMock.Dynamic;
using NUnit.Framework;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;

#endregion

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
		[TestFixtureSetUp]
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
            ISideEffectObject singleton = (ISideEffectObject) of.GetObject("singleton");
            Assert.AreEqual(initialCount, singleton.Count);
            singleton.doWork();
            Assert.AreEqual(initialCount + 1, singleton.Count);

            ISideEffectObject prototype = (ISideEffectObject) of.GetObject("prototype");
            Assert.AreEqual(initialCount, prototype.Count);
            singleton.doWork();
            Assert.AreEqual(initialCount, prototype.Count);

            ISideEffectObject prototypeByName = (ISideEffectObject) of.GetObject("prototypeByName");
            Assert.AreEqual(initialCount, prototypeByName.Count);
            singleton.doWork();
            Assert.AreEqual(initialCount, prototypeByName.Count);
        }

		[Test]
		public void TargetType()
		{
			SideEffectObject target = new SideEffectObject();
			IDynamicMock mock = new DynamicMock(typeof (IObjectFactory));
			mock.ExpectAndReturn("IsPrototype", true, null);
			mock.ExpectAndReturn("GetType", typeof(SideEffectObject), null);
			PrototypeTargetSource source = new PrototypeTargetSource();
			source.ObjectFactory = (IObjectFactory) mock.Object;
			Assert.AreEqual(target.GetType(), source.TargetType, "Wrong TargetType being returned.");
			mock.Verify();
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
			IDynamicMock mock = new DynamicMock(typeof (IObjectFactory));
			const string objectName = "Foo";
			mock.ExpectAndReturn("IsPrototype", false, objectName);
			PrototypeTargetSource source = new PrototypeTargetSource();
			source.TargetObjectName = objectName;
			try
			{
				source.ObjectFactory = (IObjectFactory) mock.Object;
				Assert.Fail("Should have thrown an ObjectDefinitionStoreException by this point.");
			}
			catch (ObjectDefinitionStoreException)
			{
				mock.Verify();
			}
		}

		[Test]
		public void GetTarget()
		{
			SideEffectObject target = new SideEffectObject();
			IDynamicMock mock = new DynamicMock(typeof (IObjectFactory));;
			mock.ExpectAndReturn("IsPrototype", true, "foo");
			mock.ExpectAndReturn("GetObject", target, "foo");
		    mock.ExpectAndReturn("GetType", typeof (string), "foo");
			PrototypeTargetSource source = new PrototypeTargetSource();
            source.TargetObjectName = "foo";
			source.ObjectFactory = (IObjectFactory) mock.Object;
			Assert.IsTrue(object.ReferenceEquals(source.GetTarget(), target),
			              "Initial target source reference not being returned by GetTarget().");
			mock.Verify();
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AfterPropertiesSetWithoutTargetObjectNameBeingSet()
		{
			PrototypeTargetSource source = new PrototypeTargetSource();
			source.AfterPropertiesSet();
		}
	}
}