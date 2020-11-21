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

#region Imports

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Spring.Core;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the PropertyRetrievingFactoryObject class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class PropertyRetrievingFactoryObjectTests
	{
		[Test]
		public void Instantiation()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			Assert.IsNotNull(fac.Arguments);
		}

		[Test]
		public void BailsWhenStaticPropertyIsSetToNull()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
            Assert.Throws<ArgumentNullException>(() => fac.StaticProperty = null);
		}

		/// <summary>
		/// Test support for nested static properties.
		/// </summary>
		[Test]
		public void NestedStaticProperty()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.StaticProperty = "Spring.Objects.Factory.Config.PropertyObject.StaticProperty.Age, Spring.Core.Tests";
			fac.AfterPropertiesSet();
			Assert.AreEqual(typeof (int), fac.ObjectType);
			object actual = fac.GetObject();
			Assert.AreEqual(PropertyObject.Age, actual);
		}

		/// <summary>
		/// Test support for nested indexed static and instance properties.
		/// </summary>
		[Test]
		public void MixOfNestedIndexedStaticAndInstanceProperty()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.StaticProperty = "Spring.Objects.Factory.Config.PropertyObject.StaticProperty.Item, Spring.Core.Tests";
			fac.Arguments = new object[] {0};
			fac.AfterPropertiesSet();
			Assert.AreEqual(typeof (int), fac.ObjectType);
			object actual = fac.GetObject();
			Assert.AreEqual(PropertyObject.StaticProperty[0], actual);
		}

		/// <summary>
		/// Test support for really really nested indexed static and instance properties.
		/// </summary>
		[Test]
		public void SuperMixOfNestedIndexedStaticAndInstanceProperty()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.StaticProperty = "Spring.Objects.Factory.Config.PropertyObject.StaticProperty.StaticProperty.Item, Spring.Core.Tests";
			fac.Arguments = new object[] {0};
			fac.AfterPropertiesSet();
			Assert.AreEqual(typeof (int), fac.ObjectType);
			object actual = fac.GetObject();
			Assert.AreEqual(PropertyObject.StaticProperty[0], actual);
		}

		[Test]
		public void ResistsSettingTheArgumentsToNull()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.Arguments = null;
			Assert.IsNotNull(fac.Arguments);
		}

		[Test]
		public void StaticProperty()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.StaticProperty = "Spring.Objects.Factory.Config.PropertyObject.Age, Spring.Core.Tests";
			fac.AfterPropertiesSet();
			Assert.AreEqual(typeof (int), fac.ObjectType);
			object actual = fac.GetObject();
			Assert.AreEqual(PropertyObject.Age, actual);
		}

		[Test]
		public void StaticPropertyThatAintAssemblyQualifiedShouldStillBeResolved()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.StaticProperty = "Spring.Objects.Factory.Config.PropertyObject.Age";
			fac.AfterPropertiesSet();
			Assert.AreEqual(typeof (int), fac.ObjectType);
			object actual = fac.GetObject();
			Assert.AreEqual(PropertyObject.Age, actual);
		}

		[Test]
		public void StaticPropertyViaClassAndFieldName()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetProperty = "Age";
			fac.TargetType = typeof (PropertyObject);
			fac.AfterPropertiesSet();
			object actual = fac.GetObject();
			Assert.AreEqual(PropertyObject.Age, actual);
		}

		[Test]
		public void InstanceProperty()
		{
			PropertyObject expected = new PropertyObject();
			expected.Name = "Haruki Murakami";
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetObject = expected;
			fac.TargetProperty = "Name";
			fac.AfterPropertiesSet();
			object actual = fac.GetObject();
			Assert.AreEqual(expected.Name, actual);
		}

		/// <summary>
		/// Test support for nested properties on an instance.
		/// </summary>
		[Test]
		public void NestedInstanceProperty()
		{
			TestObject person = new TestObject();
			person.Age = 20;
			TestObject spouse = new TestObject();
			spouse.Age = 21;
			person.Spouse = spouse;
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetObject = person;
			fac.TargetProperty = "spouse.age";
			fac.AfterPropertiesSet();
			object actual = fac.GetObject();
			int expectedAge = 21;
			Assert.AreEqual(expectedAge, actual);
		}

		[Test]
		public void IndexedProperty()
		{
			PropertyObject expected = new PropertyObject();
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetObject = expected;
			fac.TargetProperty = "Item";
			fac.Arguments = new object[] {2};
			fac.AfterPropertiesSet();
			object actual = fac.GetObject();
			Assert.AreEqual(expected[2], actual);
		}

		[Test]
		public void BailsWhenReadingIndexedPropertyWithNoArguments()
		{
			PropertyObject expected = new PropertyObject();
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetObject = expected;
			fac.TargetProperty = "Item";
            Assert.Throws<FatalObjectException>(() => fac.AfterPropertiesSet());
		}

		[Test]
		public void BailsOnWriteOnlyProperty()
		{
			PropertyObject expected = new PropertyObject();
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetObject = expected;
			fac.TargetProperty = "Greenness";
            Assert.Throws<NotWritablePropertyException>(() => fac.AfterPropertiesSet());
		}

		[Test]
		public void BailsOnNonExistantProperty()
		{
			PropertyObject expected = new PropertyObject();
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetObject = expected;
			fac.TargetProperty = "Blister";
            Assert.Throws<InvalidPropertyException>(() => fac.AfterPropertiesSet());
		}

		[Test]
		public void IsSingleton()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.IsSingleton = false;
			fac.TargetProperty = "Age";
			fac.TargetType = typeof (PropertyObject);
			fac.AfterPropertiesSet();
			object actual = fac.GetObject();
			Assert.AreEqual(PropertyObject.Age, actual);

			PropertyObject.Age = 94;
			object tryTwo = fac.GetObject();
			Assert.AreEqual(PropertyObject.Age, tryTwo);
		}

		[Test]
		public void BailsWhenNotConfigured()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet(), "One of the TargetType or TargetObject properties must be set.");
		}

		[Test]
		public void BailsWhenJustTargetPropertyIsSet()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetProperty = "Funk";
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
		}

		[Test]
		public void BailsWhenJustTargetTypeIsSet()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetType = GetType();
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
		}

		[Test]
		public void BailsWhenJustTargetObjectIsSet()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetObject = this;
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet(), "The TargetProperty property is required.");
		}

		[Test]
		public void BailsWhenStaticPropertyPassedGumpfh()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
            Assert.Throws<ArgumentException>(() => fac.StaticProperty = "Boog"); // no field specified
		}

		[Test]
		public void StaticPropertyCaseINsenSiTiVE()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.StaticProperty = "System.Globalization.CultureInfo.CURRENtUiCultURE, Mscorlib";
			fac.AfterPropertiesSet();
			Assert.AreEqual(typeof (CultureInfo), fac.ObjectType);
			CultureInfo actual = fac.GetObject() as CultureInfo;
			Assert.IsNotNull(actual);
			Assert.AreEqual(CultureInfo.CurrentUICulture, actual);
		}

		[Test]
		public void GetDateTimeDotNowToTestHandlingOfPrototypesIsCorrect()
		{
			PropertyRetrievingFactoryObject fac = new PropertyRetrievingFactoryObject();
			fac.TargetType = typeof(DateTime);
			fac.TargetProperty = "Now";
			fac.IsSingleton = false;
			fac.AfterPropertiesSet();
			DateTime then = (DateTime) fac.GetObject();
			Assert.IsNotNull(then);
			Thread.Sleep(TimeSpan.FromMilliseconds(10));
			DateTime now = (DateTime) fac.GetObject();
			Assert.IsNotNull(now);
			Assert.AreNotEqual(then, now);
		}
	}

	internal sealed class PropertyObject
	{
		private static int _age = 74;

		public static int Age
		{
			get { return _age; }
			set { _age = value; }
		}

		private string _name;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private int[] _battingAverage = new int[] {1000, 350, 400};

		public int[] Averages
		{
			get { return _battingAverage; }
		}

		public int this[int index]
		{
			get { return _battingAverage[index]; }
			set { _battingAverage[index] = value; }
		}

		public bool Greenness
		{
			set
			{
				// no-op... just here for non-readability
			}
		}

		private static readonly PropertyObject _staticProperty = new PropertyObject();

		public static PropertyObject StaticProperty
		{
			get { return _staticProperty; }
		}
	}
}