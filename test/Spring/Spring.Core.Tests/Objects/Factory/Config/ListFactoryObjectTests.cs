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

using System;
using System.Collections;
using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the ListFactoryObject class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class ListFactoryObjectTests
	{
		[Test]
		[ExpectedException(typeof (ArgumentException), ExpectedMessage="The Type passed to the TargetListType property must implement the 'System.Collections.IList' interface.")]
		public void SetTargetListTypeToNonListType()
		{
			ListFactoryObject lfo = new ListFactoryObject();
			lfo.TargetListType = typeof (ICollection);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), ExpectedMessage="The Type passed to the TargetListType property cannot be an interface; it must be a concrete class that implements the 'System.Collections.IList' interface.")]
		public void SetTargetListTypeToDerivedIListInterfaceType()
		{
			ListFactoryObject lfo = new ListFactoryObject();
			lfo.TargetListType = typeof (IExtendedList);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), ExpectedMessage="The Type passed to the TargetListType property cannot be abstract (MustInherit in VisualBasic.NET); it must be a concrete class that implements the 'System.Collections.IList' interface.")]
		public void SetTargetListTypeToAbstractIListInterfaceType()
		{
			ListFactoryObject lfo = new ListFactoryObject();
			lfo.TargetListType = typeof (AbstractList);
		}

		private interface IExtendedList : IList
		{
		}

		private abstract class AbstractList : ArrayList
		{
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetTargetListTypeToNull()
		{
			ListFactoryObject lfo = new ListFactoryObject();
			lfo.TargetListType = null;
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), ExpectedMessage="The 'SourceList' property cannot be null (Nothing in Visual Basic.NET).")]
		public void GetObjectWithoutSupplyingASourceList()
		{
			ListFactoryObject lfo = new ListFactoryObject();
			lfo.IsSingleton = false;
			lfo.GetObject();
		}

		[Test]
		public void ObjectTypeReallyIsIList()
		{
			Assert.AreEqual(typeof (IList), new ListFactoryObject().ObjectType);
		}

		[Test]
		public void GetObjectReallyDoesPopulateANewIListInstanceWithTheElementsOfTheSourceList()
		{
			// man, that has got to be my second longest ever test name :D ...
			IList source = new TestObject[]
				{
					new TestObject("Rick", 30),
					new TestObject("Mark", 35),
					new TestObject("Griffin", 21),
				};

			ListFactoryObject lfo = new ListFactoryObject();
			lfo.SourceList = source;
			lfo.AfterPropertiesSet();

			IList list = (IList) lfo.GetObject();
			Assert.IsNotNull(list);
			Assert.AreEqual(source.Count, list.Count);
			for (int i = 0; i < list.Count; ++i)
			{
				TestObject dude = (TestObject) list[i];
				TestObject originalDude = (TestObject) source[i];
				Assert.IsTrue(object.ReferenceEquals(dude, originalDude));
			}
		}
	}
}