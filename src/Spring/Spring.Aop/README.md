# Spring.Aop

Aspect-oriented programming for Spring.NET: dynamic proxies, advice (before, after, around, throws), advisors and
pointcuts, and the `aop:` XML namespace for applying them to objects declared in a Spring container. Interceptors
implement the AOP Alliance interfaces.

```
dotnet add package Spring.Aop
```

Target frameworks: netstandard2.0, net472, net10.0. Depends on Spring.Core.

## Usage

Wrap a target in a proxy that runs an interceptor around every call to the target's interfaces:

```csharp
using AopAlliance.Intercept;
using Spring.Aop.Framework;

public class LoggingInterceptor : IMethodInterceptor
{
    public object Invoke(IMethodInvocation invocation)
    {
        Console.WriteLine("Calling {0}", invocation.Method.Name);
        return invocation.Proceed();
    }
}

var factory = new ProxyFactory(new OrderService());
factory.AddAdvice(new LoggingInterceptor());
var service = (IOrderService) factory.GetProxy();
```

`OrderService` is your own class and implements `IOrderService`.

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
