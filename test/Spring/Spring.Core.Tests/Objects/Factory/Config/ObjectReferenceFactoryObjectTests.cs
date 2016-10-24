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

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the ObjectReferenceFactoryObject class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class ObjectReferenceFactoryObjectTests
    {
        private MockRepository mocks;
	    private IObjectFactory factory;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            factory = mocks.StrictMock<IObjectFactory>();
        }

        [Test]
        public void NullTargetObjectName()
		{
			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
            // simulate IFactoryObjectAware interface...
            Assert.Throws<ArgumentException>(() => fac.ObjectFactory = null);
        }
		
		[Test]
		public void WhitespaceTargetObjectName()
		{
			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = string.Empty;
            // simulate IFactoryObjectAware interface...
            Assert.Throws<ArgumentException>(() => fac.ObjectFactory = null);
		}
		
		[Test]
		public void FactoryDoesNotContainTargetObject()
		{
		    Expect.Call(factory.ContainsObject("bojangles")).Return(false);
            mocks.ReplayAll();

			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = "bojangles";
			try
			{
				// simulate IFactoryObjectAware interface...
				fac.ObjectFactory = factory;
				Assert.Fail("Must have bailed with a " +
					"NoSuchObjectDefinitionException 'cos the object doesn't " +
					"exist in the associated factory.");
			}
			catch (NoSuchObjectDefinitionException)
			{
				mocks.VerifyAll();
			}
		}

		[Test]
		public void DelegatesThroughToFactoryFor_IsSingleton()
		{
            Expect.Call(factory.ContainsObject("bojangles")).Return(true);
            Expect.Call(factory.IsSingleton("bojangles")).Return(true);
            mocks.ReplayAll();

            ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = "bojangles";
			fac.ObjectFactory = factory;

			Assert.IsTrue(fac.IsSingleton);
			mocks.VerifyAll();
		}

		[Test]
		public void DelegatesThroughToFactoryFor_GetObject()
		{
		    Expect.Call(factory.ContainsObject("bojangles")).Return(true);
            Expect.Call(factory.GetObject("bojangles")).Return("Rick");
            mocks.ReplayAll();

			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = "bojangles";
			fac.ObjectFactory = factory;

			Assert.AreEqual("Rick", fac.GetObject());
			mocks.VerifyAll();
		}

		[Test]
		public void DelegatesThroughToFactoryFor_ObjectType()
		{
            Expect.Call(factory.ContainsObject("bojangles")).Return(true);
            Expect.Call(factory.GetType("bojangles")).Return(GetType());
            mocks.ReplayAll();

			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = "bojangles";
			fac.ObjectFactory = factory;

			Assert.AreEqual(GetType(), fac.ObjectType);
			mocks.VerifyAll();
		}
    }
}
