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

using System.Collections;
using Microsoft.Extensions.Logging;
using Quartz;
using Spring.Context;
using Spring.Core.IO;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Common base class for accessing a Quartz Scheduler, i.e. for registering jobs,
/// triggers and listeners on a <see cref="IScheduler" /> instance.
/// </summary>
/// <remarks>
/// For concrete usage, check out the <see cref="SchedulerFactoryObject" /> and
/// <see cref="SchedulerAccessorObject" /> classes.
///</remarks>
/// <author>Juergen Hoeller</author>
/// <author>Marko Lahma (.NET)</author>
public abstract class SchedulerAccessor : IResourceLoaderAware
{
    /// <summary>
    /// Logger instance.
    /// </summary>
    private readonly ILogger logger;

    private bool overwriteExistingJobs;

    private IList<IJobDetail> jobDetails;
    private IDictionary calendars;
    private IList<ITrigger> triggers;

    private ISchedulerListener[] schedulerListeners;
    private IJobListener[] globalJobListeners;
    private IJobListener[] jobListeners;
    private ITriggerListener[] globalTriggerListeners;
    private ITriggerListener[] triggerListeners;

    private IPlatformTransactionManager transactionManager;

    /// <summary>
    /// Resource loader instance for sub-classes
    /// </summary>
    private IResourceLoader resourceLoader;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerAccessor"/> class.
    /// </summary>
    protected SchedulerAccessor()
    {
        logger = LogManager.GetLogger(GetType());
    }

    /// <summary>
    /// Set whether any jobs defined on this SchedulerFactoryObject should overwrite
    /// existing job definitions. Default is "false", to not overwrite already
    /// registered jobs that have been read in from a persistent job store.
    /// </summary>
    public virtual bool OverwriteExistingJobs
    {
        set => overwriteExistingJobs = value;
    }

    /// <summary>
    /// Register a list of JobDetail objects with the Scheduler that
    /// this FactoryObject creates, to be referenced by Triggers.
    /// This is not necessary when a Trigger determines the JobDetail
    /// itself: In this case, the JobDetail will be implicitly registered
    /// in combination with the Trigger.
    /// </summary>
    /// <seealso cref="Triggers" />
    /// <seealso cref="IJobDetail" />
    /// <seealso cref="JobDetailObject" />
    /// <seealso cref="IJobDetailAwareTrigger" />
    /// <seealso cref="ITrigger.JobKey" />
    public virtual IJobDetail[] JobDetails
    {
        set => jobDetails = new List<IJobDetail>(value);
    }

    /// <summary>
    /// Register a list of Quartz ICalendar objects with the Scheduler
    /// that this FactoryObject creates, to be referenced by Triggers.
    /// </summary>
    /// <value>Map with calendar names as keys as Calendar objects as values</value>
    /// <seealso cref="ICalendar" />
    /// <seealso cref="ITrigger.CalendarName" />
    public virtual IDictionary Calendars
    {
        set => calendars = value;
    }

    /// <summary>
    /// Register a list of Trigger objects with the Scheduler that
    /// this FactoryObject creates.
    /// </summary>
    /// <remarks>
    /// If the Trigger determines the corresponding JobDetail itself,
    /// the job will be automatically registered with the Scheduler.
    /// Else, the respective JobDetail needs to be registered via the
    /// "jobDetails" property of this FactoryObject.
    /// </remarks>
    /// <seealso cref="JobDetails" />
    /// <seealso cref="IJobDetail" />
    /// <seealso cref="IJobDetailAwareTrigger" />
    /// <seealso cref="CronTriggerObject" />
    /// <seealso cref="SimpleTriggerObject" />
    public virtual ITrigger[] Triggers
    {
        set => triggers = new List<ITrigger>(value);
    }

    /// <summary>
    /// Specify Quartz SchedulerListeners to be registered with the Scheduler.
    /// </summary>
    public virtual ISchedulerListener[] SchedulerListeners
    {
        set => schedulerListeners = value;
    }

    /// <summary>
    /// Specify global Quartz JobListeners to be registered with the Scheduler.
    /// Such JobListeners will apply to all Jobs in the Scheduler.
    /// </summary>
    public virtual IJobListener[] GlobalJobListeners
    {
        set => globalJobListeners = value;
    }

    /// <summary>
    /// Specify named Quartz JobListeners to be registered with the Scheduler.
    /// Such JobListeners will only apply to Jobs that explicitly activate
    /// them via their name.
    /// </summary>
    /// <seealso cref="IJobListener.Name" />
    public virtual IJobListener[] JobListeners
    {
        set => jobListeners = value;
    }

