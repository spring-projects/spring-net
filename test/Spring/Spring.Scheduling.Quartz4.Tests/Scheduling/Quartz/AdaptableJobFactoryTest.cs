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
/// Tests for <see cref="AdaptableJobFactory" />.
/// </summary>
/// <author>Marko Lahma (.NET)</author>
[TestFixture]
public class AdaptableJobFactoryTest
{
    private AdaptableJobFactory jobFactory;

    /// <summary>
    /// Test setup.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        jobFactory = new AdaptableJobFactory();
    }

    /// <summary>
    /// An object that is not a job and cannot be adapted to one is refused, with the reason as the inner exception.
    /// </summary>
    [Test]
    public void TestCreateJob_IncompatibleJob()
    {
        var factory = new FixedInstanceJobFactory(new object());
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(NoOpJob));

        var exception = Assert.Throws<SchedulerException>(() => factory.CreateJob(bundle, null));

        Assert.That(exception.InnerException, Is.TypeOf<ArgumentException>());
    }

    /// <summary>
    /// A runnable is adapted to a job that runs it.
    /// </summary>
    [Test]
    public async Task TestCreateJob_ThreadRunnable()
    {
        var runnable = new CountingRunnable();
        var factory = new FixedInstanceJobFactory(runnable);
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(NoOpJob));

        JobScope scope = await factory.CreateJob(bundle, null);

        Assert.That(scope.Job, Is.TypeOf<DelegatingJob>());
        await scope.Job.Execute(null);
        Assert.That(runnable.Count, Is.EqualTo(1));
    }

    /// <summary>
    /// A ThreadStart is adapted to a job that runs it.
    /// </summary>
    [Test]
    public async Task TestCreateJob_ThreadStartJob()
    {
        int count = 0;
        var factory = new FixedInstanceJobFactory(new ThreadStart(() => count++));
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(NoOpJob));

        JobScope scope = await factory.CreateJob(bundle, null);

        await scope.Job.Execute(null);
        Assert.That(count, Is.EqualTo(1));
    }

    /// <summary>
    /// Tests job creation.
    /// </summary>
    [Test]
    public async Task TestCreateJob_NormalIJob()
    {
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(NoOpJob));
        JobScope scope = await jobFactory.CreateJob(bundle, null);

        Assert.That(scope.Job, Is.TypeOf<NoOpJob>(), "Returned job was not created from the job type");
    }

    /// <summary>
    /// Every execution gets a job of its own.
    /// </summary>
    [Test]
    public async Task TestCreateJob_NewInstanceEveryTime()
    {
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(NoOpJob));

        JobScope first = await jobFactory.CreateJob(bundle, null);
        JobScope second = await jobFactory.CreateJob(bundle, null);

        Assert.That(first.Job, Is.Not.SameAs(second.Job));
    }

    /// <summary>
    /// A job that this factory created is disposed when it is returned.
    /// </summary>
    [Test]
    public async Task TestReturnJob_DisposesDisposableJobThatTheFactoryCreated()
    {
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(DisposableJob));
        JobScope scope = await jobFactory.CreateJob(bundle, null);
        DisposableJob job = (DisposableJob) scope.Job;
        Assert.That(job.Disposed, Is.False);

        await jobFactory.ReturnJob(scope);

        Assert.That(job.Disposed, Is.True);
    }

    /// <summary>
    /// An async disposable job is disposed asynchronously.
    /// </summary>
    [Test]
    public async Task TestReturnJob_DisposesAsyncDisposableJob()
    {
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(AsyncDisposableJob));
        JobScope scope = await jobFactory.CreateJob(bundle, null);
        AsyncDisposableJob job = (AsyncDisposableJob) scope.Job;

        await jobFactory.ReturnJob(scope);

        Assert.That(job.AsyncDisposed, Is.True);
        Assert.That(job.Disposed, Is.False, "the async disposal is preferred");
    }

    /// <summary>
    /// A job that another factory created is not the business of this one.
    /// </summary>
    [Test]
    public async Task TestReturnJob_DoesNotDisposeJobThatTheFactoryDidNotCreate()
    {
        DisposableJob job = new DisposableJob();

        await jobFactory.ReturnJob(new JobScope(job));
        await jobFactory.ReturnJob(new JobScope(job, new object()));

        Assert.That(job.Disposed, Is.False);
    }

    /// <summary>
    /// When the runnable that was adapted is disposable, it is the runnable that is disposed.
    /// </summary>
    [Test]
    public async Task TestReturnJob_DisposesTheCreatedObjectBehindAnAdaptedJob()
    {
        var runnable = new DisposableRunnable();
        var factory = new FixedInstanceJobFactory(runnable);
        TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(NoOpJob));
        JobScope scope = await factory.CreateJob(bundle, null);

        await factory.ReturnJob(scope);

        Assert.That(runnable.Disposed, Is.True);
    }

    private sealed class FixedInstanceJobFactory : AdaptableJobFactory
    {
        private readonly object instance;

        public FixedInstanceJobFactory(object instance)
        {
            this.instance = instance;
        }

        protected override object CreateJobInstance(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return instance;
        }
    }

    private class CountingRunnable : IThreadRunnable
    {
        public int Count { get; private set; }

        public Task Run()
        {
            Count++;
            return Task.CompletedTask;
        }
    }

    private sealed class DisposableRunnable : CountingRunnable, IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    public sealed class DisposableJob : NoOpJob, IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    public sealed class AsyncDisposableJob : NoOpJob, IAsyncDisposable, IDisposable
    {
        public bool Disposed { get; private set; }

        public bool AsyncDisposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }

        public ValueTask DisposeAsync()
        {
            AsyncDisposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
