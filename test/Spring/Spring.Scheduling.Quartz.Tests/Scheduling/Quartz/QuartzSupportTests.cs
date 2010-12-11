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
        private MockRepository mockery;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            mockery = new MockRepository();
        }

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
            JobDetail jobDetail1 = (JobDetail) mijdfb.GetObject();

            SimpleTriggerObject trigger1 = new SimpleTriggerObject();
            trigger1.ObjectName = ("myTrigger1");
            trigger1.JobDetail = (jobDetail1);
            trigger1.StartDelay = TimeSpan.FromMilliseconds(0);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);
            trigger1.AfterPropertiesSet();

            IScheduler scheduler = (IScheduler) mockery.CreateMock(typeof (IScheduler));

            Expect.Call(scheduler.Context).Return(new SchedulerContext());
            Expect.Call(scheduler.GetJobDetail("myJob0", SchedulerConstants.DefaultGroup)).Return(null);
            Expect.Call(scheduler.GetJobDetail("myJob1", SchedulerConstants.DefaultGroup)).Return(null);
            Expect.Call(scheduler.GetTrigger("myTrigger0", SchedulerConstants.DefaultGroup)).Return(null);
            Expect.Call(scheduler.GetTrigger("myTrigger1", SchedulerConstants.DefaultGroup)).Return(null);
            
            scheduler.AddJob(jobDetail0, true);
            LastCall.IgnoreArguments();
            Expect.Call(scheduler.ScheduleJob(trigger0)).Return(DateTime.UtcNow);
            scheduler.AddJob(jobDetail1, true);
            LastCall.IgnoreArguments();
            Expect.Call(scheduler.ScheduleJob(trigger1)).Return(DateTime.UtcNow);
            scheduler.Start();
            LastCall.IgnoreArguments();
            scheduler.Shutdown(false);
            LastCall.IgnoreArguments();


            mockery.ReplayAll();

            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
            schedulerFactoryObject.JobFactory = (null);
            IDictionary schedulerContext = new Hashtable();
            schedulerContext.Add("otherTestObject", tb);
            schedulerFactoryObject.SchedulerContextAsMap = (schedulerContext);
            if (explicitJobDetail)
            {
                schedulerFactoryObject.JobDetails = (new JobDetail[] {jobDetail0});
            }
            schedulerFactoryObject.Triggers = (new Trigger[] {trigger0, trigger1});
            try
            {
                schedulerFactoryObject.AfterPropertiesSet();
                schedulerFactoryObject.Start();
            }
            finally
            {
                schedulerFactoryObject.Dispose();
            }

            mockery.VerifyAll();
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
            JobDetail jobDetail1 = (JobDetail) mijdfb.GetObject();

            SimpleTriggerObject trigger1 = new SimpleTriggerObject();
            trigger1.ObjectName = ("myTrigger1");
            trigger1.JobDetail = (jobDetail1);
            trigger1.StartDelay = TimeSpan.FromMilliseconds(0);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);
            trigger1.AfterPropertiesSet();

            IScheduler scheduler = (IScheduler) mockery.CreateMock(typeof (IScheduler));
            Expect.Call(scheduler.Context).Return(new SchedulerContext());
            Expect.Call(scheduler.GetTrigger("myTrigger0", SchedulerConstants.DefaultGroup)).Return(null);
            Expect.Call(scheduler.GetTrigger("myTrigger1", SchedulerConstants.DefaultGroup)).Return(new SimpleTrigger());
            if (overwrite)
            {
                scheduler.AddJob(jobDetail1, true);
                LastCall.IgnoreArguments();
                Expect.Call(scheduler.RescheduleJob("myTrigger1", SchedulerConstants.DefaultGroup, trigger1)).Return(
                    DateTime.UtcNow);
            }
            else
            {
                Expect.Call(scheduler.GetJobDetail("myJob0", SchedulerConstants.DefaultGroup)).Return(null);
            }
            scheduler.AddJob(jobDetail0, true);
            LastCall.IgnoreArguments();
            Expect.Call(scheduler.ScheduleJob(trigger0)).Return(DateTime.UtcNow);
            scheduler.Start();
            scheduler.Shutdown(false);

            mockery.ReplayAll();

            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);
            schedulerFactoryObject.JobFactory = (null);
            IDictionary schedulerContext = new Hashtable();
            schedulerContext.Add("otherTestObject", tb);
            schedulerFactoryObject.SchedulerContextAsMap = (schedulerContext);
            schedulerFactoryObject.Triggers = (new Trigger[] {trigger0, trigger1});
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

            mockery.VerifyAll();
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
            JobDetail jobDetail1 = (JobDetail) mijdfb.GetObject();

            SimpleTriggerObject trigger1 = new SimpleTriggerObject();
            trigger1.ObjectName = ("myTrigger1");
            trigger1.JobDetail = (jobDetail1);
            trigger1.StartDelay = TimeSpan.FromMilliseconds(0);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);
            trigger1.AfterPropertiesSet();

            IScheduler scheduler = (IScheduler) mockery.CreateMock(typeof (IScheduler));
            Expect.Call(scheduler.Context).Return(new SchedulerContext());
            Expect.Call(scheduler.GetTrigger("myTrigger0", SchedulerConstants.DefaultGroup)).Return(null);
            Expect.Call(scheduler.GetTrigger("myTrigger1", SchedulerConstants.DefaultGroup)).Return(new SimpleTrigger());
            if (overwrite)
            {
                scheduler.AddJob(jobDetail1, true);
                LastCall.IgnoreArguments();
                Expect.Call(scheduler.RescheduleJob("myTrigger1", SchedulerConstants.DefaultGroup, trigger1)).Return(
                    DateTime.UtcNow);
            }
            else
            {
                Expect.Call(scheduler.GetJobDetail("myJob0", SchedulerConstants.DefaultGroup)).Return(null);
            }
            
            scheduler.AddJob(jobDetail0, true);
            LastCall.IgnoreArguments();

            Expect.Call(scheduler.ScheduleJob(trigger0)).Throw(new ObjectAlreadyExistsException(""));
            if (overwrite)
            {
                Expect.Call(scheduler.RescheduleJob("myTrigger0", SchedulerConstants.DefaultGroup, trigger0)).Return(
                    DateTime.UtcNow);
            }

            scheduler.Start();
            scheduler.Shutdown(false);

            mockery.ReplayAll();

            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);

            schedulerFactoryObject.JobFactory = (null);
            IDictionary schedulerContext = new Hashtable();
            schedulerContext.Add("otherTestObject", tb);
            schedulerFactoryObject.SchedulerContextAsMap = (schedulerContext);
            schedulerFactoryObject.Triggers = (new Trigger[] {trigger0, trigger1});
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

            mockery.VerifyAll();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerFactoryObjectWithListeners()
        {
            IJobFactory jobFactory = new AdaptableJobFactory();

            IScheduler scheduler = (IScheduler) mockery.CreateMock(typeof (IScheduler));

            ISchedulerListener schedulerListener = new TestSchedulerListener();
            IJobListener globalJobListener = new TestJobListener();
            IJobListener jobListener = new TestJobListener();
            ITriggerListener globalTriggerListener = new TestTriggerListener();
            ITriggerListener triggerListener = new TestTriggerListener();

            Expect.Call(scheduler.JobFactory = (jobFactory));
            scheduler.AddSchedulerListener(schedulerListener); 
            scheduler.AddGlobalJobListener(globalJobListener);
            scheduler.AddJobListener(jobListener); 
            scheduler.AddGlobalTriggerListener(globalTriggerListener);
            scheduler.AddTriggerListener(triggerListener); 
            scheduler.Start();
            scheduler.Shutdown(false);

            mockery.ReplayAll();

            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);

            schedulerFactoryObject.JobFactory = (jobFactory);
            schedulerFactoryObject.SchedulerListeners = (new ISchedulerListener[] {schedulerListener});
            schedulerFactoryObject.GlobalJobListeners = (new IJobListener[] {globalJobListener});
            schedulerFactoryObject.JobListeners = (new IJobListener[] {jobListener});
            schedulerFactoryObject.GlobalTriggerListeners = (new ITriggerListener[] {globalTriggerListener});
            schedulerFactoryObject.TriggerListeners = (new ITriggerListener[] {triggerListener});
            try
            {
                schedulerFactoryObject.AfterPropertiesSet();
                schedulerFactoryObject.Start();
            }
            finally
            {
                schedulerFactoryObject.Dispose();
            }

            mockery.VerifyAll();
        }

        /*public void TestMethodInvocationWithConcurrency()  {
		methodInvokingConcurrency(true);
	}*/

        // We can't test both since Quartz somehow seems to keep things in memory
        // enable both and one of them will fail (order doesn't matter).
        /*public void TestMethodInvocationWithoutConcurrency()  {
		methodInvokingConcurrency(false);
	}*/

        private void methodInvokingConcurrency(bool concurrent)
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
            JobDetail jobDetail1 = (JobDetail) mijdfb.GetObject();

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
            schedulerFactoryObject.JobDetails = (new JobDetail[] {jobDetail1});
            schedulerFactoryObject.Triggers = (new Trigger[] {trigger1, trigger0});
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
            JobDetail jobDetail0 = new JobDetail();
            jobDetail0.JobType = typeof (IJob);
            jobDetail0.Name = ("myJob0");
            jobDetail0.Group = (SchedulerConstants.DefaultGroup);
            jobDetail0.JobDataMap.Add("testObject", tb);
            Assert.AreEqual(tb, jobDetail0.JobDataMap.Get("testObject"));

            CronTrigger trigger0 = new CronTrigger();
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
            JobDetail jobDetail1 = (JobDetail) mijdfb.GetObject();

            SimpleTrigger trigger1 = new SimpleTrigger();
            trigger1.Name = "myTrigger1";
            trigger1.Group = SchedulerConstants.DefaultGroup;
            trigger1.JobName = "myJob1";
            trigger1.JobGroup = SchedulerConstants.DefaultGroup;
            trigger1.StartTimeUtc = (DateTime.UtcNow);
            trigger1.RepeatCount = (SimpleTrigger.RepeatIndefinitely);
            trigger1.RepeatInterval = TimeSpan.FromMilliseconds(20);

            IScheduler scheduler = (IScheduler) mockery.CreateMock(typeof (IScheduler));
            Expect.Call(scheduler.JobFactory = (jobFactory));
            Expect.Call(scheduler.GetJobDetail("myJob0", SchedulerConstants.DefaultGroup)).Return(null);
            Expect.Call(scheduler.GetJobDetail("myJob1", SchedulerConstants.DefaultGroup)).Return(null);
            Expect.Call(scheduler.GetTrigger("myTrigger0", SchedulerConstants.DefaultGroup)).Return(null);
            Expect.Call(scheduler.GetTrigger("myTrigger1", SchedulerConstants.DefaultGroup)).Return(null);
            scheduler.AddJob(jobDetail0, true);
            LastCall.IgnoreArguments();
            scheduler.AddJob(jobDetail1, true);
            LastCall.IgnoreArguments();
            Expect.Call(scheduler.ScheduleJob(trigger0)).Return(DateTime.UtcNow);
            Expect.Call(scheduler.ScheduleJob(trigger1)).Return(DateTime.UtcNow);
            scheduler.Start();
            scheduler.Shutdown(false); 

            mockery.ReplayAll();

            SchedulerFactoryObject schedulerFactoryObject = new TestSchedulerFactoryObject(scheduler);

            schedulerFactoryObject.JobFactory = (jobFactory);
            schedulerFactoryObject.JobDetails = (new JobDetail[] {jobDetail0, jobDetail1});
            schedulerFactoryObject.Triggers = (new Trigger[] {trigger0, trigger1});
            try
            {
                schedulerFactoryObject.AfterPropertiesSet();
                schedulerFactoryObject.Start();
            }
            finally
            {
                schedulerFactoryObject.Dispose();
            }

            mockery.VerifyAll();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerFactoryObjectWithApplicationContext()
        {
            TestObject tb = new TestObject("tb", 99);
            StaticApplicationContext ac = new StaticApplicationContext();

            IScheduler scheduler = (IScheduler) mockery.CreateMock(typeof (IScheduler));
            SchedulerContext schedulerContext = new SchedulerContext();
            Expect.Call(scheduler.Context).Return(schedulerContext).Repeat.Times(4);
            scheduler.Start();
            scheduler.Shutdown(false);

            mockery.ReplayAll();

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

            mockery.VerifyAll();
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
        public void TestMethodInvokingJobDetailFactoryObjectWithListenerNames()
        {
            TestMethodInvokingTask task = new TestMethodInvokingTask();
            MethodInvokingJobDetailFactoryObject mijdfb = new MethodInvokingJobDetailFactoryObject();
            String[] names = new String[] {"test1", "test2"};
            mijdfb.Name = ("myJob1");
            mijdfb.Group = (SchedulerConstants.DefaultGroup);
            mijdfb.TargetObject = (task);
            mijdfb.TargetMethod = ("doSomething");
            mijdfb.JobListenerNames = (names);
            mijdfb.AfterPropertiesSet();
            JobDetail jobDetail = (JobDetail) mijdfb.GetObject();
            ArrayList result = new ArrayList(jobDetail.JobListenerNames);
            Assert.AreEqual(names, result);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestJobDetailObjectWithListenerNames()
        {
            JobDetailObject jobDetail = new JobDetailObject();
            String[] names = new String[] {"test1", "test2"};
            jobDetail.JobListenerNames = (names);
            ArrayList result = new ArrayList(jobDetail.JobListenerNames);
            Assert.AreEqual(names, result);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestCronTriggerObjectWithListenerNames()
        {
            CronTriggerObject trigger = new CronTriggerObject();
            String[] names = new String[] {"test1", "test2"};
            trigger.TriggerListenerNames = (names);
            ArrayList result = new ArrayList(trigger.TriggerListenerNames);
            Assert.AreEqual(names, result);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSimpleTriggerObjectWithListenerNames()
        {
            SimpleTriggerObject trigger = new SimpleTriggerObject();
            String[] names = new String[] {"test1", "test2"};
            trigger.TriggerListenerNames = (names);
            ArrayList result = new ArrayList(trigger.TriggerListenerNames);
            Assert.AreEqual(names, result);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void TestSchedulerWithTaskExecutor()
        {
            CountingTaskExecutor taskExecutor = new CountingTaskExecutor();
            DummyJob.count = 0;

            JobDetail jobDetail = new JobDetail();
            jobDetail.JobType = typeof (DummyJob);
            ;
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
            factoryObject.Triggers = (new Trigger[] {trigger});
            factoryObject.JobDetails = (new JobDetail[] {jobDetail});
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

            JobDetail jobDetail = new JobDetailObject();
            jobDetail.JobType = typeof (DummyRunnable);
            jobDetail.Name = ("myJob");

            SimpleTriggerObject trigger = new SimpleTriggerObject();
            trigger.Name = ("myTrigger");
            trigger.JobDetail = (jobDetail);
            trigger.StartDelay = TimeSpan.FromMilliseconds(1);
            trigger.RepeatInterval = TimeSpan.FromMilliseconds(500);
            trigger.RepeatCount = (1);
            trigger.AfterPropertiesSet();

            SchedulerFactoryObject factoryObject = new SchedulerFactoryObject();
            factoryObject.Triggers = (new Trigger[] {trigger});
            factoryObject.JobDetails = (new JobDetail[] {jobDetail});
            factoryObject.AfterPropertiesSet();
            factoryObject.Start();

            Thread.Sleep(500);
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

            JobDetail jobDetail = new JobDetail();
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
            factoryObject.Triggers = (new Trigger[] {trigger});
            factoryObject.JobDetails = (new JobDetail[] {jobDetail});
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

            JobDetail jobDetail = new JobDetail();
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
            factoryObject.Triggers = (new Trigger[] {trigger});
            factoryObject.JobDetails = (new JobDetail[] {jobDetail});
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

            JobDetail jobDetail = new JobDetail();
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
            bean.Triggers = (new Trigger[] {trigger});
            bean.JobDetails = (new JobDetail[] {jobDetail});
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

            JobDetail jobDetail = new JobDetailObject();
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
            factoryObject.Triggers = (new Trigger[] {trigger});
            factoryObject.JobDetails = (new JobDetail[] {jobDetail});
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

            JobDetail jobDetail = new JobDetail();
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
            factoryObject.Triggers = (new Trigger[] {trigger});
            factoryObject.JobDetails = (new JobDetail[] {jobDetail});
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
            factoryObject.JobFactory = (new SpringObjectJobFactory());
            factoryObject.JobSchedulingDataLocation = ("job-scheduling-data.xml");
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
            public void JobScheduled(Trigger trigger)
            {
            }

            public void JobUnscheduled(String triggerName, String triggerGroup)
            {
            }

            public void TriggerFinalized(Trigger trigger)
            {
            }

            public void TriggersPaused(String triggerName, String triggerGroup)
            {
            }

            public void TriggersResumed(String triggerName, String triggerGroup)
            {
            }

            public void JobsPaused(String jobName, String jobGroup)
            {
            }

            public void JobsResumed(String jobName, String jobGroup)
            {
            }

            public void SchedulerError(String msg, SchedulerException cause)
            {
            }

            public void SchedulerShutdown()
            {
            }
        }


        private class TestJobListener : IJobListener
        {
            public string Name
            {
                get { return null; }
            }

            public void JobToBeExecuted(JobExecutionContext context)
            {
            }

            public void JobExecutionVetoed(JobExecutionContext context)
            {
            }

            public void JobWasExecuted(JobExecutionContext context, JobExecutionException jobException)
            {
            }
        }


        private class TestTriggerListener : ITriggerListener
        {
            public string Name
            {
                get { return null; }
            }

            public void TriggerFired(Trigger trigger, JobExecutionContext context)
            {
            }

            public bool VetoJobExecution(Trigger trigger, JobExecutionContext context)
            {
                return false;
            }

            public void TriggerMisfired(Trigger trigger)
            {
            }

            public void TriggerComplete(Trigger trigger, JobExecutionContext context,
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
            protected override void ExecuteInternal(JobExecutionContext jobExecutionContext)
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

            /// <summary>
            /// Runs thread runnable.
            /// </summary>
            public void Run()
            {
                count++;
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
        public void Execute(JobExecutionContext jobExecutionContext)
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