    /// <summary>
    /// Specify global Quartz TriggerListeners to be registered with the Scheduler.
    /// Such TriggerListeners will apply to all Triggers in the Scheduler.
    /// </summary>
    public virtual ITriggerListener[] GlobalTriggerListeners
    {
        set => globalTriggerListeners = value;
    }

    /// <summary>
    /// Specify named Quartz TriggerListeners to be registered with the Scheduler.
    /// Such TriggerListeners will only apply to Triggers that explicitly activate
    /// them via their name.
    /// </summary>
    /// <seealso cref="ITriggerListener.Name" />
    public virtual ITriggerListener[] TriggerListeners
    {
        set => triggerListeners = value;
    }

    /// <summary>
    /// Set the transaction manager to be used for registering jobs and triggers
    /// that are defined by this SchedulerFactoryObject. Default is none; setting
    /// this only makes sense when specifying a DataSource for the Scheduler.
    /// </summary>
    /// <remarks>
    /// The registration runs between the begin and the commit of the transaction, on one thread. Whether the writes of the
    /// job store belong to that transaction depends on the job store joining it: the Quartz 4 job stores that this package
    /// configures do not join a Spring transaction yet (see the README).
    /// </remarks>
    public virtual IPlatformTransactionManager TransactionManager
    {
        set => transactionManager = value;
    }

    /// <summary>
    /// Sets the <see cref="Spring.Core.IO.IResourceLoader"/>
    /// that this object runs in.
    /// </summary>
    /// <value></value>
    /// <remarks>
    /// Invoked <b>after</b> population of normal objects properties but
    /// before an init callback such as
    /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
    /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet()"/>
    /// or a custom init-method. Invoked <b>before</b> setting
    /// <see cref="Spring.Context.IApplicationContextAware"/>'s
    /// <see cref="Spring.Context.IApplicationContextAware.ApplicationContext"/>
    /// property.
    /// </remarks>
    public virtual IResourceLoader ResourceLoader
    {
        set { resourceLoader = value; }
        protected get { return resourceLoader; }
    }

    /// <summary>
    /// Logger instance.
    /// </summary>
    protected ILogger Logger => logger;

    /// <summary>
    /// Register jobs and triggers (within a transaction, if possible).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This runs synchronously on the calling thread, and calls the scheduler one operation after the other, blocking for
    /// each. This is on purpose: Spring keeps the state of a transaction (and the synchronizations of the connections that
    /// take part in it) in thread-static storage on modern .NET, and an <c>await</c> of a Quartz call would resume on another
    /// thread, which would then commit a transaction it does not know about.
    /// </para>
    /// <para>
    /// The method is called while the object factory initializes the object, which is startup code.
    /// </para>
    /// </remarks>
    protected virtual void RegisterJobsAndTriggers()
    {
        ITransactionStatus transactionStatus = null;
        if (transactionManager != null)
        {
            transactionStatus = transactionManager.GetTransaction(new DefaultTransactionDefinition());
        }

        try
        {
            IScheduler scheduler = GetScheduler();

            // Register JobDetails.
            if (jobDetails != null)
            {
                foreach (IJobDetail jobDetail in jobDetails)
                {
                    AddJobToScheduler(scheduler, jobDetail);
                }
            }
            else
            {
                // Create empty list for easier checks when registering triggers.
                jobDetails = new List<IJobDetail>();
            }

            // Register Calendars.
            if (calendars != null)
            {
                foreach (DictionaryEntry entry in calendars)
                {
                    string calendarName = (string) entry.Key;
                    ICalendar calendar = (ICalendar) entry.Value;
                    Await(scheduler.AddCalendar(calendarName, calendar, AddCalendarOptions.ReplacingAndUpdatingTriggers));
                }
            }

            // Register Triggers.
            if (triggers != null)
            {
                foreach (ITrigger trigger in triggers)
                {
                    AddTriggerToScheduler(scheduler, trigger);
                }
            }
        }
        catch (Exception ex)
        {
            if (transactionStatus != null)
            {
                try
                {
                    transactionManager.Rollback(transactionStatus);
                }
                catch (TransactionException)
                {
                    logger.LogError(ex, "Job registration exception overridden by rollback exception");
                    throw;
                }
            }

            if (ex is SchedulerException)
            {
                throw;
            }

            throw new SchedulerException("Registration of jobs and triggers failed: " + ex.Message, ex);
        }

        if (transactionStatus != null)
        {
            transactionManager.Commit(transactionStatus);
        }
    }

