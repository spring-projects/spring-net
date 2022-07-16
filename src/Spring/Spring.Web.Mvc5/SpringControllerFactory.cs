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

using System.Web.Mvc;
using System.Web.Routing;

using Spring.Context;
using Spring.Context.Support;

namespace Spring.Web.Mvc
{
    /// <summary>
    /// Controller Factory for ASP.NET MVC
    /// </summary>
    public class SpringControllerFactory : DefaultControllerFactory
    {
        private static IApplicationContext _context;

        /// <summary>
        /// Gets the application context.
        /// </summary>
        /// <value>The application context.</value>
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
        /// Gets or sets the name of the application context.
        /// </summary>
        /// <remarks>
        /// Defaults to using the root (default) Application Context.
        /// </remarks>
        /// <value>The name of the application context.</value>
        public static string ApplicationContextName { get; set; }

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
            IController controller;

            if (ApplicationContext.ContainsObjectDefinition(controllerName))
            {
                controller = ApplicationContext.GetObject(controllerName) as IController;
            }
            else
            {
                controller = base.CreateController(requestContext, controllerName);
            }

            AddActionInvokerTo(controller);

            return controller;
        }

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
            IController controller = null;

            if (controllerType != null)
            {
                var controllers = ApplicationContext.GetObjectsOfType(controllerType);
                if (controllers.Count > 0)
                {
                    controller = (IController)controllers.First().Value;
                }
            }

            if (controller == null)
            {
                //pass to base class for remainder of handling if can't find it in the context
                controller = base.GetControllerInstance(requestContext, controllerType);
            }
            
            AddActionInvokerTo(controller);

            return controller;
        }

        /// <summary>
        /// Adds the action invoker to the controller instance.
        /// </summary>
        /// <param name="controller">The controller.</param>
        protected virtual void AddActionInvokerTo(IController controller)
        {
            if (controller == null)
                return;

            if (typeof(Controller).IsAssignableFrom(controller.GetType()))
            {
                ((Controller)controller).ActionInvoker = new SpringActionInvoker(ApplicationContext);
            }
        }

    }
}
