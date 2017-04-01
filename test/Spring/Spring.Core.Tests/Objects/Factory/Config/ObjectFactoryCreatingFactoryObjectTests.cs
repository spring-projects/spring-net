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

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the ObjectFactoryCreatingFactoryObject class.
	/// </summary>
	/// <author>Colin Sampaleanu</author>
	/// <author>Simon White (.NET)</author>
	[TestFixture]
	public sealed class ObjectFactoryCreatingFactoryObjectTests
	{
        [SetUp]
        public void Setup()
        {
        }

		[Test]
		public void SunnyDay()
		{
			TestObject dude = new TestObject("Rick Evans", 30);
		    IObjectFactory objectFactory = A.Fake<IObjectFactory>();
			const string lookupObjectName = "rick";
		    A.CallTo(() => objectFactory.GetObject(lookupObjectName)).Returns(dude).Twice();
			ObjectFactoryCreatingFactoryObject factory = new ObjectFactoryCreatingFactoryObject();
		    factory.ObjectFactory = objectFactory;
			factory.TargetObjectName = lookupObjectName;
			factory.AfterPropertiesSet();

			IGenericObjectFactory gof = (IGenericObjectFactory) factory.GetObject();
			IGenericObjectFactory gofOther = (IGenericObjectFactory) factory.GetObject();
			Assert.IsTrue(Object.ReferenceEquals(gof, gofOther),
				"Not returning a shared instance (Singleton = true).");
			TestObject one = (TestObject) gof.GetObject();
			Assert.IsNotNull(one, "Must never return null (IFactoryObject contract).");
			TestObject two = (TestObject) gof.GetObject();
			Assert.IsNotNull(two, "Must never return null (IFactoryObject contract).");
			Assert.IsTrue(Object.ReferenceEquals(one, two),
			              "Not returning the same instance.");
		}

		[Test]
		public void PrototypeModeWithSingletonTarget()
		{
			TestObject dude = new TestObject("Rick Evans", 30);
            IObjectFactory objectFactory = A.Fake<IObjectFactory>();
            const string lookupObjectName = "rick";
            A.CallTo(() => objectFactory.GetObject(lookupObjectName)).Returns(dude).Twice();
			ObjectFactoryCreatingFactoryObject factory = new ObjectFactoryCreatingFactoryObject();
		    factory.ObjectFactory = objectFactory;
			factory.TargetObjectName = lookupObjectName;
			factory.IsSingleton = false;
			factory.AfterPropertiesSet();

			IGenericObjectFactory gofOne = (IGenericObjectFactory) factory.GetObject();
			IGenericObjectFactory gofTwo = (IGenericObjectFactory) factory.GetObject();
			Assert.IsFalse(Object.ReferenceEquals(gofOne, gofTwo),
				"Not returning distinct instances (Prototype = true).");
			TestObject one = (TestObject) gofOne.GetObject();
			Assert.IsNotNull(one, "Must never return null (IFactoryObject contract).");
			TestObject two = (TestObject) gofTwo.GetObject();
			Assert.IsNotNull(two, "Must never return null (IFactoryObject contract).");
			Assert.IsTrue(Object.ReferenceEquals(one, two),
				"Not returning the same instance to singleton object.");
		}

		[Test]
		public void WithMissingObjectName()
		{
			ObjectFactoryCreatingFactoryObject factory
				= new ObjectFactoryCreatingFactoryObject();
            Assert.Throws<ArgumentException>(() => factory.AfterPropertiesSet(), "The 'TargetObjectName' property must have a value.");
		}

		[Test]
		public void ObjectTypeReallyIsIGenericObjectFactory()
		{
			Assert.AreEqual(typeof (IGenericObjectFactory), new ObjectFactoryCreatingFactoryObject().ObjectType);
		}
	}
}