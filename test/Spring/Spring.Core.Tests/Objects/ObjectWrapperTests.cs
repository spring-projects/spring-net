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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using Spring.Collections;
using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Objects.Factory;
using Spring.Util;

#endregion

namespace Spring.Objects
{
	/// <summary>
	/// Unit tests for the ObjectWrapper class.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Mark Pollack (.NET)</author>
	/// <author>Rick Evans (.NET)</author>
	[TestFixture]
	public sealed class ObjectWrapperTests
	{
		/// <summary>
		/// The setup logic executed before the execution of this test fixture.
		/// </summary>
		[OneTimeSetUp]
		public void FixtureSetUp()
		{
			// enable logging (to nowhere), just to exercisee the logging code...
            LogManager.Adapter = new NoOpLoggerFactoryAdapter();
		}

		#region Classes Used During Tests
        public class Person
        {
            private IList favoriteNames = new ArrayList();
            private IDictionary properties = new Hashtable();
            private string sillyString;
            
            public Person()
            {
                favoriteNames.Add("p1");
                favoriteNames.Add("p2");
            }
            public Person(IList favNums)
            {
                favoriteNames = favNums;
            }

            public string SillyString
            {
                get { return sillyString;  }
            }
            public IList FavoriteNames
            {
                get { return favoriteNames; }
            }
            
            public IDictionary Properties
            {
                get { return properties;  }
            }
            public string this[int index]
            {
                get { return (string)favoriteNames[index]; }
                set { favoriteNames[index] = value; }
            }
            public string this[string keyName]
            {
                get { return (string) properties[keyName]; }
                set { properties.Add(keyName,value); }
            }
            public string this[int index, string keyname]
            {
                get { return sillyString;  }
                set { sillyString = index + "-" + keyname + ",val=" + value;}
            }

        }

	    public class GenericPerson
        {
            private List<string> favoriteNames = new List<string>();
            private IDictionary<string, string> properties = new Dictionary<string, string>();
       	       

            public GenericPerson()
            {
                favoriteNames.Add("p1");
                favoriteNames.Add("p2");
            }
            public GenericPerson(List<string> favNums)
            {
                favoriteNames = favNums;
            }

	        /*
            public string this[int index]
            {
                get { return (string)favoriteNames[index]; }
                set { favoriteNames[index] = value; }
            }
             */
            public string this[string keyName]
            {
                get { return properties[keyName]; }
                set { properties.Add(keyName, value); }
            }

        }
	    
        public class AltPerson
        {
            private IList favoriteNames = new ArrayList();
            public AltPerson()
            {
                favoriteNames.Add("ap1");
                favoriteNames.Add("ap2");
            }
            
            public AltPerson(IList favNums)
            {
                favoriteNames = favNums;
            }

            [IndexerName("FavoriteName")]
            public string this[int index]
            {
                get { return (string)favoriteNames[index]; }
                set { favoriteNames[index] = value; }
                
            }

        }
		public class NoNullsList : ArrayList
		{
			public override int Add(object value)
			{
				if (value == null)
				{
					throw new NullReferenceException("Adding nulls is not supported in this implementation.");
				}
				return base.Add(value);
			}
		}

	    
	    
		private class Honey
		{
			public Honey(IList akas)
			{
				_akas = new Aliases(akas);
			}

			protected Aliases _akas;

			public Aliases AlsoKnownAs
			{
				get { return _akas; }
			}

			public string this[int index]
			{
				get { return AlsoKnownAs[index]; }
			}
		}

		private class NonReadableHoney : Honey
		{
			public NonReadableHoney(IList akas) : base(akas)
			{
			}

			new public Aliases AlsoKnownAs
			{
				set { _akas = value; }
			}
		}

		private class Milk
		{
			public Milk(Honey[] honeys)
			{
			}

			public Honey[] Honeys
			{
				set
				{
				}
			}
		}

		private sealed class Aliases
		{
			private IList _names;

			public Aliases() : this(new ArrayList())
			{
			}

			public Aliases(IList names)
			{
				_names = names;
			}

			public string this[int index]
			{
				get { return (string) _names[index]; }
				set { _names[index] = value; }
			}
		}

		private sealed class RealNestedTestObject
		{
			public TestObject Datum
			{
				get { return _datum; }
				set { _datum = value; }
			}

			private TestObject _datum;
		}

		private sealed class WanPropsClass
		{
			public bool IsWan
			{
				get { return true; }
			}
		}

