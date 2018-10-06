/*
* Copyright 2002-2010 the original author or authors.
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

using System;

using NUnit.Framework;

using Quartz;
using Quartz.Spi;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Tests for <see cref="AdaptableJobFactory" />.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class AdaptableJobFactoryTest
    {
        private AdaptableJobFactory jobFactory;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            jobFactory = new AdaptableJobFactory();
        }

        /// <summary>
        /// Tests job creation.
        /// </summary>
        [Test]
        public void TestNewJob_IncompatibleJob()
        {
            try
            {
                // this actually fails already in Quartz level
                TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof (object));
                jobFactory.NewJob(bundle, null);
                Assert.Fail("Created job which was not an IJob");
            }
            catch (ArgumentException)
            {
                // ok
            }
            catch (SchedulerException)
            {
                // ok
            }
            catch (Exception)
            {
                Assert.Fail("Got exception that was not instance of SchedulerException or ArgumentException");
            }
        }

        /// <summary>
        /// Tests job creation.
        /// </summary>
        [Test]
        public void TestNewJob_ThreadStartJob()
        {
            // TODO ThreadStart is not the way to go
        }

        /// <summary>
        /// Tests job creation.
        /// </summary>
        [Test]
        public void TestNewJob_NormalIJob()
        {
            TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof (NoOpJob));
            IJob job = jobFactory.NewJob(bundle, null);

            Assert.IsNotNull(job, "Returned job was null");
        }
    }

    internal class NoOpThreadStartJob : NoOpJob
    {
        public void Execute()
        {
            Execute(null);
        }
    }
}