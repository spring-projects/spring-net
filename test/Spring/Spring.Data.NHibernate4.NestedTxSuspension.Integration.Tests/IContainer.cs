using System;

namespace Spring.Data.NHibernate4.NestedTxSuspension.Integration.Tests
{
    public interface IContainer
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
