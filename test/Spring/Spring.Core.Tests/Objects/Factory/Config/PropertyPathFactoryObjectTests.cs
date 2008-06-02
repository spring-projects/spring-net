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
using DotNetMock.Dynamic;
using NUnit.Framework;
using Spring.Core;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the PropertyPathFactoryObject class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class PropertyPathFactoryObjectTests
	{
		[Test]
		public void GetObject_ViaTargetObjectName()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("IsSingleton", true);
			mock.ExpectAndReturn("GetObject", new TestObject("Fiona Apple", 28), "foo");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.TargetObjectName = "foo";
			fac.PropertyPath = "name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mock.Verify();
		}

		[Test]
		public void GetObject_ViaTargetObjectNameWithNestedPropertyPath()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("IsSingleton", true);
			TestObject target = new TestObject("Fiona Apple", 28);
			target.Spouse = target; 
			mock.ExpectAndReturn("GetObject", target, "foo");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.TargetObjectName = "foo";
			fac.PropertyPath = "spouse.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mock.Verify();
		}

		[Test]
		public void GetObject_ViaObjectName()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("IsSingleton", true);
			mock.ExpectAndReturn("GetObject", new TestObject("Fiona Apple", 28), "foo");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mock.Verify();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetObject_ViaObjectNameThatStartsWithAPeriod()
		{
			IDynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("IsSingleton", true);
			mock.ExpectAndReturn("GetObject", new TestObject("Fiona Apple", 28), "foo");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = ".foo.name";
			fac.ObjectFactory = mockFactory;
		}

		[Test]
		public void GetObject_MakeSureLeadingAndTrailingWhitspaceIsTrimmed()
		{
			IDynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("IsSingleton", true);
			mock.ExpectAndReturn("GetObject", new TestObject("Fiona Apple", 28), "foo");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "   \nfoo.name  ";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mock.Verify();
		}

		[Test]
		public void GetObject_ViaObjectNameWithNestedPropertyPath()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("IsSingleton", true);
			TestObject target = new TestObject("Fiona Apple", 28);
			target.Spouse = target; 
			mock.ExpectAndReturn("GetObject", target, "foo");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.spouse.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mock.Verify();
		}

		[Test]
		[ExpectedException(typeof(NullValueInNestedPathException))]
		public void GetObject_ViaObjectNameWithNullInNestedPropertyPath()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("IsSingleton", true);
			mock.ExpectAndReturn("GetObject", new TestObject("Fiona Apple", 28), "foo");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.spouse.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mock.Verify();
		}

		[Test]
		[ExpectedException(typeof(FatalObjectException))]
		public void GetObject_PropertyPathEvaluatesToNull()
		{
			IDynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("IsSingleton", true);
			mock.ExpectAndReturn("GetObject", new TestObject(null, 28), "foo");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.name";
			fac.ObjectFactory = mockFactory;
			fac.GetObject();
		}
	}
}
