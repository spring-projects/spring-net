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

using Quartz;
using Quartz.Spi;
using Quartz.Util;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// JobFactory implementation that supports <see cref="ThreadStart" />
    /// objects as well as standard Quartz <see cref="IJob" /> instances.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Marko Lahma (.NET)</author>
    /// <seealso cref="DelegatingJob" />
    /// <seealso cref="AdaptJob(object)" />
    public class AdaptableJobFactory : IJobFactory
    {
        /// <summary>
        /// Called by the scheduler at the time of the trigger firing, in order to
        /// produce a <see cref="IJob"/> instance on which to call Execute.
        /// </summary>
        /// <remarks>
        /// It should be extremely rare for this method to throw an exception -
        /// basically only the the case where there is no way at all to instantiate
        /// and prepare the Job for execution.  When the exception is thrown, the
        /// Scheduler will move all triggers associated with the Job into the
        /// <see cref="TriggerState.Error"/> state, which will require human
        /// intervention (e.g. an application restart after fixing whatever
        /// configuration problem led to the issue wih instantiating the Job.
        /// </remarks>
        /// <param name="bundle">The TriggerFiredBundle from which the <see cref="IJobDetail"/>
        /// and other info relating to the trigger firing can be obtained.</param>
        /// <param name="scheduler">The scheduler instance.</param>
        /// <returns>the newly instantiated Job</returns>
        /// <throws>SchedulerException if there is a problem instantiating the Job.</throws>
        public virtual IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                object jobObject = CreateJobInstance(bundle);
                return AdaptJob(jobObject);
            }
            catch (Exception ex)
            {
                throw new SchedulerException("Job instantiation failed", ex);
            }
        }

        /// <summary>
        /// Allows the the job factory to destroy/cleanup the job if needed.
        /// </summary>
        public virtual void ReturnJob(IJob job)
        {
        }

        /// <summary>
        /// Create an instance of the specified job class.
        /// <p>
        /// Can be overridden to post-process the job instance.
        /// </p>
        /// </summary>
        /// <param name="bundle">
        /// The TriggerFiredBundle from which the JobDetail
        /// and other info relating to the trigger firing can be obtained.
        /// </param>
        /// <returns>The job instance.</returns>
        protected virtual object CreateJobInstance(TriggerFiredBundle bundle)
        {
            return ObjectUtils.InstantiateType<object>(bundle.JobDetail.JobType);
        }

        /// <summary>
        /// Adapt the given job object to the Quartz Job interface.
        /// </summary>
        /// <remarks>
        /// The default implementation supports straight Quartz Jobs
        /// as well as Runnables, which get wrapped in a DelegatingJob.
        /// </remarks>
        /// <param name="jobObject">
        /// The original instance of the specified job class.
        /// </param>
        /// <returns>The adapted Quartz Job instance.</returns>
        /// <seealso cref="DelegatingJob" />
        protected virtual IJob AdaptJob(object jobObject)
        {
            if (jobObject is IJob job)
            {
                return job;
            }

            if (jobObject is ThreadStart start)
            {
                return new DelegatingJob(start);
            }

            if (jobObject is IThreadRunnable threadRunnable)
            {
                return new DelegatingJob(() => threadRunnable.Run().ConfigureAwait(false).GetAwaiter().GetResult());
            }

            if (jobObject is Func<Task> func)
            {
                return new DelegatingJob(() => func().ConfigureAwait(false).GetAwaiter().GetResult());
            }

            string message =
                $"Unable to execute job class [{jobObject.GetType().FullName}]: only [IJob] and [ThreadStart] supported.";
            throw new ArgumentException(message);
        }
    }
}