    /// <summary>
    /// Add the given job to the Scheduler, if it doesn't already exist.
    /// Overwrites the job in any case if "overwriteExistingJobs" is set.
    /// </summary>
    /// <param name="scheduler">the scheduler</param>
    /// <param name="jobDetail">the job to add</param>
    /// <returns><code>true</code> if the job was actually added, <code>false</code> if it already existed before</returns>
    private bool AddJobToScheduler(IScheduler scheduler, IJobDetail jobDetail)
    {
        if (overwriteExistingJobs || Await(scheduler.GetJobDetail(jobDetail.Key)) == null)
        {
            Await(scheduler.AddJob(jobDetail, AddJobOptions.ReplacingAndStoringNonDurable));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add the given trigger to the Scheduler, if it doesn't already exist.
    /// Overwrites the trigger in any case if "overwriteExistingJobs" is set.
    /// </summary>
    /// <param name="scheduler">the scheduler</param>
    /// <param name="trigger">the trigger to add</param>
    /// <returns><code>true</code> if the trigger was actually added, <code>false</code> if it already existed before</returns>
    private bool AddTriggerToScheduler(IScheduler scheduler, ITrigger trigger)
    {
        bool triggerExists = Await(scheduler.GetTrigger(trigger.Key)) != null;
        if (!triggerExists || overwriteExistingJobs)
        {
            // Check if the Trigger is aware of an associated JobDetail.
            if (trigger is IJobDetailAwareTrigger awareTrigger)
            {
                IJobDetail jobDetail = awareTrigger.JobDetail;
                // Automatically register the JobDetail too.
                if (jobDetail != null
                    && !jobDetails.Contains(jobDetail)
                    && AddJobToScheduler(scheduler, jobDetail))
                {
                    jobDetails.Add(jobDetail);
                }
            }

            if (!triggerExists)
            {
                try
                {
                    Await(scheduler.ScheduleJob(trigger));
                }
                catch (ObjectAlreadyExistsException ex)
                {
                    if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    {
                        logger.LogDebug("Unexpectedly found existing trigger, assumably due to cluster race condition: {ErrorMessage} - can safely be ignored", ex.Message);
                    }

                    if (overwriteExistingJobs)
                    {
                        Await(scheduler.RescheduleJob(trigger.Key, trigger));
                    }
                }
            }
            else
            {
                Await(scheduler.RescheduleJob(trigger.Key, trigger));
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Register all specified listeners with the Scheduler.
    /// </summary>
    /// <remarks>
    /// A listener that is registered without matchers receives the events of all jobs or triggers. Quartz 4 keys listeners
    /// by their <c>Name</c>: a listener that has the name of one that is already registered replaces it.
    /// </remarks>
    protected virtual void RegisterListeners()
    {
        if (schedulerListeners != null)
        {
            for (int i = 0; i < schedulerListeners.Length; i++)
            {
                GetScheduler().ListenerManager.AddSchedulerListener(schedulerListeners[i]);
            }
        }

        if (globalJobListeners != null)
        {
            foreach (IJobListener jobListener in globalJobListeners)
            {
                GetScheduler().ListenerManager.AddJobListener(jobListener);
            }
        }

        if (jobListeners != null && jobListeners.Length > 0)
        {
            throw new InvalidOperationException("Non-global JobListeners not supported - " +
                                                "manually register a Matcher against the Quartz ListenerManager instead");
        }

        if (globalTriggerListeners != null)
        {
            foreach (ITriggerListener triggerListener in globalTriggerListeners)
            {
                GetScheduler().ListenerManager.AddTriggerListener(triggerListener);
            }
        }

        if (triggerListeners != null && triggerListeners.Length > 0)
        {
            throw new InvalidOperationException("Non-global TriggerListeners not supported - " +
                                                "manually register a Matcher against the Quartz ListenerManager instead");
        }
    }

    /// <summary>
    /// Template method that determines the Scheduler to operate on.
    /// To be implemented by subclasses.
    /// </summary>
    /// <returns></returns>
    protected abstract IScheduler GetScheduler();

    // Blocks the calling thread until the Quartz call has finished, and returns on that same thread.
    private static void Await(ValueTask task)
    {
        task.AsTask().GetAwaiter().GetResult();
    }

    private static T Await<T>(ValueTask<T> task)
    {
        return task.AsTask().GetAwaiter().GetResult();
    }
}
