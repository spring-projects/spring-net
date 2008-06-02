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
	/// Unit tests for the DictionaryFactoryObject class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class DictionaryFactoryObjectTests
	{
		[Test]
		[ExpectedException(typeof (ArgumentException), "The Type passed to the TargetDictionaryType property must implement the 'System.Collections.IDictionary' interface.")]
		public void SetTargetDictionaryTypeToNonDictionaryType()
		{
			DictionaryFactoryObject dfo = new DictionaryFactoryObject();
			dfo.TargetDictionaryType = typeof (ICollection);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), "The Type passed to the TargetDictionaryType property cannot be an interface; it must be a concrete class that implements the 'System.Collections.IDictionary' interface.")]
		public void SetTargetDictionaryTypeToDerivedIDictionaryInterfaceType()
		{
			DictionaryFactoryObject dfo = new DictionaryFactoryObject();
			dfo.TargetDictionaryType = typeof (IExtendedDictionary);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), "The Type passed to the TargetDictionaryType property cannot be abstract (MustInherit in VisualBasic.NET); it must be a concrete class that implements the 'System.Collections.IDictionary' interface.")]
		public void SetTargetDictionaryTypeToAbstractIDictionaryInterfaceType()
		{
			DictionaryFactoryObject dfo = new DictionaryFactoryObject();
			dfo.TargetDictionaryType = typeof (AbstractDictionary);
		}

		private interface IExtendedDictionary : IDictionary
		{
		}

		private abstract class AbstractDictionary : Hashtable
		{
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetTargetDictionaryTypeToNull()
		{
			DictionaryFactoryObject dfo = new DictionaryFactoryObject();
			dfo.TargetDictionaryType = null;
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), "The 'SourceDictionary' property cannot be null (Nothing in Visual Basic.NET).")]
		public void GetObjectWithoutSupplyingASourceDictionary()
		{
			DictionaryFactoryObject dfo = new DictionaryFactoryObject();
			dfo.IsSingleton = false;
			dfo.GetObject();
		}

		[Test]
		public void ObjectTypeReallyIsIDictionary()
		{
			Assert.AreEqual(typeof (IDictionary), new DictionaryFactoryObject().ObjectType);
		}

		[Test]
		public void GetObjectReallyDoesPopulateANewIDictionaryInstanceWithTheElementsOfTheSourceDictionary()
		{
			// man, that has got to be my longest ever test name :D ...
			IDictionary source = new Hashtable();
			TestObject rick = new TestObject("Rick", 30);
			source.Add(rick.Name, rick);
			TestObject mark = new TestObject("Mark", 35);
			source.Add(mark.Name, mark);
			TestObject griffin = new TestObject("Griffin", 21);
			source.Add(griffin.Name, griffin);

			DictionaryFactoryObject dfo = new DictionaryFactoryObject();
			dfo.SourceDictionary = source;
			dfo.AfterPropertiesSet();

			IDictionary dic = (IDictionary) dfo.GetObject();
			Assert.IsNotNull(dic);
			Assert.AreEqual(source.Count, dic.Count);
			foreach (DictionaryEntry entry in dic)
			{
				string name = (string) entry.Key;
				TestObject dude = (TestObject) entry.Value;
				TestObject originalDude = (TestObject) source[name];
				Assert.IsTrue(object.ReferenceEquals(dude, originalDude));
			}
		}
	}
}