		private sealed class StringAppenderConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof (string))
				{
					return true;
				}
				return base.CanConvertFrom(context, sourceType);
			}

			public override object ConvertFrom(
				ITypeDescriptorContext context, CultureInfo culture, object val)
			{
				if (val is string)
				{
					return "OctopusOil : " + val;
				}
				return base.ConvertFrom(context, culture, val);
			}
		}

		private sealed class TestStringArrayConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof (string[]))
				{
					return true;
				}
				return base.CanConvertFrom(context, sourceType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object val)
			{
				if (val is string[])
				{
					return "-" + StringUtils.CollectionToCommaDelimitedString(val as string[]) + "-";
				}
				return base.ConvertFrom(context, culture, val);
			}
		}

		private sealed class TestObjectArrayConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof (string))
				{
					return true;
				}
				return base.CanConvertFrom(context, sourceType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object val)

			{
				if (val is string)
				{
					return new TestObject((string) val, 99);
				}
				return base.ConvertFrom(context, culture, val);
			}
		}

		private sealed class ObjectWithTypeProperty
		{
			private Type _type;

			public Type Type
			{
				get { return _type; }
				set { _type = value; }
			}
		}

		/// <summary>
		/// A class for use in testing the object wrapper with read only properties.
		/// </summary>
		private sealed class NoRead
		{
			public int Age
			{
				set
				{
				}
			}
		}

		private sealed class GetterObject
		{
			public string Name
			{
				get
				{
					if (_name == null)
					{
						throw new ApplicationException("name property must be set");
					}
					return _name;
				}
				set { _name = value; }
			}

			private string _name;
		}

		private sealed class ThrowsException
		{
			public void DoSomething(Exception t)
			{
				throw t;
			}
		}

		internal sealed class PrimitiveArrayObject
		{
			public int[] Array
			{
				get { return array; }
				set { this.array = value; }
			}

			private int[] array;
		}

		private sealed class PropsTest
		{
			public NameValueCollection Properties
			{
				set
				{
				}
			}

			public String Name
			{
				set
				{
				}
			}

			public String[] StringArray
			{
				set { this.stringArray = value; }
			}

			public int[] IntArray
			{
				set { this.intArray = value; }
			}

			public String[] stringArray;
			public int[] intArray;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Factory method for getting an ObjectWrapper instance.
		/// </summary>
		/// <returns>A new object wrapper (with no WrappedObject).</returns>
		private ObjectWrapper GetWrapper()
		{
			return new ObjectWrapper();
		}

		/// <summary>
		/// Factory method for getting an ObjectWrapper instance.
		/// </summary>
		/// <param name="objectToBeWrapped">
		/// The object that is to be wrapped by the returned object wrapper.
		/// </param>
		/// <returns>
		/// A new object wrapper that wraps the supplied <paramref name="objectToBeWrapped"/>.
		/// </returns>
		private ObjectWrapper GetWrapper(object objectToBeWrapped)
		{
			return new ObjectWrapper(objectToBeWrapped);
		}

		#endregion

		[Test]
		public void SetPropertyUsingValueThatNeedsConversionWithNoCustomConverterRegistered()
		{
			ObjectWrapper wrapper = GetWrapper(new TestObject("Rick", 30));
            // needs conversion to NestedTestObject...
            Assert.Throws<TypeMismatchException>(() => wrapper.SetPropertyValue("doctor", "Pollack, Pinch, & Pounce"));
		}

		[Test]
		[Ignore("not used")]
		public void GetValueOfCustomIndexerProperty()
		{
			ObjectWrapper wrapper = GetWrapper();
			Honey darwin = new Honey(new string[] {"dickens", "of gaunt"});
			wrapper.WrappedInstance = darwin;
			string alias = (string) wrapper.GetPropertyValue("AlsoKnownAs[1]");
			Assert.AreEqual("of gaunt", alias);

            alias = (string) wrapper.GetPropertyValue("[1]");
		    Assert.AreEqual("of gaunt", alias, "indexer on object not working.");
		    wrapper.SetPropertyValue("Item[1]", "of foobar");
            alias = (string)wrapper.GetPropertyValue("[1]");
            Assert.AreEqual("of foobar", alias, "indexer on object not working.");
		    
		}

        [Test]
        [Ignore("not used")]
        public void GetSetIndexerProperties()
        {
            IList favNames = new ArrayList();
            favNames.Add("Master Shake");
            favNames.Add("Meatwad");
            favNames.Add("Frylock");
            Person p = new Person(favNames);
            ObjectWrapper wrapper = GetWrapper();
            wrapper.WrappedInstance = p;

            //This is 'new' Spring.Expressions notation, i.e. not "Item[i]" but plain [i].
            string name = (string)wrapper.GetPropertyValue("[0]");
            Assert.AreEqual("Master Shake", name);
            wrapper.SetPropertyValue("[0]", "Carl");
            name = (string)wrapper.GetPropertyValue("[0]");
            Assert.AreEqual("Carl", name);

            //Try with Custom Indexer Name.
            favNames = new ArrayList();
            favNames.Add("Master Shake");
            favNames.Add("Meatwad");
            favNames.Add("Frylock");
            AltPerson ap = new AltPerson(favNames);
            wrapper = GetWrapper();
            wrapper.WrappedInstance = ap;

            name = (string)wrapper.GetPropertyValue("FavoriteName[0]");
            Assert.AreEqual("Master Shake", name);

        }

		[Test]
		public void GetValueOfCustomIndexerPropertyWithMalformedIndexer()
		{
			ObjectWrapper wrapper = GetWrapper();
			Honey darwin = new Honey(new string[] {"dickens", "of gaunt"});
			wrapper.WrappedInstance = darwin;
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("AlsoKnownAs[1"));
		}

		[Test]
		public void GetPropertyTypeWithNullPropertyPath()
		{
            Assert.Throws<FatalObjectException>(() => GetWrapper().GetPropertyType(null));
		}

		[Test]
		public void GetPropertyTypeWithEmptyPropertyPath()
		{
            Assert.Throws<FatalObjectException>(() => GetWrapper().GetPropertyType(string.Empty));
		}

		[Test]
		public void GetPropertyTypeWithWhitespacedPropertyPath()
		{
            Assert.Throws<FatalObjectException>(() => GetWrapper().GetPropertyType("      "));
		}

		[Test]
		public void GetPropertyType()
		{
			ObjectWrapper wrapper = GetWrapper(new TestObject());
			Type propertyType = wrapper.GetPropertyType("name");
			Assert.AreEqual(typeof (string), propertyType);
		}

		[Test]
		public void GetPropertyTypeFromField_SPRNET502()
		{
			ObjectWrapper wrapper = GetWrapper(new TestObject());
			Type propertyType = wrapper.GetPropertyType("FileModeEnum");
			Assert.AreEqual(typeof (FileMode), propertyType);
		}

		[Test]
		public void GetPropertyTypeWithNestedLookup()
		{
			TestObject target = new TestObject();
			target.Spouse = new TestObject("Fiona", 28);
			ObjectWrapper wrapper = GetWrapper(target);
			Type propertyType = wrapper.GetPropertyType("spouse.age");
			Assert.AreEqual(typeof (int), propertyType);
		}

		[Test]
		public void SetValueOfCustomIndexerPropertyWithNonReadablePropertyInIndexedPath()
		{
			ObjectWrapper wrapper = GetWrapper();
			Honey[] honeys = new NonReadableHoney[] {new NonReadableHoney(new string[] {"hsu", "feng"})};
			wrapper.WrappedInstance = new Milk(honeys);
            Assert.Throws<NotWritablePropertyException>(() => wrapper.SetPropertyValue("Honeys[0][0]", "mei"));
		}

		[Test]
		public void SetPropertyThatRequiresTypeConversionWithNonConvertibleType()
		{
			ObjectWrapper wrapper = GetWrapper();
			TestObject to = new TestObject();
			wrapper.WrappedInstance = to;
            Assert.Throws<TypeMismatchException>(() => wrapper.SetPropertyValue("RealLawyer", "Noob"));
		}

		/// <summary>
		/// This test blows up because index is out of range.
		/// </summary>
		[Test]
		public void SetIndexedPropertyOnListThatsOutOfRange()
		{
			TestObject to = new TestObject();
			ObjectWrapper wrapper = GetWrapper(to);
			to.Friends = new NoNullsList();
            Assert.Throws<InvalidPropertyException>(() => wrapper.SetPropertyValue("Friends[5]", "Inheritance Tax"));
		}

		/// <summary>
		/// Tests that a property lookup such as <c>foo[0][1]['ben'][12]</c> is ok
		/// Well, rather it tests that its valid... 'cos man, that shoh ain't ok :D
		/// </summary>
		[Test]
		public void GetChainedIndexers()
		{
			Honey to = new Honey(new string[] {"wee", "jakey", "ned"});
			ObjectWrapper wrapper = GetWrapper(to);
			Assert.AreEqual('j', wrapper.GetPropertyValue("AlsoKnownAs[1][0]"));
		}

		/// <summary>
		/// Test that passing a null value of the object to manipulate with the ObjectWrapper
		/// will throw a FatalPropertyException with the correct exception message.
		/// </summary>
		[Test]
		public void NullObject()
		{
            Assert.Throws<FatalObjectException>(() => new ObjectWrapper((object) null));
		}

		[Test]
		public void InstantiateWithInterfaceType()
		{
            Assert.Throws<FatalObjectException>(() => new ObjectWrapper(typeof (IList)));
		}

		[Test]
		public void InstantiateWithAbstractType()
		{
            Assert.Throws<FatalObjectException>(() => new ObjectWrapper(typeof (AbstractObjectFactoryTests)));
		}

		[Test]
		public void InstantiateWithOkType()
		{
			IObjectWrapper wrapper = GetWrapper(typeof (TestObject));
			Assert.IsNotNull(wrapper.WrappedInstance);
		}

		[Test]
		public void NestedProperties()
		{
			string doctorCompany = "";
			string lawyerCompany = "Dr. Sueem";
			TestObject to = new TestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			wrapper.SetPropertyValue("Doctor.Company", doctorCompany);
			wrapper.SetPropertyValue("Lawyer.Company", lawyerCompany);
			Assert.AreEqual(doctorCompany, to.Doctor.Company);
			Assert.AreEqual(lawyerCompany, to.Lawyer.Company);
		}

		[Test]
		public void GetterThrowsException()
		{
			GetterObject go = new GetterObject();
			IObjectWrapper wrapper = GetWrapper(go);
			wrapper.SetPropertyValue("Name", "tom");
			Assert.IsTrue(go.Name.Equals("tom"), "Expected name to be set to 'tom'");
		}

		[Test]
		public void TryToReadTheValueOfAWriteOnlyProperty()
		{
			NoRead nr = new NoRead();
			ObjectWrapper wrapper = GetWrapper(nr);
            Assert.Throws<NotReadablePropertyException>(() => wrapper.GetPropertyValue("Age"));
		}

		[Test]
		public void TryToReadAnIndexedValueFromANullProperty()
		{
			TestObject o = new TestObject();
			o.Friends = null;
			ObjectWrapper wrapper = GetWrapper(o);
            Assert.Throws<NullValueInNestedPathException>(() => wrapper.GetPropertyValue("Friends[2]"));
		}

		/// <summary>
		/// Test that applying an empty MutablePropertyValues does not modify the object contents.
		/// </summary>
		[Test]
		public void EmptyPropertyValuesSet()
		{
			TestObject t = new TestObject();
			int age = 50;
			string name = "Tony";
			t.Age = age;
			t.Name = name;
			IObjectWrapper wrapper = GetWrapper(t);
			Assert.IsTrue(t.Age.Equals(age), "Age is not set correctly");
			Assert.IsTrue(name.Equals(t.Name), "Name is not set correctly");
			wrapper.SetPropertyValues(new MutablePropertyValues());
			Assert.IsTrue(t.Age.Equals(age), "Age is not set correctly");
			Assert.IsTrue(name.Equals(t.Name), "Name is not set correctly");
		}

		/// <summary>
		/// Test basic ObjectWrapper functionality by setting properties using MutablePropertyValues.
		/// </summary>
		[Test]
		public void AllValid()
		{
			TestObject t = new TestObject();
			string newName = "tony";
			int newAge = 65;
			string newTouchy = "valid";
			IObjectWrapper wrapper = GetWrapper(t);
			MutablePropertyValues pvs = new MutablePropertyValues();
			pvs.Add(new PropertyValue("Age", newAge));
			pvs.Add(new PropertyValue("Name", newName));
			pvs.Add(new PropertyValue("Touchy", newTouchy));
			wrapper.SetPropertyValues(pvs);
			Assert.IsTrue(t.Name.Equals(newName), "Validly set property must stick");
			Assert.IsTrue(t.Touchy.Equals(newTouchy), "Validly set property must stick");
			Assert.IsTrue(t.Age == newAge, "Validly set property must stick");
		}

		[Test]
		public void IndividualAllValid()
		{
			TestObject t = new TestObject();
			String newName = "tony";
			int newAge = 65;
			string newTouchy = "valid";
			IObjectWrapper wrapper = GetWrapper(t);
			wrapper.SetPropertyValue("Age", newAge);
			wrapper.SetPropertyValue(new PropertyValue("Name", newName));
			wrapper.SetPropertyValue(new PropertyValue("Touchy", newTouchy));
			Assert.IsTrue(t.Name.Equals(newName), "Validly set property must stick");
			Assert.IsTrue(t.Touchy.Equals(newTouchy), "Validly set property must stick");
			Assert.IsTrue(t.Age == newAge, "Validly set property must stick");
		}

		[Test]
		public void SettingAnInvalidValue()
		{
			TestObject t = new TestObject();
			string newName = "tony";
			try
			{
				IObjectWrapper wrapper = GetWrapper(t);
				MutablePropertyValues pvs = new MutablePropertyValues();
				pvs.Add(new PropertyValue("Age", "foobar"));
				pvs.Add(new PropertyValue("Name", newName));
				wrapper.SetPropertyValues(pvs);
				Assert.Fail("Should throw exception when setting an invalid value");
			}
			catch (PropertyAccessExceptionsException ex)
			{
				Assert.IsTrue(ex.ExceptionCount == 1, "Must contain 2 exceptions");
				// Test validly set property matches
				Assert.IsTrue(t.Name.Equals(newName), "Validly set property must stick");
				Assert.IsTrue(t.Age == 0, "Invalidly set property must retain old value");
			}
			catch (Exception ex)
			{
				Assert.Fail(
					"Shouldn't throw exception other than PropertyAccessExceptions.  Exception Message = " + ex.Message);
			}
		}

		[Test]
		public void ArrayToStringConversion()
		{
			TestObject t = new TestObject();
			IObjectWrapper wrapper = GetWrapper(t);
			TypeConverterRegistry.RegisterConverter(typeof(string), new TestStringArrayConverter());
			wrapper.SetPropertyValue("Name", new string[] {"a", "b"});
			Assert.AreEqual("-a,b-", t.Name);
		}

		[Test]
		public void ArrayToArrayConversion()
		{
			IndexedTestObject to = new IndexedTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
            TypeConverterRegistry.RegisterConverter(typeof(TestObject), new TestObjectArrayConverter());
			wrapper.SetPropertyValue("Array", new string[] {"a", "b"});
			Assert.AreEqual("a", to.Array[0].Name);
			Assert.AreEqual("b", to.Array[1].Name);
		}

		[Test]
		public void PrimitiveArray()
		{
			PrimitiveArrayObject to = new PrimitiveArrayObject();
			IObjectWrapper wrapper = GetWrapper(to);
			wrapper.SetPropertyValue("Array", new String[] {"1", "2"});
			Assert.AreEqual(2, to.Array.Length);
			Assert.AreEqual(1, to.Array[0]);
			Assert.AreEqual(2, to.Array[1]);
		}

		[Test]
		public void PrimitiveArrayFromCommaDelimitedString()
		{
			PrimitiveArrayObject to = new PrimitiveArrayObject();
			IObjectWrapper wrapper = GetWrapper(to);
			wrapper.SetPropertyValue("Array", "1,2");
			Assert.AreEqual(2, to.Array.Length);
			Assert.AreEqual(1, to.Array[0]);
			Assert.AreEqual(2, to.Array[1]);
		}

		[Test]
		public void ObjectWrapperUpdates()
		{
			TestObject to = new TestObject("Rick", 2);
			int newAge = 33;
			ObjectWrapper wrapper = GetWrapper(to);
			to.Age = newAge;
			object owAge = wrapper.GetPropertyValue("Age");
			Assert.IsTrue(owAge is Int32, "Age is an integer");
			int owi = (int) owAge;
			Assert.IsTrue(owi == newAge, "Object wrapper must pick up changes");
		}

		[Test]
		public void SetWrappedInstanceOfSameClass()
		{
			TestObject to = new TestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Age = 11;

			TestObject to2 = new TestObject();
			wrapper.WrappedInstance = to2;

			wrapper.SetPropertyValue("Age", 14);
			Assert.IsTrue(to2.Age == 14, "2nd changed");
			Assert.IsTrue(to.Age == 11, "1st didn't change");
		}

		[Test]
		public void SetWrappedInstanceOfDifferentClass()
		{
			ThrowsException tex = new ThrowsException();
			ObjectWrapper wrapper = GetWrapper(tex);
			TestObject to2 = new TestObject();
			wrapper.WrappedInstance = to2;

			wrapper.SetPropertyValue("Age", 14);
			Assert.IsTrue(to2.Age == 14);
		}

		[Test]
		public void TypeMismatch()
		{
			TestObject to = new TestObject();
			IObjectWrapper wrapper = GetWrapper(to);
            Assert.Throws<TypeMismatchException>(() => wrapper.SetPropertyValue("Age", "foobar"));
		}

		[Test]
		public void EmptyValueForPrimitiveProperty()
		{
			TestObject to = new TestObject();
			ObjectWrapper wrapper = GetWrapper(to);
            Assert.Throws<TypeMismatchException>(() => wrapper.SetPropertyValue("Age", ""));
		}

		[Test]
		public void GetProtectedPropertyValue()
		{
			TestObject foo = new TestObject();
			IObjectWrapper wrapper = GetWrapper(foo);
			string happyPlace = (string) wrapper.GetPropertyValue("HappyPlace");
			Assert.AreEqual(TestObject.DefaultHappyPlace, happyPlace,
			                "Failed to read the value of a property with 'protected' access.");
		}

		[Test]
		public void SetProtectedPropertyValue()
		{
			TestObject foo = new TestObject();
			IObjectWrapper wrapper = GetWrapper(foo);
			const string expectedHappyPlace = "The Rockies! Brrr...";
			wrapper.SetPropertyValue(new PropertyValue("HappyPlace", expectedHappyPlace));
			string happyPlace = (string) wrapper.GetPropertyValue("HappyPlace");
			Assert.AreEqual(expectedHappyPlace, happyPlace,
			                "Failed to read / write the value of a property with 'protected' access.");
		}

		[Test]
		public void GetPrivatePropertyValue()
		{
			TestObject foo = new TestObject();
			IObjectWrapper wrapper = GetWrapper(foo);
			string[] contents = (string[]) wrapper.GetPropertyValue("SamsoniteSuitcase");
			Assert.IsTrue(ArrayUtils.AreEqual(TestObject.DefaultContentsOfTheSuitcase, contents),
			              "Failed to read the value of a property with 'private' access.");
		}

		[Test]
		public void SetPrivatePropertyValue()
		{
			TestObject foo = new TestObject();
			IObjectWrapper wrapper = GetWrapper(foo);
			string[] expectedContents = new string[] {"Lloyd's Soul", "Harry's John Denver Records"};
			wrapper.SetPropertyValue(new PropertyValue("SamsoniteSuitcase", expectedContents));
			string[] contents = (string[]) wrapper.GetPropertyValue("SamsoniteSuitcase");
			Assert.IsTrue(ArrayUtils.AreEqual(expectedContents, contents),
			              "Failed to read / write the value of a property with 'private' access.");
		}

		[Test]
		public void GetNestedProperty()
		{
			ITestObject rod = new TestObject("rod", 31);
			ITestObject kerry = new TestObject("kerry", 35);
			rod.Spouse = kerry;
			kerry.Spouse = rod;
			IObjectWrapper wrapper = GetWrapper(rod);
			int KA = (int) wrapper.GetPropertyValue("Spouse.Age");
			Assert.IsTrue(KA == 35, "Expected kerry's age to be 35");

			int RA = (int) wrapper.GetPropertyValue("Spouse.Spouse.Age");
			Assert.IsTrue(RA == 31, "Expected rod's age to be 31");

			ITestObject spousesSpouse = (ITestObject) wrapper.GetPropertyValue("Spouse.Spouse");
			Assert.IsTrue(rod == spousesSpouse, "spousesSpouse == initial point");
		}

		[Test]
		public void GetNestedPropertyValueNullValue()
		{
			TestObject rod = new TestObject("rod", 31);
			rod.Doctor = new NestedTestObject(null);
            Assert.Throws<NullValueInNestedPathException>(() => GetWrapper(rod).GetPropertyValue("Doctor.Company.Length"));
		}

		[Test]
		public void SetNestedProperty()
		{
			ITestObject rod = new TestObject("rod", 31);
			ITestObject kerry = new TestObject("kerry", 0);
			IObjectWrapper wrapper = GetWrapper(rod);
			wrapper.SetPropertyValue("Spouse", kerry);

			Assert.AreEqual(rod.Spouse, kerry, "nested set did not work");
			wrapper.SetPropertyValue("Spouse.Spouse", rod);
			Assert.AreEqual(rod, rod.Spouse.Spouse, "Nested set did not work.");
			wrapper.SetPropertyValue("Spouse.Age", 100);
			Assert.AreEqual(100, kerry.Age, "Nested setting of primitive property did not work.");
		}

		[Test]
		public void SetPropertyValue()
		{
			ITestObject bigby = new TestObject("Bigby", 4500);
			ITestObject snow = new TestObject("Snow", 2500);
			IObjectWrapper wrapper = GetWrapper(bigby);
			wrapper.SetPropertyValue("Spouse", snow);
			Assert.AreEqual(snow, bigby.Spouse);
		}

		[Test]
		public void SetIndexedPropertyValueOnUninitializedPath()
		{
			TestObject obj = new TestObject("Bill", 4500);
			IObjectWrapper wrapper = GetWrapper(obj);
            Assert.Throws<NullValueInNestedPathException>(() => wrapper.SetPropertyValue("hats [0]", "Hicks & Co"));
		}

		[Test]
		public void SetIndexedPropertyValueOnNonIndexableType()
		{
			TestObject obj = new TestObject("Bill", 4500);
			IObjectWrapper wrapper = GetWrapper(obj);
            Assert.Throws<InvalidPropertyException>(() => wrapper.SetPropertyValue("doctor [0]", "Hicks & Co"));
		}

		[Test]
		public void SetPrimitivePropertyToNullReference()
		{
			TestObject obj = new TestObject("Bill", 4500);
			IObjectWrapper wrapper = GetWrapper(obj);
            Assert.Throws<TypeMismatchException>(() => wrapper.SetPropertyValue("Age", null));
		}

		[Test]
		public void SetPropertyValueIgnoresCase()
		{
			ITestObject bigby = new TestObject("Bigby", 4500);
			ITestObject snow = new TestObject("Snow", 2500);
			IObjectWrapper wrapper = GetWrapper(bigby);
			wrapper.SetPropertyValue("spouse", snow);
			Assert.AreEqual(snow, bigby.Spouse,
			                "Property setting is not case insensitive with regard to the property name (and for CLS compliance it should be).");
		}

		[Test]
		public void IntArrayProperty()
		{
			PropsTest pt = new PropsTest();
			IObjectWrapper wrapper = GetWrapper(pt);
			wrapper.SetPropertyValue("IntArray", new int[] {4, 5, 2, 3});
			Assert.IsTrue(pt.intArray.Length == 4, "intArray length = 4");
			Assert.IsTrue(pt.intArray[0] == 4 && pt.intArray[1] == 5 && pt.intArray[2] == 2 && pt.intArray[3] == 3,
			              "correct values");
			wrapper.SetPropertyValue("IntArray", new String[] {"4", "5", "2", "3"});
			Assert.IsTrue(pt.intArray.Length == 4, "intArray length = 4");
			Assert.IsTrue(pt.intArray[0] == 4 && pt.intArray[1] == 5 && pt.intArray[2] == 2 && pt.intArray[3] == 3,
			              "correct values");
			wrapper.SetPropertyValue("IntArray", 1);
			Assert.IsTrue(pt.intArray.Length == 1, "intArray length = 1");
			Assert.IsTrue(pt.intArray[0] == 1, "correct values");
			wrapper.SetPropertyValue("IntArray", new String[] {"1"});
			Assert.IsTrue(pt.intArray.Length == 1, "intArray length = 1");
			Assert.IsTrue(pt.intArray[0] == 1, "correct values");
		}

		[Test]
		public void NewWrappedInstancePropertyValuesGet()
		{
			ObjectWrapper wrapper = GetWrapper();
			TestObject t = new TestObject("Tony", 50);
			wrapper.WrappedInstance = t;
			Assert.AreEqual(t.Age, wrapper.GetPropertyValue("Age"), "Object wrapper returns wrong property value");

			TestObject u = new TestObject("Udo", 30);
			wrapper.WrappedInstance = u;
			Assert.AreEqual(u.Age, wrapper.GetPropertyValue("Age"), "Object wrapper returns cached property value");
		}

		[Test]
		public void NewWrappedInstanceNestedPropertyValuesGet()
		{
			IObjectWrapper wrapper = GetWrapper();
			TestObject t = new TestObject("Tony", 50);
			t.Spouse = new TestObject("Sue", 40);
			wrapper.WrappedInstance = t;
			Assert.AreEqual(t.Spouse.Age, wrapper.GetPropertyValue("Spouse.Age"),
			                "Object wrapper returns wrong nested property value");

			TestObject u = new TestObject("Udo", 30);
			u.Spouse = new TestObject("Vera", 20);
			wrapper.WrappedInstance = u;
			Assert.AreEqual(u.Spouse.Age, wrapper.GetPropertyValue("Spouse.Age"),
			                "Object wrapper returns cached nested property value");
		}

		[Test]
		public void StringArrayProperty()
		{
			PropsTest pt = new PropsTest();
			ObjectWrapper wrapper = GetWrapper(pt);

			wrapper.SetPropertyValue("StringArray", "foo,fi,fi,fum");
			Assert.IsTrue(pt.stringArray.Length == 4, "StringArray length = 4");
			Assert.IsTrue(
				pt.stringArray[0].Equals("foo") && pt.stringArray[1].Equals("fi") && pt.stringArray[2].Equals("fi") &&
					pt.stringArray[3].Equals("fum"), "in correct values of string array");
		}

		#region Test for DateTime Properties

		internal class DateTimeTestObject
		{
			private DateTime _dt;

			public DateTime TriggerDateTime
			{
				get { return _dt; }
				set { _dt = value; }
			}
		}

		[Test]
		public void SetDateTimeProperty()
		{
			DateTimeTestObject o = new DateTimeTestObject();
			ObjectWrapper wrapper = GetWrapper(o);

			wrapper.SetPropertyValue("TriggerDateTime", "1991-10-10");

			Assert.AreEqual(1991, ((DateTime) wrapper.GetPropertyValue("TriggerDateTime")).Year);
		}

		#endregion

		#region Test for CultureInfo Properties

		internal class CultureTestObject
		{
			private CultureInfo _ci;

			public CultureInfo Cult
			{
				get { return _ci; }
				set { _ci = value; }
			}
		}

		[Test]
		public void SetCultureInfoProperty()
		{
			CultureTestObject o = new CultureTestObject();
			ObjectWrapper wrapper = GetWrapper(o);

			wrapper.SetPropertyValue("Cult", "es-ES");

			Assert.AreEqual("es-ES",
			                ((CultureInfo) wrapper.GetPropertyValue("Cult")).Name);
		}

		#endregion

		#region Tests for URI Properties

		internal class URITestObject
		{
			private Uri _uri;

			public Uri ResourceIdentifier
			{
				get { return _uri; }
				set { _uri = value; }
			}
		}

		[Test]
		public void SetURIProperty()
		{
			URITestObject o = new URITestObject();
			ObjectWrapper wrapper = GetWrapper(o);

			wrapper.SetPropertyValue("ResourceIdentifier", "http://www.springframework.net");
			Assert.AreEqual("www.springframework.net",
			                ((Uri) wrapper.GetPropertyValue("ResourceIdentifier")).Host);
		}

		#endregion

		#region Tests for Indexed Array Properties

		[Test]
		public void GetIndexedFromArrayProperty()
		{
			PrimitiveArrayObject to = new PrimitiveArrayObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Array = new int[] {1, 2, 3, 4, 5};
			Assert.AreEqual(1, (int) wrapper.GetPropertyValue("Array[0]"));
		}

		[Test]
		public void GetIndexOutofRangeFromArrayProperty()
		{
			PrimitiveArrayObject to = new PrimitiveArrayObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Array = new int[] {1, 2, 3, 4, 5};
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("Array[5]"));
		}

		[Test]
		public void SetIndexedFromArrayProperty()
		{
			PrimitiveArrayObject to = new PrimitiveArrayObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Array = new int[] {1, 2, 3, 4, 5};
			wrapper.SetPropertyValue("Array[2]", 6);
			Assert.AreEqual(6, to.Array[2]);
		}

		[Test]
		public void SetIndexOutOfRangeFromArrayProperty()
		{
			PrimitiveArrayObject to = new PrimitiveArrayObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Array = new int[] {1, 2, 3, 4, 5};
            Assert.Throws<InvalidPropertyException>(() => wrapper.SetPropertyValue("Array[5]", 6));
		}

		/// <summary>
		/// Test that we bail when attempting to get an indexed property with some guff for the index
		/// </summary>
		[Test]
		public void GetIndexedPropertyValueWithGuffIndexFromArrayProperty()
		{
			PrimitiveArrayObject to = new PrimitiveArrayObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Array = new int[] {1, 2, 3, 4, 5};
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("Array[HungerHurtsButStarvingWorks]"));
		}

		/// <summary>
		/// Test that we bail when attempting to get an indexed property with some guff for the index
		/// </summary>
		[Test]
		public void GetIndexedPropertyValueWithMissingIndexFromArrayProperty()
		{
			PrimitiveArrayObject to = new PrimitiveArrayObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Array = new int[] {1, 2, 3, 4, 5};
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("Array[]"));
		}

		#endregion

		#region Tests for Indexed List Properties

		internal class ListTestObject
		{
			private IList _list;
			private ArrayList _arrayList;

			public IList List
			{
				get { return _list; }
				set { _list = value; }
			}

			public ArrayList ArrayList
			{
				get { return _arrayList; }
				set { _arrayList = value; }
			}
		}

		[Test]
		public void GetIndexedFromListProperty()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
			Assert.AreEqual(1, (int) wrapper.GetPropertyValue("List[0]"));
		}

		[Test]
		public void GetIndexedFromArrayListProperty()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.ArrayList = new ArrayList(new int[] {1, 2, 3, 4, 5});
			Assert.AreEqual(1, (int) wrapper.GetPropertyValue("ArrayList[0]"));
		}

		[Test]
		public void GetIndexOutofRangeFromListProperty()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("List[5]"));
		}

		[Test]
		public void SetIndexedFromListProperty()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
			wrapper.SetPropertyValue("List[0]", 6);
			Assert.AreEqual(6, to.List[0]);
		}

		[Test]
		public void SetIndexedFromListPropertyUsingMixOfSingleAndDoubleQuotedDelimeters()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.SetPropertyValue("List['0\"]", 6));
		}

		[Test]
		public void SetIndexedFromListPropertyUsingNonNumericValueForTheIndex()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.SetPropertyValue("List[bingo]", 6));
		}

		[Test]
		public void SetIndexedFromListPropertyUsingEmptyValueForTheIndex()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.SetPropertyValue("List[]", 6));
		}

		[Test]
		[Ignore("Addition of elements to the list via index that is out of range is not supported anymore.")]
		public void SetIndexOutOfRangeFromListProperty()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
			wrapper.SetPropertyValue("List[6]", 6);
			Assert.AreEqual(6, to.List[6]);
			Assert.IsNull(to.List[5]);
			wrapper.SetPropertyValue("List[7]", 7);
			Assert.AreEqual(7, to.List[7]);
		}

		/// <summary>
		/// Test that we bail when attempting to get an indexed property with some guff for the index
		/// </summary>
		[Test]
		public void GetIndexedPropertyValueWithGuffIndexFromListProperty()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("List[HungerHurtsButStarvingWorks]"));
		}

		[Test]
		public void GetIndexedPropertyValueWithMissingIndexFromListProperty()
		{
			ListTestObject to = new ListTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.List = new ArrayList(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("List[]"));
		}

		#endregion

		#region Tests for Indexed Dictionary Properties

		internal class DictionaryTestObject
		{
			private IDictionary _dictionary;

			public IDictionary Dictionary
			{
				get { return _dictionary; }
				set { _dictionary = value; }
			}
		}

		[Test]
		public void GetIndexedFromDictionaryProperty()
		{
			DictionaryTestObject to = new DictionaryTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Dictionary = new Hashtable();
			to.Dictionary.Add("key1", "value1");
			Assert.AreEqual("value1", (string) wrapper.GetPropertyValue("Dictionary['key1']"));
		}

		[Test]
		public void GetIndexMissingFromDictionaryProperty()
		{
			DictionaryTestObject to = new DictionaryTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Dictionary = new Hashtable();
			Assert.IsNull(wrapper.GetPropertyValue("Dictionary['notthere']"));
		}

		[Test]
		public void SetIndexedFromDictionaryProperty()
		{
			DictionaryTestObject to = new DictionaryTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Dictionary = new Hashtable();
			wrapper.SetPropertyValue("Dictionary['key1']", "value1");
			Assert.AreEqual("value1", to.Dictionary["key1"]);
		}

		[Test]
		public void SettingADictionaryPropertyJustAddsTheValuesToTheExistingDictionary()
		{
			TestObject to = new TestObject();
			to.AddPeriodicElement("Hsu", "Feng");
			to.AddPeriodicElement("Piao", "Jin");
			ObjectWrapper wrapper = GetWrapper(to);
			IDictionary elements = new Hashtable();
			elements.Add("Weekend", "News");
			wrapper.SetPropertyValue("PeriodictabLE", elements);
			Assert.AreEqual(3, to.PeriodicTable.Count);
		}

		#endregion

		#region Tests for Indexed Set Properties

		internal class SetTestObject
		{
			private ISet _set;
			private HybridSet _aSet;

			public ISet Set
			{
				get { return _set; }
				set { _set = value; }
			}

			public HybridSet HybridSet
			{
				get { return _aSet; }
				set { _aSet = value; }
			}
		}

		[Test]
		public void SettingASetPropertyJustAddsTheValuesToTheExistingSet()
		{
			TestObject to = new TestObject();
			to.AddComputerName("Atari 2900");
			to.AddComputerName("JCN");
			ObjectWrapper wrapper = GetWrapper(to);
			wrapper.SetPropertyValue("Computers", new HybridSet(new string[] {"Trusty SNES"}));
			Assert.AreEqual(3, to.Computers.Count);
		}

        [Test]
        public void GetIndexFromSetProperty()
        {
            SetTestObject to = new SetTestObject();
            IObjectWrapper wrapper = GetWrapper(to);
            to.Set = new ListSet(new int[] { 1, 2, 3, 4, 5 });
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("Set[1]"));
        }
	    
		[Test]
		public void GetIndexOutofRangeFromSetProperty()
		{
			SetTestObject to = new SetTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Set = new ListSet(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("Set[23]"));
		}

		/// <summary>
		/// Test that we bail when attempting to get an indexed property with some guff for the index
		/// </summary>
		[Test]
		public void GetIndexedPropertyValueWithGuffIndexFromSetProperty()
		{
			SetTestObject to = new SetTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Set = new ListSet(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("Set[HungerHurtsButStarvingWorks]"));
		}

		[Test]
		public void GetIndexedPropertyValueWithMissingIndexFromSetProperty()
		{
			SetTestObject to = new SetTestObject();
			IObjectWrapper wrapper = GetWrapper(to);
			to.Set = new ListSet(new int[] {1, 2, 3, 4, 5});
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("Set[]"));
		}

		#endregion

		#region Test for Enumeration Properties

		internal class EnumTestObject
		{
			private FileMode FileModeEnum;

			public FileMode FileMode
			{
				get { return FileModeEnum; }
				set { FileModeEnum = value; }
			}
		}

		[Test]
		public void SetEnumProperty()
		{
			EnumTestObject o = new EnumTestObject();
			ObjectWrapper wrapper = GetWrapper(o);
			wrapper.SetPropertyValue("FileMode", FileMode.Create);
			Assert.AreEqual(FileMode.Create, (FileMode) wrapper.GetPropertyValue("FileMode"));
		}

		#endregion

		[Test]
		public void SetTypePropertyWithString()
		{
			ObjectWithTypeProperty to = new ObjectWithTypeProperty();
			IObjectWrapper wrapper = GetWrapper(to);
			wrapper.SetPropertyValue("Type", "System.DateTime");
			Assert.AreEqual(typeof (DateTime), to.Type);
		}

		[Test]
		public void GetIndexedPropertyValueWithNonIndexedType()
		{
			TestObject to = new TestObject();
			IObjectWrapper wrapper = GetWrapper(to);
            Assert.Throws<InvalidPropertyException>(() => wrapper.GetPropertyValue("FileMode[0]"));
		}

		[Test]
		public void SetPropertyValuesWithUnknownProperty()
		{
			TestObject to = new TestObject();
			to.Doctor = null;
			ObjectWrapper wrapper = GetWrapper(to);
            Assert.Throws<NullValueInNestedPathException>(() => wrapper.SetPropertyValue("Doctor.Company", "Bingo"));
		}

        [Test]
        public void SetPropertyValuesFailsWhenSettingNonExistantProperty()
        {
            TestObject to = new TestObject();
            ObjectWrapper wrapper = GetWrapper(to);
            MutablePropertyValues values = new MutablePropertyValues();
            values.Add("JeepersCreepersWhereDidYaGetThosePeepers", "OhThisWeirdBatGuySoldEmToMe...");
            values.Add("Age", 19);
            // the unknown and ridiculously named property should fail
            Assert.Throws<InvalidPropertyException>(() => wrapper.SetPropertyValues(values, false));
        }
        
        [Test]
        public void SetPropertyValuesDoesNotFailWhenSettingNonExistantPropertyWithIgnorUnknownSetToTrue()
        {
			TestObject to = new TestObject();
			ObjectWrapper wrapper = GetWrapper(to);
			MutablePropertyValues values = new MutablePropertyValues();
			values.Add("Age", 19);
			values.Add("JeepersCreepersWhereDidYaGetThosePeepers", "OhThisWeirdBatGuySoldEmToMe...");
			// the unknown and ridiculously named property should fail
			wrapper.SetPropertyValues(values, true);
			Assert.AreEqual(19, to.Age, "The single good property in the property values should have been set though...");
		}

        [Test]
        public void SetPropertyValuesFailsWhenSettingReadOnlyProperty()
        {
            TestObject to = new TestObject();
            ObjectWrapper wrapper = GetWrapper(to);
            MutablePropertyValues values = new MutablePropertyValues();
            values.Add("ReadOnlyObjectNumber", 123);
            Assert.Throws<NotWritablePropertyException>(() => wrapper.SetPropertyValues(values, false));
        }

        [Test]
        public void SetPropertyValuesDoesNotFailWhenSettingReadOnlyPropertyWithIgnorUnknownSetToTrue()
        {
            TestObject to = new TestObject();
            ObjectWrapper wrapper = GetWrapper(to);
            MutablePropertyValues values = new MutablePropertyValues();
            values.Add("ObjectNumber", 123);
            values.Add("Age", 19);
            wrapper.SetPropertyValues(values, true);
            Assert.AreEqual(19, to.Age, "The single good property in the property values should have been set though...");
        }

		[Test]
		public void SetArrayPropertyValue()
		{
			string[] expected = new string[] {"Fedora", "Gacy", "Banjo"};
			TestObject to = new TestObject();
			ObjectWrapper wrapper = GetWrapper(to);
			wrapper.SetPropertyValue("Hats", expected);
			Assert.IsNotNull(to.Hats);
			Assert.AreEqual(expected.Length, to.Hats.Length);
			for (int i = 0; i < expected.Length; ++i)
			{
				Assert.AreEqual(expected[i], to.Hats[i]);
			}
		}

		[Test]
		public void TestToString()
		{
			ObjectWrapper wrapper = GetWrapper();
			Assert.IsTrue(wrapper.ToString().IndexOf("Exception encountered") >= 0);
			wrapper.WrappedInstance = new WanPropsClass();
			string expected = string.Format(
				"{0}: wrapping class [{1}]; IsWan={2}",
				wrapper.GetType().Name, wrapper.WrappedType.FullName, "{True}");
			Assert.AreEqual(expected, wrapper.ToString());
		}

		[Test]
		public void GetPropertyInfoWithNullArgument()
		{
			ObjectWrapper wrapper = GetWrapper(new TestObject());
            Assert.Throws<FatalObjectException>(() => wrapper.GetPropertyInfo(null));
		}

        [Test]
        public void GetPropertyInfoWithNonPropertyExpression()
        {
            ObjectWrapper wrapper = GetWrapper(new TestObject());
            Assert.Throws<FatalReflectionException>(() => wrapper.GetPropertyInfo("2 + 2"));
        }

        [Test]
        public void GetPropertyInfoWithNonParsableExpression()
        {
            ObjectWrapper wrapper = GetWrapper(new TestObject());
            Assert.Throws<FatalObjectException>(() => wrapper.GetPropertyInfo("["));
        }

		[Test]
		public void GetNestedPropertyInfo()
		{
			RealNestedTestObject o = new RealNestedTestObject();
			o.Datum = new TestObject();
			o.Datum.Doctor = new NestedTestObject("Modlin");
			ObjectWrapper wrapper = GetWrapper(o);
			PropertyInfo info = wrapper.GetPropertyInfo("Datum.Doctor.Company");
			Assert.IsNotNull(info);
		}

		[Test(Description="SPRNET-198")]
		public void AmbiguousPropertyLookupIsHandledProperlyByLookingAtDerivedClassOnly()
		{
			ObjectWrapper wrapper = GetWrapper(new DerivedFoo());
			wrapper.GetPropertyInfo("Bar");
		}
	}
}