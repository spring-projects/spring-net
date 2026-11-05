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

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Tests for <see cref="LocalTaskExecutorThreadPool" />.
/// </summary>
[TestFixture]
public class LocalTaskExecutorThreadPoolTests
{
    private static readonly TimeSpan s_short = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan s_long = TimeSpan.FromSeconds(10);

    private ThreadExecutor executor;
    private LocalTaskExecutorThreadPool pool;

    [SetUp]
    public void SetUp()
    {
        executor = new ThreadExecutor();
        pool = new LocalTaskExecutorThreadPool(executor);
    }

    [Test]
    public void TestConstructor_RequiresTaskExecutor()
    {
        Assert.Throws<ArgumentNullException>(() => new LocalTaskExecutorThreadPool(null));
    }

    [Test]
    public void TestPoolSize_DefaultsToTenAndIsAProperty()
    {
        Assert.That(pool.PoolSize, Is.EqualTo(10));

        pool.PoolSize = 3;

        Assert.That(pool.PoolSize, Is.EqualTo(3));
        Assert.That(() => pool.PoolSize = 0, Throws.Exception);
    }

    [Test]
    public async Task TestTryRun_RunsTheWorkOnTheExecutor()
    {
        TaskCompletionSource<int> ran = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        bool accepted = await pool.TryRun(() =>
        {
            ran.SetResult(Environment.CurrentManagedThreadId);
            return ValueTask.CompletedTask;
        });

        Assert.That(accepted, Is.True);
        Assert.That(await ran.Task.WaitAsync(s_long), Is.Not.EqualTo(Environment.CurrentManagedThreadId));
        Assert.That(executor.Executed, Is.EqualTo(1));
    }

