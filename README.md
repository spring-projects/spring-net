THE SPRING.NET FRAMEWORK, Release 3.0.0  (PRE-RELEASE)
---------------------------------------------------------
http://www.springframework.net/


## 1. INTRODUCTION

The 3.0.0 release of Spring.NET contains

* A full featured Inversion of Control container
* An Aspect Oriented Programming framework
* Expression Language for lightweight scripting
* UI-agnostic validation framework
* ASP.NET Framework
    * Dependency Injection for pages and user controls, bi-directional data binding, and more.
* ASP.NET MVC and ASP.NET WebAPI Framework
    * Dependency Injection for MVC (includin WebAPI).
* Declarative transaction management abstraction
    * Declarative transaction management via use of common XML configuration and attributes across different transaction APIs
* ADO.NET framework
    * Simplifies use of ADO.NET.  DAO support classes and integration with Spring's declarative transaction management functionality
* Portable Service Abstractions
    * Export plain .NET objects via .NET Remoting, Web Service or .NET Serviced Component and create client side proxies based on endpoint URL and service interface.
* NHibernate Integation
    * NHibernate 5 integration to simplify use of NHibernate and participate in Spring's declarative transaction management functionality.
* ASP.NET AJAX Integration
    * Exporter to expose plain object on which Dependency Injection and AOP have been applied to JavaScript.
* NUnit and MSTest integration
    * Provides Dependency Injection of test cases and Spring container loading and caching. .  Data access and transaction management features aid with integration testing.
* WCF integration
    * Provides Dependency Injection and applicatin of AOP advice to WCF services
* Quartz integration
    * Configure Quartz jobs using dependency injection and Spring's transactional support to persist job details
* MSMQ integration
    * Simplifies the use MSMQ by providing helper classes for message sending and receiving.  Integrates with Spring's transaction management features.
* NMS integration
    * Simplifies the use of NMS by providing helper classes for message sending and receiving.
* TIBCO EMS integration
    * Simplifies the use of TIBCO EMS by providing helper classes for message sending and receiving.
* NVelocity integration
    * Simplifies the use of configuring NVelocity

Spring.NET is a port of the Java based Spring Framework. In turn, the Java/J2EE Spring Framework is based on code published in "Expert One-on-One J2EE Design and Development" by Rod Johnson (Wrox, 2002).


## 2. SUPPORTED .NET FRAMEWORK VERSIONS

Spring.NET 3.0.0 supports .NET 4.5.2 and .NET Standard 2.0 and later.

## 3. KNOWN ISSUES

<none>


## 4. RELEASE INFO

Release contents:

* "src" contains the C# source files for the framework
* "test" contains the C# source files for Spring.NET's test suite
* "bin" contains various Spring.NET distribution dll files
* "lib/Net" contains shared third-party libraries needed for building the framework
* "lib/NHibernate3" contains NHibernate 3.3 dlls
* "doc" contains reference documentation, MSDN-style API help, and the Spring.NET xsd.
* "examples" contains sample applications.
* "build-support" contains additonal applications need to build using NAnt as some convenience
   VS.NET solution files.
* "dev-support" contains 'developer support' tools and code, such as solution templates for VS.NET

debug build is done using /DEBUG:full and release build using /DEBUG:pdbonly flags.

The VS.NET solution for the framework and examples are provided.

Latest info is available at the public website: http://www.springframework.net/

The Spring Framework is released under the terms of the Apache Software License (see license.txt).


## 5. DISTRIBUTION DLLs

The "bin" directory contains the following distinct dll files for use in applications. Dependencies are those other than on the .NET BCL.

* __Spring.Core__ (~765 KB)
    * Contents: Inversion of control container. Collection classes.
    * Dependencies: Common.Logging

* __Spring.Aop__ (~150 KB)
    * Contents: Abstract Oriented Programming Framework.
    * Dependencies: Spring.Core, Common.Logging

* __Spring.Data__ (~320 KB)
    * Contents: Transaction and ADO.NET Framework.
    * Dependencies: Spring.Core, Spring.Aop

* __Spring.Data.NHibernate5__ (~90 KB)
    * Contents: NHibernate 5.x integration
    * Dependencies: Spring.Core, Spring.Aop, Spring.Data, NHibernate

* __Spring.Services__ (~70 KB)
    * Contents: Web Services, Remoting, and Enterprise Component based services.
    * Dependencies: Spring.Core, Spring.Aop

