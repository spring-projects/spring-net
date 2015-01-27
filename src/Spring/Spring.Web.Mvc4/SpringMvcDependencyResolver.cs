using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
        /// The <see cref="IApplicationContext"/> to be used by the resolver
        /// </summary>
        protected IApplicationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringMvcDependencyResolver"/> class.
        /// </summary>
        /// <param name="context">The <see cref="IApplicationContext"/> to be used by the resolver</param>
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
            if (serviceType.FullName.StartsWith(IgnoreViewNamespace))
            {
                return null;
            }
            object service = null;

            if (serviceType != null)
            {
                var services = ApplicationContext.GetObjectsOfType(serviceType);
                if (services.Count > 0)
                {
                    service = services.First().Value;
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
            var services = ApplicationContext.GetObjectsOfType(serviceType);
            return services.Values;
        }
    }
}
