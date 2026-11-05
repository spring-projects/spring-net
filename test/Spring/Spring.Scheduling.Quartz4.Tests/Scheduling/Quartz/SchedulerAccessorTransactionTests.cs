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

using System.Collections.Concurrent;
using FakeItEasy;
using NUnit.Framework;
using Quartz;
using Quartz.Impl.Triggers;
using Spring.Transaction;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Registering jobs and triggers in a Spring transaction has to stay on one thread: on modern .NET Spring keeps the
/// transaction state in thread-static storage, and Quartz completes its asynchronous calls on other threads.
/// </summary>
[TestFixture]
public class SchedulerAccessorTransactionTests
{
    /// <summary>
    /// The transaction begins and commits on the thread that registers, and so do all the calls to the scheduler, although
    /// the scheduler completes every call on another thread.
    /// </summary>
    [Test]
    public void TestRegisterJobsAndTriggers_StaysOnOneThreadInsideTheTransaction()
    {
        ThreadRecorder recorder = new ThreadRecorder();
        IScheduler scheduler = CreateSchedulerThatCompletesOnOtherThreads(recorder);
        RecordingTransactionManager transactionManager = new RecordingTransactionManager(recorder);

        AccessorForTest accessor = new AccessorForTest(scheduler);
        accessor.TransactionManager = transactionManager;
        accessor.Triggers = new ITrigger[]
        {
            new SimpleTriggerImpl { Key = new TriggerKey("first"), JobKey = new JobKey("job") },
            new SimpleTriggerImpl { Key = new TriggerKey("second"), JobKey = new JobKey("job") }
        };
        accessor.JobDetails = new[] { JobBuilder.Create().OfType(typeof(NoOpJob)).WithIdentity("job").Build() };
        accessor.Calendars = new Dictionary<string, ICalendar> { ["calendar"] = A.Fake<ICalendar>() };

        int callingThread = Environment.CurrentManagedThreadId;
        accessor.Register();

        Assert.That(transactionManager.Begun, Is.EqualTo(callingThread), "the transaction began on another thread");
        Assert.That(transactionManager.Committed, Is.EqualTo(callingThread), "the transaction committed on another thread");
        Assert.That(recorder.Calls, Is.Not.Empty);
        Assert.That(recorder.Calls.Select(call => call.Thread).Distinct(), Is.EqualTo(new[] { callingThread }),
            "a call to the scheduler was made from another thread than the one of the transaction");
        Assert.That(recorder.Calls.Select(call => call.Name), Does.Contain("AddJob").And.Contain("AddCalendar").And.Contain("ScheduleJob"));
        Assert.That(transactionManager.RolledBack, Is.False);
    }

    /// <summary>
    /// A failure rolls the transaction back, on the same thread, and surfaces as a scheduler exception that keeps the cause.
    /// </summary>
    [Test]
    public void TestRegisterJobsAndTriggers_RollsBackOnTheSameThread()
    {
        ThreadRecorder recorder = new ThreadRecorder();
        IScheduler scheduler = CreateSchedulerThatCompletesOnOtherThreads(recorder);
        A.CallTo(() => scheduler.ScheduleJob(A<ITrigger>._, A<ScheduleJobOptions>._, A<CancellationToken>._))
            .Returns(ThrowsOnAnotherThread());
        RecordingTransactionManager transactionManager = new RecordingTransactionManager(recorder);

        AccessorForTest accessor = new AccessorForTest(scheduler);
        accessor.TransactionManager = transactionManager;
        accessor.Triggers = new ITrigger[] { new SimpleTriggerImpl { Key = new TriggerKey("first"), JobKey = new JobKey("job") } };

        int callingThread = Environment.CurrentManagedThreadId;
        var exception = Assert.Throws<SchedulerException>(accessor.Register);

        Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
        Assert.That(transactionManager.RolledBackOn, Is.EqualTo(callingThread));
        Assert.That(transactionManager.Committed, Is.Null);
    }

