using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Spring.Objects.Factory;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;
using System.Web.Routing;
using Spring.Context.Support;

namespace Spring.Web.Mvc
{
    public abstract class SpringMvcApplication : HttpApplication
    {
        protected virtual void Application_Start(object sender, EventArgs e)
        {
            RegisterAreas();
            RegisterRoutes(RouteTable.Routes);
        }

        /// <summary>
        /// Configures the <see cref="ApplicationContext"/> instance.
        /// </summary>
        /// <remarks>
        /// You must override this method in a derived class to control the manner in which the
        /// <see cref="ApplicationContext"/> is configured.
        /// </remarks>        
        protected virtual void ConfigureApplicationContext()
        {
            
        }


        public override void Init()
        {
            base.Init();

            //the Spring HTTP Module won't have built the context for us until now so we have to delay until the init
            ConfigureApplicationContext();
            RegisterSpringControllerFactory();
        }

        /// <summary>
        /// Registers the areas.
        /// </summary>
        /// <remarks>
        /// Override this method in a derived class to modify the registered areas as neeeded.
        /// </remarks>
        protected virtual void RegisterAreas()
        {
            AreaRegistration.RegisterAllAreas();
        }

        /// <summary>
        /// Registers the routes.
        /// </summary>
        /// <remarks>
        /// Override this method in a derived class to modify the registered routes as neeeded.
        /// </remarks>
        protected virtual void RegisterRoutes(RouteCollection routes)
        {
            // This IgnoreRoute call is provided to avoid the trouble of CASSINI passing all req's thru
            // ASP.NET (and thus the controller pipeline) during debugging
            // see http://stackoverflow.com/questions/487230/serving-favicon-ico-in-asp-net-mvc and elsewhere for more info
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        /// <summary>
        /// Registers the controller factory with the Mvc Framework.
        /// </summary>
        protected virtual void RegisterSpringControllerFactory()
        {
            ControllerBuilder.Current.SetControllerFactory(typeof(SpringControllerFactory));
        }

    }
}
