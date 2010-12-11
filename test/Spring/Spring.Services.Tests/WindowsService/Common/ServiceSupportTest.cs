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
using System.Reflection;
using System.Threading;
using log4net;
using NUnit.Framework;
using Spring.Threading;

namespace Spring.Services.WindowsService.Common
{
    class StoppingHelper
    {
        private readonly ServiceSupport serviceSupport;
        private readonly ISync stopping;
        private readonly ISync stopped;
        public bool wasStopped = false;

        public StoppingHelper (ServiceSupport serviceSupport, ISync stopping, ISync stopped)
        {
            this.stopping = stopping;
            this.stopped = stopped;
            this.serviceSupport = serviceSupport;
        }

        public void Stop ()
        {
            ServiceSupportTest.Log ("stopping ...");
            stopping.Release();
            ServiceSupportTest.Log ("stopping sync released ...");
            serviceSupport.Stop(true);
            wasStopped = true;
            ServiceSupportTest.Log ("releasing stopped sync ...");
            stopped.Release();
            ServiceSupportTest.Log ("stopped ...");
        }
    }

    class BlockWhileStartingExecutor : IExecutor
    {
        private readonly ISync started;
        private readonly ISync starting;
        MyServiceSupport serviceSupport;
        public bool wasStarted = false;

        public BlockWhileStartingExecutor (ISync starting, ISync started)
        {
            this.starting = starting;
            this.started = started;
        }

        public MyServiceSupport ServiceSupport
        {
            get { return serviceSupport; }
            set { serviceSupport = value; }
        }

        public void Start ()
        {
            serviceSupport.Start(true);
        }

        public void Execute (IRunnable runnable)
        {            
            if (runnable == serviceSupport.StartedCommand)
            {
                ServiceSupportTest.Log ("waiting to start ...");
                starting.Acquire();
                ServiceSupportTest.Log ("starting ...");
                runnable.Run();
                wasStarted = true;
                ServiceSupportTest.Log ("releasing started sync ...");
                started.Release();
                ServiceSupportTest.Log ("started ...");
            }
            else
            {
                ServiceSupportTest.Log ("unexpected runnable: " + runnable);
                runnable.Run();
            }
        }
    }

    class MyServiceSupport : ServiceSupport 
    {
        public MyServiceSupport(IExecutor executor, IServiceable serviceable) : base(executor, serviceable)
        {
        }

        public IRunnable StartedCommand
        {
            get
            {
                return base.startedCommand;
            }
        }

        
    }

    class MyService : ServiceSupport.IServiceable
    {
        ISync _sync1;
        ISync _sync2;

        public MyService (ISync sync1, ISync sync2)
        {
            this._sync1 = sync1;
            this._sync2 = sync2;
        }
        public void PerformStart ()
        {
        }

        public void PerformStop ()
        {
        }

        public void Dispose ()
        {
            lock (this)
            {
                _sync1.Release();
                _sync2.Acquire();
            }
        }
    }

    [TestFixture]
	public class ServiceSupportTest
	{
        MyService serviceable;
        ServiceSupport serviceSupport;
        ISync sync1;
        ISync sync2;

        static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [SetUp]
        public void SetUp ()
        {
            sync1 = new Semaphore(0);
            sync2 = new Semaphore(0);
            serviceable = new MyService(sync1, sync2);
            serviceSupport = new ServiceSupport(serviceable);
        }

        [Test(Description="Bugfix")] 
        public void DoesNotLockTheServiceableWhenQueryingForStatus()
        {
            new Thread(new ThreadStart(serviceable.Dispose)).Start();
            sync1.Acquire();
            Assert.IsFalse(serviceSupport.StopRequested);
            sync2.Release();
        }

        [Test]
        public void WaitForStartedBeforeStopping()
        {
            MyService serviceable = new MyService(sync1, sync2);
            ISync starting = new Latch();
            ISync started = new Latch();
            BlockWhileStartingExecutor executor = 
                new BlockWhileStartingExecutor(starting, started);
            MyServiceSupport support = new MyServiceSupport(executor, serviceable);
            executor.ServiceSupport = support;
            Thread startThread = new Thread(new ThreadStart(executor.Start));
            startThread.Name = "start";
            startThread.Start();
            Log ("start thread started");
            Latch stopping = new Latch();
            Latch stopped = new Latch();
            StoppingHelper helper = new StoppingHelper(support, stopping, stopped);
            Thread stopThread = new Thread(new ThreadStart(helper.Stop));
            stopThread.Name = "stop";
            stopThread.Start();
            Log ("stop thread started: waiting for stopping ...");
            stopping.Acquire();
            Log ("stopping in progress ...");
            Assert.IsFalse(executor.wasStarted);
            Assert.IsFalse(helper.wasStopped, "helper could stop before expected");
            Log ("allow to start ...");
            starting.Release();
            Log ("waiting for started ...");
            started.Acquire();
            Assert.IsTrue(executor.wasStarted);
            stopped.Acquire();
            Log ("waiting for stop ...");
            Assert.IsTrue(helper.wasStopped);
            Log ("stopped ...");
        }

        public static void Log (string msg)
        {
            Thread currentThread = Thread.CurrentThread;
            string message = String.Format("thread [#{1}-{2}], msg = {0}", msg, currentThread.GetHashCode(), currentThread.Name);
            log.Debug(message);
            Console.Out.WriteLine (message);
        }
	}
}
