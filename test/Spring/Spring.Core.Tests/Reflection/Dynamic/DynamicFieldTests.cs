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
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Spring.Context.Support;

#endregion

namespace Spring.Reflection.Dynamic
{
	/// <summary>
    /// Unit tests for the DynamicField class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: DynamicFieldTests.cs,v 1.2 2008/05/16 10:02:41 oakinger Exp $</version>
	[TestFixture]
    public class DynamicFieldTests
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
	    
        [TestFixtureTearDown]
        public void TearDown()
        {
            //DynamicReflectionManager.SaveAssembly();
        }
	    
        #endregion
        
        protected virtual IDynamicField Create(FieldInfo field)
        {
            return DynamicField.Create(field);
        }

	    [Test]
        public void TestInstanceFields() 
        {
            IDynamicField name = Create(typeof(Inventor).GetField("Name"));
	        Assert.AreEqual(tesla.Name, name.GetValue(tesla));
            name.SetValue(tesla, "Tesla, Nikola");
            Assert.AreEqual("Tesla, Nikola", tesla.Name);
            Assert.AreEqual("Tesla, Nikola", name.GetValue(tesla));

            MyStruct myYearHolder = new MyStruct();
            myYearHolder.Year = 2004;
            IDynamicField year = Create(typeof(MyStruct).GetField("year", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            Assert.AreEqual(2004, year.GetValue(myYearHolder));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestAttemptingToSetFieldOfValueTypeInstance()
        {
            MyStruct myYearHolder = new MyStruct();
            IDynamicField year = Create(typeof(MyStruct).GetField("year"));
            year.SetValue(myYearHolder, 2004);
        }
	    
	    [Test]
        public void TestStaticFieldsOfClass()
        {
            IDynamicField myField = Create(typeof(MyStaticClass).GetField("myField"));
            myField.SetValue(null, "here we go...");

            IDynamicField myConst = Create(typeof(MyStaticClass).GetField("MyConst"));
            Assert.AreEqual(3456, myConst.GetValue(null));
            try {
                myConst.SetValue(null, 7890);
            }
            catch(InvalidOperationException){}

            IDynamicField myReadonlyField = Create(typeof(MyStaticClass).GetField("myReadonlyField"));
            Assert.AreEqual("hohoho", myReadonlyField.GetValue(null));
            try {
                myReadonlyField.SetValue(null, "some other string");
            }
            catch(InvalidOperationException){}
        }

	    [Test]
        public void TestStaticFieldsOfStruct()
        {
            // static readonly
            IDynamicField maxValue = Create(typeof(DateTime).GetField("MaxValue"));
            Assert.AreEqual(DateTime.MaxValue, maxValue.GetValue(null));
            try {
                maxValue.SetValue(null, DateTime.Now);
            }
            catch(InvalidOperationException){}

            // const
	        IDynamicField int64max = Create(typeof(Int64).GetField("MaxValue", BindingFlags.Public | BindingFlags.Static));
            Assert.AreEqual(Int64.MaxValue, int64max.GetValue(null));
            try {
                int64max.SetValue(null, 0);
            }
            catch(InvalidOperationException){}

            // pure static
            IDynamicField myField = Create(typeof(MyStaticStruct).GetField("staticYear"));
            myField.SetValue(null, 2008);
            Assert.AreEqual(2008, myField.GetValue(null));
        }

        #region Performance tests

        private DateTime start, stop;

        //[Test]
        public void PerformanceTests()
        {
            int n = 10000000;
            object x = null;

            // tesla.Name
            start = DateTime.Now;
            for (int i = 0; i < n; i++)
            {
                x = tesla.Name;
            }
            stop = DateTime.Now;
            PrintTest("tesla.Name (direct)", n, Elapsed);

            start = DateTime.Now;
            IDynamicField placeOfBirth = Create(typeof(Inventor).GetField("Name"));
            for (int i = 0; i < n; i++)
            {
                x = placeOfBirth.GetValue(tesla);
            }
            stop = DateTime.Now;
            PrintTest("tesla.Name (dynamic reflection)", n, Elapsed);

            start = DateTime.Now;
            FieldInfo placeOfBirthFi = typeof(Inventor).GetField("Name");
            for (int i = 0; i < n; i++)
            {
                x = placeOfBirthFi.GetValue(tesla);
            }
            stop = DateTime.Now;
            PrintTest("tesla.Name (standard reflection)", n, Elapsed);
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

    public class ValueTypeField : IDynamicField
    {
        public object GetValue(object target)
        {
            return ((Inventor) target).Name;
        }

        public void SetValue(object target, object value)
        {
            ((Inventor)target).Name = (string) value;
        }
    }

    public class ValueTypeTargetField : IDynamicField
    {
        public object GetValue(object target)
        {
            return ((MyStruct) target).year;
        }

        public void SetValue(object target, object value)
        {
            MyStruct o = (MyStruct) target;
            o.Year = (int) value;
        }
    }
    
    public class StaticField : IDynamicField
    {
        public object GetValue(object target)
        {
            return MyStaticClass.myField;
        }

        public void SetValue(object target, object value)
        {
            MyStaticClass.myField = (string) value;
        }
    }

    public class StaticConst : IDynamicField
    {
        public object GetValue(object target)
        {
            return MyStaticClass.MyConst;
        }

        public void SetValue(object target, object value)
        {
        }
    }
    
    #endregion
    
}
