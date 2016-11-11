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

using Spring.Objects.Factory;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Base class for testing triggers. Contains common functionality.
    /// </summary>
    [TestFixture]
    public abstract class TriggerObjectTest
    {
        private IOperableTrigger trigger;

        /// <summary>
        /// Constant name for tested triggers.
        /// </summary>
        protected const string TRIGGER_NAME = "trigger";

        /// <summary>
        /// TriggerObject under test.
        /// </summary>
        protected IOperableTrigger Trigger
        {
            set { trigger = value; }
        }

        /// <summary>
        /// Tests that TriggerObject defaults values as expected in AfterPropertiesSet.
        /// </summary>
        [Test]
        public virtual void TestAfterPropertiesSet_Defaults()
        {
            ((IInitializingObject) trigger).AfterPropertiesSet();

            Assert.AreEqual(TRIGGER_NAME, trigger.Key.Name, "trigger name mismatch");
            Assert.AreEqual(SchedulerConstants.DefaultGroup, trigger.Key.Group, "trigger group name mismatch");
            Assert.IsNull(trigger.JobKey, "trigger job name not null");
            AssertDateTimesEqualityWithAllowedDelta(DateTime.UtcNow, trigger.StartTimeUtc, 1500);
        }

        /// <summary>
        /// Tests that TriggerObject defaults values as expected in AfterPropertiesSet.
        /// </summary>
        [Test]
        public virtual void TestAfterPropertiesSet_ValuesGiven()
        {
            ((IInitializingObject)trigger).AfterPropertiesSet();

            const string NAME = "newName";
            const string GROUP = "newGroup";
            DateTime START_TIME = new DateTime(1982, 6, 28, 13, 10, 0);
            trigger.StartTimeUtc = START_TIME;
            trigger.Key = new TriggerKey(NAME, GROUP);
            Assert.AreEqual(NAME, trigger.Key.Name, "trigger name mismatch");
            Assert.AreEqual(GROUP, trigger.Key.Group, "trigger group name mismatch");
            AssertDateTimesEqualityWithAllowedDelta(START_TIME, trigger.StartTimeUtc, 1500);
        }

        /// <summary>
        /// Tests that TriggerObject defaults values as expected in AfterPropertiesSet.
        /// </summary>
        [Test]
        public virtual void TestAfterPropertiesSet_JobDetailGiven()
        {
            ((IInitializingObject)trigger).AfterPropertiesSet();
            
            const string jobName = "jobName";
            const string jobGroup = "jobGroup";

            Assert.AreEqual(jobName, trigger.JobKey.Name, "trigger job name was not from job detail");
            Assert.AreEqual(jobGroup, trigger.JobKey.Group, "trigger job group was not from job detail");
        }

        /// <summary>
        /// Tests whether two datetimes are close enough.
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="allowedDeltaInMilliseconds"></param>
        protected static void AssertDateTimesEqualityWithAllowedDelta(DateTimeOffset d1, DateTimeOffset d2, int allowedDeltaInMilliseconds)
        {
            int diffInMillis = (int) Math.Abs((d1 - d2).TotalMilliseconds);
            Assert.LessOrEqual(diffInMillis, allowedDeltaInMilliseconds, "too much difference in times");
        }
    }
}
