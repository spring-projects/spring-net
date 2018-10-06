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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;

using NUnit.Framework;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;

using Spring.Core.IO;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Tests for SchedulerFactoryObject.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class SchedulerFactoryObjectTest
    {
        private static readonly MethodInfo m_InitSchedulerFactory = typeof(SchedulerFactoryObject).GetMethod("InitSchedulerFactory",
                                                        BindingFlags.Instance | BindingFlags.NonPublic);
        private SchedulerFactoryObject factory;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            factory = new SchedulerFactoryObject();

            TestSchedulerFactory.Initialize();
            A.CallTo(() => TestSchedulerFactory.MockScheduler.SchedulerName).Returns("scheduler");
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_Defaults()
        {
            factory.AfterPropertiesSet();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_NullJobFactory()
        {
            factory.JobFactory = null;
            factory.AfterPropertiesSet();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_NoAutoStartup()
        {
            factory.SchedulerFactoryType = typeof(TestSchedulerFactory);
            factory.AutoStartup = false;
            factory.AfterPropertiesSet();

            A.CallTo(() => TestSchedulerFactory.MockScheduler.Start(A<CancellationToken>._)).MustNotHaveHappened();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_Calendars()
        {
            InitForAfterPropertiesSetTest();

            const string calendarName = "calendar";
            ICalendar cal = A.Fake<ICalendar>();
            Hashtable calTable = new Hashtable();
            calTable[calendarName] = cal;
            factory.Calendars = calTable;

            factory.AfterPropertiesSet();

            A.CallTo(() => TestSchedulerFactory.MockScheduler.AddCalendar(calendarName, cal, true, true, A<CancellationToken>._)).MustHaveHappened();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_Trigger_TriggerExists()
        {
            InitForAfterPropertiesSetTest();

            const string TRIGGER_NAME = "trigName";
            const string TRIGGER_GROUP = "trigGroup";
            SimpleTriggerImpl trigger = new SimpleTriggerImpl(TRIGGER_NAME, TRIGGER_GROUP);
            factory.Triggers = new ITrigger[] { trigger };

            A.CallTo(() => TestSchedulerFactory.MockScheduler.GetTrigger(new TriggerKey(TRIGGER_NAME, TRIGGER_GROUP), A<CancellationToken>._)).Returns(trigger);

            factory.AfterPropertiesSet();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_Trigger_TriggerDoesntExist()
        {
            InitForAfterPropertiesSetTest();
            A.CallTo(() => TestSchedulerFactory.MockScheduler.GetTrigger(A<TriggerKey>._, A<CancellationToken>._)).Returns(Task.FromResult<ITrigger>(null));

            const string TRIGGER_NAME = "trigName";
            const string TRIGGER_GROUP = "trigGroup";
            SimpleTriggerImpl trigger = new SimpleTriggerImpl(TRIGGER_NAME, TRIGGER_GROUP);
            factory.Triggers = new ITrigger[] { trigger };

            factory.AfterPropertiesSet();

            A.CallTo(() => TestSchedulerFactory.MockScheduler.ScheduleJob(trigger, A<CancellationToken>._)).MustHaveHappened();
        }


        private void InitForAfterPropertiesSetTest()
        {
            factory.AutoStartup = false;
            // set expectations
            factory.SchedulerFactoryType = typeof(TestSchedulerFactory);
            TestSchedulerFactory.MockScheduler.JobFactory = null;
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public async Task TestStart()
        {
            factory.SchedulerFactoryType = typeof(TestSchedulerFactory);
            factory.AutoStartup = false;
            factory.AfterPropertiesSet();
            await factory.Start();

            A.CallTo(TestSchedulerFactory.MockScheduler)
                .Where(x => x.Method.Name.Equals("set_JobFactory"))
                .WhenArgumentsMatch(x => x.Get<IJobFactory>(0) != null)
                .MustHaveHappened();

            A.CallTo(() => TestSchedulerFactory.MockScheduler.Start(A<CancellationToken>._)).MustHaveHappened();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public async Task TestStop()
        {
            factory.SchedulerFactoryType = typeof(TestSchedulerFactory);
            factory.AutoStartup = false;
            factory.AfterPropertiesSet();
            await factory.Stop();

            A.CallTo(TestSchedulerFactory.MockScheduler)
                .Where(x => x.Method.Name.Equals("set_JobFactory"))
                .WhenArgumentsMatch(x => x.Get<IJobFactory>(0) != null)
                .MustHaveHappened();

            A.CallTo(() => TestSchedulerFactory.MockScheduler.Standby(A<CancellationToken>._)).MustHaveHappened();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestGetObject()
        {
            factory.AfterPropertiesSet();
            IScheduler sched = (IScheduler)factory.GetObject();
            Assert.IsNotNull(sched, "scheduler was null");
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestSchedulerFactoryType_InvalidType()
        {
            Assert.Throws<ArgumentException>(() => factory.SchedulerFactoryType = typeof(SchedulerFactoryObjectTest));
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestSchedulerFactoryType_ValidType()
        {
            factory.SchedulerFactoryType = typeof(StdSchedulerFactory);
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestInitSchedulerFactory_MinimalDefaults()
        {
            factory.SchedulerName = "testFactoryObject";
            StdSchedulerFactory factoryToPass = new StdSchedulerFactory();
            m_InitSchedulerFactory.Invoke(factory, new object[] { factoryToPass });
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestInitSchedulerFactory_ConfigLocationReadingShouldPreserverExtraEqualsMarksAndTrimKeysAndValues()
        {
            const string ConnectionStringValue = "Server=(local);Database=quartz;Trusted_Connection=True;";
            const string ConnectionStringKey = "quartz.dataSource.default.connectionString";
            string configuration =
                @"quartz.jobStore.type = Quartz.Impl.AdoJobStore.JobStoreTX, Quartz
quartz.jobStore.useProperties = false
quartz.jobStore.dataSource = default" + Environment.NewLine +
ConnectionStringKey+ " = " + ConnectionStringValue + Environment.NewLine +
"quartz.dataSource.default.provider = SqlServer-20";

            // initialize data
            MemoryStream ms = new MemoryStream();
            byte[] data = Encoding.UTF8.GetBytes(configuration);
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Position = 0;

            // intercept call
            InterceptingStdSChedulerFactory factoryToPass = new InterceptingStdSChedulerFactory();

            factory.ConfigLocation = new TestConfigLocation(ms, "description");

            m_InitSchedulerFactory.Invoke(factory, new object[] { factoryToPass });

            Assert.AreEqual(ConnectionStringValue, factoryToPass.Properties[ConnectionStringKey]);

        }
    }

    internal class TestConfigLocation : InputStreamResource
    {
        public TestConfigLocation(Stream inputStream, string description) : base(inputStream, description)
        {
        }
    }

    /// <summary>
    /// ISchedulerFactory implementation for testing purposes.
    /// </summary>
    public class TestSchedulerFactory : ISchedulerFactory
    {
        private static IScheduler mockScheduler;

        /// <summary>
        /// The mocked scheduler.
        /// </summary>
        public static IScheduler MockScheduler => mockScheduler;

        /// <inheritdoc />
        public Task<IScheduler> GetScheduler(CancellationToken _)
        {
            return Task.FromResult(mockScheduler);
        }

        /// <inheritdoc />
        public Task<IScheduler> GetScheduler(string schedName, CancellationToken _)
        {
            return Task.FromResult(mockScheduler);
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<IScheduler>> GetAllSchedulers(CancellationToken _) 
            => Task.FromResult<IReadOnlyList<IScheduler>>(new List<IScheduler>());

        public static void Initialize()
        {
            mockScheduler = A.Fake<IScheduler>();
        }
    }

    ///<summary>
    /// Scheduler factory that supports property interception.
    ///</summary>
    public class InterceptingStdSChedulerFactory : StdSchedulerFactory
    {
        private NameValueCollection properties;

        /// <inheritdoc />
        public override void Initialize(NameValueCollection props)
        {
            properties = props;
        }

        /// <summary>
        /// Return propeties given to this factory at initialization time.
        /// </summary>
        public NameValueCollection Properties => properties;
    }
}
