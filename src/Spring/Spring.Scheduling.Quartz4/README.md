# Spring.Scheduling.Quartz4

Quartz.NET 4.x integration for Spring.NET: `SchedulerFactoryObject` creates and manages a Quartz scheduler inside the
container, `JobDetailObject` and `MethodInvokingJobDetailFactoryObject` define jobs, and `SimpleTriggerObject` and
`CronTriggerObject` define triggers, all as ordinary Spring objects that can take injected dependencies.

```
dotnet add package Spring.Scheduling.Quartz4
```

Target framework: net10.0 (Quartz.NET 4 ships a net10.0 build only). Depends on Spring.Core, Spring.Aop, Spring.Data and
Quartz 4.7.2 or later. The namespace is `Spring.Scheduling.Quartz`, the same as in Spring.Scheduling.Quartz3.

## Usage

Call a method on a container object every second, repeating ten times after the first call:

```xml
<objects xmlns="http://www.springframework.net">
  <object id="scheduler" type="Spring.Scheduling.Quartz.SchedulerFactoryObject, Spring.Scheduling.Quartz4">
    <property name="Triggers">
      <list><ref local="exportTrigger" /></list>
    </property>
  </object>

  <object id="exportTrigger" type="Spring.Scheduling.Quartz.SimpleTriggerObject, Spring.Scheduling.Quartz4">
    <property name="JobDetail">
      <object type="Spring.Scheduling.Quartz.MethodInvokingJobDetailFactoryObject, Spring.Scheduling.Quartz4">
        <property name="TargetObject" ref="exportService" />
        <property name="TargetMethod" value="DoExport" />
      </object>
    </property>
    <property name="RepeatInterval" value="1s" />
    <property name="RepeatCount" value="10" />
    <property name="MisfireInstruction" value="NextWithRemainingCount" />
  </object>

  <object id="exportService" type="MyApp.ExportService, MyApp" />
</objects>
```

The scheduler starts when the application context has been refreshed and is shut down when the context is disposed
(`WaitForJobsToCompleteOnShutdown` decides whether running jobs are waited for).

A job that is a class of its own implements Quartz's `IJob`, or derives from `QuartzJobObject` to get the job data and
the scheduler context injected as object properties:

```csharp
public class ReportJob : QuartzJobObject
{
    public IReportService ReportService { get; set; } // from the scheduler context or the job data

    protected override async ValueTask ExecuteInternal(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        await ReportService.Publish(cancellationToken);
    }
}
```

## Configuring the scheduler

The scheduler is built with Quartz's `QuartzSchedulerBuilder`. Where a setting comes from, later wins over earlier:

1. `ConfigLocation` and `QuartzProperties`: the flat `quartz.*` keys of Quartz. Quartz throws for a key that it does not know.
2. The typed properties of `SchedulerFactoryObject`, each of which applies only when it is set: `SchedulerName`,
   `InstanceId`, `GenerateInstanceId`, `MaxConcurrency`, `MisfireThreshold`, `TablePrefix`, `Clustered`,
   `ClusterCheckinInterval`, `IdleWaitTime`, `MaxBatchSize` and `DefaultTimeZone`. `MisfireThreshold`,
   `ClusterCheckinInterval` and `IdleWaitTime` are `TimeSpan` properties, written the Spring way (`5s`, `2m`,
   `00:00:05`); an option that is not set is left to Quartz.
3. The `ConfigureQuartz` callback (`Action<IQuartzBuilder>`), which can reach everything that Quartz offers.

Quartz keeps the first registration of a component. A component that you ask for with `TaskExecutor`, `JobFactory` (when
it is set, even to `null`) or `DbProvider` is registered before the callback, so the callback cannot replace it: do not set
both. The default job factory, `AdaptableJobFactory`, is registered after the callback, so a job factory that the callback
registers wins when `JobFactory` is not set. That is the only default component; the thread pool and the job store are
Quartz's own defaults unless you set `TaskExecutor`, `MaxConcurrency` or `DbProvider`.

