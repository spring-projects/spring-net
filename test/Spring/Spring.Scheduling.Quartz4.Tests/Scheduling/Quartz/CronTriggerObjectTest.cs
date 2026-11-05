/*
 * Copyright 2002-2026 the original author or authors.
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

using System.Collections;
using NUnit.Framework;
using Quartz;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Scheduling.Quartz;

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
    /// Every misfire instruction of a cron trigger can be set, and ends up in the misfire instruction code.
    /// </summary>
    [TestCase(CronTriggerMisfireInstruction.SmartPolicy, 0)]
    [TestCase(CronTriggerMisfireInstruction.IgnoreMisfires, -1)]
    [TestCase(CronTriggerMisfireInstruction.FireAndProceed, 1)]
    [TestCase(CronTriggerMisfireInstruction.DoNothing, 2)]
    public void TestMisfireInstruction_WritesCode(CronTriggerMisfireInstruction instruction, int expectedCode)
    {
        cronTrigger.MisfireInstruction = instruction;

        Assert.That(cronTrigger.MisfireInstructionCode, Is.EqualTo(expectedCode));
        Assert.That(cronTrigger.MisfireInstruction, Is.EqualTo(instruction));
    }

    /// <summary>
    /// A misfire instruction of another trigger type is refused.
    /// </summary>
    [Test]
    public void TestMisfireInstruction_InvalidForTriggerType()
    {
        Assert.Throws<ArgumentException>(() => cronTrigger.MisfireInstruction = (CronTriggerMisfireInstruction) 42);
    }

    /// <summary>
    /// XML sets the misfire instruction by the name of the enum member.
    /// </summary>
    [Test]
    public void TestMisfireInstruction_SetByNameFromObjectDefinition()
    {
        StaticApplicationContext context = new StaticApplicationContext();
        context.RegisterObjectDefinition("trigger", new RootObjectDefinition(
            typeof(CronTriggerObject),
            new Spring.Objects.MutablePropertyValues(new Dictionary<string, object>
            {
                ["CronExpressionString"] = "0/1 * * * * ?",
                ["MisfireInstruction"] = "DoNothing"
            })));

        CronTriggerObject trigger = (CronTriggerObject) context.GetObject("trigger");

        Assert.That(trigger.MisfireInstruction, Is.EqualTo(CronTriggerMisfireInstruction.DoNothing));
        Assert.That(trigger.Key, Is.EqualTo(new TriggerKey("trigger", TriggerKey.DefaultGroup)));
    }

    /// <summary>
    /// Tests that JobDetail defaults values as expected in AfterPropertiesSet.
    /// </summary>
    [Test]
    public override void TestAfterPropertiesSet_JobDetailGiven()
    {
        IJobDetail jd = JobBuilder.Create().OfType(typeof(NoOpJob)).WithIdentity("jobName", "jobGroup").Build();
        cronTrigger.JobDetail = jd;
        cronTrigger.AfterPropertiesSet();
        base.TestAfterPropertiesSet_JobDetailGiven();
        Assert.That(cronTrigger.JobDetail, Is.SameAs(jd), "job details weren't same");
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
        Assert.That(cronTrigger.JobDataMap, Is.EquivalentTo(data), "Data differed");
    }

    /// <summary>
    /// Tests that StartDelay is respected.
    /// </summary>
    [Test]
    public void TestStartDelay()
    {
        TimeSpan expectedDelay = TimeSpan.FromMinutes(10);
        cronTrigger.StartDelay = expectedDelay;
        Assert.That(cronTrigger.StartDelay, Is.EqualTo(expectedDelay));

        cronTrigger.AfterPropertiesSet();
        DateTime now = DateTime.UtcNow;
        TimeSpan delay = cronTrigger.StartTimeUtc - now;

        // check roughly
        Assert.That(delay > TimeSpan.FromMinutes(9).Add(TimeSpan.FromSeconds(55)), Is.True);
        Assert.That(delay < TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(5)), Is.True);
    }
}
