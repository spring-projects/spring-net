/*
* Copyright 2002-2006 the original author or authors.
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
using System;
using System.Threading;

using Quartz;

namespace Spring.Scheduling.Quartz
{
	/// <summary> 
	/// Simple Quartz IJob adapter that delegates to a
    /// given <see cref="ThreadStart" /> instance.
    /// </summary>
    /// <remarks>
	/// Typically used in combination with property injection on the
	/// Runnable instance, receiving parameters from the Quartz JobDataMap
	/// that way instead of via the JobExecutionContext.
    /// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Marko Lahma (.NET)</author>
	/// <seealso cref="SpringObjectJobFactory" />
	/// <seealso cref="IJob.Execute(JobExecutionContext)" />
	public class DelegatingJob : IJob
	{
        private ThreadStart delegateInstance;

        /// <summary>
        /// Return the wrapped Runnable implementation.
        /// </summary>
        /// <value>The delegate.</value>
        public virtual ThreadStart Delegate
		{
			get { return delegateInstance; }
		}

		/// <summary> 
		/// Create a new DelegatingJob.
		/// </summary>
		/// <param name="delegateInstance">
		/// The Runnable implementation to delegate to.
		/// </param>
        public DelegatingJob(ThreadStart delegateInstance)
		{
			if (delegateInstance == null)
			{
				throw new ArgumentException("Delegate must not be null", "delegateInstance");
			}
			this.delegateInstance = delegateInstance;
		}


		/// <summary> 
        /// Delegates execution to the underlying ThreadStart,
		/// converting any Exception thrown to a Quartz JobExecutionException
		/// (as required by the Job contract).
		/// </summary>
		public virtual void Execute(JobExecutionContext context)
		{
            try
            {
                delegateInstance.Invoke();
            }
            catch (Exception ex)
            {
                throw new JobExecutionException(ex);
            }
		}
	}
}