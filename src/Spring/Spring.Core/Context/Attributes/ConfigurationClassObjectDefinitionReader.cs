#region License

/*
 * Copyright © 2010-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region

using System.Reflection;

using Common.Logging;

using Spring.Core.TypeResolution;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Parsing;
using Spring.Objects.Factory.Support;
using Spring.Stereotype;

#endregion

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Reads the class with the <see cref="ConfigurationAttribute"/> applied and converts it into an <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> instance.
    /// </summary>
    public class ConfigurationClassObjectDefinitionReader
    {
        private static readonly ILog Logger = LogManager.GetLogger<ConfigurationClassObjectDefinitionReader>();

        private IProblemReporter _problemReporter;

        private IObjectDefinitionRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the ConfigurationClassObjectDefinitionReader class.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="problemReporter"></param>
        public ConfigurationClassObjectDefinitionReader(IObjectDefinitionRegistry registry,
                                                        IProblemReporter problemReporter)
        {
            _registry = registry;
            _problemReporter = problemReporter;
        }

        private static bool HasAttributeOnMethods(Type objectType, Type attributeType)
        {
            Collections.Generic.ISet<MethodInfo> methods = ConfigurationClassParser.GetAllMethodsWithCustomAttributeForClass(objectType,
                                                                                                         attributeType);
            foreach (MethodInfo method in methods)
            {
                if (Attribute.GetCustomAttribute(method, attributeType) != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Loads the object definitions.
        /// </summary>
        /// <param name="configurationModel">The configuration model.</param>
        public void LoadObjectDefinitions(Collections.Generic.ISet<ConfigurationClass> configurationModel)
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

            // no Object definition exists yet -> this must be an imported configuration class ([Import]).
            GenericObjectDefinition configObjectDef = new GenericObjectDefinition();
            String className = configClass.ConfigurationClassType.Name;
            configObjectDef.ObjectTypeName = className;
            configObjectDef.ObjectType = configClass.ConfigurationClassType;
            if (CheckConfigurationClassCandidate(configClass.ConfigurationClassType))
            {
                String configObjectName = ObjectDefinitionReaderUtils.RegisterWithGeneratedName(configObjectDef,
                                                                                                _registry);
                configClass.ObjectName = configObjectName;
                Logger.Debug(m => m("Registered object definition for imported [Configuration] class {0}",
                                         configObjectName));
            }
        }

        /// <summary>
        /// Checks the class to see if it is a candidate to be a <see cref="ConfigurationAttribute"/> source.
        /// </summary>
        /// <param name="objectDefinition">The object definition.</param>
        /// <returns></returns>
        public static bool CheckConfigurationClassCandidate(IObjectDefinition objectDefinition)
        {
            Type objectType = null;
            if (objectDefinition is AbstractObjectDefinition)
            {
                AbstractObjectDefinition definition = (AbstractObjectDefinition)objectDefinition;
                if (definition.HasObjectType)
                {
                    objectType = definition.ObjectType;
                }
                else
                {
                    if (definition.ObjectTypeName != null && !definition.IsAbstract)
                    {
                        objectType = TypeResolutionUtils.ResolveType(definition.ObjectTypeName);
                    }
                }
                if (objectType != null)
                {
                    if (Attribute.GetCustomAttribute(objectType, typeof(ConfigurationAttribute)) != null)
                    {
                        return true;
                    }
                    if (Attribute.GetCustomAttribute(objectType, typeof(ComponentAttribute)) != null ||
                        HasAttributeOnMethods(objectType, typeof(ObjectDefAttribute)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckConfigurationClassCandidate(Type type)
        {
            if (type != null)
            {
                return (Attribute.GetCustomAttribute(type, typeof(ConfigurationAttribute)) != null);
            }

            return false;
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

            objDef.FactoryObjectName = configClass.ObjectName;
            objDef.FactoryMethodName = metadata.Name;
            objDef.AutowireMode = Objects.Factory.Config.AutoWiringMode.Constructor;

            // consider name and any aliases
            //Dictionary<String, Object> ObjectAttributes = metadata.getAnnotationAttributes(Object.class.getName());
            object[] objectAttributes = metadata.GetCustomAttributes(typeof(ObjectDefAttribute), true);
            List<string> names = new List<string>();
            foreach (object t in objectAttributes)
            {
                string[] namesAndAliases = ((ObjectDefAttribute)t).NamesToArray;

                if (namesAndAliases != null)
                {
                    names.Add(metadata.Name);
                }
                else
                {
                    namesAndAliases = new[] { metadata.Name };
                }

                names.AddRange(namesAndAliases);
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
                    Logger.Debug(m => m("Skipping loading Object definition for {0}: a definition for object " +
                                          "'{1}' already exists. This is likely due to an override in XML.", method,
                                          objectName));
                    return;
                }
            }

            //TODO: container does not presently support the concept of Primary object definition for type resolution
            //if (Attribute.GetCustomAttribute(metadata, typeof(PrimaryAttribute)) != null)
            //{
            //    ObjectDef.isPrimary = true;
            //}

            // is this Object to be instantiated lazily?
            if (Attribute.GetCustomAttribute(metadata, typeof(LazyAttribute)) != null)
            {
                objDef.IsLazyInit =
                    (Attribute.GetCustomAttribute(metadata, typeof(LazyAttribute)) as LazyAttribute).LazyInitialize;
            }

            if (Attribute.GetCustomAttribute(metadata, typeof(DependsOnAttribute)) != null)
            {
                objDef.DependsOn =
                    (Attribute.GetCustomAttribute(metadata, typeof(DependsOnAttribute)) as DependsOnAttribute).Name;
            }

            //TODO: container does not presently support autowiring to the degree needed to support this feature as of yet
            //Autowire autowire = (Autowire) ObjectAttributes.get("autowire");
            //if (autowire.isAutowire()) {
            //	ObjectDef.setAutowireMode(autowire.value());
            //}

            if (Attribute.GetCustomAttribute(metadata, typeof(ObjectDefAttribute)) != null)
            {
                objDef.InitMethodName =
                    (Attribute.GetCustomAttribute(metadata, typeof(ObjectDefAttribute)) as ObjectDefAttribute).
                        InitMethod;
                objDef.DestroyMethodName =
                    (Attribute.GetCustomAttribute(metadata, typeof(ObjectDefAttribute)) as ObjectDefAttribute).
                        DestroyMethod;
            }

            // consider scoping
            if (Attribute.GetCustomAttribute(metadata, typeof(ScopeAttribute)) != null)
            {
                objDef.Scope =
                    (Attribute.GetCustomAttribute(metadata, typeof(ScopeAttribute)) as ScopeAttribute).ObjectScope.ToString();
            }

            Logger.Debug(m => m("Registering Object definition for [ObjectDef] method {0}.{1}()",
                                            configClass.ConfigurationClassType.Name, objectName));

            _registry.RegisterObjectDefinition(objectName, objDef);
        }

        private void LoadObjectDefinitionsFromImportedResources(IEnumerable<KeyValuePair<string, Type>> importedResources)
        {
            IDictionary<Type, IObjectDefinitionReader> readerInstanceCache =
                new Dictionary<Type, IObjectDefinitionReader>();
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
                        throw new InvalidOperationException(
                            String.Format("Could not instantiate IObjectDefinitionReader class {0}",
                                          readerClass.FullName));
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
