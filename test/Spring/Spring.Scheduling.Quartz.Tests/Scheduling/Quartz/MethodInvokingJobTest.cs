/*
* Copyright 2002-2005 the original author or authors.
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
using System.Threading;

using NUnit.Framework;

using Quartz;
using Quartz.Job;
using Quartz.Spi;

using Rhino.Mocks;

using Spring.Objects.Support;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class MethodInvokingJobTest
    {
        private MethodInvokingJob methodInvokingJob;

        [SetUp]
        public void SetUp()
        {
            methodInvokingJob = new MethodInvokingJob();
        }

        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestMethodInvoker_SetWithNull()
        {
            methodInvokingJob.MethodInvoker = null;
        }

        [Test]
        [ExpectedException(ExceptionType = typeof(JobExecutionException))]
        public void TestMethodInvocation_NullMethodInvokder()
        {
            methodInvokingJob.Execute(CreateMinimalJobExecutionContext());
        }

        [Test]
        public void TestMethodInvoker_MethodSetCorrectly()
        {
            InvocationCountingJob job = new InvocationCountingJob();
            MethodInvoker mi = new MethodInvoker();
            mi.TargetObject = job;
            mi.TargetMethod = "Invoke";
            mi.Prepare();
            methodInvokingJob.MethodInvoker = mi;
            methodInvokingJob.Execute(CreateMinimalJobExecutionContext());
            Assert.AreEqual(1, job.CounterValue, "Job was not invoked once");
        }

        [Test]
        public void TestMethodInvoker_MethodSetCorrectlyThrowsException()
        {
            InvocationCountingJob job = new InvocationCountingJob();
            MethodInvoker mi = new MethodInvoker();
            mi.TargetObject = job;
            mi.TargetMethod = "InvokeAndThrowException";
            mi.Prepare();
            methodInvokingJob.MethodInvoker = mi;
            try
            {
                methodInvokingJob.Execute(CreateMinimalJobExecutionContext());
                Assert.Fail("Successful invoke when method threw exception");
            }
            catch (JobMethodInvocationFailedException)
            {
                // ok
            }
            Assert.AreEqual(1, job.CounterValue, "Job was not invoked once");
        }

        [Test]
        public void TestMethodInvoker_PrivateMethod()
        {
            InvocationCountingJob job = new InvocationCountingJob();
            MethodInvoker mi = new MethodInvoker();
            mi.TargetObject = job;
            mi.TargetMethod = "PrivateMethod";
            mi.Prepare();
            methodInvokingJob.MethodInvoker = mi;
            methodInvokingJob.Execute(CreateMinimalJobExecutionContext());
        }
        
        private static JobExecutionContext CreateMinimalJobExecutionContext()
        {
            MockRepository repo = new MockRepository();
            IScheduler sched = (IScheduler) repo.DynamicMock(typeof (IScheduler));
            JobExecutionContext ctx = new JobExecutionContext(sched, ConstructMinimalTriggerFiredBundle(), null);
            return ctx;
        }

        private static TriggerFiredBundle ConstructMinimalTriggerFiredBundle()
        {
            JobDetail jd = new JobDetail("jobName", "jobGroup", typeof(NoOpJob));
            SimpleTrigger trigger = new SimpleTrigger("triggerName", "triggerGroup");
            TriggerFiredBundle retValue = new TriggerFiredBundle(jd, trigger, null, false, null, null, null, null);

            return retValue;
        }

    }

    /// <summary>
    /// Test class for method invoker.
    /// </summary>
    public class InvocationCountingJob
    {
        private int counter;

        public void Invoke()
        {
            Interlocked.Increment(ref counter);    
        }

        public void InvokeAndThrowException()
        {
            Interlocked.Increment(ref counter);
            throw new Exception();
        }

        private void PrivateMethod()
        {
        }

        public int CounterValue
        {
            get { return counter; }
        }
    }
}
