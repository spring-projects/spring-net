using System;
using System.Collections.Generic;
using System.Text;
using Common.Logging;
using Spring.Objects.Factory.Parsing;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Config;
using Spring.Collections.Generic;
using Spring.Objects.Factory;

namespace Spring.Context.Annotation
{
    public class ConfigurationClassPostProcessor : IObjectDefinitionRegistryPostProcessor
    {
        private ILog _logger = LogManager.GetLogger(typeof(ConfigurationClassPostProcessor));

        private bool _postProcessObjectDefinitionRegistryCalled = false;

        private bool _postProcessObjectFactoryCalled = false;

        private IProblemReporter _problemReporter = new FailFastProblemReporter();

        public IProblemReporter ProblemReporter
        {
            set { _problemReporter = (value ?? new FailFastProblemReporter()); }
        }

        //public int getOrder()
        //{
        //    return Ordered.HIGHEST_PRECEDENCE;
        //}

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

        private void ProcessConfigObjectDefinitions(IObjectDefinitionRegistry registry)
        {
            ISet<ObjectDefinitionHolder> configCandidates = new HashedSet<ObjectDefinitionHolder>();
            foreach (string objectName in registry.GetObjectDefinitionNames())
            {
                IObjectDefinition objectDef = registry.GetObjectDefinition(objectName);
                if (ConfigurationClassObjectDefinitionReader.CheckConfigurationClassCandidate(objectDef.ObjectType))
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

            // Read the model and create bean definitions based on its content
            ConfigurationClassObjectDefinitionReader reader = new ConfigurationClassObjectDefinitionReader(registry, _problemReporter);
            reader.LoadBeanDefinitions(parser.ConfigurationClasses);
        }

    }
}
