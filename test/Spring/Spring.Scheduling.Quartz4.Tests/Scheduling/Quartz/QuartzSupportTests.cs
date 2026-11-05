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
using FakeItEasy;
using NUnit.Framework;
using Quartz;
using Quartz.Extensibility;
using Quartz.Impl.Triggers;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Support;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// </summary>
/// <author>Juergen Hoeller</author>
/// <author>Alef Arendsen</author>
/// <author>Rob Harrop</author>
/// <author>Marko Lahma (.NET)</author>
[TestFixture]
public class QuartzSupportTests
{
    /// <summary>
    /// Executes parametrized test.
    /// </summary>
    [Test]
    public async Task TestSchedulerFactoryObject_Defaults()
    {
        await DoTestSchedulerFactoryObject(false);
    }

    /// <summary>
    /// Executes parametrized test.
    /// </summary>
    [Test]
    public async Task TestSchedulerFactoryObjectWithExplicitJobDetail()
    {
        await DoTestSchedulerFactoryObject(true);
    }

    private async Task DoTestSchedulerFactoryObject(bool explicitJobDetail)
    {
        TestObject tb = new TestObject("tb", 99);
        JobDetailObject jobDetail0 = new JobDetailObject();
        jobDetail0.JobType = typeof(NoOpJob);
        jobDetail0.ObjectName = "myJob0";
        IDictionary jobData = new Hashtable();
        jobData.Add("testObject", tb);
        jobDetail0.JobDataAsMap = jobData;
        jobDetail0.AfterPropertiesSet();
        Assert.That(jobDetail0.JobDataMap["testObject"], Is.EqualTo(tb));

        CronTriggerObject trigger0 = new CronTriggerObject();
        trigger0.ObjectName = "myTrigger0";
        trigger0.JobDetail = jobDetail0;
        trigger0.CronExpressionString = "0/1 * * * * ?";
        trigger0.AfterPropertiesSet();

        TestMethodInvokingTask task1 = new TestMethodInvokingTask();
        MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
        mijdfb.ObjectName = "myJob1";
        mijdfb.TargetObject = task1;
        mijdfb.TargetMethod = "doSomething";
        mijdfb.AfterPropertiesSet();
        IJobDetail jobDetail1 = (IJobDetail) mijdfb.GetObject();

        SimpleTriggerObject trigger1 = new SimpleTriggerObject();
        trigger1.ObjectName = "myTrigger1";
        trigger1.JobDetail = jobDetail1;
        trigger1.StartDelay = TimeSpan.FromMilliseconds(0);
        trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);
        trigger1.AfterPropertiesSet();

        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();

        SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
        schedulerFactoryObject.JobFactory = null;
        IDictionary schedulerContext = new Hashtable();
        schedulerContext.Add("otherTestObject", tb);
        schedulerFactoryObject.SchedulerContextAsMap = schedulerContext;
        if (explicitJobDetail)
        {
            schedulerFactoryObject.JobDetails = new IJobDetail[] { jobDetail0 };
        }

        schedulerFactoryObject.Triggers = new ITrigger[] { trigger0, trigger1 };
        try
        {
            schedulerFactoryObject.AfterPropertiesSet();
            await schedulerFactoryObject.Start();
        }
        finally
        {
            schedulerFactoryObject.Dispose();
        }

