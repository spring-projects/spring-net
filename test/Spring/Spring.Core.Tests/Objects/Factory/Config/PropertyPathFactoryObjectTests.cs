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

using FakeItEasy;
using NUnit.Framework;

using Spring.Core;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the PropertyPathFactoryObject class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class PropertyPathFactoryObjectTests
	{
	    private IObjectFactory mockFactory;

        [SetUp]
        public void SetUp()
        {
            mockFactory = A.Fake<IObjectFactory>();
        }

		[Test]
		public void GetObject_ViaTargetObjectName()
		{
			A.CallTo(() => mockFactory.IsSingleton("foo")).Returns(true);
			A.CallTo(() => mockFactory.GetObject("foo")).Returns(new TestObject("Fiona Apple", 28));

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.TargetObjectName = "foo";
			fac.PropertyPath = "name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
		}

		[Test]
		public void GetObject_ViaTargetObjectNameWithNestedPropertyPath()
		{
			A.CallTo(() => mockFactory.IsSingleton("foo")).Returns(true);
			TestObject target = new TestObject("Fiona Apple", 28);
			target.Spouse = target;
			A.CallTo(() => mockFactory.GetObject("foo")).Returns(target);

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.TargetObjectName = "foo";
			fac.PropertyPath = "spouse.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
		}

		[Test]
		public void GetObject_ViaObjectName()
		{
			A.CallTo(() => mockFactory.GetObject("foo")).Returns(new TestObject("Fiona Apple", 28));
			A.CallTo(() => mockFactory.IsSingleton("foo")).Returns(true);

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
		}

		[Test]
		public void GetObject_ViaObjectNameThatStartsWithAPeriod()
		{
			A.CallTo(() => mockFactory.IsSingleton("foo")).Returns(true);
			A.CallTo(() => mockFactory.GetObject("foo")).Returns(new TestObject("Fiona Apple", 28));

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = ".foo.name";
            Assert.Throws<ArgumentException>(() => fac.ObjectFactory = mockFactory);
		}

		[Test]
		public void GetObject_MakeSureLeadingAndTrailingWhitspaceIsTrimmed()
		{
			A.CallTo(() => mockFactory.IsSingleton("foo")).Returns(true);
			A.CallTo(() => mockFactory.GetObject("foo")).Returns(new TestObject("Fiona Apple", 28));

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "   \nfoo.name  ";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
		}

		[Test]
		public void GetObject_ViaObjectNameWithNestedPropertyPath()
		{
			A.CallTo(() => mockFactory.IsSingleton("foo")).Returns(true);
			TestObject target = new TestObject("Fiona Apple", 28);
			target.Spouse = target;
			A.CallTo(() => mockFactory.GetObject("foo")).Returns(target);

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.spouse.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
		}

		[Test]
		public void GetObject_ViaObjectNameWithNullInNestedPropertyPath()
		{
            A.CallTo(() => mockFactory.IsSingleton("foo")).Returns(true);
            A.CallTo(() => mockFactory.GetObject("foo")).Returns(new TestObject("Fiona Apple", 28));

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.spouse.name";
            Assert.Throws<NullValueInNestedPathException>(() => fac.ObjectFactory = mockFactory);
		}

		[Test]
		public void GetObject_PropertyPathEvaluatesToNull()
		{
            A.CallTo(() => mockFactory.IsSingleton("foo")).Returns(true);
            A.CallTo(() => mockFactory.GetObject("foo")).Returns(new TestObject(null, 28));

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.name";
			fac.ObjectFactory = mockFactory;
            Assert.Throws<FatalObjectException>(() => fac.GetObject());
		}
	}
}
