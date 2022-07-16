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
        private ITaskExecutor taskExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalTaskExecutorThreadPool"/> class.
        /// </summary>
        public LocalTaskExecutorThreadPool()
        {
            Logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Logger instance.
        /// </summary>
        protected ILog Logger { get; }

        /// <inheritdoc />
        public virtual int PoolSize => -1;

        /// <inheritdoc />
        public string InstanceId
        {
            set { }
        }

        /// <inheritdoc />
        public string InstanceName
        {
            set { }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual void Shutdown(bool waitForJobsToComplete)
        {
        }

        /// <inheritdoc />
        public virtual bool RunInThread(Func<Task> runnable)
        {
            if (runnable == null)
            {
                return false;
            }

            try
            {
                taskExecutor.Execute(() => runnable().ConfigureAwait(false).GetAwaiter().GetResult());
                return true;
            }
            catch (TaskRejectedException ex)
            {
                Logger.Error("Task has been rejected by TaskExecutor", ex);
                return false;
            }
        }

        /// <inheritdoc />
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
