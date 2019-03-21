#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using System.Threading;
using NUnit.Framework;

#endregion

namespace Spring.Messaging.Core
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ThreadingTests
    {
        private int activeListenerCount = 0;
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test()
        {
            Interlocked.Increment(ref activeListenerCount);
            // just gets the current value...
            int count = Interlocked.CompareExchange(ref activeListenerCount, -1, -1);
            Assert.AreEqual(1, count);

        }

        
    }
}