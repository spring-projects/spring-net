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
using System.Reflection;
using System.Threading;
using System.Collections;
using Common.Logging;
using NUnit.Framework;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;

namespace Spring.Aop.Target
{
    /// <author>Rod Johnson</author>
    /// <author>Federico Spinazzi</author>
    [TestFixture]
    public class ThreadLocalTargetSourceTests
    {
        /// <summary>Initial count value set in Object factory XML </summary>
        private const int INITIAL_COUNT = 10;

        private XmlObjectFactory ObjectFactory;

        private ILog log;

        [SetUp]
        public void SetUp ()
        {
            this.ObjectFactory = new XmlObjectFactory (
                new ReadOnlyXmlTestResource ("threadLocalTests.xml", GetType ()));
            log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);
        }

        /// <summary> We must simulate container shutdown, which should clear threads.</summary>
        [TearDown]
        public void TearDown ()
        {
            this.ObjectFactory.Dispose ();
        }

        /// <summary> Check we can use two different ThreadLocalTargetSources
        /// managing objects of different types without them interfering
        /// with one another.
        /// </summary>
        [Test]
        public virtual void UseDifferentManagedInstancesInSameThread ()
        {
            ISideEffectObject apartment = (ISideEffectObject) ObjectFactory.GetObject ("apartment");
            Assert.AreEqual (INITIAL_COUNT, apartment.Count);
            apartment.doWork ();
            Assert.AreEqual (INITIAL_COUNT + 1, apartment.Count);

            ITestObject test = (ITestObject) ObjectFactory.GetObject ("threadLocal2");
            Assert.AreEqual ("Rod", test.Name);
            Assert.AreEqual ("Kerry", test.Spouse.Name);
        }

        [Test]
        public virtual void ReuseInSameThread ()
        {
            ISideEffectObject apartment = (ISideEffectObject) ObjectFactory.GetObject ("apartment");
            Assert.AreEqual (INITIAL_COUNT, apartment.Count);
            apartment.doWork ();
            Assert.AreEqual (INITIAL_COUNT + 1, apartment.Count);

            apartment = (ISideEffectObject) ObjectFactory.GetObject ("apartment");
            Assert.AreEqual (INITIAL_COUNT + 1, apartment.Count);
        }

        /// <summary> Relies on introduction
        /// 
        /// </summary>
        [Test]
        public virtual void CanGetStatsViaMixinIfThereIsAnInterceptorTakingCareOfThem ()
        {
            IThreadLocalTargetSourceStats stats = (IThreadLocalTargetSourceStats) ObjectFactory.GetObject ("apartment");
            Assert.AreEqual (0, stats.Invocations);
            ISideEffectObject apartment = (ISideEffectObject) ObjectFactory.GetObject ("apartment");
            apartment.doWork ();
            Assert.AreEqual (1, stats.Invocations);
            Assert.AreEqual (0, stats.Hits);
            apartment.doWork ();
            Assert.AreEqual (2, stats.Invocations);
            Assert.AreEqual (1, stats.Hits);
            // Only one thread so only one object can have been bound
            Assert.AreEqual (1, stats.Objects);
        }


        public class Runner
        {
            private ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);
            private ThreadLocalTargetSourceTests factory;
            public ISideEffectObject mine;

            public Runner (ThreadLocalTargetSourceTests enclosingInstance)
            {
                this.factory = enclosingInstance;
            }

            public virtual void Run ()
            {
                log.Debug ("getting object");
                this.mine = (ISideEffectObject) factory.ObjectFactory.GetObject ("apartment");
                log.Debug (String.Format ("got object; hash code: {0}", this.mine.GetHashCode ()));
                Assert.AreEqual (ThreadLocalTargetSourceTests.INITIAL_COUNT, mine.Count);
                mine.doWork ();
                Assert.AreEqual (ThreadLocalTargetSourceTests.INITIAL_COUNT + 1, mine.Count);
            }
        }

        [Test]
        public virtual void NewThreadHasOwnInstance ()
        {
            ISideEffectObject apartment = (ISideEffectObject) ObjectFactory.GetObject ("apartment");
            log.Debug (String.Format ("got object; hash code: {0}", apartment.GetHashCode ()));
            Assert.AreEqual (INITIAL_COUNT, apartment.Count);
            apartment.doWork ();
            apartment.doWork ();
            apartment.doWork ();
            Assert.AreEqual (INITIAL_COUNT + 3, apartment.Count);

            Runner r = new Runner (this);
            Thread t = new Thread (new ThreadStart (r.Run));
            t.Start ();
            t.Join ();

            Assert.IsNotNull (r);

            // Check it didn't affect the other thread's copy
            Assert.AreEqual (INITIAL_COUNT + 3, apartment.Count);

            // When we use other thread's copy in this thread 
            // it should behave like ours
            Assert.AreEqual (INITIAL_COUNT + 3, r.mine.Count);

            // Bound to two threads
            Assert.AreEqual (2, ((IThreadLocalTargetSourceStats) apartment).Objects);
        }

        private static bool multiThreadedTestFailed = false;

        [Test]
        public virtual void MultiThreadedTest()
        {
            multiThreadedTestFailed = false;

            this.ObjectFactory = new XmlObjectFactory(
                new ReadOnlyXmlTestResource("threadLocalTests.xml", GetType()));
            log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            // Initialize property.
            IMultiThreadInterface mtObject = (IMultiThreadInterface)ObjectFactory.GetObject("mtTest");

            // Start threads.
            ArrayList threads = new ArrayList();
            for (int i = 0; i < 100; i++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(CheckName));
                threads.Add(thread);
                thread.Start(mtObject);
            }

            // Wait for threads to end.
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Assert.IsFalse(multiThreadedTestFailed);
        }

        private void CheckName(object mtObject)
        {
            string name = ((IMultiThreadInterface)mtObject).GenerateAndSetName(100);

            // Returned name should be equal to property.
            if (!name.Equals(((IMultiThreadInterface)mtObject).Name))
            {
                multiThreadedTestFailed = true;
            }
            //Console.WriteLine(String.Format("Expected: {0}; Actual: {1}",
            //    name, ((IMultiThreadInterface)mtObject).Name));
        }

        #region Helper classes

        public interface IMultiThreadInterface
        {
            string Name { get; }
            string GenerateAndSetName(int sleep);
        }

        public class MultiThreadClass : IMultiThreadInterface
        {
            private string _name;

            public string Name
            {
                get { return _name; }
            }

            public string GenerateAndSetName(int sleep)
            {
                string generated = "Thread_" + Thread.CurrentThread.ManagedThreadId;
                _name = generated;

                Thread.Sleep(sleep);
                return generated;
            }
        }

        #endregion
    }
}