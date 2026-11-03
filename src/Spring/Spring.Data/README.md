# Spring.Data

Data access for Spring.NET: the ADO.NET framework (`AdoTemplate`, `IDbProvider`, row mappers, parameter builders and the
`db:` XML namespace) and declarative and programmatic transaction management (`IPlatformTransactionManager`,
`TransactionTemplate`, the `tx:` namespace). You add the ADO.NET provider package yourself, for example
System.Data.SqlClient.

```
dotnet add package Spring.Data
```

Target frameworks: netstandard2.0, net472, net10.0. Depends on Spring.Core and Spring.Aop.

## Usage

```csharp
using System.Data;
using Spring.Data.Common;
using Spring.Data.Generic;

IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
dbProvider.ConnectionString = "Server=.;Database=Spring;Integrated Security=true";

var template = new AdoTemplate(dbProvider);
var count = (int) template.ExecuteScalar(CommandType.Text, "select count(*) from Customer");
```

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
