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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Subclass of SchedulerFactoryObject for testing purposes: it returns the scheduler it was given instead of building one,
/// and keeps the configuration that the Quartz builder received so that a test can read it back.
/// </summary>
public class TestSchedulerFactoryObject : SchedulerFactoryObject
{
    private readonly IScheduler sched;
    private ServiceProvider provider;

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    /// <param name="sched"></param>
    public TestSchedulerFactoryObject(IScheduler sched)
    {
        this.sched = sched;
    }

    /// <summary>
    /// The services of the Quartz builder, as configured by <see cref="SchedulerFactoryObject"/>.
    /// </summary>
    public IServiceCollection BuilderServices { get; private set; }

    /// <summary>
    /// Creates a scheduler actually returning the scheduler this intance
    /// was instantiated with.
    /// </summary>
    protected override IScheduler CreateScheduler(QuartzSchedulerBuilder builder)
    {
        return sched;
    }

    /// <inheritdoc />
    protected override void ConfigureBuilder(IQuartzBuilder quartz)
    {
        base.ConfigureBuilder(quartz);
        BuilderServices = quartz.Services;
    }

    /// <summary>
    /// Reads the options of a type as the Quartz builder has them configured.
    /// </summary>
    public T GetOptions<T>() where T : class
    {
        provider ??= BuilderServices.BuildServiceProvider();
        return provider.GetRequiredService<IOptionsMonitor<T>>().Get(Options.DefaultName);
    }

    /// <summary>
    /// Creates a fake scheduler that finds no jobs and no triggers, and has a scheduler context.
    /// </summary>
    public static IScheduler CreateFakeScheduler()
    {
        IScheduler scheduler = A.Fake<IScheduler>();
        A.CallTo(() => scheduler.Context).Returns(new SchedulerContext());
        A.CallTo(() => scheduler.SchedulerName).Returns("fake scheduler");
        A.CallTo(() => scheduler.GetTrigger(A<TriggerKey>._, A<CancellationToken>._)).Returns(new ValueTask<ITrigger>((ITrigger) null));
        A.CallTo(() => scheduler.GetJobDetail(A<JobKey>._, A<CancellationToken>._)).Returns(new ValueTask<IJobDetail>((IJobDetail) null));
        return scheduler;
    }

    /// <summary>
    /// Disposes the services that <see cref="GetOptions{T}"/> built.
    /// </summary>
    public void DisposeServices()
    {
        provider?.Dispose();
        provider = null;
    }
}
