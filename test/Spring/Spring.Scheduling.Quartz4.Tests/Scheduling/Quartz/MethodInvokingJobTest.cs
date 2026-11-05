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
using Quartz;
using Spring.Objects.Support;

namespace Spring.Scheduling.Quartz;

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
        Assert.Throws<JobExecutionException>(() => methodInvokingJob.Execute(TestUtil.CreateMinimalJobExecutionContext()));
    }

    /// <summary>
    /// Test method invoke via execute.
    /// </summary>
    [Test]
    public async Task TestMethodInvoker_MethodSetCorrectly()
    {
        InvocationCountingJob job = new InvocationCountingJob();
        MethodInvoker mi = new MethodInvoker();
        mi.TargetObject = job;
        mi.TargetMethod = "Invoke";
        mi.Prepare();
        methodInvokingJob.MethodInvoker = mi;
        await methodInvokingJob.Execute(TestUtil.CreateMinimalJobExecutionContext());
        Assert.That(job.CounterValue, Is.EqualTo(1), "Job was not invoked once");
    }

    /// <summary>
    /// Test that invocation result is set to execution context (SPRNET-1340).
    /// </summary>
    [Test]
    public async Task TestMethodInvoker_ShouldSetResultToExecutionContext()
    {
        InvocationCountingJob job = new InvocationCountingJob();
        MethodInvoker mi = new MethodInvoker();
        mi.TargetObject = job;
        mi.TargetMethod = "InvokeWithReturnValue";
        mi.Prepare();
        methodInvokingJob.MethodInvoker = mi;
        IJobExecutionContext context = TestUtil.CreateMinimalJobExecutionContext();
        await methodInvokingJob.Execute(context);

        Assert.That(context.Result, Is.EqualTo(InvocationCountingJob.DefaultReturnValue), "result value was not set to context");
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

        Assert.Throws<JobMethodInvocationFailedException>(() => methodInvokingJob.Execute(TestUtil.CreateMinimalJobExecutionContext()));

        Assert.That(job.CounterValue, Is.EqualTo(1), "Job was not invoked once");
    }

    /// <summary>
    /// Test method invoke via execute.
    /// </summary>
    [Test]
    public async Task TestMethodInvoker_PrivateMethod()
    {
        InvocationCountingJob job = new InvocationCountingJob();
        MethodInvoker mi = new MethodInvoker();
        mi.TargetObject = job;
        mi.TargetMethod = "PrivateMethod";
        mi.Prepare();
        methodInvokingJob.MethodInvoker = mi;
        await methodInvokingJob.Execute(TestUtil.CreateMinimalJobExecutionContext());
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
