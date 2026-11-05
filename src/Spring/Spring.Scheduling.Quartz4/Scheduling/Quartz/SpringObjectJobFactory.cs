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

using Quartz;
using Quartz.Extensibility;
using Spring.Objects;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Subclass of AdaptableJobFactory that also supports Spring-style
/// dependency injection on object properties. This is essentially the direct
/// equivalent of Spring's QuartzJobObject in the shape of a
/// Quartz JobFactory.
/// </summary>
/// <remarks>
/// Applies scheduler context, job data map and trigger data map entries
/// as object property values. If no matching object property is found, the entry
/// is by default simply ignored. This is analogous to QuartzJobObject's behavior.
/// <p>
/// Quartz 4's own property-setting job factory no longer merges the scheduler context into the job's properties.
/// This factory still does, so a job that received its dependencies from the scheduler context keeps getting them.
/// </p>
/// </remarks>
/// <author>Juergen Hoeller</author>
/// <seealso cref="SchedulerFactoryObject.JobFactory" />
/// <seealso cref="QuartzJobObject" />
public class SpringObjectJobFactory : AdaptableJobFactory, ISchedulerContextAware
{
    private string[] ignoredUnknownProperties;
    private SchedulerContext schedulerContext;

    /// <summary>
    /// Specify the unknown properties (not found in the object) that should be ignored.
    /// </summary>
    /// <remarks>
    /// Default is <code>null</code>, indicating that all unknown properties
    /// should be ignored. Specify an empty array to throw an exception in case
    /// of any unknown properties, or a list of property names that should be
    /// ignored if there is no corresponding property found on the particular
    /// job class (all other unknown properties will still trigger an exception).
    /// </remarks>
    public virtual string[] IgnoredUnknownProperties
    {
        set => ignoredUnknownProperties = value;
    }

    /// <summary>
    /// Set the SchedulerContext of the current Quartz Scheduler.
    /// </summary>
    /// <remarks>
    /// Quartz 4 passes the scheduler to <see cref="AdaptableJobFactory.CreateJob"/>, so this is only needed to
    /// use a context other than the scheduler's own. When it is not set, the context of the scheduler that fires the
    /// job is used.
    /// </remarks>
    /// <seealso cref="IScheduler.Context"/>
    public virtual SchedulerContext SchedulerContext
    {
        set => schedulerContext = value;
    }

    /// <summary>
    /// Create the job instance, populating it with property values taken
    /// from the scheduler context, job data map and trigger data map.
    /// </summary>
    protected override object CreateJobInstance(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        ObjectWrapper ow = new ObjectWrapper(bundle.JobDetail.JobType.Type);
        if (IsEligibleForPropertyPopulation(ow.WrappedInstance))
        {
            MutablePropertyValues pvs = new MutablePropertyValues();
            SchedulerContext context = schedulerContext ?? scheduler?.Context;
            if (context != null)
            {
                pvs.AddAll((IDictionary<string, object>) context);
            }

            pvs.AddAll((IDictionary<string, object>) bundle.JobDetail.JobDataMap);
            pvs.AddAll((IDictionary<string, object>) bundle.Trigger.JobDataMap);
            if (ignoredUnknownProperties != null)
            {
                for (int i = 0; i < ignoredUnknownProperties.Length; i++)
                {
                    string propName = ignoredUnknownProperties[i];
                    if (pvs.Contains(propName))
                    {
                        pvs.Remove(propName);
                    }
                }

                ow.SetPropertyValues(pvs);
            }
            else
            {
                ow.SetPropertyValues(pvs, true);
            }
        }

        return ow.WrappedInstance;
    }

    /// <summary>
    /// Return whether the given job object is eligible for having
    /// its object properties populated.
    /// <p>
    /// The default implementation ignores QuartzJobObject instances,
    /// which will inject object properties themselves.
    /// </p>
    /// </summary>
    /// <param name="jobObject">
    /// The job object to introspect.
    /// </param>
    /// <seealso cref="QuartzJobObject" />
    protected virtual bool IsEligibleForPropertyPopulation(object jobObject)
    {
        return (!(jobObject is QuartzJobObject));
    }
}
