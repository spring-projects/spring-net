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

using Common.Logging;

using Spring.Core;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Parsing;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Config;
using Spring.Collections.Generic;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Postprocesses the <see cref="ConfigurationAttribute"/> applied types registered with the <see cref="IApplicationContext"/>.
    /// </summary>
    public class ConfigurationClassPostProcessor : IObjectDefinitionRegistryPostProcessor, IOrdered
    {
        private static readonly ILog Logger = LogManager.GetLogger<ConfigurationClassPostProcessor>();

        private bool _postProcessObjectDefinitionRegistryCalled;

        private bool _postProcessObjectFactoryCalled;

        private IProblemReporter _problemReporter = new FailFastProblemReporter();

        /// <summary>
        /// Return the order value of this object, where a higher value means greater in
        /// terms of sorting.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// Normally starting with 0 or 1, with <see cref="F:System.Int32.MaxValue"/> indicating
        /// greatest. Same order values will result in arbitrary positions for the affected
        /// objects.
        /// </p>
        /// 	<p>
        /// Higher value can be interpreted as lower priority, consequently the first object
        /// has highest priority.
        /// </p>
        /// </remarks>
        /// <returns>The order value.</returns>
        public int Order => int.MinValue;

        /// <summary>
        /// Sets the problem reporter.
        /// </summary>
        /// <value>The problem reporter.</value>
        public IProblemReporter ProblemReporter
        {
            set { _problemReporter = (value ?? new FailFastProblemReporter()); }
        }

        /// <summary>
        /// Postsprocesses the object definition registry.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public void PostProcessObjectDefinitionRegistry(IObjectDefinitionRegistry registry)
        {
            if (_postProcessObjectDefinitionRegistryCalled)
            {
                throw new InvalidOperationException("PostProcessObjectDefinitionRegistry already called for this post-processor");
            }
            if (_postProcessObjectFactoryCalled)
            {
                throw new InvalidOperationException("PostProcessObjectFactory already called for this post-processor");
            }
            _postProcessObjectDefinitionRegistryCalled = true;
            ProcessConfigObjectDefinitions(registry);
        }

        /// <summary>
        /// Postprocesses the object factory.
        /// </summary>
        /// <param name="objectFactory">The object factory.</param>
        public void PostProcessObjectFactory(IConfigurableListableObjectFactory objectFactory)
        {
            if (_postProcessObjectFactoryCalled)
            {
                throw new InvalidOperationException(
                        "PostProcessObjectFactory already called for this post-processor");
            }
            _postProcessObjectFactoryCalled = true;
            if (!_postProcessObjectDefinitionRegistryCalled)
            {
                // ObjectDefinitionRegistryPostProcessor hook apparently not supported...
                // Simply call processConfigObjectDefinitions lazily at this point then.
                ProcessConfigObjectDefinitions((IObjectDefinitionRegistry)objectFactory);
            }

            EnhanceConfigurationClasses(objectFactory);
        }

        private void EnhanceConfigurationClasses(IConfigurableListableObjectFactory objectFactory)
        {
            ConfigurationClassEnhancer enhancer = new ConfigurationClassEnhancer(objectFactory);

            var objectNames = objectFactory.GetObjectDefinitionNames();

            foreach (string name in objectNames)
            {
                IObjectDefinition objDef = objectFactory.GetObjectDefinition(name);

                if (((AbstractObjectDefinition)objDef).HasObjectType)
                {
                    if (Attribute.GetCustomAttribute(objDef.ObjectType, typeof(ConfigurationAttribute)) != null)
                    {
                        //TODO check type of object isn't infrastructure type.

                        Type configClass = objDef.ObjectType;
                        Type enhancedClass = enhancer.Enhance(configClass);

                        Logger.Debug(m => m("Replacing object definition '{0}' existing class '{1}' with enhanced class", name, configClass.FullName));

                        ((IConfigurableObjectDefinition)objDef).ObjectType = enhancedClass;
                    }
                }
            }
        }

        private void ProcessConfigObjectDefinitions(IObjectDefinitionRegistry registry)
        {
            Collections.Generic.ISet<ObjectDefinitionHolder> configCandidates = new HashedSet<ObjectDefinitionHolder>();
            foreach (string objectName in registry.GetObjectDefinitionNames())
            {
                IObjectDefinition objectDef = registry.GetObjectDefinition(objectName);
                if (ConfigurationClassObjectDefinitionReader.CheckConfigurationClassCandidate(objectDef))
                {
                    configCandidates.Add(new ObjectDefinitionHolder(objectDef, objectName));
                }
            }

            //if nothing to process, bail out
            if (configCandidates.Count == 0) { return; }

            ConfigurationClassParser parser = new ConfigurationClassParser(_problemReporter);
            foreach (ObjectDefinitionHolder holder in configCandidates)
            {
                IObjectDefinition bd = holder.ObjectDefinition;
                try
                {
                    if (bd is AbstractObjectDefinition && ((AbstractObjectDefinition)bd).HasObjectType)
                    {
                        parser.Parse(((AbstractObjectDefinition)bd).ObjectType, holder.ObjectName);
                    }
                    else
                    {
                        //parser.Parse(bd.ObjectTypeName, holder.ObjectName);
                    }
                }
                catch (ObjectDefinitionParsingException ex)
                {
                    throw new ObjectDefinitionStoreException("Failed to load object class: " + bd.ObjectTypeName, ex);
                }
            }
            parser.Validate();

            // Read the model and create Object definitions based on its content
            ConfigurationClassObjectDefinitionReader reader = new ConfigurationClassObjectDefinitionReader(registry, _problemReporter);
            reader.LoadObjectDefinitions(parser.ConfigurationClasses);
        }
    }
}
