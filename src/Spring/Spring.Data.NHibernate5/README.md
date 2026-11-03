# Spring.Data.NHibernate5

NHibernate 5 integration for Spring.NET: `LocalSessionFactoryObject` builds an NHibernate `ISessionFactory` inside the
container, `HibernateTemplate` handles sessions, `HibernateTransactionManager` lets NHibernate take part in Spring
transactions. `SessionScope` shares a session across a block of code, and `OpenSessionInViewModule` does it per web request on
.NET Framework.

```
dotnet add package Spring.Data.NHibernate5
```

Target frameworks: netstandard2.0, net472, net10.0. Depends on Spring.Core, Spring.Aop, Spring.Data and NHibernate 5.7.1 or later.

## Usage

```xml
<objects xmlns="http://www.springframework.net"
         xmlns:db="http://www.springframework.net/database">
  <db:provider id="DbProvider" provider="System.Data.SqlClient"
               connectionString="Data Source=(local);Initial Catalog=Spring;Integrated Security=true" />

  <object id="SessionFactory" type="Spring.Data.NHibernate.LocalSessionFactoryObject, Spring.Data.NHibernate5">
    <property name="DbProvider" ref="DbProvider" />
    <property name="MappingResources">
      <list><value>assembly://MyApp/MyApp.Domain/Customer.hbm.xml</value></list>
    </property>
    <property name="HibernateProperties">
      <dictionary>
        <entry key="hibernate.dialect" value="NHibernate.Dialect.MsSql2012Dialect" />
      </dictionary>
    </property>
  </object>
</objects>
```

```csharp
using System.Collections;
using Spring.Data.NHibernate;

var template = new HibernateTemplate(sessionFactory);
IList customers = template.Find("from Customer");
```

## Links

- [Spring.NET website](https://www.springframework.net/)
- [GitHub repository](https://github.com/spring-projects/spring-net)
- [Breaking changes since 3.x](https://github.com/spring-projects/spring-net/blob/main/BreakingChanges.txt)
