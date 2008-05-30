#if NET_3_0
#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.Collections;

using Spring.Util;
using Spring.Context;
using Spring.Core.TypeResolution;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.ServiceModel.Support;
using Spring.Context.Support;
using Spring.Proxy;

#endregion

namespace Spring.ServiceModel
{
	/// <summary>
	/// Registers an object type on the server as a WCF service.
	/// </summary>
	/// <author>Bruno Baia</author>
	/// <version>$Id: ServiceExporter.cs,v 1.2 2007/09/21 14:26:26 bbaia Exp $</version>
	public class ServiceExporter : IApplicationContextAware, IObjectFactoryAware, IInitializingObject, IDisposable
	{
		#region Logging

		private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(ServiceExporter));

		#endregion

		#region Fields

		private string targetName;
        private string[] _contracts;
        private IList _typeAttributes = new ArrayList();
        private IDictionary _memberAttributes = new Hashtable();

		private IApplicationContext applicationContext;
		private AbstractObjectFactory objectFactory;

		private System.ServiceModel.ServiceHost serviceHost;

		#endregion

		#region Constructor(s) / Destructor

		/// <summary>
		/// Creates a new instance of the <see cref="ServiceExporter"/> class.
		/// </summary>
        public ServiceExporter()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the target object definition.
		/// </summary>
		public string TargetName
		{
			get { return targetName; }
			set { targetName = value; }
		}

        /// <summary>
        /// Gets or sets the list of service contracts.
        /// </summary>
        /// <remarks>
        /// If not set, all the interfaces implemented or inherited 
        /// by the target type will be used.
        /// </remarks>
        /// <value>The interfaces.</value>
        public string[] Contracts
        {
            get { return _contracts; }
            set { _contracts = value; }
        }

        /// <summary>
        /// Gets or sets a list of custom attributes 
        /// that should be applied to the WCF service class.
        /// </summary>
        public IList TypeAttributes
        {
            get { return _typeAttributes; }
            set { _typeAttributes = value; }
        }

        /// <summary>
        /// Gets or sets a dictionary of custom attributes 
        /// that should be applied to the WCF service members.
        /// </summary>
        /// <remarks>
        /// Dictionary key is an expression that members can be matched against. 
        /// Value is a list of attributes that should be applied 
        /// to each member that matches expression.
        /// </remarks>
        public IDictionary MemberAttributes
        {
            get { return _memberAttributes; }
            set { _memberAttributes = value; }
        }

		#endregion

		#region IApplicationContextAware Members

		/// <summary>
		/// Gets or sets the <see cref="Spring.Context.IApplicationContext"/> that this
		/// object runs in.
		/// </summary>
		public IApplicationContext ApplicationContext
		{
			get { return applicationContext; }
			set { applicationContext = value; }
		}

		#endregion

		#region IObjectFactoryAware Members

		/// <summary>
		/// Sets object factory to use.
		/// </summary>
		public IObjectFactory ObjectFactory
		{
			set { objectFactory = (AbstractObjectFactory) value; }
		}

		#endregion

		#region IInitializingObject Members

		/// <summary>
		/// Publish the object 
		/// </summary>
		public void AfterPropertiesSet()
		{
			ValidateConfiguration();
			Export();
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Closes the service host.
		/// </summary>
		public void Dispose()
		{
            serviceHost.Close();
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

		private void Export()
		{
            string contextName = (applicationContext.Name == AbstractApplicationContext.DefaultRootContextName) ? null : applicationContext.Name;
            Type targetType = objectFactory.GetType(targetName);
            IProxyTypeBuilder builder = new ServiceProxyTypeBuilder(contextName, targetName, targetType);
            if (Contracts != null && Contracts.Length > 0)
            {
                builder.Interfaces = TypeResolutionUtils.ResolveInterfaceArray(Contracts);
            }
            builder.TypeAttributes = TypeAttributes;
            builder.MemberAttributes = MemberAttributes;

            Type serviceType = builder.BuildProxyType();

            serviceHost = new System.ServiceModel.ServiceHost(serviceType);
            serviceHost.Open();

            #region Instrumentation

            if (LOG.IsDebugEnabled)
            {
                LOG.Debug(String.Format("The service '{0}' is ready and can now be accessed.", targetName));
            }

            #endregion
		}

		#endregion
	}
}
#endif