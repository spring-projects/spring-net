using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.IO;
using Common.Logging;
using Spring.Context.Attributes;

namespace Spring.Objects.Factory.Support
{
    public static class AssemblyScanningExtensionMethods
    {
        public static void Scan(this IObjectDefinitionRegistry registry, IAssemblyTypeScanner scanner)
        {
            IEnumerable<Type> configTypes = scanner.Scan();

            //if we have at least one config class, ensure the post-processor is registered
            if (configTypes.Count() > 0)
            {
                EnsureConfigurationClassPostProcessorIsRegisteredFor(registry);
            }

            RegisiterDefintionsForTypes(configTypes, registry);
        }

        public static void Scan(this IObjectDefinitionRegistry registry)
        {
            Scan(registry, new AssemblyObjectDefinitionScanner());
        }

        public static void Scan(this IObjectDefinitionRegistry registry, Predicate<Type> typePredicate)
        {
            Scan(registry, null, ta => true, typePredicate);
        }

        public static void Scan(this IObjectDefinitionRegistry registry, string assemblyScanPath, Predicate<Assembly> assemblyPredicate, Predicate<Type> typePredicate)
        {
            //create a scanner instance using the scan path
            IAssemblyTypeScanner scanner = new AssemblyObjectDefinitionScanner(assemblyScanPath);

            //configure the scanner per the provided constraints
            scanner.WithAssemblyFilter(assemblyPredicate).WithIncludeFilter(typePredicate);

            //pass the scanner to primary Scan method to actually do the work
            Scan(registry, scanner);
        }

        public static void Scan(this IObjectDefinitionRegistry registry, Predicate<Assembly> assemblyPredicate, Predicate<Type> typePredicate)
        {
            Scan(registry, null, assemblyPredicate, typePredicate);
        }

        public static void Scan(this IObjectDefinitionRegistry registry, Predicate<Assembly> assemblyPredicate)
        {
            Scan(registry, null, assemblyPredicate, t => true);
        }

        /// <summary>
        /// Ensures the configuration class post processor is registered for.
        /// </summary>
        /// <param name="registry">The registry.</param>
        private static void EnsureConfigurationClassPostProcessorIsRegisteredFor(IObjectDefinitionRegistry registry)
        {
            var postProcessorBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(ConfigurationClassPostProcessor));
            if (!registry.ContainsObjectDefinition(postProcessorBuilder.ObjectDefinition.ObjectTypeName))
            {
                registry.RegisterObjectDefinition(postProcessorBuilder.ObjectDefinition.ObjectTypeName, postProcessorBuilder.ObjectDefinition);
            }
        }

        /// <summary>
        /// Regisiters the defintions for types.
        /// </summary>
        /// <param name="typesToRegister">The types to register.</param>
        /// <param name="registry">The registry.</param>
        private static void RegisiterDefintionsForTypes(IEnumerable<Type> typesToRegister, IObjectDefinitionRegistry registry)
        {
            foreach (Type type in typesToRegister)
            {
                ObjectDefinitionBuilder definition = ObjectDefinitionBuilder.GenericObjectDefinition(type);
                registry.RegisterObjectDefinition(definition.ObjectDefinition.ObjectTypeName, definition.ObjectDefinition);
            }
        }

    }
}
