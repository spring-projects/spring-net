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

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Base class for testing triggers. Contains common functionality.
    /// </summary>
    [TestFixture]
    public abstract class TriggerObjectTest
    {
        private Trigger trigger;
        protected const string TRIGGER_NAME = "trigger";


        protected Trigger Trigger
        {
            set { trigger = value; }
        }

        [Test]
        public virtual void TestAfterPropertiesSet_Defaults()
        {
            Assert.AreEqual(TRIGGER_NAME, trigger.Name, "trigger name mismatch");
            Assert.AreEqual(SchedulerConstants.DEFAULT_GROUP, trigger.Group, "trigger group name mismatch");
            AssertDateTimesEqualityWithAllowedDelta(DateTime.UtcNow, trigger.StartTimeUtc, 1000);
            Assert.IsNull(trigger.JobName, "trigger job name not null");
            Assert.AreEqual(SchedulerConstants.DEFAULT_GROUP, trigger.JobGroup, "trigger job group was not default");
        }

        [Test]
        public virtual void TestAfterPropertiesSet_ValuesGiven()
        {
            const string NAME = "newName";
            const string GROUP = "newGroup";
            DateTime START_TIME = new DateTime(10000000);
            trigger.Name = NAME;
            trigger.Group = GROUP;
            trigger.StartTimeUtc = START_TIME;
            Assert.AreEqual(NAME, trigger.Name, "trigger name mismatch");
            Assert.AreEqual(GROUP, trigger.Group, "trigger group name mismatch");
            AssertDateTimesEqualityWithAllowedDelta(START_TIME, trigger.StartTimeUtc, 1000);
        }

        
        [Test]
        public virtual void TestAfterPropertiesSet_JobDetailGiven()
        {
            const string jobName = "jobName";
            const string jobGroup = "jobGroup";
            Assert.AreEqual(jobName, trigger.JobName, "trigger job name was not from job detail");
            Assert.AreEqual(jobGroup, trigger.JobGroup, "trigger job group was not from job detail");
        }

        [Test]
        public virtual void TestTriggerListenerNames_Valis()
        {
            string[] LISTENER_NAMES = new string[] {"Foo", "Bar", "Baz"};
            trigger.TriggerListenerNames = LISTENER_NAMES;
            CollectionAssert.AreEqual(LISTENER_NAMES, trigger.TriggerListenerNames, "Trigger listeners were not equal");
        }

        protected static void AssertDateTimesEqualityWithAllowedDelta(DateTime d1, DateTime d2, int allowedDeltaInMilliseconds)
        {
            int diffInMillis = (int) Math.Abs((d1 - d2).TotalMilliseconds);
            Assert.LessOrEqual(diffInMillis, allowedDeltaInMilliseconds, "too much difference in times");
        }
    }

}
