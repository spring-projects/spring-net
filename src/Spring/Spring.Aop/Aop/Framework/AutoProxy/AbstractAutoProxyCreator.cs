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
using System.Reflection;

using AopAlliance.Aop;

using Common.Logging;

using Spring.Aop.Framework.Adapter;
using Spring.Aop.Target;
using Spring.Collections;
using Spring.Core;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// ObjectPostProcessor implementation that wraps a group of objects with AOP proxies
    /// that delegate to the given interceptors before invoking the object itself.
    /// </summary>
    /// <remarks>
    /// <p>This class distinguishes between "common" interceptors: shared for all proxies it
    /// creates, and "specific" interceptors: unique per object instance. There need not
    /// be any common interceptors. If there are, they are set using the interceptorNames
    /// property. As with ProxyFactoryObject, interceptors names in the current factory
    /// are used rather than object references to allow correct handling of prototype
    /// advisors and interceptors: for example, to support stateful mixins.
    /// Any advice type is supported for "interceptorNames" entries.</p>
    /// <p>Such autoproxying is particularly useful if there's a large number of objects that need
    /// to be wrapped with similar proxies, i.e. delegating to the same interceptors.
    /// Instead of x repetitive proxy definitions for x target objects, you can register
    /// one single such post processor with the object factory to achieve the same effect.</p>
    /// <p>Subclasses can apply any strategy to decide if a object is to be proxied,
    /// e.g. by type, by name, by definition details, etc. They can also return
    /// additional interceptors that should just be applied to the specific object
    /// instance. The default concrete implementation is ObjectNameAutoProxyCreator,
    /// identifying the objects to be proxied via a list of object names.</p>
    /// <p>Any number of TargetSourceCreator implementations can be used with any subclass,
    /// to create a custom target source - for example, to pool prototype objects.
    /// Autoproxying will occur even if there is no advice if a TargetSourceCreator specifies
    /// a custom TargetSource. If there are no TargetSourceCreators set, or if none matches,
    /// a SingletonTargetSource will be used by default to wrap the object to be autoproxied.</p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rod Johnson</author>
    /// <author>Adhari C Mahendra (.NET)</author>
    /// <seealso cref="Spring.Aop.Framework.AutoProxy.AbstractAutoProxyCreator.InterceptorNames"/>
    /// <seealso cref="Spring.Aop.Framework.AutoProxy.ObjectNameAutoProxyCreator"/>
    public abstract class AbstractAutoProxyCreator : ProxyConfig, IInstantiationAwareObjectPostProcessor, IObjectFactoryAware, IOrdered
    {
        /// <summary>
        /// The logger for this class hierarchy.
        /// </summary>
        protected readonly ILog logger;

        /// <summary>
        /// Convenience constant for subclasses: Return value for "do not proxy".
        /// </summary>
        protected static readonly IList<object> DO_NOT_PROXY = null;

        /// <summary>
        /// Convenience constant for subclasses: Return value for
        /// "proxy without additional interceptors, just the common ones".
        /// </summary>
        protected static readonly IList<object> PROXY_WITHOUT_ADDITIONAL_INTERCEPTORS = new List<object>(0);

        /// <summary>
        /// Default value is same as non-ordered
        /// </summary>
        private int order = int.MaxValue;

        /// <summary>
        /// Default is global AdvisorAdapterRegistry
        /// </summary>
        private IAdvisorAdapterRegistry advisorAdapterRegistry = GlobalAdvisorAdapterRegistry.Instance;

        /// <summary>
        /// Indicates whether to mark the create proxy as immutable.
        /// </summary>
        /// <remarks>
        /// Setting this to true effectively disables  modifying the generated
        /// proxy's advisor configuration
        /// </remarks>
        private bool freezeProxy = false;

        /// <summary>
        /// Names of common interceptors.
        /// We must use object name rather than object references
        /// to handle prototype advisors/interceptors.
        /// Default is the empty array: no common interceptors.
        /// </summary>
        private string[] interceptorNames = new string[0];

        private bool applyCommonInterceptorsFirst = true;
        private IList customTargetSourceCreators = new ArrayList();
        private IObjectFactory owningObjectFactory;

        /// <summary>
        /// Set of object type + name strings, referring to all objects that this auto-proxy
        /// creator created a custom TargetSource for.  Used to detect own pre-built proxies
        /// (from "PostProcessBeforeInstantiation") in the "PostProcessAfterInitialization" method.
        /// </summary>
        private ISet targetSourcedObjects = new SynchronizedSet(new HashedSet());

        private ISet advisedObjects = new SynchronizedSet(new HashedSet());

        private ISet nonAdvisedObjects = new SynchronizedSet(new HashedSet());

        /// <summary>
        /// Sets the AdvisorAdapterRegistry to use.
        /// </summary>
        /// <remarks>
        /// Default is the global AdvisorAdapterRegistry.
        /// </remarks>
        public IAdvisorAdapterRegistry AdvisorAdapterRegistry
        {
            set { advisorAdapterRegistry = value; }
        }

        /// <summary>
        /// Sets custom TargetSourceCreators to be applied in this order.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the list is empty, or they all return null, a SingletonTargetSource
        /// will be created.
        /// </para>
        /// <para>
        /// TargetSourceCreators can only be invoked if this post processor is used
        /// in a IObjectFactory, and its ObjectFactoryAware callback is used.
        /// </para>
        /// </remarks>
        public IList CustomTargetSourceCreators
        {
            set { customTargetSourceCreators = value; }
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
        public string[] InterceptorNames
        {
            set { interceptorNames = value; }
        }

        /// <summary>
        /// Sets whether the common interceptors should be applied before
        /// object-specific ones.
        /// </summary>
        /// <remarks>
        /// Default is true; else, object-specific interceptors will get applied first.
        /// </remarks>
        public bool ApplyCommonInterceptorsFirst
        {
            set { applyCommonInterceptorsFirst = value; }
        }

        /// <summary>
        /// Set whether or not the proxy should be frozen, preventing advice
        /// from being added to it once it is created.
        /// </summary>
        /// <remarks>
        /// 	<p>Overridden from the super class to prevent the proxy configuration
        /// from being frozen before the proxy is created. The default is not frozen.
        ///     </p>
        /// </remarks>
        public override bool IsFrozen
        {
            get { return freezeProxy; }
            set { this.freezeProxy = value; }
        }

        /// <summary>
        /// Create a new instance of this AutoProxyCreator
        /// </summary>
        protected AbstractAutoProxyCreator()
        {
            logger = LogManager.GetLogger(this.GetType());
        }

        /// <summary>
        /// Create a proxy with the configured interceptors if the object is
        /// identified as one to proxy by the subclass.
        /// </summary>
        public virtual object PostProcessAfterInitialization(object obj, string objectName)
        {
            if (targetSourcedObjects.Contains(objectName))
            {
                return obj;
            }

#if NETSTANDARD
            var isTransparentProxy = false;
#else
            var isTransparentProxy = System.Runtime.Remoting.RemotingServices.IsTransparentProxy(obj);
#endif

            Type objectType = isTransparentProxy
                ? ObjectFactory.GetType(objectName)
                : obj.GetType();

            object cacheKey = GetCacheKey(objectType, objectName);
            if (nonAdvisedObjects.Contains(cacheKey))
            {
                return obj;
            }

            if (IsInfrastructureType(objectType, objectName))
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(string.Format("Did not attempt to autoproxy infrastructure type [{0}]", objectType));
                }

                nonAdvisedObjects.Add(cacheKey);
                return obj;
            }

            if (ShouldSkip(objectType, objectName))
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(string.Format("Skipping  type [{0}]", objectType));
                }

                nonAdvisedObjects.Add(cacheKey);
                return obj;
            }

            //ITargetSource targetSource = GetCustomTargetSource(obj.GetType(), objectName);
            IList<object> specificInterceptors = GetAdvicesAndAdvisorsForObject(objectType, objectName, null);


            // proxy if we have advice or if a TargetSourceCreator wants to do some
            // fancy stuff such as pooling
            if (specificInterceptors != DO_NOT_PROXY)
            {
                advisedObjects.Add(cacheKey);
                return CreateProxy(objectType, objectName, specificInterceptors, new SingletonTargetSource(obj, objectType));
            }
            nonAdvisedObjects.Add(cacheKey);
            return obj;
        }

        /// <summary>
        /// No-op for before initialization.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual object PostProcessBeforeInitialization(object obj, string name)
        {
            return obj;
        }

        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        /// <value>
        /// Owning <see cref="T:Spring.Objects.Factory.IObjectFactory"/>
        /// (may not be <see langword="null"/>). The object can immediately
        /// call methods on the factory.
        /// </value>
        /// <remarks>
        /// 	<p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="T:Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="M:Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        /// <exception cref="T:Spring.Objects.ObjectsException">
        /// In case of initialization errors.
        /// </exception>
        public virtual IObjectFactory ObjectFactory
        {
            get { return owningObjectFactory; }
            set { owningObjectFactory = value; }
        }

        /// <summary>
        /// Propery Order
        /// </summary>
        /// <remarks>
        /// Ordering which will apply to this class's implementation
        /// of Ordered, used when applying multiple ObjectPostProcessors.
        /// Default value is int.MaxValue, meaning that it's non-ordered.
        /// </remarks>
        public virtual int Order
        {
            get { return order; }
            set { order = value; }
        }

        /// <summary>
        /// Subclasses should override this method to return true if this
        /// object should not be considered for autoproxying by this post processor.
        /// Sometimes we need to be able to avoid this happening if it will lead to
        /// a circular reference. This implementation returns false.
        /// </summary>
        /// <param name="targetType">the type of the object</param>
        /// <param name="targetName">the name of the object</param>
        /// <returns>if remarkable to skip</returns>
        protected virtual bool ShouldSkip(Type targetType, string targetName)
        {
            return false;
        }

        /// <summary>
        /// Subclasses may choose to implement this: for example,
        /// to change the interfaces exposed
        /// </summary>
        /// <param name="pf">
        /// ProxyFactory that will be used to create the proxy immediably after this method returns.
        /// </param>
        protected virtual void CustomizeProxyFactory(ProxyFactory pf)
        {
            // This implementation does nothing
        }

        /// <summary>
        /// Determines whether the object is an infrastructure type,
        /// IAdvisor, IAdvice, IAdvisors or AbstractAutoProxyCreator
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
                || typeof(AbstractAutoProxyCreator).IsAssignableFrom(type);
        }


        /// <summary>
        /// Create a target source for object instances. Uses any
        /// TargetSourceCreators if set. Returns null if no Custom TargetSource
        /// should be used.
        /// This implementation uses the customTargetSourceCreators property.
        /// Subclasses can override this method to use a different mechanism.
        /// </summary>
        /// <param name="objectType">the type of the object to create a TargetSource for</param>
        /// <param name="name">the name of the object</param>
        /// <returns>a TargetSource for this object</returns>
        protected virtual ITargetSource GetCustomTargetSource(Type objectType, string name)
        {
            // We can't create fancy target sources for directly registered singletons.
            if (customTargetSourceCreators != null &&
                owningObjectFactory != null && owningObjectFactory.ContainsObject(name))
            {
                for (int i = 0; i < customTargetSourceCreators.Count; i++)
                {
                    ITargetSourceCreator tsc = (ITargetSourceCreator)customTargetSourceCreators[i];
                    ITargetSource ts = tsc.GetTargetSource(objectType, name, owningObjectFactory);
                    if (ts != null)
                    {
                        // found a match
                        if (logger.IsInfoEnabled)
                        {
                            logger.Info(string.Format("TargetSourceCreator [{0} found custom TargetSource for object with objectName '{1}'", tsc, name));
                        }
                        return ts;
                    }
                }
            }

            // no custom TargetSource found
            return null;
        }

        /// <summary>
        /// Return whether the given object is to be proxied, what additional
        /// advices (e.g. AOP Alliance interceptors) and advisors to apply.
        /// </summary>
        /// <remarks>
        /// <p>The previous targetName of this method was "GetInterceptorAndAdvisorForObject".
        /// It has been renamed in the course of general terminology clarification
        /// in Spring 1.1. An AOP Alliance Interceptor is just a special form of
        /// Advice, so the generic Advice term is preferred now.</p>
        /// <p>The third parameter, customTargetSource, is new in Spring 1.1;
        /// add it to existing implementations of this method.</p>
        /// </remarks>
        /// <param name="targetType">the new object instance</param>
        /// <param name="targetName">the name of the object</param>
        /// <param name="customTargetSource">targetSource returned by TargetSource property:
        ///   may be ignored. Will be null unless a custom target source is in use.</param>
        /// <returns>an array of additional interceptors for the particular object;
        /// or an empty array if no additional interceptors but just the common ones;
        /// or null if no proxy at all, not even with the common interceptors.</returns>
        protected abstract IList<object> GetAdvicesAndAdvisorsForObject(Type targetType, string targetName, ITargetSource customTargetSource);

        /// <summary>
        /// Create an AOP proxy for the given object.
        /// </summary>
        /// <param name="targetType">Type of the object.</param>
        /// <param name="targetName">The name of the object.</param>
        /// <param name="specificInterceptors">The set of interceptors that is specific to this
        ///   object (may be empty but not null)</param>
        /// <param name="targetSource">The target source for the proxy, already pre-configured to access the object.</param>
        /// <returns>The AOP Proxy for the object.</returns>
        protected virtual object CreateProxy(Type targetType, string targetName, IList<object> specificInterceptors, ITargetSource targetSource)
        {
            ProxyFactory proxyFactory = CreateProxyFactory();
            // copy our properties (proxyTargetClass) inherited from ProxyConfig
            proxyFactory.CopyFrom(this);

            object target = targetSource.GetTarget();


            if (!ProxyTargetType)
            {
                // Must allow for introductions; can't just set interfaces to
                // the target's interfaces only.
                Type[] targetInterfaceTypes = AopUtils.GetAllInterfacesFromType(targetType);
                foreach (Type interfaceType in targetInterfaceTypes)
                {
                    proxyFactory.AddInterface(interfaceType);
                }
            }


            IList<IAdvisor> advisors = BuildAdvisors(targetName, specificInterceptors);

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
            proxyFactory.TargetSource = targetSource;
            CustomizeProxyFactory(proxyFactory);

            proxyFactory.IsFrozen = freezeProxy;
            return proxyFactory.GetProxy();
        }

        /// <summary>
        /// Obtain a new proxy factory instance to be used for proxying a particular object
        /// </summary>
        /// <returns>A proxy factory instance for proxying a particular object</returns>
        protected virtual ProxyFactory CreateProxyFactory()
        {
            return new ProxyFactory();
        }

        /// <summary>
        /// Determines the advisors for the given object, including the specific interceptors
        /// as well as the common interceptor, all adapted to the Advisor interface.
        /// </summary>
        /// <param name="targetName">The name of the object.</param>
        /// <param name="specificInterceptors">The set of interceptors that is specific to this
        ///   object (may be empty, but not null)</param>
        /// <returns>The list of Advisors for the given object</returns>
        protected virtual IList<IAdvisor> BuildAdvisors(string targetName, IList<object> specificInterceptors)
        {
            // handle prototypes correctly
            IList<IAdvisor> commonInterceptors = ResolveInterceptorNames();

            List<object> allInterceptors = new List<object>();
            if (specificInterceptors != null)
            {
                allInterceptors.AddRange(specificInterceptors);
                if (commonInterceptors != null)
                {
                    if (applyCommonInterceptorsFirst)
                    {
                        allInterceptors.InsertRange(0, commonInterceptors.Cast<object>());
                    }
                    else
                    {
                        allInterceptors.AddRange(commonInterceptors.Cast<object>());
                    }
                }
            }
            if (logger.IsInfoEnabled)
            {
                int nrOfCommonInterceptors = commonInterceptors != null ? commonInterceptors.Count : 0;
                int nrOfSpecificInterceptors = specificInterceptors != null ? specificInterceptors.Count : 0;
                logger.Info(string.Format("Creating implicit proxy for object '{0}' with {1} common interceptors and {2} specific interceptors", targetName, nrOfCommonInterceptors, nrOfSpecificInterceptors));
            }


            List<IAdvisor> advisors = new List<IAdvisor>(allInterceptors.Count);
            for (int i = 0; i < allInterceptors.Count; i++)
            {
                advisors.Add(advisorAdapterRegistry.Wrap(allInterceptors[i]));
            }
            return advisors;
        }

        /// <summary>
        /// Build a cache key for the given object type and object name
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectName">The object name.</param>
        /// <returns>The cache key for the given type and name</returns>
        protected virtual object GetCacheKey(Type objectType, string objectName)
        {
            return objectType.FullName + "_" + objectName;
        }


        private IList<IAdvisor> ResolveInterceptorNames()
        {
            List<IAdvisor> advisors = new List<IAdvisor>();
            foreach (string name in interceptorNames)
            {
                object next = owningObjectFactory.GetObject(name);
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

        /// <summary>
        /// Create the proxy if have a custom TargetSource
        /// </summary>
        /// <param name="objectType">The object type</param>
        /// <param name="objectName">The object name</param>
        /// <returns>null if not creating a proxy, otherwise return the proxy.</returns>
        public object PostProcessBeforeInstantiation(Type objectType, string objectName)
        {
            object cacheKey = GetCacheKey(objectType, objectName);
            if (!targetSourcedObjects.Contains(cacheKey))
            {
                if (advisedObjects.Contains(cacheKey) || nonAdvisedObjects.Contains(cacheKey))
                {
                    return null;
                }

                if (IsInfrastructureType(objectType, objectName))
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug(string.Format("Did not attempt to autoproxy infrastructure type [{0}]", objectType));
                    }

                    nonAdvisedObjects.Add(cacheKey);
                    return null;
                }

                if (ShouldSkip(objectType, objectName))
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug(string.Format("Skipping  type [{0}]", objectType));
                    }

                    nonAdvisedObjects.Add(cacheKey);
                    return null;
                }
            }
            // Create proxy here if we have a custom TargetSource.
            // Suppresses unnecessary default instantiation of the target object:
            // The TargetSource will handle target instances in a custom fashion.
            ITargetSource targetSource = GetCustomTargetSource(objectType, objectName);
            if (targetSource != null)
            {
                targetSourcedObjects.Add(objectName);
                IList<object> specificInterceptors = GetAdvicesAndAdvisorsForObject(objectType, objectName, targetSource);
                return CreateProxy(objectType, objectName, specificInterceptors, targetSource);
            }
            return null;
        }


        /// <summary>
        /// Default behavior, return true and continue processing.
        /// </summary>
        /// <param name="objectInstance">The object instance</param>
        /// <param name="objectName">The object name.</param>
        /// <returns>true</returns>
        public bool PostProcessAfterInstantiation(object objectInstance, string objectName)
        {
            return true;
        }

        /// <summary>
        /// Default behavior, return passed in PropertyValues
        /// </summary>
        /// <param name="pvs">The property values that the factory is about to apply (never <code>null</code>).</param>
        /// <param name="pis">he relevant property infos for the target object (with ignored
        /// dependency types - which the factory handles specifically - already filtered out)</param>
        /// <param name="objectInstance">The object instance created, but whose properties have not yet
        /// been set.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The passed in PropertyValues</returns>
        public IPropertyValues PostProcessPropertyValues(IPropertyValues pvs, IList<PropertyInfo> pis, object objectInstance, string objectName)
        {
            return pvs;
        }
    }
}