    [Test]
    public async Task TestTryRun_NullActionIsNotAccepted()
    {
        Assert.That(await pool.TryRun(null), Is.False);
        Assert.That(pool.ActiveCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TestTryRun_ReturnsFalseWhenTheExecutorRejectsTheTask()
    {
        executor.Reject = true;

        bool accepted = await pool.TryRun(() => ValueTask.CompletedTask);

        Assert.That(accepted, Is.False);
        Assert.That(pool.ActiveCount, Is.EqualTo(0), "a rejected task must not stay counted as running");
        Assert.That(await pool.Drain(), Is.True);
    }

    [Test]
    public async Task TestTryRun_AsynchronousWorkIsWaitedForOnTheExecutorThread()
    {
        using ManualResetEventSlim release = new ManualResetEventSlim();
        await pool.TryRun(async () =>
        {
            await Task.Yield();
            release.Wait(s_long);
        });

        Assert.That(pool.ActiveCount, Is.EqualTo(1));
        release.Set();
        Assert.That(await pool.Drain(), Is.True);
        Assert.That(pool.ActiveCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TestTryRun_TheCountIsReleasedWhenTheWorkFails()
    {
        await pool.TryRun(() => throw new InvalidOperationException("the job failed"));

        Assert.That(await pool.Drain(), Is.True);
        Assert.That(pool.ActiveCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TestWaitForAvailableThreads_IsPoolSizeMinusInFlight()
    {
        pool.PoolSize = 3;
        Assert.That(await pool.WaitForAvailableThreads(), Is.EqualTo(3));

        using ManualResetEventSlim release = new ManualResetEventSlim();
        await pool.TryRun(() =>
        {
            release.Wait(s_long);
            return ValueTask.CompletedTask;
        });

        Assert.That(await pool.WaitForAvailableThreads(), Is.EqualTo(2));
        release.Set();
        await pool.Drain();
        Assert.That(await pool.WaitForAvailableThreads(), Is.EqualTo(3));
    }

    [Test]
    public async Task TestWaitForAvailableThreads_WaitsWithoutBlockingWhenThePoolIsFull()
    {
        pool.PoolSize = 1;
        using ManualResetEventSlim release = new ManualResetEventSlim();
        await pool.TryRun(() =>
        {
            release.Wait(s_long);
            return ValueTask.CompletedTask;
        });

        ValueTask<int> waiting = pool.WaitForAvailableThreads();
        Assert.That(waiting.IsCompleted, Is.False, "there is no free slot yet");
        Task<int> waitingTask = waiting.AsTask();
        await Task.Delay(s_short);
        Assert.That(waitingTask.IsCompleted, Is.False);

        release.Set();

        Assert.That(await waitingTask.WaitAsync(s_long), Is.EqualTo(1));
    }

    [Test]
    public async Task TestWaitForAvailableThreads_CanBeCancelled()
    {
        pool.PoolSize = 1;
        using ManualResetEventSlim release = new ManualResetEventSlim();
        await pool.TryRun(() =>
        {
            release.Wait(s_long);
            return ValueTask.CompletedTask;
        });
        using CancellationTokenSource cts = new CancellationTokenSource(s_short);

        await Assert.CatchAsync<OperationCanceledException>(async () => await pool.WaitForAvailableThreads(cts.Token));

        release.Set();
        await pool.Drain();
    }

    [Test]
    public async Task TestDrain_WaitsForTheWorkToFinish()
    {
        using ManualResetEventSlim release = new ManualResetEventSlim();
        await pool.TryRun(() =>
        {
            release.Wait(s_long);
            return ValueTask.CompletedTask;
        });

        Task<bool> draining = pool.Drain().AsTask();
        await Task.Delay(s_short);
        Assert.That(draining.IsCompleted, Is.False, "Drain returned while a work item was still running");

        release.Set();

        Assert.That(await draining.WaitAsync(s_long), Is.True);
    }

    [Test]
    public async Task TestDrain_ReportsAnAbandonedWaitWhenCancelled()
    {
        using ManualResetEventSlim release = new ManualResetEventSlim();
        await pool.TryRun(() =>
        {
            release.Wait(s_long);
            return ValueTask.CompletedTask;
        });
        using CancellationTokenSource cts = new CancellationTokenSource(s_short);

        bool drained = await pool.Drain(cts.Token);

        Assert.That(drained, Is.False);
        release.Set();
        Assert.That(await pool.Drain(), Is.True);
    }

    [Test]
    public async Task TestDrain_StopsAcceptingNewWork()
    {
        await pool.Drain();

        Assert.That(await pool.TryRun(() => ValueTask.CompletedTask), Is.False);
    }

    [Test]
    public async Task TestShutdown_WaitsForJobsToComplete()
    {
        using ManualResetEventSlim release = new ManualResetEventSlim();
        await pool.TryRun(() =>
        {
            release.Wait(s_long);
            return ValueTask.CompletedTask;
        });

        Task shuttingDown = pool.Shutdown(true).AsTask();
        await Task.Delay(s_short);
        Assert.That(shuttingDown.IsCompleted, Is.False, "Shutdown returned while a work item was still running");

        release.Set();
        await shuttingDown.WaitAsync(s_long);
        Assert.That(pool.ActiveCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TestShutdown_WithoutWaitReturnsAtOnceAndRefusesNewWork()
    {
        using ManualResetEventSlim release = new ManualResetEventSlim();
        await pool.TryRun(() =>
        {
            release.Wait(s_long);
            return ValueTask.CompletedTask;
        });

        await pool.Shutdown(false).AsTask().WaitAsync(s_long);

        Assert.That(await pool.TryRun(() => ValueTask.CompletedTask), Is.False);
        release.Set();
        await pool.Drain();
    }

    /// <summary>
    /// Runs every task on a thread of its own, as an unbounded executor would.
    /// </summary>
    private sealed class ThreadExecutor : ITaskExecutor
    {
        private int executed;

        public bool Reject { get; set; }

        public int Executed => Volatile.Read(ref executed);

        public void Execute(ThreadStart runnable)
        {
            if (Reject)
            {
                throw new TaskRejectedException();
            }

            Interlocked.Increment(ref executed);
            Thread thread = new Thread(() =>
            {
                try
                {
                    runnable();
                }
                catch (InvalidOperationException)
                {
                    // a failing job ends its thread, as it does on any executor that does not rethrow
                }
            })
            {
                IsBackground = true
            };
            thread.Start();
        }
    }
}
