#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using System.Collections;
using NUnit.Framework;
using Spring.Core;
using Spring.Objects.Support;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Set of unit tests for the MethodInvokingFactoryObject.
	/// </summary>
	/// <author>Colin Sampaleanu</author>
	/// <author>Simon White (.NET)</author>
	[TestFixture]
	public class MethodInvokingFactoryObjectTests
	{
        [Test]
        public void InvokeGenericMethod()
        {
            TestClass1 tc1 = new TestClass1();
            MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
            mcfo.TargetType = typeof(Activator);
            mcfo.TargetMethod = "CreateInstance<Spring.Objects.TestObject>";
            mcfo.AfterPropertiesSet();

            object obj = mcfo.GetObject();
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is TestObject);
        }

        [Test]
		public void GetSingletonNonStatic()
		{
			TestClass1 tc1 = new TestClass1();
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetObject = tc1;
			mcfo.TargetMethod = "Method1";
			mcfo.AfterPropertiesSet();
			int i = (int) mcfo.GetObject();
			Assert.IsTrue(i == 1);
			i = (int) mcfo.GetObject();
			Assert.IsTrue(i == 1);
			Assert.IsTrue(mcfo.IsSingleton);
		}

		[Test]
		public void GetNonSingletonNonStatic()
		{
			TestClass1 tc1 = new TestClass1();
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetObject = tc1;
			mcfo.TargetMethod = "Method1";
			mcfo.IsSingleton = false;
			mcfo.AfterPropertiesSet();
			int i = (int) mcfo.GetObject();
			Assert.IsTrue(i == 1);
			i = (int) mcfo.GetObject();
			Assert.IsTrue(i == 2);
			Assert.IsFalse(mcfo.IsSingleton);
		}

		[Test]
		public void GetSingletonStatic()
		{
			TestClass1._staticField1 = 0;
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "StaticMethod1";
			mcfo.AfterPropertiesSet();
			int i = (int) mcfo.GetObject();
			Assert.IsTrue(i == 1);
			i = (int) mcfo.GetObject();
			Assert.IsTrue(i == 1);
			Assert.IsTrue(mcfo.IsSingleton);
		}

		[Test]
		public void GetNonSingletonStatic()
		{
			TestClass1._staticField1 = 0;
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
            mcfo.TargetType = typeof(TestClass1);
            mcfo.TargetMethod = "StaticMethod1";
			mcfo.IsSingleton = false;
			mcfo.AfterPropertiesSet();
			int i = (int) mcfo.GetObject();
			Assert.IsTrue(i == 1);
			i = (int) mcfo.GetObject();
			Assert.IsTrue(i == 2);
			Assert.IsFalse(mcfo.IsSingleton);
		}

		[Test]
		public void InvokingAMethodThatHasAVoidReturnTypeReturnsNullPlaceHolder()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "VoidRetvalMethod";
			mcfo.AfterPropertiesSet();
			Assert.AreEqual(MethodInvoker.Void, mcfo.GetObject());
		}

		[Test]
		public void GetSupertypesMatchNumArgs()
		{
			TestClass1._staticField1 = 0;
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "Supertypes";
			mcfo.Arguments = new Object[] {new ArrayList(), new ArrayList(), "hello"};
			// should pass
			mcfo.AfterPropertiesSet();
		}

		[Test]
		public void GetSupertypesTooManyArgs()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "Supertypes2";
			mcfo.Arguments = new Object[] {new ArrayList(), new ArrayList(), "hello", "bogus"};
            Assert.Throws<ArgumentException>(() => mcfo.AfterPropertiesSet(), "Unable to determine which exact method to call; found '2' matches.");
		}

		[Test]
		public void GetMisMatchedArgumentTypes()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "Supertypes";
			mcfo.Arguments = new Object[] {"1", "2", "3"};
            Assert.Throws<TypeMismatchException>(() => mcfo.AfterPropertiesSet());
		}

		[Test]
		public void GetObjectType()
		{
			TestClass1 tc1 = new TestClass1();
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetObject = tc1;
			mcfo.TargetMethod = "Method1";
			mcfo.AfterPropertiesSet();
			Assert.IsTrue(typeof (int).Equals(mcfo.ObjectType));

			mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "VoidRetvalMethod";
			mcfo.AfterPropertiesSet();
			Type objType = mcfo.ObjectType;
			Assert.IsTrue(objType.Equals(MethodInvoker.Void.GetType()));

			// verify that we can call a method with args that are subtypes of the
			// target method arg types
			TestClass1._staticField1 = 0;
			mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "Supertypes";
			mcfo.Arguments = new Object[] {new ArrayList(), new ArrayList(), "hello"};
			mcfo.AfterPropertiesSet();
			objType = mcfo.ObjectType;
		}

		[Test]
		public void ObjectTypeIsNullIfAfterPropertiesSetHasNotYetBeenInvoked()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "VoidRetvalMethod";
			Assert.IsNull(mcfo.ObjectType,
				"ObjectType property value must only be set to a non null value " +
				"AFTER the AfterPropertiesSet() method has been invoked.");
		}

		[Test]
		public void BailsIfTheTargetMethodPropertyAintSet()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
            Assert.Throws<ArgumentException>(() => mcfo.AfterPropertiesSet(), "The 'TargetMethod' property is required.");
		}

		[Test]
		public void AfterPropertiesSetBogusMethod()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetObject = this;
			mcfo.TargetMethod = "whatever";
            Assert.Throws<MissingMethodException>(() => mcfo.AfterPropertiesSet());
		}

		[Test]
		public void AfterPropertiesSetBogusStaticMethod()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "some.bogus.Method.name";
            Assert.Throws<MissingMethodException>(() => mcfo.AfterPropertiesSet());
		}

		[Test]
		public void AfterPropertiesSetStaticMethodMissingArgs()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetType = typeof (TestClass1);
			mcfo.TargetMethod = "Method1";
            Assert.Throws<ArgumentException>(() => mcfo.AfterPropertiesSet());
		}

		[Test]
		public void AfterPropertiesSetMissingMethod()
		{
			MethodInvokingFactoryObject mcfo = new MethodInvokingFactoryObject();
			mcfo.TargetObject = this;
            Assert.Throws<ArgumentException>(() => mcfo.AfterPropertiesSet());
		}

		[Test]
		public void InvokeWithNullArgument()
		{
			MethodInvoker methodInvoker = new MethodInvoker();
			methodInvoker.TargetType = GetType();
			methodInvoker.TargetMethod = "NullArgument";
			methodInvoker.Arguments = new object[] {null};
			methodInvoker.Prepare();
			methodInvoker.Invoke();
		}

		public static void NullArgument(object arg)
		{
		}

		// a test class to work with
		public class TestClass1
		{
			public static int _staticField1;
			public int _field1 = 0;

			public int Method1()
			{
				return ++_field1;
			}

			public static int StaticMethod1()
			{
				return ++_staticField1;
			}

			public static void VoidRetvalMethod()
			{
			}

			public static void Supertypes(ICollection c, IList l, string s)
			{
			}

			public static void Supertypes2(ICollection c, IList l, string s, object i)
			{
			}

			public static void Supertypes2(ICollection c, IList l, string s, string s2)
			{
			}
		}
	}
}