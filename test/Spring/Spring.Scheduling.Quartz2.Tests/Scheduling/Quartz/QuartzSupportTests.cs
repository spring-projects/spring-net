/* 
 * Copyright 2002-2010 the original author or authors.
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

using System;
using System.Collections;
using System.Threading;

using NUnit.Framework;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;

using Rhino.Mocks;

using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Support;

namespace Spring.Scheduling.Quartz
{
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
        public void TestSchedulerFactoryObject()
        {
            DoTestSchedulerFactoryObject(false, false);
        }

        /// <summary>
        /// Executes parametrized test.
        /// </summary>
        [Test]
        public void TestSchedulerFactoryObjectWithExplicitJobDetail()
        {
            DoTestSchedulerFactoryObject(true, false);
        }

        /// <summary>
        /// Executes parametrized test.
        /// </summary>
        [Test]
        [Ignore("Requires change to MethodInvoker for overriding target object and type")]
        public void TestSchedulerFactoryObjectWithPrototypeJob()
        {
            DoTestSchedulerFactoryObject(false, true);
        }

        private void DoTestSchedulerFactoryObject(bool explicitJobDetail, bool prototypeJob)
        {
            TestObject tb = new TestObject("tb", 99);
            JobDetailObject jobDetail0 = new JobDetailObject();
            jobDetail0.JobType = typeof (IJob);
            jobDetail0.ObjectName = ("myJob0");
            IDictionary jobData = new Hashtable();
            jobData.Add("testObject", tb);
            jobDetail0.JobDataAsMap = (jobData);
            jobDetail0.AfterPropertiesSet();
            Assert.AreEqual(tb, jobDetail0.JobDataMap.Get("testObject"));

            CronTriggerObject trigger0 = new CronTriggerObject();
            trigger0.ObjectName = ("myTrigger0");
            trigger0.JobDetail = (jobDetail0);
            trigger0.CronExpressionString = ("0/1 * * * * ?");
            trigger0.AfterPropertiesSet();

            TestMethodInvokingTask task1 = new TestMethodInvokingTask();
            MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
            mijdfb.ObjectName = ("myJob1");
            if (prototypeJob)
            {
                StaticListableObjectFactory objectFactory = new StaticListableObjectFactory();
                objectFactory.AddObject("task", task1);
                mijdfb.TargetObjectName = ("task");
                mijdfb.ObjectFactory = objectFactory;
            }
            else
            {
                mijdfb.TargetObject = (task1);
            }
            mijdfb.TargetMethod = ("doSomething");
            mijdfb.AfterPropertiesSet();
            IJobDetail jobDetail1 = (IJobDetail) mijdfb.GetObject();

            SimpleTriggerObject trigger1 = new SimpleTriggerObject();
            trigger1.ObjectName = ("myTrigger1");
            trigger1.JobDetail = (jobDetail1);
            trigger1.StartDelay = TimeSpan.FromMilliseconds(0);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);
            trigger1.AfterPropertiesSet();

            IScheduler scheduler = MockRepository.GenerateMock<IScheduler>();

            scheduler.Stub(x => x.Context).Return(new SchedulerContext());
            
            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
            schedulerFactoryObject.JobFactory = (null);
            IDictionary schedulerContext = new Hashtable();
            schedulerContext.Add("otherTestObject", tb);
            schedulerFactoryObject.SchedulerContextAsMap = (schedulerContext);
            if (explicitJobDetail)
            {
                schedulerFactoryObject.JobDetails = (new IJobDetail[] {jobDetail0});
            }
            schedulerFactoryObject.Triggers = (new ITrigger[] {trigger0, trigger1});
            try
            {
                schedulerFactoryObject.AfterPropertiesSet();
                schedulerFactoryObject.Start();
            }
            finally
            {
                schedulerFactoryObject.Dispose();
            }

            scheduler.AssertWasCalled(x => x.ScheduleJob(trigger0));
            scheduler.AssertWasCalled(x => x.ScheduleJob(trigger1));
            
            scheduler.AssertWasCalled(x => x.AddJob(jobDetail0, true, true));
            scheduler.AssertWasCalled(x => x.AddJob(jobDetail0, true, true));
            
            scheduler.AssertWasCalled(x => x.Start());
            scheduler.AssertWasCalled(x => x.Shutdown(false));
        }

        /// <summary>
        /// Executes parametrized test.
        /// </summary>
        [Test]
        public void TestSchedulerFactoryObjectWithExistingJobs()
        {
            DoTestSchedulerFactoryObjectWithExistingJobs(false);
        }

        /// <summary>
        /// Executes parametrized test.
        /// </summary>
        [Test]
        public void TestSchedulerFactoryObjectWithOverwriteExistingJobs()
        {
            DoTestSchedulerFactoryObjectWithExistingJobs(true);
        }

        private void DoTestSchedulerFactoryObjectWithExistingJobs(bool overwrite)
        {
            TestObject tb = new TestObject("tb", 99);
            JobDetailObject jobDetail0 = new JobDetailObject();
            jobDetail0.JobType = typeof (IJob);
            jobDetail0.ObjectName = ("myJob0");
            IDictionary jobData = new Hashtable();
            jobData.Add("testObject", tb);
            jobDetail0.JobDataAsMap = (jobData);
            jobDetail0.AfterPropertiesSet();
            Assert.AreEqual(tb, jobDetail0.JobDataMap.Get("testObject"));

            CronTriggerObject trigger0 = new CronTriggerObject();
            trigger0.ObjectName = ("myTrigger0");
            trigger0.JobDetail = (jobDetail0);
            trigger0.CronExpressionString = ("0/1 * * * * ?");
            trigger0.AfterPropertiesSet();

            TestMethodInvokingTask task1 = new TestMethodInvokingTask();
            MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
            mijdfb.ObjectName = ("myJob1");
            mijdfb.TargetObject = (task1);
            mijdfb.TargetMethod = ("doSomething");
            mijdfb.AfterPropertiesSet();
            IJobDetail jobDetail1 = (IJobDetail) mijdfb.GetObject();

            SimpleTriggerObject trigger1 = new SimpleTriggerObject();
            trigger1.ObjectName = ("myTrigger1");
            trigger1.JobDetail = (jobDetail1);
            trigger1.StartDelay = TimeSpan.FromMilliseconds(0);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);
            trigger1.AfterPropertiesSet();

            IScheduler scheduler = MockRepository.GenerateMock<IScheduler>();
            scheduler.Stub(x => x.Context).Return(new SchedulerContext());
            scheduler.Stub(x => x.GetTrigger(new TriggerKey("myTrigger1", SchedulerConstants.DefaultGroup))).Return(new SimpleTriggerImpl());

            if (overwrite)
            {
                scheduler.Stub(x => x.RescheduleJob(new TriggerKey("myTrigger1", SchedulerConstants.DefaultGroup), trigger1)).Return(DateTime.UtcNow);
            }

            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
            schedulerFactoryObject.JobFactory = (null);
            IDictionary schedulerContext = new Hashtable();
            schedulerContext.Add("otherTestObject", tb);
            schedulerFactoryObject.SchedulerContextAsMap = (schedulerContext);
            schedulerFactoryObject.Triggers = (new ITrigger[] {trigger0, trigger1});
            if (overwrite)
            {
                schedulerFactoryObject.OverwriteExistingJobs = (true);
            }
            try
            {
                schedulerFactoryObject.AfterPropertiesSet();
                schedulerFactoryObject.Start();
            }
            finally
            {
                schedulerFactoryObject.Dispose();
            }

            scheduler.AssertWasCalled(x => x.AddJob(jobDetail0, true, true));
            scheduler.AssertWasCalled(x => x.ScheduleJob(trigger0));
            scheduler.AssertWasCalled(x => x.Start());
            scheduler.AssertWasCalled(x => x.Shutdown(false));
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
            TestObject tb = new TestObject("tb", 99);
            JobDetailObject jobDetail0 = new JobDetailObject();
            jobDetail0.JobType = typeof (IJob);
            jobDetail0.ObjectName = ("myJob0");
            IDictionary jobData = new Hashtable();
            jobData.Add("testObject", tb);
            jobDetail0.JobDataAsMap = (jobData);
            jobDetail0.AfterPropertiesSet();
            Assert.AreEqual(tb, jobDetail0.JobDataMap.Get("testObject"));

            CronTriggerObject trigger0 = new CronTriggerObject();
            trigger0.ObjectName = ("myTrigger0");
            trigger0.JobDetail = (jobDetail0);
            trigger0.CronExpressionString = ("0/1 * * * * ?");
            trigger0.AfterPropertiesSet();

            TestMethodInvokingTask task1 = new TestMethodInvokingTask();
            MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
            mijdfb.ObjectName = ("myJob1");
            mijdfb.TargetObject = (task1);
            mijdfb.TargetMethod = ("doSomething");
            mijdfb.AfterPropertiesSet();
            IJobDetail jobDetail1 = (IJobDetail) mijdfb.GetObject();

            SimpleTriggerObject trigger1 = new SimpleTriggerObject();
            trigger1.ObjectName = ("myTrigger1");
            trigger1.JobDetail = (jobDetail1);
            trigger1.StartDelay = TimeSpan.FromMilliseconds(0);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);
            trigger1.AfterPropertiesSet();

            IScheduler scheduler = MockRepository.GenerateMock<IScheduler>();
            scheduler.Stub(x => x.Context).Return(new SchedulerContext());
            scheduler.Stub(x => x.GetTrigger(new TriggerKey("myTrigger1", SchedulerConstants.DefaultGroup))).Return(new SimpleTriggerImpl());
            if (overwrite)
            {
                scheduler.AddJob(jobDetail1, true);
                scheduler.Stub(x => x.RescheduleJob(new TriggerKey("myTrigger1", SchedulerConstants.DefaultGroup), trigger1)).Return(DateTime.UtcNow);
            }
            
            scheduler.Stub(x => x.ScheduleJob(trigger0)).Throw(new ObjectAlreadyExistsException(""));

            if (overwrite)
            {
                scheduler.Stub(x => x.RescheduleJob(new TriggerKey("myTrigger0", SchedulerConstants.DefaultGroup), trigger0)).Return(DateTime.UtcNow);
            }

            scheduler.Start();
            scheduler.Shutdown(false);

            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);

            schedulerFactoryObject.JobFactory = (null);
            IDictionary schedulerContext = new Hashtable();
            schedulerContext.Add("otherTestObject", tb);
            schedulerFactoryObject.SchedulerContextAsMap = (schedulerContext);
            schedulerFactoryObject.Triggers = (new ITrigger[] {trigger0, trigger1});
            if (overwrite)
            {
                schedulerFactoryObject.OverwriteExistingJobs = (true);
            }
            try
            {
                schedulerFactoryObject.AfterPropertiesSet();
                schedulerFactoryObject.Start();
            }
            finally
            {
                schedulerFactoryObject.Dispose();
            }

            scheduler.AssertWasCalled(x => x.AddJob(jobDetail0, true, true));

        }

        private void MethodInvokingConcurrency(bool concurrent)
        {
            // Test the concurrency flag.
            // Method invoking job with two triggers.
            // If the concurrent flag is false, the triggers are NOT allowed
            // to interfere with each other.

            TestMethodInvokingTask task1 = new TestMethodInvokingTask();
            MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
            // set the concurrency flag!
            mijdfb.Concurrent = (concurrent);
            mijdfb.ObjectName = ("myJob1");
            mijdfb.TargetObject = (task1);
            mijdfb.TargetMethod = ("doWait");
            mijdfb.AfterPropertiesSet();
            IJobDetail jobDetail1 = (IJobDetail) mijdfb.GetObject();

            SimpleTriggerObject trigger0 = new SimpleTriggerObject();
            trigger0.ObjectName = ("myTrigger1");
            trigger0.JobDetail = (jobDetail1);
            trigger0.StartDelay = TimeSpan.FromMilliseconds(0);
            trigger0.RepeatInterval = TimeSpan.FromMilliseconds(1);
            trigger0.RepeatCount = (1);
            trigger0.AfterPropertiesSet();

            SimpleTriggerObject trigger1 = new SimpleTriggerObject();
            trigger1.ObjectName = ("myTrigger1");
            trigger1.JobDetail = (jobDetail1);
            trigger1.StartDelay = TimeSpan.FromMilliseconds(1000L);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(1);
            trigger1.RepeatCount = (1);
            trigger1.AfterPropertiesSet();

            SchedulerFactoryObject schedulerFactoryObject = new SchedulerFactoryObject();
            schedulerFactoryObject.JobDetails = (new IJobDetail[] {jobDetail1});
            schedulerFactoryObject.Triggers = (new ITrigger[] {trigger1, trigger0});
            schedulerFactoryObject.AfterPropertiesSet();

            // ok scheduler is set up... let's wait for like 4 seconds
            try
            {
                Thread.Sleep(4000);
            }
            catch (ThreadInterruptedException)
            {
                // fall through
            }

            if (concurrent)
            {
                Assert.AreEqual(2, task1.counter);
                task1.Stop();
                // we're done, both jobs have ran, let's call it a day
                return;
            }
            else
            {
                Assert.AreEqual(1, task1.counter);
                task1.Stop();
                // we need to check whether or not the test succeed with non-concurrent jobs
            }

            try
            {
                Thread.Sleep(4000);
            }
            catch (ThreadInterruptedException)
            {
                // fall through
            }

            task1.Stop();
            Assert.AreEqual(2, task1.counter);

            // Although we're destroying the scheduler, it does seem to keep things in memory:
            // When executing both tests (concurrent and non-concurrent), the second test always
            // fails.
            schedulerFactoryObject.Dispose();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerFactoryObjectWithPlainQuartzObjects()
        {
            IJobFactory jobFactory = new AdaptableJobFactory();

            TestObject tb = new TestObject("tb", 99);
            JobDetailImpl jobDetail0 = new JobDetailImpl();
            jobDetail0.JobType = typeof (IJob);
            jobDetail0.Name = ("myJob0");
            jobDetail0.Group = (SchedulerConstants.DefaultGroup);
            jobDetail0.JobDataMap.Add("testObject", tb);
            Assert.AreEqual(tb, jobDetail0.JobDataMap.Get("testObject"));

            CronTriggerImpl trigger0 = new CronTriggerImpl();
            trigger0.Name = ("myTrigger0");
            trigger0.Group = SchedulerConstants.DefaultGroup;
            trigger0.JobName = "myJob0";
            trigger0.JobGroup = SchedulerConstants.DefaultGroup;
            trigger0.StartTimeUtc = (DateTime.UtcNow);
            trigger0.CronExpressionString = ("0/1 * * * * ?");

            TestMethodInvokingTask task1 = new TestMethodInvokingTask();
            MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
            mijdfb.Name = ("myJob1");
            mijdfb.Group = (SchedulerConstants.DefaultGroup);
            mijdfb.TargetObject = (task1);
            mijdfb.TargetMethod = ("doSomething");
            mijdfb.AfterPropertiesSet();
            IJobDetail jobDetail1 = (IJobDetail) mijdfb.GetObject();

            SimpleTriggerImpl trigger1 = new SimpleTriggerImpl();
            trigger1.Name = "myTrigger1";
            trigger1.Group = SchedulerConstants.DefaultGroup;
            trigger1.JobName = "myJob1";
            trigger1.JobGroup = SchedulerConstants.DefaultGroup;
            trigger1.StartTimeUtc = (DateTime.UtcNow);
            trigger1.RepeatCount = (SimpleTriggerImpl.RepeatIndefinitely);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);

            IScheduler scheduler = MockRepository.GenerateMock<IScheduler>();
            
            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);

            schedulerFactoryObject.JobFactory = (jobFactory);
            schedulerFactoryObject.JobDetails = (new IJobDetail[] {jobDetail0, jobDetail1});
            schedulerFactoryObject.Triggers = (new ITrigger[] {trigger0, trigger1});
            try
            {
                schedulerFactoryObject.AfterPropertiesSet();
                schedulerFactoryObject.Start();
            }
            finally
            {
                schedulerFactoryObject.Dispose();
            }

            scheduler.AssertWasCalled(x => x.JobFactory = jobFactory);
            scheduler.AssertWasCalled(x => x.AddJob(jobDetail0, true, true));
            scheduler.AssertWasCalled(x => x.AddJob(jobDetail1, true, true));
            scheduler.AssertWasCalled(x => x.GetJobDetail(new JobKey("myJob0", SchedulerConstants.DefaultGroup)));
            scheduler.AssertWasCalled(x => x.GetJobDetail(new JobKey("myJob1", SchedulerConstants.DefaultGroup)));
            scheduler.AssertWasCalled(x => x.GetTrigger(new TriggerKey("myTrigger0", SchedulerConstants.DefaultGroup)));
            scheduler.AssertWasCalled(x => x.GetTrigger(new TriggerKey("myTrigger1", SchedulerConstants.DefaultGroup)));
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerFactoryObjectWithApplicationContext()
        {
            TestObject tb = new TestObject("tb", 99);
            StaticApplicationContext ac = new StaticApplicationContext();

            IScheduler scheduler = MockRepository.GenerateMock<IScheduler>();
            SchedulerContext schedulerContext = new SchedulerContext();
            scheduler.Stub(x => x.Context).Return(schedulerContext).Repeat.Times(4);

            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
            schedulerFactoryObject.JobFactory = (null);
            IDictionary schedulerContextMap = new Hashtable();
            schedulerContextMap.Add("testObject", tb);
            schedulerFactoryObject.SchedulerContextAsMap = (schedulerContextMap);
            schedulerFactoryObject.ApplicationContext = (ac);
            schedulerFactoryObject.ApplicationContextSchedulerContextKey = ("appCtx");
            try
            {
                schedulerFactoryObject.AfterPropertiesSet();
                schedulerFactoryObject.Start();
                IScheduler returnedScheduler = (IScheduler) schedulerFactoryObject.GetObject();
                Assert.AreEqual(tb, returnedScheduler.Context["testObject"]);
                Assert.AreEqual(ac, returnedScheduler.Context["appCtx"]);
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
            jobDetail.JobType = typeof (IJob);
            jobDetail.ObjectName = ("myJob0");
            IDictionary jobData = new Hashtable();
            jobData.Add("testObject", tb);
            jobDetail.JobDataAsMap = (jobData);
            jobDetail.ApplicationContext = (ac);
            jobDetail.ApplicationContextJobDataKey = ("appCtx");
            jobDetail.AfterPropertiesSet();

            Assert.AreEqual(tb, jobDetail.JobDataMap.Get("testObject"));
            Assert.AreEqual(ac, jobDetail.JobDataMap.Get("appCtx"));
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerWithTaskExecutor()
        {
            CountingTaskExecutor taskExecutor = new CountingTaskExecutor();
            DummyJob.count = 0;

            JobDetailImpl jobDetail = new JobDetailImpl();
            jobDetail.JobType = typeof (DummyJob);
            jobDetail.Name = ("myJob");

            SimpleTriggerObject trigger = new SimpleTriggerObject();
            trigger.Name = ("myTrigger");
            trigger.JobDetail = (jobDetail);
            trigger.StartDelay = TimeSpan.FromMilliseconds(1);
            trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
            trigger.RepeatCount = (1);
            trigger.AfterPropertiesSet();

            SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
            factoryObject.TaskExecutor = (taskExecutor);
            factoryObject.Triggers = (new ITrigger[] {trigger});
            factoryObject.JobDetails = (new IJobDetail[] {jobDetail});
            factoryObject.AfterPropertiesSet();
            factoryObject.Start();

            Thread.Sleep(500);
            Assert.IsTrue(DummyJob.count > 0);
            Assert.AreEqual(DummyJob.count, taskExecutor.count);

            factoryObject.Dispose();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerWithRunnable()
        {
            DummyRunnable.count = 0;

            JobDetailImpl jobDetail = new JobDetailObject();
            jobDetail.JobType = typeof (DummyRunnable);
            jobDetail.Name = "myJob";

            SimpleTriggerObject trigger = new SimpleTriggerObject();
            trigger.Name = "myTrigger";
            trigger.JobDetail = jobDetail;
            trigger.StartDelay = TimeSpan.FromMilliseconds(1);
            trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
            trigger.RepeatCount = 1;
            trigger.AfterPropertiesSet();

            SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
            factoryObject.Triggers = new ITrigger[] {trigger};
            factoryObject.JobDetails = new IJobDetail[] {jobDetail};
            factoryObject.AfterPropertiesSet();
            factoryObject.Start();

            DummyRunnable.runEvent.WaitOne(500);
            Assert.IsTrue(DummyRunnable.count > 0);

            factoryObject.Dispose();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerWithQuartzJobObject()
        {
            DummyJob.param = 0;
            DummyJob.count = 0;

            JobDetailImpl jobDetail = new JobDetailImpl();
            jobDetail.JobType = (typeof (DummyJobObject));
            jobDetail.Name = ("myJob");
            jobDetail.JobDataMap.Put("param", "10");

            SimpleTriggerObject trigger = new SimpleTriggerObject();
            trigger.Name = ("myTrigger");
            trigger.JobDetail = (jobDetail);
            trigger.StartDelay = TimeSpan.FromMilliseconds(1);
            trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
            trigger.RepeatCount = (1);
            trigger.AfterPropertiesSet();

            SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
            factoryObject.Triggers = (new ITrigger[] {trigger});
            factoryObject.JobDetails = (new IJobDetail[] {jobDetail});
            factoryObject.AfterPropertiesSet();
            factoryObject.Start();

            Thread.Sleep(500);
            Assert.AreEqual(10, DummyJobObject.param);
            Assert.IsTrue(DummyJobObject.count > 0);

            factoryObject.Dispose();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerWithSpringObjectJobFactory()
        {
            DummyJob.param = 0;
            DummyJob.count = 0;

            JobDetailImpl jobDetail = new JobDetailImpl();
            jobDetail.JobType = typeof(DummyJob);
            jobDetail.Name = ("myJob");
            jobDetail.JobDataMap.Add("param", "10");
            jobDetail.JobDataMap.Add("ignoredParam", "10");

            SimpleTriggerObject trigger = new SimpleTriggerObject();
            trigger.Name = ("myTrigger");
            trigger.JobDetail = (jobDetail);
            trigger.StartDelay = TimeSpan.FromMilliseconds(1);
            trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
            trigger.RepeatCount = (1);
            trigger.AfterPropertiesSet();

            SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
            factoryObject.JobFactory = (new SpringObjectJobFactory());
            factoryObject.Triggers = (new ITrigger[] {trigger});
            factoryObject.JobDetails = (new IJobDetail[] {jobDetail});
            factoryObject.AfterPropertiesSet();
            factoryObject.Start();

            Thread.Sleep(500);
            Assert.AreEqual(10, DummyJob.param);
            Assert.IsTrue(DummyJob.count > 0);

            factoryObject.Dispose();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerWithSpringObjectJobFactoryAndParamMismatchNotIgnored()
        {
            DummyJob.param = 0;
            DummyJob.count = 0;

            JobDetailImpl jobDetail = new JobDetailImpl();
            jobDetail.JobType = typeof(DummyJob);
            jobDetail.Name = ("myJob");
            jobDetail.JobDataMap.Add("para", "10");
            jobDetail.JobDataMap.Add("ignoredParam", "10");

            SimpleTriggerObject trigger = new SimpleTriggerObject();
            trigger.Name = ("myTrigger");
            trigger.JobDetail = (jobDetail);
            trigger.StartDelay = TimeSpan.FromMilliseconds(1);
            trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
            trigger.RepeatCount = (1);
            trigger.AfterPropertiesSet();

            SchedulerFactoryObject bean = new SchedulerFactoryObject();
            SpringObjectJobFactory jobFactory = new SpringObjectJobFactory();
            jobFactory.IgnoredUnknownProperties = (new String[] {"ignoredParam"});
            bean.JobFactory = (jobFactory);
            bean.Triggers = (new ITrigger[] {trigger});
            bean.JobDetails = (new IJobDetail[] {jobDetail});
            bean.AfterPropertiesSet();

            Thread.Sleep(500);
            Assert.AreEqual(0, DummyJob.param);
            Assert.IsTrue(DummyJob.count == 0);

            bean.Dispose();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerWithSpringObjectJobFactoryAndRunnable()
        {
            DummyRunnable.param = 0;
            DummyRunnable.count = 0;

            JobDetailObject jobDetail = new JobDetailObject();
            jobDetail.JobType = typeof (DummyRunnable);
            jobDetail.Name = ("myJob");
            jobDetail.JobDataMap.Add("param", "10");

            SimpleTriggerObject trigger = new SimpleTriggerObject();
            trigger.Name = ("myTrigger");
            trigger.JobDetail = (jobDetail);
            trigger.StartDelay = TimeSpan.FromMilliseconds(1);
            trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
            trigger.RepeatCount = (1);
            trigger.AfterPropertiesSet();

            SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
            factoryObject.JobFactory = (new SpringObjectJobFactory());
            factoryObject.Triggers = (new ITrigger[] {trigger});
            factoryObject.JobDetails = (new IJobDetail[] {jobDetail});
            factoryObject.AfterPropertiesSet();
            factoryObject.Start();

            Thread.Sleep(500);
            Assert.AreEqual(10, DummyRunnable.param);
            Assert.IsTrue(DummyRunnable.count > 0);

            factoryObject.Dispose();
        }

        ///<summary>
        ///</summary>
        [Test]
        public void TestSchedulerWithSpringObjectJobFactoryAndQuartzJobObject()
        {
            DummyJobObject.param = 0;
            DummyJobObject.count = 0;

            JobDetailImpl jobDetail = new JobDetailImpl();
            jobDetail.JobType = typeof (DummyJobObject);
            jobDetail.Name = ("myJob");
            jobDetail.JobDataMap.Add("param", "10");

            SimpleTriggerObject trigger = new SimpleTriggerObject();
            trigger.Name = ("myTrigger");
            trigger.JobDetail = (jobDetail);
            trigger.StartDelay = TimeSpan.FromMilliseconds(1);
            trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
            trigger.RepeatCount = (1);
            trigger.AfterPropertiesSet();

            SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
            factoryObject.JobFactory = (new SpringObjectJobFactory());
            factoryObject.Triggers = (new ITrigger[] {trigger});
            factoryObject.JobDetails = (new IJobDetail[] {jobDetail});
            factoryObject.AfterPropertiesSet();
            factoryObject.Start();

            Thread.Sleep(500);
            Assert.AreEqual(10, DummyJobObject.param);
            Assert.IsTrue(DummyJobObject.count > 0);

            factoryObject.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestSchedulerWithSpringObjectJobFactoryAndJobSchedulingData()
        {
            DummyJob.param = 0;
            DummyJob.count = 0;

            SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
            factoryObject.JobFactory = new SpringObjectJobFactory();
            factoryObject.JobSchedulingDataLocation = "job-scheduling-data.xml";
            // TODO bean.ResourceLoader = (new FileSystemResourceLoader());
            factoryObject.AfterPropertiesSet();
            factoryObject.Start();

            Thread.Sleep(500);
            Assert.AreEqual(10, DummyJob.param);
            Assert.IsTrue(DummyJob.count > 0);

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
                Assert.AreNotSame(scheduler1, scheduler2);
                Assert.AreEqual("quartz1", scheduler1.SchedulerName);
                Assert.AreEqual("quartz2", scheduler2.SchedulerName);

                XmlApplicationContext ctx2 = new XmlApplicationContext("multipleSchedulers.xml");
                try
                {
                    IScheduler scheduler1a = (IScheduler) ctx2.GetObject("scheduler1");
                    IScheduler scheduler2a = (IScheduler) ctx2.GetObject("scheduler2");
                    Assert.AreNotSame(scheduler1a, scheduler2a);
                    Assert.AreNotSame(scheduler1a, scheduler1);
                    Assert.AreNotSame(scheduler2a, scheduler2);
                    Assert.AreEqual("quartz1", scheduler1a.SchedulerName);
                    Assert.AreEqual("quartz2", scheduler2a.SchedulerName);
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

                Assert.AreEqual(0, exportService.ImportCount, "doImport called exportService");
                Assert.AreEqual(2, exportService.ExportCount, "doExport not called on exportService");
                Assert.AreEqual(2, importService.ImportCount, "doImport not called on importService");
                Assert.AreEqual(0, importService.ExportCount, "doExport called on importService");
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

                Assert.AreEqual(0, exportService.ImportCount, "doImport called exportService");
                Assert.AreEqual(2, exportService.ExportCount, "doExport not called on exportService");
                Assert.AreEqual(2, importService.ImportCount, "doImport not called on importService");
                Assert.AreEqual(0, importService.ExportCount, "doExport called on importService");
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
            context.RegisterObjectDefinition("scheduler", new RootObjectDefinition(typeof (SchedulerFactoryObject)));
            IScheduler scheduler = (IScheduler) context.GetObject("scheduler", typeof (IScheduler));
            Assert.IsFalse(scheduler.IsStarted);
            context.Refresh();
            Assert.IsTrue(scheduler.IsStarted);
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
            
            Assert.IsFalse(scheduler.IsStarted);
            context.Refresh();
            Assert.IsFalse(scheduler.IsStarted);
        }

        /// <summary>
        /// Tests how scheduler is exposed to application context.
        /// </summary>
        [Test]
        public void TestSchedulerRepositoryExposure()
        {
            XmlApplicationContext ctx = new XmlApplicationContext("schedulerRepositoryExposure.xml");
            Assert.AreSame(SchedulerRepository.Instance.Lookup("myScheduler"), ctx.GetObject("scheduler"));
            ctx.Dispose();
        }

        private class TestSchedulerListener : ISchedulerListener
        {
            public void JobScheduled(ITrigger trigger)
            {
            }

            public void JobUnscheduled(TriggerKey triggerKey)
            {
            }

            public void TriggerFinalized(ITrigger trigger)
            {
            }

            public void TriggerPaused(TriggerKey triggerKey)
            {
            }

            public void TriggersPaused(string triggerGroup)
            {
            }

            public void TriggerResumed(TriggerKey triggerKey)
            {
            }

            public void TriggersResumed(string triggerGroup)
            {
            }

            public void JobAdded(IJobDetail jobDetail)
            {
            }

            public void JobDeleted(JobKey jobKey)
            {
            }

            public void JobPaused(JobKey jobKey)
            {
            }

            public void JobsPaused(string jobGroup)
            {
            }

            public void JobResumed(JobKey jobKey)
            {
            }

            public void JobsResumed(string jobGroup)
            {
            }

            public void SchedulerError(string msg, SchedulerException cause)
            {
            }

            public void SchedulerInStandbyMode()
            {
            }

            public void SchedulerStarted()
            {
            }

            public void SchedulerStarting()
            {
                
            }

            public void SchedulerShutdown()
            {
            }

            public void SchedulerShuttingdown()
            {
            }

            public void SchedulingDataCleared()
            {
            }
        }

        private class TestJobListener : IJobListener
        {
            public string Name
            {
                get { return null; }
            }

            public void JobToBeExecuted(IJobExecutionContext context)
            {
            }

            public void JobExecutionVetoed(IJobExecutionContext context)
            {
            }

            public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
            {
            }
        }


        private class TestTriggerListener : ITriggerListener
        {
            public string Name
            {
                get { return null; }
            }

            public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
            {
            }

            public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
            {
                return false;
            }

            public void TriggerMisfired(ITrigger trigger)
            {
            }

            public void TriggerComplete(ITrigger trigger, IJobExecutionContext context,
                                        SchedulerInstruction triggerInstructionCode)
            {
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
                count++;
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
            protected override void ExecuteInternal(IJobExecutionContext jobExecutionContext)
            {
                count++;
            }
        }

        /// <summary>
        /// Simple thread runnable.
        /// </summary>
        public class DummyRunnable : IThreadRunnable
        {
            internal static int param;
            internal static int count;
            internal static readonly ManualResetEvent runEvent = new ManualResetEvent(false);

            /// <summary>
            /// Runs thread runnable.
            /// </summary>
            public void Run()
            {
                count++;
                runEvent.Set();
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
        public void Execute(IJobExecutionContext jobExecutionContext)
        {
            count++;
        }
    }

    /// <summary>
    /// Subclass of SchedulerFactoryObject for testing purposes.
    /// </summary>
    public class TestSchedulerFactoryObject : SchedulerFactoryObject
    {
        private readonly IScheduler sched;

        /// <summary>
        /// Creates new instance of this class.
        /// </summary>
        /// <param name="sched"></param>
        public TestSchedulerFactoryObject(IScheduler sched)
        {
            this.sched = sched;
        }

        /// <summary>
        /// Creates a scheduler actually returning the scheduler this intance
        /// was instantiated with.
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="schedulerName"></param>
        /// <returns></returns>
        protected override IScheduler CreateScheduler(ISchedulerFactory schedulerFactory, String schedulerName)
        {
            return sched;
        }
    }
}