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

using System.Collections;
using System.Runtime.Serialization;

using AopAlliance.Aop;

using Spring.Util;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Aop.Framework.Adapter;
using Spring.Aop.Framework.DynamicProxy;
using Spring.Core;

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// <see cref="IObjectFactoryPostProcessor"/> implementation that replaces a group of objects
    /// with a 'true' inheritance based AOP mechanism that delegates
    /// to the given interceptors before invoking the object itself.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class InheritanceBasedAopConfigurer : IObjectFactoryPostProcessor, IObjectFactoryAware, IOrdered
    {
        private IList objectNames;
        private string[] interceptorNames = new string[0];
        private bool proxyTargetAttributes = true;
        private bool proxyDeclaredMembersOnly = true;
        private bool proxyInterfaces = false;

        private IObjectFactory objectFactory;
        private int order = int.MaxValue;
        private IAdvisorAdapterRegistry advisorAdapterRegistry = GlobalAdvisorAdapterRegistry.Instance;

        /// <summary>
        /// Set the names of the objects in IList fashioned way
        /// that should automatically get intercepted.
        /// </summary>
        /// <remarks>
        /// A name can specify a prefix to match by ending with "*", e.g. "myObject,tx*"
        /// will match the object named "myObject" and all objects whose name start with "tx".
        /// </remarks>
        public virtual IList ObjectNames
        {
            set { objectNames = value; }
        }

        /// <summary>
        /// Sets the common interceptors, a list of <see cref="AopAlliance.Aop.IAdvice"/>,
        /// <see cref="Spring.Aop.IAdvisor"/> and introduction object names.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property isn't set, there will be zero common interceptors.
        /// This is perfectly valid, if "specific" interceptors such as
        /// matching Advisors are all we want.
        /// </para>
        /// </remarks>
        /// <value>
        /// The list of <see cref="AopAlliance.Aop.IAdvice"/>,
        /// <see cref="Spring.Aop.IAdvisor"/> and introduction object names.
        /// </value>
        /// <seealso cref="AopAlliance.Aop.IAdvice"/>
        /// <seealso cref="Spring.Aop.IAdvisor"/>
        public virtual string[] InterceptorNames
        {
            set { interceptorNames = value; }
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
        /// Gets or sets a value indicating whether inherited members should be proxied.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if inherited members should be proxied;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool ProxyDeclaredMembersOnly
        {
            get { return proxyDeclaredMembersOnly; }
            set { proxyDeclaredMembersOnly = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether interfaces members should be proxied.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if interfaces members should be proxied;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool ProxyInterfaces
        {
            get { return proxyInterfaces; }
            set { proxyInterfaces = value; }
        }

        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        /// <value>
        /// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// (may not be <see langword="null"/>). The object can immediately
        /// call methods on the factory.
        /// </value>
        /// <remarks>
        /// <p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of initialization errors.
        /// </exception>
        public IObjectFactory ObjectFactory
        {
            set { objectFactory = value; }
        }

        /// <summary>
        /// Allows for custom modification of an application context's object definitions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the object name matches, replaces the type by a AOP proxied type based on inheritance.
        /// </p>
        /// </remarks>
        public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
        {
            var objectDefinitionNames = factory.GetObjectDefinitionNames();
            for (int i = 0; i < objectDefinitionNames.Count; ++i)
            {
                string name = objectDefinitionNames[i];
                if (IsObjectNameMatch(name))
                {
                    var definition = factory.GetObjectDefinition(name) as IConfigurableObjectDefinition;

                    if (definition == null || IsInfrastructureType(definition.ObjectType, name))
                    {
                        continue;
                    }

                    ProxyFactory pf = CreateProxyFactory(definition.ObjectType, name);

                    InheritanceAopProxyTypeBuilder iaptb = new InheritanceAopProxyTypeBuilder(pf);
                    iaptb.ProxyDeclaredMembersOnly = this.ProxyDeclaredMembersOnly;
                    Type type = iaptb.BuildProxyType();

                    definition.ObjectType = type;
                    definition.ConstructorArgumentValues.AddIndexedArgumentValue(definition.ConstructorArgumentValues.ArgumentCount, pf);
                }
            }
        }

        /// <summary>
        /// Return the order value of this object, where a higher value means greater in
        /// terms of sorting.
        /// </summary>
        /// <remarks>
        /// Ordering which will apply to this class's implementation
        /// of Ordered, used when applying multiple ObjectPostProcessors.
        /// Default value is int.MaxValue, meaning that it's non-ordered.
        /// </remarks>
        /// <returns>The order value.</returns>
        public virtual int Order
        {
            get { return order; }
            set { order = value; }
        }

        /// <summary>
        /// Determines whether the object is an infrastructure type,
        /// IAdvisor, IAdvice, IAdvisors, AbstractAutoProxyCreator or InheritanceBasedAopConfigurer.
        /// </summary>
        /// <param name="type">The object type to compare</param>
        /// <param name="name">The name of the object</param>
        /// <returns>
        /// <c>true</c> if [is infrastructure type] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsInfrastructureType(Type type, String name)
        {
            return typeof(IAdvisor).IsAssignableFrom(type)
                || typeof(IAdvice).IsAssignableFrom(type)
                || typeof(IAdvisors).IsAssignableFrom(type)
                || typeof(AbstractAutoProxyCreator).IsAssignableFrom(type)
                || typeof(InheritanceBasedAopConfigurer).IsAssignableFrom(type);
        }

        /// <summary>
        /// Create an AOP proxy for the given object.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <returns>The AOP Proxy for the object.</returns>
        protected virtual ProxyFactory CreateProxyFactory(Type objectType, string objectName)
        {
            ProxyFactory proxyFactory = new ProxyFactory();
            proxyFactory.ProxyTargetAttributes = this.ProxyTargetAttributes;
            proxyFactory.TargetSource = new InheritanceBasedAopTargetSource(objectType);
            if (!ProxyInterfaces)
            {
                proxyFactory.Interfaces = Type.EmptyTypes;
            }

            IList<IAdvisor> advisors = ResolveInterceptorNames();
            foreach (IAdvisor advisor in advisors)
            {
                if (advisor is IIntroductionAdvisor)
                {
                    proxyFactory.AddIntroduction((IIntroductionAdvisor)advisor);
                }
                else
                {
                    proxyFactory.AddAdvisor(advisor);
                }
            }

            return proxyFactory;
        }

        /// <summary>
        /// Return if the given object name matches one of the object names specified.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default implementation checks for "xxx*", "*xxx" and "*xxx*" matches,
        /// as well as direct equality. Can be overridden in subclasses.
        /// </p>
        /// </remarks>
        /// <param name="objectName">the object name to check</param>
        /// <returns>if the names match</returns>
        protected virtual bool IsObjectNameMatch(string objectName)
        {
            if (objectNames != null)
            {
                for (int i = 0; i < objectNames.Count; i++)
                {
                    string mappedName = String.Copy((string)objectNames[i]);
                    if (PatternMatchUtils.SimpleMatch(mappedName, objectName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private IList<IAdvisor> ResolveInterceptorNames()
        {
            List<IAdvisor> advisors = new List<IAdvisor>();
            foreach (string name in interceptorNames)
            {
                object next = objectFactory.GetObject(name);
                if (next is IAdvisors)
                {
                    advisors.AddRange(((IAdvisors)next).Advisors);
                }
                else
                {
                    advisors.Add(advisorAdapterRegistry.Wrap(next));
                }
            }
            return advisors;
        }

        [Serializable]
        private class InheritanceBasedAopTargetSource : ITargetSource, ISerializable
        {
            private readonly Type _targetType;

            public InheritanceBasedAopTargetSource(Type targetType)
            {
                _targetType = targetType;
            }


            /// <inheritdoc />
            private InheritanceBasedAopTargetSource(SerializationInfo info, StreamingContext context)
            {
                var type = info.GetString("TargetType");
                _targetType = type != null ? Type.GetType(type) : null;
            }

            /// <inheritdoc />
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("TargetType", _targetType?.AssemblyQualifiedName);
            }

            public object GetTarget()
            {
                return null;
            }

            public bool IsStatic
            {
                get { return true; }
            }

            public void ReleaseTarget(object target)
            {
            }

            public Type TargetType
            {
                get { return _targetType; }
            }
        }
    }
}
