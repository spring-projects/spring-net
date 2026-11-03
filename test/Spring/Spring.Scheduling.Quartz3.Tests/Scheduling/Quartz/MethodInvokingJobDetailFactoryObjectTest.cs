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

using NUnit.Framework;
using Quartz;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Unit tests for MethodInvokingJobDetailFactoryObject.
/// </summary>
/// <author>Marko Lahma (.NET)</author>
[TestFixture]
public class MethodInvokingJobDetailFactoryObjectTest
{
    private const string FACTORY_NAME = "springObjectFactory";
    private MethodInvokingJobDetailFactoryObject factory;

    /// <summary>
    /// Setup for the test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        factory = new MethodInvokingJobDetailFactoryObject();
        factory.ObjectName = FACTORY_NAME;
        factory.TargetMethod = "Invoke";
        factory.TargetObject = new InvocationCountingJob();
    }

    /// <summary>
    /// Tests JobDetail retrieval and it's set properties.
    /// </summary>
    [Test]
    public void TestGetObject_MinimalDefaults()
    {
        factory.AfterPropertiesSet();
        IJobDetail jd = (IJobDetail) factory.GetObject();
        Assert.That(jd, Is.Not.Null, "job detail was null");
        Assert.That(jd.Key.Name, Is.EqualTo(FACTORY_NAME), "job name did not default to factory name");
        Assert.That(jd.JobType, Is.EqualTo(typeof(MethodInvokingJob)), "factory did not create method invoking job");
        Assert.That(jd.Durable, Is.True, "job was not durable");
    }

    /// <summary>
    /// Tests JobDetail retrieval and it's set properties.
    /// </summary>
    [Test]
    public void TestGetObject_ConcurrentJob()
    {
        factory.Concurrent = false;
        factory.AfterPropertiesSet();
        IJobDetail jd = (IJobDetail) factory.GetObject();
        Assert.That(jd, Is.Not.Null, "job detail was null");
        Assert.That(jd.JobType, Is.EqualTo(typeof(StatefulMethodInvokingJob)), "factory did not create stateful method invoking job");
    }
}
