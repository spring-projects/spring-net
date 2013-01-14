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

using Common.Logging;
using Quartz;
using Quartz.Spi;

namespace Spring.Scheduling.Quartz
{
	/// <summary> 
	/// Quartz ThreadPool adapter that delegates to a Spring-managed
	/// TaskExecutor instance, specified on SchedulerFactoryObject.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <seealso cref="SchedulerFactoryObject.TaskExecutor" />
	public class LocalTaskExecutorThreadPool : IThreadPool
	{
        /// <summary>
        /// Logger available to subclasses.
        /// </summary>
        private readonly ILog logger;

        private ITaskExecutor taskExecutor;

	    /// <summary>
        /// Initializes a new instance of the <see cref="LocalTaskExecutorThreadPool"/> class.
        /// </summary>
		public LocalTaskExecutorThreadPool()
		{
			logger = LogManager.GetLogger(GetType());
		}

        /// <summary>
        /// Logger instance.
        /// </summary>
	    protected ILog Logger
	    {
	        get { return logger; }
	    }

	    /// <summary>
        /// Gets the size of the pool.
        /// </summary>
        /// <value>The size of the pool.</value>
		public virtual int PoolSize
		{
			get { return - 1; }
		}

	    /// <summary>
	    /// Inform the <see cref="T:Quartz.Spi.IThreadPool"/> of the Scheduler instance's Id, 
	    ///             prior to initialize being invoked.
	    /// </summary>
	    public string InstanceId
	    {
            set { }
	    }

	    /// <summary>
	    /// Inform the <see cref="T:Quartz.Spi.IThreadPool"/> of the Scheduler instance's name, 
	    ///             prior to initialize being invoked.
	    /// </summary>
	    public string InstanceName
	    {
	        set { }
	    }

	    /// <summary>
        /// Called by the QuartzScheduler before the <see cref="T:System.Threading.ThreadPool"/> is
        /// used, in order to give the it a chance to Initialize.
        /// </summary>
		public virtual void Initialize()
		{
			// Absolutely needs thread-bound TaskExecutor to Initialize.
			taskExecutor = SchedulerFactoryObject.ConfigTimeTaskExecutor;
			if (taskExecutor == null)
			{
				throw new SchedulerConfigException("No local TaskExecutor found for configuration - " +
				                                   "'taskExecutor' property must be set on SchedulerFactoryObject");
			}
		}

        /// <summary>
        /// Called by the QuartzScheduler to inform the <see cref="T:System.Threading.ThreadPool"/>
        /// that it should free up all of it's resources because the scheduler is
        /// shutting down.
        /// </summary>
        /// <param name="waitForJobsToComplete"></param>
		public virtual void Shutdown(bool waitForJobsToComplete)
		{
		}


        /// <summary>
        /// Execute the given <see cref="T:Quartz.IThreadRunnable"/> in the next
        /// available <see cref="T:System.Threading.Thread"/>.
        /// </summary>
        /// <param name="runnable"></param>
        /// <returns></returns>
        /// <remarks>
        /// The implementation of this interface should not throw exceptions unless
        /// there is a serious problem (i.e. a serious misconfiguration). If there
        /// are no available threads, rather it should either queue the Runnable, or
        /// block until a thread is available, depending on the desired strategy.
        /// </remarks>
		public virtual bool RunInThread(IThreadRunnable runnable)
		{
			if (runnable == null)
			{
				return false;
			}
			try
			{
				taskExecutor.Execute(runnable.Run);
				return true;
			}
			catch (TaskRejectedException ex)
			{
				logger.Error("Task has been rejected by TaskExecutor", ex);
				return false;
			}
		}

        /// <summary>
        /// Determines the number of threads that are currently available in in
        /// the pool.  Useful for determining the number of times
        /// <see cref="M:Quartz.Spi.IThreadPool.RunInThread(Quartz.IThreadRunnable)"/>  can be called before returning
        /// false.
        /// </summary>
        /// <returns>
        /// the number of currently available threads
        /// </returns>
        /// <remarks>
        /// The implementation of this method should block until there is at
        /// least one available thread.
        /// </remarks>
		public virtual int BlockForAvailableThreads()
		{
			// The present implementation always returns 1, making Quartz (1.6)
			// always schedule any tasks that it feels like scheduling.
			// This could be made smarter for specific TaskExecutors,
			// for example calling <code>getMaximumPoolSize() - getActiveCount()</code>
			// on a <code>java.util.concurrent.ThreadPoolExecutor</code>.
			return 1;
		}
	}
}