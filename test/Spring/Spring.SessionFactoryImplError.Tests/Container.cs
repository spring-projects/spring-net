using System;

namespace Spring.SessionFactoryImplError.Tests
{
    public class Container : IContainer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