`TaskExecutor` makes a Spring `ITaskExecutor` the thread pool of the scheduler (`LocalTaskExecutorThreadPool`).
`JobFactory` defaults to `AdaptableJobFactory`; use `SpringObjectJobFactory` to have the scheduler context, the job data
and the trigger's job data injected as properties of any `IJob`.

### A persistent job store

`DbProvider` takes a Spring `IDbProvider`, whose connections the Quartz ADO job store then uses. `QuartzDbProvider` names
the SQL dialect: one of Quartz's built-in provider names, `SqlServer`, `MicrosoftDataSqlClient`, `SystemDataSqlClient`,
`Npgsql`, `MySql`, `MySqlConnector`, `SQLite`, `SQLite-Microsoft`, `Firebird` or `OracleODPManaged`. It selects Quartz's
driver delegate and Quartz's description of the dialect (parameter prefix, how a binary parameter is typed), and the driver
that Quartz names for it has to be installed. Without `DbProvider` the scheduler uses the in-memory store. The database
needs the Quartz 4 schema; see the migration links below.

Quartz 4 stores job data as JSON. A value that it cannot read back is refused when the job is written. That excludes what
`MethodInvokingJobDetailFactoryObject` and `JobDetailObject.ApplicationContextJobDataKey` put in the job data (the method
invoker, the application context): they work with the in-memory store only. For a persistent store, put Spring objects in
the scheduler context (`SchedulerContextAsMap`, `ApplicationContextSchedulerContextKey`) and write a job class that uses
them.

### Spring transactions and the job store

TODO (Quartz.NET 4.8): in Quartz 3, `LocalDataSourceJobStore` made the job store take part in a Spring transaction. The
job stores of Quartz 4 cannot be subclassed, so that is not available in this release. The store that `DbProvider`
configures already accepts a transaction that the application enlists with the scheduler
(`AcceptEnlistedTransactions`); the Spring side that enlists the current Spring transaction arrives with the enlistment
hook of Quartz.NET 4.8. Until then, `SchedulerAccessor.TransactionManager` runs the registration of jobs and triggers
between the begin and the commit of a transaction, on one thread, but the writes of the job store are not part of it.

## Moving from Spring.Scheduling.Quartz3

Quartz.NET 4 changed its API a lot; start with Quartz's own [3.x to 4.x migration guide][migration]. What changes in
Spring.Scheduling:

- The assembly name in `type=` strings is `Spring.Scheduling.Quartz4`. The namespaces are the same.
- `IJob.Execute` returns `ValueTask` and takes a `CancellationToken`, and so does `QuartzJobObject.Execute`.
  `QuartzJobObject.ExecuteInternal` is now `protected abstract ValueTask ExecuteInternal(IJobExecutionContext context,
  CancellationToken cancellationToken)`: subclasses have to change their signature.
- `Quartz.Spi` is `Quartz.Extensibility` (`IJobFactory`, `IThreadPool`, `TriggerFiredBundle`). `IJobFactory` has
  `CreateJob` and `ReturnJob`, and `AdaptableJobFactory` implements them: `CreateJobInstance(TriggerFiredBundle,
  IScheduler)` has a scheduler parameter. A job that the factory created is disposed after its execution if it is
  `IDisposable` or `IAsyncDisposable`, which Quartz 3 never did.
- `CronTriggerObject` and `SimpleTriggerObject` follow Quartz 4's trigger API. `Name`, `Group`, `JobName` and `JobGroup`
  are gone: the trigger key defaults to the Spring object name in the default group, `Key` sets it, and the job key comes
  from `JobDetail` (or `JobKey`).
- `MisfireInstructionName` is gone. `MisfireInstruction` is a typed property, `CronTriggerMisfireInstruction` or
  `SimpleTriggerMisfireInstruction`, set by the name of the enum member. Names of Quartz 3: cron `FireOnceNow` is
  `FireAndProceed`, `IgnoreMisfirePolicy` is `IgnoreMisfires`; simple `RescheduleNowWithExistingRepeatCount` is
  `NowWithExistingCount`, `RescheduleNowWithRemainingRepeatCount` is `NowWithRemainingCount`,
  `RescheduleNextWithRemainingCount` is `NextWithRemainingCount`, `RescheduleNextWithExistingCount` is
  `NextWithExistingCount`. `SmartPolicy`, `DoNothing` and `FireNow` are the same.
