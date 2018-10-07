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

using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

using Quartz;
using Quartz.Impl.Triggers;
using Quartz.Spi;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Unit tests for SpringObjectJobFactory.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class SpringObjectJobFactoryTest
    {
        private SpringObjectJobFactory factory;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            factory = new SpringObjectJobFactory();
        }

        /// <summary>
        /// Tests job instane creation.
        /// </summary>
        [Test]
        public void TestCreateJobInstance_SimpleDefaults()
        {
            IOperableTrigger trigger = new SimpleTriggerImpl();
            TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof (NoOpJob), trigger);

            IJob job = factory.NewJob(bundle, null);
            Assert.IsNotNull(job, "Created job was null");
        }

        /// <summary>
        /// Tests job instane creation.
        /// </summary>
        [Test]
        public void TestCreateJobInstance_SchedulerContextGiven()
        {
            IOperableTrigger trigger = new SimpleTriggerImpl();
            TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(InjectableJob), trigger);

            IDictionary<string, object> items = new Dictionary<string, object>();
            items["foo"] = "bar";
            items["number"] = 123;
            factory.SchedulerContext = new SchedulerContext(items);
            InjectableJob job = (InjectableJob)factory.NewJob(bundle, null);

            Assert.IsNotNull(job, "Created job was null");
            Assert.AreEqual("bar", job.Foo, "string injection failed");
            Assert.AreEqual(123, job.Number, "integer injection failed");
        }

        /// <summary>
        /// Tests job instane creation.
        /// </summary>
        [Test]
        public void TestCreateJobInstance_IgnoredProperties()
        {
            factory.IgnoredUnknownProperties = new string[] {"foo", "baz"};
            IOperableTrigger trigger = new SimpleTriggerImpl();
            trigger.JobDataMap["foo"] = "should not be injected";
            trigger.JobDataMap["number"] = 123;
            TriggerFiredBundle bundle = TestUtil.CreateMinimalFiredBundleWithTypedJobDetail(typeof(InjectableJob), trigger);

            InjectableJob job = (InjectableJob)factory.NewJob(bundle, null);
            Assert.IsNotNull(job, "Created job was null");
            Assert.AreEqual(123, job.Number, "integer injection failed");
            Assert.IsNull(job.Foo, "foo was injected when it was not supposed to    ");
        }

    }
    
    public class NoOpJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Test job object that has injectable properties
    /// </summary>
    public class InjectableJob : NoOpJob
    {
        /// <summary>
        /// Simple int property.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Simple string property.
        /// </summary>
        public string Foo { get; set; }
    }
}
