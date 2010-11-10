using System;
using System.Collections.Generic;
using System.Text;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Parsing;
using Spring.Collections.Generic;
using System.Reflection;
using Spring.Objects.Factory.Config;
using Common.Logging;

namespace Spring.Context.Annotation
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
                return (Attribute.GetCustomAttribute(type.GetType(), typeof(ConfigurationAttribute)) != null);
            }

            return false;
        }

        public void LoadBeanDefinitions(ISet<ConfigurationClass> configurationModel)
        {
            foreach (ConfigurationClass configClass in configurationModel)
            {
                LoadBeanDefinitionsForConfigurationClass(configClass);
            }
        }

        private void LoadBeanDefinitionForConfigurationClassIfNecessary(ConfigurationClass configClass)
        {
            if (configClass.ObjectName != null)
            {
                // a bean definition already exists for this configuration class -> nothing to do
                return;
            }

            // no bean definition exists yet -> this must be an imported configuration class (@Import).
            GenericObjectDefinition configBeanDef = new GenericObjectDefinition();
            String className = configClass.ConfigurationClassType.Name;
            configBeanDef.ObjectTypeName = className;
            if (CheckConfigurationClassCandidate(configClass.ConfigurationClassType))
            {
                String configObjectName = ObjectDefinitionReaderUtils.RegisterWithGeneratedName(configBeanDef, _registry);
                configClass.ObjectName = configObjectName;
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug(String.Format("Registered bean definition for imported [Configuration] class {0}", configObjectName));
                }
            }
        }

        private void LoadBeanDefinitionsForConfigurationClass(ConfigurationClass configClass)
        {
            LoadBeanDefinitionForConfigurationClassIfNecessary(configClass);

            foreach (ConfigurationClassMethod method in configClass.Methods)
            {
                LoadBeanDefinitionsForModelMethod(method);
            }

            LoadBeanDefinitionsFromImportedResources(configClass.ImportedResources);
        }

        private void LoadBeanDefinitionsForModelMethod(ConfigurationClassMethod method)
        {
            ConfigurationClass configClass = method.ConfigurationClass;
            MethodInfo metadata = method.MethodMetadata;

            RootObjectDefinition beanDef = new ConfigurationClassObjectDefinition();
            //beanDef.Resource = configClass.Resource;
            //beanDef.setSource(this.sourceExtractor.extractSource(metadata, configClass.getResource()));
            beanDef.FactoryObjectName = configClass.ObjectName;
            beanDef.FactoryMethodName = metadata.Name;
            beanDef.AutowireMode = Objects.Factory.Config.AutoWiringMode.Constructor;
            //beanDef.setAttribute(RequiredAnnotationBeanPostProcessor.SKIP_REQUIRED_CHECK_ATTRIBUTE, Boolean.TRUE);

            // consider name and any aliases
            //Dictionary<String, Object> beanAttributes = metadata.getAnnotationAttributes(Bean.class.getName());
            object[] objectAttributes = metadata.GetCustomAttributes(typeof(DefinitionAttribute), true);
            List<string> names = new List<string>();
            for (int i = 0; i < objectAttributes.Length; i++)
            {
                string[] namesAndAliases = ((DefinitionAttribute)objectAttributes[i]).Name;
                for (int j = 0; j > namesAndAliases.Length; j++)
                {
                    names.Add(namesAndAliases[j]);
                }

            }

            string beanName = (names.Count > 0 ? names[0] : method.MethodMetadata.Name);
            for (int i = 1; i < names.Count; i++)
            {
                _registry.RegisterAlias(beanName, names[i]);
            }

            // has this already been overridden (e.g. via XML)?
            if (_registry.ContainsObjectDefinition(beanName))
            {
                IObjectDefinition existingBeanDef = _registry.GetObjectDefinition(beanName);
                // is the existing bean definition one that was created from a configuration class?
                if (!(existingBeanDef is ConfigurationClassObjectDefinition))
                {
                    // no -> then it's an external override, probably XML
                    // overriding is legal, return immediately
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug(String.Format("Skipping loading bean definition for {0}: a definition for object " +
                                "'{1}' already exists. This is likely due to an override in XML.", method, beanName));
                    }
                    return;
                }
            }

            if (Attribute.GetCustomAttribute(metadata, typeof(PrimaryAttribute)) != null)
            {
                //TODO: determine how to respond to this attribute's presence
                //beanDef.isPrimary = true;
            }

            // is this bean to be instantiated lazily?
            if (Attribute.GetCustomAttribute(metadata, typeof(LazyAttrribute)) != null)
            {
                beanDef.IsLazyInit = (Attribute.GetCustomAttribute(metadata, typeof(LazyAttrribute)) as LazyAttrribute).LazyInitialize;
            }

            if (Attribute.GetCustomAttribute(metadata, typeof(DependsOnAttribute)) != null)
            {
                DependsOnAttribute attrib = Attribute.GetCustomAttribute(metadata, typeof(DependsOnAttribute)) as DependsOnAttribute;
                beanDef.DependsOn = (Attribute.GetCustomAttribute(metadata, typeof(DependsOnAttribute)) as DependsOnAttribute).Name;
            }

            //Autowire autowire = (Autowire) beanAttributes.get("autowire");
            //if (autowire.isAutowire()) {
            //	beanDef.setAutowireMode(autowire.value());
            //}

            //String initMethodName = (String) beanAttributes.get("initMethod");
            //if (StringUtils.hasText(initMethodName)) {
            //	beanDef.setInitMethodName(initMethodName);
            //}

            //String destroyMethodName = (String) beanAttributes.get("destroyMethod");
            //if (StringUtils.hasText(destroyMethodName)) {
            //	beanDef.setDestroyMethodName(destroyMethodName);
            //}

            // consider scoping
            if (Attribute.GetCustomAttribute(metadata, typeof(ScopeAttribute)) != null)
            {
                ScopeAttribute attrib = Attribute.GetCustomAttribute(metadata, typeof(ScopeAttribute)) as ScopeAttribute;
                beanDef.Scope = (Attribute.GetCustomAttribute(metadata, typeof(ScopeAttribute)) as ScopeAttribute).ObjectScope.ToString();
            }

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(String.Format("Registering bean definition for [Definition] method {0}.{1}()", configClass.ConfigurationClassType.Name, beanName));
            }

            _registry.RegisterObjectDefinition(beanName, beanDef);
        }

        private void LoadBeanDefinitionsFromImportedResources(IDictionary<string, Type> importedResources)
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
                               (IObjectDefinitionReader)Activator.CreateInstance(readerClass.GetType(), _registry);

                        readerInstanceCache.Add(readerClass, readerInstance);
                    }
                    catch (Exception ex)
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
