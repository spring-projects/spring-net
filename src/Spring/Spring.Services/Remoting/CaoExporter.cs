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

using System.Runtime.Remoting;

using Spring.Context;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.Remoting.Support;

namespace Spring.Remoting
{
	/// <summary>
	/// Registers an object type on the server
	/// as a Client Activated Object (CAO).
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	/// <author>Mark Pollack</author>
	/// <author>Bruno Baia</author>
	public class CaoExporter : ConfigurableLifetime, IApplicationContextAware, IObjectFactoryAware, IInitializingObject, IDisposable
	{
		#region Logging

		private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(CaoExporter));

		#endregion

		#region Fields

		private string targetName;
        private string[] interfaces;

		private IApplicationContext applicationContext;
		private AbstractObjectFactory objectFactory;

		private CaoRemoteFactory remoteFactory;

		#endregion

		#region Constructor(s) / Destructor

		/// <summary>
		/// Creates a new instance of the <see cref="CaoExporter"/> class.
		/// </summary>
		public CaoExporter()
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
        /// Sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// <p>
        /// Normally this call will be used to initialize the object.
        /// </p>
        /// <p>
        /// Invoked after population of normal object properties but before an
        /// init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// or a custom init-method. Invoked after the setting of any
        /// <see cref="Spring.Context.IResourceLoaderAware"/>'s
        /// <see cref="Spring.Context.IResourceLoaderAware.ResourceLoader"/>
        /// property.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// In the case of application context initialization errors.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If thrown by any application context methods.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
		public IApplicationContext ApplicationContext
		{
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
		/// Disconnect the remote object from the registered remoting channels.
		/// </summary>
		public void Dispose()
		{
			RemotingServices.Disconnect(remoteFactory);
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
			remoteFactory = new CaoRemoteFactory(this, targetName, interfaces, objectFactory);

			RemotingServices.Marshal(remoteFactory, targetName);

			#region Instrumentation

			if (LOG.IsDebugEnabled)
			{
				LOG.Debug(String.Format("Target '{0}' registered.", targetName));
			}

			#endregion
		}

		#endregion

        #region BaseCao inner class definition

        /// <summary>
        /// This class extends <see cref="Spring.Remoting.Support.BaseRemoteObject"/> to allow CAOs
        /// to be disconnect from the client.
        /// </summary>
        public abstract class BaseCao : BaseRemoteObject, IDisposable
        {
            #region IDisposable Members

            void IDisposable.Dispose()
            {
                RemotingServices.Disconnect(this);
            }

            #endregion
        }

        #endregion

        #region CaoRemoteFactory inner class definition

        private sealed class CaoRemoteFactory : MarshalByRefObject, ICaoRemoteFactory
		{
			#region Fields

			private AbstractObjectFactory objectFactory;
			private string targetName;
            private RemoteObjectFactory remoteObjectFactory;

			#endregion

			#region Constructor(s) / Destructor

			/// <summary>
			/// Create a new instance of the RemoteFactory.
			/// </summary>
            public CaoRemoteFactory(ILifetime lifetime, string targetName,
                string[] interfaces, AbstractObjectFactory objectFactory)
			{
				this.targetName = targetName;
				this.objectFactory = objectFactory;

                this.remoteObjectFactory = new RemoteObjectFactory();
                this.remoteObjectFactory.BaseType = typeof(BaseCao);
                this.remoteObjectFactory.Interfaces = interfaces;
                this.remoteObjectFactory.Infinite = lifetime.Infinite;
                this.remoteObjectFactory.InitialLeaseTime = lifetime.InitialLeaseTime;
                this.remoteObjectFactory.RenewOnCallTime = lifetime.RenewOnCallTime;
                this.remoteObjectFactory.SponsorshipTimeout = lifetime.SponsorshipTimeout;
			}

			#endregion

			#region Membres de ICaoRemoteFactory

			/// <summary>
			/// Returns the CAO proxy.
			/// </summary>
			/// <returns>The remote object.</returns>
			public object GetObject()
			{
				remoteObjectFactory.Target = objectFactory.GetObject(targetName);

                return remoteObjectFactory.GetObject();
			}

			/// <summary>
			/// Returns the CAO proxy using the
			/// argument list to call the constructor.
			/// </summary>
			/// <remarks>
			/// The matching of arguments to call the constructor is done
			/// by type. The alternative ways, by index and by constructor
			/// name are not supported.
			/// </remarks>
			/// <param name="constructorArguments">Constructor
			/// arguments used to create the object.</param>
			/// <returns>The remote object.</returns>
			public object GetObject(object[] constructorArguments)
			{
				RootObjectDefinition mergedObjectDefinition = objectFactory.GetMergedObjectDefinition(targetName, false);

				if (typeof(IFactoryObject).IsAssignableFrom(mergedObjectDefinition.ObjectType))
				{
					throw new NotSupportedException(
                        "Client activated objects with constructor arguments is not supported with IFactoryObject implementations.");
				}

                remoteObjectFactory.Target = objectFactory.GetObject(targetName, constructorArguments);

                return remoteObjectFactory.GetObject();
			}

			#endregion

            #region Overrided Methods

            /// <summary>
            /// Set infinite lifetime.
            /// </summary>
            public override object InitializeLifetimeService()
            {
                return null;
            }

            #endregion
        }

		#endregion
	}
}