* __Spring.Web__ (~165 KB)
    * Contents: ASP.NET based Web Application Framework.
    * Dependencies: Spring.Core, Spring.Aop

* __Spring.Web.Extensions__ (~8 KB)
    * Contents: ASP.NET AJAX Integartion
    * Dependencies: Spring.Core, Spring.Aop, System.Web.Extensions

* __Spring.Web.Mvc5__ (~8 KB)
    * Contents: ASP.NET MVC5 and WebAPI Integartion
    * Dependencies: Spring.Core, Spring.Web

* __Spring.Testing.NUnit__ (~24 KB)
    * Contents: NUnit Integration
    * Dependencies: Spring.Core, Spring.Data, NUnit

* __Spring.Testing.Microsoft__ (~24 KB)
    * Contents: MSTest Integration
    * Dependencies: Spring.Core, Spring.Data, MSTest

* __Spring.Messaging__ (~65 KB)
    * Contents: MSMQ Integration
    * Dependencies: Spring.Core, Spring.Data, System.Messaging

* __Spring.Messaging.Nms__ (~100 KB)
    * Contents: NMS Integration
    * Dependencies: Spring.Core, Spring.Data, Apache NMS

* __Spring.Scheduling.Quartz3__ (~44 KB)
    * Contents: Quartz32.x Integration
    * Dependencies: Spring.Core, Spring.Data, Quartz

* __Spring.Template.Velocity__ (~44 KB)
    * Contents: NVelocity Integration
    * Dependencies: Spring.Core, NVelocity


## 6. WHERE TO START?

Documentation can be found in the "docs" directory:
* The Spring reference documentation

Documented sample applications can be found in "examples":
* IoCQuickStart.MovieFinder - A simple example demonstrating basic IoC container behavior.
* IoCQuickStart.AppContext - Show use of various IApplicationContext features.
* IoCQuickStart.EventRegistry - Show use of loosely coupled eventing features.
* AopQuickStart - Show use of AOP features.
* CachingQuickStart - Show use of Caching abstraction.
* SpringAir - Show use of Spring.Web features.
* Calculator - Show use of Spring.Services features.
* WebQuickStart - Show step by step usage of Spring.Web features.
* Web.Extensions.Example - Show ASP.NET AJAX integartion.
* DataQuickStart - Show use of Spring.Data data access features.
* TxQuickStart - Show use of Spring's transaction features.
* Data.NHibernate.Northwind - Show use of Spring's NHibernate features.
* WCFQuickStart - Show use of DI and AOP with WCF
* NMSQuickStart - Sample application using NMS
* MSMQ QuickStart - Sample application using MSMQ
* Quartz Example - Scheduling using Quartz
* Mvc5QuickStart - Show the configuration of the ASP.NET MVC 5 support

## 7. How to build

VS.NET
------

Visual Studio 2017 is required to open and build the solution. The free community version of Visual Studio should suffice.

For the first time you need to execute `Build.cmd` which will generate `GenCommonAssemblyInfo.cs` file required for builds.

NAnt
----

Build scripts are delivered with the download package.

To build the source and run the unit tests type

build test

If you want to run the build to create strongly signed assemblies you can generate a key file by executing the following command (assuming that sn.exe is properly on your search path):

sn -k Spring.Net.snk

You need to place the Spring.NET.snk file into the root folder of the source tree. All builds are strongly named using this key file when executing the following nant command:

nant -D:project.build.sign=true

InnovaSys Document X! is used to generate the SDK documentation.


## 8. Support

The user forums at http://forum.springframework.net/ are available for you to submit questions, support requests, and interact with other Spring.NET users.

Bug and issue tracking can be found at https://jira.springsource.org/browse/SPRNET

A Fisheye repository browser is located at https://fisheye.springframework.org/browse/spring-net

To get the sources, fork us on github at https://github.com/SpringSource/spring-net

We are always happy to receive your feedback on the forums. If you think you found a bug, have an improvement suggestion or feature request, please submit a ticket in JIRA (see link above).

A word on bug reports: If at all possible, try to download one of the nightly snapshots at http://www.springframework.net/downloads/nightly/ and see, if this bug has already been fixed. If the problem still persists, don't forget to mention the version of Spring.NET you are using (check the file versions of Spring.NET dlls), the .NET version you are running on and a description how to reproduce the problem.
Ideally attach some sample code reproducing the problem to the JIRA ticket.


## 9. Acknowledgements

InnovaSys Document X!
---------------------
InnovSys has kindly provided a license to generate the SDK documentation and supporting utilities for
integration with Visual Studio.
