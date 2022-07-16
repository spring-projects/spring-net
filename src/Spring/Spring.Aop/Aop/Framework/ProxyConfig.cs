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

using System.Runtime.Serialization;
using System.Text;

using Spring.Util;
using Spring.Reflection.Dynamic;

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Convenience superclass for configuration used in creating proxies,
	/// to ensure that all proxy creators have consistent properties.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Note that it is no longer possible to configure subclasses to
	/// expose the <see cref="AopAlliance.Intercept.IMethodInvocation"/>.
	/// Interceptors should normally manage their own thread locals if they
	/// need to make resources available to advised objects. If it is
	/// absolutely necessary to expose the
	/// <see cref="AopAlliance.Intercept.IMethodInvocation"/>, use an
	/// interceptor to do so.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
    public class ProxyConfig : ISerializable
    {
	    private static readonly IDynamicConstructor cachedAopProxyFactoryDynCtor =
            new SafeConstructor(typeof(ProxyConfig).Assembly.GetType("Spring.Aop.Framework.DynamicProxy.CachedAopProxyFactory", false, false).GetConstructor(Type.EmptyTypes));

        private bool proxyTargetType;
	    private bool proxyTargetAttributes = true;
		private bool optimize;
		private bool frozen;

        private IAopProxyFactory aopProxyFactory = cachedAopProxyFactoryDynCtor.Invoke(ObjectUtils.EmptyObjects) as IAopProxyFactory;

		private bool exposeProxy;
        private readonly object syncRoot = new object();

	    /// <inheritdoc />
	    public ProxyConfig()
	    {
	    }

	    /// <inheritdoc />
	    protected ProxyConfig(SerializationInfo info, StreamingContext context)
	    {
		    proxyTargetType = info.GetBoolean("proxyTargetType");
		    proxyTargetAttributes = info.GetBoolean("proxyTargetAttributes");
		    optimize = info.GetBoolean("optimize");
		    frozen = info.GetBoolean("frozen");
		    exposeProxy = info.GetBoolean("exposeProxy");
		    syncRoot = info.GetValue("syncRoot", typeof(object));
	    }

	    /// <inheritdoc />
	    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	    {
		    info.AddValue("proxyTargetType", proxyTargetType);
		    info.AddValue("proxyTargetAttributes", proxyTargetAttributes);
		    info.AddValue("optimize", optimize);
		    info.AddValue("frozen", frozen);
		    info.AddValue("exposeProxy", exposeProxy);
		    info.AddValue("syncRoot", syncRoot);
	    }

	    /// <summary>
        /// Use to synchronize access to this ProxyConfig instance
        /// </summary>
	    public object SyncRoot
	    {
            get { return syncRoot; }
	    }

		/// <summary>
		/// Is the target <see cref="System.Type"/> to be proxied in addition
		/// to any interfaces declared on the proxied <see cref="System.Type"/>?
		/// </summary>
		public virtual bool ProxyTargetType
		{
			get { return this.proxyTargetType; }
			set { this.proxyTargetType = value; }
		}

        /// <summary>
        /// Is target type attributes, method attributes, method's return type attributes
        /// and method's parameter attributes to be proxied in addition
        /// to any interfaces declared on the proxied <see cref="System.Type"/>?
        /// </summary>
        public virtual bool ProxyTargetAttributes
        {
            get { return this.proxyTargetAttributes; }
            set { this.proxyTargetAttributes = value; }
        }

		/// <summary>
		/// Are any <i>agressive optimizations</i> to be performed?
		/// </summary>
		/// <remarks>
		/// <p>
		/// The exact meaning of <i>agressive optimizations</i> will differ
		/// between proxies, but there is usually some tradeoff.
		/// </p>
		/// <p>
		/// For example, optimization will usually mean that advice changes
		/// won't take effect after a proxy has been created. For this reason,
		/// optimization is disabled by default. An optimize value of
		/// <see langword="true"/> may be ignored if other settings preclude
		/// optimization: for example, if the
		/// <see cref="Spring.Aop.Framework.ProxyConfig.ExposeProxy"/> property
		/// is set to <see langword="true"/> and such a value is not compatible
		/// with the optimization.
		/// </p>
		/// <p>
		/// The default is <see langword="false"/>.
		/// </p>
		/// </remarks>
		public virtual bool Optimize
		{
			get { return this.optimize; }
			set { this.optimize = value; }
		}

		/// <summary>
		/// Should proxies obtained from this configuration expose
		/// the AOP proxy to the
		/// <see cref="Spring.Aop.Framework.AopContext"/> class?
		/// </summary>
		/// <remarks>
		/// <p>
		/// The default is <see langword="false"/>, as enabling this property
		/// may impair performance.
		/// </p>
		/// </remarks>
		public bool ExposeProxy
		{
			get { return this.exposeProxy; }
			set { this.exposeProxy = value; }
		}

		/// <summary>
		/// Gets and set the factory to be used to create AOP proxies.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This obviously allows one to customise the
		/// <see cref="Spring.Aop.Framework.IAopProxyFactory"/> implementation,
		/// allowing different strategies to be dropped in without changing the
		/// core framework. For example, an
		/// <see cref="Spring.Aop.Framework.IAopProxyFactory"/> implementation
		/// could return an <see cref="Spring.Aop.Framework.IAopProxy"/>
		/// using remoting proxies, <c>Reflection.Emit</c> or a code generation
		/// strategy.
		/// </p>
		/// </remarks>
		public virtual IAopProxyFactory AopProxyFactory
		{
			get { return this.aopProxyFactory; }
			set { this.aopProxyFactory = value; }
		}

		/// <summary>
		/// Is this configuration frozen?
		/// </summary>
		/// <remarks>
		/// <p>
		/// The default is not frozen.
		/// </p>
		/// </remarks>
		public virtual bool IsFrozen
		{
			get { return this.frozen; }
			set { this.frozen = value; }
        }

	    /// <summary>
		/// Copies the configuration from the supplied
		/// <paramref name="otherConfiguration"/> into this instance.
		/// </summary>
		/// <param name="otherConfiguration">
		/// The configuration to be copied.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="otherConfiguration"/> is
		/// <see langword="null"/>.
		/// </exception>
		public virtual void CopyFrom(ProxyConfig otherConfiguration)
		{
			AssertUtils.ArgumentNotNull(otherConfiguration, "otherConfiguration");

			this.optimize = otherConfiguration.optimize;
			this.proxyTargetType = otherConfiguration.proxyTargetType;
            this.proxyTargetAttributes = otherConfiguration.proxyTargetAttributes;
			this.exposeProxy = otherConfiguration.exposeProxy;
			this.frozen = otherConfiguration.frozen;
			this.aopProxyFactory = otherConfiguration.aopProxyFactory;
		}

		/// <summary>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="Spring.Aop.Framework.ProxyConfig"/> configuration.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="Spring.Aop.Framework.ProxyConfig"/> configuration.
		/// </returns>
		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();
            buffer.Append("proxyTargetType=" + ProxyTargetType + "; ");
            buffer.Append("proxyTargetAttributes=" + ProxyTargetAttributes + "; ");
			buffer.Append("exposeProxy=" + ExposeProxy + "; ");
			buffer.Append("isFrozen=" + IsFrozen + "; ");
			buffer.Append("optimize=" + Optimize + "; ");
			buffer.Append("aopProxyFactory=" + AopProxyFactory.GetType().FullName + "; ");
			return buffer.ToString();
		}
	}
}
