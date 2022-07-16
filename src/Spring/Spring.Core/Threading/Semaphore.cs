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
	/// <summary>
    /// <p>Base class for counting semaphores based on Semaphore implementation
    /// from Doug Lea.</p>
    /// </summary>
    /// <remarks>
    ///
    /// <p>Conceptually, a semaphore
    /// maintains a set of permits. Each acquire() blocks if
    /// necessary until a permit is available, and then takes it.</p>
    ///
    /// <p>Each release adds a permit. However, no actual permit objects are used;
    /// the Semaphore just keeps a count of the number available
    /// and acts accordingly.</p>
    ///
    /// <p>A semaphore initialized to 1 can serve as a mutual exclusion lock. </p>
    ///
	/// Used for implementation of a <see cref="Spring.Pool.Support.SimplePool"/>
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Federico Spinazzi (.Net)</author>
    public class Semaphore : ISync
	{
        /// <summary>
        /// current number of available permits
        /// </summary>
		protected long nPermits;

        /// <summary>
        /// <p>Create a Semaphore with the given initial number of permits.</p>
        /// <p>Using a seed of 1 makes the semaphore act as a mutual
        /// exclusion lock.</p>
        ///
        /// <p>Negative seeds are also allowed,
        /// in which case no acquires will proceed until the number of
        /// releases has pushed the number of permits past 0.</p>
        /// </summary>
		public Semaphore(long initialPermits)
		{
            nPermits = initialPermits;
		}

        /// <summary>
        /// Release a permit
        /// </summary>
		public virtual void Release ()
		{
			lock (this)
			{
				++nPermits;
			    Monitor.Pulse(this);
			}
		}

        /// <summary>
        /// Acquire a permit
        /// </summary>
		public virtual void Acquire ()
		{
			lock (this)
			{
				try
				{
					while (nPermits <= 0)
						Monitor.Wait(this);
					--nPermits;
				}
				catch (ThreadInterruptedException)
				{
					Monitor.Pulse(this);
					throw;
				}
			}
		}

        /// <summary>
        /// Wait at most msecs millisconds for a permit
        /// </summary>
        /// <param name="msecs">number of ms to wait</param>
        /// <returns>true if aquired</returns>
        public virtual bool Attempt(long msecs)
        {
            lock (this)
            {
                if (nPermits > 0)
                {
                    --nPermits;
                    return true;
                }
                else if (msecs <= 0)
                    return false;
                else
                {
                    try
                    {
                        double startTime = Utils.CurrentTimeMillis;
                        long waitTime = msecs;

                        for (; ; )
                        {
                            Monitor.Wait(this, TimeSpan.FromMilliseconds(waitTime));
                            if (nPermits > 0)
                            {
                                --nPermits;
                                return true;
                            }
                            else
                            {
                                waitTime = (long) (msecs - (Utils.CurrentTimeMillis - startTime));
                                if (waitTime <= 0)
                                    return false;
                            }
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        Monitor.Pulse(this);
                        throw;
                    }
                }
            }
        }

        /// <summary> Release N permits. <code>release(n)</code> is
        /// equivalent in effect to:
        /// <pre>
        /// for (int i = 0; i &lt; n; ++i) release();
        /// </pre>
        /// But may be more efficient in some semaphore implementations.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">  if n is negative.
        ///
        /// </exception>
        public virtual void Release(long n)
        {
            lock (this)
            {
                if (n < 0)
                    throw new ArgumentOutOfRangeException("n", n, "Negative argument");

                nPermits += n;
                for (long i = 0; i < n; ++i)
                    Monitor.Pulse(this);
            }
        }

        /// <summary> Return the current number of available permits.
        /// Returns an accurate, but possibly unstable value,
        /// that may change immediately after returning.
        /// </summary>
        public virtual long Permits
	    {
	        get
	        {
	            lock (this)
	            {
	                return nPermits;
	            }
	        }
	    }


	}
}
