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

using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Quartz.NET integration testing helpers.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    public class TestUtil
    {
        /// <summary>
        /// Creates the minimal fired bundle with job detail that has
        /// given job type.
        /// </summary>
        /// <param name="jobType">Type of the job.</param>
        /// <returns>Minimal TriggerFiredBundle</returns>
        public static TriggerFiredBundle CreateMinimalFiredBundleWithTypedJobDetail(Type jobType)
        {
            return CreateMinimalFiredBundleWithTypedJobDetail(jobType, null);
        }

        /// <summary>
        /// Creates the minimal fired bundle with job detail that has
        /// given job type.
        /// </summary>
        /// <param name="jobType">Type of the job.</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>Minimal TriggerFiredBundle</returns>
        public static TriggerFiredBundle CreateMinimalFiredBundleWithTypedJobDetail(Type jobType, IOperableTrigger trigger)
        {
            IJobDetail jd = new JobDetailImpl("jobName", "jobGroup", jobType);
            TriggerFiredBundle bundle = new TriggerFiredBundle(jd, trigger, null, false, DateTimeOffset.UtcNow, null, null, null);
            return bundle;
        }
    }
}
