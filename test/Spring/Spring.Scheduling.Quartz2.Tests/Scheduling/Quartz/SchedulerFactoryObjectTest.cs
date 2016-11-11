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
using NUnit.Framework;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using Rhino.Mocks;

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
            TestSchedulerFactory.MockScheduler.Stub(x => x.SchedulerName).Return("scheduler").Repeat.Any();
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
            // set expectations
            TestSchedulerFactory.MockScheduler.JobFactory = null;

            factory.SchedulerFactoryType = typeof(TestSchedulerFactory);
            factory.AutoStartup = false;
            factory.AfterPropertiesSet();

            TestSchedulerFactory.MockScheduler.AssertWasCalled(x => x.JobFactory = null);
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_Calendars()
        {
            InitForAfterPropertiesSetTest();

            const string calendarName = "calendar";
            ICalendar cal = MockRepository.GenerateMock<ICalendar>();
            Hashtable calTable = new Hashtable();
            calTable[calendarName] = cal;
            factory.Calendars = calTable;

            factory.AfterPropertiesSet();

            TestSchedulerFactory.MockScheduler.AssertWasCalled(x => x.AddCalendar(calendarName, cal, true, true));
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

            TestSchedulerFactory.MockScheduler.Stub(x => x.GetTrigger(new TriggerKey(TRIGGER_NAME, TRIGGER_GROUP))).Return(trigger);

            factory.AfterPropertiesSet();
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestAfterPropertiesSet_Trigger_TriggerDoesntExist()
        {
            InitForAfterPropertiesSetTest();

            const string TRIGGER_NAME = "trigName";
            const string TRIGGER_GROUP = "trigGroup";
            SimpleTriggerImpl trigger = new SimpleTriggerImpl(TRIGGER_NAME, TRIGGER_GROUP);
            factory.Triggers = new ITrigger[] { trigger };

            factory.AfterPropertiesSet();

            TestSchedulerFactory.MockScheduler.AssertWasCalled(x => x.ScheduleJob(trigger));
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
        public void TestStart()
        {
            factory.SchedulerFactoryType = typeof(TestSchedulerFactory);
            factory.AutoStartup = false;
            factory.AfterPropertiesSet();
            factory.Start();

            TestSchedulerFactory.MockScheduler.AssertWasCalled(x => x.JobFactory = Arg<IJobFactory>.Is.NotNull);
            TestSchedulerFactory.MockScheduler.AssertWasCalled(x => x.Start());
        }

        /// <summary>
        /// Tests AfterPropertiesSet behavior.
        /// </summary>
        [Test]
        public void TestStop()
        {
            factory.SchedulerFactoryType = typeof(TestSchedulerFactory);
            factory.AutoStartup = false;
            factory.AfterPropertiesSet();
            factory.Stop();

            TestSchedulerFactory.MockScheduler.AssertWasCalled(x => x.JobFactory = Arg<IJobFactory>.Is.NotNull);
            TestSchedulerFactory.MockScheduler.AssertWasCalled(x => x.Standby());
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
        public static IScheduler MockScheduler
        {
            get { return mockScheduler; }
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public IScheduler GetScheduler()
        {
            return mockScheduler;
        }

        ///<summary>
        ///</summary>
        ///<param name="schedName"></param>
        ///<returns></returns>
        public IScheduler GetScheduler(string schedName)
        {
            return mockScheduler;
        }

        ///<summary>
        ///</summary>
        public ICollection<IScheduler> AllSchedulers
        {
            get { return new List<IScheduler>(); }
        }

        public static void Initialize()
        {
            mockScheduler = MockRepository.GenerateMock<IScheduler>();
        }
    }

    ///<summary>
    /// Scheduler factory that supports property interception.
    ///</summary>
    public class InterceptingStdSChedulerFactory : StdSchedulerFactory
    {
        private NameValueCollection properties;

        ///<summary>
        /// Initializes the factory.
        ///</summary>
        ///<param name="props"></param>
        public override void Initialize(NameValueCollection props)
        {
            this.properties = props;
        }

        /// <summary>
        /// Return propeties given to this factory at initialization time.
        /// </summary>
        public NameValueCollection Properties
        {
            get { return properties; }
        }
    }
}
