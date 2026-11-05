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

using FakeItEasy;
using NUnit.Framework;
using Quartz;
using Quartz.Core;
using Quartz.Extensibility;
using Quartz.Impl.Triggers;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Unit tests for SpringObjectJobFactory.
/// </summary>
/// <author>Marko Lahma (.NET)</author>
[TestFixture]
public class SpringObjectJobFactoryTest
{
    private SpringObjectJobFactory factory;

    /// <summary>
    /// Test setup.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        factory = new SpringObjectJobFactory();
    }

    /// <summary>
    /// Tests job instane creation.
    /// </summary>
    [Test]
    public async Task TestCreateJobInstance_SimpleDefaults()
    {
        IOperableTrigger trigger = new SimpleTriggerImpl();
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(NoOpJob), trigger);

        JobScope scope = await factory.CreateJob(bundle, null);
        Assert.That(scope.Job, Is.Not.Null, "Created job was null");
    }

    /// <summary>
    /// Tests job instane creation.
    /// </summary>
    [Test]
    public async Task TestCreateJobInstance_SchedulerContextGiven()
    {
        IOperableTrigger trigger = new SimpleTriggerImpl();
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(InjectableJob), trigger);

        IDictionary<string, object> items = new Dictionary<string, object>();
        items["foo"] = "bar";
        items["number"] = 123;
        factory.SchedulerContext = new SchedulerContext(items);
        InjectableJob job = (InjectableJob) (await factory.CreateJob(bundle, null)).Job;

        Assert.That(job, Is.Not.Null, "Created job was null");
        Assert.That(job.Foo, Is.EqualTo("bar"), "string injection failed");
        Assert.That(job.Number, Is.EqualTo(123), "integer injection failed");
    }

    /// <summary>
    /// The context of the scheduler that fires the job is used when no context was handed to the factory.
    /// </summary>
    [Test]
    public async Task TestCreateJobInstance_SchedulerContextOfFiringScheduler()
    {
        IOperableTrigger trigger = new SimpleTriggerImpl();
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(InjectableJob), trigger);
        IScheduler scheduler = A.Fake<IScheduler>();
        SchedulerContext context = new SchedulerContext();
        context["foo"] = "from scheduler";
        A.CallTo(() => scheduler.Context).Returns(context);

        InjectableJob job = (InjectableJob) (await factory.CreateJob(bundle, scheduler)).Job;

        Assert.That(job.Foo, Is.EqualTo("from scheduler"));
    }

    /// <summary>
    /// The job data wins over the scheduler context, and the trigger's job data over the job's.
    /// </summary>
    [Test]
    public async Task TestCreateJobInstance_JobDataOverridesSchedulerContext()
    {
        IOperableTrigger trigger = new SimpleTriggerImpl();
        trigger.JobDataMap["number"] = 3;
        IJobDetail jobDetail = JobBuilder.Create().OfType(typeof(InjectableJob)).WithIdentity("job")
            .UsingJobData("foo", "from job data").UsingJobData("number", 2).Build();
        TriggerFiredBundle bundle = TestUtil.CreateFiredBundle(jobDetail, trigger);
        SchedulerContext context = new SchedulerContext();
        context["foo"] = "from context";
        context["number"] = 1;
        factory.SchedulerContext = context;

        InjectableJob job = (InjectableJob) (await factory.CreateJob(bundle, null)).Job;

        Assert.That(job.Foo, Is.EqualTo("from job data"));
        Assert.That(job.Number, Is.EqualTo(3));
    }

    /// <summary>
    /// Tests job instane creation.
    /// </summary>
    [Test]
    public async Task TestCreateJobInstance_IgnoredProperties()
    {
        factory.IgnoredUnknownProperties = new string[] { "foo", "baz" };
        IOperableTrigger trigger = new SimpleTriggerImpl();
        trigger.JobDataMap["foo"] = "should not be injected";
        trigger.JobDataMap["number"] = 123;
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(InjectableJob), trigger);

        InjectableJob job = (InjectableJob) (await factory.CreateJob(bundle, null)).Job;
        Assert.That(job, Is.Not.Null, "Created job was null");
        Assert.That(job.Number, Is.EqualTo(123), "integer injection failed");
        Assert.That(job.Foo, Is.Null, "foo was injected when it was not supposed to    ");
    }

    /// <summary>
    /// Job objects inject their properties themselves, so the factory leaves them alone.
    /// </summary>
    [Test]
    public async Task TestCreateJobInstance_QuartzJobObjectIsNotPopulated()
    {
        IOperableTrigger trigger = new SimpleTriggerImpl();
        trigger.JobDataMap["unknownProperty"] = "would fail to be injected strictly";
        factory.IgnoredUnknownProperties = Array.Empty<string>();
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(MethodInvokingJob), trigger);

        JobScope scope = await factory.CreateJob(bundle, null);

        Assert.That(scope.Job, Is.TypeOf<MethodInvokingJob>());
    }
}

public class NoOpJob : IJob
{
    public ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Test job object that has injectable properties
/// </summary>
public class InjectableJob : NoOpJob
{
    /// <summary>
    /// Simple int property.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Simple string property.
    /// </summary>
    public string Foo { get; set; }
}
