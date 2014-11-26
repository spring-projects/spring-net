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
using System.Collections.Generic;
using NUnit.Framework;
//using NUnit.Framework.SyntaxHelpers;

#endregion

namespace Spring.Collections.Generic
{
    /// <summary>
    /// This class contains tests for ReadOnlyDictionary
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ReadOnlyDictionaryTests
    {
        private IDictionary<string, int> personDictionary;
        private IDictionary<string, int> readonlyPersonDictionary;

        [SetUp]
        public void Setup()
        {
            personDictionary = new Dictionary<string, int>();
            personDictionary.Add("Mark", 38);
            personDictionary.Add("Daniela", 38);
            readonlyPersonDictionary = new ReadOnlyDictionary<string, int>(personDictionary);

        }

        [Test]
        public void TestSunnyDay()
        {

            Assert.That(readonlyPersonDictionary.IsReadOnly, Is.True);
            Assert.That(readonlyPersonDictionary.Contains(new KeyValuePair<string, int>("Mark", 38)));
            Assert.That(readonlyPersonDictionary.ContainsKey("Mark"), Is.True);
            Assert.That(readonlyPersonDictionary.Count, Is.EqualTo(2));
        }

        [Test]
        public void Exceptions()
        {
            //updating to nunit 2.5 would be nice to use Assert.That( SomeMethod, Throws.Exception<ArgumentException>());
            // Execute(delegate {  });

            Execute(() => { readonlyPersonDictionary["Mark"] = 4; });
            Execute(() => readonlyPersonDictionary.Add("Gabriel", 3));
            Execute(() => readonlyPersonDictionary.Add(new KeyValuePair<string, int>("Mark", 38)));
            Execute(() => readonlyPersonDictionary.Clear());
            Execute(() => readonlyPersonDictionary.Remove("Mark"));
            Execute(() => readonlyPersonDictionary.Remove(new KeyValuePair<string, int>("Mark", 38)));
        }

        public void Execute(DoInTryCatch exceptionDelegate)
        {
            try
            {
                exceptionDelegate.Invoke();
                Assert.Fail("Should throw NotSupportedException");
            } catch (NotSupportedException)
            {
                
            }
        }

        public delegate void DoInTryCatch();

        
    }
}
