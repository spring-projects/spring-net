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
using System;
using System.Threading;

using Quartz;
using Quartz.Simpl;
using Spring.Objects.Factory;

namespace Spring.Scheduling.Quartz
{
	/// <summary> 
	/// Subclass of Quartz's SimpleThreadPool that implements Spring's
	/// TaskExecutor interface and listens to Spring lifecycle callbacks.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <seealso cref="SimpleThreadPool" />
	/// <seealso cref="ITaskExecutor" />
	/// <seealso cref="SchedulerFactoryObject.TaskExecutor" />
	public class SimpleThreadPoolTaskExecutor : SimpleThreadPool, ISchedulingTaskExecutor, IInitializingObject, IDisposable
	{
		private bool waitForJobsToCompleteOnShutdown = false;

		/// <summary>
		/// Set whether to wait for running jobs to complete on Shutdown.
		/// Default is "false".
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [wait for jobs to complete on shutdown]; otherwise, <c>false</c>.
		/// </value>
		/// <seealso cref="SimpleThreadPool.Shutdown(bool)"/>
		public virtual bool WaitForJobsToCompleteOnShutdown
		{
			set { waitForJobsToCompleteOnShutdown = value; }
		}

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// 	<p>
        /// Please do consult the class level documentation for the
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
        /// description of exactly <i>when</i> this method is invoked. In
        /// particular, it is worth noting that the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// callbacks will have been invoked <i>prior</i> to this method being
        /// called.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
		public virtual void AfterPropertiesSet()
		{
			Initialize();
		}

        /// <summary>
        /// Executes the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
		public virtual void Execute(ThreadStart task)
		{
			if (task == null)
			{
				throw new ArgumentException("Runnable must not be null", "task");
			}
			if (!RunInThread(new ThreadRunnableDelegate(task)))
			{
				throw new SchedulingException("Quartz SimpleThreadPool already shut down");
			}
		}

		/// <summary> This task executor prefers short-lived work units.</summary>
		public virtual bool PrefersShortLivedTasks
		{
			get { return true; }
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
		public virtual void Dispose()
		{
			Shutdown(waitForJobsToCompleteOnShutdown);
		}

        internal class ThreadRunnableDelegate : IThreadRunnable
        {
            private ThreadStart ts;


            public ThreadRunnableDelegate(ThreadStart ts)
            {
                this.ts = ts;
            }

            public void Run()
            {
                ts.Invoke();
            }
        }
	}
}