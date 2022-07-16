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
using Spring.Objects;

namespace Spring.Scheduling.Quartz
{
    /// <summary> 
    /// Simple implementation of the Quartz Job interface, applying the
    /// passed-in JobDataMap and also the SchedulerContext as object property
    /// values. This is appropriate because a new Job instance will be created
    /// for each execution. JobDataMap entries will override SchedulerContext
    /// entries with the same keys.
    /// </summary>
    /// <remarks>
    /// <p>
    /// For example, let's assume that the JobDataMap contains a key
    /// "myParam" with value "5": The Job implementation can then expose
    /// a object property "myParam" of type int to receive such a value,
    /// i.e. a method "setMyParam(int)". This will also work for complex
    /// types like business objects etc.
    /// </p>
    /// 
    /// <p>
    /// Note: The QuartzJobObject class itself only implements the standard
    /// Quartz IJob interface. Let your subclass explicitly implement the 
    /// Quartz IStatefulJob interface to  mark your concrete job object as stateful.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <seealso cref="IJobExecutionContext.MergedJobDataMap" />
    /// <seealso cref="IScheduler.Context" />
    /// <seealso cref="JobDetailObject.JobDataAsMap" />
    /// <seealso cref="CronTriggerObject.JobDataAsMap" />
    /// <seealso cref="SchedulerFactoryObject.SchedulerContextAsMap" />
    /// <seealso cref="SpringObjectJobFactory" />
    /// <seealso cref="SchedulerFactoryObject.JobFactory" />
    public abstract class QuartzJobObject : IJob
    {
        /// <summary> 
        /// This implementation applies the passed-in job data map as object property
        /// values, and delegates to <code>ExecuteInternal</code> afterwards.
        /// </summary>
        /// <seealso cref="ExecuteInternal" />
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                ObjectWrapper bw = new ObjectWrapper(this);
                MutablePropertyValues pvs = new MutablePropertyValues();
                pvs.AddAll(context.Scheduler.Context);
                pvs.AddAll(context.MergedJobDataMap);
                bw.SetPropertyValues(pvs, true);
            }
            catch (SchedulerException ex)
            {
                throw new JobExecutionException(ex);
            }

            return ExecuteInternal(context);
        }

        /// <summary> 
        /// Execute the actual job. The job data map will already have been
        /// applied as object property values by execute. The contract is
        /// exactly the same as for the standard Quartz execute method.
        /// </summary>
        /// <seealso cref="Execute" />
        protected abstract Task ExecuteInternal(IJobExecutionContext context);
    }
}
