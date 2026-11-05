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
using Spring.Context;
using Spring.Objects.Factory;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Job detail that eases properties based usage in a Spring container.
/// </summary>
/// <remarks>
/// <para>
/// Quartz 4 no longer has a public job detail class that can be derived from, and lets an application implement
/// <see cref="IJobDetail"/> itself. This class does, and it uses the Spring object name as job name, and the Quartz default
/// group ("DEFAULT") as job group if not specified.
/// </para>
/// <para>
/// The job type must implement <see cref="IJob"/>. Quartz 3 let this class carry any type and adapted it with
/// <see cref="DelegatingJob"/>; that is not possible any more, because Quartz 4 refuses a job type that is not an
/// <see cref="IJob"/>. To run a method of an object, or something else that is not a job, use
/// <see cref="MethodInvokingJobDetailFactoryObject"/>, or wrap the work in a <see cref="DelegatingJob"/> of your own.
/// </para>
/// <para>
/// <see cref="IJobDetail.ConcurrentExecutionDisallowed"/> and <see cref="IJobDetail.PersistJobDataAfterExecution"/> follow
/// the <see cref="DisallowConcurrentExecutionAttribute"/> and <see cref="PersistJobDataAfterExecutionAttribute"/> on the job
/// type (or on an interface that it implements).
/// </para>
/// </remarks>
/// <author>Juergen Hoeller</author>
/// <seealso cref="IJobDetail.Key" />
/// <seealso cref="JobKey.DefaultGroup" />
public class JobDetailObject : IJobDetail, IObjectNameAware, IApplicationContextAware, IInitializingObject
{
    private Type jobType;
    private JobType quartzJobType;
    private JobDataMap jobDataMap;
    private JobKey key;
    private string name;
    private string group;
    private string objectName;
    private IApplicationContext applicationContext;
    private string applicationContextJobDataKey;
    private bool? concurrentExecutionDisallowed;
    private bool? persistJobDataAfterExecution;

    /// <summary>
    /// The key of the job: the name and group that were set, or else the Spring object name in the default group.
    /// </summary>
    /// <remarks>
    /// The key is complete after <see cref="AfterPropertiesSet"/>. Setting it overrides <see cref="Name"/> and
    /// <see cref="Group"/>.
    /// </remarks>
    public virtual JobKey Key
    {
        get => key;
        set => key = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Sets the name of the job. Default is the object name.
    /// </summary>
    public virtual string Name
    {
        set => name = value;
    }

    /// <summary>
    /// Sets the group of the job. Default is <see cref="JobKey.DefaultGroup"/>.
    /// </summary>
    public virtual string Group
    {
        set => group = value;
    }

    /// <summary>
    /// A description of the job.
    /// </summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// The type of the job, which must implement <see cref="IJob"/>.
    /// </summary>
    /// <exception cref="SchedulerConfigException">if the type does not implement <see cref="IJob"/>.</exception>
    public virtual Type JobType
    {
        get => jobType;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (!typeof(IJob).IsAssignableFrom(value))
            {
                throw new SchedulerConfigException(
                    $"Job type '{value.FullName}' does not implement {typeof(IJob).FullName}. Quartz 4 accepts only IJob types: " +
                    $"implement IJob, use {nameof(MethodInvokingJobDetailFactoryObject)} to run a method of an object, or wrap " +
                    $"the work in a {nameof(DelegatingJob)}.");
            }

            jobType = value;
            quartzJobType = new JobType(value);
            concurrentExecutionDisallowed = null;
            persistJobDataAfterExecution = null;
        }
    }

