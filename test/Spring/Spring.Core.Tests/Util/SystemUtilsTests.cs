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
using System.Threading;
using NUnit.Framework;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// This class contains tests for SystemUtils
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class SystemUtilsTests
    {
        private static string threadName = "TestingThread";


        [Test]
        public void ThreadIdIsName()
        {
            Thread t = new Thread(new ThreadStart(AssertThreadName));
            t.Name = threadName;
            t.Start();
            t.Join();
        }


        public void AssertThreadName()
        {
            Assert.AreEqual(threadName, SystemUtils.ThreadId);               
        }

        [Test]
        public void ThreadIdIsInt()
        {
            Thread t = new Thread(new ThreadStart(AssertThreadIsInt));
            t.Start();
            t.Join();
            
        }

        public void AssertThreadIsInt()
        {
            try
            {
                int.Parse(SystemUtils.ThreadId);
            }
            catch (Exception)
            {
                Assert.Fail("ThreadId should be an integer");
            }
        }
    }
}