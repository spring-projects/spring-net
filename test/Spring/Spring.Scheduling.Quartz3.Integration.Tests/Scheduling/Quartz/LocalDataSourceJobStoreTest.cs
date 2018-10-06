#region License

/*
 * Copyright � 2002-2007 the original author or authors.
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

using System.Threading;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

#endregion

namespace Spring.Scheduling.Quartz.Integration.Tests
{
    [TestFixture]
    public class LocalDataSourceJobStoreTest
    {
        private IApplicationContext ctx;

        [SetUp]
        public void SetUp()
        {
            ctx = new XmlApplicationContext(
                "assembly://Spring.Scheduling.Quartz3.Integration.Tests/Spring.Scheduling.Quartz/LocalDataSourceJobStoreTest.xml");
        }

        [Test]
        [Explicit("Appveyor problems")]
        public void TestLocalDataSourceJobStore()
        {
            // sleep 20 seconds
            Thread.Sleep(20000);
        }
    }
}