#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

#if !MONO

using System.Reflection;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;

namespace Spring.EnterpriseServices
{
    /// <summary>
    /// Factory Object that instantiates and configures ServicedComponent.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This factory object should be used to instantiate and configure
    /// serviced components created by <see cref="ServicedComponentExporter"/>.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class ServicedComponentFactory : IConfigurableFactoryObject, IInitializingObject
    {
        #region Fields

        private string name;
        private string server;

        private bool isSingleton;
        private IObjectDefinition productTemplate;

        private Type componentType;
        private object singletonInstance;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates new instance of serviced component factory.
        /// </summary>
        public ServicedComponentFactory()
        {
            this.isSingleton = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets component name, as registered with COM+ Services.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets name of the remote server that COM+ component is registered with.
        /// </summary>
        public string Server
        {
            get { return server; }
            set { server = value; }
        }

        #endregion

        #region IConfigurableFactoryObject Members

        /// <summary>
        /// Returns configured instance of the serviced component.
        /// </summary>
        /// <returns>Configured instance of the serviced component.</returns>
        public object GetObject()
        {
            if (IsSingleton)
            {
                if (singletonInstance == null)
                {
                    singletonInstance = CreateInstance();
                }
                return singletonInstance;
            }
            else
            {
                return CreateInstance();
            }
        }

        /// <summary>
        /// Returns type of serviced component.
        /// </summary>
        public Type ObjectType
        {
            get { return componentType; }
        }

        /// <summary>
        /// Gets or sets whether serviced component should be treated as singleton. Default is <b>false</b>.
        /// </summary>
        public bool IsSingleton
        {
            get { return isSingleton; }
            set { isSingleton = value; }
        }

        /// <summary>
        /// Gets or sets the template object definition
        /// that should be used to configure proxy instance.
        /// </summary>
        public IObjectDefinition ProductTemplate
        {
            get { return productTemplate; }
            set { productTemplate = value; }
        }

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Initializes factory object.
        /// </summary>
        public void AfterPropertiesSet()
        {
            ValidateConfiguration();
            componentType = Type.GetTypeFromProgID(Name, Server);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        #endregion

        #region Private Methods

        private void ValidateConfiguration()
        {
            if (Name == null)
            {
                throw new ArgumentException("The Name property is required.");
            }
        }

        /// <summary>
        /// Creates new instance of serviced component.
        /// </summary>
        /// <returns>New instance of serviced component.</returns>
        private object CreateInstance()
        {
            return Activator.CreateInstance(componentType);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom(componentType.Assembly.CodeBase);
        }

        #endregion
    }
}

#endif // !MONO
