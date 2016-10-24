using System.Threading;

using NUnit.Framework;

#region Licence

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

namespace Spring.Threading
{
    [TestFixture]
    public class LatchTest 
    {

        class Worker
        {
            public bool worked;
            ISync startPermit;
            ISync workedSync;

            public Worker (ISync startPermit, ISync workedSync)
            {
                this.startPermit = startPermit;
                this.workedSync = workedSync;
            }

            public void Work() 
            {
                // very slow worker initialization ...
                // ... attach to messaging system
                // ... connect to database
                startPermit.Acquire();
                // ... use resources initialized in Mush
                // ... do real work
                worked = true;
                // ... we are finished
                workedSync.Release();
            }
        }

        [Test]
        public void BossWorkerDemo() 
        {
            int n = 10;

            Latch startPermit = new Latch();
            Semaphore wait = new Semaphore(-(n - 1));
            
            Worker [] workers = new Worker[n];
            for (int i=0; i<n; ++i) 
            {
                workers[i] = new Worker(startPermit, wait);
                new Thread(new ThreadStart(workers[i].Work)).Start();
            }
            // very slow main initialization ...
            // ... parse configuration
            // ... initialize other resources used by workers
            startPermit.Release();

            // now it is our turn to wait for workers
            wait.Acquire();

            for (int i=0; i<n; ++i) 
                Assert.IsTrue(workers[i].worked);
        }
    }
}