# Spring.Messaging

MSMQ integration for Spring.NET: `MessageQueueTemplate` for sending and receiving with message conversion, queues declared
as container objects (`MessageQueueFactoryObject`), and message listener containers. MSMQ is a Windows feature.

```
dotnet add package Spring.Messaging
```

Target frameworks: netstandard2.0, net472, net10.0. Depends on Spring.Core and Spring.Data. On net472 it uses the
framework's System.Messaging; the other targets use Experimental.System.Messaging.Signed.

## Usage

```xml
<objects xmlns="http://www.springframework.net">
  <object id="ordersQueue" type="Spring.Messaging.Support.MessageQueueFactoryObject, Spring.Messaging">
    <property name="Path" value=".\Private$\orders" />
  </object>
  <object id="queue" type="Spring.Messaging.Core.MessageQueueTemplate, Spring.Messaging">
    <property name="DefaultMessageQueueObjectName" value="ordersQueue" />
  </object>
</objects>
```

```csharp
using Spring.Messaging.Core;

var queue = context.GetObject<MessageQueueTemplate>("queue");
queue.ConvertAndSend("Hello World");
```

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
