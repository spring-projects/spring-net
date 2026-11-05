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
using System.Text;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Quartz;
using Quartz.Configuration;
using Quartz.Extensibility;
using Quartz.Impl.Triggers;
using Spring.Core.IO;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Tests for SchedulerFactoryObject.
/// </summary>
/// <author>Marko Lahma (.NET)</author>
[TestFixture]
public class SchedulerFactoryObjectTest
{
    private SchedulerFactoryObject factory;
    private ILoggerFactory originalLoggerFactory;

    /// <summary>
    /// Test setup.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        factory = new SchedulerFactoryObject();
        originalLoggerFactory = LogManager.LoggerFactory;
    }

    [TearDown]
    public void TearDown()
    {
        factory?.Dispose();
        LogManager.LoggerFactory = originalLoggerFactory;
    }

    // ---- lifecycle against a fake scheduler

    /// <summary>
    /// Disposing a factory that was never initialized must not throw.
    /// </summary>
    [Test]
    public void TestDispose_NotInitialized()
    {
        Assert.That(() => factory.Dispose(), Throws.Nothing);
    }

    /// <summary>
    /// Disposing an initialized factory shuts the scheduler down.
    /// </summary>
    [Test]
    public void TestDispose_Initialized_ShutsDownScheduler()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.AutoStartup = false;
        factory.AfterPropertiesSet();

        factory.Dispose();

        A.CallTo(() => scheduler.Shutdown(false, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    /// <summary>
    /// The scheduler is shut down with the wait that was asked for.
    /// </summary>
    [Test]
    public void TestDispose_WaitForJobsToCompleteOnShutdown()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.WaitForJobsToCompleteOnShutdown = true;
        factory.AfterPropertiesSet();

        factory.Dispose();

        A.CallTo(() => scheduler.Shutdown(true, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    /// <summary>
    /// The scheduler is created from the configured builder, once.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_CreatesSchedulerThroughTheHookOnce()
    {
        HookCountingFactoryObject counting = new HookCountingFactoryObject();
        factory = counting;

        factory.AfterPropertiesSet();

        Assert.That(counting.CreateCount, Is.EqualTo(1));
        Assert.That(counting.Builder, Is.Not.Null);
        Assert.That(factory.GetObject(), Is.SameAs(counting.Created));
    }

    /// <summary>
    /// Tests AfterPropertiesSet behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_NoAutoStartup()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.AutoStartup = false;
        factory.AfterPropertiesSet();

        A.CallTo(() => scheduler.Start(A<CancellationToken>._)).MustNotHaveHappened();
    }

    /// <summary>
    /// Tests AfterPropertiesSet behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_Calendars()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.AutoStartup = false;

        const string calendarName = "calendar";
        ICalendar cal = A.Fake<ICalendar>();
        Hashtable calTable = new Hashtable();
        calTable[calendarName] = cal;
        factory.Calendars = calTable;

        factory.AfterPropertiesSet();

        A.CallTo(() => scheduler.AddCalendar(calendarName, cal, AddCalendarOptions.ReplacingAndUpdatingTriggers, A<CancellationToken>._)).MustHaveHappened();
    }

    /// <summary>
    /// Tests AfterPropertiesSet behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_Trigger_TriggerExists()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.AutoStartup = false;

        const string TRIGGER_NAME = "trigName";
        const string TRIGGER_GROUP = "trigGroup";
        SimpleTriggerImpl trigger = new SimpleTriggerImpl { Key = new TriggerKey(TRIGGER_NAME, TRIGGER_GROUP) };
        factory.Triggers = new ITrigger[] { trigger };

        A.CallTo(() => scheduler.GetTrigger(new TriggerKey(TRIGGER_NAME, TRIGGER_GROUP), A<CancellationToken>._)).Returns(new ValueTask<ITrigger>(trigger));

        factory.AfterPropertiesSet();

        A.CallTo(() => scheduler.ScheduleJob(A<ITrigger>._, A<ScheduleJobOptions>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    /// <summary>
    /// Tests AfterPropertiesSet behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_Trigger_TriggerDoesntExist()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.AutoStartup = false;

        SimpleTriggerImpl trigger = new SimpleTriggerImpl { Key = new TriggerKey("trigName", "trigGroup") };
        factory.Triggers = new ITrigger[] { trigger };

        factory.AfterPropertiesSet();

        A.CallTo(() => scheduler.ScheduleJob(trigger, A<ScheduleJobOptions>._, A<CancellationToken>._)).MustHaveHappened();
    }

    /// <summary>
    /// Tests AfterPropertiesSet behavior.
    /// </summary>
    [Test]
    public async Task TestStart()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.AutoStartup = false;
        factory.AfterPropertiesSet();
        await factory.Start();

        A.CallTo(() => scheduler.Start(A<CancellationToken>._)).MustHaveHappened();
    }

    /// <summary>
    /// Tests AfterPropertiesSet behavior.
    /// </summary>
    [Test]
    public async Task TestStop()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.AutoStartup = false;
        factory.AfterPropertiesSet();
        await factory.Stop();

        A.CallTo(() => scheduler.Standby(A<CancellationToken>._)).MustHaveHappened();
    }

    /// <summary>
    /// A job factory that is aware of the scheduler context gets the context of the scheduler.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_JobFactoryGetsSchedulerContext()
    {
        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        ContextRecordingJobFactory jobFactory = new ContextRecordingJobFactory();
        factory = new TestSchedulerFactoryObject(scheduler);
        factory.JobFactory = jobFactory;
        factory.AfterPropertiesSet();

        Assert.That(jobFactory.Received, Is.SameAs(scheduler.Context));
    }

    private sealed class ContextRecordingJobFactory : SpringObjectJobFactory
    {
        public SchedulerContext Received { get; private set; }

        public override SchedulerContext SchedulerContext
        {
            set
            {
                Received = value;
                base.SchedulerContext = value;
            }
        }
    }

    // ---- against a real scheduler

    /// <summary>
    /// Tests AfterPropertiesSet behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_Defaults()
    {
        factory.AfterPropertiesSet();

        IScheduler scheduler = (IScheduler) factory.GetObject();
        Assert.That(scheduler, Is.Not.Null);
        Assert.That(scheduler.Status, Is.Not.EqualTo(SchedulerStatus.Running), "the scheduler starts on context refresh");
        Assert.That(factory.Running, Is.False);
    }

    /// <summary>
    /// Tests AfterPropertiesSet behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_NullJobFactory()
    {
        factory.JobFactory = null;
        factory.AfterPropertiesSet();

        Assert.That(factory.GetObject(), Is.Not.Null);
    }

    /// <summary>
    /// The scheduler can be started and stopped.
    /// </summary>
    [Test]
    public async Task TestStartAndStopRealScheduler()
    {
        factory.AfterPropertiesSet();
        IScheduler scheduler = (IScheduler) factory.GetObject();

        await factory.Start();
        Assert.That(scheduler.Status, Is.EqualTo(SchedulerStatus.Running));
        Assert.That(factory.Running, Is.True);

        await factory.Stop();
        Assert.That(scheduler.Status, Is.EqualTo(SchedulerStatus.Standby));
        Assert.That(factory.Running, Is.False);
    }

    /// <summary>
    /// Disposing shuts the real scheduler down and releases the Quartz factory that owns it.
    /// </summary>
    [Test]
    public void TestDispose_ShutsDownRealScheduler()
    {
        factory.AfterPropertiesSet();
        IScheduler scheduler = (IScheduler) factory.GetObject();

        factory.Dispose();

        Assert.That(scheduler.Status, Is.EqualTo(SchedulerStatus.Shutdown));
    }

    /// <summary>
    /// The object type is the scheduler's.
    /// </summary>
    [Test]
    public void TestObjectType()
    {
        Assert.That(factory.ObjectType, Is.EqualTo(typeof(IScheduler)));
        factory.AfterPropertiesSet();
        Assert.That(factory.ObjectType, Is.EqualTo(factory.GetObject().GetType()));
        Assert.That(factory.IsSingleton, Is.True);
    }

    /// <summary>
    /// The scheduler context gets the objects it was given, and the application context.
    /// </summary>
    [Test]
    public void TestSchedulerContext()
    {
        Spring.Context.Support.StaticApplicationContext ctx = new Spring.Context.Support.StaticApplicationContext();
        factory.SchedulerContextAsMap = new Hashtable { ["testObject"] = ctx };
        factory.ApplicationContext = ctx;
        factory.ApplicationContextSchedulerContextKey = "appCtx";

        factory.AfterPropertiesSet();

        IScheduler scheduler = (IScheduler) factory.GetObject();
        Assert.That(scheduler.Context["testObject"], Is.SameAs(ctx));
        Assert.That(scheduler.Context["appCtx"], Is.SameAs(ctx));
    }

    /// <summary>
    /// The application context key needs an application context.
    /// </summary>
    [Test]
    public void TestSchedulerContext_ApplicationContextKeyWithoutApplicationContext()
    {
        factory.ApplicationContextSchedulerContextKey = "appCtx";

        Assert.Throws<SystemException>(() => factory.AfterPropertiesSet());
    }

    // ---- typed options

    /// <summary>
    /// Nothing is applied that was not set.
    /// </summary>
    [Test]
    public void TestTypedProperties_NothingSetAppliesNothing()
    {
        TestSchedulerFactoryObject test = new TestSchedulerFactoryObject(TestSchedulerFactoryObject.CreateFakeScheduler());
        factory = test;
        factory.AfterPropertiesSet();

        QuartzSchedulerOptions scheduler = test.GetOptions<QuartzSchedulerOptions>();
        Assert.That(scheduler.InstanceName, Is.EqualTo(QuartzSchedulerOptions.DefaultInstanceName));
        Assert.That(scheduler.InstanceId, Is.EqualTo(QuartzSchedulerOptions.DefaultInstanceId));
        Assert.That(scheduler.GenerateInstanceId, Is.False);
        Assert.That(scheduler.IdleWaitTime, Is.EqualTo(new QuartzSchedulerOptions().IdleWaitTime));
        Assert.That(scheduler.MaxBatchSize, Is.EqualTo(new QuartzSchedulerOptions().MaxBatchSize));
        Assert.That(scheduler.DefaultTimeZone, Is.Null);
        Assert.That(test.GetOptions<ThreadPoolOptions>().MaxConcurrency, Is.EqualTo(ThreadPoolOptions.DefaultMaxConcurrency));
        Assert.That(test.GetOptions<InMemoryJobStoreOptions>().MisfireThreshold, Is.EqualTo(new InMemoryJobStoreOptions().MisfireThreshold));
        Assert.That(test.GetOptions<AdoJobStoreOptions>().TablePrefix, Is.EqualTo(new AdoJobStoreOptions().TablePrefix));
    }

    /// <summary>
    /// Each typed property of the scheduler reaches the options that the builder has.
    /// </summary>
    [Test]
    public void TestTypedProperties_ReachTheBuilderOptions()
    {
        TimeZoneInfo timeZone = TimeZoneInfo.Utc;
        TestSchedulerFactoryObject test = new TestSchedulerFactoryObject(TestSchedulerFactoryObject.CreateFakeScheduler());
        factory = test;
        factory.SchedulerName = "typed";
        factory.InstanceId = "instance-1";
        factory.GenerateInstanceId = true;
        factory.IdleWaitTime = TimeSpan.FromSeconds(7);
        factory.MaxBatchSize = 12;
        factory.DefaultTimeZone = timeZone;
        factory.MaxConcurrency = 3;
        factory.MisfireThreshold = TimeSpan.FromSeconds(42);
        factory.TablePrefix = "SPRING_";
        factory.AfterPropertiesSet();

        QuartzSchedulerOptions scheduler = test.GetOptions<QuartzSchedulerOptions>();
        Assert.That(scheduler.InstanceName, Is.EqualTo("typed"));
        Assert.That(scheduler.InstanceId, Is.EqualTo("instance-1"));
        Assert.That(scheduler.GenerateInstanceId, Is.True);
        Assert.That(scheduler.IdleWaitTime, Is.EqualTo(TimeSpan.FromSeconds(7)));
        Assert.That(scheduler.MaxBatchSize, Is.EqualTo(12));
        Assert.That(scheduler.DefaultTimeZone, Is.SameAs(timeZone));
        Assert.That(test.GetOptions<ThreadPoolOptions>().MaxConcurrency, Is.EqualTo(3));
        Assert.That(test.GetOptions<InMemoryJobStoreOptions>().MisfireThreshold, Is.EqualTo(TimeSpan.FromSeconds(42)));
        Assert.That(test.GetOptions<AdoJobStoreOptions>().MisfireThreshold, Is.EqualTo(TimeSpan.FromSeconds(42)));
        Assert.That(test.GetOptions<AdoJobStoreOptions>().TablePrefix, Is.EqualTo("SPRING_"));
    }

    /// <summary>
    /// The scheduler name defaults to the object name, and an explicit name wins over it.
    /// </summary>
    [Test]
    public void TestSchedulerName_ObjectNameIsTheDefault()
    {
        factory.ObjectName = "fromObjectName";
        factory.AfterPropertiesSet();

        Assert.That(((IScheduler) factory.GetObject()).SchedulerName, Is.EqualTo("fromObjectName"));
    }

    /// <summary>
    /// Typed properties reach a real scheduler.
    /// </summary>
    [Test]
    public async Task TestTypedProperties_ReachTheRealScheduler()
    {
        factory.SchedulerName = "typedScheduler";
        factory.InstanceId = "typedInstance";
        factory.MaxConcurrency = 4;
        factory.AfterPropertiesSet();

        IScheduler scheduler = (IScheduler) factory.GetObject();
        SchedulerMetadata metadata = await scheduler.GetMetadata();

        Assert.That(scheduler.SchedulerName, Is.EqualTo("typedScheduler"));
        Assert.That(scheduler.SchedulerInstanceId, Is.EqualTo("typedInstance"));
        Assert.That(metadata.ThreadPoolSize, Is.EqualTo(4));
        Assert.That(metadata.JobStorePersistent, Is.False);
    }

    /// <summary>
    /// The task executor becomes the thread pool.
    /// </summary>
    [Test]
    public async Task TestTaskExecutorBecomesTheThreadPool()
    {
        factory.TaskExecutor = new InlineTaskExecutor();
        factory.MaxConcurrency = 6;
        factory.AfterPropertiesSet();

        SchedulerMetadata metadata = await ((IScheduler) factory.GetObject()).GetMetadata();

        Assert.That(metadata.ThreadPoolTypeName, Does.Contain(nameof(LocalTaskExecutorThreadPool)));
        Assert.That(metadata.ThreadPoolSize, Is.EqualTo(6));
    }

    // ---- precedence

    /// <summary>
    /// The flat properties are the weakest source: a typed property wins over them.
    /// </summary>
    [Test]
    public void TestPrecedence_PropertiesAreOverriddenByTypedProperties()
    {
        factory.QuartzProperties = new Hashtable { ["quartz.scheduler.instanceName"] = "fromProperties" };
        factory.AfterPropertiesSet();
        Assert.That(((IScheduler) factory.GetObject()).SchedulerName, Is.EqualTo("fromProperties"), "the property applies when nothing overrides it");
        factory.Dispose();

        factory = new SchedulerFactoryObject();
        factory.QuartzProperties = new Hashtable { ["quartz.scheduler.instanceName"] = "fromProperties" };
        factory.SchedulerName = "fromTypedProperty";
        factory.AfterPropertiesSet();
        Assert.That(((IScheduler) factory.GetObject()).SchedulerName, Is.EqualTo("fromTypedProperty"));
    }

    /// <summary>
    /// The callback has the last word, over the typed properties and over the flat properties.
    /// </summary>
    [Test]
    public void TestPrecedence_CallbackWinsOverTypedProperties()
    {
        factory.QuartzProperties = new Hashtable { ["quartz.scheduler.instanceName"] = "fromProperties" };
        factory.SchedulerName = "fromTypedProperty";
        factory.ConfigureQuartz = quartz => quartz.ConfigureScheduler(options => options.InstanceName = "fromCallback");
        factory.AfterPropertiesSet();

        Assert.That(((IScheduler) factory.GetObject()).SchedulerName, Is.EqualTo("fromCallback"));
    }

    /// <summary>
    /// The three sources of one numeric setting, in a real scheduler.
    /// </summary>
    [Test]
    public async Task TestPrecedence_MaxConcurrency()
    {
        SchedulerFactoryObject propertiesOnly = new SchedulerFactoryObject();
        propertiesOnly.SchedulerName = "precedence1";
        propertiesOnly.QuartzProperties = new Hashtable { ["quartz.threadPool.maxConcurrency"] = "2" };
        propertiesOnly.AfterPropertiesSet();
        Assert.That((await ((IScheduler) propertiesOnly.GetObject()).GetMetadata()).ThreadPoolSize, Is.EqualTo(2));
        propertiesOnly.Dispose();

        SchedulerFactoryObject typed = new SchedulerFactoryObject();
        typed.SchedulerName = "precedence2";
        typed.QuartzProperties = new Hashtable { ["quartz.threadPool.maxConcurrency"] = "2" };
        typed.MaxConcurrency = 3;
        typed.AfterPropertiesSet();
        Assert.That((await ((IScheduler) typed.GetObject()).GetMetadata()).ThreadPoolSize, Is.EqualTo(3));
        typed.Dispose();

        SchedulerFactoryObject callback = new SchedulerFactoryObject();
        callback.SchedulerName = "precedence3";
        callback.QuartzProperties = new Hashtable { ["quartz.threadPool.maxConcurrency"] = "2" };
        callback.MaxConcurrency = 3;
        callback.ConfigureQuartz = quartz => quartz.UseDefaultThreadPool(5);
        callback.AfterPropertiesSet();
        Assert.That((await ((IScheduler) callback.GetObject()).GetMetadata()).ThreadPoolSize, Is.EqualTo(5));
        callback.Dispose();
    }

    /// <summary>
    /// Quartz refuses a property it does not know, and the factory lets that surface.
    /// </summary>
    [Test]
    public void TestQuartzProperties_UnknownKeyThrows()
    {
        factory.QuartzProperties = new Hashtable { ["quartz.thisIsNotAKey"] = "x" };

        Assert.Throws<SchedulerConfigException>(() => factory.AfterPropertiesSet());
    }

    /// <summary>
    /// The configuration file is read as key = value lines, keeps the equals signs of a value, and is overridden by the properties.
    /// </summary>
    [Test]
    public void TestConfigLocation_ReadingPreservesExtraEqualsMarksAndTrimsKeysAndValues()
    {
        string configuration =
            "  quartz.scheduler.instanceName   =  name=with=equals  " + Environment.NewLine +
            "quartz.threadPool.maxConcurrency = 2";
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(configuration));
        factory.ConfigLocation = new InputStreamResource(ms, "description");
        factory.QuartzProperties = new Hashtable { ["quartz.threadPool.maxConcurrency"] = "3" };

        factory.AfterPropertiesSet();

        IScheduler scheduler = (IScheduler) factory.GetObject();
        Assert.That(scheduler.SchedulerName, Is.EqualTo("name=with=equals"));
        Assert.That(scheduler.GetMetadata().AsTask().GetAwaiter().GetResult().ThreadPoolSize, Is.EqualTo(3));
    }

    // ---- logging

    /// <summary>
    /// Quartz logs through the logger factory of Spring.
    /// </summary>
    [Test]
    public async Task TestLogging_QuartzUsesTheSpringLoggerFactory()
    {
        CapturingLoggerFactory loggerFactory = new CapturingLoggerFactory();
        LogManager.LoggerFactory = loggerFactory;

        factory.AfterPropertiesSet();
        await factory.Start();
        factory.Dispose();

        Assert.That(loggerFactory.Categories, Has.Some.StartsWith("Quartz"));
    }

    // ---- the persistent store

    /// <summary>
    /// A database provider needs the name of the Quartz dialect.
    /// </summary>
    [Test]
    public void TestDbProvider_RequiresQuartzDbProvider()
    {
        factory.DbProvider = A.Fake<Spring.Data.Common.IDbProvider>();

        var exception = Assert.Throws<InvalidOperationException>(() => factory.AfterPropertiesSet());

        Assert.That(exception.Message, Does.Contain("QuartzDbProvider"));
    }

    /// <summary>
    /// Clustering needs the persistent store.
    /// </summary>
    [Test]
    public void TestClustered_RequiresDbProvider()
    {
        factory.Clustered = true;

        Assert.Throws<InvalidOperationException>(() => factory.AfterPropertiesSet());
    }

    /// <summary>
    /// The check-in interval needs clustering.
    /// </summary>
    [Test]
    public void TestClusterCheckinInterval_RequiresClustered()
    {
        factory.DbProvider = A.Fake<Spring.Data.Common.IDbProvider>();
        factory.QuartzDbProvider = "SystemDataSqlClient";
        factory.ClusterCheckinInterval = TimeSpan.FromSeconds(3);

        Assert.Throws<InvalidOperationException>(() => factory.AfterPropertiesSet());
    }

    // ---- repository

    /// <summary>
    /// An exposed scheduler is found by the accessor, and gone after disposal.
    /// </summary>
    [Test]
    public void TestExposeSchedulerInRepository_IsFoundAndRemovedOnDispose()
    {
        factory.SchedulerName = "exposed";
        factory.ExposeSchedulerInRepository = true;
        factory.AfterPropertiesSet();
        IScheduler scheduler = (IScheduler) factory.GetObject();

        SchedulerFinder finder = new SchedulerFinder();
        Assert.That(finder.Find("exposed"), Is.SameAs(scheduler));

        factory.Dispose();

        Assert.Throws<InvalidOperationException>(() => finder.Find("exposed"));
    }

    /// <summary>
    /// A scheduler that is not exposed is not found.
    /// </summary>
    [Test]
    public void TestExposeSchedulerInRepository_NotExposedIsNotFound()
    {
        factory.SchedulerName = "hidden";
        factory.AfterPropertiesSet();

        Assert.Throws<InvalidOperationException>(() => new SchedulerFinder().Find("hidden"));
    }

    /// <summary>
    /// A second scheduler of the name of an exposed one is refused with the message of Quartz 3.
    /// </summary>
    [Test]
    public void TestExposeSchedulerInRepository_NameCollision()
    {
        factory.SchedulerName = "collision";
        factory.ExposeSchedulerInRepository = true;
        factory.AfterPropertiesSet();

        using SchedulerFactoryObject second = new SchedulerFactoryObject();
        second.SchedulerName = "collision";
        var exception = Assert.Throws<InvalidOperationException>(() => second.AfterPropertiesSet());
        Assert.That(exception.Message, Is.EqualTo(
            "Active Scheduler of name 'collision' already registered in Quartz SchedulerRepository. Cannot create a new Spring-managed Scheduler of the same name!"));

        // the first one is still the one that is registered
        Assert.That(new SchedulerFinder().Find("collision"), Is.SameAs(factory.GetObject()));
    }

    /// <summary>
    /// The collision is found when the name comes from the Quartz properties, after the scheduler has been built, and
    /// the scheduler that was built is shut down.
    /// </summary>
    [Test]
    public void TestExposeSchedulerInRepository_NameCollisionFromProperties()
    {
        factory.SchedulerName = "collision2";
        factory.ExposeSchedulerInRepository = true;
        factory.AfterPropertiesSet();

        using SchedulerFactoryObject second = new SchedulerFactoryObject();
        second.QuartzProperties = new Hashtable { ["quartz.scheduler.instanceName"] = "collision2" };
        Assert.Throws<InvalidOperationException>(() => second.AfterPropertiesSet());
        Assert.That(second.GetObject(), Is.Null);
    }

    /// <summary>
    /// A name can be used again once the scheduler that had it is gone.
    /// </summary>
    [Test]
    public void TestExposeSchedulerInRepository_NameIsFreeAfterDispose()
    {
        factory.SchedulerName = "reused";
        factory.ExposeSchedulerInRepository = true;
        factory.AfterPropertiesSet();
        factory.Dispose();

        factory = new SchedulerFactoryObject();
        factory.SchedulerName = "reused";
        factory.ExposeSchedulerInRepository = true;

        Assert.That(() => factory.AfterPropertiesSet(), Throws.Nothing);
    }

    /// <summary>
    /// Two schedulers that are not exposed can share a name, since each has a repository of its own.
    /// </summary>
    [Test]
    public void TestSameNameWithoutExposureIsAllowed()
    {
        factory.SchedulerName = "shared";
        factory.AfterPropertiesSet();

        using SchedulerFactoryObject second = new SchedulerFactoryObject();
        second.SchedulerName = "shared";

        Assert.That(() => second.AfterPropertiesSet(), Throws.Nothing);
        Assert.That(second.GetObject(), Is.Not.SameAs(factory.GetObject()));
    }

    private sealed class SchedulerFinder : SchedulerAccessorObject
    {
        public IScheduler Find(string name)
        {
            return FindScheduler(name);
        }
    }

    private sealed class HookCountingFactoryObject : SchedulerFactoryObject
    {
        public int CreateCount { get; private set; }

        public QuartzSchedulerBuilder Builder { get; private set; }

        public IScheduler Created { get; private set; }

        protected override IScheduler CreateScheduler(QuartzSchedulerBuilder builder)
        {
            CreateCount++;
            Builder = builder;
            Created = TestSchedulerFactoryObject.CreateFakeScheduler();
            return Created;
        }
    }

    private sealed class InlineTaskExecutor : ITaskExecutor
    {
        public void Execute(ThreadStart runnable)
        {
            runnable();
        }
    }

    private sealed class CapturingLoggerFactory : ILoggerFactory
    {
        private readonly List<string> categories = new List<string>();

        public IReadOnlyList<string> Categories
        {
            get
            {
                lock (categories)
                {
                    return categories.ToArray();
                }
            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            lock (categories)
            {
                categories.Add(categoryName);
            }

            return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }

        public void Dispose()
        {
        }
    }
}
