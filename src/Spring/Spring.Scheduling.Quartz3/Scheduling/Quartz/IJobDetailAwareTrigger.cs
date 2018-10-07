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

namespace Spring.Scheduling.Quartz
{
    /// <summary> 
    /// Interface to be implemented by Quartz Triggers that are aware
    /// of the JobDetail object that they are associated with.
    /// </summary>
    /// <remarks>
    /// <p>
    /// SchedulerFactoryObject will auto-detect Triggers that implement this
    /// interface and register them for the respective JobDetail accordingly.
    /// </p>
    /// 
    /// <p>
    /// The alternative is to configure a Trigger for a Job name and group:
    /// This involves the need to register the JobDetail object separately
    /// with SchedulerFactoryObject.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <seealso cref="SchedulerAccessor.Triggers" />
    /// <seealso cref="SchedulerAccessor.JobDetails" />
    public interface IJobDetailAwareTrigger
    {
        /// <summary> 
        /// Return the JobDetail that this Trigger is associated with.
        /// </summary>
        /// <returns>The associated JobDetail, or <code>null</code> if none</returns>
        IJobDetail JobDetail { get; }
    }
}