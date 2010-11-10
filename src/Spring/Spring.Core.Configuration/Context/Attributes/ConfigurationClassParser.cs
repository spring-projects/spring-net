using System;
using System.Collections.Generic;
using System.Text;
using Spring.Objects.Factory.Parsing;
using Spring.Collections.Generic;
using Spring.Util;
using System.Reflection;

namespace Spring.Context.Attributes
{

    public class ConfigurationClassParser
    {
        private ISet<ConfigurationClass> _configurationClasses = new HashedSet<ConfigurationClass>();

        private Stack<ConfigurationClass> _importStack = new Stack<ConfigurationClass>();

        private IProblemReporter _problemReporter;

        /// <summary>
        /// Initializes a new instance of the ConfigurationClassParser class.
        /// </summary>
        /// <param name="problemReporter"></param>
        public ConfigurationClassParser(IProblemReporter problemReporter)
        {
            _problemReporter = problemReporter;
        }

        public ISet<ConfigurationClass> ConfigurationClasses
        {
            get { return _configurationClasses; }
        }

        //public void Parse(String className, String beanName)
        //{
        //    ProcessConfigurationClass(new ConfigurationClass(className, beanName));
        //}

        public void Parse(Type type, string objectName)
        {
            ProcessConfigurationClass(new ConfigurationClass(objectName, type));
        }

        public void Validate()
        {
            foreach (ConfigurationClass configClass in ConfigurationClasses)
            {
                configClass.Validate(_problemReporter);
            }
        }

        protected void ProcessConfigurationClass(ConfigurationClass configurationClass)
        {
            DoProcessConfigurationClass(configurationClass);

            if (ConfigurationClasses.Contains(configurationClass) && configurationClass.ObjectName != null)
            {
                // Explicit object definition found, probably replacing an import.
                // Let's remove the old one and go with the new one.
                ConfigurationClasses.Remove(configurationClass);
            }
            ConfigurationClasses.Add(configurationClass);
        }

        private void DoProcessConfigurationClass(ConfigurationClass configurationClass)
        {
            if (Attribute.GetCustomAttribute(configurationClass.ConfigurationClassType, typeof(ImportAttribute)) != null)
            {
                ImportAttribute attrib = Attribute.GetCustomAttribute(configurationClass.ConfigurationClassType.GetType(), typeof(ImportAttribute)) as ImportAttribute;
                ProcessImport(configurationClass, attrib.Types);
            }

            if (Attribute.GetCustomAttribute(configurationClass.ConfigurationClassType, typeof(ImportResourceAttribute)) != null)
            {
                ImportResourceAttribute attrib = Attribute.GetCustomAttribute(configurationClass.ConfigurationClassType.GetType(), typeof(ImportResourceAttribute)) as ImportResourceAttribute;

                foreach (string resource in attrib.Resources)
                {
                    configurationClass.AddImportedResource(resource, attrib.DefinitionReader.GetType());
                }
            }

            ISet<MethodInfo> definitionMethods = GetAllMethodsWithCustomAttributeForClass(configurationClass.ConfigurationClassType, typeof(DefinitionAttribute));
            foreach (MethodInfo definitionMethod in definitionMethods)
            {
                configurationClass.Methods.Add(new ConfigurationClassMethod(definitionMethod, configurationClass));

            }
        }

        private ISet<MethodInfo> GetAllMethodsWithCustomAttributeForClass(Type theClass, Type customAttribute)
        {
            ISet<MethodInfo> methods = new HashedSet<MethodInfo>();

            foreach (MethodInfo method in theClass.GetMethods())
            {
                if (Attribute.GetCustomAttribute(method, customAttribute) != null)
                {
                    methods.Add(method);
                }
            }

            return methods;
        }

        private void ProcessImport(ConfigurationClass configClass, Type[] classesToImport)
        {
            if (_importStack.Contains(configClass))
            {
                _problemReporter.Error(new CircularImportProblem(configClass, _importStack, configClass.ConfigurationClassType));
            }
            else
            {
                _importStack.Push(configClass);
                foreach (Type classToImport in classesToImport)
                {
                    ProcessConfigurationClass(new ConfigurationClass(null, classToImport));
                }
                _importStack.Pop();
            }
        }

        private class CircularImportProblem : Problem
        {
            public CircularImportProblem(ConfigurationClass configClass, Stack<ConfigurationClass> importStack, Type configurationClassType)
                : base(String.Format("A circular @Import has been detected: " +
                             "Illegal attempt by [Configuration] class '{0}' to import class '{1}' as '{2}' is " +
                             "already present in the current import stack [{3}]",
                             importStack.Peek().SimpleName, configClass.SimpleName,
                             configClass.SimpleName, importStack),
                      new Location(importStack.Peek().Resource, configurationClassType)
                )
            { }

        }

    }
}
