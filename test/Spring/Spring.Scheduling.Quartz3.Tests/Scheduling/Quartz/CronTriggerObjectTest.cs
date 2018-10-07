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
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using Quartz;
using Quartz.Impl;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Tests for <see cref="CronTriggerObject" />.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class CronTriggerObjectTest : TriggerObjectTest
    {
        private CronTriggerObject cronTrigger;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            cronTrigger = new CronTriggerObject();
            cronTrigger.ObjectName = TRIGGER_NAME;
            Trigger = cronTrigger;
        }

        /// <summary>
        /// Tests all possible misfire instructions for cron trigger 
        /// from strings to int.
        /// </summary>
        [Test]
        public void TestMisfireInstructionNames()
        {
            string[] names = new string[] { "DoNothing", "FireOnceNow", "SmartPolicy" };
            foreach (string name in names)
            {
                cronTrigger.MisfireInstructionName = name;                
            }
        }

        /// <summary>
        /// Tests that JobDetail defaults values as expected in AfterPropertiesSet.
        /// </summary>
        [Test]
        public override void TestAfterPropertiesSet_JobDetailGiven()
        {
            const string jobName = "jobName";
            const string jobGroup = "jobGroup";
            IJobDetail jd = new JobDetailImpl(jobName, jobGroup, typeof(NoOpJob));
            cronTrigger.JobDetail = jd;
            cronTrigger.AfterPropertiesSet();
            base.TestAfterPropertiesSet_JobDetailGiven();
            Assert.AreSame(jd, cronTrigger.JobDetail, "job details weren't same");
        }

        /// <summary>
        /// Tests that JobDetail maps job data map as expected.
        /// </summary>
        [Test]
        public void TestJobDataAsMap()
        {
            IDictionary data = new Dictionary<string, object>();
            data["foo"] = "bar";
            data["number"] = 123;
            cronTrigger.JobDataAsMap = data;
            CollectionAssert.AreEquivalent(data, cronTrigger.JobDataMap, "Data differed");
        }

        /// <summary>
        /// Tests that StartDelay is respected.
        /// </summary>
        [Test]
        public void TestStartDelay()
        {
            TimeSpan expectedDelay = TimeSpan.FromMinutes(10);
            cronTrigger.StartDelay = expectedDelay;
            Assert.AreEqual(expectedDelay, cronTrigger.StartDelay);

            cronTrigger.AfterPropertiesSet();
            DateTime now = DateTime.UtcNow;
            TimeSpan delay = cronTrigger.StartTimeUtc - now;

            // check roughly
            Assert.IsTrue(delay > TimeSpan.FromMinutes(9).Add(TimeSpan.FromSeconds(55)));
            Assert.IsTrue(delay < TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(5)));
        }

    }

}
