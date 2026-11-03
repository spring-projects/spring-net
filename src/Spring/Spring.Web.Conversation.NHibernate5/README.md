# Spring.Web.Conversation.NHibernate5

Conversation-per-business-transaction support for Spring.NET web applications using NHibernate 5: a conversation spans
several requests and keeps one NHibernate session open until it ends. `WebConversationManager` tracks the conversations
of a user's session, and `ConversationModule` ends the ones that have timed out.

```
dotnet add package Spring.Web.Conversation.NHibernate5
```

Target framework: net472 (ASP.NET on .NET Framework). Depends on Spring.Core, Spring.Data, Spring.Data.NHibernate5 and
NHibernate 5.7.1 or later.

## Usage

Declare the manager as a session-scoped object, and register the HTTP module in `web.config`:

```xml
<object name="conversationManager" type="Spring.Web.Conversation.WebConversationManager, Spring.Web.Conversation.NHibernate5" scope="session">
  <property name="SessionFactoryName" value="NHibernateSessionFactory" />
  <property name="EndPaused" value="true" />
</object>
```

```xml
<system.webServer>
  <modules>
    <add name="ConversationModule" type="Spring.Web.Conversation.HttpModule.ConversationModule, Spring.Web.Conversation.NHibernate5" />
  </modules>
</system.webServer>
```

The module learns the manager names through `HttpApplicationConfigurer` from Spring.Web (`ConversationManagerNameList`).

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
