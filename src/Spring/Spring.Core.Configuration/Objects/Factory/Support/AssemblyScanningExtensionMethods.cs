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
        private static ILog _logger = LogManager.GetLogger(typeof(AssemblyScanningExtensionMethods));

        /// <summary>
        /// Scans the assemblies for definitions.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="assemblyScanPath">The assembly scan path.</param>
        /// <param name="assemblyFilenamePredicate">The assembly filename predicate.</param>
        /// <param name="assemblyMetadataPredicate">The assembly metadata predicate.</param>
        /// <returns></returns>
        public static void ScanAssembliesAndRegisterDefinitions(this IObjectDefinitionRegistry registry, string assemblyScanPath, Func<string, bool> assemblyFilenamePredicate, Func<Assembly, bool> assemblyMetadataPredicate)
        {
            IEnumerable<Assembly> assemblies = GetAllMatchingAssemblies(assemblyScanPath, assemblyFilenamePredicate);

            assemblies = assemblies.Where(assembly => assemblyMetadataPredicate(assembly));

            IEnumerable<Type> configTypes = GetAllConfigurationTypesDefinedIn(assemblies);

            //if we have at least one config class, ensure the post-processor is registered
            if (configTypes.Count() > 0)
            {
                EnsureConfigurationClassPostProcessorIsRegisteredFor(registry);
            }

            RegisiterDefintionsForConfigTypes(configTypes, registry);
        }

        /// <summary>
        /// Scans the assemblies for definitions.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="assemblyMetadataPredicate">The assembly metadata predicate.</param>
        /// <returns></returns>
        public static void ScanAssembliesAndRegisterDefinitions(this IObjectDefinitionRegistry registry, Func<Assembly, bool> assemblyMetadataPredicate)
        {
            ScanAssembliesAndRegisterDefinitions(registry, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fn => true, assemblyMetadataPredicate);
        }

        /// <summary>
        /// Scans the assemblies for definitions.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="assemblyFilenamePredicate">The assembly filename predicate.</param>
        /// <param name="assemblyMetadataPredicate">The assembly metadata predicate.</param>
        /// <returns></returns>
        public static void ScanAssembliesAndRegisterDefinitions(this IObjectDefinitionRegistry registry, Func<string, bool> assemblyFilenamePredicate, Func<Assembly, bool> assemblyMetadataPredicate)
        {
            ScanAssembliesAndRegisterDefinitions(registry, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyFilenamePredicate, assemblyMetadataPredicate);
        }

        public static void ScanAssembliesAndRegisterDefinitions(this IObjectDefinitionRegistry registry)
        {
            ScanAssembliesAndRegisterDefinitions(registry, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fn => true, a => true);
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
        /// Gets all configuration types defined in the assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetAllConfigurationTypesDefinedIn(IEnumerable<Assembly> assemblies)
        {
            IList<Type> types = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (Attribute.GetCustomAttribute(type, typeof(ConfigurationAttribute), true) != null)
                    {
                        types.Add(type);
                    }
                }
            }

            return types;
        }

        /// <summary>
        /// Gets all matching assemblies.
        /// </summary>
        /// <param name="assemblyScanPath">The assembly scan path.</param>
        /// <param name="assemblyFilenamePredicate">The assembly filename predicate.</param>
        /// <returns></returns>
        private static IEnumerable<Assembly> GetAllMatchingAssemblies(string assemblyScanPath, Func<string, bool> assemblyFilenamePredicate)
        {
            IList<Assembly> assemblies = new List<Assembly>();

            IEnumerable<string> files = Directory.GetFiles(assemblyScanPath, "*.dll").Where(s => assemblyFilenamePredicate(Path.GetFileName(s)));

            foreach (string file in files)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFrom(file));
                }
                catch (Exception ex)
                {
                    //log and swallow everything that might go wrong here...
                    if (_logger.IsDebugEnabled)
                        _logger.Debug("Failed to load type while scanning Assemblies for Defintions!", ex);

                }
            }

            return assemblies;
        }

        /// <summary>
        /// Regisiters the defintions for config types.
        /// </summary>
        /// <param name="configTypes">The config types.</param>
        /// <param name="registry">The registry.</param>
        private static void RegisiterDefintionsForConfigTypes(IEnumerable<Type> configTypes, IObjectDefinitionRegistry registry)
        {
            foreach (Type configType in configTypes)
            {
                ObjectDefinitionBuilder definition = ObjectDefinitionBuilder.GenericObjectDefinition(configType);
                registry.RegisterObjectDefinition(definition.ObjectDefinition.ObjectTypeName, definition.ObjectDefinition);
            }
        }

    }
}
