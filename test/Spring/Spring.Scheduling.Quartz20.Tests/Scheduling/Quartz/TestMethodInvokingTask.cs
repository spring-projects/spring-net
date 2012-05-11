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

using System.Threading;

namespace Spring.Scheduling.Quartz
{

    /// <summary>
    /// Simple test task.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    public class TestMethodInvokingTask
    {
        /// <summary>
        /// Counter for DoSomething and DoWait calls.
        /// </summary>
        public int counter;
        private readonly object lockObject = new object();

        /// <summary>
        /// Simple test method.
        /// </summary>
        public void DoSomething()
        {
            counter++;
        }

        /// <summary>
        /// Waits until stop is called.
        /// </summary>
        public void DoWait()
        {
            counter++;
            // wait until stop is called
            lock (lockObject)
            {
                try
                {
                    Monitor.Wait(lockObject);
                }
                catch (ThreadInterruptedException)
                {
                    // fall through
                }
            }
        }

        /// <summary>
        /// Informs test object that stop should be called.
        /// </summary>
        public void Stop()
        {
            lock (lockObject)
            {
                Monitor.Pulse(lockObject);
            }
        }

    }
}