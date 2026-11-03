# Spring.Scheduling.Quartz3

Quartz.NET 3.x integration for Spring.NET: `SchedulerFactoryObject` creates and manages a Quartz scheduler inside the
container, `JobDetailObject` and `MethodInvokingJobDetailFactoryObject` define jobs, and `SimpleTriggerObject` and
`CronTriggerObject` define triggers, all as ordinary Spring objects that can take injected dependencies.

```
dotnet add package Spring.Scheduling.Quartz3
```

Target frameworks: netstandard2.0, net472, net10.0. Depends on Spring.Core, Spring.Data and Quartz 3.22.9 or later.

## Usage

Call a method on a container object every second, repeating ten times after the first call:

```xml
<objects xmlns="http://www.springframework.net">
  <object id="scheduler" type="Spring.Scheduling.Quartz.SchedulerFactoryObject, Spring.Scheduling.Quartz3">
    <property name="Triggers">
      <list><ref local="exportTrigger" /></list>
    </property>
  </object>

  <object id="exportTrigger" type="Spring.Scheduling.Quartz.SimpleTriggerObject, Spring.Scheduling.Quartz3">
    <property name="JobDetail">
      <object type="Spring.Scheduling.Quartz.MethodInvokingJobDetailFactoryObject, Spring.Scheduling.Quartz3">
        <property name="TargetObject" ref="exportService" />
        <property name="TargetMethod" value="DoExport" />
      </object>
    </property>
    <property name="RepeatInterval" value="1s" />
    <property name="RepeatCount" value="10" />
  </object>

  <object id="exportService" type="MyApp.ExportService, MyApp" />
</objects>
```

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
