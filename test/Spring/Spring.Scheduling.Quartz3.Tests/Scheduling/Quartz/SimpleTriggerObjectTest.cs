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

using Quartz.Impl;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Tests for <see cref="SimpleTriggerObject" />.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class SimpleTriggerObjectTest : TriggerObjectTest
    {
        private SimpleTriggerObject simpleTrigger;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            simpleTrigger = new SimpleTriggerObject();
            simpleTrigger.ObjectName = TRIGGER_NAME;
            Trigger = simpleTrigger;
        }

        /// <summary>
        /// Tests all possible misfire instructions for cron trigger 
        /// from strings to int.
        /// </summary>
        [Test]
        public void TestMisfireInstructionNames()
        {
            string[] names = new string[] { "FireNow", "RescheduleNextWithExistingCount", "RescheduleNextWithRemainingCount", "RescheduleNowWithExistingRepeatCount", "RescheduleNowWithRemainingRepeatCount", "SmartPolicy" };
            foreach (string name in names)
            {
                simpleTrigger.MisfireInstructionName = name;                
            }
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public override void TestAfterPropertiesSet_Defaults()
        {
            simpleTrigger.AfterPropertiesSet();
            base.TestAfterPropertiesSet_Defaults();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public override void TestAfterPropertiesSet_ValuesGiven()
        {
            simpleTrigger.StartDelay = TimeSpan.FromMilliseconds(100);
            simpleTrigger.AfterPropertiesSet();
            base.TestAfterPropertiesSet_ValuesGiven();
        }
        
        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_StartDelayGiven()
        {
            const int START_DELAY = 100000;
            simpleTrigger.StartDelay = TimeSpan.FromMilliseconds(START_DELAY);
            DateTime startTime = DateTime.UtcNow;
            simpleTrigger.AfterPropertiesSet();
            AssertDateTimesEqualityWithAllowedDelta(startTime.AddMilliseconds(START_DELAY), simpleTrigger.StartTimeUtc, 1500);
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public override void TestAfterPropertiesSet_JobDetailGiven()
        {
            const string jobName = "jobName";
            const string jobGroup = "jobGroup";
            JobDetailImpl jd = new JobDetailImpl(jobName, jobGroup, typeof(NoOpJob));
            simpleTrigger.JobDetail = jd;
            simpleTrigger.AfterPropertiesSet();
            base.TestAfterPropertiesSet_JobDetailGiven();
            Assert.AreSame(jd, simpleTrigger.JobDetail, "job details weren't same");
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestJobDataAsMap()
        {
            IDictionary data = new Dictionary<string, object>();
            data["foo"] = "bar";
            data["number"] = 123;
            simpleTrigger.JobDataAsMap = data;
            CollectionAssert.AreEquivalent(data, simpleTrigger.JobDataMap, "Data differed");
        }
    }
}
