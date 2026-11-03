# Spring.Core

The core of Spring.NET: the IoC container (object factories and application contexts configured from XML or code), the
Spring Expression Language (SpEL), type conversion, resource abstraction, validation and the dynamic reflection helpers
the other Spring.NET packages build on.

```
dotnet add package Spring.Core
```

Target frameworks: netstandard2.0, net472, net10.0.

## Usage

Define objects in XML, then load them through an application context:

```xml
<objects xmlns="http://www.springframework.net">
  <object id="greeter" type="MyApp.Greeter, MyApp">
    <property name="Message" value="Hello" />
  </object>
</objects>
```

```csharp
using Spring.Context;
using Spring.Context.Support;

IApplicationContext context = new XmlApplicationContext("file://objects.xml");
Greeter greeter = context.GetObject<Greeter>("greeter");
```

Spring.NET logs through Microsoft.Extensions.Logging. Assign `Spring.LogManager.LoggerFactory` at startup to receive its output.

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
