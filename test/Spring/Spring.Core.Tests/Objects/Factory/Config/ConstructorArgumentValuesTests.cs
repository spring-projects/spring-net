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
using Spring.Collections;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the ConstructorArgumentValues class.
	/// </summary>
	/// <author>Rick Evans (.NET)</author>
	[TestFixture]
	public sealed class ConstructorArgumentValuesTests
	{
		[Test]
		public void Instantiation()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			Assert.IsNotNull(values.GenericArgumentValues, "The 'GenericArgumentValues' property was not initialised.");
			Assert.IsNotNull(values.IndexedArgumentValues, "The 'IndexedArgumentValues' property was not initialised.");
			Assert.IsNotNull(values.NamedArgumentValues, "The 'NamedArgumentValues' property was not initialised.");
			Assert.AreEqual(0, values.ArgumentCount, "There were some arguments in a newly initialised instance.");
			Assert.IsTrue(values.Empty, "A newly initialised instance was not initially empty.");
		}

		[Test]
		public void GetGenericArgumentValueIgnoresAlreadyUsedValues()
		{
			ISet used = new ListSet();

			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddGenericArgumentValue(1);
			values.AddGenericArgumentValue(2);
			values.AddGenericArgumentValue(3);

			Type intType = typeof (int);
			ConstructorArgumentValues.ValueHolder one = values.GetGenericArgumentValue(intType, used);
			Assert.AreEqual(1, one.Value);
			used.Add(one);
			ConstructorArgumentValues.ValueHolder two = values.GetGenericArgumentValue(intType, used);
			Assert.AreEqual(2, two.Value);
			used.Add(two);
			ConstructorArgumentValues.ValueHolder three = values.GetGenericArgumentValue(intType, used);
			Assert.AreEqual(3, three.Value);
			used.Add(three);
			ConstructorArgumentValues.ValueHolder four = values.GetGenericArgumentValue(intType, used);
			Assert.IsNull(four);
		}

		[Test]
		public void GetGeneric_Untyped_ArgumentValue()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			const string expectedValue = "Rick";
			values.AddGenericArgumentValue(expectedValue);

			ConstructorArgumentValues.ValueHolder name = values.GetGenericArgumentValue(null, null);
			Assert.IsNotNull(name,
				"Must get non-null valueholder back if no required type is specified.");
			Assert.AreEqual(expectedValue, name.Value);
		}

		[Test]
		public void GetGeneric_Untyped_ArgumentValueWithOnlyStronglyTypedValuesInTheCtorValueList()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			const string expectedValue = "Rick";
			values.AddGenericArgumentValue(expectedValue, typeof(string).FullName);

			ConstructorArgumentValues.ValueHolder name = values.GetGenericArgumentValue(null, null);
			Assert.IsNull(name,
				"Must get null valueholder back if no required type is specified but only " +
				"strongly typed values are present in the ctor values list.");
		}

		[Test]
		public void GetArgumentValueIgnoresAlreadyUsedValues()
		{
			ISet used = new ListSet();

			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddGenericArgumentValue(1);
			values.AddNamedArgumentValue("2", 2);
			values.AddIndexedArgumentValue(3, 3);

			Type intType = typeof (int);
			ConstructorArgumentValues.ValueHolder one = values.GetArgumentValue(10, string.Empty, intType, used);
			Assert.AreEqual(1, one.Value);
			used.Add(one);
			ConstructorArgumentValues.ValueHolder two = values.GetArgumentValue(10, "2", intType, used);
			Assert.AreEqual(2, two.Value);
			used.Add(two);
			ConstructorArgumentValues.ValueHolder three = values.GetArgumentValue(3, string.Empty, intType, used);
			Assert.AreEqual(3, three.Value);
			used.Add(three);
			ConstructorArgumentValues.ValueHolder four = values.GetArgumentValue(10, string.Empty, intType, used);
			Assert.IsNull(four);
		}

		[Test]
		public void AddNamedArgumentWithNullName()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
            Assert.Throws<ArgumentNullException>(() => values.AddNamedArgumentValue(null, 1));
		}

		[Test]
		public void AddNamedArgumentWithEmptyStringName()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
            Assert.Throws<ArgumentNullException>(() => values.AddNamedArgumentValue(string.Empty, 1));
		}

		[Test]
		public void AddNamedArgumentWithWhitespaceStringName()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
            Assert.Throws<ArgumentNullException>(() => values.AddNamedArgumentValue(Environment.NewLine + "  ", 1));
		}

		[Test]
		public void AddIndexedArgumentValue()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddIndexedArgumentValue(1, DBNull.Value);
			Assert.IsFalse(values.Empty, "Added one value, but the collection is sayin' it's empty.");
			Assert.AreEqual(1, values.ArgumentCount, "Added one value, but the collection ain't sayin' that it's got a single element in it.");
			Assert.AreEqual(1, values.IndexedArgumentValues.Count, "Added one indexed value, but the collection of indexed values ain't sayin' that it's got a single element in it.");
		}

		[Test]
		public void AddGenericArgumentValue()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddGenericArgumentValue(DBNull.Value);
			Assert.IsFalse(values.Empty, "Added one value, but the collection is sayin' it's empty.");
			Assert.AreEqual(1, values.ArgumentCount, "Added one value, but the collection ain't sayin' that it's got a single element in it.");
			Assert.AreEqual(1, values.GenericArgumentValues.Count, "Added one generic value, but the collection of indexed values ain't sayin' that it's got a single element in it.");
		}

		[Test]
		public void GetIndexedArgumentValue()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			Assert.IsNull(values.GetIndexedArgumentValue(0, typeof (object)), "Mmm... managed to get a non null instance back from an empty instance.");
			values.AddIndexedArgumentValue(16, DBNull.Value, typeof (DBNull).FullName);
			Assert.IsNull(values.GetIndexedArgumentValue(0, typeof (object)), "Mmm... managed to get a non null instance back from an instance that should have now't at the specified index.");
			ConstructorArgumentValues.ValueHolder value =
				values.GetIndexedArgumentValue(16, typeof (DBNull));
			Assert.IsNotNull(value, "Stored a value at a specified index, but got null when retrieving it.");
			Assert.AreSame(DBNull.Value, value.Value, "The value stored at the specified index was not the exact same instance as was added.");
			ConstructorArgumentValues.ValueHolder wrongValue =
				values.GetIndexedArgumentValue(16, typeof (string));
			Assert.IsNull(wrongValue, "Stored a value at a specified index, and got it (or rather something) back when retrieving it with the wrong Type specified.");
		}

		[Test]
		public void GetGenericArgumentValue()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			Assert.IsNull(values.GetGenericArgumentValue(typeof (object)), "Mmm... managed to get a non null instance back from an empty instance.");
			values.AddGenericArgumentValue(DBNull.Value, typeof (DBNull).FullName);
			Assert.IsNull(values.GetGenericArgumentValue(typeof (string)), "Mmm... managed to get a non null instance back from an instance that should have now't with the specified Type.");
			ConstructorArgumentValues.ValueHolder value =
				values.GetGenericArgumentValue(typeof (DBNull));
			Assert.IsNotNull(value, "Stored a value of a specified Type, but got null when retrieving it using said Type.");
			Assert.AreSame(DBNull.Value, value.Value, "The value stored at the specified index was not the exact same instance as was added.");
		}

		[Test]
		public void GetArgumentValue()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			Assert.IsNull(values.GetArgumentValue(0, typeof (object)), "Mmm... managed to get a non null instance back from an empty instance.");
			values.AddGenericArgumentValue(DBNull.Value, typeof (DBNull).FullName);
			values.AddNamedArgumentValue("foo", DBNull.Value);
			values.AddIndexedArgumentValue(16, DBNull.Value, typeof (DBNull).FullName);
			Assert.IsNull(values.GetArgumentValue(100, typeof (string)), "Mmm... managed to get a non null instance back from an instance that should have now't with the specified Type.");
			ConstructorArgumentValues.ValueHolder value =
				values.GetArgumentValue(-3, typeof (DBNull));
			Assert.IsNotNull(value, "Stored a value of a specified Type at a specified index, but got null when retrieving it using the wrong index but the correct Type.");
			Assert.AreSame(DBNull.Value, value.Value, "The retrieved value was not the exact same instance as was added.");
			
			value = values.GetArgumentValue("foo", typeof (DBNull));
			Assert.IsNotNull(value, "Stored a value of a specified Type under a name, but got null when retrieving it using the wrong name but the correct Type.");
			Assert.AreSame(DBNull.Value, value.Value, "The retrieved value was not the exact same instance as was added.");
		}

		[Test]
		public void AddAllDoesntChokeOnNullArgument()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddAll(null);
		}

		[Test]
		public void AddAllFromOther()
		{
			ConstructorArgumentValues other = new ConstructorArgumentValues();
			other.AddIndexedArgumentValue(1, DBNull.Value);
			other.AddIndexedArgumentValue(2, "Foo");
			other.AddIndexedArgumentValue(3, 3);

			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddAll(other);
			Assert.AreEqual(other.ArgumentCount, values.ArgumentCount,
				"Must have been the same since one was filled up with the values in the other.");
		}

		[Test]
		public void AddRangeOfIndexedArgumentValues()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddIndexedArgumentValue(1, DBNull.Value);
			values.AddIndexedArgumentValue(2, "Foo");
			values.AddIndexedArgumentValue(3, 3);
			new ConstructorArgumentValues(values);
			Assert.IsFalse(values.Empty, "Added three indexed values(as a range), but the collection is sayin' it's empty.");
			Assert.AreEqual(3, values.ArgumentCount, "Added three indexed values(as a range), but the collection ain't sayin' that it's got 3 elements in it.");
			Assert.AreEqual(3, values.IndexedArgumentValues.Count, "Added three indexed values(as a range), but the collection of indexed values ain't sayin' that it's got 3 elements in it.");
		}

		[Test]
		public void NamedArgumentsAreCaseInsensitive()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddNamedArgumentValue("foo", "sball");
			Assert.AreEqual(1, values.NamedArgumentValues.Count, "Added one named argument but it doesn't seem to have been added to the named arguments collection.");
			Assert.AreEqual(1, values.ArgumentCount, "Added one named argument but it doesn't seem to be reflected in the overall argument count.");
			Assert.IsTrue(values.ContainsNamedArgument("FOo"), "Mmm, the ContainsNamedArgument() method eveidently IS case sensitive (which is wrong).");
			ConstructorArgumentValues.ValueHolder arg = values.GetNamedArgumentValue("fOo");
			Assert.IsNotNull(arg, "The named argument previously added could not be pulled from the ctor arg collection.");
			Assert.AreEqual("sball", arg.Value, "The value of the named argument passed in is not the same as the one that was pulled out.");
		}

		[Test]
		public void AddNamedArgument()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddNamedArgumentValue("foo", "sball");
			Assert.AreEqual(1, values.NamedArgumentValues.Count, "Added one named argument but it doesn't seem to have been added to the named arguments collection.");
			Assert.AreEqual(1, values.ArgumentCount, "Added one named argument but it doesn't seem to be reflected in the overall argument count.");
			ConstructorArgumentValues.ValueHolder arg = values.GetNamedArgumentValue("foo");
			Assert.IsNotNull(arg, "The named argument previously added could not be pulled from the ctor arg collection.");
			Assert.AreEqual("sball", arg.Value, "The value of the named argument passed in is not the same as the one that was pulled out.");
		}

		[Test]
		public void AddNamedArgumentFromAotherCtorArgCollection()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddNamedArgumentValue("foo", "sball");
			ConstructorArgumentValues copy = new ConstructorArgumentValues(values);
			Assert.AreEqual(1, copy.NamedArgumentValues.Count, "Added one named argument but it doesn't seem to have been added to the named arguments collection.");
			Assert.AreEqual(1, copy.ArgumentCount, "Added one named argument but it doesn't seem to be reflected in the overall argument count.");
			ConstructorArgumentValues.ValueHolder arg = copy.GetNamedArgumentValue("foo");
			Assert.IsNotNull(arg, "The named argument previously added could not be pulled from the ctor arg collection.");
			Assert.AreEqual("sball", arg.Value, "The value of the named argument passed in is not the same as the one that was pulled out.");
		}

		[Test]
		public void ValueHolderToStringsNicely()
		{
			ConstructorArgumentValues values = new ConstructorArgumentValues();
			values.AddGenericArgumentValue(1, typeof(int).FullName);
			ConstructorArgumentValues.ValueHolder vh = values.GetGenericArgumentValue(typeof(int));
			Assert.AreEqual("'1' [System.Int32]", vh.ToString());
		}
	}
}