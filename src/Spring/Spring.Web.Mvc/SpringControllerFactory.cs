using System;
using System.Web.Mvc;
using System.Web.Routing;
using Spring.Objects.Factory;
using Spring.Core;
using Spring.Context;
using Spring.Context.Support;
using System.Linq;
using System.Collections;

namespace Spring.Web.Mvc
{
    /// <summary>
    /// Controller Factory for ASP.NET MVC
    /// </summary>
    public class SpringControllerFactory : DefaultControllerFactory
    {
        private static IApplicationContext _context;

        public static IApplicationContext ApplicationContext
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
        }

        /// <summary>
        /// Creates the specified controller by using the specified request context.
        /// </summary>
        /// <param name="requestContext">The context of the HTTP request, which includes the HTTP context and route data.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <returns>A reference to the controller.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="requestContext"/> parameter is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="controllerName"/> parameter is null or empty.</exception>
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            if (ApplicationContext.ContainsObjectDefinition(controllerName))
                return ApplicationContext.GetObject(controllerName) as IController;

            return base.CreateController(requestContext, controllerName);
        }


        /// <summary>
        /// Gets or sets the name of the application context.
        /// </summary>
        /// <remarks>
        /// Defaults to using the root (default) Application Context.
        /// </remarks>
        /// <value>The name of the application context.</value>
        public static string ApplicationContextName { get; set; }

        /// <summary>
        /// Retrieves the controller instance for the specified request context and controller type.
        /// </summary>
        /// <param name="requestContext">The context of the HTTP request, which includes the HTTP context and route data.</param>
        /// <param name="controllerType">The type of the controller.</param>
        /// <returns>The controller instance.</returns>
        /// <exception cref="T:System.Web.HttpException">
        /// 	<paramref name="controllerType"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="controllerType"/> cannot be assigned.</exception>
        /// <exception cref="T:System.InvalidOperationException">An instance of <paramref name="controllerType"/> cannot be created.</exception>
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType != null)
            {
                var controllers = ApplicationContext.GetObjectsOfType(controllerType);
                if (controllers.Count > 0)
                {
                    return (IController)controllers.Cast<DictionaryEntry>().First<DictionaryEntry>().Value;
                }
            }

            //pass to base class for remainder of handling if can't find it in the context
            return base.GetControllerInstance(requestContext, controllerType);
        }

    }
}