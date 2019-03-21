#region License
/*
* Copyright 2002-2010 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      https://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using log4net;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;
using Spring.Threading;

namespace Spring.Services.WindowsService.Common.Deploy.FileSystem
{
    public class TestingDispatcher : IDeployEventDispatcher
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IList events = new ArrayList();
        ISync wait = new Semaphore(0);

        public void PerformDispatch (object sender)
        {
            lock (this)
            {
                log.Debug(String.Format("performing dispatch: {0} events", events.Count));
                foreach (DeployEventArgs eventArgs in events)
                {
                    if (DeployEvent != null)
                    {
                        log.Debug(String.Format("raising event {0}", eventArgs.EventType));
                        DeployEvent(sender, eventArgs);
                    }
                }
                log.Debug(String.Format("clearing events list", events.Count));
                events.Clear();
            }
        }

        public void Dispatch (IDeployLocation sender, DeployEventType eventType, IApplication application)
        {
            lock (this)
            {
                log.Debug(String.Format("collecting event {0} (collected so far: {1})", eventType, events.Count));
                events.Add(new DeployEventArgs(application, DeployEventType.ApplicationUpdated));
                wait.Release();
            }
        }

        public event DeployEventHandler DeployEvent;

        public void Dispose ()
        {
        }

        public void Wait()
        {
            wait.Acquire();
        }

        public void WaitAndPerformDispatch(object o)
        {
            Wait();
            PerformDispatch(o);
        }
    }


    // problems with dynamic mocks: too many calls 
    public class MockFileSystemEventFilter : IFileSystemEventFilter
    {
        public bool called = false;
        public bool accept;

        public MockFileSystemEventFilter (bool accept)
        {
            this.accept = accept;
        }

        public bool Filter (FileSystemEventArgs args)
        {
            called = true;
            return accept;
        }
    }


    [TestFixture]
    public class FilteringSupportTest
    {
        IList allow;
        IList disallow;
        FileSystemEventArgs anEvent;
        FilteringSupport support;
        FileSystemEventArgs springAssemblyEvent;
        IApplication application;

        [SetUp]
        public void SetUp ()
        {
            allow = new ArrayList();
            disallow = new ArrayList();
            application = new Application(Path.GetFullPath("foo"));
            anEvent = new FileSystemEventArgs(WatcherChangeTypes.All, "foo", "foo.dll");
            springAssemblyEvent = 
                new FileSystemEventArgs(WatcherChangeTypes.All, 
                    Path.GetFullPath(SpringAssembliesDeployer.PrivateBinPathPrefix + "foo"), "foo.dll");
            support = new FilteringSupport(allow, disallow, SpringAssembliesDeployer.DisallowFilter);
        }

        [Test]
        public void AnEventIsAcceptedIfThereIsNoFilterDefined ()
        {
            Assert.IsTrue(support.Accept(anEvent), "event not accepted with no filter defined");
        }

        [Test]
        public void EventsRelatedToSpringAssembliesAreNeverAccepted ()
        {
            Assert.IsFalse(support.Accept(springAssemblyEvent), "spring assemblies event accepted");
            allow.Add(new RegularExpressionFilter(
                String.Format ("**/{0}*/**", SpringAssembliesDeployer.PrivateBinPathPrefix), true));
            Assert.IsFalse(support.Accept(springAssemblyEvent), "spring assemblies event accepted");
        }

        [Test]
        public void AnEventIsAcceptedIfAtLeastOneAllowFilterFiltersIt ()
        {

            allow.Add(new MockFileSystemEventFilter(false));            
            Assert.IsFalse(support.Accept(anEvent), "allowed when no allow filter allows");
            allow.Add(new MockFileSystemEventFilter(true));            
            Assert.IsTrue(support.Accept(anEvent), "not allowed when one allow filter allows");
            allow.Add(new MockFileSystemEventFilter(false));
            Assert.IsTrue(support.Accept(anEvent), "not allowed when one allow filter allows");
        }

        [Test]
        public void AnEventIsNotAcceptedIfAtLeastOneDisallowFilterFiltersIt ()
        {
            disallow.Add(new MockFileSystemEventFilter(false));            
            Assert.IsTrue(support.Accept(anEvent), "disallowed when no disallow filter disallows");
            disallow.Add(new MockFileSystemEventFilter(true));            
            Assert.IsFalse(support.Accept(anEvent), "allowed when one disallow filter disallows");
            disallow.Add(new MockFileSystemEventFilter(false));
            Assert.IsFalse(support.Accept(anEvent), "allowed when one disallow filter disallows");
        }

        [Test]
        public void ADisallowingFilterDominatesAllowFilters ()
        {
            disallow.Add(new MockFileSystemEventFilter(true));            
            allow.Add(new MockFileSystemEventFilter(false));            
            Assert.IsFalse(support.Accept(anEvent), "event accepted when a disallow filter disallows");
            allow.Add(new MockFileSystemEventFilter(true));            
            Assert.IsFalse(support.Accept(anEvent), "event accepted when a disallow filter disallows");
        }

        [Test]
        public void WithNoDisallowingFilterItDependsOnThePresenceOfAnAllowingFilters ()
        {
            disallow.Add(new MockFileSystemEventFilter(false));            
            allow.Add(new MockFileSystemEventFilter(false));            
            Assert.IsFalse(support.Accept(anEvent), "event accepted when a disallow filter disallows");
            allow.Add(new MockFileSystemEventFilter(true));            
            Assert.IsTrue(support.Accept(anEvent), "event not accepted when an allow filter allows and no disallow filter disallows");
            disallow.Add(new MockFileSystemEventFilter(true));            
            Assert.IsFalse(support.Accept(anEvent), "event accepted when a disallow filter disallows");
        }

        [Test]
        public void IgnoreEventsRegardingTheDirectoryOfTheApplication()
        {
            FileSystemEventArgs e = 
                new FileSystemEventArgs(WatcherChangeTypes.All, 
                Directory.GetCurrentDirectory(), "foo");
            Assert.IsFalse(support.IsApplicationEvent(e, application, true));

            e = new FileSystemEventArgs(WatcherChangeTypes.All, 
                Path.GetFullPath("foo"), "foo.dll");
            Assert.IsTrue(support.IsApplicationEvent(e, application, true));

            e = new FileSystemEventArgs(WatcherChangeTypes.All, 
                Directory.GetCurrentDirectory(), "foo/foo.dll");
            Assert.IsTrue(support.IsApplicationEvent(e, application, true));

            e = new FileSystemApplicationWatcher(application, 
                new NullWatcherConfigurer()).GenericApplicationEvent;
            Assert.IsTrue(support.IsApplicationEvent(e, application, true));
        }

        [Test]
        public void CanDecideIfAnEventIsAnApplicationEventIgnoringCase()
        {
            FileSystemEventArgs e = 
                new FileSystemEventArgs(WatcherChangeTypes.All, 
                Directory.GetCurrentDirectory(), "FOO/FOO.DLL");
            application = new Application(Path.GetFullPath("foo").ToUpper());
            Assert.IsTrue(support.IsApplicationEvent(e, application, true), "failed to match without case");
            e = new FileSystemEventArgs(WatcherChangeTypes.All, "C:/", "foo/foo.dll");
            Assert.IsFalse(support.IsApplicationEvent(e, application, false), "unexpected match with case");
        }

    }
	

    [TestFixture]
	public class FileSystemApplicationWatcherTest
	{
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Latch eventLatch;
        string appName, anotherAppName;
        string appFullPath, anotherAppFullPath;
        private IDeployEventDispatcher dispatcher;
        private FileSystemApplicationWatcher watcher;
        private IApplication application;
        private bool dispatched;
        private ISync controlledEventLatch;


        [SetUp]
        public void SetUp ()
        {
            TestUtils.ConfigureLog4Net();
            dispatched = false;
            appName = Guid.NewGuid().ToString();
            appFullPath = Path.GetFullPath(appName);
            anotherAppName = Guid.NewGuid().ToString();
            anotherAppFullPath = Path.GetFullPath(anotherAppName);
            Directory.CreateDirectory(appFullPath);
            Directory.CreateDirectory(anotherAppFullPath);
            eventLatch = new Latch();
            controlledEventLatch = new Latch();
            dispatcher = new ForwardingDeployEventDispatcher();
            application = new Application(appFullPath);
            dispatcher.DeployEvent += new DeployEventHandler(dispatcher_DeployEvent);
            watcher = new FileSystemApplicationWatcher(application);
        }

        [TearDown]
        public void TearDown ()
        {
            if (watcher != null)
            {
                watcher.StopWatching();
                watcher.Dispose();
            }
            dispatcher.DeployEvent -= new DeployEventHandler(dispatcher_DeployEvent);
            Directory.Delete(appFullPath, true);
            Directory.Delete(anotherAppFullPath, true);
        }

        [Test]
		public void ProduceUpdateEventsForWatchedApplication()
		{
            TestingDispatcher controlledDispatcher = new TestingDispatcher();
            controlledDispatcher.DeployEvent += new DeployEventHandler(controlledDispatcher_DeployEvent);
            watcher.StartWatching(controlledDispatcher);
            string subDir = Path.Combine(appFullPath, "foo");
            Directory.CreateDirectory(subDir);
            controlledDispatcher.WaitAndPerformDispatch(null);

            using (File.Create(Path.Combine (subDir, "foo.bar"))) {}
            controlledDispatcher.WaitAndPerformDispatch(null);
        }

        [Test]
		public void DoNotProduceUpdateEventsForOtherApplications()
		{
            TestingDispatcher controlledDispatcher = new TestingDispatcher();
            controlledDispatcher.DeployEvent += new DeployEventHandler(controlledDispatcher_DeployEvent);
            watcher.StartWatching(controlledDispatcher);
            string subDir = Path.Combine(anotherAppFullPath, "foo");
            Directory.CreateDirectory(subDir);
            using (File.Create(Path.Combine (subDir, "foo.bar"))) {}
            dispatched = false;
            controlledDispatcher.PerformDispatch(null);
            Assert.IsFalse(dispatched);
        }

        [Test]
        public void UsesEventFiltersToAllowAnEventToPropagated ()
        {
            MockFileSystemEventFilter filter = new MockFileSystemEventFilter(true);
            watcher.StartWatching(dispatcher);
            watcher.AddAllowFilter (filter);
            string subDir = Path.Combine(appFullPath, "foo");
            Directory.CreateDirectory(subDir);
            using (File.Create(Path.Combine (appFullPath, "foo.bar"))) {}
            eventLatch.Acquire();            
            Assert.IsTrue(filter.called);
        }

        [Test]
        public void CanBeConfiguredToIncludeOrExcludePathsForEvents ()
        {
            string simple = "Data/Xml/watcher-simple.xml";
            XmlObjectFactory f = new XmlObjectFactory(new FileSystemResource(simple));
            f.RegisterSingleton(DefaultApplicationWatcherFactory.InjectedApplicationName, application);
            watcher = f["watcher"] as FileSystemApplicationWatcher;
            Assert.IsNotNull(watcher, 
                String.Format("test file [{0}] should define a file sistem resource!", simple));
            Assert.AreEqual(1, watcher.Excludes.Count);
            Assert.AreEqual(1, watcher.Includes.Count);

            watcher.StartWatching(dispatcher);

            // propagated
            string subDir = Path.Combine(appFullPath, "foo");
            Directory.CreateDirectory(subDir);
            using (File.Create(Path.Combine (subDir, "foo.bar"))) {}
            eventLatch.Acquire();            
            Assert.IsFalse(dispatched);
        }

        [Test]
        public void ByDefaultTheCaseIsIngoredButCanBeConfigured ()
        {
            Assert.IsTrue(watcher.IgnoreCase);
            watcher.IgnoreCase = false;
            Assert.IsFalse(watcher.IgnoreCase);
        }

        private void dispatcher_DeployEvent(object sender, DeployEventArgs args)
        {
            log.Debug("releasing latch");
            eventLatch.Release();
            log.Debug("latch released");
        }

        private void controlledDispatcher_DeployEvent(object sender, DeployEventArgs args)
        {
            dispatched = true;
            log.Debug(String.Format("application = {0}", args.Application.FullPath));
            log.Debug(String.Format("anotherAppFullPath = {0}", anotherAppFullPath));
            controlledEventLatch.Release();
        }
    }
}
