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

using System.Reflection;
using System.Collections;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;

namespace Spring.Aop.Target;

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
    public void SetUp()
    {
        this.ObjectFactory = new XmlObjectFactory(
            new ReadOnlyXmlTestResource("threadLocalTests.xml", GetType()));
        log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }

    /// <summary> We must simulate container shutdown, which should clear threads.</summary>
    [TearDown]
    public void TearDown()
    {
        this.ObjectFactory.Dispose();
    }

    /// <summary> Check we can use two different ThreadLocalTargetSources
    /// managing objects of different types without them interfering
    /// with one another.
    /// </summary>
    [Test]
    public virtual void UseDifferentManagedInstancesInSameThread()
    {
        ISideEffectObject apartment = (ISideEffectObject) ObjectFactory.GetObject("apartment");
        Assert.That(apartment.Count, Is.EqualTo(INITIAL_COUNT));
        apartment.doWork();
        Assert.That(apartment.Count, Is.EqualTo(INITIAL_COUNT + 1));

        ITestObject test = (ITestObject) ObjectFactory.GetObject("threadLocal2");
        Assert.That(test.Name, Is.EqualTo("Rod"));
        Assert.That(test.Spouse.Name, Is.EqualTo("Kerry"));
    }

    [Test]
    public virtual void ReuseInSameThread()
    {
        ISideEffectObject apartment = (ISideEffectObject) ObjectFactory.GetObject("apartment");
        Assert.That(apartment.Count, Is.EqualTo(INITIAL_COUNT));
        apartment.doWork();
        Assert.That(apartment.Count, Is.EqualTo(INITIAL_COUNT + 1));

        apartment = (ISideEffectObject) ObjectFactory.GetObject("apartment");
        Assert.That(apartment.Count, Is.EqualTo(INITIAL_COUNT + 1));
    }

    /// <summary> Relies on introduction
    ///
    /// </summary>
    [Test]
    public virtual void CanGetStatsViaMixinIfThereIsAnInterceptorTakingCareOfThem()
    {
        IThreadLocalTargetSourceStats stats = (IThreadLocalTargetSourceStats) ObjectFactory.GetObject("apartment");
        Assert.That(stats.Invocations, Is.EqualTo(0));
        ISideEffectObject apartment = (ISideEffectObject) ObjectFactory.GetObject("apartment");
        apartment.doWork();
        Assert.That(stats.Invocations, Is.EqualTo(1));
        Assert.That(stats.Hits, Is.EqualTo(0));
        apartment.doWork();
        Assert.That(stats.Invocations, Is.EqualTo(2));
        Assert.That(stats.Hits, Is.EqualTo(1));
        // Only one thread so only one object can have been bound
        Assert.That(stats.Objects, Is.EqualTo(1));
    }

    public class Runner
    {
        private ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ThreadLocalTargetSourceTests factory;
        public ISideEffectObject mine;

        public Runner(ThreadLocalTargetSourceTests enclosingInstance)
        {
            this.factory = enclosingInstance;
        }

        public virtual void Run()
        {
            log.LogDebug("getting object");
            this.mine = (ISideEffectObject) factory.ObjectFactory.GetObject("apartment");
            log.LogDebug("got object; hash code: {HashCode}", this.mine.GetHashCode());
            Assert.That(mine.Count, Is.EqualTo(ThreadLocalTargetSourceTests.INITIAL_COUNT));
            mine.doWork();
            Assert.That(mine.Count, Is.EqualTo(ThreadLocalTargetSourceTests.INITIAL_COUNT + 1));
        }
    }

    [Test]
    public virtual void NewThreadHasOwnInstance()
    {
        ISideEffectObject apartment = (ISideEffectObject) ObjectFactory.GetObject("apartment");
        log.LogDebug("got object; hash code: {HashCode}", apartment.GetHashCode());
        Assert.That(apartment.Count, Is.EqualTo(INITIAL_COUNT));
        apartment.doWork();
        apartment.doWork();
        apartment.doWork();
        Assert.That(apartment.Count, Is.EqualTo(INITIAL_COUNT + 3));

        Runner r = new Runner(this);
        Thread t = new Thread(new ThreadStart(r.Run));
        t.Start();
        t.Join();

        Assert.That(r, Is.Not.Null);

        // Check it didn't affect the other thread's copy
        Assert.That(apartment.Count, Is.EqualTo(INITIAL_COUNT + 3));

        // When we use other thread's copy in this thread
        // it should behave like ours
        Assert.That(r.mine.Count, Is.EqualTo(INITIAL_COUNT + 3));

        // Bound to two threads
        Assert.That(((IThreadLocalTargetSourceStats) apartment).Objects, Is.EqualTo(2));
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
        IMultiThreadInterface mtObject = (IMultiThreadInterface) ObjectFactory.GetObject("mtTest");

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

        Assert.That(multiThreadedTestFailed, Is.False);
    }

    private void CheckName(object mtObject)
    {
        string name = ((IMultiThreadInterface) mtObject).GenerateAndSetName(100);

        // Returned name should be equal to property.
        if (!name.Equals(((IMultiThreadInterface) mtObject).Name))
        {
            multiThreadedTestFailed = true;
        }
        //Console.WriteLine(String.Format("Expected: {0}; Actual: {1}",
        //    name, ((IMultiThreadInterface)mtObject).Name));
    }

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
}
