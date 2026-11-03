[![Build](https://github.com/spring-projects/spring-net/actions/workflows/ci.yml/badge.svg)](https://github.com/spring-projects/spring-net/actions/workflows/ci.yml)


THE SPRING.NET FRAMEWORK
---------------------------------------------------------
http://www.springframework.net/

## INTRODUCTION

Spring.NET contains:

* A full featured Inversion of Control container
* An Aspect Oriented Programming framework
* Expression Language for lightweight scripting
* UI-agnostic validation framework
* ASP.NET Framework
    * Dependency Injection for pages and user controls, bi-directional data binding, and more.
* ASP.NET MVC and ASP.NET WebAPI Framework
    * Dependency Injection for MVC (includin WebAPI).
* Declarative transaction management abstraction
    * Declarative transaction management via use of common XML configuration and attributes across different transaction
      APIs
* ADO.NET framework
    * Simplifies use of ADO.NET. DAO support classes and integration with Spring's declarative transaction management
      functionality
* NHibernate Integation
    * NHibernate 5 integration to simplify use of NHibernate and participate in Spring's declarative transaction
      management functionality.
* NUnit and MSTest integration
    * Provides Dependency Injection of test cases and Spring container loading and caching. . Data access and
      transaction management features aid with integration testing.
* Quartz integration
    * Configure Quartz jobs using dependency injection and Spring's transactional support to persist job details
* MSMQ integration
    * Simplifies the use MSMQ by providing helper classes for message sending and receiving. Integrates with Spring's
      transaction management features.
* NMS integration
    * Simplifies the use of NMS by providing helper classes for message sending and receiving.

Spring.NET is a port of the Java based Spring Framework. In turn, the Java/J2EE Spring Framework is based on code
published in "Expert One-on-One J2EE Design and Development" by Rod Johnson (Wrox, 2002).

## SUPPORTED .NET FRAMEWORK VERSIONS

Spring.NET 4.0.0 supports .NET Framework 4.7.2 and .NET Standard 2.0 and later.

## KNOWN ISSUES

<none>

## RELEASE INFO

Release contents:

* `src` contains the C# source files for the framework
* `test` contains the C# source files for Spring.NET's test suite
* `doc` contains the reference documentation sources and the Spring.NET xsd.
* `examples` contains sample applications.
* `build-support` contains the Fallout build (`build-support/build`) and the SQL scripts for the integration-test databases.
* `website` contains the sources of the project website.

Build with `./build.cmd` (Windows) or `./build.sh` (Linux/macOS); see the targets in `build-support/build`.

Latest info is available at the public website: http://www.springframework.net/

The Spring Framework is released under the terms of the Apache Software License (see license.txt).

## DISTRIBUTION DLLs

The "bin" directory contains the following distinct dll files for use in applications. Dependencies are those other than
on the .NET BCL.

* __Spring.Core__ (~765
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Core.svg)](https://www.nuget.org/packages/Spring.Core/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Core)](#)
    * Contents: Inversion of control container. Collection classes.
    * Dependencies: Common.Logging

* __Spring.Aop__ (~150
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Aop.svg)](https://www.nuget.org/packages/Spring.Aop/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Aop)](#)
    * Contents: Abstract Oriented Programming Framework.
    * Dependencies: Spring.Core, Common.Logging

* __Spring.Data__ (~320
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Data.svg)](https://www.nuget.org/packages/Spring.Data/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Data)](#)
    * Contents: Transaction and ADO.NET Framework.
    * Dependencies: Spring.Core, Spring.Aop

* __Spring.Data.NHibernate5__ (~90
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Data.NHibernate5.svg)](https://www.nuget.org/packages/Spring.Data.NHibernate5/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Data.NHibernate5)](#)
    * Contents: NHibernate 5.x integration
    * Dependencies: Spring.Core, Spring.Aop, Spring.Data, NHibernate

* __Spring.Web__ (~165
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Web.svg)](https://www.nuget.org/packages/Spring.Web/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Web)](#)
    * Contents: ASP.NET based Web Application Framework.
    * Dependencies: Spring.Core, Spring.Aop

* __Spring.Web.Mvc5__ (~8
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Web.Mvc5.svg)](https://www.nuget.org/packages/Spring.Web.Mvc5/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Web.Mvc5)](#)
    * Contents: ASP.NET MVC5 and WebAPI Integartion
    * Dependencies: Spring.Core, Spring.Web

* __Spring.Testing.NUnit__ (~24
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Testing.NUnit.svg)](https://www.nuget.org/packages/Spring.Testing.NUnit/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Testing.NUnit)](#)
    * Contents: NUnit Integration
    * Dependencies: Spring.Core, Spring.Data, NUnit

* __Spring.Testing.Microsoft__ (~24
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Testing.Microsoft.svg)](https://www.nuget.org/packages/Spring.Testing.Microsoft/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Testing.Microsoft)](#)
    * Contents: MSTest Integration
    * Dependencies: Spring.Core, Spring.Data, MSTest

* __Spring.Messaging__ (~65
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Messaging.svg)](https://www.nuget.org/packages/Spring.Messaging/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Messaging)](#)
    * Contents: MSMQ Integration
    * Dependencies: Spring.Core, Spring.Data, System.Messaging

* __Spring.Messaging.Nms__ (~100
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Messaging.Nms.svg)](https://www.nuget.org/packages/Spring.Messaging.Nms/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Messaging.Nms)](#)
    * Contents: NMS Integration
    * Dependencies: Spring.Core, Spring.Data, Apache NMS

* __Spring.Scheduling.Quartz3__ (~44
  KB) [![NuGet](http://img.shields.io/nuget/v/Spring.Scheduling.Quartz3.svg)](https://www.nuget.org/packages/Spring.Scheduling.Quartz3/) [![Downloads](https://img.shields.io/nuget/dt/Spring.Scheduling.Quartz3)](#)
    * Contents: Quartz32.x Integration
    * Dependencies: Spring.Core, Spring.Data, Quartz

## WHERE TO START?

Documentation can be found in the "docs" directory:

* The Spring reference documentation

Documented sample applications can be found in "examples":

* IoCQuickStart.MovieFinder - A simple example demonstrating basic IoC container behavior.
* IoCQuickStart.AppContext - Show use of various IApplicationContext features.
* IoCQuickStart.EventRegistry - Show use of loosely coupled eventing features.
* AopQuickStart - Show use of AOP features.
* CachingQuickStart - Show use of Caching abstraction.
* SpringAir - Show use of Spring.Web features.
* WebQuickStart - Show step by step usage of Spring.Web features.
* DataQuickStart - Show use of Spring.Data data access features.
* TxQuickStart - Show use of Spring's transaction features.
* Data.NHibernate.Northwind - Show use of Spring's NHibernate features.
* NMSQuickStart - Sample application using NMS
* MSMQ QuickStart - Sample application using MSMQ
* Quartz Example - Scheduling using Quartz
* Mvc5QuickStart - Show the configuration of the ASP.NET MVC 5 support

## How to build

VS.NET
------

Visual Studio 2022 is required to open and build the solution. The free community version of Visual Studio should
suffice.

### Running tests

- `./build.cmd Test` (`./build.sh Test`) runs the unit tests. They need nothing but the .NET SDK.
- The SQL Server backed integration tests (`Spring.Data.Integration.Tests`, `Spring.Data.NHibernate5.Integration.Tests`,
  `Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests` and `Spring.Scheduling.Quartz3.Integration.Tests`) provision their
  own database with [Testcontainers for .NET](https://dotnet.testcontainers.org/). All you need is a running Docker daemon
  (Linux containers):

  ```
  ./build.cmd IntegrationTest
  ```

  Each test assembly starts a pinned SQL Server 2022 container, creates the `springqa` login and the databases from
  `build-support/create-integration-test-databases-and-users.sql` and the project's own `*_database.sql` script (the `quartz`
  database gets the Quartz.NET tables), runs, and removes the container. There is no database to install, no hosts entry and no
  script to run by hand. CI runs the same target on Linux (the `integration-linux` job).
- To run against a SQL Server you already have, for example on a Windows machine without a Linux container engine, set
  `SPRING_TEST_SQLSERVER` to an administrator's connection string. No container is started and the same scripts run against that
  server (they drop and recreate the test databases and the `springqa` login, so do not point it at a server you care about):

  ```
  set SPRING_TEST_SQLSERVER=Server=localhost;Integrated Security=true;TrustServerCertificate=true
  ./build.cmd IntegrationTest
  ```
- The tests learn where the server is from these environment variables, which the test assembly sets once the scripts have run
  (the XML contexts use them as `${...}` placeholders):

  | Variable | Value |
  |---|---|
  | `SPRING_TEST_SQLSERVER_HOST` | `host,port` of the server |
  | `SPRING_TEST_SQLSERVER_USER`, `SPRING_TEST_SQLSERVER_PASSWORD` | the `springqa` login (`springqa` / `springqa`) |
  | `SPRING_TEST_SQLSERVER_CS_<DATABASE>` | a complete connection string, as `springqa`, for each database the scripts created, with the name upper-cased: `..._CS_SPRING`, `..._CS_CREDITS`, `..._CS_DEBITS`, `..._CS_CREDITSANDDEBITS`, `..._CS_QUARTZ` |

- Tests that need a distributed transaction (MSDTC) are `[Explicit]`: a SQL Server container cannot take part in one. Run them
  by name against a Windows server with MSDTC enabled if you work on that code.
- `./build.cmd Test --test-integration-data` also runs the four SQL Server projects as part of the `Test` target, and
  `--test-full` runs every integration project, including the ones that need ActiveMQ, MSMQ or a SQLite file.


## Acknowledgements

InnovaSys Document X!
---------------------
InnovSys has kindly provided a license to generate the SDK documentation and supporting utilities for
integration with Visual Studio.
