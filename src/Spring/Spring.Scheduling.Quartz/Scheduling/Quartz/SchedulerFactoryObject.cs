/*
* Copyright 2002-2006 the original author or authors.
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
using System.Collections;
using System.Collections.Specialized;
using System.IO;

using Common.Logging;

using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Spi;
using Quartz.Util;
using Quartz.Xml;

using Spring.Context;
using Spring.Core.IO;
using Spring.Data.Common;
using Spring.Objects.Factory;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Scheduling.Quartz
{
    /// <summary> 
    /// FactoryObject that sets up a Quartz Scheduler and exposes it for object references.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Allows registration of JobDetails, Calendars and Triggers, automatically
    /// starting the scheduler on initialization and shutting it down on destruction.
    /// In scenarios that just require static registration of jobs at startup, there
    /// is no need to access the Scheduler instance itself in application code.
    /// </p>
    /// 
    /// <p>
    /// For dynamic registration of jobs at runtime, use a object reference to
    /// this SchedulerFactoryObject to get direct access to the Quartz Scheduler
    /// (<see cref="IScheduler" />). This allows you to create new jobs
    /// and triggers, and also to control and monitor the entire Scheduler.
    /// </p>
    /// 
    /// <p>
    /// Note that Quartz instantiates a new Job for each execution, in
    /// contrast to Timer which uses a TimerTask instance that is shared
    /// between repeated executions. Just JobDetail descriptors are shared.
    /// </p>
    /// 
    /// <p>
    /// When using persistent jobs, it is strongly recommended to perform all
    /// operations on the Scheduler within Spring-managed transactions.
    /// Else, database locking will not properly work and might even break.
    /// </p>
    /// <p>
    /// The preferred way to achieve transactional execution is to demarcate
    /// declarative transactions at the business facade level, which will
    /// automatically apply to Scheduler operations performed within those scopes.
    /// Alternatively, define a TransactionProxyFactoryObject for the Scheduler itself.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Marko Lahma (.NET)</author>
    /// <seealso cref="IScheduler" />
    /// <seealso cref="ISchedulerFactory" />
    /// <seealso cref="StdSchedulerFactory" />
    public class SchedulerFactoryObject : IFactoryObject, IApplicationContextAware, IInitializingObject, IDisposable
    {
        /// <summary>
        /// Default thread count to be set to thread pool.
        /// </summary>
        public const int DEFAULT_THREAD_COUNT = 10;
        
        /// <summary>
        /// Property name for thread count in thread pool.
        /// </summary>
        public const string PROP_THREAD_COUNT = "quartz.threadPool.threadCount";

        [ThreadStatic]
        private static IDbProvider configTimeDbProvider;

        [ThreadStatic]
        private static ITaskExecutor configTimeTaskExecutor;

        /// <summary>
        /// Return the IDbProvider for the currently configured Quartz Scheduler,
        /// to be used by LocalDataSourceJobStore.
        /// </summary>
        /// <remarks>
        /// This instance will be set before initialization of the corresponding
        /// Scheduler, and reset immediately afterwards. It is thus only available
        /// during configuration.
        /// </remarks>
        /// <seealso cref="DbProvider" />
        /// <seealso cref="LocalDataSourceJobStore" />
        public static IDbProvider ConfigTimeDbProvider
        {
            get { return configTimeDbProvider; }
        }

        /// <summary>
        /// Return the TaskExecutor for the currently configured Quartz Scheduler,
        /// to be used by LocalTaskExecutorThreadPool.
        /// </summary>
        /// <remarks>
        /// This instance will be set before initialization of the corresponding
        /// Scheduler, and reset immediately afterwards. It is thus only available
        /// during configuration.
        /// </remarks>
        public static ITaskExecutor ConfigTimeTaskExecutor
        {
            get { return configTimeTaskExecutor; }
        }


        /// <summary>
        /// Logger for this instance and its sub-class instances.
        /// </summary>
        protected ILog logger;

        private IApplicationContext applicationContext;
        private string applicationContextSchedulerContextKey;
        private bool autoStartup = true;
        private IDictionary calendars;
        private IResource configLocation;
        private IJobListener[] globalJobListeners;
        private ITriggerListener[] globalTriggerListeners;
        private ArrayList jobDetails;
        private IJobFactory jobFactory;
        private bool jobFactorySet;
        private IJobListener[] jobListeners;
        private string[] jobSchedulingDataLocations;
        private bool overwriteExistingJobs;
        private IDictionary quartzProperties;
        private IScheduler  scheduler;
        private IDictionary schedulerContextMap;
        private Type schedulerFactoryType;
        private ISchedulerListener[] schedulerListeners;
        private string schedulerName;
        private TimeSpan startupDelay = TimeSpan.Zero;
        private ITaskExecutor taskExecutor;
        private ITriggerListener[] triggerListeners;
        private ArrayList triggers;
        private bool waitForJobsToCompleteOnShutdown;
        private IDbProvider dbProvider;
        private IPlatformTransactionManager transactionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerFactoryObject"/> class.
        /// </summary>
        public SchedulerFactoryObject()
        {
            logger = LogManager.GetLogger(GetType());
            schedulerFactoryType = typeof (StdSchedulerFactory);
        }


        /// <summary>
        /// Set the Quartz SchedulerFactory implementation to use.
        /// </summary>
        /// <remarks>
        /// Default is StdSchedulerFactory, reading in the standard
        /// quartz.properties from Quartz' dll. To use custom Quartz
        /// properties, specify "configLocation" or "quartzProperties".
        /// </remarks>
        /// <value>The scheduler factory class.</value>
        /// <seealso cref="StdSchedulerFactory"/>
        /// <seealso cref="ConfigLocation"/>
        /// <seealso cref="QuartzProperties"/>
        public virtual Type SchedulerFactoryType
        {
            set
            {
                if (value == null || !typeof (ISchedulerFactory).IsAssignableFrom(value))
                {
                    throw new ArgumentException("schedulerFactoryType must implement [Quartz.ISchedulerFactory]");
                }
                schedulerFactoryType = value;
            }
        }

        /// <summary>
        /// Set the name of the Scheduler to fetch from the SchedulerFactory.
        /// If not specified, the default Scheduler will be used.
        /// </summary>
        /// <value>The name of the scheduler.</value>
        /// <seealso cref="ISchedulerFactory.GetScheduler(string)"/>
        /// <seealso cref="ISchedulerFactory.GetScheduler()"/>
        public virtual string SchedulerName
        {
            set { schedulerName = value; }
        }

        /// <summary> 
        /// Set the location of the Quartz properties config file, for example
        /// as assembly resource "assembly:quartz.properties".
        /// </summary>
        /// <remarks>
        /// Note: Can be omitted when all necessary properties are specified
        /// locally via this object, or when relying on Quartz' default configuration.
        /// </remarks>
        /// <seealso cref="QuartzProperties" />
        public virtual IResource ConfigLocation
        {
            set { configLocation = value; }
        }

        /// <summary> 
        /// Set Quartz properties, like "quartz.threadPool.type".
        /// </summary>
        /// <remarks>
        /// Can be used to override values in a Quartz properties config file,
        /// or to specify all necessary properties locally.
        /// </remarks>
        /// <seealso cref="ConfigLocation" />
        public virtual IDictionary QuartzProperties
        {
            set { quartzProperties = value; }
        }

        /// <summary>
        /// Set the Spring TaskExecutor to use as Quartz backend.
        /// Exposed as thread pool through the Quartz SPI.
        /// </summary>
        /// <remarks>
        /// By default, a Quartz SimpleThreadPool will be used, configured through
        /// the corresponding Quartz properties.
        /// </remarks>
        /// <value>The task executor.</value>
        /// <seealso cref="QuartzProperties"/>
        /// <seealso cref="LocalTaskExecutorThreadPool"/>
        public virtual ITaskExecutor TaskExecutor
        {
            set { taskExecutor = value; }
        }

        /// <summary> 
        /// Register objects in the Scheduler context via a given Map.
        /// These objects will be available to any Job that runs in this Scheduler.
        /// </summary>
        /// <remarks>
        /// Note: When using persistent Jobs whose JobDetail will be kept in the
        /// database, do not put Spring-managed object or an ApplicationContext
        /// reference into the JobDataMap but rather into the SchedulerContext.
        /// </remarks>
        /// <value>
        /// Map with string keys and any objects as
        /// values (for example Spring-managed objects)
        /// </value>
        /// <seealso cref="JobDetailObject.JobDataAsMap" />
        public virtual IDictionary SchedulerContextAsMap
        {
            set { schedulerContextMap = value; }
        }

        /// <summary>
        /// Set the key of an IApplicationContext reference to expose in the
        /// SchedulerContext, for example "applicationContext". Default is none.
        /// Only applicable when running in a Spring ApplicationContext.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note: When using persistent Jobs whose JobDetail will be kept in the
        /// database, do not put an IApplicationContext reference into the JobDataMap
        /// but rather into the SchedulerContext.
        /// </p>
        /// 	
        /// <p>
        /// In case of a QuartzJobObject, the reference will be applied to the Job
        /// instance as object property. An "applicationContext" attribute will
        /// correspond to a "setApplicationContext" method in that scenario.
        /// </p>
        /// 	
        /// <p>
        /// Note that ObjectFactory callback interfaces like IApplicationContextAware
        /// are not automatically applied to Quartz Job instances, because Quartz
        /// itself is reponsible for the lifecycle of its Jobs.
        /// </p>
        /// </remarks>
        /// <value>The application context scheduler context key.</value>
        /// <seealso cref="JobDetailObject.ApplicationContextJobDataKey"/>
        public virtual string ApplicationContextSchedulerContextKey
        {
            set { applicationContextSchedulerContextKey = value; }
        }

        /// <summary> 
        /// Set the Quartz JobFactory to use for this Scheduler.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Default is Spring's <see cref="AdaptableJobFactory" />, which supports
        /// standard Quartz <see cref="IJob" /> instances. Note that this default only applies
 	 	/// to a <i>local</i> Scheduler, not to a RemoteScheduler (where setting
 	 	/// a custom JobFactory is not supported by Quartz).
        /// </p>
        /// <p>
        /// Specify an instance of Spring's <see cref="SpringObjectJobFactory" /> here
        /// (typically as an inner object definition) to automatically populate a job's 
        /// object properties from the specified job data map and scheduler context.
        /// </p>
        /// </remarks>
        /// <seealso cref="AdaptableJobFactory" />
        /// <seealso cref="SpringObjectJobFactory" />
        public virtual IJobFactory JobFactory
        {
            set 
            { 
                jobFactory = value;
                jobFactorySet = true;
            }
        }

        /// <summary> 
        /// Set whether any jobs defined on this SchedulerFactoryObject should overwrite
        /// existing job definitions. Default is "false", to not overwrite already
        /// registered jobs that have been read in from a persistent job store.
        /// </summary>
        public virtual bool OverwriteExistingJobs
        {
            set { overwriteExistingJobs = value; }
        }

        /// <summary> 
        /// Set the location of a Quartz job definition XML file that follows the
        /// "job_scheduling_data" XSD. Can be specified to automatically
        /// register jobs that are defined in such a file, possibly in addition
        /// to jobs defined directly on this SchedulerFactoryObject.
        /// </summary>
        /// <seealso cref="ResourceJobSchedulingDataProcessor" />
        /// <seealso cref="JobSchedulingDataProcessor" />
        public virtual string JobSchedulingDataLocation
        {
            set { jobSchedulingDataLocations = new string[] {value}; }
        }

        /// <summary>
        /// Set the locations of Quartz job definition XML files that follow the
        /// "job_scheduling_data" XSD. Can be specified to automatically
        /// register jobs that are defined in such files, possibly in addition
        /// to jobs defined directly on this SchedulerFactoryObject.
        /// </summary>
        /// <value>The job scheduling data locations.</value>
        /// <seealso cref="ResourceJobSchedulingDataProcessor"/>
        /// <seealso cref="JobSchedulingDataProcessor"/>
        public virtual string[] JobSchedulingDataLocations
        {
            set { jobSchedulingDataLocations = value; }
        }

        /// <summary> 
        /// Register a list of JobDetail objects with the Scheduler that
        /// this FactoryObject creates, to be referenced by Triggers.
        /// <p>This is not necessary when a Trigger determines the JobDetail
        /// itself: In this case, the JobDetail will be implicitly registered
        /// in combination with the Trigger.</p>
        /// </summary>
        /// <seealso cref="Triggers" />
        /// <seealso cref="JobDetail" />
        /// <seealso cref="JobDetailObject" />
        /// <seealso cref="IJobDetailAwareTrigger" />
        /// <seealso cref="Trigger.JobName" />
        public virtual JobDetail[] JobDetails
        {
            set
            {
                // Use modifiable ArrayList here, to allow for further adding of
                // JobDetail objects during autodetection of JobDetailAwareTriggers.
                jobDetails = new ArrayList(value);
            }
        }

        /// <summary>
        /// Register a list of Quartz ICalendar objects with the Scheduler
        /// that this FactoryObject creates, to be referenced by Triggers.
        /// </summary>
        /// <value>The calendars.</value>
        /// <seealso cref="ICalendar"/>
        /// <seealso cref="Trigger.CalendarName"/>
        public virtual IDictionary Calendars
        {
            set { calendars = value; }
        }

        /// <summary> 
        /// Register a list of Trigger objects with the Scheduler that
        /// this FactoryObject creates.
        /// <p>
        /// If the Trigger determines the corresponding JobDetail itself,
        /// the job will be automatically registered with the Scheduler.
        /// Else, the respective JobDetail needs to be registered via the
        /// "jobDetails" property of this FactoryObject.
        /// </p>
        /// </summary>
        /// <seealso cref="JobDetails" />
        /// <seealso cref="JobDetail" />
        /// <seealso cref="IJobDetailAwareTrigger" />
        /// <seealso cref="CronTriggerObject" />
        public virtual Trigger[] Triggers
        {
            set { triggers = new ArrayList(value); }
        }

        /// <summary> 
        /// Specify Quartz SchedulerListeners to be registered with the Scheduler.
        /// </summary>
        public virtual ISchedulerListener[] SchedulerListeners
        {
            set { schedulerListeners = value; }
        }

        /// <summary> 
        /// Specify global Quartz JobListeners to be registered with the Scheduler.
        /// Such JobListeners will apply to all Jobs in the Scheduler.
        /// </summary>
        public virtual IJobListener[] GlobalJobListeners
        {
            set { globalJobListeners = value; }
        }

        /// <summary>
        /// Specify named Quartz JobListeners to be registered with the Scheduler.
        /// Such JobListeners will only apply to Jobs that explicitly activate
        /// them via their name.
        /// </summary>
        /// <value>The job listeners.</value>
        /// <seealso cref="IJobListener.Name"/>
        /// <seealso cref="JobDetail.AddJobListener"/>
        /// <seealso cref="JobDetail.JobListenerNames"/>
        public virtual IJobListener[] JobListeners
        {
            set { jobListeners = value; }
        }

        /// <summary>
        /// Specify global Quartz TriggerListeners to be registered with the Scheduler.
        /// Such TriggerListeners will apply to all Triggers in the Scheduler.
        /// </summary>
        /// <value>The global trigger listeners.</value>
        public virtual ITriggerListener[] GlobalTriggerListeners
        {
            set { globalTriggerListeners = value; }
        }

        /// <summary> 
        /// Specify named Quartz TriggerListeners to be registered with the Scheduler.
        /// Such TriggerListeners will only apply to Triggers that explicitly activate
        /// them via their name.
        /// </summary>
        /// <seealso cref="ITriggerListener.Name" />
        /// <seealso cref="Trigger.AddTriggerListener" />
        /// <seealso cref="Trigger.TriggerListenerNames" />
        /// <seealso cref="Trigger.TriggerListenerNames" />
        public virtual ITriggerListener[] TriggerListeners
        {
            set { triggerListeners = value; }
        }

        /// <summary> 
        /// Set whether to automatically start the scheduler after initialization.
        /// Default is "true"; set this to "false" to allow for manual startup.
        /// </summary>
        public virtual bool AutoStartup
        {
            set { autoStartup = value; }
        }

        /// <summary> 
        /// Set the time span to wait after initialization before
        /// starting the scheduler asynchronously. Default is 0, meaning
        /// immediate synchronous startup on initialization of this object.
        /// </summary>
        /// <remarks>
        /// Setting this to 10 or 20 seconds makes sense if no jobs
        /// should be run before the entire application has started up.
        /// </remarks>
        public virtual TimeSpan StartupDelay
        {
            set { startupDelay = value; }
        }

        /// <summary>
        /// Set whether to wait for running jobs to complete on Shutdown.
        /// Default is "false".
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [wait for jobs to complete on Shutdown]; otherwise, <c>false</c>.
        /// </value>
        /// <seealso cref="IScheduler.Shutdown(bool)"/>
        public virtual bool WaitForJobsToCompleteOnShutdown
        {
            set { waitForJobsToCompleteOnShutdown = value; }
        }

        /// <summary>
        /// Set the default DbProvider to be used by the Scheduler. If set,
        /// this will override corresponding settings in Quartz properties.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note: If this is set, the Quartz settings should not define
        ///  a job store "dataSource" to avoid meaningless double configuration.
        /// </p>
        /// <p>
        /// A Spring-specific subclass of Quartz' JobStoreSupport will be used.
        /// It is therefore strongly recommended to perform all operations on
        /// the Scheduler within Spring-managed transactions.
        /// Else, database locking will not properly work and might even break
        /// (e.g. if trying to obtain a lock on Oracle without a transaction).
        /// </p>
        /// </remarks>
        /// <seealso cref="QuartzProperties" />
        /// <seealso cref="TransactionManager" />
        /// <seealso cref="LocalDataSourceJobStore" />
        public IDbProvider DbProvider
        {
            set { dbProvider = value; }
        }

        /// <summary>
        /// Set the transaction manager to be used for registering jobs and triggers
	    /// that are defined by this SchedulerFactoryObject. Default is none; setting
    	///  this only makes sense when specifying a DataSource for the Scheduler.
        /// </summary>
        /// <seealso cref="DbProvider" />
	    public IPlatformTransactionManager TransactionManager
	    {
	        set { transactionManager = value; }
	    }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SchedulerFactoryObject"/> is running.
        /// </summary>
        /// <value><c>true</c> if running; otherwise, <c>false</c>.</value>
        public virtual bool Running
        {
            get
            {
                if (scheduler != null)
                {
                    try
                    {
                        return !scheduler.InStandbyMode;
                    }
                    catch (SchedulerException)
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        #region IApplicationContextAware Members

        /// <summary>
        /// Sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// <p>
        /// Normally this call will be used to initialize the object.
        /// </p>
        /// <p>
        /// Invoked after population of normal object properties but before an
        /// init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// or a custom init-method. Invoked after the setting of any
        /// <see cref="Spring.Context.IResourceLoaderAware"/>'s
        /// <see cref="Spring.Context.IResourceLoaderAware.ResourceLoader"/>
        /// property.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// In the case of application context initialization errors.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If thrown by any application context methods.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
        public virtual IApplicationContext ApplicationContext
        {
            set { applicationContext = value; }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Shut down the Quartz scheduler on object factory Shutdown,
        /// stopping all scheduled jobs.
        /// </summary>
        public virtual void Dispose()
        {
            logger.Info("Shutting down Quartz Scheduler");
            scheduler.Shutdown(waitForJobsToCompleteOnShutdown);
        }

        #endregion

        #region IFactoryObject Members

        /// <summary>
        /// Return an instance (possibly shared or independent) of the object
        /// managed by this factory.
        /// </summary>
        /// <returns>
        /// An instance (possibly shared or independent) of the object managed by
        /// this factory.
        /// </returns>
        /// <remarks>
        /// <note type="caution">
        /// If this method is being called in the context of an enclosing IoC container and
        /// returns <see langword="null"/>, the IoC container will consider this factory
        /// object as not being fully initialized and throw a corresponding (and most
        /// probably fatal) exception.
        /// </note>
        /// </remarks>
        public object GetObject()
        {
            return scheduler;
        }

        /// <summary>
        /// Return the <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
        /// <see langword="null"/> if not known in advance.
        /// </summary>
        /// <value></value>
        public virtual Type ObjectType
        {
            get { return (scheduler != null) ? scheduler.GetType() : typeof (IScheduler); }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        /// <value></value>
        public virtual bool IsSingleton
        {
            get { return true; }
        }

        #endregion

        //---------------------------------------------------------------------
        // Implementation of IInitializingObject interface
        //---------------------------------------------------------------------

        #region IInitializingObject Members

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// 	<p>
        /// Please do consult the class level documentation for the
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
        /// description of exactly <i>when</i> this method is invoked. In
        /// particular, it is worth noting that the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// callbacks will have been invoked <i>prior</i> to this method being
        /// called.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
        public virtual void AfterPropertiesSet()
        {
            // Create SchedulerFactory instance.
            ISchedulerFactory schedulerFactory = (ISchedulerFactory) ObjectUtils.InstantiateType(schedulerFactoryType);

            InitSchedulerFactory(schedulerFactory);
            
		    if (taskExecutor != null) 
            {
			    // Make given TaskExecutor available for SchedulerFactory configuration.
			    configTimeTaskExecutor = taskExecutor;
		    }
		    if (dbProvider != null) 
            {
			    // Make given db provider available for SchedulerFactory configuration.
			    configTimeDbProvider = dbProvider;
		    }


            try
            {
                // Get Scheduler instance from SchedulerFactory.
                scheduler = CreateScheduler(schedulerFactory, schedulerName);
                PopulateSchedulerContext();

                if (!jobFactorySet && !(scheduler is RemoteScheduler)) 
                {
 	 	            // Use AdaptableJobFactory as default for a local Scheduler, unless when
 	 	            // explicitly given a null value through the "jobFactory" bean property.
 	 	            jobFactory = new AdaptableJobFactory();
 	 	        }

                if (jobFactory != null)
                {
                    if (jobFactory is ISchedulerContextAware)
                    {
                        ((ISchedulerContextAware) jobFactory).SchedulerContext = scheduler.Context;
                    }
                    scheduler.JobFactory = jobFactory;
                }
            }
            finally
            {
			    if (taskExecutor != null) 
                {
				    configTimeTaskExecutor = null;
			    }
			    if (dbProvider != null) 
                {
				    configTimeDbProvider = null;
			    }
            }

            RegisterListeners();
            RegisterJobsAndTriggers();

            // Start Scheduler immediately, if demanded.
            if (autoStartup)
            {
                StartScheduler(scheduler, startupDelay);
            }
        }

        #endregion

        /// <summary> 
        /// Load and/or apply Quartz properties to the given SchedulerFactory.
        /// </summary>
        /// <param name="schedulerFactory">the SchedulerFactory to Initialize</param>
        private void InitSchedulerFactory(ISchedulerFactory schedulerFactory)
        {
            if (configLocation != null || quartzProperties != null || schedulerName != null ||
                taskExecutor != null || dbProvider != null)
            {
                if (!(schedulerFactory is StdSchedulerFactory))
                {
                    throw new ArgumentException("StdSchedulerFactory required for applying Quartz properties");
                }

                NameValueCollection mergedProps = new NameValueCollection();

                // Set necessary default properties here, as Quartz will not apply
                // its default configuration when explicitly given properties.
                if (taskExecutor != null)
                {
                    mergedProps[StdSchedulerFactory.PropertyThreadPoolType] =
                        typeof (LocalTaskExecutorThreadPool).AssemblyQualifiedName;
                }
                else
                {
                    mergedProps.Set(StdSchedulerFactory.PropertyThreadPoolType, typeof(SimpleThreadPool).Name);
                    mergedProps[PROP_THREAD_COUNT] = Convert.ToString(DEFAULT_THREAD_COUNT);
                }

                if (configLocation != null)
                {
                    if (logger.IsInfoEnabled)
                    {
                        logger.Info("Loading Quartz config from [" + configLocation + "]");
                    }
                    using (StreamReader sr = new StreamReader(configLocation.InputStream))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] lineItems = line.Split(new char[] { '=' }, 2);
                            if (lineItems.Length == 2)
                            {
                                mergedProps[lineItems[0].Trim()] = lineItems[1].Trim();
                            }
                        } 
                    }
                    
                }

                if (quartzProperties != null)
                {
                    // if given quartz properties, merge to them to configuration
                	MergePropertiesIntoMap(quartzProperties, mergedProps);
                }

        		if (dbProvider != null) 
                {
                    mergedProps.Add(StdSchedulerFactory.PropertyJobStoreType, typeof(LocalDataSourceJobStore).AssemblyQualifiedName);
		        }


                // Make sure to set the scheduler name as configured in the Spring configuration.
                if (schedulerName != null)
                {
                    mergedProps.Add(StdSchedulerFactory.PropertySchedulerInstanceName, schedulerName);
                }

                ((StdSchedulerFactory) schedulerFactory).Initialize(mergedProps);
            }
        }

        /// <summary>
        /// Merges the properties into map. This effectively also
        /// overwrites existing properties with same key in map.
        /// </summary>
        /// <param name="properties">The properties to merge into given map.</param>
        /// <param name="map">The map to merge to.</param>
        protected virtual void MergePropertiesIntoMap(IDictionary properties, NameValueCollection map)
        {
            foreach (string key in properties.Keys)
            {
                map[key] = (string) properties[key];
            }
        }


        /// <summary>
        /// Create the Scheduler instance for the given factory and scheduler name.
        /// Called by afterPropertiesSet.
        /// </summary>
        /// <remarks>
        /// Default implementation invokes SchedulerFactory's <code>GetScheduler</code>
        /// method. Can be overridden for custom Scheduler creation.
        /// </remarks>
        /// <param name="schedulerFactory">the factory to create the Scheduler with</param>
        /// <param name="schedName">the name of the scheduler to create</param>
        /// <returns>the Scheduler instance</returns>
        /// <seealso cref="AfterPropertiesSet"/>
        /// <seealso cref="ISchedulerFactory.GetScheduler()"/>
        protected virtual IScheduler CreateScheduler(ISchedulerFactory schedulerFactory, string schedName)
        {
            // StdSchedulerFactory's default "getScheduler" implementation
            // uses the scheduler name specified in the Quartz properties,
            // which we have set before (in "InitSchedulerFactory").
            return schedulerFactory.GetScheduler();
        }

        /// <summary>
        /// Expose the specified context attributes and/or the current
        /// IApplicationContext in the Quartz SchedulerContext.
        /// </summary>
        private void PopulateSchedulerContext()
        {
            // Put specified objects into Scheduler context.
            if (schedulerContextMap != null)
            {
                scheduler.Context.PutAll(schedulerContextMap);
            }

            // Register IApplicationContext in Scheduler context.
            if (applicationContextSchedulerContextKey != null)
            {
                if (applicationContext == null)
                {
                    throw new SystemException("SchedulerFactoryObject needs to be set up in an IApplicationContext " +
                                              "to be able to handle an 'applicationContextSchedulerContextKey'");
                }
                scheduler.Context.Put(applicationContextSchedulerContextKey, applicationContext);
            }
        }


        /// <summary> 
        /// Register all specified listeners with the Scheduler.
        /// </summary>
        private void RegisterListeners()
        {
            if (schedulerListeners != null)
            {
                for (int i = 0; i < schedulerListeners.Length; i++)
                {
                    scheduler.AddSchedulerListener(schedulerListeners[i]);
                }
            }
            if (globalJobListeners != null)
            {
                for (int i = 0; i < globalJobListeners.Length; i++)
                {
                    scheduler.AddGlobalJobListener(globalJobListeners[i]);
                }
            }
            if (jobListeners != null)
            {
                for (int i = 0; i < jobListeners.Length; i++)
                {
                    scheduler.AddJobListener(jobListeners[i]);
                }
            }
            if (globalTriggerListeners != null)
            {
                for (int i = 0; i < globalTriggerListeners.Length; i++)
                {
                    scheduler.AddGlobalTriggerListener(globalTriggerListeners[i]);
                }
            }
            if (triggerListeners != null)
            {
                for (int i = 0; i < triggerListeners.Length; i++)
                {
                    scheduler.AddTriggerListener(triggerListeners[i]);
                }
            }
        }

        /// <summary>
        /// Register jobs and triggers (within a transaction, if possible).
        /// </summary>
        private void RegisterJobsAndTriggers()
        {
		    ITransactionStatus transactionStatus = null;
		    if (transactionManager != null) {
			    transactionStatus = transactionManager.GetTransaction(new DefaultTransactionDefinition());
		    }

            try
            {
                if (jobSchedulingDataLocations != null)
                {
                    ResourceJobSchedulingDataProcessor dataProcessor = new ResourceJobSchedulingDataProcessor();
                    if (applicationContext != null)
                    {
                        dataProcessor.ResourceLoader = applicationContext;
                    }
                    for (int i = 0; i < jobSchedulingDataLocations.Length; i++)
                    {
                        dataProcessor.ProcessFileAndScheduleJobs(jobSchedulingDataLocations[i], scheduler,
                                                                 overwriteExistingJobs);
                    }
                }

                // Register JobDetails.
                if (jobDetails != null)
                {
                    foreach (JobDetail jobDetail in jobDetails)
                    {
                        AddJobToScheduler(jobDetail);
                    }
                }
                else
                {
                    // Create empty list for easier checks when registering triggers.
                    jobDetails = new ArrayList();
                }

                // Register Calendars.
                if (calendars != null)
                {
                    foreach (DictionaryEntry entry in calendars)
                    {
                        string calendarName = (string) entry.Key;
                        ICalendar calendar = (ICalendar) entry.Value;
                        scheduler.AddCalendar(calendarName, calendar, true, true);
                    }
                }

                // Register Triggers.
                if (triggers != null)
                {
                    foreach (Trigger trigger in triggers)
                    {
                        AddTriggerToScheduler(trigger);
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
					    logger.Error("Job registration exception overridden by rollback exception", ex);
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
        /// <param name="jobDetail">the job to add</param>
        /// <returns>
        /// 	<code>true</code> if the job was actually added,
        /// <code>false</code> if it already existed before
        /// </returns>
        /// <seealso cref="OverwriteExistingJobs"/>
        private bool AddJobToScheduler(JobDetail jobDetail)
        {
            if (overwriteExistingJobs || scheduler.GetJobDetail(jobDetail.Name, jobDetail.Group) == null)
            {
                scheduler.AddJob(jobDetail, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add the given trigger to the Scheduler, if it doesn't already exist.
        /// Overwrites the trigger in any case if "overwriteExistingJobs" is set.
        /// </summary>
        /// <param name="trigger">the trigger to add</param>
        /// <returns>
        /// 	<code>true</code> if the trigger was actually added,
        /// <code>false</code> if it already existed before
        /// </returns>
        /// <seealso cref="OverwriteExistingJobs"/>
        private bool AddTriggerToScheduler(Trigger trigger)
        {
            bool triggerExists = (scheduler.GetTrigger(trigger.Name, trigger.Group) != null);
            if (!triggerExists || overwriteExistingJobs)
            {
                // Check if the Trigger is aware of an associated JobDetail.
                if (trigger is IJobDetailAwareTrigger)
                {
                    JobDetail jobDetail = ((IJobDetailAwareTrigger) trigger).JobDetail;
                    // Automatically register the JobDetail too.
                    if (!jobDetails.Contains(jobDetail) && AddJobToScheduler(jobDetail))
                    {
                        jobDetails.Add(jobDetail);
                    }
                }
                if (!triggerExists)
                {
                    try
                    {
                        scheduler.ScheduleJob(trigger);
                    }
                    catch (ObjectAlreadyExistsException ex)
                    {
                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug(
                                string.Format(
                                    "Unexpectedly found existing trigger, assumably due to cluster race condition: {0} - can safely be ignored",
                                    ex.Message));
                        }
                        if (overwriteExistingJobs)
                        {
                            scheduler.RescheduleJob(trigger.Name, trigger.Group, trigger);
                        }
                    }
                }
                else
                {
                    scheduler.RescheduleJob(trigger.Name, trigger.Group, trigger);
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// Start the Quartz Scheduler, respecting the "startDelay" setting.
        /// </summary>
        /// <param name="sched">the Scheduler to start</param>
        /// <param name="startDelay">the time span to wait before starting
        /// the Scheduler asynchronously</param>
        protected virtual void StartScheduler(IScheduler sched, TimeSpan startDelay)
        {
            if (startDelay.TotalSeconds <= 0)
            {
                logger.Info("Starting Quartz Scheduler now");
                sched.Start();
            }
            else
            {
                if (logger.IsInfoEnabled)
                {
                    logger.Info(
                        string.Format("Will start Quartz Scheduler [{0}] in {1} seconds", sched.SchedulerName,
                                      startDelay));
                }
                sched.StartDelayed(startDelay);
            }
        }


        //---------------------------------------------------------------------
        // Implementation of Lifecycle interface
        //---------------------------------------------------------------------

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public virtual void Start()
        {
            if (scheduler != null)
            {
                try
                {
                    scheduler.Start();
                }
                catch (SchedulerException ex)
                {
                    throw new SchedulingException("Could not start Quartz Scheduler", ex);
                }
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public virtual void Stop()
        {
            if (scheduler != null)
            {
                try
                {
                    scheduler.Standby();
                }
                catch (SchedulerException ex)
                {
                    throw new SchedulingException("Could not stop Quartz Scheduler", ex);
                }
            }
        }

    }
}
