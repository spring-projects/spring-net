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

using NUnit.Framework;
using Quartz;
using Quartz.Extensibility;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Which job factory a scheduler gets: the one that is set, else the one that the callback registers, else the default.
/// </summary>
[TestFixture]
public class JobFactoryPrecedenceTests
{
    private SchedulerFactoryObject factory;

    [TearDown]
    public void TearDown()
    {
        factory?.Dispose();
    }

    private async Task RunOneJob(Action<SchedulerFactoryObject> configure)
    {
        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(NoOpJob);
        jobDetail.ObjectName = "job";
        jobDetail.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "trigger";
        trigger.JobDetail = jobDetail;
        trigger.RepeatCount = 0;
        trigger.AfterPropertiesSet();

        factory = new SchedulerFactoryObject();
        factory.JobDetails = new IJobDetail[] { jobDetail };
        factory.Triggers = new ITrigger[] { trigger };
        configure(factory);
        factory.AfterPropertiesSet();
        await factory.Start();
    }

    private static bool WaitFor(CountingJobFactory jobFactory)
    {
        return SpinWait.SpinUntil(() => jobFactory.Created > 0, TimeSpan.FromSeconds(10));
    }

    [Test]
    public async Task TestCallbackJobFactoryIsUsedWhenJobFactoryIsNotSet()
    {
        CountingJobFactory fromCallback = new CountingJobFactory();

        await RunOneJob(f => f.ConfigureQuartz = quartz => quartz.UseJobFactory(fromCallback));

        Assert.That(WaitFor(fromCallback), Is.True, "the job factory of the callback was not used");
    }

    [Test]
    public async Task TestExplicitJobFactoryWinsOverTheCallback()
    {
        CountingJobFactory explicitFactory = new CountingJobFactory();
        CountingJobFactory fromCallback = new CountingJobFactory();

        await RunOneJob(f =>
        {
            f.JobFactory = explicitFactory;
            f.ConfigureQuartz = quartz => quartz.UseJobFactory(fromCallback);
        });

        Assert.That(WaitFor(explicitFactory), Is.True, "the job factory that was set was not used");
        Assert.That(fromCallback.Created, Is.EqualTo(0), "Quartz keeps the first registration, which is the explicit one");
    }

    [Test]
    public async Task TestNullJobFactoryLeavesTheChoiceToTheCallback()
    {
        CountingJobFactory fromCallback = new CountingJobFactory();

        await RunOneJob(f =>
        {
            f.JobFactory = null;
            f.ConfigureQuartz = quartz => quartz.UseJobFactory(fromCallback);
        });

        Assert.That(WaitFor(fromCallback), Is.True);
    }

    [Test]
    public async Task TestDefaultJobFactoryRunsTheJobWhenNothingElseIsRegistered()
    {
        DefaultCheckJob.Ran.Reset();
        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(DefaultCheckJob);
        jobDetail.ObjectName = "defaultJob";
        jobDetail.AfterPropertiesSet();
        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "defaultTrigger";
        trigger.JobDetail = jobDetail;
        trigger.RepeatCount = 0;
        trigger.AfterPropertiesSet();
        factory = new SchedulerFactoryObject();
        factory.JobDetails = new IJobDetail[] { jobDetail };
        factory.Triggers = new ITrigger[] { trigger };
        factory.AfterPropertiesSet();
        await factory.Start();

        Assert.That(DefaultCheckJob.Ran.Wait(TimeSpan.FromSeconds(10)), Is.True);
    }

    public sealed class DefaultCheckJob : IJob
    {
        public static ManualResetEventSlim Ran { get; } = new ManualResetEventSlim();

        public ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            Ran.Set();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CountingJobFactory : AdaptableJobFactory
    {
        private int created;

        public int Created => Volatile.Read(ref created);

        public override ValueTask<JobScope> CreateJob(TriggerFiredBundle bundle, IScheduler scheduler, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref created);
            return base.CreateJob(bundle, scheduler, cancellationToken);
        }
    }
}
