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
using System.Collections;

using NUnit.Framework;

#endregion

namespace Spring.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <version>$Id: $</version>
    [TestFixture]
    public class SynchronizedHashtableTests
    {
        [Test]
        public void BehavesLikeHashtable()
        {
            SynchronizedHashtable st = new SynchronizedHashtable();
            st.Add("key", "value");
            Assert.AreEqual("value", st["key"]);
            st["key"] = "value2";
            Assert.AreEqual("value2", st["key"]);
            st["key2"] = "value3";
            Assert.AreEqual("value3", st["key2"]);

            try
            {
                st.Add("key", "value4");
                Assert.Fail();
            }
            catch(ArgumentException)
            {}

            Assert.AreEqual(2, st.Count);
        }

        [Test]
        public void InitializeFromOtherCopiesValues()
        {
            Hashtable ht = new Hashtable();
            ht["key"] = "value";
            ht["key2"] = "value2";

            SynchronizedHashtable st = new SynchronizedHashtable(ht, false);
            Assert.AreEqual(2, st.Count);
            ht.Remove("key");
            Assert.AreEqual(1, ht.Count);
            Assert.AreEqual(2, st.Count);
        }

        [Test]
        public void DefaultsToCaseSensitive()
        {
            SynchronizedHashtable st = new SynchronizedHashtable();
            st.Add("key","value");            
            st.Add("KEY","value");            
            Assert.AreEqual(2, st.Count);
        }

        [Test]
        public void IgnoreCaseIgnoresCase()
        {
            SynchronizedHashtable st = new SynchronizedHashtable(true);
            st.Add("key", "value");
            Assert.AreEqual("value", st["KEY"]);
            st["KeY"] = "value2";
            Assert.AreEqual(1, st.Count);
            Assert.AreEqual("value2", st["key"]);

            try
            {
                st.Add("KEY", "value2");
                Assert.Fail();
            }
            catch (ArgumentException)
            {}

            Hashtable ht = new Hashtable();
            ht.Add("key","value");            
            ht.Add("KEY","value");
            try
            {
                st = new SynchronizedHashtable(ht, true);
                Assert.Fail();
            }
            catch (ArgumentException)
            {}
        }

        [Test]
        public void WrapKeepsOriginalHashtableReference()
        {
            Hashtable ht = new Hashtable();
            ht["key"] = "value";
            ht["key2"] = "value2";

            SynchronizedHashtable st = SynchronizedHashtable.Wrap(ht);
            Assert.AreEqual(2, st.Count);
            ht.Remove("key");
            Assert.AreEqual(1, ht.Count);
            Assert.AreEqual(1, st.Count);            
        }

        /// <summary>
        /// On my Notebook gives 
        /// Normal Hashtable: 00:00:00.0937500
        /// Synced Hashtable: 00:00:00.9375000
        /// =locking *has* a peformance impact.
        /// </summary>
        [Test, Explicit]
        public void TestLockingPerformanceImpact()
        {
            StopWatch watch = new StopWatch();
            object[] buckets = new object[10];
            
            int iterations = 10000000;

            IDictionary testDict = new Hashtable();
            buckets[5] = "value";
            object testResult;

            using(watch.Start("Normal Hashtable: {0}"))
            for(int i=0;i<iterations;i++)
            {
                testResult = buckets[6];
                buckets[5] = "value 2";
            }

            testDict = new Hashtable();
            testDict.Add( "key", "value" );
            using(watch.Start("Synced Hashtable: {0}"))
            for(int i=0;i<iterations;i++)
            {
                lock(buckets)
                {
                    testResult = buckets[6];                    
                }
                lock(buckets)
                {
                    buckets[5] = "value 2";
                }
            }
        }

        /// <summary>
        /// On my Notebook gives 
        /// Method returning exception: 00:00:00.0156250
        /// Method throwing exception: 00:00:02.1562500        
        /// </summary>
        [Test, Explicit]
        public void TestPeformanceImpactOfThrowingExceptions()
        {
            StopWatch watch = new StopWatch();

            int iterations = 1000000;

            using(watch.Start("Method returning exception: {0}"))
            for(int i=0;i<iterations;i++)
            {
                Exception ex = MethodReturnExceptionInformation();
            }
            
            using(watch.Start("Method throwing exception: {0}"))
            for(int i=0;i<iterations;i++)
            {
                Exception ex;
                try
                {
                    MethodThrowingException();
                }
                catch (Exception innerEx)
                {
                    ex = innerEx;
                }
            }
            
        }

        private void MethodThrowingException()
        {
            throw new InvalidOperationException( "da message" );
        }

        private Exception MethodReturnExceptionInformation()
        {
            return new InvalidOperationException( "da message" );
        }

    }
}