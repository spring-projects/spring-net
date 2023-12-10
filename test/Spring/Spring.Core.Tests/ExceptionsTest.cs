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

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable SYSLIB0050
#pragma warning disable SYSLIB0051

namespace Spring
{
	/// <summary>
	/// Tests the various exception classes.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Shamelessly lifted from the NAnt test suite.
	/// </para>
	/// </remarks>
	public abstract class ExceptionsTest : StandardsComplianceTest
	{
        // sorry folks, but this is really a special case - default ctor policy doesn't apply here
        private bool ExcludeFromConstructorPolicyCheck(Type t)
        {
            //TODO: uncomment when making SyntaxErrorException public:
            if (t.FullName == "Spring.Expressions.SyntaxErrorException") return true;
            return false;
        }

		protected ExceptionsTest()
		{
			CheckedType = typeof (Exception);
		}

		#region Tests

		[Test]
		public void TestStandardsCompliance()
		{
			ProcessAssembly(AssemblyToCheck);
		}

		[Test]
		public void TestThisTest()
		{
			ProcessAssembly(Assembly.GetAssembly(GetType()));
		}

		#endregion

		#region Methods

		protected override void CheckStandardsCompliance(Assembly assembly, Type t)
		{
			// check to see that the exception is correctly named, with "Exception" at the end
			bool nameIsValid = t.Name.EndsWith("Exception");
			Assert.IsTrue(nameIsValid, t.Name + " class name must end with Exception.");
			if (t.IsAbstract)
			{
				return;
			}

            if (!ExcludeFromConstructorPolicyCheck(t))
            {
			    // Does the exception have the 3 standard constructors?
			    // Default constructor
			    CheckPublicConstructor(t, "()");
			    // Constructor with a single string parameter
			    CheckPublicConstructor(t, "(string message)", typeof (String));
			    // Constructor with a string and an inner exception
			    CheckPublicConstructor(t, "(string message, Exception inner)",
			                           typeof (String), typeof (Exception));
            }


            // check to see if the serialization constructor is present
			// if exception is sealed, constructor should be private
			// if exception is not sealed, constructor should be protected
			if (t.IsSealed)
			{
				// check to see if the private serialization constructor is present...
				CheckPrivateConstructor(t, "(SerializationInfo info, StreamingContext context)",
				                        typeof (SerializationInfo),
				                        typeof (StreamingContext));
			}
			else
			{
				// check to see if the protected serialization constructor is present...
				CheckProtectedConstructor(t, "(SerializationInfo info, StreamingContext context)",
				                          typeof (SerializationInfo),
				                          typeof (StreamingContext));
			}
			// check to see if the type is marked as serializable
			Assert.IsTrue(t.IsSerializable, t.Name + " is not serializable, missing [Serializable]?");
			// check to see if there are any public fields. These should be properties instead...
			FieldInfo[] publicFields = t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
			if (publicFields.Length != 0)
			{
				foreach (FieldInfo fieldInfo in publicFields)
				{
					Assert.Fail(t.Name + "." + fieldInfo.Name + " is a public field, should be exposed through property instead.");
				}
			}
			// If this exception has any fields, check to make sure it has a 
			// version of GetObjectData. If not, it does't serialize those fields.
			FieldInfo[] fields =
				t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (fields.Length != 0)
			{
				if (t.GetMethod("GetObjectData", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) == null)
				{
					Assert.Fail(t.Name + " does not implement GetObjectData but has private fields.");
				}
			}
			if (!t.IsAbstract 
                && !ExcludeFromConstructorPolicyCheck(t))
			{
                CheckInstantiation(t);
			}
		}

