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
/// Convenience subclass of Quartz's CronTrigger type, making property based
/// usage easier.
/// </summary>
/// <remarks>
/// <p>
/// CronTrigger itself is already property based but lacks sensible defaults.
/// This class uses the Spring object name as trigger name, the Quartz default group
/// ("DEFAULT") as trigger group, the current time as start time, and indefinite
/// repetition, if not specified.
/// </p>
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
/// </remarks>
/// <author>Juergen Hoeller</author>
/// <seealso cref="ITrigger.Key" />
/// <seealso cref="ITrigger.StartTimeUtc" />
/// <seealso cref="ITrigger.JobKey" />
/// <seealso cref="JobDetail" />
/// <seealso cref="SchedulerAccessor.Triggers" />
/// <seealso cref="SchedulerAccessor.JobDetails" />
public class CronTriggerObject : CronTriggerImpl, IJobDetailAwareTrigger, IObjectNameAware, IInitializingObject
{
    private IJobDetail jobDetail;
    private string objectName;
    private TimeSpan startDelay;

    /// <summary>
    /// Initializes a new instance of the <see cref="CronTriggerObject"/> class.
    /// </summary>
    public CronTriggerObject()
    {
    }

    /// <summary>
    /// Register objects in the JobDataMap via a given Map.
    /// </summary>
    /// <remarks>
    /// These objects will be available to this Trigger only,
    /// in contrast to objects in the JobDetail's data map.
    /// </remarks>
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
    /// The misfire instruction, set by the name of the <see cref="CronTriggerMisfireInstruction"/> member (for example
    /// <c>DoNothing</c> or <c>FireAndProceed</c>).
    /// Default is <see cref="CronTriggerMisfireInstruction.SmartPolicy" />.
    /// </summary>
    /// <remarks>
    /// This hides the read-only property of the base class and writes <see cref="TriggerBase.MisfireInstructionCode"/>.
    /// </remarks>
    public new virtual CronTriggerMisfireInstruction MisfireInstruction
    {
        get => base.MisfireInstruction;
        set => MisfireInstructionCode = (int) value;
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
    /// </summary>
    /// <remarks>
    /// This is typically used with a object reference if the JobDetail
    /// is a Spring-managed object. Alternatively, the trigger can also
    /// be associated with a job by its <see cref="TriggerBase.JobKey"/>.
    /// </remarks>
    /// <seealso cref="ITrigger.JobKey" />
    public virtual IJobDetail JobDetail
    {
        get => jobDetail;
        set => jobDetail = value;
    }

    /// <summary>
    /// Set the start delay as <see cref="TimeSpan" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The start delay is added to the current system UTC time
    /// (when the object starts) to control the <see cref="ITrigger.StartTimeUtc" />
    /// of the trigger.
    /// </para>
    /// <para>
    /// If the start delay is non-zero it will <strong>always</strong>
    /// take precedence over start time.
    /// </para>
    /// </remarks>
    /// <value>the start delay, as <see cref="TimeSpan" /> object.</value>
    public TimeSpan StartDelay
    {
        set
        {
            AssertUtils.State(value >= TimeSpan.Zero, "Start delay cannot be negative.");
            startDelay = value;
        }
        get => startDelay;
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
