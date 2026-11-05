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

using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Extensibility;
using Spring.Util;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Quartz ThreadPool adapter that delegates to a Spring-managed
/// TaskExecutor instance, specified on SchedulerFactoryObject.
/// </summary>
/// <remarks>
/// The pool counts the work items that are running, and <see cref="Shutdown"/> and <see cref="Drain"/> wait for the
/// count to reach zero. A work item covers the whole execution of a job including the job store update that completes
/// its trigger, so a persistent store has finished writing when they return.
/// <p>
/// The executor itself is unbounded as far as this class knows. <see cref="PoolSize"/> is what Quartz is told: it sizes the
/// batch of triggers that the scheduler acquires, and it bounds the work items that this pool lets run at the same time.
/// </p>
/// </remarks>
/// <author>Juergen Hoeller</author>
/// <seealso cref="SchedulerFactoryObject.TaskExecutor" />
public class LocalTaskExecutorThreadPool : IThreadPool
{
    /// <summary>
    /// The default for <see cref="PoolSize"/>, the same as Quartz's own default thread pool.
    /// </summary>
    public const int DefaultPoolSize = 10;

    private readonly ITaskExecutor taskExecutor;
    private readonly object sync = new();

    private int poolSize = DefaultPoolSize;
    private int inFlight;
    private bool shutdown;

    // Completed (and cleared) when the last running work item ends.
    private TaskCompletionSource<bool> idle;

    // Completed (and cleared) when any running work item ends.
    private TaskCompletionSource<bool> slotFreed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalTaskExecutorThreadPool"/> class.
    /// </summary>
    /// <param name="taskExecutor">The Spring task executor that runs the jobs.</param>
    public LocalTaskExecutorThreadPool(ITaskExecutor taskExecutor)
    {
        AssertUtils.ArgumentNotNull(taskExecutor, nameof(taskExecutor));
        this.taskExecutor = taskExecutor;
        Logger = LogManager.GetLogger(GetType());
    }

    /// <summary>
    /// Logger instance.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// The number of jobs that Quartz may run at the same time through this pool. Default is
    /// <see cref="DefaultPoolSize"/>.
    /// </summary>
    public virtual int PoolSize
    {
        get
        {
            lock (sync)
            {
                return poolSize;
            }
        }

        set
        {
            AssertUtils.State(value > 0, "Pool size must be greater than zero.");
            lock (sync)
            {
                poolSize = value;
            }
        }
    }

    /// <summary>
    /// The number of work items that have been handed to the executor and have not finished.
    /// </summary>
    public virtual int ActiveCount
    {
        get
        {
            lock (sync)
            {
                return inFlight;
            }
        }
    }

    /// <inheritdoc />
    public virtual ValueTask Initialize(CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async ValueTask<int> WaitForAvailableThreads(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            TaskCompletionSource<bool> waiter;
            lock (sync)
            {
                int available = Math.Max(0, poolSize - inFlight);
                if (available > 0 || shutdown)
                {
                    return available;
                }

                slotFreed ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                waiter = slotFreed;
            }

            // Not blocking: the scheduler's own loop awaits this.
            await waiter.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual ValueTask<bool> TryRun(Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        if (action == null)
        {
            return new ValueTask<bool>(false);
        }

        lock (sync)
        {
            if (shutdown)
            {
                return new ValueTask<bool>(false);
            }

            inFlight++;
        }

        try
        {
            taskExecutor.Execute(() => RunAndRelease(action));
            return new ValueTask<bool>(true);
        }
        catch (TaskRejectedException ex)
        {
            Release();
            Logger.LogError(ex, "Task has been rejected by TaskExecutor");
            return new ValueTask<bool>(false);
        }
        catch
        {
            Release();
            throw;
        }
    }

    /// <inheritdoc />
    public virtual async ValueTask Shutdown(bool waitForJobsToComplete = true, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            shutdown = true;
        }

        if (waitForJobsToComplete)
        {
            await WaitForIdle(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual async ValueTask<bool> Drain(CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            shutdown = true;
        }

        try
        {
            await WaitForIdle(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private void RunAndRelease(Func<ValueTask> action)
    {
        try
        {
            // The executor hands over a thread, and the work item occupies it until the job has finished: the Spring
            // executor contract is synchronous.
            action().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        finally
        {
            Release();
        }
    }

    private void Release()
    {
        TaskCompletionSource<bool> freed;
        TaskCompletionSource<bool> drained = null;
        lock (sync)
        {
            inFlight--;
            freed = slotFreed;
            slotFreed = null;
            if (inFlight == 0)
            {
                drained = idle;
                idle = null;
            }
        }

        freed?.TrySetResult(true);
        drained?.TrySetResult(true);
    }

    private Task WaitForIdle(CancellationToken cancellationToken)
    {
        TaskCompletionSource<bool> waiter;
        lock (sync)
        {
            if (inFlight == 0)
            {
                return Task.CompletedTask;
            }

            idle ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            waiter = idle;
        }

        return waiter.Task.WaitAsync(cancellationToken);
    }
}
