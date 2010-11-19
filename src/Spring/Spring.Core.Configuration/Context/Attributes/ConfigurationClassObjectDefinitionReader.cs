using System;
using System.Collections.Generic;
using System.Text;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Parsing;
using Spring.Collections.Generic;
using System.Reflection;
using Spring.Objects.Factory.Config;
using Common.Logging;
using Spring.Objects;
using Spring.Util;

namespace Spring.Context.Attributes
{
    public class ConfigurationClassObjectDefinitionReader
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ConfigurationClassObjectDefinitionReader));

        private IProblemReporter _problemReporter;

        private IObjectDefinitionRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the ConfigurationClassObjectDefinitionReader class.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="problemReporter"></param>
        public ConfigurationClassObjectDefinitionReader(IObjectDefinitionRegistry registry, IProblemReporter problemReporter)
        {
            _registry = registry;
            _problemReporter = problemReporter;
        }

        public static bool CheckConfigurationClassCandidate(Type type)
        {
            if (type != null)
            {
                return (Attribute.GetCustomAttribute(type, typeof(ConfigurationAttribute)) != null);
            }

            return false;
        }

        public void LoadObjectDefinitions(ISet<ConfigurationClass> configurationModel)
        {
            foreach (ConfigurationClass configClass in configurationModel)
            {
                LoadObjectDefinitionsForConfigurationClass(configClass);
            }
        }

        private void LoadObjectDefinitionForConfigurationClassIfNecessary(ConfigurationClass configClass)
        {
            if (configClass.ObjectName != null)
            {
                // a Object definition already exists for this configuration class -> nothing to do
                return;
            }

            // no Object definition exists yet -> this must be an imported configuration class (@Import).
            GenericObjectDefinition configObjectDef = new GenericObjectDefinition();
            String className = configClass.ConfigurationClassType.Name;
            configObjectDef.ObjectType = configClass.ConfigurationClassType;
            if (CheckConfigurationClassCandidate(configClass.ConfigurationClassType))
            {
                String configObjectName = ObjectDefinitionReaderUtils.RegisterWithGeneratedName(configObjectDef, _registry);
                configClass.ObjectName = configObjectName;
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug(String.Format("Registered object definition for imported [Configuration] class {0}", configObjectName));
                }
            }
        }

        private void LoadObjectDefinitionsForConfigurationClass(ConfigurationClass configClass)
        {
            LoadObjectDefinitionForConfigurationClassIfNecessary(configClass);

            foreach (ConfigurationClassMethod method in configClass.Methods)
            {
                LoadObjectDefinitionsForModelMethod(method);
            }

            LoadObjectDefinitionsFromImportedResources(configClass.ImportedResources);
        }

        private void LoadObjectDefinitionsForModelMethod(ConfigurationClassMethod method)
        {
            ConfigurationClass configClass = method.ConfigurationClass;
            MethodInfo metadata = method.MethodMetadata;

            RootObjectDefinition objDef = new ConfigurationClassObjectDefinition();
            //ObjectDef.Resource = configClass.Resource;
            //ObjectDef.setSource(this.sourceExtractor.extractSource(metadata, configClass.getResource()));



            objDef.FactoryObjectName = configClass.ObjectName;
            objDef.FactoryMethodName = metadata.Name;
            objDef.AutowireMode = Objects.Factory.Config.AutoWiringMode.Constructor;

            //ObjectDef.setAttribute(RequiredAnnotationObjectPostProcessor.SKIP_REQUIRED_CHECK_ATTRIBUTE, Boolean.TRUE);

            // consider name and any aliases
            //Dictionary<String, Object> ObjectAttributes = metadata.getAnnotationAttributes(Object.class.getName());
            object[] objectAttributes = metadata.GetCustomAttributes(typeof(DefinitionAttribute), true);
            List<string> names = new List<string>();
            for (int i = 0; i < objectAttributes.Length; i++)
            {
                string[] namesAndAliases = ((DefinitionAttribute)objectAttributes[i]).NamesToArray;

                if (namesAndAliases != null)
                {
                    names.Add(metadata.Name);
                }
                else
                {
                    namesAndAliases = new[] { metadata.Name };
                }

                for (int j = 0; j < namesAndAliases.Length; j++)
                {
                    names.Add(namesAndAliases[j]);
                }
            }

            string objectName = (names.Count > 0 ? names[0] : method.MethodMetadata.Name);
            for (int i = 1; i < names.Count; i++)
            {
                _registry.RegisterAlias(objectName, names[i]);
            }

            // has this already been overridden (e.g. via XML)?
            if (_registry.ContainsObjectDefinition(objectName))
            {
                IObjectDefinition existingObjectDef = _registry.GetObjectDefinition(objectName);
                // is the existing Object definition one that was created from a configuration class?
                if (!(existingObjectDef is ConfigurationClassObjectDefinition))
                {
                    // no -> then it's an external override, probably XML
                    // overriding is legal, return immediately
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug(String.Format("Skipping loading Object definition for {0}: a definition for object " +
                                "'{1}' already exists. This is likely due to an override in XML.", method, objectName));
                    }
                    return;
                }
            }

            if (Attribute.GetCustomAttribute(metadata, typeof(PrimaryAttribute)) != null)
            {
                //TODO: determine how to respond to this attribute's presence
                //ObjectDef.isPrimary = true;
            }

            // is this Object to be instantiated lazily?
            if (Attribute.GetCustomAttribute(metadata, typeof(LazyAttribute)) != null)
            {
                objDef.IsLazyInit = (Attribute.GetCustomAttribute(metadata, typeof(LazyAttribute)) as LazyAttribute).LazyInitialize;
            }

            if (Attribute.GetCustomAttribute(metadata, typeof(DependsOnAttribute)) != null)
            {
                objDef.DependsOn = (Attribute.GetCustomAttribute(metadata, typeof(DependsOnAttribute)) as DependsOnAttribute).Name;
            }

            //Autowire autowire = (Autowire) ObjectAttributes.get("autowire");
            //if (autowire.isAutowire()) {
            //	ObjectDef.setAutowireMode(autowire.value());
            //}

            if (Attribute.GetCustomAttribute(metadata, typeof(DefinitionAttribute)) != null)
            {
                objDef.InitMethodName = (Attribute.GetCustomAttribute(metadata, typeof(DefinitionAttribute)) as DefinitionAttribute).InitMethod;
                objDef.DestroyMethodName = (Attribute.GetCustomAttribute(metadata, typeof(DefinitionAttribute)) as DefinitionAttribute).DestroyMethod;
            }

            // consider scoping
            if (Attribute.GetCustomAttribute(metadata, typeof(ScopeAttribute)) != null)
            {
                objDef.Scope = (Attribute.GetCustomAttribute(metadata, typeof(ScopeAttribute)) as ScopeAttribute).ObjectScope.ToString();
            }

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(String.Format("Registering Object definition for [Definition] method {0}.{1}()", configClass.ConfigurationClassType.Name, objectName));
            }

            _registry.RegisterObjectDefinition(objectName, objDef);
        }

        private void LoadObjectDefinitionsFromImportedResources(IDictionary<string, Type> importedResources)
        {
            IDictionary<Type, IObjectDefinitionReader> readerInstanceCache = new Dictionary<Type, IObjectDefinitionReader>();
            foreach (KeyValuePair<string, Type> entry in importedResources)
            {
                String resource = entry.Key;
                Type readerClass = entry.Value;

                if (!readerInstanceCache.ContainsKey(readerClass))
                {
                    try
                    {
                        IObjectDefinitionReader readerInstance = 
                               (IObjectDefinitionReader)Activator.CreateInstance(readerClass, _registry);

                        readerInstanceCache.Add(readerClass, readerInstance);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException(String.Format("Could not instantiate IObjectDefinitionReader class {0}", readerClass.FullName));
                    }
                }

                IObjectDefinitionReader reader = readerInstanceCache[readerClass];

                reader.LoadObjectDefinitions(resource);
            }
        }

        private class ConfigurationClassObjectDefinition : RootObjectDefinition
        {

        }

    }

}
