---
templateKey: default-template
path: /overview
title: Overview
description: Spring.NET is a port and extension of the Java based Spring Framework for .NET.
keywords:
  - .NET,
  - C#
  - Framework
  - Application
  - Framework
  - Web
  - Spring
  - Open source
---

# Overview

Spring.NET provides comprehensive infrastructural support for developing enterprise .NET applications.  It allows you to remove incidental complexity when using the base class libraries makes best practices, such as test driven development, easy practices.  Spring.NET is created, supported and sustained by [SpringSource](http://www.springsource.com/).

The design of Spring.NET is based on the Java version of the Spring Framework, which has shown real-world benefits and is used in thousands of enterprise applications world wide.  Spring .NET is not a quick port from the Java version, but rather a 'spiritual port' based on following proven  architectural and design patterns in that are not tied to a particular platform. 

The breath of functionality in Spring .NET spans application tiers which allows you to treat it as a ‘one stop shop’ but that is not required.  Spring .NET is not an all-or-nothing solution.  You can use the functionality in its modules independently.

Spring.NET consists of the following modules. Click on the module name for more information.

**[Spring.Core](/doc-latest/reference/html/objects.html)** – Use this module to configure your application using Dependency Injection.   

**[Spring.Aop](/doc-latest/reference/html/aop.html)** – Use this module to perform Aspect-Oriented Programming (AOP).  AOP centralizes common functionality that can then be declaratively applied across your application in a targeted manner.   An [aspect library](/doc-latest/reference/html/aop-aspect-library.html) provides predefined easy to use aspects for transactions, logging, performance monitoring, caching, method retry, and exception handling.

**[Spring.Data](/doc-latest/reference/html/spring-middle-tier.html)** – Use this module to achieve greater efficiency and consistency in writing data access functionality in ADO.NET and to perform declarative transaction management.

**[Spring.Data.NHibernate](/doc-latest/reference/html/orm.html)** – Use this module to integrate NHibernate with Spring’s declarative transaction management functionality allowing easy mixing of ADO.NET and NHibernate operations within the same transaction. NHibernate 1.0 users will benefit from ease of use APIs to perform data access operations.

**[Spring.Web](/doc-latest/reference/html/web.html)** – Use this module to raise the level of abstraction when writing ASP.NET web applications allowing you to effectively address common pain-points in ASP.NET such as data binding, validation, and ASP.NET page/control/module/provider configuration.

**[Spring.Web.Extensions](/doc-latest/reference/html/ajax.html)** – Use this module to easily expose a plain .NET object (PONO), that is one that doesn't have any attributes or special base classes, as a web service, configured via dependency injection, 'decorated' by applying AOP, and then exposed to client side java script.

**[Spring.Services](/doc-latest/reference/html/spring-services.html)** – Use this module to adapt plain .NET objects so they can be used with a specific distributed communication technology, such as .NET Remoting, Enterprise Services, ASMX Web Services, and WCF services.  These services can be configured via dependency injection and ‘decorated’ by applying AOP.

**[Spring.Testing.NUnit](/doc-latest/reference/html/testing.html)** - Use this module to perform integration testing with NUnit.

**[Spring.Testing.Microsoft](/doc-latest/reference/html/testing.html)** - Use this module to perform integration testing with Microsoft test framework (MSTEST).

**[Spring.Messaging](/doc-latest/reference/html/msmq.html)** - Use this module to increase your productivity creating Microsoft Message Queue (MSMQ) applications that adhere to architectural best practices

**[Spring.Messaging.Nms](/doc-latest/reference/html/messaging.html)** - Use this module to increase your productivity creating Apache ActiveMQ applications that adhere to architectural best practices.

**[Spring.Messaging.Ems](/doc-latest/reference/html/messaging.html)** - Use this module to increase your productivity creating TIBCO EMS applications that adhere to architectural best practices.

**[Spring.Scheduling.Quartz](/doc-latest/reference/html/scheduling.html#scheduling-quartz)** - Provides integration with the Quartz.NET job scheduler providing declarative configuration of Quartz jobs, schedulers, and triggers as well as several convenience classes to increase productivity when creating job scheduling applications.

**[Spring.Template.Velocity](/doc-latest/reference/html/templating.html)** - Helper classes to configure a NVelocity template engine in a Spring based application.

### Additional Functionality in Spring.Core

The Spring.Core module also includes the following additional features the you may find useful in their own right.
- [Expression Language](/doc-latest/reference/html/expressions.html) - provides efficient querying and manipulation of an object graphs at runtime.  
- [Validation Framework](/doc-latest/reference/html/validation.html) - a robust UI agnostic framework for creating complex validation rules for business objects either programmatically or declaratively.
- Data binding Framework - a UI agnostic framework for performing data binding.
- Dynamic Reflection - provides a high performance reflection API.
- [Threading](/doc-latest/reference/html/threading.html) - provides additional concurrency abstractions such as Latch, Semaphore and Thread Local Storage.
- [Resource abstraction](/doc-latest/reference/html/resources.html) - provides a common interface to treat the InputStream from a file and from a URL in a polymorphic and protocol-independent manner. 