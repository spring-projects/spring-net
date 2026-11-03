# Spring.Messaging.Nms

Apache NMS integration for Spring.NET, used with ActiveMQ and other NMS providers: `NmsTemplate` for sending and receiving
with message conversion, message listener containers, and the `nms:` XML namespace. You add the NMS provider package
yourself, for example Apache.NMS.ActiveMQ.

```
dotnet add package Spring.Messaging.Nms
```

Target frameworks: netstandard2.0, net472, net10.0. Depends on Spring.Core, Spring.Aop, Spring.Data and Apache.NMS.

## Usage

```csharp
using Apache.NMS;
using Spring.Messaging.Nms.Core;

// connectionFactory comes from your NMS provider, for example Apache.NMS.ActiveMQ.
IConnectionFactory connectionFactory = CreateConnectionFactory();

var template = new NmsTemplate(connectionFactory)
{
    DefaultDestinationName = "orders"
};
template.ConvertAndSend("Hello World");
```

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