		private void CheckPublicConstructor(Type t, string description, params Type[] parameters)
		{
			// locate constructor
			ConstructorInfo ci =
				t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, parameters, null);
			// fail if constructor does not exist
			Assert.IsNotNull(ci, t.Name + description + " is a required constructor.");
			// fail if constructor is private
			Assert.IsFalse(ci.IsPrivate, t.Name + description + " is private, must be public.");
			// fail if constructor is protected
			Assert.IsFalse(ci.IsFamily, t.Name + description + " is internal, must be public.");
			// fail if constructor is internal
			Assert.IsFalse(ci.IsAssembly, t.Name + description + " is internal, must be public.");
			// fail if constructor is protected internal
			Assert.IsFalse(ci.IsFamilyOrAssembly, t.Name + description + " is protected internal, must be public.");
			// sanity check to make sure the constructor is public
			Assert.IsTrue(ci.IsPublic, t.Name + description + " is not public, must be public.");
		}

		private void CheckProtectedConstructor(Type t, string description, params Type[] parameters)
		{
			// locate constructor
			ConstructorInfo ci =
				t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, parameters, null);
			// fail if constructor does not exist
			Assert.IsNotNull(ci, t.Name + description + " is a required constructor.");
			// fail if constructor is public
			Assert.IsFalse(ci.IsPublic, t.Name + description + " is public, must be protected.");
			// fail if constructor is private
			Assert.IsFalse(ci.IsPrivate, t.Name + description + " is private, must be public or protected.");
			// fail if constructor is internal
			Assert.IsFalse(ci.IsAssembly, t.Name + description + " is internal, must be protected.");
			// fail if constructor is protected internal
			Assert.IsFalse(ci.IsFamilyOrAssembly, t.Name + description + " is protected internal, must be protected.");
			// sanity check to make sure the constructor is protected
			Assert.IsTrue(ci.IsFamily, t.Name + description + " is not protected, must be protected.");
		}

		private void CheckPrivateConstructor(Type t, string description, params Type[] parameters)
		{
			// locate constructor
			ConstructorInfo ci =
				t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, parameters, null);
			// fail if constructor does not exist
			Assert.IsNotNull(ci, t.Name + description + " is a required constructor.");
			// fail if constructor is public
			Assert.IsFalse(ci.IsPublic, t.Name + description + " is public, must be private.");
			// fail if constructor is protected
			Assert.IsFalse(ci.IsFamily, t.Name + description + " is protected, must be private.");
			// fail if constructor is internal
			Assert.IsFalse(ci.IsAssembly, t.Name + description + " is internal, must be private.");
			// fail if constructor is protected internal
			Assert.IsFalse(ci.IsFamilyOrAssembly, t.Name + description + " is protected internal, must be private.");
			// sanity check to make sure the constructor is private
			Assert.IsTrue(ci.IsPrivate, t.Name + description + " is not private, must be private.");
		}

		/// <summary>
		/// If we've got here, then the standards compliance tests have passed...
		/// </summary>
		/// <param name="t">The exception type being tested.</param>
		private void CheckInstantiation(Type t)
		{
			// attempt to instantiate the 3 standard ctors...
			ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
			try
			{
				ctor.Invoke(null);
			}
			catch (Exception ex)
			{
				Assert.Fail("Ctor () for '" + t.Name + "' threw an exception : " + ex.Message);
			}
			ctor = t.GetConstructor(new Type[] {typeof (string)});
			try
			{
				Exception ex = (Exception) ctor.Invoke(new object[] {"My Fingers Turn To Fists"});
				Assert.IsNotNull(ex.Message, t.Name + "'s Message was null.");
			}
			catch (Exception ex)
			{
				Assert.Fail("Ctor (string) for '" + t.Name + "' threw an exception : " + ex.Message);
			}
			ctor = t.GetConstructor(new Type[] {typeof (string), typeof (Exception)});
			try
			{
				Exception ex = (Exception) ctor.Invoke(new object[] {"My Fingers Turn To Fists", new FormatException("Bing")});
				Assert.IsNotNull(ex.Message, t.Name + "'s Message was null.");
				Assert.IsNotNull(ex.InnerException, t.Name + "'s InnerException was null.");
				Assert.AreEqual("Bing", ex.InnerException.Message);
			}
			catch (Exception ex)
			{
				Assert.Fail("Ctor (string, Exception) for '" + t.Name + "' threw an exception : " + ex.Message);
			}
			// test the serialization ctor
			try
			{
				ctor = t.GetConstructor(new Type[] {typeof (string)});
				Exception ex = (Exception) ctor.Invoke(new object[] {"HungerHurtsButStarvingWorks"});
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, ex);
                ms.Seek(0,0);
                Exception inex = (Exception)bf.Deserialize(ms);
				Assert.IsNotNull(inex);
			}
			catch (Exception ex)
			{
				Assert.Fail("Ctor (Serialization) for '" + t.Name + "' threw an exception : " + ex.Message);
			}

		}

		#endregion

		#region Properties

		/// <summary>
		/// We will test all of the exceptions in this assembly for their correctness.
		/// </summary>
		protected Assembly AssemblyToCheck
		{
			get { return _assemblyToCheck; }
			set { _assemblyToCheck = value; }
		}

		#endregion

		private Assembly _assemblyToCheck = null;
	}

	#region Inner Class : SimpleTestException

	/// <summary>
	/// Do nothing exception to verify that the exception tester
	/// is working correctly.
	/// </summary>
	[Serializable]
	public class SimpleTestException : ApplicationException
	{
		#region Public Instance Constructors

		public SimpleTestException()
		{
		}

		public SimpleTestException(string message) : base(message)
		{
		}

		public SimpleTestException(string message, Exception inner)
			: base(message, inner)
		{
		}

		#endregion Public Instance Constructors

		#region Protected Instance Constructors

		// deserialization constructor
		protected SimpleTestException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion Protected Instance Constructors
	}

	#endregion

	#region Inner Class : TestException

	/// <summary>
	/// Exception to verify that the exception tester is working correctly.
	/// </summary>
	[Serializable]
	public class TestException : ApplicationException, ISerializable
	{
		#region Private Instance Fields

		private int _value;

		#endregion Private Instance Fields

		#region Public Instance Constructors

		public TestException()
		{
		}

		public TestException(string message) : base(message)
		{
		}

		public TestException(string message, Exception inner)
			: base(message, inner)
		{
		}

		// constructors that take the added value
		public TestException(string message, int value) : base(message)
		{
			_value = value;
		}

		#endregion Public Instance Constructors

		#region Protected Instance Constructors

		// deserialization constructor
		protected TestException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_value = info.GetInt32("Value");
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		public int Value
		{
			get { return _value; }
		}

		#endregion Public Instance Properties

		#region Override implementation of ApplicationException

		// Called by the frameworks during serialization
		// to fetch the data from an object.
		public override void GetObjectData(
			SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Value", _value);
		}

		// overridden Message property. This will give the
		// proper textual representation of the exception,
		// with the added field value.
		public override string Message
		{
			get
			{
				// NOTE: should be localized...
				string s =
					string.Format(
						CultureInfo.InvariantCulture, "Value: {0}", _value);
				return base.Message + Environment.NewLine + s;
			}
		}

		#endregion Override implementation of ApplicationException
	}

	#endregion

	#region Inner Class : SealedTestException

	/// <summary>
	/// Exception to verify that the exception tester is working on
	/// sealed exception.
	/// </summary>
	[Serializable]
	public sealed class SealedTestException : TestException
	{
		#region Public Instance Constructors

		public SealedTestException()
		{
		}

		public SealedTestException(string message) : base(message)
		{
		}

		public SealedTestException(string message, Exception inner)
			: base(message, inner)
		{
		}

		// constructors that take the added value
		public SealedTestException(string message, int value)
			: base(message, value)
		{
		}

		#endregion Public Instance Constructors

		#region Private Instance Constructors

		// deserialization constructor
		private SealedTestException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion Private Instance Constructors
	}

	#endregion
}
