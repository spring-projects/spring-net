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

using System.Runtime.Serialization;

using AopAlliance.Aop;
using AopAlliance.Intercept;

using Common.Logging;

using Spring.Aop.Framework.Adapter;
using Spring.Aop.Support;
using Spring.Aop.Target;
using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Aop.Framework
{
    /// <summary>
    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation to
    /// source AOP proxies from a Spring.NET IoC container (an
    /// <see cref="Spring.Objects.Factory.IObjectFactory"/>).
    /// </summary>
    /// <remarks>
    /// <p>
    /// <see cref="AopAlliance.Intercept.IInterceptor"/>s and
    /// <see cref="Spring.Aop.IAdvisor"/>s are identified by a list of object
    /// names in the current container.</p>
    /// <p>
    /// Global interceptors and advisors can be added at the factory level
    /// (that is, outside the context of a
    /// <see cref="Spring.Aop.Framework.ProxyFactoryObject"/> definition). The
    /// specified interceptors and advisors are expanded in an interceptor list
    /// (see
    /// <see cref="Spring.Aop.Framework.ProxyFactoryObject.InterceptorNames"/>)
    /// where an <c>'xxx*'</c> wildcard-style entry is included in the list,
    /// matching the given prefix with the object names. For example,
    /// <c>'global*'</c> would match both <c>'globalObject1'</c> and
    /// <c>'globalObjectBar'</c>, and <c>'*'</c> would match all defined
    /// interceptors. The matching interceptors get applied according to their
    /// returned order value, if they implement the
    /// <see cref="Spring.Core.IOrdered"/> interface. An interceptor name list
    /// may not conclude with a global <c>'xxx*'</c> pattern, as global
    /// interceptors cannot invoke targets.
    /// </p>
    /// <p>
    /// It is possible to cast a proxy obtained from this factory to an
    /// <see cref="Spring.Aop.Framework.IAdvised"/> reference, or to obtain the
    /// <see cref="Spring.Aop.Framework.ProxyFactoryObject"/> reference and
    /// programmatically manipulate it. This won't work for existing prototype
    /// references, which are independent... however, it will work for prototypes
    /// subsequently obtained from the factory. Changes to interception will
    /// work immediately on singletons (including existing references).
    /// However, to change interfaces or the target it is necessary to obtain a
    /// new instance from the surrounding container. This means that singleton
    /// instances obtained from the factory do not have the same object
    /// identity... however, they do have the same interceptors and target, and
    /// changing any reference will change all objects.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Federico Spinazzi (.NET)</author>
    /// <author>Choy Rim (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    /// <seealso cref="Spring.Aop.Framework.ProxyFactoryObject.InterceptorNames"/>
    /// <seealso cref="Spring.Aop.Framework.ProxyFactoryObject.ProxyInterfaces"/>
    /// <seealso cref="AopAlliance.Intercept.IMethodInterceptor"/>
    /// <seealso cref="Spring.Aop.IAdvisor"/>
    /// <seealso cref="Spring.Aop.Target.SingletonTargetSource"/>
    [Serializable]
    public class ProxyFactoryObject
        : AdvisedSupport, IFactoryObject, IObjectFactoryAware
    {
        /// <summary>
        /// The <see cref="Common.Logging.ILog"/> instance for this class.
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger<ProxyFactoryObject>();

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        private bool singleton = true;

        /// <summary>
        /// This suffix in a value in an interceptor list indicates to expand globals.
        /// </summary>
        public static readonly string GlobalInterceptorSuffix = "*";

        /// <summary>
        /// The cached instance if this proxy factory object is a singleton.
        /// </summary>
        private object singletonInstance;

        /// <summary>
        /// The owning object factory (which cannot be changed after this object is initialized).
        /// </summary>
        private IObjectFactory objectFactory;

        /// <summary>
        /// The advisor adapter registry for wrapping pure advices and pointcuts according to needs
        /// </summary>
        private IAdvisorAdapterRegistry advisorAdapterRegistry;

        /// <summary>
        /// Names of interceptors and pointcut objects in the factory.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Default is for globals expansion only.
        /// </p>
        /// </remarks>
        private string[] interceptorNames;

        /// <summary>
        /// Names of introductions and pointcut objects in the factory.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Default is for globals expansion only.
        /// </p>
        /// </remarks>
        private string[] introductionNames;

        /// <summary>
        /// The name of the target object(in the enclosing
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/>).
        /// </summary>
        private string targetName;

        /// <summary>
        /// Indicates if the advisor chain has already been initialized
        /// </summary>
        private bool initialized;

        /// <summary>
        /// Indicate whether this config shall be frozen upon creation
        /// of the first proxy instance
        /// </summary>
        private bool freezeProxy;

        /// <inheritdoc />
        protected ProxyFactoryObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            singleton = info.GetBoolean("singleton");
            singletonInstance = info.GetValue("singletonInstance", typeof(object));
            objectFactory = (IObjectFactory) info.GetValue("objectFactory", typeof(IObjectFactory));
            advisorAdapterRegistry = (IAdvisorAdapterRegistry) info.GetValue("advisorAdapterRegistry", typeof(IAdvisorAdapterRegistry));
            interceptorNames = (string[]) info.GetValue("interceptorNames", typeof(string[]));
            introductionNames = (string[]) info.GetValue("introductionNames", typeof(string[]));
            targetName = info.GetString("targetName");
            initialized = info.GetBoolean("initialized");
            freezeProxy = info.GetBoolean("freezeProxy");
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("singleton", singleton);
            info.AddValue("singletonInstance", singletonInstance);
            info.AddValue("objectFactory", objectFactory);
            info.AddValue("advisorAdapterRegistry", advisorAdapterRegistry);
            info.AddValue("interceptorNames", interceptorNames);
            info.AddValue("introductionNames", introductionNames);
            info.AddValue("targetName", targetName);
            info.AddValue("initialized", initialized);
            info.AddValue("freezeProxy", freezeProxy);
        }

        /// <summary>
        /// Indicate whether this config shall be frozen upon creation
        /// of the first proxy instance
        /// </summary>
        public bool FreezeProxy
        {
            get { return freezeProxy; }
            set { freezeProxy = value; }
        }

        /// <summary>
        /// If set true, any attempt to modify this proxy configuration will raise an exception
        /// </summary>
        public override bool IsFrozen
        {
            set
            {
                // defer freezing this config until the first proxy gets created
                freezeProxy = value;
            }
        }

        /// <summary>
        /// Specify the AdvisorAdapterRegistry to use. Default is the <see cref="GlobalAdvisorAdapterRegistry.Instance"/>
        /// </summary>
        public IAdvisorAdapterRegistry AdvisorAdapterRegistry
        {
            get { return advisorAdapterRegistry; }
            set { advisorAdapterRegistry = value; }
        }

        /// <summary>
        /// Sets the names of the interfaces that are to be implemented by the proxy.
        /// </summary>
        /// <value>
        /// The names of the interfaces that are to be implemented by the proxy.
        /// </value>
        /// <exception cref="Spring.Aop.Framework.AopConfigException">
        /// If the supplied value (or any of its elements) is <see langword="null"/>;
        /// or if any of the element values is not the (assembly qualified) name of
        /// an interface type.
        /// </exception>
        public virtual string[] ProxyInterfaces
        {
            set
            {
                try
                {
                    Interfaces = TypeResolutionUtils.ResolveInterfaceArray(value);
                }
                catch (Exception ex)
                {
                    throw new AopConfigException("Bad value passed to the ProxyInterfaces property (see inner exception).", ex);
                }
            }
        }

        /// <summary>
        /// Sets the name of the target object being proxied.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Only works when the
        /// <see cref="Spring.Aop.Framework.ProxyFactoryObject.ObjectFactory"/>
        /// property is set; it is a logic error on the part of the programmer
        /// if this value is set and the accompanying
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> is not also set.
        /// </p>
        /// </remarks>
        /// <value>
        /// The name of the target object being proxied.
        /// </value>
        public virtual string TargetName
        {
            set { targetName = value; }
        }

        /// <summary>
        /// Sets the list of <see cref="AopAlliance.Intercept.IMethodInterceptor"/> and
        /// <see cref="Spring.Aop.IAdvisor"/> object names.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This property must always be set (configured) when using a
        /// <see cref="Spring.Aop.Framework.ProxyFactoryObject"/> in an
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> context.
        /// </p>
        /// </remarks>
        /// <value>
        /// The list of <see cref="AopAlliance.Intercept.IMethodInterceptor"/> and
        /// <see cref="Spring.Aop.IAdvisor"/> object names.
        /// </value>
        /// <seealso cref="AopAlliance.Intercept.IInterceptor"/>
        /// <seealso cref="Spring.Aop.IAdvisor"/>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory"/>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/>
        public virtual string[] InterceptorNames
        {
            set { interceptorNames = value; }
        }

        /// <summary>
        /// Sets the list of introduction object names.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Only works when the
        /// <see cref="Spring.Aop.Framework.ProxyFactoryObject.ObjectFactory"/>
        /// property is set; it is a logic error on the part of the programmer
        /// if this value is set and the accompanying
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> is not supplied.
        /// </p>
        /// </remarks>
        /// <value>
        /// The list of introduction object names. .
        /// </value>
        public virtual string[] IntroductionNames
        {
            set { introductionNames = value; }
        }

        /// <summary>
        /// Creates a new instance of ProxyFactoryObject
        /// </summary>
        public ProxyFactoryObject()
        {
            advisorAdapterRegistry = GlobalAdvisorAdapterRegistry.Instance;
            singleton = true;
        }

        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        /// <value>
        /// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// (may not be <see langword="null"/>). The object can immediately
        /// call methods on the factory.
        /// </value>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of initialization errors.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory"/>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/>
        public virtual IObjectFactory ObjectFactory
        {
            set
            {
                objectFactory = value;
            }
        }


        /// <summary>
        /// Creates an instance of the AOP proxy to be returned by this factory
        /// </summary>
        /// <remarks>
        /// <p>
        /// Invoked when clients obtain objects from this factory object. The
        /// (proxy) instance will be cached for a singleton, and created on each
        /// call to <see cref="Spring.Aop.Framework.ProxyFactoryObject.GetObject"/>
        /// for a prototype.
        /// </p>
        /// </remarks>
        /// <returns>
        /// A fresh AOP proxy reflecting the current state of this factory.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IFactoryObject.GetObject()"/>
        public virtual object GetObject()
        {
            lock (SyncRoot)
            {
                if (!initialized)
                {
                    Initialize();
                    initialized = true;
                }

                if (IsSingleton)
                {
                    return SingletonInstance;
                }

                if (targetName == null)
                {
                    logger.Warn("Using non-singleton proxies with singleton targets is often undesirable. " +
                        "Enable prototype proxies by setting the 'targetName' property.");
                }
                return NewPrototypeInstance();
            }
        }

        /// <summary>
        /// Return the <see cref="System.Type"/> of the proxy.
        /// </summary>
        /// <remarks>
        /// Will check the singleton instance if already created,
        /// else fall back to the proxy interface (if a single one),
        /// the target bean type, or the TargetSource's target class.
        /// </remarks>
        /// Return the <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
        /// <see langword="null"/> if not known in advance.
        public virtual Type ObjectType
        {
            get
            {
                // TODO (EE): sync with Java
                lock (SyncRoot)
                {
                    if (singletonInstance != null)
                    {
                        return singletonInstance.GetType();
                    }
                    else if (Interfaces.Count == 1)
                    {
                        return Interfaces[0];
                    }
                    else if (targetName != null && objectFactory != null)
                    {
                        return objectFactory.GetType(targetName);
                    }
                    else
                    {
                        return TargetSource.TargetType;
                    }
                }
            }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        public virtual bool IsSingleton
        {
            get { return singleton; }
            set { singleton = value; }
        }

        private object SingletonInstance
        {
            get
            {
                if (singletonInstance == null)
                {
                    TargetSource = FreshTargetSource();
                    singletonInstance = CreateAopProxy().GetProxy();
                    base.IsFrozen = freezeProxy; // freeze after creating proxy to allow for interface autodetection
                }
                return singletonInstance;
            }
        }

        private object NewPrototypeInstance()
        {
            // in the case of a prototype, we need to give the proxy
            // an independent instance of the configuration...

            if (logger.IsDebugEnabled)
            {
                logger.Debug("Creating copy of prototype ProxyFactoryObject config: " + this);
            }

            // The copy needs a fresh advisor chain, and a fresh TargetSource.
            ITargetSource targetSource = FreshTargetSource();
            IList<IAdvisor> advisorChain = FreshAdvisorChain();
            IList<IIntroductionAdvisor> introductionChain = FreshIntroductionChain();
            AdvisedSupport copy = new AdvisedSupport();
            copy.CopyConfigurationFrom(this, targetSource, advisorChain, introductionChain);

            if (logger.IsDebugEnabled)
            {
                logger.Debug("Using ProxyConfig: " + copy);
            }

            object generatedProxy = copy.CreateAopProxy().GetProxy();
            base.IsFrozen = freezeProxy; // freeze after creating proxy to allow for interface autodetection
            return generatedProxy;
        }

        /// <summary>
        /// Initialize this proxy factory - usually called after all properties are set
        /// </summary>
        private void Initialize()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(string.Format("Initialize: begin configure target, interceptors and introductions for {0}[{1}]", GetType().Name, GetHashCode()));
            }

            InitializeAdvisorChain();
            InitializeIntroductionChain();

            if (logger.IsDebugEnabled)
            {
                logger.Debug(string.Format("Initialize: completed configuration for {0}[{1}]: {2}", GetType().Name, GetHashCode(), ToProxyConfigString()));
            }
        }

        /// <summary>Create the advisor (interceptor) chain.</summary>
        /// <remarks>
        /// The advisors that are sourced from an ObjectFactory will be refreshed each time
        /// a new prototype instance is added. Interceptors added programmatically through
        /// the factory API are unaffected by such changes.
        /// </remarks>
        private void InitializeAdvisorChain()
        {
            if (ObjectUtils.IsEmpty(interceptorNames))
            {
                return;
            }

            CheckInterceptorNames();

            // Globals can't be last unless we specified a targetSource using the property...
            if (interceptorNames[interceptorNames.Length - 1] != null
                && interceptorNames[interceptorNames.Length - 1].EndsWith(GlobalInterceptorSuffix)
                && targetName == null
                && TargetSource == EmptyTargetSource.Empty)
            {
                throw new AopConfigException("Target required after globals");
            }

            // materialize interceptor chain from object names...
            foreach (string name in interceptorNames)
            {
                if (name == null)
                {
                    throw new AopConfigException("Found null interceptor name value in the InterceptorNames list; check your configuration.");
                }

                if (name.EndsWith(GlobalInterceptorSuffix))
                {
                    IListableObjectFactory lof = objectFactory as IListableObjectFactory;
                    if (lof == null)
                    {
                        throw new AopConfigException("Can only use global advisors or interceptors in conjunction with an IListableObjectFactory.");
                    }

                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Adding global advisor '" + name + "'");
                    }

                    AddGlobalAdvisor(lof, name.Substring(0, (name.Length - GlobalInterceptorSuffix.Length)));
                }
                else
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("resolving advisor name " + "'" + name + "'");
                    }

                    // If we get here, we need to add a named interceptor.
                    // We must check if it's a singleton or prototype.
                    object advice;
                    if (IsSingleton || objectFactory.IsSingleton(name))
                    {
                        advice = objectFactory.GetObject(name);
                        AssertUtils.ArgumentNotNull(advice, "advice", "object factory returned a null object");
                    }
                    else
                    {
                        advice = new PrototypePlaceholder(name);
                    }
                    AddAdvisorOnChainCreation(advice, name);
                }
            }
        }

        private void AddAdvisorOnChainCreation(object advice, string name)
        {
            if (advice is IAdvisors)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(string.Format("Adding advisor list '{0}'", name));
                }

                IAdvisors advisors = (IAdvisors)advice;
                foreach (object element in advisors.Advisors)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug(string.Format("Adding advisor '{0}' of type {1}", name, element.GetType().FullName));
                    }

                    IAdvisor advisor = NamedObjectToAdvisor(element);
                    AddAdvisor(advisor);
                }
            }
            else
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(string.Format("Adding advisor '{0}' of type {1}", name, advice.GetType().FullName));
                }

                IAdvisor advisor = NamedObjectToAdvisor(advice);
                AddAdvisor(advisor);
            }
        }

        private bool IsNamedObjectAnAdvisorOrAdvice(string name)
        {
            Type namedObjectType = objectFactory.GetType(name);
            if (namedObjectType != null)
            {
                return typeof(IAdvisors).IsAssignableFrom(namedObjectType)
                    || typeof(IAdvisor).IsAssignableFrom(namedObjectType)
                    || typeof(IAdvice).IsAssignableFrom(namedObjectType);
            }
            // treat it as an IAdvisor if we can't tell...
            return true;
        }

        /// <summary> Add all global interceptors and pointcuts.</summary>
        private void AddGlobalAdvisor(IListableObjectFactory objectFactory, string prefix)
        {
            var globalAspectNames =
                ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IAdvisors));
            var globalAdvisorNames =
                ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IAdvisor));
            var globalInterceptorNames =
                ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IInterceptor));

            List<object> objects = new List<object>();
            Dictionary<object, string> names = new Dictionary<object, string>();
            for (int i = 0; i < globalAspectNames.Count; i++)
            {
                string name = globalAspectNames[i];
                if (name.StartsWith(prefix))
                {
                    IAdvisors advisors = (IAdvisors)objectFactory.GetObject(name);
                    foreach (object advisor in advisors.Advisors)
                    {
                        // exclude introduction advisors from interceptor list
                        if (!(advisor is IIntroductionAdvisor))
                        {
                            objects.Add(advisor);
                            names[advisor] = name;
                        }
                    }
                }
            }
            for (int i = 0; i < globalAdvisorNames.Count; i++)
            {
                string name = globalAdvisorNames[i];
                if (name.StartsWith(prefix))
                {
                    object obj = objectFactory.GetObject(name);
                    // exclude introduction advisors from interceptor list
                    if (!(obj is IIntroductionAdvisor))
                    {
                        objects.Add(obj);
                        names[obj] = name;
                    }
                }
            }
            for (int i = 0; i < globalInterceptorNames.Count; i++)
            {
                string name = globalInterceptorNames[i];
                if (name.StartsWith(prefix))
                {
                    object obj = objectFactory.GetObject(name);
                    objects.Add(obj);
                    names[obj] = name;
                }
            }
            objects.Sort(OrderComparator.Instance);
            foreach (object obj in objects)
            {
                string name = names[obj];
                AddAdvisorOnChainCreation(obj, name);
            }
        }

        /// <summary>
        /// Configures introductions for this proxy.
        /// </summary>
        private void InitializeIntroductionChain()
        {
            if (ObjectUtils.IsEmpty(introductionNames))
            {
                return;
            }

            // Materialize introductions from object names...
            foreach (string name in introductionNames)
            {
                if (name == null)
                {
                    throw new AopConfigException("Found null interceptor name value in the InterceptorNames list; check your configuration.");
                }

                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Adding introduction '" + name + "'");
                }

                if (name.EndsWith(GlobalInterceptorSuffix))
                {
                    if (!(objectFactory is IListableObjectFactory))
                    {
                        throw new AopConfigException("Can only use global introductions with a ListableObjectFactory");
                    }
                    AddGlobalIntroduction((IListableObjectFactory)objectFactory, name.Substring(0, (name.Length - GlobalInterceptorSuffix.Length)));
                }
                else
                {
                    // add a named introduction
                    object introduction;
                    if (IsSingleton || objectFactory.IsSingleton(name))
                    {
                        introduction = objectFactory.GetObject(name);
                        AssertUtils.ArgumentNotNull(introduction, "introduction", "object factory returned a null object");
                    }
                    else
                    {
                        introduction = new PrototypePlaceholder(name);
                    }
                    AddIntroductionOnChainCreation(introduction, name);
                }
            }
        }

        /// <summary> Add all global introductions.</summary>
        private void AddGlobalIntroduction(IListableObjectFactory objectFactory, string prefix)
        {
            var globalAspectNames =
                ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IAdvisors));
            var globalAdvisorNames =
                ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IAdvisor));
            var globalIntroductionNames =
                ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IAdvice));
            List<object> objects = new List<object>();
            Dictionary<object, string> names = new Dictionary<object, string>();

            for (int i = 0; i < globalAspectNames.Count; i++)
            {
                string name = globalAspectNames[i];
                if (name.StartsWith(prefix))
                {
                    IAdvisors advisors = (IAdvisors)objectFactory.GetObject(name);
                    foreach (object advisor in advisors.Advisors)
                    {
                        // only include introduction advisors
                        if (advisor is IIntroductionAdvisor)
                        {
                            objects.Add(advisor);
                            names[advisor] = name;
                        }
                    }
                }
            }
            for (int i = 0; i < globalAdvisorNames.Count; i++)
            {
                string name = globalAdvisorNames[i];
                if (name.StartsWith(prefix))
                {
                    object obj = objectFactory.GetObject(name);
                    // only include introduction advisors
                    if (obj is IIntroductionAdvisor)
                    {
                        objects.Add(obj);
                        names[obj] = name;
                    }
                }
            }
            for (int i = 0; i < globalIntroductionNames.Count; i++)
            {
                string name = globalIntroductionNames[i];
                if (name.StartsWith(prefix))
                {
                    object obj = objectFactory.GetObject(name);
                    // exclude other advice types
                    if (!(obj is IInterceptor || obj is IBeforeAdvice || obj is IAfterReturningAdvice))
                    {
                        objects.Add(obj);
                        names[obj] = name;
                    }
                }
            }
            objects.Sort(OrderComparator.Instance);
            for (var i = 0; i < objects.Count; i++)
            {
                object obj = objects[i];
                string name = names[obj];
                AddIntroductionOnChainCreation(obj, name);
            }
        }

        /// <summary>Add the introduction to the introduction list.</summary>
        /// <remarks>
        /// If specified parameter is IIntroducionAdvisor it is added directly, otherwise it is wrapped
        /// with DefaultIntroductionAdvisor first.
        /// </remarks>
        /// <param name="introduction">introducion to add</param>
        /// <param name="name">object name from which we obtained this object in our owning object factory</param>
        private void AddIntroductionOnChainCreation(object introduction, string name)
        {
            logger.Debug($"Adding introduction with name '{name}'");
            IIntroductionAdvisor advisor = NamedObjectToIntroduction(introduction);
            AddIntroduction(advisor);
        }

        /// <summary>
        /// Refreshes target object for prototype instances.
        /// </summary>
        private ITargetSource FreshTargetSource()
        {
            if (StringUtils.IsNullOrEmpty(targetName))
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Not Refreshing TargetSource: No target name specified");
                }

                return TargetSource;
            }

            AssertUtils.ArgumentNotNull(objectFactory, "ObjectFactory");

            if (logger.IsDebugEnabled)
            {
                logger.Debug("Refreshing TargetSource with name '" + targetName + "'");
            }

            object target = objectFactory.GetObject(targetName);
            ITargetSource targetSource = NamedObjectToTargetSource(target);
            return targetSource;
        }

        /// <summary> Refresh named objects from the interceptor chain.
        /// We need to do this every time a new prototype instance is returned,
        /// to return distinct instances of prototype interfaces and pointcuts.
        /// </summary>
        private IList<IAdvisor> FreshAdvisorChain()
        {
            IList<IAdvisor> advisors = Advisors;
            List<IAdvisor> freshAdvisors = new List<IAdvisor>();
            foreach (IAdvisor advisor in advisors)
            {
                if (advisor is PrototypePlaceholder)
                {
                    PrototypePlaceholder pa = (PrototypePlaceholder)advisor;
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug(string.Format("Refreshing advisor '{0}'", pa.ObjectName));
                    }

                    AssertUtils.ArgumentNotNull(objectFactory, "ObjectFactory");

                    object advisorObject = objectFactory.GetObject(pa.ObjectName);
                    IAdvisor freshAdvisor = NamedObjectToAdvisor(advisorObject);
                    freshAdvisors.Add(freshAdvisor);
                }
                else
                {
                    freshAdvisors.Add(advisor);
                }
            }
            return freshAdvisors;
        }

        /// <summary> Refresh named objects from the interceptor chain.
        /// We need to do this every time a new prototype instance is returned,
        /// to return distinct instances of prototype interfaces and pointcuts.
        /// </summary>
        private IList<IIntroductionAdvisor> FreshIntroductionChain()
        {
            IList<IIntroductionAdvisor> introductions = Introductions;
            List<IIntroductionAdvisor> freshIntroductions = new List<IIntroductionAdvisor>();
            foreach (IIntroductionAdvisor introduction in introductions)
            {
                if (introduction is PrototypePlaceholder)
                {
                    PrototypePlaceholder pa = (PrototypePlaceholder)introduction;
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug(string.Format("Refreshing introduction '{0}'", pa.ObjectName));
                    }

                    AssertUtils.ArgumentNotNull(objectFactory, "ObjectFactory");

                    object introductionObject = objectFactory.GetObject(pa.ObjectName);
                    IIntroductionAdvisor freshIntroduction = NamedObjectToIntroduction(introductionObject);
                    freshIntroductions.Add(freshIntroduction);
                }
                else
                {
                    freshIntroductions.Add(introduction);
                }
            }
            return freshIntroductions;
        }

        /// <summary>Wraps target with SingletonTargetSource if necessary</summary>
        /// <param name="target">target or target source object</param>
        /// <returns>target source passed or target wrapped with SingletonTargetSource</returns>
        private ITargetSource NamedObjectToTargetSource(object target)
        {
            if (target is ITargetSource)
            {
                return (ITargetSource)target;
            }
            // It's an object that needs target source around it.
            return new SingletonTargetSource(target);
        }

        /// <summary>Wraps introduction with IIntroductionAdvisor if necessary</summary>
        /// <summary>Wraps pointcut or interceptor with appropriate advisor</summary>
        /// <param name="next">pointcut or interceptor that needs to be wrapped with advisor</param>
        /// <returns>Advisor</returns>
        private IAdvisor NamedObjectToAdvisor(object next)
        {
            try
            {
                return advisorAdapterRegistry.Wrap(next);
            }
            catch (UnknownAdviceTypeException ex)
            {
                throw new AopConfigException(string.Format("Unknown advisor type '{0}'; Can only include Advisor or Advice type beans in interceptorNames chain except for last entry,which may also be target or TargetSource", next.GetType().FullName), ex);
            }
        }

        /// <param name="introduction">object to wrap</param>
        /// <returns>Introduction advisor</returns>
        private IIntroductionAdvisor NamedObjectToIntroduction(object introduction)
        {
            if (introduction is IIntroductionAdvisor)
            {
                return (IIntroductionAdvisor)introduction;
            }
            return new DefaultIntroductionAdvisor((IAdvice)introduction);
        }

        /// <summary>
        /// Callback method that is invoked when the list of proxied interfaces
        /// has changed.
        /// </summary>
        /// <remarks>
        /// <p>
        /// An example of such a change would be when a new introduction is
        /// added. Resetting
        /// <see cref="Spring.Aop.Framework.AdvisedSupport.ProxyType"/> to
        /// <cref lang="null"/> will cause a new proxy <see cref="System.Type"/>
        /// to be generated on the next call to get a proxy.
        /// </p>
        /// </remarks>
        protected override void InterfacesChanged()
        {
            logger.Info("Implemented interfaces have changed; reseting singleton instance");
            singletonInstance = null;
            base.InterfacesChanged();
        }

        /// <summary>
        /// Returns textual information about this configuration object
        /// </summary>
        protected override string ToProxyConfigStringInternal()
        {
            return string.Format("{0}\ntargetName={1}", base.ToProxyConfigStringInternal(), targetName);
        }

        /// <summary>
        /// Check the interceptorNames list whether it contains a target name as final element.
        /// If found, remove the final name from the list and set it as targetName.
        /// </summary>
        private void CheckInterceptorNames()
        {
            if (!ObjectUtils.IsEmpty(interceptorNames))
            {
                String finalName = interceptorNames[interceptorNames.Length - 1];
                if (finalName != null && targetName == null && TargetSource == EmptyTargetSource.Empty)
                {
                    // The last name in the chain may be an Advisor/Advice or a target/TargetSource.
                    // Unfortunately we don't know; we must look at type of the bean.
                    if (!finalName.EndsWith(GlobalInterceptorSuffix)
                        && !IsNamedObjectAnAdvisorOrAdvice(finalName))
                    {
                        // The target isn't an interceptor.
                        targetName = finalName;
                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug(string.Format("Object with name '{0}' concluding interceptor chain is not an advisor class: treating it as a target or TargetSource", finalName));
                        }
                        String[] newNames = new String[interceptorNames.Length - 1];
                        Array.Copy(interceptorNames, 0, newNames, 0, newNames.Length);
                        interceptorNames = newNames;
                    }
                }
            }
        }

        [Serializable]
        private class PrototypePlaceholder : IIntroductionAdvisor
        {
            private readonly string objectName;
            private readonly string message;

            public string ObjectName
            {
                get { return objectName; }
            }

            public PrototypePlaceholder(string objectName)
            {
                this.objectName = objectName;
                message = "Placeholder for prototype Advisor/Advice/Introduction with bean name '" + objectName + "'";
            }

            public bool IsPerInstance
            {
                get { throw new NotSupportedException("Cannot invoke methods: " + message); }
            }

            public IAdvice Advice
            {
                get { throw new NotSupportedException("Cannot invoke methods: " + message); }
            }

            public ITypeFilter TypeFilter
            {
                get { throw new NotSupportedException("Cannot invoke methods: " + message); }
            }

            public Type[] Interfaces
            {
                get { throw new NotSupportedException("Cannot invoke methods: " + message); }
            }

            public void ValidateInterfaces()
            {
                throw new NotSupportedException("Cannot invoke methods: " + message);
            }
        }
    }
}
