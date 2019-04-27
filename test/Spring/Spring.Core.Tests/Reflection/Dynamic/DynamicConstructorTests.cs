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
    /// Unit tests for the DynamicConstructor class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
	[TestFixture]
    public sealed class DynamicConstructorTests
    {
        private Inventor tesla;
        private Inventor pupin;
        private Society ieee;

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
        
	    [Test]
        public void TestConstructors() 
        {
            IDynamicConstructor newInventor = DynamicConstructor.Create(
                typeof(Inventor).GetConstructor(new Type[] { typeof(string), typeof(DateTime), typeof(string) }));
            Inventor ana = (Inventor) newInventor.Invoke(new object[] {"Ana Maria Seovic", new DateTime(2004, 8, 14), "Serbian"});
            Assert.AreEqual("Ana Maria Seovic", ana.Name);

            IDynamicConstructor newDate = DynamicConstructor.Create(
                typeof(DateTime).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int) }));
            Assert.AreEqual(DateTime.Today, newDate.Invoke(new object[] { DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day }));
        }

        #region Performance tests

        private DateTime start, stop;

        //[Test]
        public void PerformanceTests()
        {
            int n = 10000000;
            object x = null;

            // new Inventor()
            start = DateTime.Now;
            for (int i = 0; i < n; i++)
            {
                x = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            }
            stop = DateTime.Now;
            PrintTest("new Inventor() (direct)", n, Elapsed);

            start = DateTime.Now;
            IDynamicConstructor newInventor = DynamicConstructor.Create(
                typeof(Inventor).GetConstructor(new Type[] { typeof(string), typeof(DateTime), typeof(string) }));
            for (int i = 0; i < n; i++)
            {
                object[] args = new object[] { "Nikola Tesla", new DateTime(1856, 7, 9), "Serbian" };
                x = newInventor.Invoke(args);
            }
            stop = DateTime.Now;
            PrintTest("new Inventor() (dynamic reflection)", n, Elapsed);

            start = DateTime.Now;
            ConstructorInfo newInventorCi = 
                typeof(Inventor).GetConstructor(new Type[] { typeof(string), typeof(DateTime), typeof(string) });
            for (int i = 0; i < n; i++)
            {
                object[] args = new object[] { "Nikola Tesla", new DateTime(1856, 7, 9), "Serbian" };
                x = newInventorCi.Invoke(args);
            }
            stop = DateTime.Now;
            PrintTest("new Inventor() (standard reflection)", n, Elapsed);
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
}
