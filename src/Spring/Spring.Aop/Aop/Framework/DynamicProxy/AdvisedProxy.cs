/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AopAlliance.Aop;
using AopAlliance.Intercept;
using Spring.Util;

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Represents the AOP configuration data built-in with the proxy.
    /// </summary>
    /// <author>Bruno Baia</author>
    [Serializable]
    public class AdvisedProxy : IAdvised, ISerializable
    {
        /// <summary>
        /// Should we use dynamic reflection for method invocation ?
        /// </summary>
        public static bool UseDynamicReflection;

        /// <summary>
        /// Optimization fields
        /// </summary>
        private static IList<object> EmptyList = new ReadOnlyCollection<object>(new object[0]);

        /// <summary>
        /// IAdvised delegate
        /// </summary>
        public IAdvised m_advised;

        /// <summary>
        /// Array of introduction delegates
        /// </summary>
        public IAdvice[] m_introductions;

        /// <summary>
        /// Target source
        /// </summary>
        public ITargetSource m_targetSource;

        /// <summary>
        /// Type of target object.
        /// </summary>
        public Type m_targetType;

        /// <summary>
        /// Creates a new instance of the <see cref="AdvisedProxy"/> class.
        /// </summary>
        static AdvisedProxy()
        {
            string appSettingsKey = typeof(AdvisedProxy).FullName + ".UseDynamicReflection";
            NameValueCollection appSettings =
                ConfigurationUtils.GetSection("appSettings") as NameValueCollection;

            if (appSettings != null && StringUtils.HasLength(appSettings[appSettingsKey]))
            {
                UseDynamicReflection = bool.Parse(appSettings[appSettingsKey]);
            }
            else
            {
                UseDynamicReflection = true;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AdvisedProxy"/> class.
        /// </summary>
        public AdvisedProxy()
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="AdvisedProxy"/> class.
        /// </summary>
        /// <param name="advised">The proxy configuration.</param>
        protected AdvisedProxy(IAdvised advised)
        {
            m_advised = advised;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.DynamicProxy.AdvisedProxy"/> class.
        /// </summary>
        /// <param name="advised">The proxy configuration.</param>
        /// <param name="proxy">The proxy.</param>
        public AdvisedProxy(IAdvised advised, IAopProxy proxy)
        {
            Initialize(advised, proxy);
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization data.</param>
        /// <param name="context">Serialization context.</param>
        protected AdvisedProxy(SerializationInfo info, StreamingContext context)
        {
            m_advised = (IAdvised)info.GetValue("advised", typeof(IAdvised));
            m_introductions = (IAdvice[])info.GetValue("introductions", typeof(IAdvice[]));
            m_targetSource = (ITargetSource)info.GetValue("targetSource", typeof(ITargetSource));

            var type = info.GetString("targetType");
            m_targetType = type != null ? Type.GetType(type) : null;
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <param name="info">Serialization data.</param>
        /// <param name="context">Serialization context.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("advised", m_advised);
            info.AddValue("introductions", m_introductions);
            info.AddValue("targetSource", m_targetSource);
            info.AddValue("targetType", m_targetType?.AssemblyQualifiedName);
        }

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="advised">The proxy configuration.</param>
        /// <param name="proxy">
        /// The current <see cref="Spring.Aop.Framework.IAopProxy"/> implementation.
        /// </param>
        protected void Initialize(IAdvised advised, IAopProxy proxy)
        {
            this.m_advised = advised;
            this.m_targetSource = advised.TargetSource;
            this.m_targetType = advised.TargetSource.TargetType;

            // initialize introduction advice
            this.m_introductions = new IAdvice[advised.Introductions.Count];
            for (int i = 0; i < advised.Introductions.Count; i++)
            {
                this.m_introductions[i] = advised.Introductions[i].Advice;

                // set target proxy on introduction instance if it implements ITargetAware
                if (this.m_introductions[i] is ITargetAware)
                {
                    ((ITargetAware)this.m_introductions[i]).TargetProxy = proxy;
                }
            }
        }

        /// <summary>
        /// Invokes intercepted methods using reflection
        /// </summary>
        /// <param name="proxy">proxy object</param>
        /// <param name="target">target object to invoke method on</param>
        /// <param name="targetType">target type</param>
        /// <param name="targetMethod">taget method to invoke</param>
        /// <param name="proxyMethod">The method to invoke on proxy.</param>
        /// <param name="args">method arguments</param>
        /// <param name="interceptors">interceptor chain</param>
        /// <returns>value returned by invocation chain</returns>
        public object Invoke(object proxy, object target, Type targetType,
            MethodInfo targetMethod, MethodInfo proxyMethod, object[] args, IList interceptors)
        {
            IMethodInvocation invocation = null;
            if (UseDynamicReflection)
            {
                invocation = new DynamicMethodInvocation(
                    proxy, target, targetMethod, proxyMethod, args, targetType, interceptors);
            }
            else
            {
                invocation = new ReflectiveMethodInvocation(
                    proxy, target, targetMethod, proxyMethod, args, targetType, interceptors);
            }
            return invocation.Proceed();
        }

        /// <summary>
        /// Returns a list of method interceptors
        /// </summary>
        /// <param name="targetType">target type</param>
        /// <param name="method">target method</param>
        /// <returns>list of inteceptors for the specified method</returns>
        public IList<object> GetInterceptors(Type targetType, MethodInfo method)
        {
            if (m_advised.Advisors.Count == 0)
            {
                return EmptyList;
            }
            else
            {
                return m_advised.AdvisorChainFactory.GetInterceptors(m_advised, this, method, targetType);
            }
        }

        bool IAdvised.ExposeProxy
        {
            get { return m_advised.ExposeProxy; }
        }

        IAdvisorChainFactory IAdvised.AdvisorChainFactory
        {
            get { return m_advised.AdvisorChainFactory; }
        }

        bool IAdvised.ProxyTargetType
        {
            get { return m_advised.ProxyTargetType; }
        }

        bool IAdvised.ProxyTargetAttributes
        {
            get { return m_advised.ProxyTargetAttributes; }
        }

        IList<IAdvisor> IAdvised.Advisors
        {
            get { return m_advised.Advisors; }
        }

        IList<IIntroductionAdvisor> IAdvised.Introductions
        {
            get { return m_advised.Introductions; }
        }

        IList<Type> IAdvised.Interfaces
        {
            get { return m_advised.Interfaces; }
        }

        IDictionary<Type, object> IAdvised.InterfaceMap
        {
            get { return m_advised.InterfaceMap; }
        }

        bool IAdvised.IsFrozen
        {
            get { return m_advised.IsFrozen; }
        }

        ITargetSource IAdvised.TargetSource
        {
            get { return m_advised.TargetSource; }
        }

        bool IAdvised.IsSerializable
        {
            get { return m_advised.IsSerializable; }
        }

        /// <summary>
        /// Adds the supplied <paramref name="advice"/> to the end (or tail)
        /// of the advice (interceptor) chain.
        /// </summary>
        /// <param name="advice">
        /// The <see cref="AopAlliance.Aop.IAdvice"/> to be added.
        /// </param>
        /// <seealso cref="Spring.Aop.Support.DefaultPointcutAdvisor"/>
        /// <seealso cref="Spring.Aop.Framework.IAdvised.AddAdvice(int,IAdvice)"/>
        public void AddAdvice(IAdvice advice)
        {
            this.m_advised.AddAdvice(advice);
        }

        /// <summary>
        /// Adds the supplied <paramref name="advice"/> to the supplied
        /// <paramref name="position"/> in the advice (interceptor) chain.
        /// </summary>
        /// <param name="position">
        /// The zero (0) indexed position (from the head) at which the
        /// supplied <paramref name="advice"/> is to be inserted into the
        /// advice (interceptor) chain.
        /// </param>
        /// <param name="advice">
        /// The <see cref="AopAlliance.Aop.IAdvice"/> to be added.
        /// </param>
        /// <seealso cref="Spring.Aop.Support.DefaultPointcutAdvisor"/>
        /// <seealso cref="Spring.Aop.Framework.IAdvised.AddAdvice(IAdvice)"/>
        public void AddAdvice(int position, IAdvice advice)
        {
            this.m_advised.AddAdvice(position, advice);
        }

        bool IAdvised.IsInterfaceProxied(Type intf)
        {
            return m_advised.IsInterfaceProxied(intf);
        }

        void IAdvised.AddAdvisors(IAdvisors advisors)
        {
            m_advised.AddAdvisors(advisors);
        }

        void IAdvised.AddAdvisor(IAdvisor advisor)
        {
            m_advised.AddAdvisor(advisor);
        }

        void IAdvised.AddAdvisor(int pos, IAdvisor advisor)
        {
            m_advised.AddAdvisor(pos, advisor);
        }

        void IAdvised.AddIntroduction(IIntroductionAdvisor advisor)
        {
            m_advised.AddIntroduction(advisor);
        }

        void IAdvised.AddIntroduction(int pos, IIntroductionAdvisor advisor)
        {
            m_advised.AddIntroduction(pos, advisor);
        }

        int IAdvised.IndexOf(IAdvisor advisor)
        {
            return m_advised.IndexOf(advisor);
        }

        int IAdvised.IndexOf(IIntroductionAdvisor advisor)
        {
            return m_advised.IndexOf(advisor);
        }

        bool IAdvised.RemoveAdvisor(IAdvisor advisor)
        {
            return m_advised.RemoveAdvisor(advisor);
        }

        void IAdvised.RemoveAdvisor(int index)
        {
            m_advised.RemoveAdvisor(index);
        }

        bool IAdvised.RemoveAdvice(IAdvice advice)
        {
            return m_advised.RemoveAdvice(advice);
        }

        bool IAdvised.RemoveIntroduction(IIntroductionAdvisor advisor)
        {
            return m_advised.RemoveIntroduction(advisor);
        }

        void IAdvised.RemoveIntroduction(int index)
        {
            m_advised.RemoveIntroduction(index);
        }

        void IAdvised.ReplaceIntroduction(int index, IIntroductionAdvisor advisor)
        {
            m_advised.ReplaceIntroduction(index, advisor);
        }

        bool IAdvised.ReplaceAdvisor(IAdvisor a, IAdvisor b)
        {
            return m_advised.ReplaceAdvisor(a, b);
        }

        string IAdvised.ToProxyConfigString()
        {
            return m_advised.ToProxyConfigString();
        }

        /// <summary>
        /// Gets the target type behind the implementing object.
        /// Ttypically a proxy configuration or an actual proxy.
        /// </summary>
        /// <value>The type of the target or null if not known.</value>
        public Type TargetType
        {
            get { return m_targetType; }
        }
    }
}
