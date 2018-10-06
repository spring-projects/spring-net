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
using System.Threading;

using FakeItEasy;

using NUnit.Framework;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;

using Spring.Objects.Support;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Tests for MethodInvokingJob.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class MethodInvokingJobTest
    {
        private MethodInvokingJob methodInvokingJob;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            methodInvokingJob = new MethodInvokingJob();
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
        [Test]
        public void TestMethodInvoker_SetWithNull()
        {
            Assert.Throws<ArgumentException>(() => methodInvokingJob.MethodInvoker = null);
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
        [Test]
        public void TestMethodInvocation_NullMethodInvokder()
        {
            Assert.Throws<JobExecutionException>(() => methodInvokingJob.Execute(CreateMinimalJobExecutionContext()));
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
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

        /// <summary>
        /// Test that invocation result is set to execution context (SPRNET-1340).
        /// </summary>
        [Test]
        public void TestMethodInvoker_ShouldSetResultToExecutionContext()
        {
            InvocationCountingJob job = new InvocationCountingJob();
            MethodInvoker mi = new MethodInvoker();
            mi.TargetObject = job;
            mi.TargetMethod = "InvokeWithReturnValue";
            mi.Prepare();
            methodInvokingJob.MethodInvoker = mi;
            IJobExecutionContext context = CreateMinimalJobExecutionContext();
            methodInvokingJob.Execute(context);

            Assert.AreEqual(InvocationCountingJob.DefaultReturnValue, context.Result, "result value was not set to context");
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
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

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
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
        
        private static IJobExecutionContext CreateMinimalJobExecutionContext()
        {
            IScheduler sched = A.Fake<IScheduler>();
            IJobExecutionContext ctx = new JobExecutionContextImpl(sched, ConstructMinimalTriggerFiredBundle(), null);
            return ctx;
        }

        private static TriggerFiredBundle ConstructMinimalTriggerFiredBundle()
        {
            IJobDetail jd = new JobDetailImpl("jobName", "jobGroup", typeof(NoOpJob));
            IOperableTrigger trigger = new SimpleTriggerImpl("triggerName", "triggerGroup");
            TriggerFiredBundle retValue = new TriggerFiredBundle(
                jd, 
                trigger, 
                null, 
                false, 
                DateTimeOffset.UtcNow, 
                null, 
                null, 
                null);

            return retValue;
        }

    }

    /// <summary>
    /// Test class for method invoker.
    /// </summary>
    public class InvocationCountingJob
    {
        private int counter;
        internal const string DefaultReturnValue = "return value";

        /// <summary>
        /// Increments method invoke counter.
        /// </summary>
        public void Invoke()
        {
            Interlocked.Increment(ref counter);    
        }

        /// <summary>
        /// Throws exception after incrementing counter.
        /// </summary>
        public void InvokeAndThrowException()
        {
            Interlocked.Increment(ref counter);
            throw new Exception();
        }

        private void PrivateMethod()
        {
        }

        /// <summary>
        /// Returns <see cref="DefaultReturnValue" /> as return value.
        /// </summary>
        public string InvokeWithReturnValue()
        {
            return DefaultReturnValue;
        }

        /// <summary>
        /// Invocation count.
        /// </summary>
        public int CounterValue
        {
            get { return counter; }
        }
    }
}
