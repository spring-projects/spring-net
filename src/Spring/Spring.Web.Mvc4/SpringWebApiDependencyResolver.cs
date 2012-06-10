using System;
using System.Collections.Generic;
using System.Web.Http.Services;
using Spring.Context;


namespace Spring.Web.Mvc
{
    /// <summary>
    /// Spring-based implementation of the <see cref="IDependencyResolver"/> interface for ASP.NET MVC Web API.
    /// </summary>
    public class SpringWebApiDependencyResolver : SpringMvcDependencyResolver, System.Web.Http.Services.IDependencyResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpringMvcDependencyResolver"/> class.
        /// </summary>
        /// <param name="context">The <see cref="IApplicationContext"/> to be used by the resolver</param>
        public SpringWebApiDependencyResolver(IApplicationContext context) : base(context)
        {
        }
    }
}