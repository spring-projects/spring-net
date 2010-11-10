using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Objects.Factory.Support
{
    public interface IObjectDefinitionRegistryPostProcessor
    {
        void PostProcessObjectDefinitionRegistry(IObjectDefinitionRegistry registry);
    }
}
