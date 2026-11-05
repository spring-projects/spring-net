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
using Spring.Objects.Factory.Support;

namespace Spring.Scheduling.Quartz;

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
    /// Every misfire instruction of a simple trigger can be set, and ends up in the misfire instruction code.
    /// </summary>
    [TestCase(SimpleTriggerMisfireInstruction.SmartPolicy, 0)]
    [TestCase(SimpleTriggerMisfireInstruction.IgnoreMisfires, -1)]
    [TestCase(SimpleTriggerMisfireInstruction.FireNow, 1)]
    [TestCase(SimpleTriggerMisfireInstruction.NowWithExistingCount, 2)]
    [TestCase(SimpleTriggerMisfireInstruction.NowWithRemainingCount, 3)]
    [TestCase(SimpleTriggerMisfireInstruction.NextWithRemainingCount, 4)]
    [TestCase(SimpleTriggerMisfireInstruction.NextWithExistingCount, 5)]
    public void TestMisfireInstruction_WritesCode(SimpleTriggerMisfireInstruction instruction, int expectedCode)
    {
        simpleTrigger.MisfireInstruction = instruction;

        Assert.That(simpleTrigger.MisfireInstructionCode, Is.EqualTo(expectedCode));
        Assert.That(simpleTrigger.MisfireInstruction, Is.EqualTo(instruction));
    }

    /// <summary>
    /// A misfire instruction that does not exist is refused.
    /// </summary>
    [Test]
    public void TestMisfireInstruction_Invalid()
    {
        Assert.Throws<ArgumentException>(() => simpleTrigger.MisfireInstruction = (SimpleTriggerMisfireInstruction) 42);
    }

    /// <summary>
    /// XML sets the misfire instruction by the name of the enum member.
    /// </summary>
    [Test]
    public void TestMisfireInstruction_SetByNameFromObjectDefinition()
    {
        StaticApplicationContext context = new StaticApplicationContext();
        context.RegisterObjectDefinition("trigger", new RootObjectDefinition(
            typeof(SimpleTriggerObject),
            new Spring.Objects.MutablePropertyValues(new Dictionary<string, object>
            {
                ["MisfireInstruction"] = "NextWithRemainingCount"
            })));

        SimpleTriggerObject trigger = (SimpleTriggerObject) context.GetObject("trigger");

        Assert.That(trigger.MisfireInstruction, Is.EqualTo(SimpleTriggerMisfireInstruction.NextWithRemainingCount));
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
        IJobDetail jd = JobBuilder.Create().OfType(typeof(NoOpJob)).WithIdentity(jobName, jobGroup).Build();
        simpleTrigger.JobDetail = jd;
        simpleTrigger.AfterPropertiesSet();
        base.TestAfterPropertiesSet_JobDetailGiven();
        Assert.That(simpleTrigger.JobDetail, Is.SameAs(jd), "job details weren't same");
    }

    /// <summary>
    /// The trigger repeats indefinitely unless told otherwise.
    /// </summary>
    [Test]
    public void TestRepeatsIndefinitelyByDefault()
    {
        Assert.That(simpleTrigger.RepeatCount, Is.EqualTo(SimpleTriggerObject.RepeatIndefinitely));
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
        Assert.That(simpleTrigger.JobDataMap, Is.EquivalentTo(data), "Data differed");
    }
}
