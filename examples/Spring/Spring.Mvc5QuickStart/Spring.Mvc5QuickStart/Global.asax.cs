using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Spring.Web.Mvc;

namespace Spring.Mvc5QuickStart
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : SpringMvcApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected override System.Web.Http.Dependencies.IDependencyResolver BuildWebApiDependencyResolver()
        {
            //get the 'default' resolver, populated from the 'main' config metadata
            var resolver = base.BuildWebApiDependencyResolver();

            //check if its castable to a SpringWebApiDependencyResolver
            var springResolver = resolver as SpringWebApiDependencyResolver;

            //if it is, add additional config sources as needed
            if (springResolver != null)
            {
                springResolver.AddChildApplicationContextConfigurationLocation("file://~/Config/child_controllers.xml");
            }

            //return the fully-configured resolver
            return resolver;
        }
    }
}