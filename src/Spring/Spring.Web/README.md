# Spring.Web

ASP.NET WebForms integration for Spring.NET: a web application context built at startup, request, session and
application scoped objects, dependency injection into pages and user controls, and an extended `Spring.Web.UI.Page` with
data binding, validation, master-page support and result mapping.

```
dotnet add package Spring.Web
```

Target framework: net472 (ASP.NET on .NET Framework). Depends on Spring.Core.

## Usage

Register the Spring module and page handler in `web.config` (IIS integrated pipeline shown), and load the context from the
`spring/objects` section:

```xml
<configSections>
  <sectionGroup name="spring">
    <section name="context" type="Spring.Context.Support.WebContextHandler, Spring.Web" requirePermission="false" />
    <section name="objects" type="Spring.Context.Support.DefaultSectionHandler, Spring.Core" requirePermission="false" />
  </sectionGroup>
</configSections>
<system.webServer>
  <modules>
    <add name="SpringModule" type="Spring.Context.Support.WebSupportModule, Spring.Web" />
  </modules>
  <handlers>
    <add name="SpringPageHandler" verb="*" path="*.aspx" type="Spring.Web.Support.PageHandlerFactory, Spring.Web" />
  </handlers>
</system.webServer>
<spring>
  <context><resource uri="config://spring/objects" /></context>
  <objects xmlns="http://www.springframework.net">
    <object type="Default.aspx"><property name="Message" value="Hello" /></object>
  </objects>
</spring>
```

The page then receives `Message` as a public property.

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
