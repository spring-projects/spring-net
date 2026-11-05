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

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Starting a scheduler, and shutting it down with and without waiting for the jobs that are running.
/// </summary>
[TestFixture]
public class SchedulerLifecycleTests
{
    private static readonly TimeSpan s_long = TimeSpan.FromSeconds(20);

    private SchedulerFactoryObject factory;

    [SetUp]
    public void SetUp()
    {
        BlockingJob.Reset();
        factory = null;
    }

    [TearDown]
    public void TearDown()
    {
        BlockingJob.Release.Set();
        factory?.Dispose();
    }

    private async Task StartSchedulerWithBlockingJob(bool waitForJobsToComplete, ITaskExecutor taskExecutor = null)
    {
        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(BlockingJob);
        jobDetail.ObjectName = "blockingJob";
        jobDetail.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "blockingTrigger";
        trigger.JobDetail = jobDetail;
        trigger.RepeatCount = 0;
        trigger.AfterPropertiesSet();

        factory = new SchedulerFactoryObject();
        factory.WaitForJobsToCompleteOnShutdown = waitForJobsToComplete;
        factory.TaskExecutor = taskExecutor;
        factory.JobDetails = new IJobDetail[] { jobDetail };
        factory.Triggers = new ITrigger[] { trigger };
        factory.AfterPropertiesSet();
        await factory.Start();

        Assert.That(BlockingJob.Started.Wait(s_long), Is.True, "the job did not start");
    }

    /// <summary>
    /// A scheduler that waits for jobs is not shut down until the running job has finished.
    /// </summary>
    [Test]
    public async Task TestDispose_WaitsForRunningJobs()
    {
        await StartSchedulerWithBlockingJob(waitForJobsToComplete: true);

        Task disposing = Task.Run(() => factory.Dispose());
        await Task.Delay(TimeSpan.FromMilliseconds(750));
        Assert.That(disposing.IsCompleted, Is.False, "the scheduler was shut down while a job was running");

        BlockingJob.Release.Set();

        await disposing.WaitAsync(s_long);
        Assert.That(BlockingJob.Completed, Is.True);
    }

    /// <summary>
    /// A scheduler that does not wait is shut down while the job is still running.
    /// </summary>
    [Test]
    public async Task TestDispose_DoesNotWaitForRunningJobsByDefault()
    {
        await StartSchedulerWithBlockingJob(waitForJobsToComplete: false);

        Task disposing = Task.Run(() => factory.Dispose());
        await disposing.WaitAsync(s_long);

        Assert.That(BlockingJob.Completed, Is.False, "the factory waited for a job that it was not asked to wait for");
    }

    /// <summary>
    /// The Spring task executor runs the job, and shutting down with a wait covers the work that it runs.
    /// </summary>
    [Test]
    public async Task TestDispose_WaitsForJobsRunByTheTaskExecutor()
    {
        await StartSchedulerWithBlockingJob(waitForJobsToComplete: true, new ThreadTaskExecutor());

        Task disposing = Task.Run(() => factory.Dispose());
        await Task.Delay(TimeSpan.FromMilliseconds(750));
        Assert.That(disposing.IsCompleted, Is.False, "the scheduler was shut down while a job was running");

        BlockingJob.Release.Set();

        await disposing.WaitAsync(s_long);
        Assert.That(BlockingJob.Completed, Is.True);
    }

    public sealed class BlockingJob : IJob
    {
        public static ManualResetEventSlim Started { get; private set; } = new ManualResetEventSlim();

        public static ManualResetEventSlim Release { get; private set; } = new ManualResetEventSlim();

        public static bool Completed { get; private set; }

        public static void Reset()
        {
            Started = new ManualResetEventSlim();
            Release = new ManualResetEventSlim();
            Completed = false;
        }

        public ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            Started.Set();
            Release.Wait(s_long);
            Completed = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThreadTaskExecutor : ITaskExecutor
    {
        public void Execute(ThreadStart runnable)
        {
            new Thread(runnable) { IsBackground = true }.Start();
        }
    }
}
