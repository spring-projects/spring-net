# Spring.Testing.NUnit

Base classes for NUnit that load a Spring application context for your tests, cache it across test fixtures, inject
dependencies into the fixture, and (in the transactional variants) roll back each test's database changes.

```
dotnet add package Spring.Testing.NUnit
```

Target frameworks: net10.0, net472. There is no netstandard2.0 build because NUnit 5 has none. Depends on Spring.Core,
Spring.Data and NUnit 5.0.0 or later.

## Usage

Public properties of the fixture are filled from the context by type before each test:

```csharp
using NUnit.Framework;
using Spring.Testing.NUnit;

[TestFixture]
public class OrderServiceTests : AbstractDependencyInjectionSpringContextTests
{
    protected override string[] ConfigLocations => new[] { "assembly://MyApp.Tests/MyApp.Tests/objects.xml" };

    public IOrderService OrderService { get; set; }

    [Test]
    public void PlacesAnOrder()
    {
        Assert.That(OrderService, Is.Not.Null);
    }
}
```

`AbstractTransactionalSpringContextTests` and `AbstractTransactionalDbProviderSpringContextTests` extend this with a
transaction per test.

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
