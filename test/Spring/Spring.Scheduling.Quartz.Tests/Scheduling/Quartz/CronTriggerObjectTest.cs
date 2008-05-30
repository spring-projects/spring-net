/*
* Copyright 2002-2005 the original author or authors.
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
using Quartz.Job;

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

        [Test]
        public override void TestAfterPropertiesSet_Defaults()
        {
            cronTrigger.AfterPropertiesSet();
            base.TestAfterPropertiesSet_Defaults();
            AssertDateTimesEqualityWithAllowedDelta(DateTime.UtcNow, cronTrigger.StartTimeUtc, 1000);
            Assert.AreEqual(TimeZone.CurrentTimeZone, cronTrigger.TimeZone, "trigger time zone mismatch");
        }

        [Test]
        public override void TestAfterPropertiesSet_ValuesGiven()
        {
            TimeZone TZ = TimeZone.CurrentTimeZone;
            cronTrigger.TimeZone = TZ;
            cronTrigger.AfterPropertiesSet();
            base.TestAfterPropertiesSet_ValuesGiven();
            Assert.AreSame(TZ, cronTrigger.TimeZone, "trigger time zone mismatch");
        }

        
        [Test]
        public override void TestAfterPropertiesSet_JobDetailGiven()
        {
            const string jobName = "jobName";
            const string jobGroup = "jobGroup";
            JobDetail jd = new JobDetail(jobName, jobGroup, typeof (NoOpJob));
            cronTrigger.JobDetail = jd;
            cronTrigger.AfterPropertiesSet();
            base.TestAfterPropertiesSet_JobDetailGiven();
            Assert.AreSame(jd, cronTrigger.JobDetail, "job details weren't same");
        }


    }

}