- `JobDetailObject` implements `IJobDetail` itself (`JobDetailImpl` is internal in Quartz 4). Its `JobType` must implement
  `IJob`: Quartz 3 accepted other types and adapted them with `DelegatingJob`, Quartz 4 refuses them. Use
  `MethodInvokingJobDetailFactoryObject` to run a method of an object, or write the one-line job.
- `SchedulerFactoryObject`: `SchedulerFactoryType` is gone, there is no scheduler factory to replace. Override
  `CreateScheduler(QuartzSchedulerBuilder)` or use `ConfigureQuartz` instead. `ConfigTimeDbProvider`,
  `ConfigTimeTaskExecutor`, `DefaultThreadCount`, `PropThreadCount` and `LocalDataSourceJobStore` are gone. `DbProvider`
  needs `QuartzDbProvider`. The typed properties above are new.
- `JobSchedulingDataLocation(s)` is gone: Quartz 4 keeps its XML scheduling data processor internal. Add the
  `Quartz.Plugins` package and register its plugin in the callback, for example
  `factory.ConfigureQuartz = quartz => quartz.UseXmlSchedulingConfiguration("quartz_jobs.xml")`. The plugin processes the
  file when the scheduler starts, with its own path syntax, and not inside `TransactionManager`'s transaction.
- `SchedulerAccessor.RegisterJobsAndTriggers` is `void`, and runs synchronously (a Spring transaction is bound to the
  thread). `GlobalJobListeners` and `GlobalTriggerListeners` register their listeners without matchers, which means all
  jobs or triggers. Quartz 4 keys listeners by their `Name`: a second listener with a name that is already taken replaces
  the first. `JobListeners` and `TriggerListeners` (the non-global ones) throw, as they did in Quartz 3: register those with
  matchers on `IScheduler.ListenerManager`.
- `LocalTaskExecutorThreadPool` takes the executor in its constructor and implements Quartz 4's `IThreadPool`.
  `SpringDbProviderAdapter` takes the Quartz dialect name, and `SpringMetadataAdapter` is gone.
- Schedulers that `ExposeSchedulerInRepository` are in a table of Spring's own, since Quartz 4 gives each scheduler builder a
  repository of its own. `SchedulerAccessorObject.SchedulerName` finds them there.
- Behaviour that Quartz changed and that your jobs can see: `EndTimeUtc` is inclusive, so a trigger whose end time is
  exactly one of its fire times fires once more; cron expressions are parsed more strictly (`MON/2` is a step, Unix
  five-field expressions are accepted) and a cron time inside a spring-forward gap fires when the gap ends (03:00 for a
  02:30 expression), where Quartz 3 fired at 03:30; `JobKey` and `TriggerKey` refuse a null name or group; the indexers of `JobDataMap` and `SchedulerContext`
  throw for a key that is missing. The database schema is a different one: run Quartz's [migration scripts][schema].
- Quartz 4's own property-setting job factory no longer merges the scheduler context into a job's properties.
  `SpringObjectJobFactory` still does, so the jobs of a Spring application see no difference.

## If you don't need Spring-managed jobs or triggers

Quartz 4 has hosting of its own. If your jobs are plain classes that need constructor injection and your triggers are
configured in code, you do not need this package: call `AddQuartz(...)` and `AddQuartzHostedService(...)` on the host
builder (or `services.AddQuartz(...)` on the `IServiceCollection`), as described in the [Quartz.NET quick start][quartz].
This package is for applications whose scheduler, jobs and triggers are objects of a Spring container.

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
- [Quartz.NET 3.x to 4.x migration guide][migration]
- [Quartz.NET database schema migration scripts][schema]

[migration]: https://www.quartz-scheduler.net/documentation/quartz-4.x/migration-guide.html
[schema]: https://github.com/quartznet/quartznet/tree/main/database/migrations
[quartz]: https://www.quartz-scheduler.net/documentation/quartz-4.x/quick-start.html
