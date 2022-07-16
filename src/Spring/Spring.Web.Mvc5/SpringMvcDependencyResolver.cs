using System.Collections.Concurrent;
using System.Web.Mvc;

using Common.Logging;

using Spring.Context;
using Spring.Context.Support;

namespace Spring.Web.Mvc
{
    /// <summary>
    /// Spring-based implementation of the <see cref="IDependencyResolver"/> interface for ASP.NET MVC.
    /// </summary>
    public class SpringMvcDependencyResolver : IDependencyResolver
    {
        private static readonly string IgnoreViewNamespace = "ASP.";

        /// <summary>
        /// The <see cref="Spring.Context.IApplicationContext"/> to be used by the resolver
        /// </summary>
        private IApplicationContext _context;

        private static readonly ILog logger = LogManager.GetLogger(typeof(SpringMvcDependencyResolver));
        private readonly ConcurrentBag<Type> _nonResolvableTypes = new ConcurrentBag<Type>();
        private readonly ConcurrentDictionary<Type, string> _resolvedNames = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringMvcDependencyResolver"/> class.
        /// </summary>
        /// <param name="context">The <see cref="Spring.Context.IApplicationContext"/> to be used by the resolver</param>
        public SpringMvcDependencyResolver(IApplicationContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Gets the application context.
        /// </summary>
        /// <value>The application context.</value>
        public IApplicationContext ApplicationContext
        {
            get
            {
                if (_context == null || _context.Name != ApplicationContextName)
                {
                    if (string.IsNullOrEmpty(ApplicationContextName))
                    {
                        _context = ContextRegistry.GetContext();
                    }
                    else
                    {
                        _context = ContextRegistry.GetContext(ApplicationContextName);
                    }
                }

                return _context;
            }
            protected set { _context = value; }
        }

        /// <summary>
        /// Gets or sets the name of the application context.
        /// </summary>
        /// <remarks>
        /// Defaults to using the root (default) Application Context.
        /// </remarks>
        /// <value>The name of the application context.</value>
        public string ApplicationContextName { get; set; }


        /// <summary>
        /// Resolves singly registered services that support arbitrary object creation.
        /// </summary>
        /// <param name="serviceType">The type of the requested service or object.</param>
        /// <returns>The requested service or object.</returns>
        public object GetService(Type serviceType)
        {
            object service = null;

            if (serviceType != null)
            {
                //if its an MVC auto-generated View Class...
                if (serviceType.FullName.StartsWith(IgnoreViewNamespace))
                {
                    return null;
                }

                //if we already know the container has tried and failed to resolve the type prior...
                if (_nonResolvableTypes.Contains(serviceType))
                {
                    return null;
                }

                // fastest lookup is if we have direct name match
                if (ApplicationContext.ContainsObjectDefinition(serviceType.Name))
                {
                    service = ApplicationContext.GetObject(serviceType.Name);
                }
                else
                {
                    string resolvedName;
                    if (_resolvedNames.TryGetValue(serviceType, out resolvedName))
                    {
                        service = ApplicationContext.GetObject(resolvedName);
                    }
                    else
                    {
                        // fall back to more expensive searching with type
                        var matchingServices = ApplicationContext.GetObjectNamesForType(serviceType);
                        if (matchingServices.Count > 0)
                        {
                            _resolvedNames.TryAdd(serviceType, matchingServices[0]);
                            service = ApplicationContext.GetObject(matchingServices[0]);
                        }
                    }
                }

                if (service == null)
                {
                    _nonResolvableTypes.Add(serviceType);

                    if (logger.IsDebugEnabled)
                    {
                        logger.DebugFormat("could not find service from Spring container with type: {0}", serviceType);
                    }
                }
            }
            return service;
        }

        /// <summary>
        /// Resolves multiply registered services.
        /// </summary>
        /// <param name="serviceType">The type of the requested services.</param>
        /// <returns>The requested services.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ApplicationContext.GetObjectsOfType(serviceType).Values;
        }
    }
}
