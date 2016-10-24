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

using System;
using NUnit.Framework;
using Rhino.Mocks;
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
	    private MockRepository mocks;
	    private IObjectFactory mockFactory;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            mockFactory = mocks.StrictMock<IObjectFactory>();
        }

		[Test]
		public void GetObject_ViaTargetObjectName()
		{
			Expect.Call(mockFactory.IsSingleton("foo")).Return(true);
			Expect.Call(mockFactory.GetObject("foo")).Return(new TestObject("Fiona Apple", 28));
            mocks.ReplayAll();

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.TargetObjectName = "foo";
			fac.PropertyPath = "name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetObject_ViaTargetObjectNameWithNestedPropertyPath()
		{
			Expect.Call(mockFactory.IsSingleton("foo")).Return(true);
			TestObject target = new TestObject("Fiona Apple", 28);
			target.Spouse = target; 
			Expect.Call(mockFactory.GetObject("foo")).Return(target);
            mocks.ReplayAll();

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.TargetObjectName = "foo";
			fac.PropertyPath = "spouse.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetObject_ViaObjectName()
		{
			Expect.Call(mockFactory.IsSingleton("foo")).Return(true);
			Expect.Call(mockFactory.GetObject("foo")).Return(new TestObject("Fiona Apple", 28));
            mocks.ReplayAll();

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetObject_ViaObjectNameThatStartsWithAPeriod()
		{
			Expect.Call(mockFactory.IsSingleton("foo")).Return(true);
			Expect.Call(mockFactory.GetObject("foo")).Return(new TestObject("Fiona Apple", 28));
            mocks.ReplayAll();

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = ".foo.name";
            Assert.Throws<ArgumentException>(() => fac.ObjectFactory = mockFactory);
		}

		[Test]
		public void GetObject_MakeSureLeadingAndTrailingWhitspaceIsTrimmed()
		{
			Expect.Call(mockFactory.IsSingleton("foo")).Return(true);
			Expect.Call(mockFactory.GetObject("foo")).Return(new TestObject("Fiona Apple", 28));
            mocks.ReplayAll();

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "   \nfoo.name  ";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetObject_ViaObjectNameWithNestedPropertyPath()
		{
			Expect.Call(mockFactory.IsSingleton("foo")).Return(true);
			TestObject target = new TestObject("Fiona Apple", 28);
			target.Spouse = target; 
			Expect.Call(mockFactory.GetObject("foo")).Return(target);
            mocks.ReplayAll();

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.spouse.name";
			fac.ObjectFactory = mockFactory;
			string name = (string) fac.GetObject();
			Assert.AreEqual("Fiona Apple", name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetObject_ViaObjectNameWithNullInNestedPropertyPath()
		{
            Expect.Call(mockFactory.IsSingleton("foo")).Return(true);
            Expect.Call(mockFactory.GetObject("foo")).Return(new TestObject("Fiona Apple", 28));
            mocks.ReplayAll();

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.spouse.name";
            Assert.Throws<NullValueInNestedPathException>(() => fac.ObjectFactory = mockFactory);
		}

		[Test]
		public void GetObject_PropertyPathEvaluatesToNull()
		{
            Expect.Call(mockFactory.IsSingleton("foo")).Return(true);
            Expect.Call(mockFactory.GetObject("foo")).Return(new TestObject(null, 28));
            mocks.ReplayAll();

			PropertyPathFactoryObject fac = new PropertyPathFactoryObject();
			fac.ObjectName = "foo.name";
			fac.ObjectFactory = mockFactory;
            Assert.Throws<FatalObjectException>(() => fac.GetObject());
		}
	}
}
