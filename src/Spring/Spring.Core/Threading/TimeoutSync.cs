#region License
/*
* Copyright © 2002-2011 the original author or authors.
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
/*
Originally written by Doug Lea and released into the public domain.
This may be used for any purposes whatsoever without acknowledgment.
Thanks for the assistance and support of Sun Microsystems Labs,
and everyone contributing, testing, and using this code.
*/

namespace Spring.Threading
{
	
	/// <summary> A TimeoutSync is an adaptor class that transforms all
	/// calls to acquire to instead invoke attempt with a predetermined
	/// timeout value.
	/// </summary>
	/// <seealso cref="ISync"/>		
	public class TimeoutSync : ISync
	{
		/// <summary>
		/// the adapted sync
		/// </summary>
		protected readonly internal ISync sync_; 
        /// <summary>
        /// timeout value
        /// </summary>
		protected readonly internal long timeout_; 
		
		/// <summary> Create a TimeoutSync using the given Sync object, and
		/// using the given timeout value for all calls to acquire.
		/// </summary>
		public TimeoutSync(ISync sync, long timeout)
		{
			sync_ = sync;
			timeout_ = timeout;
		}
		
        /// <summary>
        /// Try to acquire the sync before the timeout
        /// </summary>
        /// <exception cref="TimeoutException">In case a time out occurred</exception>
		public virtual void  Acquire()
		{
			if (!sync_.Attempt(timeout_))
				throw new TimeoutException(timeout_);
		}

		/// <summary>
		/// <see cref="ISync.Attempt"/>
		/// </summary>
		public virtual bool Attempt(long msecs)
		{
			return sync_.Attempt(msecs);
		}
		
        /// <summary>
        /// <see cref="ISync.Release"/>
        /// </summary>
		public virtual void  Release()
		{
			sync_.Release();
		}
	}
}