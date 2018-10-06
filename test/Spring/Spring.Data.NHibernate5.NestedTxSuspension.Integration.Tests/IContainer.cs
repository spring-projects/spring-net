using System;

namespace Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests
{
    public interface IContainer
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
