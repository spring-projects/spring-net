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

using System.Reflection;

using Spring.Proxy;
using Spring.Objects.Factory;
using Spring.Remoting.Support;
using Spring.Core.TypeResolution;

namespace Spring.Remoting
{
	/// <summary>
	/// Factory for creating MarshalByRefObject wrapper around target class.
	/// </summary>
	/// <author>Bruno Baia</author>
	public class RemoteObjectFactory : ConfigurableLifetime, IInitializingObject, IFactoryObject
	{
		#region Fields

		private object target;
        private Type baseType = typeof(BaseRemoteObject);
        private string[] interfaces;

		private ConstructorInfo proxyConstructor;

		#endregion

		#region Constructor(s) / Destructor

		/// <summary>
		/// Creates a new instance of the MarshalByRefObjectFactory.
		/// </summary>
		public RemoteObjectFactory()
		{
        }

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the target object.
		/// </summary>
		public object Target
		{
			get { return target; }
			set { target = value; }
		}

        /// <summary>
        /// Gets or sets the <see cref="Spring.Remoting.Support.BaseRemoteObject"/> class or subclass
        /// that the proxy must inherit from.
        /// </summary>
        public Type BaseType
        {
            get { return baseType; }
            set { baseType = value; }
        }

        /// <summary>
        /// Gets or sets the list of interfaces to wrap.
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

		#region IInitializingObject Members

		/// <summary>
		/// Initializes factory object.
		/// </summary>
		public void AfterPropertiesSet()
		{
			ValidateConfiguration();
            GenerateProxy();
		}

		#endregion

		#region IFactoryObject Members

		/// <summary>
		/// Returns type of the remotable target proxy.
		/// </summary>
		public Type ObjectType
		{
			get
			{
				return (proxyConstructor != null) ? proxyConstructor.DeclaringType : target.GetType();
			}
		}

		/// <summary>
		/// Creates new instance of the remotable target proxy.
		/// </summary>
		/// <returns>New instance of the remotable target proxy.</returns>
		public object GetObject()
		{
			if (proxyConstructor == null)
				GenerateProxy();

			return proxyConstructor.Invoke(new object[1]{ target });
		}

		/// <summary>
		/// Always returns false.
		/// </summary>
		public bool IsSingleton
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Private Methods

		private void ValidateConfiguration()
		{
			if (Target == null)
			{
				throw new ArgumentException("The Target property is required.");
			}
            if (!typeof(BaseRemoteObject).IsAssignableFrom(BaseType))
            {
                throw new ArgumentException("The type BaseRemoteObject cannot be assigned from BaseType.");
            }
		}

		private void GenerateProxy()
		{
            IProxyTypeBuilder builder = new RemoteObjectProxyTypeBuilder(this);
            builder.TargetType = target.GetType();
            builder.BaseType = baseType;
            if (interfaces != null && interfaces.Length > 0)
            {
                builder.Interfaces = TypeResolutionUtils.ResolveInterfaceArray(interfaces);
            }

			Type remotableObjectType = builder.BuildProxyType();

			proxyConstructor = remotableObjectType.GetConstructor(new Type[1] { builder.TargetType });
		}

		#endregion
	}
}