    /// <summary>
    /// Without a transaction manager the registration still works.
    /// </summary>
    [Test]
    public void TestRegisterJobsAndTriggers_WithoutTransactionManager()
    {
        ThreadRecorder recorder = new ThreadRecorder();
        IScheduler scheduler = CreateSchedulerThatCompletesOnOtherThreads(recorder);

        AccessorForTest accessor = new AccessorForTest(scheduler);
        accessor.Triggers = new ITrigger[] { new SimpleTriggerImpl { Key = new TriggerKey("first"), JobKey = new JobKey("job") } };
        accessor.Register();

        A.CallTo(() => scheduler.ScheduleJob(A<ITrigger>._, A<ScheduleJobOptions>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    /// <summary>
    /// The control of the tests above: a registration that awaits the scheduler is moved to another thread by the fake
    /// scheduler, so the tests would fail for such an implementation.
    /// </summary>
    [Test]
    public async Task TestControl_AwaitingTheSchedulerMovesToAnotherThread()
    {
        ThreadRecorder recorder = new ThreadRecorder();
        IScheduler scheduler = CreateSchedulerThatCompletesOnOtherThreads(recorder);
        RecordingTransactionManager transactionManager = new RecordingTransactionManager(recorder);

        transactionManager.GetTransaction(null);
        await scheduler.AddJob(JobBuilder.Create().OfType(typeof(NoOpJob)).WithIdentity("job").Build()).ConfigureAwait(false);
        transactionManager.Commit(null);

        Assert.That(transactionManager.Committed, Is.Not.EqualTo(transactionManager.Begun));
    }

    private static ValueTask<DateTimeOffset> ThrowsOnAnotherThread()
    {
        TaskCompletionSource<DateTimeOffset> source = new TaskCompletionSource<DateTimeOffset>();
        new Thread(() =>
        {
            Thread.Sleep(10);
            source.SetException(new InvalidOperationException("the store failed"));
        })
        {
            IsBackground = true
        }.Start();
        return new ValueTask<DateTimeOffset>(source.Task);
    }

    private static IScheduler CreateSchedulerThatCompletesOnOtherThreads(ThreadRecorder recorder)
    {
        IScheduler scheduler = A.Fake<IScheduler>();
        A.CallTo(() => scheduler.GetJobDetail(A<JobKey>._, A<CancellationToken>._))
            .ReturnsLazily(() => recorder.Complete<IJobDetail>("GetJobDetail", null));
        A.CallTo(() => scheduler.GetTrigger(A<TriggerKey>._, A<CancellationToken>._))
            .ReturnsLazily(() => recorder.Complete<ITrigger>("GetTrigger", null));
        A.CallTo(() => scheduler.AddJob(A<IJobDetail>._, A<AddJobOptions>._, A<CancellationToken>._))
            .ReturnsLazily(() => recorder.Complete("AddJob"));
        A.CallTo(() => scheduler.AddCalendar(A<string>._, A<ICalendar>._, A<AddCalendarOptions>._, A<CancellationToken>._))
            .ReturnsLazily(() => recorder.Complete("AddCalendar"));
        A.CallTo(() => scheduler.ScheduleJob(A<ITrigger>._, A<ScheduleJobOptions>._, A<CancellationToken>._))
            .ReturnsLazily(() => recorder.Complete("ScheduleJob", DateTimeOffset.UtcNow));
        return scheduler;
    }

    private sealed class AccessorForTest : SchedulerAccessor
    {
        private readonly IScheduler scheduler;

        public AccessorForTest(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public void Register()
        {
            RegisterJobsAndTriggers();
        }

        protected override IScheduler GetScheduler()
        {
            return scheduler;
        }
    }

    private sealed class ThreadRecorder
    {
        private readonly ConcurrentQueue<(string Name, int Thread)> calls = new ConcurrentQueue<(string, int)>();

        public IReadOnlyList<(string Name, int Thread)> Calls => calls.ToArray();

        // Records the thread of the caller, then completes on a thread of its own after a delay, as the real scheduler
        // completes on whichever thread it happens to be on. The completing thread is never the thread of the caller, and
        // it runs the continuation of a caller that awaits the call, so an implementation that awaits is moved to it.
        public ValueTask Complete(string name)
        {
            calls.Enqueue((name, Environment.CurrentManagedThreadId));
            return new ValueTask(CompleteOnNewThread(true));
        }

        public ValueTask<T> Complete<T>(string name, T result)
        {
            calls.Enqueue((name, Environment.CurrentManagedThreadId));
            return new ValueTask<T>(CompleteOnNewThread(result));
        }

        private static Task<T> CompleteOnNewThread<T>(T result)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();
            Thread thread = new Thread(() =>
            {
                Thread.Sleep(10);
                source.SetResult(result);
            })
            {
                IsBackground = true
            };
            thread.Start();
            return source.Task;
        }
    }

    private sealed class RecordingTransactionManager : IPlatformTransactionManager
    {
        private readonly ThreadRecorder recorder;

        public RecordingTransactionManager(ThreadRecorder recorder)
        {
            this.recorder = recorder;
        }

        public int? Begun { get; private set; }

        public int? Committed { get; private set; }

        public int? RolledBackOn { get; private set; }

        public bool RolledBack => RolledBackOn.HasValue;

        public ITransactionStatus GetTransaction(ITransactionDefinition definition)
        {
            Begun = Environment.CurrentManagedThreadId;
            return A.Fake<ITransactionStatus>();
        }

        public void Commit(ITransactionStatus transactionStatus)
        {
            Committed = Environment.CurrentManagedThreadId;
        }

        public void Rollback(ITransactionStatus transactionStatus)
        {
            RolledBackOn = Environment.CurrentManagedThreadId;
        }
    }
}
