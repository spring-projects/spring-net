using System;
using NUnit.Framework;

namespace Spring.Util
{
	/// <summary>
	/// Summary description for TestMemberwiseCopy.
	/// </summary>
	[TestFixture]
	public class ReflectionUtilsMemberwiseCopyTests
	{
		[Test]
		public void TestSameType()
		{
			SampleBaseClass i1 = new SampleDerivedClass("1st config val");
			SampleBaseClass i2 = new SampleDerivedClass("2nd config val");

			ReflectionUtils.MemberwiseCopy(i1, i2);

			Assert.AreEqual(i1, i2);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), "object types are not related")]
		public void DifferentTypesForbidden()
		{
			ReflectionUtils.MemberwiseCopy("test", 2);
		}

		[Test]
		public void TestDerivedTypeAllowed()
		{
			SampleBaseClass i1 = new SampleDerivedClass("1st config val");
			SampleBaseClass i2 = new SampleFurtherDerivedClass("2nd config val");

			ReflectionUtils.MemberwiseCopy(i1, i2);

			Assert.AreEqual(i1, i2);
		}

		[Test]
		public void TestBaseTypeAllowed()
		{
			SampleBaseClass i1 = new SampleDerivedClass("1st config val");
			SampleBaseClass i2 = new SampleFurtherDerivedClass("2nd config val");

			ReflectionUtils.MemberwiseCopy(i2, i1);

			Assert.AreEqual(i2, i1);
		}
	}
	
	#region Test Support Classes
	
	public class SampleBaseClass
	{
		private const string MyConstant = "SampleBaseClass.MyConstant";
		private readonly string _someReadOnlyVal = "SampleBaseClass.SomeReadOnlyVal";
		protected readonly string _someProtectedReadOnlyVal = "SampleBaseClass.SomeProtectedReadOnlyVal";
		private string _someConfigVal = "SampleBaseClass.SomeConfigVal";

		public SampleBaseClass(string someConfigVal)
		{
			if (someConfigVal == null) throw new ArgumentNullException("someConfigVal must not be null");
			_someConfigVal = someConfigVal;
			_someProtectedReadOnlyVal = "Protected" + someConfigVal;
		}

		public override int GetHashCode()
		{
			int result = _someReadOnlyVal != null ? _someReadOnlyVal.GetHashCode() : 0;
			result = 29*result + (_someProtectedReadOnlyVal != null ? _someProtectedReadOnlyVal.GetHashCode() : 0);
			result = 29*result + (_someConfigVal != null ? _someConfigVal.GetHashCode() : 0);
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj) return true;
			SampleBaseClass sampleBaseClass = obj as SampleBaseClass;
			if (sampleBaseClass == null) return false;
			if (!Equals(_someReadOnlyVal, sampleBaseClass._someReadOnlyVal)) return false;
			if (!Equals(_someProtectedReadOnlyVal, sampleBaseClass._someProtectedReadOnlyVal)) return false;
			if (!Equals(_someConfigVal, sampleBaseClass._someConfigVal)) return false;
			return true;
		}
	}
	
	public class SampleDerivedClass : SampleBaseClass
	{
		private string _someConfigVal = "SampleDerivedClass.SomeConfigVal";

		public SampleDerivedClass(string someConfigVal) : base(someConfigVal)
		{
			_someConfigVal = "SampleDerivedClass." + someConfigVal;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() + 29*(_someConfigVal != null ? _someConfigVal.GetHashCode() : 0);
		}

		public override bool Equals(object obj)
		{
			if (this == obj) return true;
			SampleDerivedClass sampleDerivedClass = obj as SampleDerivedClass;
			if (sampleDerivedClass == null) return false;
			if (!base.Equals(obj)) return false;
			if (!Equals(_someConfigVal, sampleDerivedClass._someConfigVal)) return false;
			return true;
		}
	}
	
	public class SampleFurtherDerivedClass : SampleDerivedClass
	{
		public SampleFurtherDerivedClass(string someConfigVal) : base(someConfigVal)
		{
		}
	}
	
	#endregion		
}