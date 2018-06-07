#region License
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
#endregion
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using DotNetMock.Dynamic;
using log4net;
using NUnit.Framework;
using Spring.Threading;

namespace Spring.Services.WindowsService.Common.Deploy.FileSystem
{
    public abstract class FileSystemDeployLocationTestsBase
    {
        protected ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);

        protected string deployPath;
        protected string deploy = "deploy";
        protected string sampleDir, sampleDir2;

        protected FileSystemDeployLocation location;
        protected TestingHandler handler;
        protected TestingHandler dontUseHandler;
        protected IDeployEventDispatcher dispatcher;
        protected ITrigger trigger;
        protected string serviceXml;
        protected string watcherXml;
        protected Latch triggeredLatch;

        protected void SetUp_ ()
        {
            deployPath = Guid.NewGuid ().ToString ();
            sampleDir = Path.Combine (deployPath, deploy);
            sampleDir2 = Path.Combine (deployPath, deploy + "2");
            serviceXml = Path.Combine (sampleDir, Application.ServiceXml);
            watcherXml = Path.Combine (sampleDir, Application.WatcherXml);
            triggeredLatch = new Latch();
            trigger = new ThreadingTimerTrigger (50);
            trigger.Triggered += new EventHandler(trigger_Triggered);
            dispatcher = new AggregatedDeployEventDispatcher (trigger);
            TestUtils.ConfigureLog4Net ();
        }

        protected void AddApplication ()
        {
            AddApplication (sampleDir);
        }

        protected void AddApplication (string dir)
        {
            Directory.CreateDirectory (dir);
            File.Copy("Data/Xml/watcher-1.xml", new Application(dir).WatcherXmlFullPath);
            using (File.Create (Path.Combine (dir, Application.ServiceXml)))
            {}
            Assert.IsTrue (Directory.Exists (dir), "directory does not exist: " + dir);
        }

        protected void trigger_Triggered (object sender, EventArgs e)
        {
            triggeredLatch.Release();
        }

        protected void InitHandlerAndStartLocation (ISync sync)
        {
            GC.SuppressFinalize (location);
            InitHandler (sync);
            location.StartWatching ();
        }

        protected void InitHandler (ISync sync)
        {
            location = new FileSystemDeployLocation (dispatcher, deployPath);
            handler = NewHandler (sync);
        }

        protected TestingHandler NewHandler (ISync sync)
        {
            try
            {
                log.Debug("disconnecting testing handler");
                location.DeployEvent -= new DeployEventHandler (dontUseHandler.Handle);
            }
            catch
            {}
            dontUseHandler = new TestingHandler (sync);
            log.Debug("connecting testing handler");
            location.DeployEvent += new DeployEventHandler (dontUseHandler.Handle);
            return dontUseHandler;
        }

