#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Diagnostics;
using System.Globalization;
using Common.Logging;
using Spring.Context.Events;
using Spring.Core;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Events;
using Spring.Objects.Events.Support;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Support;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Partial implementation of the
    /// <see cref="Spring.Context.IApplicationContext"/> interface.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Does not mandate the type of storage used for configuration, but does
    /// implement common functionality. Uses the Template Method design
    /// pattern, requiring concrete subclasses to implement
    /// <see langword="abstract"/> methods.
    /// </p>
    /// <p>
    /// In contrast to a plain vanilla
    /// <see cref="Spring.Objects.Factory.IObjectFactory"/>, an
    /// <see cref="Spring.Context.IApplicationContext"/> is supposed
    /// to detect special objects defined in its object factory: therefore,
    /// this class automatically registers
    /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>s,
    /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s
    /// and <see cref="Spring.Context.IApplicationEventListener"/>s that are
    /// defined as objects in the context.
    /// </p>
    /// <p>
    /// An <see cref="Spring.Context.IMessageSource"/> may be also supplied as
    /// an object in the context, with the special, well-known-name of
    /// <c>"messageSource"</c>. Else, message resolution is delegated to the
    /// parent context.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergan Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <seealso cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
    /// <seealso cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
    public abstract class AbstractApplicationContext
        : ConfigurableResourceLoader, IConfigurableApplicationContext, IObjectDefinitionRegistry
    {
        #region Constants

        /// <summary>
        /// Name of the .Net config section that contains Spring.Net context definition.
        /// </summary>
        public const string ContextSectionName = "spring/context";

        /// <summary>
        /// Default name of the root context.
        /// </summary>
        public const string DefaultRootContextName = "spring.root";

        #endregion

        #region Fields

        private const long TicksAtEpoch = 621355968000000000;

        /// <summary>
        /// The special, well-known-name of the default
        /// <see cref="Spring.Context.IMessageSource"/> in the context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If no <see cref="Spring.Context.IMessageSource"/> can be found
        /// in the context using this lookup key, then message resolution
        /// will be delegated to the parent context (if any).
        /// </p>
        /// </remarks>
        public static readonly string MessageSourceObjectName = "messageSource";

        /// <summary>
        /// The special, well-known-name of the default
        /// <see cref="Spring.Objects.Events.IEventRegistry"/> in the context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If no <see cref="Spring.Objects.Events.IEventRegistry"/> can be found
        /// in the context using this lookup key, then a default
        /// <see cref="Spring.Objects.Events.IEventRegistry"/> will be used.
        /// </p>
        /// </remarks>
        public static readonly string EventRegistryObjectName = "eventRegistry";

        /// <summary>
        /// The <see cref="Common.Logging.ILog"/> instance for this class.
        /// </summary>
        protected readonly ILog log;

        /// <summary>
        /// The <see cref="Spring.Context.IMessageSource"/> instance we delegate
        /// our implementation of said interface to.
        /// </summary>
        private IMessageSource _messageSource;

        /// <summary>
        /// The <see cref="Spring.Objects.Events.IEventRegistry"/> instance we
        /// delegate our implementation of said interface to.
        /// </summary>
        private IEventRegistry _eventRegistry;

        private IApplicationContext _parentApplicationContext;
        private readonly IList _objectFactoryPostProcessors;
        private readonly IList _defaultObjectPostProcessors;
        private string _name;
        private DateTime _startupDate;
        private readonly bool _isCaseSensitive;
        private EventRaiser _eventRaiser;

        #endregion


        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the <see cref="AbstractApplicationContext"/>
        /// with no parent context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes
        /// no public constructors.
        /// </p>
        /// </remarks>
        protected AbstractApplicationContext()
            : this(null, true, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AbstractApplicationContext"/>
        /// with no parent context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes
        /// no public constructors.
        /// </p>
        /// </remarks>
        /// <param name="caseSensitive">Flag specifying whether to make this context case sensitive or not.</param>
        protected AbstractApplicationContext(bool caseSensitive)
            : this(null, caseSensitive, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AbstractApplicationContext"/>
        /// with the supplied parent context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes
        /// no public constructors.
        /// </p>
        /// </remarks>
        /// <param name="name">The application context name.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this context case sensitive or not.</param>
        /// <param name="parentApplicationContext">The parent application context.</param>
        protected AbstractApplicationContext(string name, bool caseSensitive,
                                             IApplicationContext parentApplicationContext)
        {
            log = LogManager.GetLogger(this.GetType());

            _name = (StringUtils.IsNullOrEmpty(name)) ? DefaultRootContextName : name;
            _isCaseSensitive = caseSensitive;
            _parentApplicationContext = parentApplicationContext;
            EventRaiser = CreateEventRaiser();
            _objectFactoryPostProcessors = new ArrayList();
            _defaultObjectPostProcessors = new ArrayList();
            AddDefaultObjectPostProcessor(new ObjectPostProcessorChecker());
            AddDefaultObjectPostProcessor(new ApplicationContextAwareProcessor(this));
            AddDefaultObjectPostProcessor(new SharedStateAwareProcessor(new ISharedStateFactory[] { new ByTypeSharedStateFactory() }, Int32.MaxValue));
        }

        /// <summary>
        /// Adds the given <see cref="IObjectPostProcessor"/> to the list of standard
        /// processors being added to the underlying <see cref="IConfigurableObjectFactory"/>
        /// </summary>
        /// <remarks>
        /// Each time <see cref="Refresh()"/> is called on this context, the context ensures, that 
        /// all default <see cref="IObjectPostProcessor"/>s are registered with the underlying <see cref="IConfigurableObjectFactory"/>.
        /// </remarks>
        /// <param name="defaultObjectPostProcessor">The <see cref="IObjectPostProcessor"/> instance.</param>
        protected void AddDefaultObjectPostProcessor(IObjectPostProcessor defaultObjectPostProcessor)
        {
            _defaultObjectPostProcessors.Add(defaultObjectPostProcessor);
        }

        /// <summary>
        /// Closes this context and disposes of any resources (such as
        /// singleton objects in the wrapped
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/>).
        /// </summary>
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format(
                              CultureInfo.InvariantCulture,
                              "Closing application context [{0}].",
                              Name));
            }

            #endregion

            // Closed event is raised before destroying objectfactory to enable registered IApplicationEventListeners 
            // to handle the event before they get disposed.
            PublishEvent(this, new ContextClosedEventArgs());

            ObjectFactory.Dispose();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Subclasses must implement this method to perform the actual
        /// configuration loading.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is invoked by
        /// <see cref="Spring.Context.Support.AbstractApplicationContext.Refresh()"/>,
        /// before any other initialization occurs.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of errors encountered while refreshing the object factory.
        /// </exception>
        protected abstract void RefreshObjectFactory();

        #endregion

        /// <summary>
        /// An object that can be used to synchronize access to the <see cref="AbstractXmlApplicationContext"/>
        /// </summary>
        public object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Set the <see cref="EventRaiser"/> to be used by this context.
        /// </summary>
        public EventRaiser EventRaiser
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "EventRaiser");
                _eventRaiser = value;
            }
        }

        /// <summary>
        /// The timestamp when this context was first loaded.
        /// </summary>
        /// <returns>
        /// The timestamp (milliseconds) when this context was first loaded.
        /// </returns>
        public long StartupDateMilliseconds
        {
            get { return (StartupDate.Ticks - TicksAtEpoch) / 10000; }
        }


        /// <summary>
        /// Gets a flag indicating whether context should be case sensitive.
        /// </summary>
        /// <value><c>true</c> if object lookups are case sensitive; otherwise, <c>false</c>.</value>
        public bool IsCaseSensitive
        {
            get { return _isCaseSensitive; }
        }

        /// <summary>
        /// The <see cref="Spring.Context.IMessageSource"/> for this context.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// If the context has not been initialized yet.
        /// </exception>
        public IMessageSource MessageSource
        {
            get
            {
                if (_messageSource == null)
                {
                    throw new InvalidOperationException(
                        "MessageSource not initialized - call 'Refresh()' " +
                        "before accessing messages via the context: " + this);
                }
                return _messageSource;
            }
        }

        /// <summary>
        /// The <see cref="Spring.Objects.Events.IEventRegistry"/> for this context.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// If the context has not been initialized yet.
        /// </exception>
        public IEventRegistry EventRegistry
        {
            get
            {
                if (_eventRegistry == null)
                {
                    throw new InvalidOperationException(
                        "EventRegistry not initialized - call 'Refresh()' " +
                        "before accessing the event registry via the context: " + this);
                }
                return _eventRegistry;
            }
        }

        /// <summary>
        /// Returns the internal object factory of the parent context if it implements
        /// <see cref="Spring.Context.IConfigurableApplicationContext"/>; else,
        /// returns the parent context itself.
        /// </summary>
        /// <returns>
        /// The parent context's object factory, or the parent itself.
        /// </returns>
        protected IObjectFactory GetInternalParentObjectFactory()
        {
            IConfigurableApplicationContext configContext
                = _parentApplicationContext as IConfigurableApplicationContext;
            if (configContext != null)
            {
                return ((IConfigurableApplicationContext)
                        _parentApplicationContext).ObjectFactory;
            }
            else
            {
                return _parentApplicationContext;
            }
        }

        /// <summary>
        /// Raises an application context event.
        /// </summary>
        /// <param name="e">
        /// Any arguments to the event. May be <see langword="null"/>.
        /// </param>
        protected virtual void OnContextEvent(ApplicationEventArgs e)
        {
            OnContextEvent(this, e);
        }

        /// <summary>
        /// Raises an application context event.
        /// </summary>
        /// <param name="source">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// Any arguments to the event. May be <see langword="null"/>.
        /// </param>
        protected virtual void OnContextEvent(object source, ApplicationEventArgs e)
        {
            IEventExceptionsCollector exceptions = _eventRaiser.Raise(ContextEvent, source, e);
            if (exceptions.HasExceptions)
            {
                Delegate target = ContextEvent.GetInvocationList()[0];
                Exception exception = (Exception) exceptions[target];
                throw new ApplicationContextException(string.Format("An unhandled exception occured during processing application event {0} in handler {1}", e.GetType(), target.Method), exception);
            }
        }

        /// <summary>
        /// Create the <see cref="EventRaiser"/> strategy to be used
        /// </summary>
        protected virtual EventRaiser CreateEventRaiser()
        {
            return new DefensiveEventRaiser();
        }

        /// <summary>
        /// Modify the application context's internal object factory after its standard
        /// initialization.
        /// </summary>
        /// <remarks>
        /// <p>
        /// All object definitions will have been loaded, but no objects
        /// will have been instantiated yet. This allows for the registration
        /// of special
        /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s
        /// in certain
        /// <see cref="Spring.Context.IApplicationContext"/> implementations.
        /// </p>
        /// </remarks>
        /// <param name="objectFactory">
        /// The object factory used by the application context.
        /// </param>
        /// <exception cref="ObjectsException">
        /// In the case of errors.
        /// </exception>.
        protected virtual void PostProcessObjectFactory(
            IConfigurableListableObjectFactory objectFactory)
        {
        }

        /// <summary>
        /// Template method which can be overridden to add context-specific
        /// work before the underlying object factory gets refreshed.
        /// </summary>
        protected virtual void OnPreRefresh()
        {
        }

        /// <summary>
        /// Template method which can be overridden to add context-specific
        /// refresh work.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Called on initialization of special objects, before instantiation
        /// of singletons.
        /// </p>
        /// </remarks>
        protected virtual void OnRefresh()
        {
        }

        /// <summary>
        /// Template method which can be overridden to add context-specific
        /// work after the context was refreshed but before the <see cref="ContextEventArgs.ContextEvent.Refreshed"/>
        /// event gets raised.
        /// </summary>
        protected virtual void OnPostRefresh()
        {
            PublishEvent(this, new ContextRefreshedEventArgs());
        }

        /// <summary>
        /// Instantiate and invoke all registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
        /// objects, respecting any explicit ordering.
        /// </summary>
        /// <remarks>
        /// <note type="caution">
        /// <b>Must</b> be called before singleton instantiation.
        /// </note>
        /// </remarks>
        /// <exception cref="ObjectsException">In the case of errors.</exception>
        private void InvokeObjectFactoryPostProcessors()
        {
            // do NOT include IFactoryObjects; they (typically) need to be instantiated
            // to determine the Type of object that they create, and if they are instantiated
            // then we won't be able to do any factory post processin' on 'em...
            string[] factoryProcessorNames
                = GetObjectNamesForType(typeof(IObjectFactoryPostProcessor), true, false);
            ArrayList orderedFactoryProcessors = new ArrayList();
            IList nonOrderedFactoryProcessorNames = new ArrayList();
            for (int i = 0; i < factoryProcessorNames.Length; ++i)
            {
                string processorName = factoryProcessorNames[i];
                object processor = GetObject(processorName);
                if (typeof(IOrdered).IsAssignableFrom(GetType(processorName)))
                {
                    orderedFactoryProcessors.Add(processor);
                }
                else
                {
                    nonOrderedFactoryProcessorNames.Add(processor);
                }
            }
            // first, invoke those IObjectFactoryPostProcessors that implement IOrdered...
            orderedFactoryProcessors.Sort(new OrderComparator());
            ProcessObjectFactoryPostProcessors(orderedFactoryProcessors);
            // and then the unordered ones...
            ProcessObjectFactoryPostProcessors(nonOrderedFactoryProcessorNames);

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format(
                              CultureInfo.InvariantCulture,
                              "processed {0} IFactoryObjectPostProcessors defined in application context [{1}].",
                              factoryProcessorNames.Length,
                              Name));
            }

            #endregion
        }

        private void ProcessObjectFactoryPostProcessors(IList objectFactoryPostProcessors)
        {
            foreach (IObjectFactoryPostProcessor processor in objectFactoryPostProcessors)
            {
                processor.PostProcessObjectFactory(ObjectFactory);
            }
        }

        private void RegisterObjectPostProcessors(IConfigurableListableObjectFactory objectFactory)
        {
            RefreshObjectPostProcessorChecker(objectFactory);
            IDictionary dict = GetObjectsOfType(typeof(IObjectPostProcessor), true, false);
            ArrayList objectProcessors = new ArrayList(dict.Values);
            //            objectProcessors.Sort(new OrderComparator());
            foreach (IObjectPostProcessor objectPostProcessor in objectProcessors)
            {
                ObjectFactory.AddObjectPostProcessor(objectPostProcessor);
            }

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format(
                              CultureInfo.InvariantCulture,
                              "processed {0} IObjectPostProcessors defined in application context [{1}].",
                              objectProcessors.Count,
                              Name));
            }
        }

        /// <summary>
        /// Resets the well-known ObjectPostProcessorChecker that logs an info
        /// message when an object is created during IObjectPostProcessor
        /// instantiation, i.e. when an object is not eligible for being
        /// processed by all IObjectPostProcessors.
        /// </summary>
        private void RefreshObjectPostProcessorChecker(IConfigurableListableObjectFactory objectFactory)
        {
            int registeredObjectPostProcessorCount = GetObjectNamesForType(typeof(IObjectPostProcessor), true, false).Length;
            int objectPostProcessorCount = ObjectFactory.ObjectPostProcessorCount + 1
                                           + registeredObjectPostProcessorCount;
            ((ObjectPostProcessorChecker)_defaultObjectPostProcessors[0]).Reset(objectFactory, objectPostProcessorCount);
        }

        /// <summary>
        /// Initializes the default event registry for this context.
        /// </summary>
        private void InitEventRegistry()
        {
            if (ContainsLocalObject(EventRegistryObjectName))
            {
                object candidateRegistry = GetObject(EventRegistryObjectName);
                if (candidateRegistry is IEventRegistry)
                {
                    _eventRegistry = (IEventRegistry)candidateRegistry;

                    #region Instrumentation

                    log.Debug(StringUtils.Surround(
                                  "Using IEventRegistry [", EventRegistry, "]"));

                    #endregion
                }
                else
                {
                    _eventRegistry = new EventRegistry();

                    #region Instrumentation

                    if (log.IsWarnEnabled)
                    {
                        log.Warn(string.Format(
                                     "Found object in context named '{0}' : this name " +
                                     "is typically reserved for IEventRegistry objects. " +
                                     "Falling back to default '{1}'.",
                                     EventRegistryObjectName, EventRegistry));
                    }

                    #endregion
                }
            }
            else
            {
                _eventRegistry = new EventRegistry();

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(
                                  "No IEventRegistry found with name '{0}' : using default '{1}'.",
                                  EventRegistryObjectName, EventRegistry));
                }

                #endregion
            }
            ICollection interestedParties
                = GetObjectsOfType(typeof(IEventRegistryAware), true, false).Values;
            foreach (IEventRegistryAware party in interestedParties)
            {
                party.EventRegistry = EventRegistry;
            }
            EventRegistry.PublishEvents(this);
        }

        /// <summary>
        /// Returns the internal message source of the parent context if said
        /// parent context is an <see cref="AbstractApplicationContext"/>, else
        /// simply the parent context itself.
        /// </summary>
        /// <returns>
        /// The internal message source of the parent context if said
        /// parent context is an <see cref="AbstractApplicationContext"/>, else
        /// simply the parent context itself.
        /// </returns>
        protected virtual IMessageSource GetInternalParentMessageSource()
        {
            AbstractApplicationContext parent
                = ParentContext as AbstractApplicationContext;
            return parent == null ? ParentContext : parent._messageSource;
        }

        /// <summary>
        /// Initializes the default message source for this context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Uses any parent context's message source if one is not available
        /// in this context.
        /// </p>
        /// </remarks>
        private void InitMessageSource()
        {
            if (ContainsLocalObject(MessageSourceObjectName))
            {
                object candidateSource = GetObject(MessageSourceObjectName);
                if (candidateSource is IMessageSource)
                {
                    _messageSource
                        = (IMessageSource)GetObject(MessageSourceObjectName);

                    // make IMessageSource aware of any parent IMessageSource...
                    if (ParentContext != null)
                    {
                        IHierarchicalMessageSource hierSource
                            = MessageSource as IHierarchicalMessageSource;
                        if (hierSource != null)
                        {
                            IMessageSource parentMessageSource
                                = GetInternalParentMessageSource();
                            hierSource.ParentMessageSource = parentMessageSource;
                        }
                    }

                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(StringUtils.Surround(
                                      "Using MessageSource [", MessageSource, "]"));
                    }

                    #endregion
                }
                else
                {
                    _messageSource = new DelegatingMessageSource(
                        GetInternalParentMessageSource());

                    #region Instrumentation

                    if (log.IsWarnEnabled)
                    {
                        log.Warn(string.Format(
                                     "Found object in context named '{0}' : this name " +
                                     "is typically reserved for IMessageSource objects. " +
                                     "Falling back to default '{1}'.",
                                     MessageSourceObjectName, MessageSource));
                    }

                    #endregion
                }
            }
            else if (ParentContext != null)
            {
                _messageSource = new DelegatingMessageSource(
                    GetInternalParentMessageSource());
                ObjectFactory.RegisterSingleton(MessageSourceObjectName, _messageSource);

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(
                                  "No message source found in the current context: using parent context's message source '{0}'.",
                                  MessageSource));
                }

                #endregion
            }
            else
            {
                _messageSource = new StaticMessageSource();
                ObjectFactory.RegisterSingleton(MessageSourceObjectName, _messageSource);

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(
                                  "No IMessageSource found with name '{0}' : using default '{1}'.",
                                  MessageSourceObjectName, MessageSource));
                }

                #endregion
            }
        }

        private void RefreshApplicationEventListeners()
        {
            ICollection listeners
                = GetObjectsOfType(
                    typeof(IApplicationEventListener), true, false).Values;
            foreach (IApplicationEventListener applicationListener in listeners)
            {
                EventRegistry.Subscribe(applicationListener);
            }
        }

        /// <summary>
        /// Returns the list of the
        /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>s
        /// that will be applied to the objects created with this factory.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The elements of this list are instances of implementations of the
        /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
        /// interface.
        /// </p>
        /// </remarks>
        /// <value>
        /// The list of the
        /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>s
        /// that will be applied to the objects created with this factory.
        /// </value>
        private IList ObjectFactoryPostProcessors
        {
            get { return _objectFactoryPostProcessors; }
        }

        #region IConfigurableApplicationContext Members

        /// <summary>
        /// Return the internal object factory of this application context.
        /// </summary>
        public abstract IConfigurableListableObjectFactory ObjectFactory { get; }

        /// <summary>
        /// Add a new <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
        /// that will get applied to the internal object factory of this application context
        /// on refresh, before any of the object definitions are evaluated.
        /// </summary>
        /// <param name="objectFactoryPostProcessor">
        /// The factory processor to register.
        /// </param>
        public void AddObjectFactoryPostProcessor(
            IObjectFactoryPostProcessor objectFactoryPostProcessor)
        {
            _objectFactoryPostProcessors.Add(objectFactoryPostProcessor);
        }

        /// <summary>
        /// Load or refresh the persistent representation of the configuration,
        /// which might an XML file, properties file, or relational database schema.
        /// </summary>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// If the configuration cannot be loaded.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object factory could not be initialized.
        /// </exception>
        public void Refresh()
        {
            lock (SyncRoot)
            {
                _startupDate = DateTime.Now;

                OnPreRefresh();

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("ApplicationContext Refresh: Refreshing object factory "));
                }

                #endregion

                RefreshObjectFactory();

                IConfigurableListableObjectFactory objectFactory = ObjectFactory;

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("ApplicationContext Refresh: Registering well-known processors and objects"));
                }

                #endregion

                PrepareObjectFactory(objectFactory);

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("ApplicationContext Refresh: Custom post processing object factory"));
                }

                #endregion

                PostProcessObjectFactory(objectFactory);

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("ApplicationContext Refresh: Post processing object factory using pre-registered processors"));
                }

                #endregion

                foreach (IObjectFactoryPostProcessor factoryProcessor in ObjectFactoryPostProcessors)
                {
                    factoryProcessor.PostProcessObjectFactory(objectFactory);
                }

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(
                                  CultureInfo.InvariantCulture,
                                  "{0} objects defined in application context [{1}].",
                                  ObjectDefinitionCount == 0 ? "No" : ObjectDefinitionCount.ToString(),
                                  Name));
                }

                #endregion

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("ApplicationContext Refresh: Post processing object factory using defined processors"));
                }

                #endregion

                InvokeObjectFactoryPostProcessors();

                RegisterObjectPostProcessors(objectFactory);
                InitEventRegistry();
                InitMessageSource();

                RefreshApplicationEventListeners();

                OnRefresh();

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("ApplicationContext Refresh: Preinstantiating singletons"));
                }

                #endregion

                objectFactory.PreInstantiateSingletons();

                OnPostRefresh();

                #region Instrumentation

                if (log.IsInfoEnabled)
                {
                    log.Info(string.Format("ApplicationContext Refresh: Completed"));
                }

                #endregion
            }
        }

        /// <summary>
        /// Registers well-known <see cref="IObjectPostProcessor"/>s and 
        /// preregisters well-known dependencies using <see cref="IConfigurableListableObjectFactory.RegisterResolvableDependency"/>
        /// </summary>
        /// <param name="objectFactory">the raw object factory as returned from <see cref="RefreshObjectFactory"/></param>
        private void PrepareObjectFactory(IConfigurableListableObjectFactory objectFactory)
        {
            EnsureKnownObjectPostProcessors(objectFactory);
            objectFactory.IgnoreDependencyType(typeof(IResourceLoader));
            objectFactory.IgnoreDependencyType(typeof(IApplicationContext));

            objectFactory.RegisterResolvableDependency(typeof(IObjectFactory), objectFactory);
            objectFactory.RegisterResolvableDependency(typeof(IResourceLoader), this);
            objectFactory.RegisterResolvableDependency(typeof(IApplicationEventPublisher), this);
            objectFactory.RegisterResolvableDependency(typeof(IApplicationContext), this);
            objectFactory.RegisterResolvableDependency(typeof(IEventRegistry), this);
        }

        /// <summary>
        /// Ensures, that predefined ObjectPostProcessors are registered with this ObjectFactory
        /// </summary>
        /// <param name="objectFactory"></param>
        protected void EnsureKnownObjectPostProcessors(IConfigurableListableObjectFactory objectFactory)
        {
            // index 0 contains the ObjectPostProcessorChecker that is handled separately!
            for (int i = 1; i < _defaultObjectPostProcessors.Count; i++)
            {
                objectFactory.AddObjectPostProcessor((IObjectPostProcessor)this._defaultObjectPostProcessors[i]);
            }
        }

        /// <summary>
        /// Gets the parent context, or <see langword="null"/> if there is no
        /// parent context.
        /// </summary>
        /// <returns>
        /// The parent context, or <see langword="null"/>  if there is no
        /// parent.
        /// </returns>
        /// <seealso cref="Spring.Context.IApplicationContext.ParentContext"/>
        public virtual IApplicationContext ParentContext
        {
            get { return _parentApplicationContext; }
            set { _parentApplicationContext = value; }
        }

        #endregion

        #region ILifecycle Members

        /// <summary>
        /// Starts this component.
        /// </summary>
        /// <remarks>Should not throw an exception if the component is already running.
        /// In the case of a container, this will propagate the start signal
        /// to all components that apply.
        /// </remarks>
        public void Start()
        {
            IDictionary lifecycleObjects = LifeCycleObjects;
            foreach (DictionaryEntry dictionaryEntry in lifecycleObjects)
            {
                //TODO start dependencies of the lifecycle objects
                ILifecycle obj = dictionaryEntry.Value as ILifecycle;
                if (obj != null)
                {
                    if (!obj.IsRunning)
                    {
                        obj.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Stops this component.
        /// </summary>
        /// <remarks>
        /// Should not throw an exception if the component isn't started yet.
        /// In the case of a container, this will propagate the stop signal
        /// to all components that apply.
        /// </remarks>
        public void Stop()
        {
            IDictionary lifecycleObjects = LifeCycleObjects;
            foreach (DictionaryEntry dictionaryEntry in lifecycleObjects)
            {
                //TODO stop dependencies of the lifecycle objects
                ILifecycle obj = dictionaryEntry.Value as ILifecycle;
                if (obj != null)
                {
                    if (obj.IsRunning)
                    {
                        obj.Stop();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this component is currently running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this component is running; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// In the case of a container, this will return <code>true</code>
        /// only if <i>all</i> components that apply are currently running.
        /// </remarks>
        public bool IsRunning
        {
            get
            {
                IDictionary lifecycleObjects = LifeCycleObjects;
                foreach (DictionaryEntry dictionaryEntry in lifecycleObjects)
                {
                    ILifecycle obj = dictionaryEntry.Value as ILifecycle;
                    if (obj != null)
                    {
                        if (!obj.IsRunning)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets a dictionary of all singleton beans that implement the
        /// ILifecycle interface in this context.
        /// </summary>
        /// <value>A dictionary of ILifecycle objects with object name as key.</value>
        private IDictionary LifeCycleObjects
        {
            get
            {
                IConfigurableListableObjectFactory objectFactory = ObjectFactory;
                string[] objectNames = objectFactory.SingletonNames;
                IDictionary lifeCycleObjects = new Hashtable();
                foreach (string objectName in objectNames)
                {
                    object obj = objectFactory.GetSingleton(objectName);
                    if (obj is ILifecycle)
                    {
                        lifeCycleObjects[objectName] = obj;
                    }
                }
                return lifeCycleObjects;
            }
        }

        #endregion

        #region IApplicationContext Members

        /// <summary>
        /// Raised in response to an implementation-dependant application
        /// context event.
        /// </summary>
        public event ApplicationEventHandler ContextEvent;

        /// <summary>
        /// The date and time this context was first loaded.
        /// </summary>
        /// <returns>
        /// The <see cref="System.DateTime"/> representing when this context
        /// was first loaded.
        /// </returns>
        public DateTime StartupDate
        {
            get { return _startupDate; }
        }

        /// <summary>
        /// A name for this context.
        /// </summary>
        /// <returns>
        /// A name for this context.
        /// </returns>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }



        #endregion

        #region IListableObjectFactory Members

        /// <summary>
        /// Return the names of objects matching the given <see cref="System.Type"/>
        /// (including subclasses), judging from the object definitions.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
        /// for all object names.
        /// </param>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectNamesForType(Type)"/>
        public string[] GetObjectNamesForType(Type type)
        {
            return ObjectFactory.GetObjectNamesForType(type);
        }

        /// <summary>
        /// Return the names of objects matching the given <see cref="System.Type"/>
        /// (including subclasses), judging from the object definitions.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
        /// for all object names.
        /// </param>
        /// <param name="includePrototypes">
        /// Whether to include prototype objects too or just singletons (also applies to
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s).
        /// </param>
        /// <param name="includeFactoryObjects">
        /// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/>s too
        /// or just normal objects.
        /// </param>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectNamesForType(Type, bool, bool)"/>
        public string[] GetObjectNamesForType(
            Type type, bool includePrototypes, bool includeFactoryObjects)
        {
            return ObjectFactory.GetObjectNamesForType(type, includePrototypes, includeFactoryObjects);
        }

        /// <summary>
        /// Return the names of all objects defined in this factory.
        /// </summary>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectDefinitionNames()"/>
        public string[] GetObjectDefinitionNames()
        {
            return ObjectFactory.GetObjectDefinitionNames();
        }

        /// <summary>
        /// Return the registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> for the
        /// given object, allowing access to its property values and constructor
        /// argument values.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <returns>
        /// The registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>.
        /// </returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there is no object with the given name.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of errors.
        /// </exception>        
        public virtual IObjectDefinition GetObjectDefinition(string name)
        {
            return ObjectFactory.GetObjectDefinition(name);
        }


        /// <summary>
        /// Return the registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> for the
        /// given object, allowing access to its property values and constructor
        /// argument values.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="includeAncestors">Whether to search parent object factories.</param>
        /// <returns>
        /// The registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>.
        /// </returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there is no object with the given name.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of errors.
        /// </exception>       
        public IObjectDefinition GetObjectDefinition(string name, bool includeAncestors)
        {
            return ObjectFactory.GetObjectDefinition(name, includeAncestors);
        }

        /// <summary>
        /// Return the object instances that match the given object
        /// <see cref="System.Type"/> (including subclasses), judging from either object
        /// definitions or the value of
        /// <see cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/> in the case of
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match.
        /// </param>
        /// <returns>
        /// A <see cref="System.Collections.IDictionary"/> of the matching objects,
        /// containing the object names as keys and the corresponding object instances
        /// as values.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the objects could not be created.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectsOfType(Type)"/>
        public IDictionary GetObjectsOfType(Type type)
        {
            return GetObjectsOfType(type, true, true);
        }

        /// <summary>
        /// Return the object instances that match the given object
        /// <see cref="System.Type"/> (including subclasses), judging from either object
        /// definitions or the value of
        /// <see cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/> in the case of
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match.
        /// </param>
        /// <param name="includePrototypes">
        /// Whether to include prototype objects too or just singletons (also applies to
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s).
        /// </param>
        /// <param name="includeFactoryObjects">
        /// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/>s too
        /// or just normal objects.
        /// </param>
        /// <returns>
        /// A <see cref="System.Collections.IDictionary"/> of the matching objects,
        /// containing the object names as keys and the corresponding object instances
        /// as values.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the objects could not be created.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectsOfType(Type, bool, bool)"/>
        public IDictionary GetObjectsOfType(
            Type type, bool includePrototypes, bool includeFactoryObjects)
        {
            return ObjectFactory.GetObjectsOfType(type, includePrototypes, includeFactoryObjects);
        }

        /// <summary>
        /// Return the number of objects defined in the factory.
        /// </summary>
        /// <value>
        /// The number of objects defined in the factory.
        /// </value>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.ObjectDefinitionCount"/>
        public int ObjectDefinitionCount
        {
            get { return ObjectFactory.ObjectDefinitionCount; }
        }

        /// <summary>
        /// Check if this object factory contains an object definition with the given name.
        /// </summary>
        /// <param name="name">The name of the object to look for.</param>
        /// <returns>
        /// True if this object factory contains an object definition with the given name.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.ContainsObjectDefinition(string)"/>
        public bool ContainsObjectDefinition(string name)
        {
            return ObjectFactory.ContainsObjectDefinition(name);
        }

        #endregion

        #region IObjectFactory Members

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
        public object this[string name]
        {
            get { return ObjectFactory.GetObject(name); }
        }

        /// <summary>
        /// Does this object factory contain an object with the given name?
        /// </summary>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>
        /// <see langword="true"/> if an object with the given name is defined.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ContainsObject(string)"/>
        public bool ContainsObject(string name)
        {
            return ObjectFactory.ContainsObject(name);
        }

        /// <summary>
        /// Return the aliases for the given object name, if defined.
        /// </summary>
        /// <param name="name">The object name to check for aliases.</param>
        /// <returns>The aliases, or an empty array if none.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetAliases(string)"/>
        public string[] GetAliases(string name)
        {
            return ObjectFactory.GetAliases(name);
        }


        /// <summary>
        /// Return an unconfigured(!) instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="requiredType">
        /// The <see cref="System.Type"/> the object may match. Can be an interface or
        /// superclass of the actual class. For example, if the value is the
        /// <see cref="System.Object"/> class, this method will succeed whatever the
        /// class of the returned instance.
        /// </param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a <see lang="static"/> factory method. If there is no factory method and the
        /// supplied <paramref name="arguments"/> array is not <see lang="null"/>, then
        /// match the argument values by type and call the object's constructor.
        /// </param>
        /// <returns>The unconfigured(!) instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
        /// If the object is not of the required type.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="IObjectFactory.CreateObject"/>
        /// <remarks>
        ///  This method will only <b>instantiate</b> the requested object. It does <b>NOT</b> inject any dependencies!
        /// </remarks>
        public object CreateObject(string name, Type requiredType, object[] arguments)
        {
            return ObjectFactory.CreateObject(name, requiredType, arguments);
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="requiredType">
        /// <see cref="System.Type"/> the object may match. Can be an interface or
        /// superclass of the actual class. For example, if the value is the
        /// <see cref="System.Object"/> class, this method will succeed whatever the
        /// class of the returned instance.
        /// </param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
        /// If the object is not of the required type.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, Type)"/>
        public object GetObject(string name, Type requiredType)
        {
            return ObjectFactory.GetObject(name, requiredType);
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
        public object GetObject(string name)
        {
            return ObjectFactory.GetObject(name);
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method allows an object factory to be used as a replacement for the
        /// Singleton or Prototype design pattern.
        /// </p>
        /// <p>
        /// Note that callers should retain references to returned objects. There is no
        /// guarantee that this method will be implemented to be efficient. For example,
        /// it may be synchronized, or may need to run an RDBMS query.
        /// </p>
        /// <p>
        /// Will ask the parent factory if the object cannot be found in this factory
        /// instance.
        /// </p>
        /// </remarks>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a static factory method. If there is no factory method and the
        /// arguments are not null, then match the argument values by type and
        /// call the object's constructor.
        /// </param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public object GetObject(string name, object[] arguments)
        {
            return ObjectFactory.GetObject(name, arguments);
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="requiredType">
        /// The <see cref="System.Type"/> the object may match. Can be an interface or
        /// superclass of the actual class. For example, if the value is the
        /// <see cref="System.Object"/> class, this method will succeed whatever the
        /// class of the returned instance.
        /// </param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a <see lang="static"/> factory method. If there is no factory method and the
        /// supplied <paramref name="arguments"/> array is not <see lang="null"/>, then
        /// match the argument values by type and call the object's constructor.
        /// </param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
        /// If the object is not of the required type.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, Type, object[])"/>
        public object GetObject(string name, Type requiredType, object[] arguments)
        {
            return ObjectFactory.GetObject(name, requiredType, arguments);
        }

        /// <summary>
        /// Is this object a singleton?
        /// </summary>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>True if the named object is a singleton.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.IsSingleton(string)"/>
        public bool IsSingleton(string name)
        {
            return ObjectFactory.IsSingleton(name);
        }

        /// <summary>
        /// Determines whether the specified object name is prototype.  That is, will GetObject
        /// always return independent instances?
        /// </summary>
        /// <param name="name">The name of the object to query</param>
        /// <returns>
        /// 	<c>true</c> if the specified object name will always deliver independent instances; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>This method returning false does not clearly indicate a singleton object.
        /// It indicated non-independent instances, which may correspond to a scoped object as
        /// well.  use the IsSingleton property to explicitly check for a shared
        /// singleton instance.
        /// <para>Translates aliases back to the corresponding canonical object name.  Will ask the
        /// parent factory if the object can not be found in this factory instance.
        /// </para>
        /// </remarks>
        /// <exception cref="NoSuchObjectDefinitionException">if there is no object with the given name.</exception>
        public bool IsPrototype(string name)
        {
            return ObjectFactory.IsPrototype(name);
        }


        /// <summary>
        /// Determines whether the object with the given name matches the specified type.
        /// </summary>
        /// <remarks>More specifically, check whether a GetObject call for the given name
        /// would return an object that is assignable to the specified target type.
        /// Translates aliases back to the corresponding canonical bean name.
        /// Will ask the parent factory if the bean cannot be found in this factory instance.
        /// </remarks>
        /// <param name="name">The name of the object to query.</param>
        /// <param name="targetType">Type of the target to match against.</param>
        /// <returns>
        /// 	<c>true</c> if the object type matches; otherwise, <c>false</c>
        /// if it doesn't match or cannot be determined yet.
        /// </returns>
        /// <exception cref="NoSuchObjectDefinitionException">Ff there is no object with the given name
        /// </exception>
        public bool IsTypeMatch(string name, Type targetType)
        {
            return ObjectFactory.IsTypeMatch(name, targetType);
        }

        /// <summary>
        /// Determine the <see cref="System.Type"/>  of the object with the
        /// given name.
        /// </summary>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>
        /// The <see cref="System.Type"/> of the object, or <see langword="null"/>
        /// if not determinable.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetType(string)"/>
        public Type GetType(string name)
        {
            return ObjectFactory.GetType(name);
        }

        /// <summary>
        /// Injects dependencies into the supplied <paramref name="target"/> instance
        /// using the named object definition.
        /// </summary>
        /// <param name="target">
        /// The object instance that is to be so configured.
        /// </param>
        /// <param name="name">
        /// The name of the object definition expressing the dependencies that are to
        /// be injected into the supplied <parameref name="target"/> instance.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        public object ConfigureObject(object target, string name)
        {
            return ObjectFactory.ConfigureObject(target, name);
        }

        /// <summary>
        /// Injects dependencies into the supplied <paramref name="target"/> instance
        /// using the supplied <paramref name="definition"/>.
        /// </summary>
        /// <param name="target">
        /// The object instance that is to be so configured.
        /// </param>
        /// <param name="name">
        /// The name of the object definition expressing the dependencies that are to
        /// be injected into the supplied <parameref name="target"/> instance.
        /// </param>
        /// <param name="definition">
        /// An object definition that should be used to configure object.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        public object ConfigureObject(object target, string name, IObjectDefinition definition)
        {
            return ObjectFactory.ConfigureObject(target, name, definition);
        }

        #endregion

        #region IHierarchicalObjectFactory Members

        /// <summary>
        /// Return the parent object factory, or <see langword="null"/> if there is none.
        /// </summary>
        /// <value>
        /// The parent object factory, or <see langword="null"/> if there is none.
        /// </value>
        /// <seealso cref="Spring.Objects.Factory.IHierarchicalObjectFactory.ParentObjectFactory"/>
        public IObjectFactory ParentObjectFactory
        {
            get { return _parentApplicationContext; }
        }

        /// <summary>
        /// Determines whether the local object factory contains a bean of the given name,
        /// ignoring object defined in ancestor contexts.
        /// This is an alternative to <code>ContainsObject</code>, ignoring an object
        /// of the given name from an ancestor object factory.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>
        /// 	<c>true</c> if objects with the specified name is defined in the local factory; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsLocalObject(string name)
        {
            return ObjectFactory.ContainsLocalObject(name);
        }

        #endregion

        #region IObjectDefinitionRegistry Members

        /// <summary>
        /// Determine whether the given object name is already in use within this context, 
        /// i.e. whether there is a local object. May be override by subclasses, the default 
        /// implementation simply returns <see cref="ContainsLocalObject"/>
        /// </summary>
        public virtual bool IsObjectNameInUse(string objectName)
        {
            return ContainsLocalObject(objectName);
        }

        /// <summary>
        /// Register a new object definition with this registry.
        /// Must support
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// and <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>.
        /// </summary>
        /// <param name="name">The name of the object instance to register.</param>
        /// <param name="definition">The definition of the object instance to register.</param>
        /// <remarks>
        /// 	<p>
        /// Must support
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> and
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object definition is invalid.
        /// </exception>
        public virtual void RegisterObjectDefinition(string name, IObjectDefinition definition)
        {
            ObjectFactory.RegisterObjectDefinition(name, definition);
        }

        /// <summary>
        /// Given a object name, create an alias. We typically use this method to
        /// support names that are illegal within XML ids (used for object names).
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="theAlias">The alias that will behave the same as the object name.</param>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there is no object with the given name.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectDefinitionStoreException">
        /// If the alias is already in use.
        /// </exception>
        public virtual void RegisterAlias(string name, string theAlias)
        {
            ObjectFactory.RegisterAlias(name, theAlias);
        }

        #endregion

        #region IMessageSource Members

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.
        /// </param>
        /// <param name="arguments">
        /// The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If no message could be resolved.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, CultureInfo, object[])"/>
        public string GetMessage(
            string name, CultureInfo culture, params object[] arguments)
        {
            return MessageSource.GetMessage(name, culture, arguments);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="defaultMessage">The default message.</param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.
        /// </param>
        /// <param name="arguments">
        /// The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If no message could be resolved.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, CultureInfo, object[])"/>
        public string GetMessage(string name, string defaultMessage, CultureInfo culture, params object[] arguments)
        {
            return MessageSource.GetMessage(name, defaultMessage, culture, arguments);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <returns>
        /// The resolved message if the lookup was successful.
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If no message could be resolved.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string)"/>
        public string GetMessage(string name)
        {
            return MessageSource.GetMessage(name);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="arguments">
        /// The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful.
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If no message could be resolved.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, object[])"/>
        public string GetMessage(string name, params object[] arguments)
        {
            return MessageSource.GetMessage(name, arguments);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If no message could be resolved.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, CultureInfo)"/>
        public string GetMessage(string name, CultureInfo culture)
        {
            return MessageSource.GetMessage(name, culture);
        }

        /// <summary>
        /// Resolve the message using all of the attributes contained within
        /// the supplied <see cref="Spring.Context.IMessageSourceResolvable"/>
        /// argument.
        /// </summary>
        /// <param name="resolvable">
        /// The value object storing those attributes that are required to
        /// properly resolve a message.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If the message could not be resolved.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(IMessageSourceResolvable, CultureInfo)"/>
        public string GetMessage(IMessageSourceResolvable resolvable, CultureInfo culture)
        {
            return MessageSource.GetMessage(resolvable, culture);
        }

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the resource object to resolve.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.
        /// </param>
        /// <returns>
        /// The resolved object, or <see langword="null"/> if not found.
        /// </returns>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, CultureInfo)"/>
        object IMessageSource.GetResourceObject(string name, CultureInfo culture)
        {
            return GetResourceObject(name, culture);
        }

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the resource object to resolve.
        /// </param>
        /// <returns>
        /// The resolved object, or <see langword="null"/> if not found.
        /// </returns>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string)"/>
        object IMessageSource.GetResourceObject(string name)
        {
            return GetResourceObject(name);
        }

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the resource object to resolve.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.
        /// </param>
        /// <returns>
        /// The resolved object, or <see langword="null"/> if not found.
        /// </returns>
        /// <seealso cref="Spring.Context.IMessageSource.GetResourceObject(string, CultureInfo)"/>
        public object GetResourceObject(string name, CultureInfo culture)
        {
            return MessageSource.GetResourceObject(name, culture);
        }

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the resource object to resolve.
        /// </param>
        /// <returns>
        /// The resolved object, or <see langword="null"/> if not found.
        /// </returns>
        /// <seealso cref="Spring.Context.IMessageSource.GetResourceObject(string)"/>
        public object GetResourceObject(string name)
        {
            return MessageSource.GetResourceObject(name);
        }

        /// <summary>
        /// Applies resources to object properties.
        /// </summary>
        /// <param name="value">
        /// An object that contains the property values to be applied.
        /// </param>
        /// <param name="objectName">
        /// The base name of the object to use for key lookup.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.
        /// </param>
        /// <seealso cref="Spring.Context.IMessageSource.ApplyResources(object, string, CultureInfo)"/>
        public void ApplyResources(object value, string objectName, CultureInfo culture)
        {
            MessageSource.ApplyResources(value, objectName, culture);
        }

        #endregion

        #region IEventRegistry Members

        /// <summary>
        /// Publishes <b>all</b> events of the source object.
        /// </summary>
        /// <param name="sourceObject">
        /// The source object containing events to publish.
        /// </param>
        /// <seealso cref="Spring.Objects.Events.IEventRegistry.PublishEvents"/>
        public void PublishEvents(object sourceObject)
        {
            _eventRegistry.PublishEvents(sourceObject);
        }

        /// <summary>
        /// Subscribes to <b>all</b> events published, if the subscriber
        /// implements compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use.</param>
        /// <seealso cref="Spring.Objects.Events.IEventRegistry.Subscribe(object)"/>
        public void Subscribe(object subscriber)
        {
            _eventRegistry.Subscribe(subscriber);
        }

        /// <summary>
        /// Subscribes to published events of a all objects of a given
        /// <see cref="System.Type"/>, if the subscriber implements
        /// compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use.</param>
        /// <param name="targetSourceType">
        /// The target <see cref="System.Type"/> to subscribe to.
        /// </param>
        /// <seealso cref="Spring.Objects.Events.IEventRegistry.Subscribe(object, Type)"/>
        public void Subscribe(object subscriber, Type targetSourceType)
        {
            _eventRegistry.Subscribe(subscriber, targetSourceType);
        }


        /// <summary>
        /// Unsubscribes to <b>all</b> events published, if the subscriber
        /// implmenets compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use</param>
        public void Unsubscribe(object subscriber)
        {
            _eventRegistry.Unsubscribe(subscriber);
        }

        /// <summary>
        /// Unsubscribes to the published events of all objects of a given
        /// <see cref="System.Type"/>, if the subscriber implements
        /// compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use.</param>
        /// <param name="targetSourceType">
        /// The target <see cref="System.Type"/> to unsubscribe from
        /// </param>
        public void Unsubscribe(object subscriber, Type targetSourceType)
        {
            _eventRegistry.Unsubscribe(subscriber, targetSourceType);
        }

        #endregion

        #region IApplicationEventPublisher

        /// <summary>
        /// Publishes an application context event.
        /// </summary>
        /// <remarks>
        /// <p>
        /// 
        /// </p>
        /// </remarks>
        /// <param name="sender">
        /// The source of the event. May be <see langword="null"/>.
        /// </param>
        /// <param name="e">
        /// The event that is to be raised.
        /// </param>
        /// <seealso cref="Spring.Context.IApplicationEventPublisher.PublishEvent"/>
        public void PublishEvent(object sender, ApplicationEventArgs e)
        {
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format(
                              CultureInfo.InvariantCulture,
                              "Publishing event in context [{0}] : {1}",
                              Name, e));
            }

            #endregion

            OnContextEvent(sender, e);

            if (ParentContext != null)
            {
                ParentContext.PublishEvent(sender, e);
            }
        }

        #endregion

        #region IPostProcessor implementation

        private sealed class ObjectPostProcessorChecker : IObjectPostProcessor, IOrdered
        {
            private readonly ILog log;
            private int _objectPostProcessorTargetCount;
            private IConfigurableListableObjectFactory _objectFactory;


            public ObjectPostProcessorChecker()
            {
                log = LogManager.GetLogger(this.GetType());
            }

            public void Reset(IConfigurableListableObjectFactory objectFactory, int objectPostProcessorTargetCount)
            {
                _objectFactory = objectFactory;
                _objectPostProcessorTargetCount = objectPostProcessorTargetCount;
            }

            public object PostProcessBeforeInitialization(object obj, string name)
            {
                return obj;
            }

            public object PostProcessAfterInitialization(object obj, string objectName)
            {
                if (_objectFactory.ObjectPostProcessorCount < _objectPostProcessorTargetCount)
                {
                    #region Instrumentation

                    if (log.IsInfoEnabled)
                    {
                        log.Info(string.Format(
                                     "Object '{0}' is not eligible for being processed by all " +
                                     "IObjectPostProcessors (for example: not eligible for auto-proxying).", objectName));
                    }

                    #endregion
                }
                return obj;
            }

            public int Order
            {
                get { return Int32.MinValue; }
            }
        }

        #endregion
    }
}