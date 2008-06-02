#region License

/*
 * Copyright 2002-2004 the original author or authors.
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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting;

using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Proxy;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.Remoting.Support;
using Spring.Util;

#endregion

namespace Spring.Remoting
{
	/// <summary>
	/// Publishes an instance of an object under
	/// a given url as a Server Activated Object (SAO).
	/// </summary>
	/// <remarks>
	/// Object can be exported either as SingleCall or Singleton.
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
	/// <author>Mark Pollack</author>
	/// <author>Bruno Baia</author>
	public class SaoExporter : ConfigurableLifetime, IApplicationContextAware, IObjectFactoryAware, IInitializingObject, IDisposable
	{
		#region Logging

		private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(SaoExporter));

		#endregion

		#region Fields

		private string targetName;
		private string applicationName;
		private string serviceName;
        private string[] interfaces;

		private IApplicationContext applicationContext;
		private IObjectFactory objectFactory;

		private MarshalByRefObject remoteObject;

		#endregion

		#region Constructor(s) / Destructor

		/// <summary>
		/// Creates a new instance of the <see cref="SaoExporter"/> class.
		/// </summary>
		public SaoExporter()
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
		/// Gets or sets the name of the remote application.
		/// </summary>
		public string ApplicationName
		{
			get { return applicationName; }
			set { applicationName = value; }
		}

		/// <summary>
		/// Gets or sets the name of the exported remote service.
		/// <remarks>
		/// The name that will be used in the URI to refer to this service.
		/// This will be of the form, tcp://host:port/ServiceName or
		/// tcp://host:port/ApplicationName/ServiceName
		/// </remarks>
		/// </summary>
		public string ServiceName
		{
			get { return serviceName; }
			set { serviceName = value; }
		}

        /// <summary>
        /// Gets or sets the list of interfaces whose methods should be exported.
        /// </summary>
        /// <remarks>
        /// The default value of this property is all the interfaces 
        /// implemented or inherited by the target type.
        /// </remarks>
        /// <value>The interfaces to export.</value>
        public string[] Interfaces
        {
            get { return interfaces; }
            set { interfaces = value; }
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
			set { objectFactory = value; }
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
		/// Disconnect the remote object from the registered remoting channels.
		/// </summary>
		public void Dispose()
		{
			RemotingServices.Disconnect(remoteObject);
		}

		#endregion
		
		#region Private Methods

		private void ValidateConfiguration()
		{
			if (TargetName == null)
			{
				throw new ArgumentException("The TargetName property is required.");
			}

			if (ServiceName == null)
			{
				throw new ArgumentException("The ServiceName property is required.");
			}
		}

		private void Export()
		{
            IProxyTypeBuilder builder = new SaoRemoteObjectProxyTypeBuilder(applicationContext.Name, targetName, this);
			builder.TargetType = this.objectFactory.GetType(targetName);
            if (interfaces != null && interfaces.Length > 0)
            {
                builder.Interfaces = TypeResolutionUtils.ResolveInterfaceArray(interfaces);
            }

			ConstructorInfo proxyConstructor = 
				builder.BuildProxyType().GetConstructor(Type.EmptyTypes);

			string objectUri = (applicationName != null ? applicationName + "/" + serviceName : serviceName);

			remoteObject = (MarshalByRefObject) proxyConstructor.Invoke(ObjectUtils.EmptyObjects);

			RemotingServices.Marshal(remoteObject, objectUri);

			#region Instrumentation

			if (LOG.IsDebugEnabled)
			{
				LOG.Debug(String.Format("Target '{0}' exported as '{1}'.", targetName, objectUri));
			}

			#endregion
		}

		#endregion

		#region SaoRemoteObjectProxyTypeBuilder inner class definition

        /// <summary>
        /// Builds a proxy type based on <see cref="BaseRemoteObject"/> to wrap a target object 
        /// that is intended to be remotable.
        /// </summary>
        /// <remarks>
        /// The wrapped target object is retrieved by name from the IoC container.
        /// </remarks>
        private sealed class SaoRemoteObjectProxyTypeBuilder : RemoteObjectProxyTypeBuilder
        {
            #region Fields

            private static readonly MethodInfo TimeSpan_FromTicks =
                typeof(TimeSpan).GetMethod("FromTicks", BindingFlags.Public | BindingFlags.Static);

            private static readonly MethodInfo ContextRegistry_GetContext =
                typeof(ContextRegistry).GetMethod("GetContext", new Type[1] { typeof(string) });

            private static readonly MethodInfo IObjectFactory_GetObject =
                typeof(IObjectFactory).GetMethod("GetObject", new Type[1] { typeof(string) });

            private string contextName;
            private string targetObjectName;

            #endregion

            #region Constructor(s) / Destructor

            /// <summary>
            /// Creates a new instance of the 
            /// <see cref="SaoRemoteObjectProxyTypeBuilder"/> class.
            /// </summary>
            /// <param name="contextName">
            /// The name of the target object definition.
            /// </param>
            /// <param name="targetObjectName">
            /// The name of the application context that this object runs in.
            /// </param>
            /// <param name="lifetime">
            /// The lifetime properties to be applied to the target object.
            /// </param>
            public SaoRemoteObjectProxyTypeBuilder(string contextName, string targetObjectName, ILifetime lifetime)
                : base(lifetime)
            {
                this.contextName = contextName;
                this.targetObjectName = targetObjectName;

                Name = "SaoRemoteObjectProxy";
                BaseType = typeof(BaseRemoteObject);
            }

            #endregion

            #region IProxyTypeGenerator Members

            /// <summary>
            /// Generates the IL instructions that pushes 
            /// the target instance on which calls should be delegated to.
            /// </summary>
            /// <param name="il">The IL generator to use.</param>
            public override void PushTarget(ILGenerator il)
            {
                il.Emit(OpCodes.Ldstr, this.contextName);
                il.Emit(OpCodes.Call, ContextRegistry_GetContext);
                il.Emit(OpCodes.Ldstr, this.targetObjectName);
                il.Emit(OpCodes.Callvirt, IObjectFactory_GetObject);
            }

            #endregion

            #region Protected Methods

            /// <summary>
            /// Implements constructors for the proxy class.
            /// </summary>
            /// <param name="builder">
            /// The <see cref="System.Reflection.Emit.TypeBuilder"/> to use.
            /// </param>
            protected override void ImplementConstructors(TypeBuilder builder)
            {
                MethodAttributes attributes = MethodAttributes.Public |
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName;
                ConstructorBuilder cb = builder.DefineConstructor(attributes,
                  CallingConventions.Standard, Type.EmptyTypes);

                ILGenerator il = cb.GetILGenerator();
                GenerateRemoteObjectLifetimeInitialization(il);
                il.Emit(OpCodes.Ret);
            }

            /// <summary>
            /// Deaclares a field that holds the target object instance.
            /// </summary>
            /// <param name="builder">
            /// The <see cref="System.Type"/> builder to use for code generation.
            /// </param>
            protected override void DeclareTargetInstanceField(TypeBuilder builder)
            {
            }

            #endregion
        }

		#endregion
	}
}