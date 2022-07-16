#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Runtime.Serialization;

using AopAlliance.Intercept;

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Factory for AOP proxies for programmatic use, rather than via a
	/// Spring.NET IoC container.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This class provides a simple way of obtaining and configuring AOP
	/// proxies in code.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
    public class ProxyFactory : AdvisedSupport
	{
	    /// <summary>
		/// Creates a new instance of the <see cref="Spring.Aop.Framework.ProxyFactory"/>
		/// class.
		/// </summary>
		public ProxyFactory()
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Aop.Framework.ProxyFactory"/>
		/// class that proxys all of the interfaces exposed by the supplied
		/// <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The object to proxy.</param>
		/// <exception cref="AopConfigException">
		/// If the <paramref name="target"/> is <cref lang="null"/>.
		/// </exception>
		public ProxyFactory(object target) : base(target)
		{
		}

	    /// <summary>
	    /// Creates a new instance of the <see cref="Spring.Aop.Framework.ProxyFactory"/>
	    /// class that has no target object, only interfaces.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Interceptors must be added if this factory is to do anything useful.
	    /// </p>
	    /// </remarks>
	    /// <param name="interfaces">The interfaces to implement.</param>
	    public ProxyFactory(Type[] interfaces) : base(interfaces)
	    {
	    }

        /// <summary>
        /// Creates a new instance of the <see cref="ProxyFactory"/> class for the
        /// given interface and interceptor.
        /// </summary>
        /// <remarks>Convenience method for creating a proxy for a single interceptor
        /// , assuming that the interceptor handles all calls itself rather than delegating
        /// to a target, like in the case of remoting proxies.</remarks>
        /// <param name="proxyInterface">The interface that the proxy should implement.</param>
        /// <param name="interceptor">The interceptor that the proxy should invoke.</param>
        public ProxyFactory(Type proxyInterface, IInterceptor interceptor)
        {
            AddInterface(proxyInterface);
            AddAdvice(interceptor);
        }


        /// <summary>
        /// Create a new instance of the <see cref="ProxyFactory"/> class for the specified
        /// <see cref="ITargetSource"/> making the proxy implement the specified interface.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="proxyInterface">The interface that the proxy should implement.</param>
        /// <param name="targetSource">The target source that the proxy should invoek.</param>
        public ProxyFactory(Type proxyInterface, ITargetSource targetSource)
        {
            AddInterface(proxyInterface);
            TargetSource = targetSource;
        }

		/// <inheritdoc />
		protected ProxyFactory(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

	    /// <summary>
	    /// Creates a new proxy according to the settings in this factory.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Can be called repeatedly; the effect of repeated invocations will
	    /// (of course) vary if interfaces have been added or removed.
	    /// </p>
	    /// </remarks>
	    /// <returns>An AOP proxy for target object.</returns>
	    public virtual object GetProxy()
	    {
	        IAopProxy proxy = CreateAopProxy();
	        return proxy.GetProxy();
	    }

		/// <summary>
	    /// Creates a new proxy for the supplied <paramref name="proxyInterface"/>
	    /// and <paramref name="interceptor"/>.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// This is a convenience method for creating a proxy for a single
	    /// interceptor.
	    /// </p>
	    /// </remarks>
	    /// <param name="proxyInterface">
	    /// The interface that the proxy must implement.
	    /// </param>
	    /// <param name="interceptor">
	    /// The interceptor that the proxy must invoke.
	    /// </param>
	    /// <returns>
	    /// A new AOP proxy for the supplied <paramref name="proxyInterface"/>
	    /// and <paramref name="interceptor"/>.
	    /// </returns>
	    public static object GetProxy(Type proxyInterface, IInterceptor interceptor)
	    {
	        ProxyFactory proxyFactory = new ProxyFactory();
	        proxyFactory.AddInterface(proxyInterface);
	        proxyFactory.AddAdvice(interceptor);
	        return proxyFactory.GetProxy();
	    }
	}
}
