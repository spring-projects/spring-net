using System;
using System.Diagnostics;
using System.Reflection;

using NUnit.Framework;
using Spring.Context.Support;

namespace Spring.Reflection.Dynamic
{
    /// <summary>
    /// Unit tests common for all IDynamicProperty implementations.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public abstract class BasePropertyTests
    {
        protected Inventor tesla;
        protected Inventor pupin;
        protected Society ieee;

        #region SetUp and TearDown

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ContextRegistry.Clear();
            tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            tesla.Inventions = new string[]
                {
                    "Telephone repeater", "Rotating magnetic field principle",
                    "Polyphase alternating-current system", "Induction motor",
                    "Alternating-current power transmission", "Tesla coil transformer",
                    "Wireless communication", "Radio", "Fluorescent lights"
                };
            tesla.PlaceOfBirth.City = "Smiljan";

            pupin = new Inventor("Mihajlo Pupin", new DateTime(1854, 10, 9), "Serbian");
            pupin.Inventions = new string[] { "Long distance telephony & telegraphy", "Secondary X-Ray radiation", "Sonar" };
            pupin.PlaceOfBirth.City = "Idvor";
            pupin.PlaceOfBirth.Country = "Serbia";

            ieee = new Society();
            ieee.Members.Add(tesla);
            ieee.Members.Add(pupin);
            ieee.Officers["president"] = pupin;
            ieee.Officers["advisors"] = new Inventor[] { tesla, pupin }; // not historically accurate, but I need an array in the map ;-)
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            //DynamicReflectionManager.SaveAssembly();
        }

        #endregion

        protected abstract IDynamicProperty Create(PropertyInfo property);

        [Test]
        public void TestInstanceProperties()
        {
            IDynamicProperty placeOfBirth = Create(typeof(Inventor).GetProperty("PlaceOfBirth"));
            Assert.AreEqual(tesla.PlaceOfBirth, placeOfBirth.GetValue(tesla));

            IDynamicProperty dateOfBirth = Create(typeof(Inventor).GetProperty("DOB"));
            Assert.AreEqual(tesla.DOB, dateOfBirth.GetValue(tesla));
            dateOfBirth.SetValue(tesla, new DateTime(2004, 8, 14));
            Assert.AreEqual(new DateTime(2004, 8, 14), dateOfBirth.GetValue(tesla));

            DateTime mostImportantDayInTheWorldEver = new DateTime(2004, 8, 14);
            IDynamicProperty year = Create(typeof(DateTime).GetProperty("Year"));
            Assert.AreEqual(mostImportantDayInTheWorldEver.Year, year.GetValue(mostImportantDayInTheWorldEver));
        }

        [Test]
        public void AccessInheritedPropertyFromBaseClass()
        {
            IDynamicProperty p = Create(typeof(ClassWithNonReadableProperty).GetProperty("MyBaseProperty"));
            BaseClass baseObject = new BaseClass();
            baseObject.MyBaseProperty = "testtext";
            Assert.AreEqual("testtext", p.GetValue(baseObject));
        }

        [Test]
        public void AccessInheritedPropertyFromDerivedClass()
        {
            IDynamicProperty p = Create(typeof(BaseClass).GetProperty("MyBaseProperty"));
            ClassWithNonReadableProperty derivedObject = new ClassWithNonReadableProperty();
            derivedObject.MyBaseProperty = "testtext";
            Assert.AreEqual("testtext", p.GetValue(derivedObject));
        }

        [Test]
        public void AccessOverriddenProperty()
        {
            IDynamicProperty pVirt = Create(typeof(BaseClass).GetProperty("MyVirtualBaseProperty"));
            IDynamicProperty pOverridden = Create(typeof(ClassWithNonReadableProperty).GetProperty("MyVirtualBaseProperty"));

            Assert.AreEqual("MyVirtualBasePropertyText", pVirt.GetValue(new BaseClass()));
            try
            {
                Assert.AreEqual("MyVirtualBasePropertyText", pOverridden.GetValue(new BaseClass()));
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
            }
            Assert.AreEqual("MyOverridenDerivedPropertyText", pVirt.GetValue(new ClassWithNonReadableProperty()));
            Assert.AreEqual("MyOverridenDerivedPropertyText", pOverridden.GetValue(new ClassWithNonReadableProperty()));
        }


        [Test]
        public void TestStaticProperties()
        {
            IDynamicProperty today = Create(typeof(DateTime).GetProperty("Today"));
            Assert.AreEqual(DateTime.Today, today.GetValue(null));

            IDynamicProperty myProperty = Create(typeof(MyStaticClass).GetProperty("MyProperty"));
            myProperty.SetValue(null, "here we go...");
            Assert.AreEqual("here we go...", myProperty.GetValue(null));
        }

        [Test, Ignore("test N/A anymore due to System.Reflection.Emit.DynamicMethod")]
        public void TestForRestrictiveGetter()
        {
            Something something = new Something();

            IDynamicProperty third = Create(typeof(Something).GetProperty("Third"));
            //this should be ok, because both get and set of the "Third" property are public
            third.SetValue(something, 456);
            Assert.AreEqual(456, third.GetValue(something));

            IDynamicProperty first = Create(typeof(Something).GetProperty("First"));
            first.SetValue(something, 123);
            //this should cause MethodAccessException, because get is private
            Assert.Throws<MethodAccessException>(() => first.GetValue(something));
        }

