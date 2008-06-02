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
        [Test]
		[ExpectedException(typeof(ArgumentException))]
        public void NullTargetObjectName()
		{
			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			// simulate IFactoryObjectAware interface...
			fac.ObjectFactory = null;
        }
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void WhitespaceTargetObjectName()
		{
			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = string.Empty;
			// simulate IFactoryObjectAware interface...
			fac.ObjectFactory = null;
		}
		
		[Test]
		public void FactoryDoesNotContainTargetObject()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("ContainsObject", false);
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = "bojangles";
			try
			{
				// simulate IFactoryObjectAware interface...
				fac.ObjectFactory = mockFactory;
				Assert.Fail("Must have bailed with a " +
					"NoSuchObjectDefinitionException 'cos the object doesn't " +
					"exist in the associated factory.");
			}
			catch (NoSuchObjectDefinitionException)
			{
				mock.Verify();
			}
		}

		[Test]
		public void DelegatesThroughToFactoryFor_IsSingleton()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("ContainsObject", true);
			mock.ExpectAndReturn("IsSingleton", true);
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = "bojangles";
			fac.ObjectFactory = mockFactory;

			Assert.IsTrue(fac.IsSingleton);
			mock.Verify();
		}

		[Test]
		public void DelegatesThroughToFactoryFor_GetObject()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("ContainsObject", true);
			mock.ExpectAndReturn("GetObject", "Rick");
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = "bojangles";
			fac.ObjectFactory = mockFactory;

			Assert.AreEqual("Rick", fac.GetObject());
			mock.Verify();
		}

		[Test]
		public void DelegatesThroughToFactoryFor_ObjectType()
		{
			DynamicMock mock = new DynamicMock(typeof(IObjectFactory));
			mock.ExpectAndReturn("ContainsObject", true);
			mock.ExpectAndReturn("GetType", GetType());
			IObjectFactory mockFactory = (IObjectFactory) mock.Object;

			ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
			fac.TargetObjectName = "bojangles";
			fac.ObjectFactory = mockFactory;

			Assert.AreEqual(GetType(), fac.ObjectType);
			mock.Verify();
		}
    }
}
