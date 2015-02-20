using System;

namespace Spring.SessionFactoryImplError.Tests
{
    public interface IContainer
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