        [Test, Ignore("test N/A anymore due to System.Reflection.Emit.DynamicMethod")]
        public void TestForRestrictiveSetter()
        {
            Something something = new Something();

            IDynamicProperty third = Create(typeof(Something).GetProperty("Third"));
            third.SetValue(something, 456);
            //this should be ok, because both get and set of the "Third" property are public
            Assert.AreEqual(456, third.GetValue(something));

            IDynamicProperty second = Create(typeof(Something).GetProperty("Second"));
            Assert.AreEqual(2, second.GetValue(something));
            //this should cause MethodAccessException, because set is private in "Second" property
            second.SetValue(something, 123);

            //this should never execute
            Assert.Throws<MethodAccessException>(() => second.GetValue(something));
        }

        #region Performance tests

        private DateTime start, stop;

        [Test]
        [Explicit]
        public void PerformanceTests()
        {
            int n = 10000000;
            object x = null;

            // tesla.PlaceOfBirth
            start = DateTime.Now;
            for (int i = 0; i < n; i++)
            {
                x = tesla.PlaceOfBirth;
            }
            stop = DateTime.Now;
            PrintTest("tesla.PlaceOfBirth (direct)", n, Elapsed);

            start = DateTime.Now;
            IDynamicProperty placeOfBirth = DynamicProperty.Create(typeof(Inventor).GetProperty("PlaceOfBirth"));
            for (int i = 0; i < n; i++)
            {
                x = placeOfBirth.GetValue(tesla);
            }
            stop = DateTime.Now;
            PrintTest("tesla.PlaceOfBirth (dynamic reflection)", n, Elapsed);

            start = DateTime.Now;
            PropertyInfo placeOfBirthPi = typeof(Inventor).GetProperty("PlaceOfBirth");
            for (int i = 0; i < n; i++)
            {
                x = placeOfBirthPi.GetValue(tesla, null);
            }
            stop = DateTime.Now;
            PrintTest("tesla.PlaceOfBirth (standard reflection)", n, Elapsed);
        }

        private double Elapsed
        {
            get { return (stop.Ticks - start.Ticks) / 10000000f; }
        }

        private void PrintTest(string name, int iterations, double duration)
        {
            Debug.WriteLine(String.Format("{0,-60} {1,12:#,###} {2,12:##0.000} {3,12:#,###}", name, iterations, duration, iterations / duration));
        }

        #endregion
    }

    #region IL generation helper classes (they help if you look at them in Reflector ;-)

    public class ValueTypeProperty //: IDynamicProperty
    {
        public object GetValue(object target)
        {
            return ((Inventor)target).DOB;
        }

        public void SetValue(object target, object value)
        {
            ((Inventor)target).DOB = (DateTime)value;
        }
    }

    public class ValueTypeTarget //: IDynamicProperty
    {
        public object GetValue(object target)
        {
            return ((MyStruct)target).Year;
        }

        public void SetValue(object target, object value)
        {
            MyStruct o = (MyStruct)target;
            o.Year = (int)value;
        }
    }

    public class StaticProperty //: IDynamicProperty
    {
        public object GetValue(object target)
        {
            return MyStaticClass.MyProperty;
        }

        public void SetValue(object target, object value)
        {
            MyStaticClass.MyProperty = (string)value;
        }
    }

    #endregion

    public class MyStaticClass
    {
        public const Int64 MyConst = 3456;
        public static readonly string myReadonlyField;
        private static readonly string myPrivateReadonlyField;
        public static string myField;

        static MyStaticClass()
        {
            myReadonlyField = "hohoho";
            myPrivateReadonlyField = "hahaha";
        }

        public static string MyProperty
        {
            get { return myField; }
            set { myField = value; }
        }

        public static string MyPrivateReadOnylFieldAccessor
        {
            get { return myPrivateReadonlyField; }
        }
    }

    public struct MyStaticStruct
    {
        public const int constant = 20;
        public static int staticYear;

        public static int StaticYear
        {
            get { return staticYear; }
            set { staticYear = value; }
        }
    }

    public struct MyStruct
    {
        public int year;

        public int Year
        {
            get { return year; }
            set { year = value; }
        }
    }

    public class BaseClass
    {
        private string myBaseProperty;

        public string MyBaseProperty
        {
            set { myBaseProperty = value; }
            get { return myBaseProperty; }
        }        

        public virtual string MyVirtualBaseProperty
        {
            get { return "MyVirtualBasePropertyText"; }
        }
    }

    public class ClassWithNonReadableProperty : BaseClass
    {
        private string myProperty;

        public string MyProperty
        {
            set { myProperty = value; }
        }

        public override string MyVirtualBaseProperty
        {
            get { return "MyOverridenDerivedPropertyText"; }
        }
    }

    public class Something
    {
        public int First
        {
            private get { return first; }
            set { first = value; }
        }

        public int Second
        {
            get { return second; }
            private set { second = value; }
        }

        public int Third
        {
            get { return third; }
            set { third = value; }
        }
        public Something()
        {
            first = 1;
            second = 2;
            third = 3;
        }
        private int first;
        private int second;
        private int third;

    }
}
