# Spring.Testing.Microsoft

Base classes for MSTest that load a Spring application context for your tests, cache it across test classes, inject
dependencies into the test class, and (in the transactional variants) roll back each test's database changes.

```
dotnet add package Spring.Testing.Microsoft
```

Target frameworks: netstandard2.0, net472, net10.0. Depends on Spring.Core, Spring.Data and MSTest.TestFramework 4.5.2 or later.

## Usage

Public properties of the test class are filled from the context by type before each test:

```csharp
using Spring.Testing.Microsoft;

[TestClass]
public class OrderServiceTests : AbstractDependencyInjectionSpringContextTests
{
    protected override string[] ConfigLocations => new[] { "assembly://MyApp.Tests/MyApp.Tests/objects.xml" };

    public IOrderService OrderService { get; set; }

    [TestMethod]
    public void PlacesAnOrder()
    {
        Assert.IsNotNull(OrderService);
    }
}
```

`AbstractTransactionalSpringContextTests` and `AbstractTransactionalDbProviderSpringContextTests` extend this with a
transaction per test.

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
