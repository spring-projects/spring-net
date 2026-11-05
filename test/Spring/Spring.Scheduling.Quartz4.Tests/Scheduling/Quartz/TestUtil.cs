/*
 * Copyright 2002-2026 the original author or authors.
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

using FakeItEasy;
using Quartz;
using Quartz.Core;
using Quartz.Extensibility;

namespace Spring.Scheduling.Quartz;

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
        IJobDetail jd = JobBuilder.Create().OfType(jobType).WithIdentity("jobName", "jobGroup").Build();
        return CreateFiredBundle(jd, trigger);
    }

    /// <summary>
    /// Creates a fired bundle for the given job detail and trigger.
    /// </summary>
    public static TriggerFiredBundle CreateFiredBundle(IJobDetail jobDetail, IOperableTrigger trigger)
    {
        return new TriggerFiredBundle
        {
            JobDetail = jobDetail,
            Trigger = trigger,
            Calendar = null,
            Recovering = false,
            FireTimeUtc = DateTimeOffset.UtcNow,
            ScheduledFireTimeUtc = null,
            PreviousFireTimeUtc = null,
            NextFireTimeUtc = null
        };
    }

    /// <summary>
    /// Creates a job execution context whose scheduler has an (empty) scheduler context and whose result can be set.
    /// </summary>
    public static IJobExecutionContext CreateMinimalJobExecutionContext()
    {
        IScheduler scheduler = A.Fake<IScheduler>();
        A.CallTo(() => scheduler.Context).Returns(new SchedulerContext());
        IJobExecutionContext context = A.Fake<IJobExecutionContext>();
        A.CallTo(() => context.Scheduler).Returns(scheduler);
        A.CallTo(() => context.MergedJobDataMap).Returns(new JobDataMap());
        return context;
    }
}
