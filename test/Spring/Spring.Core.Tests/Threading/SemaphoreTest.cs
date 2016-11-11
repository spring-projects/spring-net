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
using System.Threading;
using Spring.Pool;
using NUnit.Framework;


namespace Spring.Threading
{
    [TestFixture]
    public class SemaphoreTest
    {
        private static long SHORT_DELAY_MS = 300;
        [Test]
        public void UsingLikeAMutex ()
        {
            
            Semaphore semaphore = new Semaphore (1);
            Latch latch = new Latch ();
            Helper helper = new Helper (semaphore, latch);
            Thread thread = new Thread (new ThreadStart (helper.Go));
            semaphore.Acquire ();
            thread.Start ();
            latch.Acquire ();
            Assert.IsFalse (helper.gone);
            semaphore.Release ();
            thread.Join ();
            Assert.IsTrue (helper.gone, "not gone");
            
        }
        [Test]
        public void AquireInSameThrad()
        {
            Semaphore s = new Semaphore(2);
            Assert.AreEqual(2, s.Permits);
            s.Acquire();
            s.Acquire();
            Assert.AreEqual(0, s.Permits);
            
        }
        
        [Test]
        public void ReleaseMultipleTimesInSameThread()
        {
            Semaphore s = new Semaphore(2);
            Assert.AreEqual(2, s.Permits);
            s.Acquire();
            s.Acquire();
            s.Release(2);
            Assert.AreEqual(2, s.Permits);            
        }
        
        [Test]
        public void AttemptWithNegativeMillisInSameThread()
        {
            Semaphore s = new Semaphore(2);
            Assert.AreEqual(2, s.Permits);
            s.Acquire();
            Assert.IsTrue(s.Attempt(-400));
            s.Release(2);
            Assert.AreEqual(2, s.Permits);            
        }
        
        [Test]
        public void ReleaseMultipleBadArgument()
        {
            Semaphore s = new Semaphore(2);
            Assert.AreEqual(2, s.Permits);
            s.Acquire();
            s.Acquire();
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Release(-2));
        }
        
        [Test]
        public void AttemptInSameThread()
        {
            Semaphore s = new Semaphore(1);
            Assert.IsTrue(s.Attempt(SHORT_DELAY_MS));
            s.Release();
            Assert.IsTrue(s.Attempt(SHORT_DELAY_MS));
            s.Release();
            Assert.IsTrue(s.Attempt(SHORT_DELAY_MS));
            s.Release();
            Assert.IsTrue(s.Attempt(SHORT_DELAY_MS));
            s.Release();
            Assert.IsTrue(s.Attempt(SHORT_DELAY_MS));
            s.Release();
            Assert.AreEqual(1, s.Permits);

        }
        
        //TODO tests that interrupt a thread.
        
        [Test]
        public void AquireReleaseInSameThread()
        {
            Semaphore s = new Semaphore(1);            
            s.Acquire();
            s.Release();
            s.Acquire();
            s.Release();
            s.Acquire();
            s.Release();
            s.Acquire();
            s.Release();
            s.Acquire();
            s.Release();
            s.Acquire();
            s.Release();
            Assert.AreEqual(1, s.Permits);
        }
        
        [Test]
        public void AcquireReleaseInDifferentThreads()
        {
            Semaphore s = new Semaphore(0); 
            AcquireReleaseWorker worker1 = new AcquireReleaseWorker(s);
            Thread thread1 = new Thread(new ThreadStart(worker1.DoWork));
            
            thread1.Start();
            Thread.Sleep(300);
            s.Release();
            s.Release();
            s.Acquire();
            s.Acquire();
            s.Release();
            thread1.Join();
            
        }
        
        [Test]
        public void AttemptReleaseInDifferentThreads()
        {
            Semaphore s = new Semaphore(0); 
            AttemptReleaseWorker worker1 = new AttemptReleaseWorker(s);
            Thread thread1 = new Thread(new ThreadStart(worker1.DoWork));
            
            thread1.Start();
            //TODO investigate...
            //Assert.IsTrue(s.Attempt(SHORT_DELAY_MS));
            s.Attempt(SHORT_DELAY_MS);
            s.Release();
            //Assert.IsTrue(s.Attempt(SHORT_DELAY_MS));
            s.Attempt(SHORT_DELAY_MS);
            s.Release();
            s.Release();
            thread1.Join();
        }
        
        [Test]
        public void TimetoMillis()
        {
            long millis = Utils.CurrentTimeMillis;
            //hmm should be using UtcNow, the impl in CurrentTimeMillis doesn't use UTC.
            long epochTime = ((DateTime.Now-new DateTime (1970, 1, 1)).Ticks)/TimeSpan.TicksPerMillisecond;
            long delta = epochTime-millis;            
            Assert.IsTrue(delta < 500);
        }
    }
    
    public class AcquireReleaseWorker
    {
        private Semaphore sem;
        public AcquireReleaseWorker(Semaphore s)
        {
            sem = s;
        }
        
        public void DoWork()
        {
            sem.Acquire();
            sem.Release();
            sem.Release();
            sem.Acquire();
        }
    }
    
    public class AttemptReleaseWorker
    {
        private Semaphore sem;
        public AttemptReleaseWorker(Semaphore s)
        {
            sem = s;
        }
        
        public void DoWork()
        {
            long SHORT_DELAY_MS = 300;
            sem.Release();
            
            //TODO can we do Assert.IsTrue here?
            sem.Attempt(SHORT_DELAY_MS);
            sem.Release();
            sem.Attempt(SHORT_DELAY_MS);
        }
    }
    
}
