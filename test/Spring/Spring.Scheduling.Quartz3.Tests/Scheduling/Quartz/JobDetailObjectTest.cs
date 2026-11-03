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

using System.Collections;
using NUnit.Framework;
using Quartz;
using Spring.Context.Support;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Tests for <see cref="JobDetailObject" />.
/// </summary>
/// <author>Marko Lahma (.NET)</author>
[TestFixture]
public class JobDetailObjectTest
{
    private JobDetailObject jobDetail;

    /// <summary>
    /// Test setup.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        jobDetail = new JobDetailObject();
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobType_Null()
    {
        Assert.Throws<ArgumentException>(() => jobDetail.JobType = null);
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobType_NonIJob()
    {
        jobDetail.JobType = typeof(object);
        Assert.That(jobDetail.JobType, Is.EqualTo(typeof(object)), "JobDetail did not create same type as expected");
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobType_IJob()
    {
        Type CORRECT_IJOB = typeof(NoOpJob);
        jobDetail.JobType = CORRECT_IJOB;
        Assert.That(CORRECT_IJOB, Is.EqualTo(jobDetail.JobType), "JobDetail did not register correct job type");
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobDataAsMap_Null()
    {
        Assert.Throws<ArgumentException>(() => jobDetail.JobDataAsMap = null);
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestJobDataAsMap_ProperValues()
    {
        IDictionary values = new Hashtable();
        values["baz"] = "foo";
        values["foo"] = 123;
        values["bar"] = null;
        jobDetail.JobDataAsMap = values;
        Assert.That(jobDetail.JobDataMap.Count, Is.EqualTo(values.Count), "Data of inequal size");
        Assert.That(jobDetail.JobDataMap.Keys, Is.EquivalentTo(values.Keys), "JobDataMap values not equal");
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_Defaults()
    {
        const string objectName = "springJobDetailObject";
        jobDetail.ObjectName = objectName;
        jobDetail.Group = null;
        jobDetail.AfterPropertiesSet();
        Assert.That(jobDetail.Group, Is.EqualTo(SchedulerConstants.DefaultGroup), "Groups differ");
        Assert.That(jobDetail.Name, Is.EqualTo(objectName), "Names differ");
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_CustomNameAndGroup()
    {
        const string objectName = "springJobDetailObject";
        const string jobDetailName = "jobDetailName";
        const string jobDetailGroup = "jobDetailGroup";
        jobDetail.ObjectName = objectName;
        jobDetail.Name = jobDetailName;
        jobDetail.Group = jobDetailGroup;
        jobDetail.AfterPropertiesSet();
        Assert.That(jobDetail.Group, Is.EqualTo(jobDetailGroup), "Groups differ");
        Assert.That(jobDetail.Name, Is.EqualTo(jobDetailName), "Names differ");
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_ApplicationContextJobDataKeySetWithApplicationContext()
    {
        const string objectName = "springJobDetailObject";
        jobDetail.ObjectName = objectName;
        StaticApplicationContext ctx = new StaticApplicationContext();
        jobDetail.ApplicationContext = ctx;
        string key = "applicationContextJobDataKey";
        jobDetail.ApplicationContextJobDataKey = key;
        jobDetail.AfterPropertiesSet();
        Assert.That(jobDetail.ApplicationContext, Is.SameAs(ctx), "ApplicationContext was not set correctly");
        Assert.That(jobDetail.JobDataMap[key], Is.SameAs(ctx), "ApplicationContext was not set to job data map");
    }

    /// <summary>
    /// Tests job detail's property behavior.
    /// </summary>
    [Test]
    public void TestAfterPropertiesSet_ApplicationContextJobDataKeySetWithoutApplicationContext()
    {
        const string objectName = "springJobDetailObject";
        jobDetail.ObjectName = objectName;
        jobDetail.ApplicationContextJobDataKey = "applicationContextJobDataKey";
        Assert.Throws<ArgumentException>(() => jobDetail.AfterPropertiesSet());
    }
}
