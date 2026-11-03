# Spring.Web.Mvc5

ASP.NET MVC 5 and Web API 2 integration for Spring.NET: a dependency resolver that creates controllers and their
dependencies from a Spring application context, and `SpringMvcApplication` as the base class for `Global.asax`.

```
dotnet add package Spring.Web.Mvc5
```

Target framework: net472 (ASP.NET on .NET Framework). Depends on Spring.Core, Spring.Web, Microsoft.AspNet.Mvc and
Microsoft.AspNet.WebApi.WebHost 5.3.0 or later. Configure the Spring web module in `web.config` as described in the
Spring.Web package.

## Usage

Derive the application class from `SpringMvcApplication`. It registers the Spring dependency resolvers for MVC and Web API,
so controllers are created by the container:

```csharp
using System.Web.Mvc;
using System.Web.Routing;
using Spring.Web.Mvc;

public class MvcApplication : SpringMvcApplication
{
    protected void Application_Start()
    {
        AreaRegistration.RegisterAllAreas();
        RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
}
```

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