        A.CallTo(() => scheduler.ScheduleJob(trigger0, A<ScheduleJobOptions>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.ScheduleJob(trigger1, A<ScheduleJobOptions>._, A<CancellationToken>._)).MustHaveHappened();

        A.CallTo(() => scheduler.AddJob(jobDetail0, AddJobOptions.ReplacingAndStoringNonDurable, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.AddJob(jobDetail1, AddJobOptions.ReplacingAndStoringNonDurable, A<CancellationToken>._)).MustHaveHappened();

        A.CallTo(() => scheduler.Start(A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.Shutdown(false, A<CancellationToken>._)).MustHaveHappened();
        Assert.That(scheduler.Context["otherTestObject"], Is.EqualTo(tb));
    }

    /// <summary>
    /// Executes parametrized test.
    /// </summary>
    [Test]
    public async Task TestSchedulerFactoryObjectWithExistingJobs()
    {
        await DoTestSchedulerFactoryObjectWithExistingJobs(false);
    }

    /// <summary>
    /// Executes parametrized test.
    /// </summary>
    [Test]
    public async Task TestSchedulerFactoryObjectWithOverwriteExistingJobs()
    {
        await DoTestSchedulerFactoryObjectWithExistingJobs(true);
    }

    private async Task DoTestSchedulerFactoryObjectWithExistingJobs(bool overwrite)
    {
        CronTriggerObject trigger0 = CreateCronTrigger("myTrigger0", "myJob0");
        SimpleTriggerObject trigger1 = CreateSimpleTrigger("myTrigger1", "myJob1");

        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        A.CallTo(() => scheduler.GetTrigger(new TriggerKey("myTrigger0", TriggerKey.DefaultGroup), A<CancellationToken>._)).Returns(new ValueTask<ITrigger>((ITrigger) null));
        A.CallTo(() => scheduler.GetTrigger(new TriggerKey("myTrigger1", TriggerKey.DefaultGroup), A<CancellationToken>._)).Returns(new ValueTask<ITrigger>(new SimpleTriggerImpl()));

        SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
        schedulerFactoryObject.JobFactory = null;
        schedulerFactoryObject.Triggers = new ITrigger[] { trigger0, trigger1 };
        if (overwrite)
        {
            schedulerFactoryObject.OverwriteExistingJobs = true;
        }

        try
        {
            schedulerFactoryObject.AfterPropertiesSet();
            await schedulerFactoryObject.Start();
        }
        finally
        {
            schedulerFactoryObject.Dispose();
        }

        A.CallTo(() => scheduler.AddJob(trigger0.JobDetail, AddJobOptions.ReplacingAndStoringNonDurable, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.ScheduleJob(trigger0, A<ScheduleJobOptions>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.ScheduleJob(trigger1, A<ScheduleJobOptions>._, A<CancellationToken>._)).MustNotHaveHappened();
        if (overwrite)
        {
            A.CallTo(() => scheduler.RescheduleJob(new TriggerKey("myTrigger1", TriggerKey.DefaultGroup), trigger1, A<CancellationToken>._)).MustHaveHappened();
            A.CallTo(() => scheduler.AddJob(trigger1.JobDetail, AddJobOptions.ReplacingAndStoringNonDurable, A<CancellationToken>._)).MustHaveHappened();
        }
        else
        {
            A.CallTo(() => scheduler.RescheduleJob(A<TriggerKey>._, A<ITrigger>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => scheduler.AddJob(trigger1.JobDetail, A<AddJobOptions>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        A.CallTo(() => scheduler.Start(A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.Shutdown(false, A<CancellationToken>._)).MustHaveHappened();
    }

    /// <summary>
    /// Executes parametrized test.
    /// </summary>
    [Test]
    public void TestSchedulerFactoryObjectWithExistingJobsAndRaceCondition()
    {
        DoTestSchedulerFactoryObjectWithExistingJobsAndRaceCondition(false);
    }

    /// <summary>
    /// Executes parametrized test.
    /// </summary>
    [Test]
    public void TestSchedulerFactoryObjectWithOverwriteExistingJobsAndRaceCondition()
    {
        DoTestSchedulerFactoryObjectWithExistingJobsAndRaceCondition(true);
    }

    private void DoTestSchedulerFactoryObjectWithExistingJobsAndRaceCondition(bool overwrite)
    {
        CronTriggerObject trigger0 = CreateCronTrigger("myTrigger0", "myJob0");

        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();
        // somebody else (another cluster node) schedules the trigger between the existence check and our own scheduling
        A.CallTo(() => scheduler.ScheduleJob(trigger0, A<ScheduleJobOptions>._, A<CancellationToken>._)).Throws(new ObjectAlreadyExistsException(""));

        SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
        schedulerFactoryObject.JobFactory = null;
        schedulerFactoryObject.Triggers = new ITrigger[] { trigger0 };
        schedulerFactoryObject.OverwriteExistingJobs = overwrite;
        try
        {
            schedulerFactoryObject.AfterPropertiesSet();
        }
        finally
        {
            schedulerFactoryObject.Dispose();
        }

        A.CallTo(() => scheduler.AddJob(trigger0.JobDetail, AddJobOptions.ReplacingAndStoringNonDurable, A<CancellationToken>._)).MustHaveHappened();
        if (overwrite)
        {
            A.CallTo(() => scheduler.RescheduleJob(new TriggerKey("myTrigger0", TriggerKey.DefaultGroup), trigger0, A<CancellationToken>._)).MustHaveHappened();
        }
        else
        {
            A.CallTo(() => scheduler.RescheduleJob(A<TriggerKey>._, A<ITrigger>._, A<CancellationToken>._)).MustNotHaveHappened();
        }
    }

    private static CronTriggerObject CreateCronTrigger(string name, string jobName)
    {
        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(NoOpJob);
        jobDetail.ObjectName = jobName;
        jobDetail.AfterPropertiesSet();

        CronTriggerObject trigger = new CronTriggerObject();
        trigger.ObjectName = name;
        trigger.JobDetail = jobDetail;
        trigger.CronExpressionString = "0/1 * * * * ?";
        trigger.AfterPropertiesSet();
        return trigger;
    }

    private static SimpleTriggerObject CreateSimpleTrigger(string name, string jobName)
    {
        MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
        mijdfb.ObjectName = jobName;
        mijdfb.TargetObject = new TestMethodInvokingTask();
        mijdfb.TargetMethod = "doSomething";
        mijdfb.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = name;
        trigger.JobDetail = (IJobDetail) mijdfb.GetObject();
        trigger.StartDelay = TimeSpan.FromMilliseconds(0);
        trigger.RepeatInterval = TimeSpan.FromMilliseconds(20);
        trigger.AfterPropertiesSet();
        return trigger;
    }

    /// <summary>
    /// </summary>
    [Test]
    public async Task TestSchedulerFactoryObjectWithPlainQuartzObjects()
    {
        IJobFactory jobFactory = new AdaptableJobFactory();

        IJobDetail jobDetail0 = JobBuilder.Create().OfType(typeof(NoOpJob)).WithIdentity("myJob0").Build();

        CronTriggerImpl trigger0 = new CronTriggerImpl
        {
            Key = new TriggerKey("myTrigger0"),
            JobKey = new JobKey("myJob0"),
            CronExpressionString = "0/1 * * * * ?"
        };

        TestMethodInvokingTask task1 = new TestMethodInvokingTask();
        MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
        mijdfb.Name = "myJob1";
        mijdfb.Group = JobKey.DefaultGroup;
        mijdfb.TargetObject = task1;
        mijdfb.TargetMethod = "doSomething";
        mijdfb.AfterPropertiesSet();
        IJobDetail jobDetail1 = (IJobDetail) mijdfb.GetObject();

        SimpleTriggerImpl trigger1 = new SimpleTriggerImpl
        {
            Key = new TriggerKey("myTrigger1"),
            JobKey = new JobKey("myJob1"),
            StartTimeUtc = DateTime.UtcNow,
            RepeatCount = SimpleTriggerImpl.RepeatIndefinitely,
            RepeatInterval = TimeSpan.FromMilliseconds(20)
        };

        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();

        SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);

        schedulerFactoryObject.JobFactory = jobFactory;
        schedulerFactoryObject.JobDetails = new IJobDetail[] { jobDetail0, jobDetail1 };
        schedulerFactoryObject.Triggers = new ITrigger[] { trigger0, trigger1 };
        try
        {
            schedulerFactoryObject.AfterPropertiesSet();
            await schedulerFactoryObject.Start();
        }
        finally
        {
            schedulerFactoryObject.Dispose();
        }

        A.CallTo(() => scheduler.AddJob(jobDetail0, AddJobOptions.ReplacingAndStoringNonDurable, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.AddJob(jobDetail1, AddJobOptions.ReplacingAndStoringNonDurable, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.GetJobDetail(new JobKey("myJob0", JobKey.DefaultGroup), A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.GetJobDetail(new JobKey("myJob1", JobKey.DefaultGroup), A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.GetTrigger(new TriggerKey("myTrigger0", TriggerKey.DefaultGroup), A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => scheduler.GetTrigger(new TriggerKey("myTrigger1", TriggerKey.DefaultGroup), A<CancellationToken>._)).MustHaveHappened();
    }

    /// <summary>
    /// </summary>
    [Test]
    public async Task TestSchedulerFactoryObjectWithApplicationContext()
    {
        TestObject tb = new TestObject("tb", 99);
        StaticApplicationContext ac = new StaticApplicationContext();

        IScheduler scheduler = TestSchedulerFactoryObject.CreateFakeScheduler();

        SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
        schedulerFactoryObject.JobFactory = null;
        IDictionary schedulerContextMap = new Hashtable();
        schedulerContextMap.Add("testObject", tb);
        schedulerFactoryObject.SchedulerContextAsMap = schedulerContextMap;
        schedulerFactoryObject.ApplicationContext = ac;
        schedulerFactoryObject.ApplicationContextSchedulerContextKey = "appCtx";
        try
        {
            schedulerFactoryObject.AfterPropertiesSet();
            await schedulerFactoryObject.Start();
            IScheduler returnedScheduler = (IScheduler) schedulerFactoryObject.GetObject();
            Assert.That(returnedScheduler.Context["testObject"], Is.EqualTo(tb));
            Assert.That(returnedScheduler.Context["appCtx"], Is.EqualTo(ac));
        }
        finally
        {
            schedulerFactoryObject.Dispose();
        }
    }

    /// <summary>
    /// </summary>
    [Test]
    public void TestJobDetailObjectWithApplicationContext()
    {
        TestObject tb = new TestObject("tb", 99);
        StaticApplicationContext ac = new StaticApplicationContext();

        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(NoOpJob);
        jobDetail.ObjectName = "myJob0";
        IDictionary jobData = new Hashtable();
        jobData.Add("testObject", tb);
        jobDetail.JobDataAsMap = jobData;
        jobDetail.ApplicationContext = ac;
        jobDetail.ApplicationContextJobDataKey = "appCtx";
        jobDetail.AfterPropertiesSet();

        Assert.That(jobDetail.JobDataMap["testObject"], Is.EqualTo(tb));
        Assert.That(jobDetail.JobDataMap["appCtx"], Is.EqualTo(ac));
    }

    /// <summary>
    /// </summary>
    [Test]
    public async Task TestSchedulerWithTaskExecutor()
    {
        CountingTaskExecutor taskExecutor = new CountingTaskExecutor();
        DummyJob.count = 0;

        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(DummyJob);
        jobDetail.ObjectName = "myJob";
        jobDetail.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "myTrigger";
        trigger.JobDetail = jobDetail;
        trigger.StartDelay = TimeSpan.FromMilliseconds(1);
        trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
        trigger.RepeatCount = 1;
        trigger.AfterPropertiesSet();

        SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
        factoryObject.TaskExecutor = taskExecutor;
        factoryObject.Triggers = new ITrigger[] { trigger };
        factoryObject.JobDetails = new IJobDetail[] { jobDetail };
        factoryObject.AfterPropertiesSet();
        await factoryObject.Start();

        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.That(DummyJob.count > 0, Is.True);
        Assert.That(taskExecutor.count, Is.EqualTo(DummyJob.count));

        factoryObject.Dispose();
    }

    /// <summary>
    /// </summary>
    [Test]
    public async Task TestSchedulerWithQuartzJobObject()
    {
        DummyJobObject.param = 0;
        DummyJobObject.count = 0;

        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(DummyJobObject);
        jobDetail.ObjectName = "myJob";
        jobDetail.JobDataMap.Add("param", "10");
        jobDetail.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "myTrigger";
        trigger.JobDetail = jobDetail;
        trigger.StartDelay = TimeSpan.FromMilliseconds(1);
        trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
        trigger.RepeatCount = 1;
        trigger.AfterPropertiesSet();

        SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
        factoryObject.Triggers = new ITrigger[] { trigger };
        factoryObject.JobDetails = new IJobDetail[] { jobDetail };
        factoryObject.AfterPropertiesSet();
        await factoryObject.Start();

        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.That(DummyJobObject.param, Is.EqualTo(10));
        Assert.That(DummyJobObject.count > 0, Is.True);

        factoryObject.Dispose();
    }

    /// <summary>
    /// </summary>
    [Test]
    public async Task TestSchedulerWithSpringObjectJobFactory()
    {
        DummyJob.param = 0;
        DummyJob.count = 0;

        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(DummyJob);
        jobDetail.ObjectName = "myJob";
        jobDetail.JobDataMap.Add("param", "10");
        jobDetail.JobDataMap.Add("ignoredParam", "10");
        jobDetail.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "myTrigger";
        trigger.JobDetail = jobDetail;
        trigger.StartDelay = TimeSpan.FromMilliseconds(1);
        trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
        trigger.RepeatCount = 1;
        trigger.AfterPropertiesSet();

        SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
        factoryObject.JobFactory = new SpringObjectJobFactory();
        factoryObject.Triggers = new ITrigger[] { trigger };
        factoryObject.JobDetails = new IJobDetail[] { jobDetail };
        factoryObject.AfterPropertiesSet();
        await factoryObject.Start();

        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.That(DummyJob.param, Is.EqualTo(10));
        Assert.That(DummyJob.count > 0, Is.True);

        factoryObject.Dispose();
    }

    /// <summary>
    /// </summary>
    [Test]
    public async Task TestSchedulerWithSpringObjectJobFactoryAndParamMismatchNotIgnored()
    {
        DummyJob.param = 0;
        DummyJob.count = 0;

        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(DummyJob);
        jobDetail.ObjectName = "myJob";
        jobDetail.JobDataMap.Add("para", "10");
        jobDetail.JobDataMap.Add("ignoredParam", "10");
        jobDetail.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "myTrigger";
        trigger.JobDetail = jobDetail;
        trigger.StartDelay = TimeSpan.FromMilliseconds(1);
        trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
        trigger.RepeatCount = 1;
        trigger.AfterPropertiesSet();

        SchedulerFactoryObject bean = new SchedulerFactoryObject();
        SpringObjectJobFactory jobFactory = new SpringObjectJobFactory();
        jobFactory.IgnoredUnknownProperties = new String[] { "ignoredParam" };
        bean.JobFactory = jobFactory;
        bean.Triggers = new ITrigger[] { trigger };
        bean.JobDetails = new IJobDetail[] { jobDetail };
        bean.AfterPropertiesSet();
        await bean.Start();

        await Task.Delay(TimeSpan.FromSeconds(1));
        Assert.That(DummyJob.param, Is.EqualTo(0));
        Assert.That(DummyJob.count == 0, Is.True);

        bean.Dispose();
    }

    ///<summary>
    ///</summary>
    [Test]
    public async Task TestSchedulerWithSpringObjectJobFactoryAndQuartzJobObject()
    {
        DummyJobObject.param = 0;
        DummyJobObject.count = 0;

        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(DummyJobObject);
        jobDetail.ObjectName = "myJob";
        jobDetail.JobDataMap.Add("param", "10");
        jobDetail.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "myTrigger";
        trigger.JobDetail = jobDetail;
        trigger.StartDelay = TimeSpan.FromMilliseconds(1);
        trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
        trigger.RepeatCount = 1;
        trigger.AfterPropertiesSet();

        SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
        factoryObject.JobFactory = new SpringObjectJobFactory();
        factoryObject.Triggers = new ITrigger[] { trigger };
        factoryObject.JobDetails = new IJobDetail[] { jobDetail };
        factoryObject.AfterPropertiesSet();
        await factoryObject.Start();

        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.That(DummyJobObject.param, Is.EqualTo(10));
        Assert.That(DummyJobObject.count > 0, Is.True);

        factoryObject.Dispose();
    }

    /// <summary>
    /// The scheduler context reaches a job created by the default job factory too, through QuartzJobObject.
    /// </summary>
    [Test]
    public async Task TestSchedulerContextReachesQuartzJobObject()
    {
        DummyJobObject.param = 0;
        DummyJobObject.count = 0;

        JobDetailObject jobDetail = new JobDetailObject();
        jobDetail.JobType = typeof(DummyJobObject);
        jobDetail.ObjectName = "myJob";
        jobDetail.AfterPropertiesSet();

        SimpleTriggerObject trigger = new SimpleTriggerObject();
        trigger.ObjectName = "myTrigger";
        trigger.JobDetail = jobDetail;
        trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
        trigger.RepeatCount = 1;
        trigger.AfterPropertiesSet();

        SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
        factoryObject.SchedulerContextAsMap = new Hashtable { ["param"] = 7 };
        factoryObject.Triggers = new ITrigger[] { trigger };
        factoryObject.JobDetails = new IJobDetail[] { jobDetail };
        factoryObject.AfterPropertiesSet();
        await factoryObject.Start();

        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.That(DummyJobObject.param, Is.EqualTo(7));

        factoryObject.Dispose();
    }

    /// <summary>
    /// Tests the creation of multiple schedulers (SPR-772)
    /// </summary>
    [Test]
    public void TestMultipleSchedulers()
    {
        XmlApplicationContext ctx = new XmlApplicationContext("multipleSchedulers.xml");
        try
        {
            IScheduler scheduler1 = (IScheduler) ctx.GetObject("scheduler1");
            IScheduler scheduler2 = (IScheduler) ctx.GetObject("scheduler2");
            Assert.That(scheduler2, Is.Not.SameAs(scheduler1));
            Assert.That(scheduler1.SchedulerName, Is.EqualTo("quartz1"));
            Assert.That(scheduler2.SchedulerName, Is.EqualTo("quartz2"));

            XmlApplicationContext ctx2 = new XmlApplicationContext("multipleSchedulers.xml");
            try
            {
                IScheduler scheduler1a = (IScheduler) ctx2.GetObject("scheduler1");
                IScheduler scheduler2a = (IScheduler) ctx2.GetObject("scheduler2");
                Assert.That(scheduler2a, Is.Not.SameAs(scheduler1a));
                Assert.That(scheduler1, Is.Not.SameAs(scheduler1a));
                Assert.That(scheduler2, Is.Not.SameAs(scheduler2a));
                Assert.That(scheduler1a.SchedulerName, Is.EqualTo("quartz1"));
                Assert.That(scheduler2a.SchedulerName, Is.EqualTo("quartz2"));
            }
            finally
            {
                ctx2.Dispose();
            }
        }
        finally
        {
            ctx.Dispose();
        }
    }

    /// <summary>
    /// Tests calling of services with method invoke.
    /// </summary>
    [Test]
    public void TestWithTwoAnonymousMethodInvokingJobDetailFactoryObjects()
    {
        XmlApplicationContext ctx = new XmlApplicationContext("multipleAnonymousMethodInvokingJobDetailFB.xml");
        Thread.Sleep(3000);
        try
        {
            QuartzTestObject exportService = (QuartzTestObject) ctx.GetObject("exportService");
            QuartzTestObject importService = (QuartzTestObject) ctx.GetObject("importService");

            Assert.That(exportService.ImportCount, Is.EqualTo(0), "doImport called exportService");
            Assert.That(exportService.ExportCount, Is.EqualTo(2), "doExport not called on exportService");
            Assert.That(importService.ImportCount, Is.EqualTo(2), "doImport not called on importService");
            Assert.That(importService.ExportCount, Is.EqualTo(0), "doExport called on importService");
        }
        finally
        {
            ctx.Dispose();
        }
    }

    /// <summary>
    /// Tests how quartz triggers and services interact.
    /// </summary>
    [Test]
    public void TestSchedulerAccessorObject()
    {
        XmlApplicationContext ctx = new XmlApplicationContext("schedulerAccessorObject.xml");
        Thread.Sleep(3000);
        try
        {
            QuartzTestObject exportService = (QuartzTestObject) ctx.GetObject("exportService");
            QuartzTestObject importService = (QuartzTestObject) ctx.GetObject("importService");

            Assert.That(exportService.ImportCount, Is.EqualTo(0), "doImport called exportService");
            Assert.That(exportService.ExportCount, Is.EqualTo(2), "doExport not called on exportService");
            Assert.That(importService.ImportCount, Is.EqualTo(2), "doImport not called on importService");
            Assert.That(importService.ExportCount, Is.EqualTo(0), "doExport called on importService");
        }
        finally
        {
            ctx.Dispose();
        }
    }

    [Test]
    public void TestSchedulerAutoStartsOnContextRefreshedEventByDefault()
    {
        StaticApplicationContext context = new StaticApplicationContext();
        context.RegisterObjectDefinition("scheduler", new RootObjectDefinition(typeof(SchedulerFactoryObject)));
        IScheduler scheduler = (IScheduler) context.GetObject("scheduler", typeof(IScheduler));
        try
        {
            Assert.That(scheduler.Status, Is.Not.EqualTo(SchedulerStatus.Running));
            context.Refresh();
            Assert.That(scheduler.Status, Is.EqualTo(SchedulerStatus.Running));
        }
        finally
        {
            context.Dispose();
        }
    }

    [Test]
    public void TestSchedulerAutoStartupFalse()
    {
        StaticApplicationContext context = new StaticApplicationContext();
        ObjectDefinitionBuilder beanDefinition = ObjectDefinitionBuilder
            .GenericObjectDefinition(typeof(SchedulerFactoryObject))
            .AddPropertyValue("autoStartup", false);

        context.RegisterObjectDefinition("scheduler", beanDefinition.ObjectDefinition);
        IScheduler scheduler = (IScheduler) context.GetObject("scheduler", typeof(IScheduler));
        try
        {
            Assert.That(scheduler.Status, Is.Not.EqualTo(SchedulerStatus.Running));
            context.Refresh();
            Assert.That(scheduler.Status, Is.Not.EqualTo(SchedulerStatus.Running));
        }
        finally
        {
            context.Dispose();
        }
    }

    /// <summary>
    /// The typed options can be set from an object definition, with the types that XML has: numbers, time spans and booleans.
    /// </summary>
    [Test]
    public void TestTypedPropertiesFromObjectDefinition()
    {
        StaticApplicationContext context = new StaticApplicationContext();
        ObjectDefinitionBuilder definition = ObjectDefinitionBuilder
            .GenericObjectDefinition(typeof(SchedulerFactoryObject))
            .AddPropertyValue("autoStartup", false)
            .AddPropertyValue("SchedulerName", "fromDefinition")
            .AddPropertyValue("MaxConcurrency", "3")
            .AddPropertyValue("IdleWaitTime", "5s")
            .AddPropertyValue("MisfireThreshold", "1m")
            .AddPropertyValue("GenerateInstanceId", "false");
        context.RegisterObjectDefinition("scheduler", definition.ObjectDefinition);
        try
        {
            IScheduler scheduler = (IScheduler) context.GetObject("scheduler", typeof(IScheduler));

            Assert.That(scheduler.SchedulerName, Is.EqualTo("fromDefinition"));
            Assert.That(scheduler.GetMetadata().AsTask().GetAwaiter().GetResult().ThreadPoolSize, Is.EqualTo(3));
        }
        finally
        {
            context.Dispose();
        }
    }

    /// <summary>
    /// Tests how scheduler is exposed to application context.
    /// </summary>
    [Test]
    public void TestSchedulerRepositoryExposure()
    {
        XmlApplicationContext ctx = new XmlApplicationContext("schedulerRepositoryExposure.xml");
        Thread.Sleep(3000);
        try
        {
            // the accessor found the scheduler by its name and registered the triggers on it
            QuartzTestObject exportService = (QuartzTestObject) ctx.GetObject("exportService");
            QuartzTestObject importService = (QuartzTestObject) ctx.GetObject("importService");
            Assert.That(ctx.GetObject("scheduler"), Is.InstanceOf<IScheduler>());
            Assert.That(exportService.ExportCount, Is.EqualTo(2), "doExport not called on exportService");
            Assert.That(importService.ImportCount, Is.EqualTo(2), "doImport not called on importService");
        }
        finally
        {
            ctx.Dispose();
        }
    }

    /// <summary>
    /// Simple task executor that tracks invocation count.
    /// </summary>
    public class CountingTaskExecutor : ITaskExecutor
    {
        internal int count;

        /// <summary>
        /// Executes task instance.
        /// </summary>
        /// <param name="task"></param>
        public void Execute(ThreadStart task)
        {
            Interlocked.Increment(ref count);
            task.Invoke();
        }
    }

    /// <summary>
    /// Simple test job object.
    /// </summary>
    public class DummyJobObject : QuartzJobObject
    {
        internal static int param;
        internal static int count;

        /// <summary>
        /// Sets parameter value.
        /// </summary>
        /// <param name="value"></param>
        public void SetParam(int value)
        {
            if (param > 0)
            {
                throw new NotSupportedException("Param already set");
            }

            param = value;
        }

        /// <summary>
        /// Execute the actual job. The job data map will already have been
        /// applied as object property values by execute. The contract is
        /// exactly the same as for the standard Quartz execute method.
        /// </summary>
        protected override ValueTask ExecuteInternal(IJobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
        {
            count++;
            return ValueTask.CompletedTask;
        }
    }
}

/// <summary>
/// A simple job that tracks invocation count and allows setting of a simple parameter.
/// </summary>
public class DummyJob : IJob
{
    internal static int param;
    internal static int count;

    /// <summary>
    /// Sets param value.
    /// </summary>
    /// <param name="value"></param>
    public void SetParam(int value)
    {
        if (param > 0)
        {
            throw new NotSupportedException("Param already set");
        }

        param = value;
    }

    ///<summary>
    /// Executes this job instance.
    ///</summary>
    ///<param name="jobExecutionContext"></param>
    ///<param name="cancellationToken"></param>
    public ValueTask Execute(IJobExecutionContext jobExecutionContext, CancellationToken cancellationToken = default)
    {
        count++;
        return ValueTask.CompletedTask;
    }
}
