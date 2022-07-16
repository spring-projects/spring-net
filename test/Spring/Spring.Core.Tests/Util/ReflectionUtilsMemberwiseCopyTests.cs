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
		public void DifferentTypesForbidden()
		{
            Assert.Throws<ArgumentException>(() => ReflectionUtils.MemberwiseCopy("test", 2), "object types are not related");
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

			Assert.AreEqual(i1, i2);
		}

#if !NETCOREAPP
		[Test]
		public void MediumTrustAllowsCopyingBetweenTypesFromSameModule()
		{
			SampleBaseClass i1 = new SampleDerivedClass("1st config val");
			SampleBaseClass i2 = new SampleFurtherDerivedClass("2nd config val");

            SecurityTemplate.MediumTrustInvoke(new System.Threading.ThreadStart(new CopyCommand(i2, i1).Execute));
			Assert.AreEqual(i1, i2);
		}

		[Test]
		public void MediumTrustThrowsSecurityExceptionWhenCopyingBetweenTypesFromDifferentModules()
		{
			Exception e1 = new Exception("my name is e1");
            var e2 = new System.Web.HttpException("my name is e2");
            // I know, I am a bit paranoid about that basic assumption
            Assert.AreNotEqual( e1.GetType().Assembly, e2.GetType().Assembly );

            SecurityTemplate.MediumTrustInvoke(new System.Threading.ThreadStart(new CopyCommand(e2, e1).Execute));
			Assert.AreEqual(e1.Message, e2.Message);
		}
#endif

        class CopyCommand
        {
            private object a;
            private object b;

            public CopyCommand(object a, object b)
            {
                this.a = a;
                this.b = b;
            }

            public void Execute()
            {
                ReflectionUtils.MemberwiseCopy(a, b);
            }
        }
    }

	#region Test Support Classes

	public class SampleBaseClass
	{
		private const string MyConstant = "SampleBaseClass.MyConstant";
		private readonly string _someReadOnlyVal = "SampleBaseClass.SomeReadOnlyVal";
		protected readonly string _someProtectedReadOnlyVal = "SampleBaseClass.SomeProtectedReadOnlyVal";
		private string _someConfigVal;

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
            if (ReferenceEquals(obj, null) || (!this.GetType().IsAssignableFrom(obj.GetType())))
                return false;
            if (ReferenceEquals(this , obj))
                return true;

            SampleBaseClass sampleBaseClass = (SampleBaseClass) obj;
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
			if (!base.Equals(obj))
                return false;

            SampleDerivedClass sampleDerivedClass = (SampleDerivedClass)obj;
            if (!Equals(_someConfigVal, sampleDerivedClass._someConfigVal))
                return false;
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
