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

using System.Collections;
using Quartz;
using Quartz.Impl.Triggers;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Convenience subclass of Quartz's <see cref="ISimpleTrigger" />
/// class, making properties based usage easier.
/// </summary>
/// <remarks>
/// <p>
/// SimpleTrigger itself is already a PONO but lacks sensible defaults.
/// This class uses the Spring object name as trigger name, the Quartz default group
/// ("DEFAULT") as trigger group, the current time as start time, and indefinite
/// repetition, if not specified.
/// </p>
///
/// <p>
/// This class will also register the trigger with the key of
/// a given <see cref="JobDetail" />. This allows <see cref="SchedulerFactoryObject" />
/// to automatically register a trigger for the corresponding JobDetail,
/// instead of registering the JobDetail separately.
/// </p>
/// <p>
/// This class follows the API of Quartz 4. The trigger is identified by <see cref="TriggerBase.Key"/> (the 3.x name and
/// group properties are gone), the job it fires by <see cref="TriggerBase.JobKey"/> or <see cref="JobDetail"/>, and the
/// misfire instruction is set with the typed <see cref="MisfireInstruction"/>.
/// </p>
/// <p>
/// Quartz 4 treats <see cref="TriggerBase.EndTimeUtc"/> as inclusive: a trigger whose end time is exactly one of its fire
/// times fires once more than it did in Quartz 3.
/// </p>
/// </remarks>
/// <author>Juergen Hoeller</author>
/// <seealso cref="ITrigger.Key" />
/// <seealso cref="ITrigger.StartTimeUtc" />
/// <seealso cref="ITrigger.JobKey" />
/// <seealso cref="SimpleTriggerObject.JobDetail" />
/// <seealso cref="SchedulerAccessor.Triggers" />
/// <seealso cref="SchedulerAccessor.JobDetails" />
/// <seealso cref="CronTriggerObject" />
public class SimpleTriggerObject : SimpleTriggerImpl, IJobDetailAwareTrigger, IObjectNameAware, IInitializingObject
{
    private TimeSpan startDelay = TimeSpan.Zero;
    private IJobDetail jobDetail;
    private string objectName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleTriggerObject"/> class.
    /// </summary>
    public SimpleTriggerObject()
    {
        RepeatCount = RepeatIndefinitely;
    }

    /// <summary>
    /// Register objects in the JobDataMap via a given Map.
    /// <p>
    /// These objects will be available to this Trigger only,
    /// in contrast to objects in the JobDetail's data map.
    /// </p>
    /// </summary>
    /// <seealso cref="JobDetailObject.JobDataAsMap" />
    public virtual IDictionary JobDataAsMap
    {
        set
        {
            foreach (DictionaryEntry entry in value)
            {
                JobDataMap[(string) entry.Key] = entry.Value;
            }
        }
    }

    /// <summary>
    /// The misfire instruction, set by the name of the <see cref="SimpleTriggerMisfireInstruction"/> member (for example
    /// <c>FireNow</c> or <c>NextWithRemainingCount</c>).
    /// Default is <see cref="SimpleTriggerMisfireInstruction.SmartPolicy" />.
    /// </summary>
    /// <remarks>
    /// This hides the read-only property of the base class and writes <see cref="TriggerBase.MisfireInstructionCode"/>.
    /// </remarks>
    public new virtual SimpleTriggerMisfireInstruction MisfireInstruction
    {
        get => base.MisfireInstruction;
        set => MisfireInstructionCode = (int) value;
    }

    /// <summary>
    /// Set the delay before starting the job for the first time.
    /// The given time span will be added to the current
    /// time to calculate the start time. Default is <see cref="TimeSpan.Zero" />.
    /// </summary>
    /// <remarks>
    /// This delay will just be applied if no custom start time was
    /// specified. However, in typical usage within a Spring context,
    /// the start time will be the container startup time anyway.
    /// Specifying a relative delay is appropriate in that case.
    /// </remarks>
    /// <seealso cref="ITrigger.StartTimeUtc" />
    public virtual TimeSpan StartDelay
    {
        set
        {
            AssertUtils.State(value >= TimeSpan.Zero, "Start delay cannot be negative.");
            startDelay = value;
        }
        get => startDelay;
    }

    /// <summary>
    /// Set the name of the object in the object factory that created this object.
    /// </summary>
    /// <value>The name of the object in the factory.</value>
    /// <remarks>
    /// <p>
    /// Invoked after population of normal object properties but before an init
    /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
    /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
    /// method or a custom init-method.
    /// </p>
    /// </remarks>
    public virtual string ObjectName
    {
        set => objectName = value;
    }

    /// <summary>
    /// Set the JobDetail that this trigger should be associated with.
    /// <p>
    /// This is typically used with a object reference if the JobDetail
    /// is a Spring-managed object. Alternatively, the trigger can also
    /// be associated with a job by its <see cref="TriggerBase.JobKey"/>.
    /// </p>
    /// </summary>
    public virtual IJobDetail JobDetail
    {
        get => jobDetail;
        set => jobDetail = value;
    }

    /// <summary>
    /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
    /// after it has injected all of an object's dependencies.
    /// </summary>
    /// <remarks>
    /// The trigger key defaults to the Spring object name in <see cref="TriggerKey.DefaultGroup"/> unless
    /// <see cref="TriggerBase.Key"/> was set, and the job key comes from <see cref="JobDetail"/> if that is set.
    /// </remarks>
    /// <exception cref="System.Exception">
    /// In the event of misconfiguration (such as the failure to set a
    /// required property) or if initialization fails.
    /// </exception>
    public virtual void AfterPropertiesSet()
    {
        if (StartTimeUtc == DateTimeOffset.MinValue)
        {
            StartTimeUtc = DateTimeOffset.UtcNow;
        }

        if (StartDelay > TimeSpan.Zero)
        {
            StartTimeUtc = DateTime.UtcNow.Add(startDelay);
        }

        if (Key == null && objectName != null)
        {
            Key = new TriggerKey(objectName, TriggerKey.DefaultGroup);
        }

        if (jobDetail != null)
        {
            JobKey = jobDetail.Key;
        }
    }
}