    /// <summary>
    /// The data that is passed to the job, together with the job data of the trigger and the scheduler context.
    /// </summary>
    /// <remarks>
    /// Quartz 4 stores job data as JSON in a persistent store and refuses what it cannot read back.
    /// </remarks>
    public virtual JobDataMap JobDataMap
    {
        get => jobDataMap ??= new JobDataMap();
        set => jobDataMap = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Whether the job is kept in the store after no trigger refers to it. Default is <c>false</c>.
    /// </summary>
    public virtual bool Durable { get; set; }

    /// <summary>
    /// Whether the scheduler should re-run the job when it was running while the scheduler crashed or another node failed.
    /// </summary>
    public virtual bool RequestsRecovery { get; set; }

    /// <inheritdoc />
    public virtual bool ConcurrentExecutionDisallowed
    {
        get
        {
            concurrentExecutionDisallowed ??= jobType != null && IsAttributePresent(jobType, typeof(DisallowConcurrentExecutionAttribute));
            return concurrentExecutionDisallowed.Value;
        }
    }

    /// <inheritdoc />
    public virtual bool PersistJobDataAfterExecution
    {
        get
        {
            persistJobDataAfterExecution ??= jobType != null && IsAttributePresent(jobType, typeof(PersistJobDataAfterExecutionAttribute));
            return persistJobDataAfterExecution.Value;
        }
    }

    JobType IJobDetail.JobType => quartzJobType;

    /// <summary>
    /// Register objects in the JobDataMap via a given Map.
    /// </summary>
    /// <remarks>
    /// These objects will be available to this Job only,
    /// in contrast to objects in the SchedulerContext.
    /// <p>
    /// Note: When using persistent Jobs whose JobDetail will be kept in the
    /// database, do not put Spring-managed objects or an ApplicationContext
    /// reference into the JobDataMap but rather into the SchedulerContext. The persistent stores of Quartz 4 refuse
    /// job data that they cannot serialize to JSON, which is the case for such objects.
    /// </p>
    /// </remarks>
    /// <seealso cref="SchedulerFactoryObject.SchedulerContextAsMap" />
    public virtual IDictionary JobDataAsMap
    {
        set
        {
            if (value == null)
            {
                throw new ArgumentException("Value cannot be null", "value");
            }

            foreach (DictionaryEntry entry in value)
            {
                JobDataMap[(string) entry.Key] = entry.Value;
            }
        }
    }

    /// <summary>
    /// Set the name of the object in the object factory that created this object.
    /// </summary>
    /// <value>The name of the object in the factory.</value>
    /// <remarks>
    /// Invoked after population of normal object properties but before an init
    /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
    /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
    /// method or a custom init-method.
    /// </remarks>
    public virtual string ObjectName
    {
        set => objectName = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="Spring.Context.IApplicationContext"/> that this
    /// object runs in.
    /// </summary>
    /// <remarks>
    /// Invoked after population of normal object properties but before an
    /// init callback such as
    /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
    /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
    /// or a custom init-method.
    /// </remarks>
    public virtual IApplicationContext ApplicationContext
    {
        set => applicationContext = value;
        get => applicationContext;
    }

    /// <summary>
    /// Set the key of an IApplicationContext reference to expose in the JobDataMap,
    /// for example "applicationContext". Default is none.
    /// Only applicable when running in a Spring ApplicationContext.
    /// </summary>
    /// <remarks>
    /// <p>
    /// In case of a QuartzJobObject, the reference will be applied to the Job
    /// instance as object property. An "applicationContext" attribute will correspond
    /// to a "setApplicationContext" method in that scenario.
    /// </p>
    /// <p>
    /// Note that ObjectFactory callback interfaces like IApplicationContextAware
    /// are not automatically applied to Quartz Job instances, because Quartz
    /// itself is responsible for the lifecycle of its Jobs.
    /// </p>
    /// <p>
    /// <b>Note: The reference cannot be serialized, so it works with the in-memory job store only. When using persistent
    /// job stores where JobDetail contents will be kept in the database, do not put an IApplicationContext reference into
    /// the JobDataMap but rather into the SchedulerContext.</b>
    /// </p>
    /// </remarks>
    /// <seealso cref="SchedulerFactoryObject.ApplicationContextSchedulerContextKey" />
    /// <seealso cref="IApplicationContext" />
    public virtual string ApplicationContextJobDataKey
    {
        set => applicationContextJobDataKey = value;
    }

    /// <summary>
    /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
    /// after it has injected all of an object's dependencies.
    /// </summary>
    /// <exception cref="System.Exception">
    /// In the event of misconfiguration (such as the failure to set a
    /// required property) or if initialization fails.
    /// </exception>
    public virtual void AfterPropertiesSet()
    {
        if (jobType == null)
        {
            throw new SchedulerConfigException("JobDetailObject needs a JobType");
        }

        if (key == null)
        {
            string keyName = name ?? objectName;
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new SchedulerConfigException("JobDetailObject needs a Name, a Key, or a Spring object name to use as the job name");
            }

            key = new JobKey(keyName, group ?? JobKey.DefaultGroup);
        }

        if (applicationContextJobDataKey != null)
        {
            if (applicationContext == null)
            {
                throw new ArgumentException("JobDetailObject needs to be set up in an IApplicationContext " +
                                            "to be able to handle an 'applicationContextJobDataKey'");
            }

            JobDataMap[applicationContextJobDataKey] = applicationContext;
        }
    }

    /// <inheritdoc />
    public virtual IJobDetail WithJobData(JobDataMap jobDataMap)
    {
        ArgumentNullException.ThrowIfNull(jobDataMap);

        var copy = (JobDetailObject) MemberwiseClone();
        copy.jobDataMap = jobDataMap;
        return copy;
    }

    /// <inheritdoc />
    public virtual IJobDetail Clone()
    {
        var copy = (JobDetailObject) MemberwiseClone();
        if (jobDataMap != null)
        {
            copy.jobDataMap = new JobDataMap(jobDataMap);
        }

        return copy;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is JobDetailObject other && Equals(Key, other.Key) && jobType == other.jobType;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(key, jobType);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"JobDetail '{key}': jobType: '{jobType?.FullName}' persistJobDataAfterExecution: {PersistJobDataAfterExecution} " +
               $"concurrentExecutionDisallowed: {ConcurrentExecutionDisallowed} isDurable: {Durable} requestsRecovery: {RequestsRecovery}";
    }

    private static bool IsAttributePresent(Type type, Type attributeType)
    {
        if (type.GetCustomAttributes(attributeType, inherit: true).Length > 0)
        {
            return true;
        }

        foreach (Type interfaceType in type.GetInterfaces())
        {
            if (interfaceType.GetCustomAttributes(attributeType, inherit: true).Length > 0)
            {
                return true;
            }
        }

        return false;
    }
}
