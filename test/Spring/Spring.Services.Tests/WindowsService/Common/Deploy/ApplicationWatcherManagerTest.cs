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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using NUnit.Framework;

using Spring.Threading;

namespace Spring.Services.WindowsService.Common.Deploy
{
    [TestFixture]
	public class ApplicationWatcherManagerTest
	{
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IApplication application;
        private string appName;
        private ApplicationWatcherManager manager;
        private ISync reconfigured;
        private int nReconfigured;

        [SetUp]
        public void SetUp ()
        {
            nReconfigured = 0;
            appName = Guid.NewGuid().ToString();
            application = new Application(appName);
            Directory.CreateDirectory(application.FullPath);
            manager = new ApplicationWatcherManager(application, 10);
            manager.Reconfigured += new EventHandler(manager_Reconfigured);
            reconfigured = new Semaphore(0);
        }

        [TearDown]
        public void TearDown ()
        {
            manager.Dispose ();
            Directory.Delete(application.FullPath, true);
        }

        [Test]
		public void RecreatesTheWatcherWhenItsDefinitonChanges()
		{
            Assert.IsTrue(manager.Watcher is NullApplicationWatcher);
            File.Copy("Data/Xml/watcher-0.xml", application.WatcherXmlFullPath, true);
            reconfigured.Acquire();
            Assert.IsTrue(manager.Watcher is NullApplicationWatcher);
            TestUtils.SafeCopyFile("Data/Xml/watcher-1.xml", application.WatcherXmlFullPath, true);
            reconfigured.Acquire();
            Assert.IsTrue(manager.Watcher is FileSystemApplicationWatcher);
            TestUtils.SafeDeleteFile(application.WatcherXmlFullPath);
            reconfigured.Acquire();
            Assert.IsTrue(manager.Watcher is NullApplicationWatcher);
            Assert.AreEqual(3, nReconfigured);
        }

        class Watcher : IApplicationWatcher
        {
            IApplication application;
            public bool started, stopped, disposed, filters;

            public IApplication Application
            {
                get { return application; }
                set { application = value; }
            }

            public void StartWatching (IDeployEventDispatcher dispatcher)
            {
                started = true;
            }

            public void StopWatching ()
            {
                stopped = true;
            }

            public void SetFilters (IList allows, IList disallows)
            {
                filters = true;
            }

            public void Dispose ()
            {
                disposed = true;
            }
        }

        class Factory : IApplicationWatcherFactory
        {
            public Watcher lastWatcher;
            public int called = 0;

            public IApplicationWatcher CreateApplicationWatcher (IApplication application)
            {
                called++;
                lastWatcher = new Watcher();
                lastWatcher.Application = application;
                return lastWatcher;
            }

            public ApplicationWatcherManager CreateApplicationWatcherMonitor (IApplication application)
            {
                throw new NotImplementedException ();
            }
        }

        [Test]
        public void StopsAndDisposeTheOldWatcherAndStartTheNewWatcherWhenItChanges ()
        {
            TestUtils.ConfigureLog4Net();
            IApplication application = new Application(appName);
            Factory factory = new Factory();
            ApplicationWatcherManager m = 
                new ApplicationWatcherManager(factory, application, 100);
            m.StartWatching(new ForwardingDeployEventDispatcher());
            manager.Reconfigured -= new EventHandler(manager_Reconfigured);
            m.Reconfigured += new EventHandler(manager_Reconfigured);


            File.Copy("Data/Xml/watcher-0.xml", application.WatcherXmlFullPath, true);
            reconfigured.Acquire();
            log.Debug("first empty definition copied");
            Assert.IsTrue(manager.Watcher is NullApplicationWatcher);
            Watcher watcher1 = factory.lastWatcher;
            Assert.IsTrue(watcher1.started);

            bool ok = true;
            do
            {
                try
                {
                    File.Copy("Data/Xml/watcher-1.xml", application.WatcherXmlFullPath, true);
                }
                catch (Exception e)
                {
                    ok = false;
                    Thread.Sleep(100);
                    log.Error("error copying file", e);
                }
            } while (ok == false);
            reconfigured.Acquire(); 
            log.Debug("good definition copied");
            while (!(manager.Watcher is FileSystemApplicationWatcher))
                Thread.Sleep(10);
            Watcher watcher2 = factory.lastWatcher;
            Assert.IsTrue(watcher2.started);
            Assert.IsTrue(watcher1.stopped);
            Assert.IsTrue(watcher1.disposed);
            Assert.IsTrue(manager.Watcher is FileSystemApplicationWatcher);

            do
            {
                try
                {
                    File.Delete(application.WatcherXmlFullPath);
                }
                catch (Exception e)
                {
                    log.Error("error cancelling file", e);
                }
            } while (File.Exists(application.WatcherXmlFullPath));

            reconfigured.Acquire();
            log.Debug("good definition deleted");
            while (!(manager.Watcher is NullApplicationWatcher))
                Thread.Sleep(10);
            Watcher watcher3 = factory.lastWatcher;
            Assert.IsTrue(watcher3.started);
            Assert.IsTrue(watcher2.stopped);
            Assert.IsTrue(watcher2.disposed);

            Assert.IsTrue(manager.Watcher is NullApplicationWatcher);
            Assert.AreEqual(3, nReconfigured);
            Assert.AreEqual(3 + 1, factory.called); // +1 in ApplicationWatcherManager ctor
        }

        private void manager_Reconfigured(object sender, EventArgs e)
        {
            ++nReconfigured;
            log.Debug("releasing reconfigured sync. n. of reconfigs: " + nReconfigured);
            reconfigured.Release();
        }
    }
}
