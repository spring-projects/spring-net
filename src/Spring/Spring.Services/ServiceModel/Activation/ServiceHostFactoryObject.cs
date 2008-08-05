#if NET_3_5
#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

#region Imports

using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

using Spring.Util;
using Spring.Context;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.ServiceModel.Activation
{
    /// <summary>
    /// Factory that provides instances of <see cref="Spring.ServiceModel.ServiceHost" /> 
    /// to host objects created with Spring's IoC container.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: ServiceHostFactory.cs,v 1.2 2007/05/18 21:04:37 bbaia Exp $</version>
    public class ServiceHostFactoryObject : IFactoryObject, IApplicationContextAware, IInitializingObject, IDisposable
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(ServiceHostFactoryObject));

        #endregion

        #region Fields

        private string _targetName;
        private Uri[] _baseAddresses = new Uri[] { };
        private IApplicationContext _applicationContext;

        /// <summary>
        /// The <see cref="Spring.ServiceModel.SpringServiceHost" /> instance managed by this factory.
        /// </summary>
        protected SpringServiceHost springServiceHost;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the target object that should be exposed as a service.
        /// </summary>
        /// <value>
        /// The name of the target object that should be exposed as a service.
        /// </value>
        public string TargetName
        {
            get { return _targetName; }
            set { _targetName = value; }
        }

        /// <summary>
        /// Gets or sets the base addresses for the hosted service.
        /// </summary>
        /// <value>
        /// The base addresses for the hosted service.
        /// </value>
        public Uri[] BaseAddresses
        {
            get { return _baseAddresses; }
            set { _baseAddresses = value; }
        }

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the 
        /// <see cref="Spring.ServiceModel.Activation.ServiceHostFactoryObject"/> class.
        /// </summary>
        public ServiceHostFactoryObject()
        {
        }

        #endregion

        #region IApplicationContextAware Members

        /// <summary>
        /// Gets or sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        public IApplicationContext ApplicationContext
        {
            get { return _applicationContext; }
            set { _applicationContext = value; }
        }

        #endregion

        #region IFactoryObject Members

        /// <summary>
        /// Return a <see cref="Spring.ServiceModel.SpringServiceHost" /> instance 
        /// managed by this factory.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="Spring.ServiceModel.SpringServiceHost" /> 
        /// managed by this factory.
        /// </returns>
        public virtual object GetObject()
        {
            return springServiceHost;
        }

        /// <summary>
        /// Return the <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates.
        /// </summary>
        public virtual Type ObjectType
        {
            get { return typeof(SpringServiceHost); }
        }

        /// <summary>
        /// Always returns <see langword="false"/>
        /// </summary>
        public virtual bool IsSingleton
        {
            get { return false; }
        }

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Publish the object 
        /// </summary>
        public void AfterPropertiesSet()
        {
            ValidateConfiguration();

            springServiceHost = new SpringServiceHost(TargetName, _applicationContext, BaseAddresses);

            springServiceHost.Open();

            #region Instrumentation

            if (LOG.IsInfoEnabled)
            {
                LOG.Info(String.Format("The service '{0}' is ready and can now be accessed.", TargetName));
            }

            #endregion
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (springServiceHost != null)
            {
                springServiceHost.Close();
            }
        }

        #endregion

        #region Private Methods

        private void ValidateConfiguration()
        {
            if (TargetName == null)
            {
                throw new ArgumentException("The TargetName property is required.");
            }
        }

        #endregion
    }
}
#endif