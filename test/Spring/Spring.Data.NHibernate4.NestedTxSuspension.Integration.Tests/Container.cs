using System;

namespace Spring.Data.NHibernate4.NestedTxSuspension.Integration.Tests
{
    public class Container : IContainer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
