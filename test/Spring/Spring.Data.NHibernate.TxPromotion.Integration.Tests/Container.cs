using System;

namespace Spring.Data.NHibernate.TxPromotion.Integration.Tests
{
    public class Container : IContainer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