        public void TearDown_ ()
        {
            log.Debug("disposing location ");
            if (location != null)
            {
                location.Dispose ();
            }                
            try
            {
                TestUtils.SafeDeleteDirectory(Path.GetFullPath (deployPath));
            }
            catch (Exception e)
            {
                log.Error("deploy folder not deleted", e);                
            }
            try
            {
                TestUtils.SafeDeleteDirectory(Path.GetFullPath (deployPath));
            }
            catch (Exception e)
            {
                log.Error("test folder not deleted", e);
            }
            Assert.IsFalse(Directory.Exists(Path.GetFullPath (deployPath)), "the root of the deploy directory still exist"); 
        }
    }

    [TestFixture]
    public class FileSystemDeployLocationTest_Mocked : FileSystemDeployLocationTestsBase
    {
        private DynamicMock factoryMock;
        private DynamicMock watcherMock;
        private IApplicationWatcherFactory factory;
        private IApplicationWatcher watcher;
        private bool deployEventDispatched;
        private ISync forwardDispatcherSync;

        [SetUp]
        public void SetUp ()
        {
            base.SetUp_();
            factoryMock = new DynamicMock(typeof(IApplicationWatcherFactory));
            watcherMock = new DynamicMock(typeof(IApplicationWatcher));
            factory = (IApplicationWatcherFactory) factoryMock.Object;
            watcher = (IApplicationWatcher) watcherMock.Object;
            deployEventDispatched = false;
            forwardDispatcherSync = new Latch();
        }

        [TearDown]
        public void TearDown ()
        {
            base.TearDown_();
            factoryMock.Verify();
            watcherMock.Verify();
        }

        [Test]
        public void DisposeDispatcherOnDispose ()
        {
            DynamicMock mock = new DynamicMock (typeof (IDeployEventDispatcher));
            IDeployEventDispatcher dispatcher = (IDeployEventDispatcher) mock.Object;
            mock.Expect ("Dispose");
            IDeployLocation location = new FileSystemDeployLocation (dispatcher, deployPath);
            location.Dispose ();
            mock.Verify ();
        }

        [Test]
        public void ANewWatcherGetsCreatedAndAssociatedToAnyExistingApplication ()
        {
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            watcherMock.Expect("StartWatching");
            AddApplication ();
            location = new FileSystemDeployLocation (new ForwardingDeployEventDispatcher(), factory, deployPath, true);
        }

        [Test]
        public void TheAssociatedWatcherIsStoppedAndDisposedWhenAnApplicationGetsRemoved ()
        {
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            watcherMock.Expect("StartWatching");
            watcherMock.Expect("StopWatching");
            watcherMock.Expect("Dispose");
            AddApplication();
            location = new FileSystemDeployLocation (new ForwardingDeployEventDispatcher(), factory, deployPath, true);
            Directory.Delete(sampleDir, true);
            triggeredLatch.Acquire();
        }        

        [Test]
        public void TheAssociatedWatcherIsStoppedAndDisposedOnDispose ()
        {
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            watcherMock.Expect("StartWatching");
            watcherMock.Expect("StopWatching");
            watcherMock.Expect("Dispose");
            AddApplication();
            location = new FileSystemDeployLocation (new ForwardingDeployEventDispatcher(), factory, deployPath, true);
            triggeredLatch.Acquire();
            location.Dispose();
        }        

        [Test]
        public void IgnoresExceptionAddingExistingApplication ()
        {
            factoryMock.ExpectAndThrow("CreateApplicationWatcherMonitor", new Exception("exception generated to test behaviour adding application"));
            AddApplication();
            location = new FileSystemDeployLocation (new ForwardingDeployEventDispatcher(), factory, deployPath, true);            
        }

        [Test] // Bug fix
        public void NotInfiniteLoopRemovingAnApplicationWithAManagerThatThrowsExceptionWhenStopped ()
        {
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            watcherMock.Expect("StartWatching");
            watcherMock.ExpectAndThrow("StopWatching", new Exception());
            AddApplication();
            ForwardingDeployEventDispatcher dispatcher = new ForwardingDeployEventDispatcher();
            dispatcher.DeployEvent += new DeployEventHandler(dispatcher_DeployEvent);
            location = new FileSystemDeployLocation (dispatcher, factory, deployPath, true);
            Directory.Delete(sampleDir, true);
            forwardDispatcherSync.Acquire();
            Assert.IsTrue(deployEventDispatched, "removal not dispatched in case of error on application watcher");
        }

        [Test] // Bug fix
        public void NotInfiniteLoopRemovingAnApplicationWithAManagerThatThrowsExceptionWhenDisposed ()
        {
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            watcherMock.Expect("StartWatching");
            watcherMock.Expect("StopWatching");
            watcherMock.ExpectAndThrow("Dispose", new Exception());
            AddApplication();
            ForwardingDeployEventDispatcher dispatcher = new ForwardingDeployEventDispatcher();
            dispatcher.DeployEvent += new DeployEventHandler(dispatcher_DeployEvent);
            location = new FileSystemDeployLocation (dispatcher, factory, deployPath, true);
            Directory.Delete(sampleDir, true);
            forwardDispatcherSync.Acquire();
            Assert.IsTrue(deployEventDispatched, "removal not dispatched in case of error on application watcher");
        }

        class ExceptionDispatcher : IDeployEventDispatcher
        {
            public event DeployEventHandler DeployEvent;
            ISync started, canContinue;

            public ExceptionDispatcher (ISync started, ISync canContinue)
            {
                this.started = started;
                this.canContinue = canContinue;
            }

            public void Dispose ()
            {
            }

            public void Dispatch (IDeployLocation sender, DeployEventType eventType, IApplication application)
            {
                if (DeployEvent != null) DeployEvent(null, new DeployEventArgs(application, eventType));
                started.Release();
                canContinue.Acquire();
                throw new Exception ("this dispatcher is for testing and always raises exception");
            }

        }

        [Test]
        public void IgnoreExceptionOnDispatcherWhenAdding ()
        {
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            ISync canContinue = new Semaphore(0);
            ISync started = new Semaphore(0);
            IDeployEventDispatcher dispatcher = new ExceptionDispatcher(started, canContinue);
            location = new FileSystemDeployLocation (dispatcher, factory, deployPath, true);
            AddApplication();
            started.Acquire();
            canContinue.Release();
            Assert.AreEqual(1, location.Applications.Count);
            AddApplication(sampleDir2);
            started.Acquire();
            canContinue.Release();
            Assert.AreEqual(2, location.Applications.Count);
        }

        [Test]
        public void IgnoreExceptionOnDispatcherWhenRemoving ()
        {
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            watcherMock.Expect("StartWatching");
            watcherMock.Expect("StopWatching");
            watcherMock.Expect("Dispose");
            watcherMock.Expect("StartWatching");
            watcherMock.Expect("StopWatching");
            watcherMock.Expect("Dispose");
            ISync canContinue = new Semaphore(0);
            ISync started = new Semaphore(0);
            AddApplication();
            AddApplication(sampleDir2);
            IDeployEventDispatcher dispatcher = new ExceptionDispatcher(started, canContinue);
            location = new FileSystemDeployLocation (dispatcher, factory, deployPath, true);
            Assert.AreEqual(2, location.Applications.Count);
            Directory.Delete(sampleDir, true);
            started.Acquire();
            canContinue.Release();
            Assert.AreEqual(1, location.Applications.Count);
            Directory.Delete(sampleDir2, true);
            started.Acquire();
            canContinue.Release();
            Assert.AreEqual(0, location.Applications.Count);
        }

        [Test]
        public void OnlyApplicationsWithAManagerSuccesfullyStartedAreListed ()
        {
            factoryMock.ExpectAndReturn("CreateApplicationWatcherMonitor", watcher);
            watcherMock.ExpectAndThrow("StartWatching", new Exception());
            AddApplication();
            dispatcher.DeployEvent += new DeployEventHandler(dispatcher_DeployEvent);
            location = new FileSystemDeployLocation (dispatcher, factory, deployPath, true);
            triggeredLatch.Acquire();
            Assert.AreEqual(0, location.Applications.Count);
            Assert.IsFalse(deployEventDispatched, "add dispatched in case of error on application watcher");
        }

        private void dispatcher_DeployEvent (object sender, DeployEventArgs args)
        {
            deployEventDispatched = true;
            forwardDispatcherSync.Release();
        }
    }

    [TestFixture]
    public class FileSystemDeployLocationTest : FileSystemDeployLocationTestsBase
    {
        [SetUp]
        public void SetUp ()
        {
            base.SetUp_ ();
            InitHandler (new NullSync ());
        }

        [TearDown]
        public void TearDown ()
        {
            log.Debug("tear down");
            base.TearDown_();
        }

        private void AddInvalidApplication ()
        {
            Directory.CreateDirectory (sampleDir);
            Assert.IsTrue (Directory.Exists (sampleDir), "directory does not exist: " + sampleDir);
        }


        [Test]
        public void CreateDeployLocationPath ()
        {
            Assert.IsTrue (Directory.Exists (location.FullPath));
            Assert.AreEqual (0, location.Applications.Count);
        }

        [Test]
        public void ListExistingDirectoriesAsApplications ()
        {
            log.Debug ("start ListExistingDirectoriesAsApplications");
            AddApplication ();
            location = new FileSystemDeployLocation (deployPath);
            Assert.AreEqual (1, location.Applications.Count);
            log.Debug ("end ListExistingDirectoriesAsApplications");
        }

        [Test]
        public void CreateAFileToPreventDeployDirectoryDeletion ()
        {
            location.StartWatching ();
            Assert.IsTrue (File.Exists (location.LockFileName));
        }

        [Test]
        public void LockFileIsDeletedOnDisposeOrStop ()
        {
            FileSystemDeployLocation location = 
                new FileSystemDeployLocation (dispatcher, deployPath);
            location.StartWatching ();
            location.Dispose();
            Assert.IsFalse (File.Exists (location.LockFileName));
        }

        [Test]
        public void RaiseEventAddingAnApplication ()
        {
            Semaphore sync = new Semaphore (0);
            InitHandlerAndStartLocation (sync);
            AddApplication ();
            sync.Acquire ();
            Assert.IsTrue (handler.applicationAdded, "application not added");
            Assert.AreEqual (1, location.Applications.Count);
        }

        [Test]
        public void RaiseEventRemovingAnApplication ()
        {
            AddApplication ();
            Semaphore sync = new Semaphore (0);
            InitHandlerAndStartLocation (sync);
            Directory.Delete (sampleDir, true);
            Thread.SpinWait(1);
            Assert.IsFalse(Directory.Exists(sampleDir), "directory still exists");
            log.Debug("directory deleted");
            sync.Acquire ();
            log.Debug("sync acquired");
            Assert.IsFalse (Directory.Exists (sampleDir), "directory still exists: " + sampleDir);
            Assert.IsTrue (handler.applicationRemoved, "application not removed");
            Assert.AreEqual (0, location.Applications.Count);
        }

        [Test]
        public void RenamingADirectoryHostingAnApplicationItEqualsRemoveOldApplicationAndAddUnderNewName ()
        {
            Semaphore sync = new Semaphore (0);
            InitHandlerAndStartLocation (sync);
            AddApplication ();
            sync.Acquire ();
            Assert.IsTrue (handler.applicationAdded, "application not added");
            Directory.Move (sampleDir, sampleDir2);
            sync.Acquire (); // add or remove
            sync.Acquire (); // add or remove
            Assert.IsTrue (handler.applicationRemoved, "application was not removed");
        }


        //[Test, Ignore("Gets stuck when running entire WindowsService.Tests dll")]
        [Test]
        public void EventsAreRaisedOnlyForValidatedApplication ()
        {
            Semaphore sync = new Semaphore (0);
            InitHandlerAndStartLocation (sync);
            AddInvalidApplication ();
            triggeredLatch.Acquire();
            Assert.IsFalse (handler.applicationAdded, "invalid application was added");
            Assert.AreEqual (0, location.Applications.Count, "invalid application was added");
        }

        [Test]
        public void RemoveAfterSpringAssemblyDeploy ()
        {
            Semaphore sync = new Semaphore (0);
            InitHandlerAndStartLocation (sync);
            AddApplication();
            sync.Acquire();
            string springPrivateBin = Path.Combine(sampleDir, SpringAssembliesDeployer.PrivateBinPathPrefix);
            Directory.CreateDirectory(springPrivateBin);
            using (File.Create(Path.Combine(springPrivateBin, "foo.dll"))) {}
            Directory.Delete(sampleDir, true);
            Assert.IsFalse(Directory.Exists(sampleDir), "directory still exists");
            log.Debug (String.Format ("directory {0} removed to simulate application removal", sampleDir));
            sync.Acquire();
            Assert.IsTrue (handler.applicationRemoved, "application not removed");
            Assert.AreEqual (0, location.Applications.Count, "application not removed");
        }

        [Test]
        public void AddingRemovingUpdatingOrRenamingAFileUnderAnApplicationDirectoryItGetsUpdated ()
        {
            Semaphore sync = new Semaphore (0);
            AddApplication();
            InitHandlerAndStartLocation (sync);
            string subDir = Path.Combine(sampleDir, "foo");
            Directory.CreateDirectory(subDir);
            string aFile = Path.Combine(subDir, "foo.bar");
            sync.Acquire();

            // create
            log.Debug (String.Format ("creating a file"));
            using (File.Create(aFile)) {}
            sync.Acquire();
            Assert.IsTrue(handler.applicationUpdated, "new file created but application not updated");

            // update
            log.Debug (String.Format ("updating a file"));
            handler.applicationUpdated = false;
            File.SetLastWriteTime(aFile, DateTime.Now);
            sync.Acquire();
            Assert.IsTrue(handler.applicationUpdated, "new file created but application not updated");

            // delete
            log.Debug (String.Format ("deleting a file"));
            handler.applicationUpdated = false;
            File.Delete(aFile);
            sync.Acquire();
            Assert.IsTrue(handler.applicationUpdated, "new file created but application not updated");
        }

        [Test]
        public void CanBeDisposedMoreThanOnce ()
        {
            IDeployLocation location = new FileSystemDeployLocation(dispatcher, 
                DefaultApplicationWatcherFactory.Instance, deployPath, true);
            location.Dispose();
            location.Dispose();
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void CanBeStartedExplicitlyStartedButIsImplicitlyStoppedWhenDisposed ()
        {
            FileSystemDeployLocation location = new FileSystemDeployLocation(deployPath);
            location.StartWatching();
            location.Dispose();
            location.StartWatching();
        }

        [Test]
        public void EventsRelatedToFilesInDeployPathAreIgnored ()
        {
            FieldInfo f = location.GetType().GetField("_fileSystemWatcher", 
                BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(f);
            //FileSystemWatcher watcher = (FileSystemWatcher) f.GetValue(location);
            FileSystemMonitor watcher = (FileSystemMonitor) f.GetValue(location);
            Assert.IsNotNull(watcher);
            Assert.AreEqual(NotifyFilters.DirectoryName | NotifyFilters.Attributes, watcher.NotifyFilter);
            Assert.AreEqual(
                Enum.Parse(typeof(NotifyFilters), 0.ToString()) , 
                watcher.NotifyFilter & NotifyFilters.FileName);
        }

        [Test]
        public void AnInvalidApplicationIsNotUpdatedNorRemovedButItWillNotBeListed ()
        {
            Semaphore sync = new Semaphore (0);
            AddApplication();
            InitHandlerAndStartLocation (sync);
            File.Delete(serviceXml);
            Assert.IsFalse(sync.Attempt(1000), "some events propagated, expecting no one");
            Assert.IsFalse (handler.applicationUpdated, "application wrongly updated");
            Assert.IsFalse(handler.applicationRemoved, "application wrongly removed");
            Assert.AreEqual (0, location.Applications.Count, "application listed");

            location.Dispose();            
            location = new FileSystemDeployLocation (deployPath);
            Assert.AreEqual (0, location.Applications.Count, "invalid application listed");
        }

        [Test]
        public void AnInvalidApplicationIsNotUpdatedNorRemovedButWhenRemovedStillRaisesTheRemovedEvent ()
        {
            Semaphore sync = new Semaphore (0);
            AddApplication();
            InitHandlerAndStartLocation (sync);
            File.Delete(serviceXml);
            Assert.IsFalse(sync.Attempt(1000), "some events propagated, expecting no one");

            // remove
            TestUtils.SafeDeleteDirectory(sampleDir);
            Thread.SpinWait(1);
            Assert.IsFalse(Directory.Exists(sampleDir), "directory still exists");
            log.Debug("directory deleted");
            sync.Acquire ();
            log.Debug("sync acquired");
            Assert.IsFalse (Directory.Exists (sampleDir), "directory still exists: " + sampleDir);
            Assert.IsTrue (handler.applicationRemoved, "application not removed");
            Assert.AreEqual (0, location.Applications.Count);
        }
    }
}