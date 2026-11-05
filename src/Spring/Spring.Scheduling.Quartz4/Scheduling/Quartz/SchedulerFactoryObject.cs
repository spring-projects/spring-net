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
using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Configuration;
using Quartz.Extensibility;
using Spring.Context;
using Spring.Context.Events;
using Spring.Core.IO;
using Spring.Core.TypeConversion;
using Spring.Data.Common;
using Spring.Objects.Factory;

namespace Spring.Scheduling.Quartz;

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
/// <h4>How the scheduler is configured</h4>
/// <p>
/// The scheduler is built with <see cref="QuartzSchedulerBuilder"/>. The sources of configuration apply in this order, and
/// a later one wins over an earlier one:
/// </p>
/// <list type="number">
/// <item><description><see cref="ConfigLocation"/> and <see cref="QuartzProperties"/>: the flat <c>quartz.*</c> keys.
/// Quartz refuses a key that it does not know.</description></item>
/// <item><description>The typed properties of this class (<see cref="SchedulerName"/>, <see cref="MaxConcurrency"/>,
/// <see cref="MisfireThreshold"/> and the others), each of which applies only when it is set, and the components that
/// Spring supplies: <see cref="TaskExecutor"/>, <see cref="JobFactory"/> and <see cref="DbProvider"/>.</description></item>
/// <item><description>The <see cref="ConfigureQuartz"/> callback, which receives the <see cref="IQuartzBuilder"/> last.
/// It reaches everything that Quartz offers (plugins, middleware, serializers, the Quartz.Plugins XML
/// configuration and so on).</description></item>
/// </list>
/// <p>
/// Quartz decides a component that is registered twice in favour of the first registration. A component that Spring
/// registers because of <see cref="TaskExecutor"/>, <see cref="JobFactory"/> or <see cref="DbProvider"/> is therefore not
/// replaced by one that the callback registers: do not set both.
/// </p>
///
/// <h4>Transactions</h4>
/// <p>
/// With a persistent store, Quartz 3 took part in Spring transactions through <c>LocalDataSourceJobStore</c>. The job
/// stores of Quartz 4 cannot be subclassed, so that is not available yet (see the README). The store that
/// <see cref="DbProvider"/> configures accepts a transaction that the application hands to the scheduler, which is the
/// hook that Spring's transaction support will use.
/// </p>
/// </remarks>
/// <author>Juergen Hoeller</author>
/// <author>Marko Lahma (.NET)</author>
/// <seealso cref="IScheduler" />
/// <seealso cref="ISchedulerFactory" />
public class SchedulerFactoryObject : SchedulerAccessor, IFactoryObject, IObjectNameAware,
    IApplicationContextAware, IApplicationEventListener, IInitializingObject, IDisposable
{
    private IApplicationContext applicationContext;
    private string applicationContextSchedulerContextKey;
    private bool autoStartup = true;
    private IResource configLocation;
    private IJobFactory jobFactory;
    private bool jobFactorySet;
    private IDictionary quartzProperties;
    private IScheduler scheduler;
    private StandaloneSchedulerFactory schedulerFactory;
    private IDictionary schedulerContextMap;
    private string schedulerName;
    private TimeSpan startupDelay = TimeSpan.Zero;
    private ITaskExecutor taskExecutor;
    private bool exposeSchedulerInRepository;
    private bool exposed;
    private bool waitForJobsToCompleteOnShutdown;
    private Spring.Data.Common.IDbProvider dbProvider;
    private string quartzDbProvider;
    private Action<IQuartzBuilder> configureQuartz;

    static SchedulerFactoryObject()
    {
        // The typed options with a time span (IdleWaitTime, MisfireThreshold, ClusterCheckinInterval) are nullable, and
        // .NET's converter for a nullable time span knows only "00:00:05", not Spring's "5s". Unless the application has
        // registered a converter of its own, the Spring one is used for them, as it is for TimeSpan.
        if (TypeConverterRegistry.GetConverter(typeof(TimeSpan?)) is System.ComponentModel.NullableConverter)
        {
            TypeConverterRegistry.RegisterConverter(typeof(TimeSpan?), new TimeSpanConverter());
        }
    }

    /// <summary>
    /// Set the name of the scheduler (Quartz's <c>InstanceName</c>). Default is the object name of this factory
    /// object, and failing that Quartz's default, "QuartzScheduler".
    /// </summary>
    /// <value>The name of the scheduler.</value>
    public virtual string SchedulerName
    {
        get => schedulerName;
        set => schedulerName = value;
    }

    /// <summary>
    /// Set the instance id of the scheduler. Quartz's default is "NON_CLUSTERED".
    /// </summary>
    public virtual string InstanceId { get; set; }

    /// <summary>
    /// Set whether Quartz generates the instance id of the scheduler.
    /// </summary>
    public virtual bool? GenerateInstanceId { get; set; }

    /// <summary>
    /// Set the number of jobs that the scheduler runs at the same time (the size of Quartz's default thread pool).
    /// Quartz's default is 10.
    /// </summary>
    /// <remarks>
    /// If a <see cref="TaskExecutor"/> is set, the value is the pool size that <see cref="LocalTaskExecutorThreadPool"/>
    /// reports to Quartz.
    /// </remarks>
    public virtual int? MaxConcurrency { get; set; }

    /// <summary>
    /// Set the time after which a trigger counts as misfired. Quartz's default is 5 seconds for the in-memory store and
    /// 1 minute for the persistent store.
    /// </summary>
    public virtual TimeSpan? MisfireThreshold { get; set; }

    /// <summary>
    /// Set the prefix of the names of the tables of the persistent store. Quartz's default is <c>QRTZ_</c>.
    /// </summary>
    public virtual string TablePrefix { get; set; }

    /// <summary>
    /// Set whether the scheduler runs in a cluster. Needs a <see cref="DbProvider"/>.
    /// </summary>
    public virtual bool? Clustered { get; set; }

    /// <summary>
    /// Set how often a clustered scheduler checks in with the database.
    /// Quartz's default is 7.5 seconds. Needs <see cref="Clustered"/>.
    /// </summary>
    public virtual TimeSpan? ClusterCheckinInterval { get; set; }

    /// <summary>
    /// Set how long the scheduler waits before it looks for triggers again when there is nothing to fire.
    /// Quartz's default is 30 seconds.
    /// </summary>
    public virtual TimeSpan? IdleWaitTime { get; set; }

    /// <summary>
    /// Set the largest number of triggers that the scheduler acquires at once.
    /// </summary>
    public virtual int? MaxBatchSize { get; set; }

    /// <summary>
    /// Set the time zone that triggers use when they do not have one of their own.
    /// </summary>
    /// <remarks>
    /// In an XML object definition, use <c>System.TimeZoneInfo</c> with
    /// <c>factory-method="FindSystemTimeZoneById"</c>.
    /// </remarks>
    public virtual TimeZoneInfo DefaultTimeZone { get; set; }

    /// <summary>
    /// Set the location of a Quartz properties config file (<c>key = value</c> lines), for example
    /// as assembly resource "assembly:quartz.properties".
    /// </summary>
    /// <remarks>
    /// Note: Can be omitted when all necessary properties are specified
    /// locally via this object, or when relying on Quartz' default configuration.
    /// </remarks>
    /// <seealso cref="QuartzProperties" />
    public virtual IResource ConfigLocation
    {
        set => configLocation = value;
    }

    /// <summary>
    /// Set Quartz properties, like "quartz.scheduler.idleWaitTime".
    /// </summary>
    /// <remarks>
    /// Can be used to override values in a Quartz properties config file,
    /// or to specify all necessary properties locally. The typed properties of this class and
    /// <see cref="ConfigureQuartz"/> win over them. Quartz throws on a key that it does not know.
    /// </remarks>
    /// <seealso cref="ConfigLocation" />
    public virtual IDictionary QuartzProperties
    {
        set => quartzProperties = value;
    }

    /// <summary>
    /// Set the Spring TaskExecutor to use as Quartz backend.
    /// Exposed as thread pool through the Quartz extensibility interfaces.
    /// </summary>
    /// <remarks>
    /// By default, Quartz's default thread pool is used (see <see cref="MaxConcurrency"/>).
    /// </remarks>
    /// <value>The task executor.</value>
    /// <seealso cref="LocalTaskExecutorThreadPool"/>
    public virtual ITaskExecutor TaskExecutor
    {
        set => taskExecutor = value;
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
        set => schedulerContextMap = value;
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
    /// <p>
    /// In case of a QuartzJobObject, the reference will be applied to the Job
    /// instance as object property. An "applicationContext" attribute will
    /// correspond to a "setApplicationContext" method in that scenario.
    /// </p>
    /// <p>
    /// Note that ObjectFactory callback interfaces like IApplicationContextAware
    /// are not automatically applied to Quartz Job instances, because Quartz
    /// itself is responsible for the lifecycle of its Jobs.
    /// </p>
    /// </remarks>
    /// <seealso cref="JobDetailObject.ApplicationContextJobDataKey" />
    /// <seealso cref="IApplicationContext" />
    public virtual string ApplicationContextSchedulerContextKey
    {
        set => applicationContextSchedulerContextKey = value;
    }

    /// <summary>
    /// Set the Quartz JobFactory to use for this Scheduler.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Default is Spring's <see cref="AdaptableJobFactory" />, which supports
    /// standard Quartz Jobs and Runnables. Specify an instance of Spring's
    /// <see cref="SpringObjectJobFactory" /> (or a subclass) to
    /// get object property population via the scheduler context, job data map and trigger data map.
    /// </p>
    /// <p>
    /// Specify an explicit null value if you want to use Quartz's own default job factory.
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
    /// Set whether to expose the Spring-managed Scheduler instance in the process-wide table that
    /// <see cref="SchedulerAccessorObject.SchedulerName"/> looks schedulers up in. Default is "false".
    /// </summary>
    /// <remarks>
    /// <p>
    /// Quartz 4 gives each scheduler builder a repository of its own, so this is not Quartz's repository. Exposing a
    /// scheduler makes it findable by name, and makes the name unavailable to another Spring-managed scheduler: creating a
    /// second one with the same name throws.
    /// </p>
    /// </remarks>
    public virtual bool ExposeSchedulerInRepository
    {
        set => exposeSchedulerInRepository = value;
    }

    /// <summary>
    /// Set whether to automatically start the scheduler after initialization.
    /// Default is "true"; set this to "false" to allow for manual startup.
    /// </summary>
    public virtual bool AutoStartup
    {
        set => autoStartup = value;
    }

    /// <summary>
    /// Set the time span to wait after initialization before
    /// starting the scheduler asynchronously. Default is 0, meaning
    /// immediate synchronous startup on initialization of this object.
    /// </summary>
    public virtual TimeSpan StartupDelay
    {
        set => startupDelay = value;
    }

    /// <summary>
    /// Set whether to wait for running jobs to complete on shutdown.
    /// Default is "false".
    /// </summary>
    /// <seealso cref="IScheduler.Shutdown(bool, CancellationToken)"/>
    public virtual bool WaitForJobsToCompleteOnShutdown
    {
        set => waitForJobsToCompleteOnShutdown = value;
    }

    /// <summary>
    /// Sets the Spring database provider whose connections the persistent job store uses.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Needs <see cref="QuartzDbProvider"/>. When this is set, the scheduler uses Quartz's ADO job store; without it, the
    /// in-memory store.
    /// </p>
    /// <p>
    /// The store is configured to accept a transaction that the application enlists
    /// (<c>AdoJobStoreOptions.AcceptEnlistedTransactions</c>), and to use database locks. Without an enlisted
    /// transaction it behaves like Quartz's own store, with its own transactions.
    /// </p>
    /// </remarks>
    public virtual Spring.Data.Common.IDbProvider DbProvider
    {
        set => dbProvider = value;
    }

    /// <summary>
    /// Sets the name of the Quartz database provider that describes the SQL dialect of the <see cref="DbProvider"/>. It
    /// selects Quartz's driver delegate and its metadata (parameter prefix, binary parameter type).
    /// </summary>
    /// <remarks>
    /// One of <c>SqlServer</c>, <c>SystemDataSqlClient</c>, <c>MicrosoftDataSqlClient</c>, <c>Npgsql</c>, <c>MySql</c>,
    /// <c>MySqlConnector</c>, <c>SQLite</c>, <c>SQLite-Microsoft</c>, <c>Firebird</c> or <c>OracleODPManaged</c>. The
    /// driver that Quartz names for it has to be installed.
    /// </remarks>
    public virtual string QuartzDbProvider
    {
        set => quartzDbProvider = value;
    }

    /// <summary>
    /// Sets a callback that receives the <see cref="IQuartzBuilder"/> after everything else has been applied to it, to
    /// configure the parts of Quartz that this class has no property for.
    /// </summary>
    public virtual Action<IQuartzBuilder> ConfigureQuartz
    {
        set => configureQuartz = value;
    }

    /// <summary>
    /// Set the name of the object in the object factory that created this object.
    /// </summary>
    /// <value>The name of the object in the factory.</value>
    /// <remarks>
    /// The name is used as the scheduler name if none is set explicitly.
    /// </remarks>
    public string ObjectName
    {
        set
        {
            if (schedulerName == null)
            {
                schedulerName = value;
            }
        }
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
                    return scheduler.Status == SchedulerStatus.Running;
                }
                catch (SchedulerException)
                {
                    return false;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Sets the application context.
    /// </summary>
    public virtual IApplicationContext ApplicationContext
    {
        set => applicationContext = value;
    }

    /// <summary>
    /// Shut down the Quartz scheduler on object factory shutdown,
    /// stopping all scheduled jobs.
    /// </summary>
    public virtual void Dispose()
    {
        if (scheduler == null)
        {
            // never initialized (AfterPropertiesSet was not called, or failed before creating the scheduler)
            return;
        }

        Logger.LogInformation("Shutting down Quartz Scheduler");
        try
        {
            // First, with the wait that was asked for: disposing the factory shuts down every scheduler it owns without
            // waiting for jobs.
            scheduler.Shutdown(waitForJobsToCompleteOnShutdown).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        finally
        {
            if (exposed)
            {
                SchedulerRegistry.Remove(scheduler);
                exposed = false;
            }

            schedulerFactory?.Dispose();
            schedulerFactory = null;
        }
    }

    /// <summary>
    /// Template method that determines the Scheduler to operate on.
    /// To be implemented by subclasses.
    /// </summary>
    protected override IScheduler GetScheduler()
    {
        return scheduler;
    }

    /// <summary>
    /// Return an instance (possibly shared or independent) of the object
    /// managed by this factory.
    /// </summary>
    public virtual object GetObject()
    {
        return scheduler;
    }

    /// <summary>
    /// Return the <see cref="System.Type"/> of object that this
    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
    /// <see langword="null"/> if not known in advance.
    /// </summary>
    public virtual Type ObjectType => (scheduler != null) ? scheduler.GetType() : typeof(IScheduler);

    /// <summary>
    /// Is the object managed by this factory a singleton or a prototype?
    /// </summary>
    public virtual bool IsSingleton => true;

    //---------------------------------------------------------------------
    // Implementation of IInitializingObject interface
    //---------------------------------------------------------------------

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
        ValidateConfiguration();

        QuartzSchedulerBuilder builder = QuartzSchedulerBuilder.Create(ConfigureBuilder);
        NameValueCollection properties = MergeProperties();
        if (properties.Count > 0)
        {
            builder.UseProperties(properties);
        }

        lock (SchedulerRegistry.SyncRoot)
        {
            if (schedulerName != null)
            {
                SchedulerRegistry.AssertNotRegistered(schedulerName);
            }

            scheduler = CreateScheduler(builder);

            try
            {
                SchedulerRegistry.AssertNotRegistered(scheduler.SchedulerName);
                if (exposeSchedulerInRepository)
                {
                    SchedulerRegistry.Bind(scheduler);
                    exposed = true;
                }
            }
            catch
            {
                Dispose();
                scheduler = null;
                throw;
            }
        }

        PopulateSchedulerContext();

        if (jobFactory is ISchedulerContextAware contextAware)
        {
            contextAware.SchedulerContext = scheduler.Context;
        }

        RegisterListeners();
        RegisterJobsAndTriggers();
    }

    /// <summary>
    /// Builds the scheduler from the configured builder. This is the extension point that replaces the scheduler factory
    /// type of Quartz 3: override it to return a scheduler of your own (or a test double).
    /// </summary>
    /// <remarks>
    /// The default implementation calls <see cref="QuartzSchedulerBuilder.Build"/> and keeps the resulting factory, which
    /// <see cref="Dispose"/> disposes after the scheduler has been shut down.
    /// </remarks>
    /// <param name="builder">The builder, configured with everything this object has been given.</param>
    /// <returns>The scheduler to manage.</returns>
    protected virtual IScheduler CreateScheduler(QuartzSchedulerBuilder builder)
    {
        StandaloneSchedulerFactory factory = builder.Build();
        try
        {
            IScheduler newScheduler = factory.GetScheduler().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            schedulerFactory = factory;
            return newScheduler;
        }
        catch
        {
            factory.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Applies the configuration of this object to the Quartz builder, then calls the <see cref="ConfigureQuartz"/> callback.
    /// </summary>
    /// <remarks>
    /// Override it to configure Quartz in a subclass; call the base implementation first to keep the precedence in which
    /// the callback has the last word.
    /// </remarks>
    /// <param name="quartz">The builder.</param>
    protected virtual void ConfigureBuilder(IQuartzBuilder quartz)
    {
        ConfigureLogging(quartz);
        ConfigureSchedulerOptions(quartz);
        ConfigureThreadPool(quartz);
        ConfigureJobFactory(quartz);
        ConfigureJobStore(quartz);

        configureQuartz?.Invoke(quartz);
    }

    private void ValidateConfiguration()
    {
        if (dbProvider != null && string.IsNullOrEmpty(quartzDbProvider))
        {
            throw new InvalidOperationException(
                "QuartzDbProvider is required when DbProvider is set. It selects Quartz's SQL dialect for the database, " +
                $"and is one of: {string.Join(", ", QuartzDialect.Names)}.");
        }

        if (dbProvider == null && (Clustered == true || ClusterCheckinInterval.HasValue))
        {
            throw new InvalidOperationException(
                "Clustered and ClusterCheckinInterval configure the persistent store that DbProvider sets up. " +
                "Set DbProvider, or configure clustering for your own store in the ConfigureQuartz callback.");
        }

        if (ClusterCheckinInterval.HasValue && Clustered != true)
        {
            throw new InvalidOperationException("ClusterCheckinInterval needs Clustered to be true.");
        }
    }

    private void ConfigureLogging(IQuartzBuilder quartz)
    {
        ILoggerFactory loggerFactory = LogManager.LoggerFactory;
        if (loggerFactory == null)
        {
            return;
        }

        // Quartz replaces an ILoggerFactory in the builder's services with its own LogProvider bridge, unless an
        // ILoggerProvider is registered next to it (the case of a container that has a logging configuration). The
        // provider hands out the loggers of the same factory, so Quartz logs through the Spring logger factory.
        quartz.Services.AddSingleton(loggerFactory);
        quartz.Services.AddSingleton<ILoggerProvider>(new SpringLoggerProvider(loggerFactory));
    }

    private void ConfigureSchedulerOptions(IQuartzBuilder quartz)
    {
        quartz.ConfigureScheduler(options =>
        {
            // Make sure to set the scheduler name as configured in the Spring configuration.
            if (schedulerName != null)
            {
                options.InstanceName = schedulerName;
            }

            if (InstanceId != null)
            {
                options.InstanceId = InstanceId;
            }

            if (GenerateInstanceId.HasValue)
            {
                options.GenerateInstanceId = GenerateInstanceId.Value;
            }

            if (IdleWaitTime.HasValue)
            {
                options.IdleWaitTime = IdleWaitTime.Value;
            }

            if (MaxBatchSize.HasValue)
            {
                options.MaxBatchSize = MaxBatchSize.Value;
            }

            if (DefaultTimeZone != null)
            {
                options.DefaultTimeZone = DefaultTimeZone;
            }
        });
    }

    private void ConfigureThreadPool(IQuartzBuilder quartz)
    {
        if (taskExecutor != null)
        {
            var threadPool = new LocalTaskExecutorThreadPool(taskExecutor);
            if (MaxConcurrency.HasValue)
            {
                threadPool.PoolSize = MaxConcurrency.Value;
            }

            quartz.UseThreadPool(threadPool);
        }
        else if (MaxConcurrency.HasValue)
        {
            quartz.UseDefaultThreadPool(MaxConcurrency.Value);
        }
    }

    private void ConfigureJobFactory(IQuartzBuilder quartz)
    {
        if (!jobFactorySet)
        {
            // Use AdaptableJobFactory as default, unless when explicitly given a null value
            // through the "jobFactory" object property.
            jobFactory = new AdaptableJobFactory();
        }

        if (jobFactory != null)
        {
            quartz.UseJobFactory(jobFactory);
        }
    }

    private void ConfigureJobStore(IQuartzBuilder quartz)
    {
        // The options classes are shared by every store of their kind, so these apply to the store that is chosen here,
        // by the properties or by the callback alike.
        if (MisfireThreshold.HasValue)
        {
            TimeSpan threshold = MisfireThreshold.Value;
            quartz.ConfigureOptions<InMemoryJobStoreOptions>(options => options.MisfireThreshold = threshold);
            quartz.ConfigureOptions<AdoJobStoreOptions>(options => options.MisfireThreshold = threshold);
        }

        if (TablePrefix != null)
        {
            string prefix = TablePrefix;
            quartz.ConfigureOptions<AdoJobStoreOptions>(options => options.TablePrefix = prefix);
        }

        if (dbProvider == null)
        {
            // The in-memory store is Quartz's default.
            return;
        }

        Spring.Data.Common.IDbProvider springProvider = dbProvider;
        string dialect = quartzDbProvider;
        var adapter = new SpringDbProviderAdapter(springProvider, dialect);
        quartz.UsePersistentStore(store =>
        {
            store.UseConnectionProvider(_ => adapter);
            QuartzDialect.UseDriverDelegate(store, dialect);
            store.ConfigureStore(options =>
            {
                // The store runs inside a transaction that the application hands to the scheduler, if there is one, and in
                // its own transactions otherwise. Locks have to be taken in the database for that to be safe.
                options.AcceptEnlistedTransactions = true;
                options.UseDbLocks = true;
            });

            if (Clustered == true)
            {
                store.UseClustering(clustering =>
                {
                    if (ClusterCheckinInterval.HasValue)
                    {
                        clustering.CheckinInterval = ClusterCheckinInterval.Value;
                    }
                });
            }
        });
    }

    /// <summary>
    /// Merges the Quartz properties file and <see cref="QuartzProperties"/>, which override the file.
    /// </summary>
    private NameValueCollection MergeProperties()
    {
        NameValueCollection mergedProps = new NameValueCollection();

        if (configLocation != null)
        {
            Logger.LogInformation("Loading Quartz config from [{ConfigLocation}]", configLocation);

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

        return mergedProps;
    }

    /// <summary>
    /// Merges the properties into the map.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <param name="map">The map.</param>
    protected virtual void MergePropertiesIntoMap(IDictionary properties, NameValueCollection map)
    {
        foreach (string key in properties.Keys)
        {
            map[key] = (string) properties[key];
        }
    }

    private void PopulateSchedulerContext()
    {
        // Put specified objects into Scheduler context.
        if (schedulerContextMap != null)
        {
            foreach (DictionaryEntry entry in schedulerContextMap)
            {
                scheduler.Context[entry.Key.ToString()] = entry.Value;
            }
        }

        // Register IApplicationContext in Scheduler context.
        if (applicationContextSchedulerContextKey != null)
        {
            if (applicationContext == null)
            {
                throw new SystemException("SchedulerFactoryObject needs to be set up in an IApplicationContext " +
                                          "to be able to handle an 'applicationContextSchedulerContextKey'");
            }

            scheduler.Context[applicationContextSchedulerContextKey] = applicationContext;
        }
    }

    /// <summary>
    /// Start the Quartz Scheduler, respecting the "startDelay" setting.
    /// </summary>
    /// <param name="sched">the Scheduler to start</param>
    /// <param name="startDelay">the time span to wait before starting
    /// the Scheduler asynchronously</param>
    protected virtual async Task StartScheduler(IScheduler sched, TimeSpan startDelay)
    {
        if (startDelay.TotalSeconds <= 0)
        {
            Logger.LogInformation("Starting Quartz Scheduler now");
            await sched.Start().ConfigureAwait(false);
        }
        else
        {
            Logger.LogInformation("Will start Quartz Scheduler [{SchedulerName}] in {StartDelaySeconds} seconds", sched.SchedulerName, startDelay);

            await sched.StartDelayed(startDelay).ConfigureAwait(false);
        }
    }

    //---------------------------------------------------------------------
    // Implementation of Lifecycle interface
    //---------------------------------------------------------------------

    /// <summary>
    /// Starts this instance.
    /// </summary>
    public virtual async Task Start()
    {
        if (scheduler != null)
        {
            try
            {
                await scheduler.Start().ConfigureAwait(false);
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
    public virtual async Task Stop()
    {
        if (scheduler != null)
        {
            try
            {
                await scheduler.Standby().ConfigureAwait(false);
            }
            catch (SchedulerException ex)
            {
                throw new SchedulingException("Could not stop Quartz Scheduler", ex);
            }
        }
    }

    /// <summary>
    /// Handle an application event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event that is being handled.</param>
    public void HandleApplicationEvent(object sender, ApplicationEventArgs e)
    {
        // auto-start Scheduler if demanded
        if (e is ContextRefreshedEventArgs && autoStartup)
        {
            try
            {
                StartScheduler(scheduler, startupDelay).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (SchedulerException ex)
            {
                throw new ObjectInitializationException("failed to auto-start scheduler", ex);
            }
        }
    }

    /// <summary>
    /// Gives Quartz the loggers of the Spring logger factory.
    /// </summary>
    private sealed class SpringLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerFactory loggerFactory;

        public SpringLoggerProvider(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return loggerFactory.CreateLogger(categoryName);
        }

        public void Dispose()
        {
            // The logger factory belongs to Spring.
        }
    }
}
