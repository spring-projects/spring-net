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

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Tests for <see cref="JobDetailObject" />.
/// </summary>
/// <author>Marko Lahma (.NET)</author>
[TestFixture]
public class JobDetailObjectTest
{
    private JobDetailObject jobDetail;

    /// <summary>
    /// Test setup.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        jobDetail = new JobDetailObject();
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobType_Null()
    {
        Assert.Throws<ArgumentNullException>(() => jobDetail.JobType = null);
    }

    /// <summary>
    /// Quartz 4 accepts IJob types only, so a type that is not one is refused with a message that names the way out.
    /// </summary>
    [Test]
    public void TestJobType_NonIJob()
    {
        var exception = Assert.Throws<SchedulerConfigException>(() => jobDetail.JobType = typeof(object));

        Assert.That(exception.Message, Does.Contain("System.Object"));
        Assert.That(exception.Message, Does.Contain(nameof(MethodInvokingJobDetailFactoryObject)));
        Assert.That(exception.Message, Does.Contain(nameof(DelegatingJob)));
        Assert.That(jobDetail.JobType, Is.Null);
    }

    /// <summary>
    /// A type that is not an IJob is also refused when it is set from an object definition.
    /// </summary>
    [Test]
    public void TestJobType_NonIJobFromObjectDefinition()
    {
        StaticApplicationContext context = new StaticApplicationContext();
        context.RegisterObjectDefinition("job", new Spring.Objects.Factory.Support.RootObjectDefinition(
            typeof(JobDetailObject),
            new Spring.Objects.MutablePropertyValues(new Dictionary<string, object> { ["JobType"] = typeof(object) })));

        Assert.That(() => context.GetObject("job"), Throws.Exception);
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobType_IJob()
    {
        Type CORRECT_IJOB = typeof(NoOpJob);
        jobDetail.JobType = CORRECT_IJOB;
        Assert.That(CORRECT_IJOB, Is.EqualTo(jobDetail.JobType), "JobDetail did not register correct job type");
        Assert.That(((IJobDetail) jobDetail).JobType.Type, Is.EqualTo(CORRECT_IJOB));
    }

    /// <summary>
    /// A job detail without a job type cannot be initialized.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_NoJobType()
    {
        jobDetail.ObjectName = "job";

        Assert.Throws<SchedulerConfigException>(() => jobDetail.AfterPropertiesSet());
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobDataAsMap_Null()
    {
        Assert.Throws<ArgumentException>(() => jobDetail.JobDataAsMap = null);
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobDataAsMap_ProperValues()
    {
        IDictionary values = new Hashtable();
        values["baz"] = "foo";
        values["foo"] = 123;
        values["bar"] = null;
        jobDetail.JobDataAsMap = values;
        Assert.That(jobDetail.JobDataMap.Count, Is.EqualTo(values.Count), "Data of inequal size");
        Assert.That(jobDetail.JobDataMap.Keys, Is.EquivalentTo(values.Keys), "JobDataMap values not equal");
    }

    /// <summary>
    /// The key defaults to the object name in the default group.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_Defaults()
    {
        const string objectName = "springJobDetailObject";
        jobDetail.JobType = typeof(NoOpJob);
        jobDetail.ObjectName = objectName;
        jobDetail.Group = null;
        jobDetail.AfterPropertiesSet();
        Assert.That(jobDetail.Key, Is.EqualTo(new JobKey(objectName, JobKey.DefaultGroup)));
    }

    /// <summary>
    /// A name and a group that were set win over the object name.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_CustomNameAndGroup()
    {
        const string objectName = "springJobDetailObject";
        const string jobDetailName = "jobDetailName";
        const string jobDetailGroup = "jobDetailGroup";
        jobDetail.JobType = typeof(NoOpJob);
        jobDetail.ObjectName = objectName;
        jobDetail.Name = jobDetailName;
        jobDetail.Group = jobDetailGroup;
        jobDetail.AfterPropertiesSet();
        Assert.That(jobDetail.Key, Is.EqualTo(new JobKey(jobDetailName, jobDetailGroup)));
    }

    /// <summary>
    /// A key that was set is not replaced.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_KeyGiven()
    {
        jobDetail.JobType = typeof(NoOpJob);
        jobDetail.ObjectName = "springJobDetailObject";
        jobDetail.Key = new JobKey("explicit", "explicitGroup");
        jobDetail.AfterPropertiesSet();
        Assert.That(jobDetail.Key, Is.EqualTo(new JobKey("explicit", "explicitGroup")));
    }

    /// <summary>
    /// Without a name or an object name there is nothing to name the job after.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_NoName()
    {
        jobDetail.JobType = typeof(NoOpJob);

        Assert.Throws<SchedulerConfigException>(() => jobDetail.AfterPropertiesSet());
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_ApplicationContextJobDataKeySetWithApplicationContext()
    {
        const string objectName = "springJobDetailObject";
        jobDetail.JobType = typeof(NoOpJob);
        jobDetail.ObjectName = objectName;
        StaticApplicationContext ctx = new StaticApplicationContext();
        jobDetail.ApplicationContext = ctx;
        string key = "applicationContextJobDataKey";
        jobDetail.ApplicationContextJobDataKey = key;
        jobDetail.AfterPropertiesSet();
        Assert.That(jobDetail.ApplicationContext, Is.SameAs(ctx), "ApplicationContext was not set correctly");
        Assert.That(jobDetail.JobDataMap[key], Is.SameAs(ctx), "ApplicationContext was not set to job data map");
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_ApplicationContextJobDataKeySetWithoutApplicationContext()
    {
        const string objectName = "springJobDetailObject";
        jobDetail.JobType = typeof(NoOpJob);
        jobDetail.ObjectName = objectName;
        jobDetail.ApplicationContextJobDataKey = "applicationContextJobDataKey";
        Assert.Throws<ArgumentException>(() => jobDetail.AfterPropertiesSet());
    }

    /// <summary>
    /// The concurrency and persistence flags follow the attributes of the job type.
    /// </summary>
    [Test]
    public void TestFlagsFollowJobTypeAttributes()
    {
        jobDetail.JobType = typeof(NoOpJob);
        Assert.That(jobDetail.ConcurrentExecutionDisallowed, Is.False);
        Assert.That(jobDetail.PersistJobDataAfterExecution, Is.False);

        jobDetail.JobType = typeof(StatefulJob);
        Assert.That(jobDetail.ConcurrentExecutionDisallowed, Is.True);
        Assert.That(jobDetail.PersistJobDataAfterExecution, Is.True);

        // the attributes are inherited, and an interface can carry them too
        jobDetail.JobType = typeof(DerivedStatefulJob);
        Assert.That(jobDetail.ConcurrentExecutionDisallowed, Is.True);
        jobDetail.JobType = typeof(JobWithAttributedInterface);
        Assert.That(jobDetail.PersistJobDataAfterExecution, Is.True);
        Assert.That(jobDetail.ConcurrentExecutionDisallowed, Is.True);
    }

    /// <summary>
    /// Clone copies the job data, WithJobData replaces it, and neither changes the original.
    /// </summary>
    [Test]
    public void TestCloneAndWithJobData()
    {
        jobDetail.JobType = typeof(StatefulJob);
        jobDetail.ObjectName = "job";
        jobDetail.Durable = true;
        jobDetail.JobDataMap["a"] = 1;
        jobDetail.AfterPropertiesSet();

        IJobDetail clone = jobDetail.Clone();
        clone.JobDataMap["a"] = 2;
        Assert.That(jobDetail.JobDataMap["a"], Is.EqualTo(1), "the clone shares the job data of the original");
        Assert.That(clone.Key, Is.EqualTo(jobDetail.Key));
        Assert.That(clone.Durable, Is.True);
        Assert.That(clone.ConcurrentExecutionDisallowed, Is.True);

        JobDataMap replacement = new JobDataMap { ["b"] = 3 };
        IJobDetail replaced = jobDetail.WithJobData(replacement);
        Assert.That(replaced.JobDataMap, Is.SameAs(replacement));
        Assert.That(jobDetail.JobDataMap.ContainsKey("b"), Is.False);
        Assert.That(replaced, Is.Not.SameAs(jobDetail));
        Assert.That(replaced.Key, Is.EqualTo(jobDetail.Key));
    }

    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    private class StatefulJob : NoOpJob
    {
    }

    private class DerivedStatefulJob : StatefulJob
    {
    }

    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    private interface IAttributedJob : IJob
    {
    }

    private class JobWithAttributedInterface : IAttributedJob
    {
        public ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }
}
