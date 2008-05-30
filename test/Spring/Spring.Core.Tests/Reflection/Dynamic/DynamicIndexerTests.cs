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
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Spring.Context.Support;

#endregion

namespace Spring.Reflection.Dynamic
{
	/// <summary>
    /// Unit tests for the DynamicIndexer class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: DynamicIndexerTests.cs,v 1.1 2007/08/08 04:06:01 bbaia Exp $</version>
	[TestFixture]
    public sealed class DynamicIndexerTests
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
	    
        [TestFixtureTearDown]
        public void TearDown()
        {
            //DynamicReflectionManager.SaveAssembly();
        }
	    
        #endregion
        
	    [Test]
        public void TestIndexers() 
        {
            IDynamicIndexer members = DynamicIndexer.Create(typeof(ArrayList).GetProperty("Item"));
            Inventor nikola = (Inventor) members.GetValue(ieee.Members, new object[] { 0 });
            Assert.AreEqual(tesla, nikola);
            members.SetValue(ieee.Members, new object[] { 0 }, new Inventor("Ana Maria Seovic", new DateTime(2004, 8, 14), "Serbian"));
            Assert.AreEqual("Ana Maria Seovic", ((Inventor) members.GetValue(ieee.Members, 0)).Name);
            members.SetValue(ieee.Members, 1, tesla);
            Assert.AreEqual("Nikola Tesla", ((Inventor) members.GetValue(ieee.Members, 1)).Name);
	        
            IDynamicIndexer officers = DynamicIndexer.Create(typeof(Hashtable).GetProperty("Item"));
            Assert.AreEqual(pupin, officers.GetValue(ieee.Officers, new object[] {"president"}));
            officers.SetValue(ieee.Officers, "president",
                              new Inventor("Aleksandar Seovic", new DateTime(1974, 8, 24), "Serbian"));
            Assert.AreEqual("Aleksandar Seovic", ((Inventor)officers.GetValue(ieee.Officers, "president")).Name);
        }

        #region Performance tests

        private DateTime start, stop;

        //[Test]
        public void PerformanceTests()
        {
            int n = 10000000;
            object x = null;

            // ieee.Members[0]
            start = DateTime.Now;
            for (int i = 0; i < n; i++)
            {
                x = ieee.Members[0];
            }
            stop = DateTime.Now;
            PrintTest("ieee.Members[0] (direct)", n, Elapsed);

            start = DateTime.Now;
            IDynamicIndexer members = DynamicIndexer.Create(typeof(ArrayList).GetProperty("Item"));
            for (int i = 0; i < n; i++)
            {
                x = members.GetValue(ieee.Members, 0);
            }
            stop = DateTime.Now;
            PrintTest("ieee.Members[0] (dynamic reflection)", n, Elapsed);

            start = DateTime.Now;
            PropertyInfo membersPi = typeof(ArrayList).GetProperty("Item");
            object[] indexArgs = new object[] { 0 };
            for (int i = 0; i < n; i++)
            {
                x = membersPi.GetValue(ieee.Members, indexArgs);
            }
            stop = DateTime.Now;
            PrintTest("ieee.Members[0] (standard reflection)", n, Elapsed);
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
