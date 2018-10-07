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

namespace Spring.Scheduling.Quartz
{
    /// <summary> 
    /// Callback interface to be implemented by Spring-managed
    /// Quartz artifacts that need access to the SchedulerContext
    /// (without having natural access to it).
    /// </summary>
    /// <remarks>
    /// Currently only supported for custom JobFactory implementations
    /// that are passed in via Spring's SchedulerFactoryObject.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <seealso cref="IJobFactory" /> 
    /// <seealso cref="SchedulerFactoryObject.JobFactory" />
    public interface ISchedulerContextAware
    {
        /// <summary>
        /// Set the SchedulerContext of the current Quartz Scheduler.
        /// </summary>
        /// <seealso cref="IScheduler.Context" />
        SchedulerContext SchedulerContext { set; }
    }
}