#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using System.Text;
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
        /// <summary>
        /// On my Notebook gives 
        /// Normal Hashtable: 00:00:01.5937500
        /// Synced Hashtable: 00:00:02.8593750
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