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
    /// <summary>
    /// Spring.NET-specific HttpApplication for ASP.NET MVC integration.
    /// </summary>
    public abstract class SpringMvcApplication : HttpApplication
    {
        /// <summary>
        /// Handles the Start event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void Application_Start(object sender, EventArgs e)
        {
            RegisterAreas();
            RegisterRoutes(RouteTable.Routes);
        }

        /// <summary>
        /// Configures the <see cref="Spring.Context.IApplicationContext"/> instance.
        /// </summary>
        /// <remarks>
        /// You must override this method in a derived class to control the manner in which the
        /// <see cref="Spring.Context.IApplicationContext"/> is configured.
        /// </remarks>        
        protected virtual void ConfigureApplicationContext()
        {

        }


        /// <summary>
        /// Executes custom initialization code after all event handler modules have been added.
        /// </summary>
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
