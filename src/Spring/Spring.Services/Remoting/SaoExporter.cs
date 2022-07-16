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

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting;
using Spring.Core.TypeResolution;
using Spring.Proxy;
using Spring.Objects.Factory;
using Spring.Remoting.Support;
using Spring.Util;

namespace Spring.Remoting
{
    /// <summary>
    /// Publishes an instance of an object under
    /// a given url as a Server Activated Object (SAO).
    /// </summary>
    /// <remarks>
    /// Remoting servers exported by <see cref="SaoExporter"/> always correspond to <see cref="WellKnownObjectMode.Singleton"/>.
    /// Objects can be exported either as SingleCall or Singleton by marking the exported object identified by
    /// <see name="TargetName"/> as either singleton or prototype.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Mark Pollack</author>
    /// <author>Bruno Baia</author>
    /// <author>Erich Eichinger</author>
    public class SaoExporter : ConfigurableLifetime, IObjectFactoryAware, IInitializingObject, IDisposable
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger( typeof( SaoExporter ) );

        #endregion

        /// <summary>
        /// Holds EXPORTER_ID to SaoExporter instance mappings.
        /// </summary>
        private static readonly IDictionary s_activeExporters = new Hashtable();

        ///<summary>
        /// Returns the target object instance exported by the SaoExporter identified by <see cref="EXPORTER_ID"/>.
        ///</summary>
        ///<param name="exporterId"></param>
        ///<returns></returns>
        public static object GetTarget( string exporterId )
        {
            SaoExporter exporterInstance;
            lock (s_activeExporters.SyncRoot)
            {
                exporterInstance = (SaoExporter)s_activeExporters[exporterId];
            }
            AssertUtils.ArgumentNotNull( exporterInstance, "exporterId", "Remoting Server object is not associated with any active SaoExporter" );
            object target = exporterInstance.GetTargetInstance();
            AssertUtils.ArgumentNotNull( target, "exporterId", string.Format( "Failed retrieving target object for SaoExporter ID {0}", exporterId ) );
            return target;
        }

        #region Fields

        private readonly string EXPORTER_ID = Guid.NewGuid().ToString();
        private string targetName;
        private string applicationName;
        private string serviceName;
        private string[] interfaces;

        private IObjectFactory objectFactory;
        private MarshalByRefObject remoteObject;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the <see cref="SaoExporter"/> class.
        /// </summary>
        public SaoExporter()
        {
            lock (s_activeExporters.SyncRoot)
            {
                s_activeExporters[EXPORTER_ID] = this;
            }
        }

        /// <summary>
        /// Cleanup before GC
        /// </summary>
        ~SaoExporter()
        {
            Dispose( false );
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
            GC.SuppressFinalize( this );
            Dispose( true );
        }

        /// <summary>
        /// Stops exporting the object identified by <see cref="TargetName"/>.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose( bool disposing )
        {
            if (disposing)
            {
                RemotingServices.Disconnect( remoteObject );
                lock (s_activeExporters.SyncRoot)
                {
                    s_activeExporters.Remove( this.EXPORTER_ID );
                }
                remoteObject = null;
                objectFactory = null;
            }
        }

        #endregion

        #region Private Methods

        private object GetTargetInstance()
        {
            return objectFactory.GetObject( targetName );
        }

        private void ValidateConfiguration()
        {
            if (TargetName == null)
            {
                throw new ArgumentException( "The TargetName property is required." );
            }

            if (ServiceName == null)
            {
                throw new ArgumentException( "The ServiceName property is required." );
            }
        }

        private void Export()
        {
            if (remoteObject != null)
            {
                throw new InvalidOperationException("object is already exported");
            }

            IProxyTypeBuilder builder = new SaoRemoteObjectProxyTypeBuilder( this );
            Type targetType = this.objectFactory.GetType( targetName );
            if (targetType == null)
            {
                // perform full object retrieval if type cannot be predicted - this will
                // also cause any object creation exceptions to be thrown
                targetType = this.objectFactory.GetObject(targetName).GetType();
            }

            if (targetType.IsInterface)
            {
                builder.Interfaces = new Type[] { targetType };
                builder.TargetType = typeof(object);
            }
            else
            {
                if (interfaces != null && interfaces.Length > 0)
                {
                    builder.Interfaces = TypeResolutionUtils.ResolveInterfaceArray(interfaces);
                }
                builder.TargetType = targetType;
            }

            Type proxyType = builder.BuildProxyType();
            string objectUri = (applicationName != null ? applicationName + "/" + serviceName : serviceName);

            ConstructorInfo proxyConstructor = proxyType.GetConstructor( Type.EmptyTypes );
            remoteObject = (MarshalByRefObject)proxyConstructor.Invoke( ObjectUtils.EmptyObjects );

            RemotingServices.Marshal( remoteObject, objectUri );

            #region Instrumentation

            if (LOG.IsDebugEnabled)
            {
                LOG.Debug( String.Format( "Target '{0}' exported as '{1}'.", targetName, objectUri ) );
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

            private static readonly MethodInfo SaoExporter_GetTargetInstance = typeof( SaoExporter ).GetMethod( "GetTarget", new Type[] { typeof( string ) } );

            private SaoExporter _saoExporter;

            #endregion

            #region Constructor(s) / Destructor

            /// <summary>
            /// Creates a new instance of the
            /// <see cref="SaoRemoteObjectProxyTypeBuilder"/> class.
            /// </summary>
            /// <param name="saoExporter">
            /// The exporter to be associated with the proxy.
            /// </param>
            public SaoRemoteObjectProxyTypeBuilder( SaoExporter saoExporter )
                : base( saoExporter )
            {
                this._saoExporter = saoExporter;

                Name = "SaoRemoteObjectProxy";
                BaseType = typeof( BaseRemoteObject );
            }

            #endregion

            #region IProxyTypeGenerator Members

            /// <summary>
            /// Generates the IL instructions that pushes
            /// the target instance on which calls should be delegated to.
            /// </summary>
            /// <param name="il">The IL generator to use.</param>
            public override void PushTarget( ILGenerator il )
            {
                il.Emit( OpCodes.Ldstr, this._saoExporter.EXPORTER_ID );
                il.Emit( OpCodes.Call, SaoExporter_GetTargetInstance );
            }

            #endregion

            #region Protected Methods

            /// <summary>
            /// Implements constructors for the proxy class.
            /// </summary>
            /// <param name="builder">
            /// The <see cref="System.Reflection.Emit.TypeBuilder"/> to use.
            /// </param>
            protected override void ImplementConstructors( TypeBuilder builder )
            {
                MethodAttributes attributes = MethodAttributes.Public |
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName;
                ConstructorBuilder cb = builder.DefineConstructor( attributes,
                  CallingConventions.Standard, Type.EmptyTypes );

                ILGenerator il = cb.GetILGenerator();
                GenerateRemoteObjectLifetimeInitialization( il );
                il.Emit( OpCodes.Ret );
            }

            /// <summary>
            /// Deaclares a field that holds the target object instance.
            /// </summary>
            /// <param name="builder">
            /// The <see cref="System.Type"/> builder to use for code generation.
            /// </param>
            protected override void DeclareTargetInstanceField( TypeBuilder builder )
            {
            }

            #endregion
        }

        #endregion
    }
}
