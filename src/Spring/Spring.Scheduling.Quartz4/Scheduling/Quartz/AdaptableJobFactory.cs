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

using Quartz;
using Quartz.Extensibility;
using Spring.Util;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// JobFactory implementation that supports <see cref="ThreadStart" />
/// objects as well as standard Quartz <see cref="IJob" /> instances.
/// </summary>
/// <remarks>
/// A job that implements <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/> is disposed when Quartz returns it
/// to the factory, provided that the factory created it. This is a change from Quartz 3, where
/// <c>ReturnJob</c> did nothing.
/// </remarks>
/// <author>Juergen Hoeller</author>
/// <author>Marko Lahma (.NET)</author>
/// <seealso cref="DelegatingJob" />
/// <seealso cref="AdaptJob(object)" />
public class AdaptableJobFactory : IJobFactory
{
    /// <summary>
    /// Called by the scheduler at the time of the trigger firing, in order to
    /// produce a <see cref="IJob"/> instance on which to call Execute.
    /// </summary>
    /// <remarks>
    /// It should be extremely rare for this method to throw an exception -
    /// basically only the the case where there is no way at all to instantiate
    /// and prepare the Job for execution.  When the exception is thrown, the
    /// Scheduler will move all triggers associated with the Job into the
    /// <see cref="TriggerState.Error"/> state, which will require human
    /// intervention (e.g. an application restart after fixing whatever
    /// configuration problem led to the issue wih instantiating the Job.
    /// </remarks>
    /// <param name="bundle">The TriggerFiredBundle from which the <see cref="IJobDetail"/>
    /// and other info relating to the trigger firing can be obtained.</param>
    /// <param name="scheduler">The scheduler instance.</param>
    /// <param name="cancellationToken">The cancellation token. The job is created synchronously, so it is not observed.</param>
    /// <returns>The scope that holds the newly instantiated job.</returns>
    /// <throws>SchedulerException if there is a problem instantiating the Job.</throws>
    public virtual ValueTask<JobScope> CreateJob(TriggerFiredBundle bundle, IScheduler scheduler, CancellationToken cancellationToken = default)
    {
        // Kept synchronous: Quartz documents that an async body would discard the AsyncLocal values that the job's
        // constructor or property setters put in place.
        try
        {
            object jobObject = CreateJobInstance(bundle, scheduler);
            IJob job = AdaptJob(jobObject);
            return new ValueTask<JobScope>(new JobScope(job, new CreatedJob(jobObject)));
        }
        catch (Exception ex)
        {
            throw new SchedulerException("Job instantiation failed", ex);
        }
    }

    /// <summary>
    /// Allows the the job factory to destroy/cleanup the job if needed.
    /// </summary>
    /// <remarks>
    /// Disposes the job object if this factory created it and it is <see cref="IAsyncDisposable"/> or
    /// <see cref="IDisposable"/>. A scope that another factory (or a subclass that overrides
    /// <see cref="CreateJob"/>) created carries a different state and is left alone.
    /// </remarks>
    public virtual async ValueTask ReturnJob(JobScope scope, CancellationToken cancellationToken = default)
    {
        if (scope.State is not CreatedJob created)
        {
            return;
        }

        if (created.Instance is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (created.Instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Create an instance of the specified job class.
    /// <p>
    /// Can be overridden to post-process the job instance. The instance should be a new one for every call: it is disposed
    /// when the job has run, if it is disposable.
    /// </p>
    /// </summary>
    /// <param name="bundle">
    /// The TriggerFiredBundle from which the JobDetail
    /// and other info relating to the trigger firing can be obtained.
    /// </param>
    /// <param name="scheduler">The scheduler instance.</param>
    /// <returns>The job instance.</returns>
    protected virtual object CreateJobInstance(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return ObjectUtils.InstantiateType(bundle.JobDetail.JobType.Type);
    }

    /// <summary>
    /// Adapt the given job object to the Quartz Job interface.
    /// </summary>
    /// <remarks>
    /// The default implementation supports straight Quartz Jobs
    /// as well as Runnables, which get wrapped in a DelegatingJob.
    /// </remarks>
    /// <param name="jobObject">
    /// The original instance of the specified job class.
    /// </param>
    /// <returns>The adapted Quartz Job instance.</returns>
    /// <seealso cref="DelegatingJob" />
    protected virtual IJob AdaptJob(object jobObject)
    {
        if (jobObject is IJob job)
        {
            return job;
        }

        if (jobObject is ThreadStart start)
        {
            return new DelegatingJob(start);
        }

        if (jobObject is IThreadRunnable threadRunnable)
        {
            return new DelegatingJob(() => threadRunnable.Run().ConfigureAwait(false).GetAwaiter().GetResult());
        }

        if (jobObject is Func<Task> func)
        {
            return new DelegatingJob(() => func().ConfigureAwait(false).GetAwaiter().GetResult());
        }

        string message =
            $"Unable to execute job class [{jobObject.GetType().FullName}]: only [IJob], [IThreadRunnable], [ThreadStart] and [Func<Task>] supported.";
        throw new ArgumentException(message);
    }

    /// <summary>
    /// The state that <see cref="CreateJob"/> puts in the <see cref="JobScope"/>: the object that this factory created,
    /// which can differ from the job when it was adapted.
    /// </summary>
    private sealed class CreatedJob
    {
        public CreatedJob(object instance)
        {
            Instance = instance;
        }

        public object Instance { get; }
    }
}
