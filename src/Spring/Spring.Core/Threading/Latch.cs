#region License
/*
* Copyright � 2002-2011 the original author or authors.
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

using System.Threading;

namespace Spring.Threading
{
    /// <summary> A latch is a boolean condition that is set at most once, ever.
    /// Once a single release is issued, all acquires will pass.
    /// <p>
    /// <b>Sample usage.</b> Here are a set of classes that use
    /// a latch as a start signal for a group of worker threads that
    /// are created and started beforehand, and then later enabled.
    /// </p>
    /// <example>
    /// class Worker implements IRunnable {
    ///   private readonly Latch startSignal;
    ///   Worker(Latch l)
    ///   {
    ///     startSignal = l;
    ///   }
    ///
    ///   public void Run() {
    ///     startSignal.acquire();
    ///     DoWork();
    ///   }
    ///
    ///   void DoWork() { ... }
    ///   }
    ///
    ///   class Driver { // ...
    ///     void Main() {
    ///       Latch go = new Latch();
    ///       for (int i = 0; i &lt; N; ++i) // make threads
    ///       new Thread(new ThreadStart(new Worker(go)).Start();
    ///       DoSomethingElse();         // don't let run yet
    ///       go.Release();              // let all threads proceed
    /// }
    /// }
    /// </example>
    /// </summary>
    /// <author>Doug Lea</author>
    /// <author>Federico Spinazzi (.Net)</author>
    public class Latch : ISync
    {
        /// <summary>
        /// can acquire ?
        /// </summary>
        protected bool latched_ = false;

        /// <summary>
        /// Method mainly used by clients who are trying to get the latch
        /// </summary>
        public void Acquire ()
        {
            lock (this)
            {
                while (!latched_)
                {
                    Monitor.Wait (this);
                }
            }
        }

        /// <summary>Wait at most msecs millisconds for a permit</summary>
        public bool Attempt (long msecs)
        {
            lock (this)
            {
                if (latched_)
                {
                    return true;
                }
                else if (msecs <= 0)
                {
                    return false;
                }
                else
                {
                    long waitTime = msecs;
                    //double start = new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds;
                    double start = Utils.CurrentTimeMillis;
                    for (;;)
                    {
                        Monitor.Wait (this, TimeSpan.FromMilliseconds(waitTime));
                        if (latched_)
                        {
                            return true;
                        }
                        else
                        {
                            waitTime = (long) (msecs - (Utils.CurrentTimeMillis - start));
                            if (waitTime <= 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enable all current and future acquires to pass
        /// </summary>
        public void Release ()
        {
            lock (this)
            {
                latched_ = true;
                Monitor.PulseAll(this);
            }
        }
    }
}
