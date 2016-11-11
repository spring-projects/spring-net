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
using System.Reflection;
using NUnit.Framework;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Unit tests for the AutowireUtilsTests class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class AutowireUtilsTests
	{
		#region GetTypeDifferenceWeight Tests

		[Test]
		public void GetTypeDifferenceWeightForZeroArgCtor()
		{
			int expectedWeight = 0;
			int actualWeight = AutowireUtils.GetTypeDifferenceWeight(
                ReflectionUtils.GetParameterTypes(typeof(Fable).GetConstructor(Type.EmptyTypes).GetParameters()), new object[] { });
			Assert.AreEqual(expectedWeight, actualWeight, "AutowireUtils.GetTypeDifferenceWeight() was wrong, evidently.");
		}

		[Test]
		public void GetTypeDifferenceWeightWhenPassingDerivedTypeArgsToBaseTypeCtor()
		{
			int expectedWeight = 2;
			int actualWeight = AutowireUtils.GetTypeDifferenceWeight(
                ReflectionUtils.GetParameterTypes(typeof(Fable).GetConstructor(
					new Type[] {typeof (NurseryRhymeCharacter)}).GetParameters()),
				new object[] {new EnglishCharacter()});
			Assert.AreEqual(expectedWeight, actualWeight, "AutowireUtils.GetTypeDifferenceWeight() was wrong, evidently.");
		}

		[Test]
		public void GetTypeDifferenceWeightWhenPassingExactTypeMatch()
		{
			int expectedWeight = 0;
			int actualWeight = AutowireUtils.GetTypeDifferenceWeight(
                ReflectionUtils.GetParameterTypes(typeof(Fable).GetConstructor(
					new Type[] {typeof (EnglishCharacter)}).GetParameters()),
				new object[] {new EnglishCharacter()});
			Assert.AreEqual(expectedWeight, actualWeight, "AutowireUtils.GetTypeDifferenceWeight() was wrong, evidently.");
		}

		/// <summary>
		/// We want to use the ctor with the most specific argument type... so in the case of the test
		/// classes used in this test, when we create a new instance of the Fable class using a
		/// PeterPumpkinEater instance, we want the Fable(PeterPumpkinEater) ctor to be picked in
		/// preference to any of the (still valid) ctors that accept Types that the PeterPumpkinEater
		/// class inherits from.
		/// </summary>
		[Test]
		public void UsingTypeDifferenceWeightPicksMostSpecificCtor()
		{
			object[] arguments = new object[] {new EnglishCharacter()};
			ConstructorInfo expectedCtor
				= typeof (Fable).GetConstructor(new Type[] {typeof (EnglishCharacter)});
			ConstructorInfo pickedCtor = AssertTypeWeightingPicksCorrectCtor(arguments, typeof (Fable));
			Assert.IsNotNull(pickedCtor);
			Assert.AreEqual(expectedCtor, pickedCtor);
		}

		[Test]
		public void UsingTypeDifferenceWeightPicksNothingWhenGivenAnIncompatibleArgument()
		{
			object[] arguments = new object[] {"This argument is incompatible with any of the Fable ctors."};
			ConstructorInfo pickedCtor = AssertTypeWeightingPicksCorrectCtor(arguments, typeof (Fable));
			Assert.IsNull(pickedCtor);
		}

		[Test]
		public void UsingTypeDifferenceWeightPicksMostGenericCtorWhenGivenA_NULL_Argument()
		{
			// using null means that we can match anything that takes an object as an argument...
			object[] arguments = new object[] {null};
			// we'll want to pick the 'least' specific (most generic) ctor that we can find...
			ConstructorInfo expectedCtor
				= typeof (Fable).GetConstructor(new Type[] {typeof (FableCharacter)});
			ConstructorInfo pickedCtor = AssertTypeWeightingPicksCorrectCtor(arguments, typeof (Fable));
			Assert.IsNotNull(pickedCtor);
			Assert.AreEqual(expectedCtor, pickedCtor);
		}

		private static ConstructorInfo AssertTypeWeightingPicksCorrectCtor(object[] arguments, Type ctorType)
		{
			ConstructorInfo pickedCtor = null;
			// we want to pick the ctor that has the lowest type difference weight...
			ConstructorInfo[] ctors = ctorType.GetConstructors();
			int weighting = Int32.MaxValue;
			foreach (ConstructorInfo ctor in ctors)
			{
				ParameterInfo[] parameters = ctor.GetParameters();
				if (parameters.Length == arguments.Length)
				{
				    Type[] paramTypes = ReflectionUtils.GetParameterTypes(parameters);
                    int weight = AutowireUtils.GetTypeDifferenceWeight(paramTypes, arguments);
					if (weight < weighting)
					{
						pickedCtor = ctor;
						weighting = weight;
					}
				}
			}
			return pickedCtor;
		}

		[Test]
        [Ignore("Investigate details of new type weight algorithm")]
		public void GetTypeDifferenceWeightWithMismatchedLengths()
		{
            Assert.Throws<ArgumentException>(() => AutowireUtils.GetTypeDifferenceWeight(new Type[] {}, new object[] {1}), "Cannot calculate the type difference weight for argument types and arguments with differing lengths.");
		}

		[Test]
		public void GetTypeDifferenceWeightWithNullParameterTypes()
		{
            Assert.Throws<NullReferenceException>(() => AutowireUtils.GetTypeDifferenceWeight(null, new object[] {}));
		}

		[Test]
		public void GetTypeDifferenceWeightWithNullArgumentTypes()
		{
            Assert.Throws<NullReferenceException>(() => AutowireUtils.GetTypeDifferenceWeight(ReflectionUtils.GetParameterTypes(typeof(Fable).GetConstructor(new Type[] { typeof(FableCharacter) }).GetParameters()), null));
		}

		[Test]
		public void GetTypeDifferenceWeightWithAllNulls()
		{
            Assert.Throws<NullReferenceException>(() => AutowireUtils.GetTypeDifferenceWeight(null, null));
		}

		[Test]
		public void GetTypeDifferenceWeightWithSameTypes()
		{
			int expectedWeight = 0;
			int actualWeight = AutowireUtils.GetTypeDifferenceWeight(
                ReflectionUtils.GetParameterTypes(typeof(Fool).GetConstructor(new Type[] { typeof(string) }).GetParameters()),
				new object[] {"Noob"});
			Assert.AreEqual(expectedWeight, actualWeight, "AutowireUtils.GetTypeDifferenceWeight() was wrong, evidently.");
		}

		#endregion

		#region SortConstructors Tests

		[Test]
		public void SortCtorsReallyIsGreedy()
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			Type foolType = typeof (Fool);
			ConstructorInfo[] ctors = foolType.GetConstructors(flags);
			AutowireUtils.SortConstructors(ctors);
			ConstructorInfo[] expected = new ConstructorInfo[]
				{
					foolType.GetConstructor(new Type[] {typeof (string), typeof (int), typeof (Double), typeof (Fool)}),
					foolType.GetConstructor(new Type[] {typeof (string), typeof (int)}),
					foolType.GetConstructor(new Type[] {typeof (string)}),
					foolType.GetConstructor(Type.EmptyTypes),
				};
			Assert.IsTrue(ArrayUtils.AreEqual(expected, ctors), "AutowireUtils.SortConstructors() did not put the greedy public ctors first.");
		}

		[Test]
		public void SortCtorsReallyDoesFavourPublicOnes()
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			Type foolType = typeof (Fool);
			ConstructorInfo[] ctors = foolType.GetConstructors(flags);
			AutowireUtils.SortConstructors(ctors);
			ConstructorInfo[] expected = new ConstructorInfo[]
				{
					foolType.GetConstructor(new Type[] {typeof (string), typeof (int), typeof (Double), typeof (Fool)}),
					foolType.GetConstructor(new Type[] {typeof (string), typeof (int)}),
					foolType.GetConstructor(new Type[] {typeof (string)}),
					foolType.GetConstructor(Type.EmptyTypes),
					foolType.GetConstructor(flags, null, new Type[] {typeof (string), typeof (int), typeof (Double)}, null),
					foolType.GetConstructor(flags, null, new Type[] {typeof (string), typeof (Fool)}, null),
				};
			Assert.IsTrue(ArrayUtils.AreEqual(expected, ctors), "AutowireUtils.SortConstructors() did not put the greedy public ctors first.");
		}

		[Test]
		public void SortCtorsBailsSilentlyWhenGivenNull()
		{
			AutowireUtils.SortConstructors(null);
		}

		#endregion

		#region Inner Class used to test ctor sorting

		private class Fool
		{
			private string _name;
			private int _age;
			private Double _money;
			private Fool _mate;

			public Fool()
			{
			}

			public Fool(string name) : this(name, 0)
			{
			}

			public Fool(string name, int age) : this(name, age, 10.9D)
			{
			}

			protected Fool(string name, int age, Double money)
			{
				_name = name;
				_age = age;
				_money = money;
			}

			public Fool(string name, int age, Double money, Fool mate) : this(name, age, money)
			{
				_mate = mate;
			}

			private Fool(string name, Fool mate) : this(name, 0, 10.9D, mate)
			{
			}
		}

		#endregion

		#region Inner Classes (deep inheritance chain) used to test type weighting

		private sealed class Fable
		{
			public Fable()
			{
			}

			public Fable(FableCharacter character)
			{
			}

			public Fable(NurseryRhymeCharacter character)
			{
			}

			public Fable(EnglishCharacter character)
			{
			}
		}

		private class FableCharacter
		{
		}

		private class NurseryRhymeCharacter : FableCharacter
		{
		}

		private sealed class EnglishCharacter : NurseryRhymeCharacter
		{
		}

		#endregion
	}
}