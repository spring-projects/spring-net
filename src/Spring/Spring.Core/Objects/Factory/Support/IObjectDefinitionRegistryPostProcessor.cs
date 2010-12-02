using System;
using System.Collections.Generic;
using System.Text;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    public interface IObjectDefinitionRegistryPostProcessor : IObjectFactoryPostProcessor
    {
        void PostProcessObjectDefinitionRegistry(IObjectDefinitionRegistry registry);
    }
}
