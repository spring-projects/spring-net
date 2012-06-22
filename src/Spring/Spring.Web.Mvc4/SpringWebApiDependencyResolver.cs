using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using System.Web.Http.Services;
using Spring.Context;
using Spring.Context.Support;


namespace Spring.Web.Mvc
{
    /// <summary>
    /// Spring-based implementation of the <see cref="System.Web.Http.Dependencies.IDependencyResolver"/> interface for ASP.NET MVC Web API.
    /// </summary>
    public class SpringWebApiDependencyResolver : SpringMvcDependencyResolver, System.Web.Http.Dependencies.IDependencyResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpringMvcDependencyResolver"/> class.
        /// </summary>
        /// <param name="context">The <see cref="IApplicationContext"/> to be used by the resolver</param>
        public SpringWebApiDependencyResolver(IApplicationContext context)
            : base(context)
        {
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            //no unmanaged resources to dispose
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The initialized <see cref="IDependencyScope"/> instance.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual IDependencyScope BeginScope()
        {
            var abstractXmlApplicationContext = ApplicationContext as AbstractXmlApplicationContext;
            
            if (abstractXmlApplicationContext != null)
            {
                var locations = abstractXmlApplicationContext.ConfigurationLocations;
                var resources = abstractXmlApplicationContext.ConfigurationResources;

                var args = new MvcApplicationContextArgs(string.Format("child_of_{0}", ApplicationContext.Name), ApplicationContext, locations, resources, false);

                var newResolver = new SpringWebApiDependencyResolver(new MvcApplicationContext(args));
                return newResolver;
            }
            else
            {
                return this;
            }
        }
    }
}